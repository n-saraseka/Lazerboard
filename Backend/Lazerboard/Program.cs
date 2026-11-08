using System.Net;
using System.Text.Json;
using System.Threading.RateLimiting;
using Lazerboard.Data.ApiFetchers;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Lazerboard.Data.Database;
using Lazerboard.Data.Database.Entities.Enums;
using Lazerboard.Data.Database.Repositories;
using Lazerboard.Data.Database.Repositories.Interfaces;
using Lazerboard.Data.OsuEntities.Enums;
using Lazerboard.Data.OsuEntities.OsuEntityToDtoService;
using Lazerboard.Data.Redis.Repositories;
using Lazerboard.Data.Redis.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Serilog;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

#region Database

var dbConfig = builder.Configuration.GetSection("Database");

var connectionString = new NpgsqlConnectionStringBuilder
{
    Host = dbConfig["Host"],
    Port = int.Parse(dbConfig["Port"]!),
    Database = dbConfig["Database"],
    Username = dbConfig["Username"],
    Password = dbConfig["Password"],
};

builder.Services.AddDbContext<ScoreDataContext>(
    opt =>
    opt.UseNpgsql(
            connectionString.ConnectionString,
            o => o
                .MapEnum<Mode>("mode")
                .MapEnum<Grade>("grade")
                .MapEnum<BeatmapStatus>("beatmap_status")
                .MapEnum<ScoreSource>("score_source")
                .CommandTimeout(300))
                .UseSnakeCaseNamingConvention());

builder.Services.AddScoped<IBeatmapRepository, BeatmapRepository>();
builder.Services.AddScoped<IBeatmapsetRepository, BeatmapsetRepository>();
builder.Services.AddScoped<ICountryRepository, CountryRepository>();
builder.Services.AddScoped<IScoreRepository, ScoreRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });
builder.Services.AddRazorComponents();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
#endregion

#region Web
// Cloudflare
if (builder.Environment.IsProduction())
{
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedForHeaderName = "CF-Connecting-IP";
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    });
}

// Rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetTokenBucketLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() 
                          ?? httpContext.Request.Headers["CF-Connecting-IP"].FirstOrDefault() 
                          ?? "default",
            factory: partition => new TokenBucketRateLimiterOptions
            {
                AutoReplenishment = true,
                ReplenishmentPeriod = TimeSpan.FromSeconds(1),
                TokenLimit = 15,
                TokensPerPeriod = 1,
                QueueLimit = 3,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            }
        ));
    options.OnRejected = async (context, cancellationToken) =>
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers["Retry-After"] = $"{(int)retryAfter.TotalSeconds}";
        }

        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", cancellationToken);
    };
});
#endregion

#region Auth

builder.Services.AddSingleton<IOsuApiFetcher, OsuApiFetcher>();
builder.Services.AddHttpClient<OsuApiFetcher>()
    .SetHandlerLifetime(TimeSpan.FromMinutes(5))
    .AddResilienceHandler("Retry", (resilienceBuilder, context) =>
    {
        resilienceBuilder.AddRetry(new HttpRetryStrategyOptions
        {
            ShouldHandle = static args => args.Outcome switch
            {
                { Result: { IsSuccessStatusCode: false, StatusCode: not HttpStatusCode.UnprocessableEntity } } => PredicateResult.True(),
                _ => PredicateResult.False()
            },
            
            MaxRetryAttempts = 5,
            Delay = TimeSpan.FromSeconds(5),
            
            OnRetry = args =>
            {
                var logger = context.ServiceProvider.GetRequiredService<ILogger<OsuApiFetcher>>();
                
                logger.Log(LogLevel.Warning, args.Outcome.Exception ,"HTTP request error for URL: {@requestURL} (status code: {statusCode}). Retry no. {attempt}. Next retry in {timespan}", 
                    args.Outcome.Result?.RequestMessage?.RequestUri, args.Outcome.Result?.StatusCode, args.AttemptNumber, args.RetryDelay);
                
                return default;
            }
        });
    });
builder.Services.AddSingleton<IOsuEntityToDtoService, OsuEntityToDtoService>();
var externalApisConfig = builder.Configuration.GetSection("ExternalApis");
var osuConfig = externalApisConfig.GetSection("OsuApi");

