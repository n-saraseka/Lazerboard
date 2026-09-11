using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using Lazerboard.Data.ApiFetchers;
using Lazerboard.ExternalApis.Services.DirectApi;
using Lazerboard.ExternalApis.Services.OsuApi;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddSingleton<OsuRateLimiter>();
builder.Services.AddScoped<OsuApiService>();
builder.Services.AddSingleton<DirectApiLimiter>();
builder.Services.AddScoped<DirectApiService>();

// Logging
builder.Host.UseSerilog((context, services, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
);

static bool IsTransient(Exception? ex)
{
    for (var e = ex; e is not null; e = e.InnerException)
    {
        switch (e)
        {
            case SocketException:
            case IOException:
            case AuthenticationException:
            case HttpRequestException:
                return true;
        }
    }
    return false;
}

builder.Services.AddHttpClient<OsuApiService>()
    .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
    {
        ConnectTimeout = TimeSpan.FromSeconds(10),
        PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1),
        PooledConnectionLifetime = TimeSpan.FromMinutes(2)
    })
    .AddResilienceHandler("Retry", (resilienceBuilder, context) =>
    {
        resilienceBuilder.AddRetry(new HttpRetryStrategyOptions
        {
            ShouldHandle = static args =>
            {
                if (args.Outcome.Result is { IsSuccessStatusCode: false } r &&
                    r.StatusCode != HttpStatusCode.UnprocessableEntity && r.StatusCode != HttpStatusCode.NotFound)
                {
                    return PredicateResult.True();
                }

                if (IsTransient(args.Outcome.Exception))
                {
                    return PredicateResult.True();
                }
                
                return PredicateResult.False();
            },
            
            MaxRetryAttempts = 7,
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

builder.Services.AddHttpClient<DirectApiService>()
    .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
    {
        ConnectTimeout = TimeSpan.FromSeconds(10),
        PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1),
        PooledConnectionLifetime = TimeSpan.FromMinutes(2)
    })
    .AddResilienceHandler("Retry", (resilienceBuilder, context) =>
    {
        resilienceBuilder.AddRetry(new HttpRetryStrategyOptions
        {
            ShouldHandle = static args =>
            {
                if (args.Outcome.Result is { IsSuccessStatusCode: false } r &&
                    r.StatusCode != HttpStatusCode.UnprocessableEntity && r.StatusCode != HttpStatusCode.NotFound)
                {
                    return PredicateResult.True();
                }

                if (IsTransient(args.Outcome.Exception))
                {
                    return PredicateResult.True();
                }
                
                return PredicateResult.False();
            },
            
            MaxRetryAttempts = 7,
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

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();