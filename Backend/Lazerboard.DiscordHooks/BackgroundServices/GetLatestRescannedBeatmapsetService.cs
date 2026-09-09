using System.Timers;
using Discord;
using Discord.Webhook;
using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Repositories.Interfaces;
using Lazerboard.DiscordHooks.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lazerboard.DiscordHooks.BackgroundServices;

public class GetLatestRescannedBeatmapsetService : BackgroundService
{
    private IServiceProvider _serviceProvider;
    private ILogger<GetLatestRescannedBeatmapsetService> _logger;
    private readonly string _webhookUrl;
    private readonly TimeSpan _updateInterval;
    private System.Timers.Timer _updateTimer;

    public GetLatestRescannedBeatmapsetService(IServiceProvider serviceProvider, 
        ILogger<GetLatestRescannedBeatmapsetService> logger)
    {
        _serviceProvider = serviceProvider;
        using var scope = _serviceProvider.CreateScope();
        
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        _logger = logger;
        
        var webhooksConfig = config.GetSection("DiscordHooks");
        var beatmapScoresConfig = webhooksConfig.GetSection("Rescans");
        _webhookUrl = beatmapScoresConfig.GetValue<string>("HookUrl");
        _updateInterval = TimeSpan.FromMinutes(beatmapScoresConfig.GetValue<int>("UpdateIntervalMinutes"));
        
        _updateTimer = new System.Timers.Timer(_updateInterval.TotalMilliseconds);
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _updateTimer.Elapsed += OnUpdateTimerActivated;
        _updateTimer.AutoReset = true;
        _updateTimer.Enabled = true;
        try
        {
            await GetLatestScannedBeatmapsetAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.Log(LogLevel.Error, ex, "Latest rescanned map service failed!");
        }
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(_updateInterval, cancellationToken);
        }
    }
    
    private void OnUpdateTimerActivated(object? source, ElapsedEventArgs e)
    {
        Task.Run(async () =>
        {
            try
            {
                await GetLatestScannedBeatmapsetAsync();
            }
            catch (Exception exception)
            {
                _logger.Log(LogLevel.Error, exception, "Failed to retrieve latest scanned map!");
            }
        });
    }

    /// <summary>
    /// Get the latest scanned beatmapset and post it via the Discord webhook
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    private async Task GetLatestScannedBeatmapsetAsync(CancellationToken cancellationToken = default)
    {
        var beatmaps = await GetBeatmapsDataAsync(cancellationToken);
        if (beatmaps.Count > 0)
        {
            var embed = BuildBeatmapsetEmbed(beatmaps);
                    
            using var client = new DiscordWebhookClient(_webhookUrl);
                    
            var beatmapsetId = beatmaps.First().BeatmapsetId;
            _logger.Log(LogLevel.Information, "Latest rescanned beatmapset ID: {beatmapId}", beatmapsetId);
            
            await client.SendMessageAsync("Latest rescanned beatmapset:", false, [embed]);
        }
    }
    
    /// <summary>
    /// Get <see cref="Beatmap"/> data from the latest <see cref="Beatmapset"/>
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>List of <see cref="Beatmap"/>s</returns>
    private async Task<List<Beatmap>> GetBeatmapsDataAsync(CancellationToken cancellationToken)
    {
        _logger.Log(LogLevel.Information, "Getting latest rescanned beatmapset...");
        
        using var scope = _serviceProvider.CreateScope();
        
        var beatmapRepository = scope.ServiceProvider.GetRequiredService<IBeatmapRepository>();
        var beatmapsetRepository = scope.ServiceProvider.GetRequiredService<IBeatmapsetRepository>();

        var latestRescannedBeatmapset = await beatmapsetRepository.GetLatestRescannedMapsetAsync(cancellationToken);
        var beatmapsetId = latestRescannedBeatmapset?.Id ?? 1;
        var beatmapsData = await beatmapRepository.GetByBeatmapsetIdAsync(beatmapsetId, cancellationToken);
        
        return beatmapsData;
    }

    private Embed BuildBeatmapsetEmbed(IList<Beatmap> beatmaps)
    {
        var firstBeatmap = beatmaps.First();
        var beatmapset = firstBeatmap.Beatmapset;
        var title = $"{beatmapset.Artist} - {beatmapset.Title}";
        
        var mainMode = EmbedUtils.GetMainMode(beatmaps);
        var beatmapsetModes = EmbedUtils.GetModesString(beatmaps);
        var beatmapStatuses = EmbedUtils.GetStatusesString(beatmaps);

        var rankedAt = beatmapset.RankedDate == null ? "" : $"**Ranked at**: <t:{beatmapset.RankedDate.Value.ToUnixTimeSeconds()}:f>\n";
        var mapsetBy = $"**Mapset by**: [{beatmapset.Creator}](https://osu.ppy.sh/users/{beatmapset.UserId})";
        var mode = $"**Beatmapset modes**: {beatmapsetModes}";
        var beatmapStatus = $"**Beatmap statuses**: {beatmapStatuses}";
        var lazerboardLink = $"**[Lazerboard link](https://lazerboard.melguy.com/beatmapsets/{beatmapset.Id})**";

        var imageUrl = $"https://assets.ppy.sh/beatmaps/{beatmapset.Id}/covers/cover@2x.jpg";
        var thumbnailUrl = $"https://a.ppy.sh/{beatmapset.UserId}";
        var beatmapUrl = $"https://osu.ppy.sh/beatmapsets/{beatmapset.Id}";
        
        var timestamp = DateTimeOffset.UtcNow;
        var color = EmbedUtils.GetModeColor(mainMode);
        
        var builder = new EmbedBuilder
        {
            Title = title,
            Description = $"{rankedAt}{mapsetBy}\n{mode}\n{beatmapStatus}\n\n{lazerboardLink}",
            Color = color,
            ImageUrl = imageUrl,
            ThumbnailUrl = thumbnailUrl,
            Url = beatmapUrl,
            Timestamp = timestamp,
            Footer = new EmbedFooterBuilder
            {
                Text = "Lazerboard: Beatmap rescans"
            }
        };
        return builder.Build();
    }
}