var externalHost = externalApisConfig.GetValue<string>("Host");
var externalPort = externalApisConfig.GetValue<int>("Port");
var externalAddress = osuConfig.GetValue<string>("BaseAddress");

var externalBuilder = new UriBuilder
{
    Scheme = "http",
    Host = externalHost,
    Port = externalPort,
    Path = externalAddress
};
        
var externalUrl = externalBuilder.Uri.ToString();

Console.WriteLine(externalAddress);
Console.WriteLine(externalUrl);

builder.Services.AddAuthentication("InternalCookies")
    .AddCookie("InternalCookies", options =>
    {
        options.LoginPath = string.Empty;
        options.AccessDeniedPath = string.Empty;
        options.LogoutPath = string.Empty;
        options.Cookie.Path = "/";
        options.SlidingExpiration = true;
        options.Events.OnValidatePrincipal = context =>
        {
            var name = context.Principal?.Identity?.Name;
            if (string.IsNullOrEmpty(name) || !long.TryParse(name, out _))
            {
                context.RejectPrincipal();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            }

            return Task.CompletedTask;
        };

        static Task UnauthorizedRedirect(RedirectContext<CookieAuthenticationOptions> context)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;

            return Task.CompletedTask;
        }

        options.Events.OnRedirectToLogin = UnauthorizedRedirect;
        options.Events.OnRedirectToAccessDenied = UnauthorizedRedirect;
    })
    .AddCookie("ExternalCookies")
    .AddOAuth("osu-user", options =>
    {
        options.SignInScheme = "ExternalCookies";

        options.TokenEndpoint = $"{externalUrl}/usertoken";
        options.AuthorizationEndpoint = "https://osu.ppy.sh/oauth/authorize";
        options.ClientId = osuConfig["ClientId"]!;
        options.ClientSecret = osuConfig["ClientSecret"]!;
        options.CallbackPath = osuConfig["CallbackUrl"];

        options.CorrelationCookie.SameSite = SameSiteMode.Lax;
        options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;

        options.SaveTokens = true;

        options.Events.OnRemoteFailure = ctx =>
        {
            Console.WriteLine(ctx.Failure);
            ctx.Response.Redirect("/");
            ctx.HandleResponse();
            return Task.CompletedTask;
        };

        options.Validate();
    }); 
#endregion

// Caching
var redisConfig = builder.Configuration.GetSection("Redis");
var host = redisConfig["Host"]!;
var username = redisConfig["Username"];
var password = redisConfig["Password"];
builder.Services.AddSingleton<IConnectionMultiplexer>(_ => 
    ConnectionMultiplexer.Connect(new ConfigurationOptions
    {
        EndPoints = { { host, 6379 } },
        User = username,
        Password = password
    }));
builder.Services.AddScoped<IScoreCacheRepository, ScoreCacheRepository>();

// osu! API
builder.Services.AddScoped<IOsuApiFetcher, OsuApiFetcher>();

// Logs
builder.Host.UseSerilog((context, services, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
    );

var app = builder.Build();

if (app.Environment.IsProduction())
{
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedForHeaderName = "CF-Connecting-IP",
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });
}

app.UseRateLimiter();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStatusCodePagesWithReExecute("/{0}");
app.UseExceptionHandler("/500");

app.UseDefaultFiles();
app.UseHttpsRedirection();

app.MapControllerRoute(
    name: "user",
    pattern: "users/{id}",
    defaults: new { controller = "User", action = "UserPage" });

app.MapControllerRoute(
    name: "index",
    pattern: "/",
    defaults: new { controller = "General", action = "Index" });

app.MapControllerRoute(
    name: "about",
    pattern: "/about",
    defaults: new { controller = "General", action = "About" });

app.MapControllerRoute(
    name: "beatmapset",
    pattern: "/beatmapsets/{id:int}",
    defaults: new { controller = "Beatmapset", action = "BeatmapsetPage"});

app.MapControllerRoute(
    name: "beatmap",
    pattern: "/b/{id:int}",
    defaults: new { controller = "Beatmapset", action = "BeatmapPage"});

app.MapControllerRoute(
    name: "scoreranking",
    pattern: "scoreranking",
    defaults: new { controller = "ScoreRanking", action = "ScoreRanking"});

app.MapControllerRoute(
    name: "millionsranking",
    pattern: "millionsranking",
    defaults: new { controller = "ScoreRanking", action = "MillionsRanking"});

app.MapControllers();

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
