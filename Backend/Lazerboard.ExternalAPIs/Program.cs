using System.Net;
using Lazerboard.Data.ApiFetchers;
using Lazerboard.ExternalApis.Services.OsuApi;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddSingleton<OsuRateLimiter>();
builder.Services.AddScoped<OsuApiService>();

// Logging
builder.Host.UseSerilog((context, services, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
);

builder.Services.AddHttpClient<OsuApiService>()
    .SetHandlerLifetime(TimeSpan.FromMinutes(5))
    .AddResilienceHandler("Retry", (resilienceBuilder, context) =>
    {
        resilienceBuilder.AddRetry(new HttpRetryStrategyOptions
        {
            ShouldHandle = static args => args.Outcome switch
            {
                { Result: { IsSuccessStatusCode: false, StatusCode: not HttpStatusCode.UnprocessableEntity } } => PredicateResult.True(),
                { Exception: System.Net.Sockets.SocketException } => PredicateResult.True(),
                { Exception.InnerException: System.Net.Sockets.SocketException } => PredicateResult.True(),
                _ => PredicateResult.False()
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