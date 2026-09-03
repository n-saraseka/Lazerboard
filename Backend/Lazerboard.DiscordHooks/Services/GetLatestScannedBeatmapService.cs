using Discord;
using Discord.Webhook;
using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Repositories.Interfaces;
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
                var beatmap = await GetBeatmapDataAsync(cancellationToken);
                if (beatmap != null)
                {
                    var embed = BuildBeatmapEmbed(beatmap);
                    using var client = new DiscordWebhookClient(_webhookUrl);
                    _logger.Log(LogLevel.Information, "Most recent scanned beatmap ID: {beatmapId}", beatmap.Id);
                    await client.SendMessageAsync("Most recent scanned beatmap:", false, [embed]);
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

    private async Task<Beatmap?> GetBeatmapDataAsync(CancellationToken cancellationToken)
    {
        _logger.Log(LogLevel.Information, "Getting most recent scanned beatmap...");
        
        using var scope = _serviceProvider.CreateScope();
        
        var scoreRepository = scope.ServiceProvider.GetRequiredService<IScoreRepository>();
        var beatmapRepository = scope.ServiceProvider.GetRequiredService<IBeatmapRepository>();
        
        var beatmapId = await scoreRepository.GetMaxBeatmapIdAsync(cancellationToken);
        var beatmapData = await beatmapRepository.GetWithBeatmapsetDataAsync(beatmapId, cancellationToken);
        
        return beatmapData;
    }

    private Embed BuildBeatmapEmbed(Beatmap beatmap)
    {
        var title = $"{beatmap.Beatmapset.Artist} - {beatmap.Beatmapset.Title} [{beatmap.DifficultyName}]";
        
        var mapsetBy = $"**Mapset by**: [{beatmap.Beatmapset.Creator}](https://osu.ppy.sh/users/{beatmap.Beatmapset.UserId})";
        var mode = $"**Mode**: {EmbedUtils.GetModeText(beatmap.Mode)}";
        var difficulty = $"**Difficulty**: {Math.Round(beatmap.Difficulty, 2)} :star:";
        var beatmapStatus = $"**Status**: {EmbedUtils.GetStatusText(beatmap.Status)}";

        var imageUrl = $"https://assets.ppy.sh/beatmaps/{beatmap.Beatmapset.Id}/covers/cover@2x.jpg";
        var thumbnailUrl = $"https://a.ppy.sh/{beatmap.Beatmapset.UserId}";
        var beatmapUrl = $"https://osu.ppy.sh/b/{beatmap.Id}";
        
        var timestamp = DateTimeOffset.UtcNow;
        var color = EmbedUtils.GetModeColor(beatmap.Mode);
        
        var builder = new EmbedBuilder
        {
            Title = title,
            Description = $"{mapsetBy}\n{mode}\n{difficulty}\n{beatmapStatus}",
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