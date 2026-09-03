using Discord;
using Discord.Webhook;
using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Repositories.Interfaces;
using Lazerboard.Data.OsuEntities.Enums;
using Lazerboard.DiscordHooks.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lazerboard.DiscordHooks.Services;

public class GetLatestScannedBeatmapService : BackgroundService
{
    private IServiceProvider _serviceProvider;
    private ILogger<GetLatestScannedBeatmapService> _logger;
    private readonly string _webhookUrl;
    private readonly TimeSpan _updateInterval;

    public GetLatestScannedBeatmapService(IServiceProvider serviceProvider, 
        ILogger<GetLatestScannedBeatmapService> logger)
    {
        _serviceProvider = serviceProvider;
        using var scope = _serviceProvider.CreateScope();
        
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        _logger = logger;
        
        var webhooksConfig = config.GetSection("DiscordHooks");
        var beatmapScoresConfig = webhooksConfig.GetSection("BeatmapScores");
        _webhookUrl = beatmapScoresConfig.GetValue<string>("HookUrl");
        _updateInterval = TimeSpan.FromMinutes(beatmapScoresConfig.GetValue<int>("UpdateIntervalMinutes"));
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var beatmaps = await GetBeatmapsDataAsync(cancellationToken);
                if (beatmaps.Count > 0)
                {
                    var embed = BuildBeatmapsetEmbed(beatmaps);
                    
                    using var client = new DiscordWebhookClient(_webhookUrl);
                    
                    var beatmapsetId = beatmaps.First().BeatmapsetId;
                    _logger.Log(LogLevel.Information, "Most recent scanned beatmapset ID: {beatmapId}", beatmapsetId);
                    
                    await client.SendMessageAsync("Most recent scanned beatmapset:", false, [embed]);
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex, "Latest scanned map service failed!");
            }

            // It'll be a few seconds late, but it's fine for what we're doing here. 
            await Task.Delay(_updateInterval, cancellationToken);
        }
    }
    
    /// <summary>
    /// Get <see cref="Beatmap"/> data from the most recent <see cref="Beatmapset"/>
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>List of <see cref="Beatmap"/>s</returns>
    private async Task<List<Beatmap>> GetBeatmapsDataAsync(CancellationToken cancellationToken)
    {
        _logger.Log(LogLevel.Information, "Getting most recent scanned beatmapset...");
        
        using var scope = _serviceProvider.CreateScope();
        
        var scoreRepository = scope.ServiceProvider.GetRequiredService<IScoreRepository>();
        var beatmapRepository = scope.ServiceProvider.GetRequiredService<IBeatmapRepository>();
        
        var beatmapsetId = await scoreRepository.GetMaxBeatmapsetIdAsync(cancellationToken);
        var beatmapsData = await beatmapRepository.GetByBeatmapsetIdAsync(beatmapsetId, cancellationToken);
        
        return beatmapsData;
    }

    /// <summary>
    /// Get the main <see cref="Mode"/> from a list of <see cref="Beatmap"/>s
    /// </summary>
    /// <param name="beatmaps">The List of <see cref="Beatmap"/>s</param>
    /// <returns>The main <see cref="Mode"/></returns>
    private Mode GetMainMode(IList<Beatmap> beatmaps) =>
        beatmaps
            .OrderBy(b => b.Mode)
            .GroupBy(b => b.Mode)
            .Select(g => new { Mode = g.Key, Count = g.Count() })
            .OrderByDescending(b => b.Count)
            .ThenBy(b => b.Mode)
            .First().Mode;

    /// <summary>
    /// Get <see cref="Mode"/>s string from a list of <see cref="Beatmap"/>s
    /// </summary>
    /// <param name="beatmaps">The List of <see cref="Beatmap"/>s</param>
    /// <returns>The <see cref="Mode"/>s string</returns>
    private string GetModesString(IList<Beatmap> beatmaps)
    {
        var modes = beatmaps
            .Select(b => b.Mode)
            .Distinct()
            .ToList();
        
        var modeStrings = modes.Select(EmbedUtils.GetModeText).ToList();
        return string.Join(", ", modeStrings);
    }

    /// <summary>
    /// Get a list of <see cref="BeatmapStatus"/>es from a list of <see cref="Beatmap"/>s
    /// </summary>
    /// <param name="beatmaps">The List of <see cref="Beatmap"/>s</param>
    /// <returns>The list of <see cref="BeatmapStatus"/>es</returns>
    private string GetStatusesString(IList<Beatmap> beatmaps)
    {
        var statuses = beatmaps
            .Select(b => b.Status)
            .Distinct()
            .ToList();
        
        var statusStrings = statuses.Select(EmbedUtils.GetStatusText).ToList();
        return string.Join(", ", statusStrings);
    }

    private Embed BuildBeatmapsetEmbed(IList<Beatmap> beatmaps)
    {
        var firstBeatmap = beatmaps.First();
        var title = $"{firstBeatmap.Beatmapset.Artist} - {firstBeatmap.Beatmapset.Title}";
        
        var mainMode = GetMainMode(beatmaps);
        var beatmapsetModes = GetModesString(beatmaps);
        var beatmapStatuses = GetStatusesString(beatmaps);
        
        var mapsetBy = $"**Mapset by**: [{firstBeatmap.Beatmapset.Creator}](https://osu.ppy.sh/users/{firstBeatmap.Beatmapset.UserId})";
        var mode = $"**Beatmapset modes**: {beatmapsetModes}";
        var beatmapStatus = $"**Beatmap statuses**: {beatmapStatuses}";

        var imageUrl = $"https://assets.ppy.sh/beatmaps/{firstBeatmap.Beatmapset.Id}/covers/cover@2x.jpg";
        var thumbnailUrl = $"https://a.ppy.sh/{firstBeatmap.Beatmapset.UserId}";
        var beatmapUrl = $"https://osu.ppy.sh/beatmapsets/{firstBeatmap.Beatmapset.Id}";
        
        var timestamp = DateTimeOffset.UtcNow;
        var color = EmbedUtils.GetModeColor(mainMode);
        
        var builder = new EmbedBuilder
        {
            Title = title,
            Description = $"{mapsetBy}\n{mode}\n{beatmapStatus}",
            Color = color,
            ImageUrl = imageUrl,
            ThumbnailUrl = thumbnailUrl,
            Url = beatmapUrl,
            Timestamp = timestamp,
            Footer = new EmbedFooterBuilder
            {
                Text = "Lazerboard: Beatmap scans"
            }
        };
        return builder.Build();
    }
}