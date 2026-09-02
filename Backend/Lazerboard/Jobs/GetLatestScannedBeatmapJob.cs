using Discord;
using Discord.Webhook;
using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Repositories.Interfaces;
using Lazerboard.Data.OsuEntities.Enums;
using Quartz;

namespace Lazerboard.Jobs;

public class GetLatestScannedBeatmapJob : IJob
{
    private IScoreRepository _scoreRepository;
    private IBeatmapRepository _beatmapRepository;
    private ILogger<GetLatestScannedBeatmapJob> _logger;
    private readonly string _webhookUrl;

    public GetLatestScannedBeatmapJob(IScoreRepository scoreRepository,
        IBeatmapRepository beatmapRepository,
        ILogger<GetLatestScannedBeatmapJob> logger,
        IConfiguration config)
    {
        _scoreRepository = scoreRepository;
        _beatmapRepository = beatmapRepository;
        _logger = logger;
        
        var webhooksConfig = config.GetSection("DiscordHooks");
        var beatmapScoresConfig = webhooksConfig.GetSection("BeatmapScores");
        _webhookUrl = beatmapScoresConfig.GetValue<string>("HookUrl");
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var beatmap = await GetBeatmapDataAsync(context.CancellationToken);
            if (beatmap != null)
            {
                var embed = BuildBeatmapEmbed(beatmap);
                using var client = new DiscordWebhookClient(_webhookUrl);
                await client.SendMessageAsync("Most recent scanned beatmap:", false, [embed]);
            }
        }
        catch (Exception ex)
        {
            _logger.Log(LogLevel.Error, ex, "Latest scanned map job failed!");
        }
    }

    private async Task<Beatmap?> GetBeatmapDataAsync(CancellationToken ct)
    {
        _logger.Log(LogLevel.Information, "Getting most recent scanned beatmap...");
        var beatmapId = await _scoreRepository.GetMaxBeatmapIdAsync(ct);
        var beatmapData = await _beatmapRepository.GetWithBeatmapsetDataAsync(beatmapId, ct);
        return beatmapData;
    }

    private Embed BuildBeatmapEmbed(Beatmap beatmap)
    {
        var title = $"{beatmap.Beatmapset.Artist} - {beatmap.Beatmapset.Title} [{beatmap.DifficultyName}]";
        
        var mapsetBy = $"**Mapset by**: [{beatmap.Beatmapset.Creator}](https://osu.ppy.sh/users/{beatmap.Beatmapset.UserId})";
        var mode = $"**Mode**: {GetModeText(beatmap)}";
        var difficulty = $"**Difficulty**: {Math.Round(beatmap.Difficulty, 2)}";
        var beatmapStatus = $"**Status**: {GetStatusText(beatmap)}";

        var imageUrl = $"https://assets.ppy.sh/beatmaps/{beatmap.Beatmapset.Id}/covers/cover@2x.jpg";
        var thumbnailUrl = $"https://a.ppy.sh/${beatmap.Beatmapset.UserId}";
        
        var timestamp = DateTimeOffset.UtcNow;
        var color = GetModeColor(beatmap);
        
        var builder = new EmbedBuilder
        {
            Title = title,
            Description = $"{mapsetBy}\n{mode}\n{difficulty}\n{beatmapStatus}",
            Color = color,
            ImageUrl = imageUrl,
            ThumbnailUrl = thumbnailUrl,
            Timestamp = timestamp,
            Footer = new EmbedFooterBuilder
            {
                Text = "Lazerboard - Beatmap scans"
            }
        };
        return builder.Build();
    }

    private string GetModeText(Beatmap beatmap)
    {
        switch (beatmap.Mode)
        {
            case Mode.Osu:
                return ":red_circle: osu!";
            case Mode.Taiko:
                return ":drum: osu!taiko";
            case Mode.Fruits:
                return ":green_apple: osu!catch";
            case Mode.Mania:
                return ":musical_keyboard: osu!mania";
            default:
                return "unknown";
        }
    }

    private Color GetModeColor(Beatmap beatmap)
    {
        switch (beatmap.Mode)
        {
            case Mode.Osu:
                return new Color(200, 120, 120);
            case Mode.Taiko:
                return new Color(200, 180, 60);
            case Mode.Fruits:
                return new Color(120, 200, 100);
            case Mode.Mania:
                return new Color(100, 100, 200);
            default:
                return new Color(200, 200, 200);
        }
    }

    private string GetStatusText(Beatmap beatmap)
    {
        switch (beatmap.Status)
        {
            case BeatmapStatus.Graveyard:
                return ":headstone: Graveyard";
            case BeatmapStatus.Wip:
                return ":construction: WIP";
            case BeatmapStatus.Pending:
                return ":timer: Pending";
            case BeatmapStatus.Ranked:
                return ":arrow_double_up: Ranked";
            case BeatmapStatus.Approved:
                return ":arrow_double_up: Approved";
            case BeatmapStatus.Qualified:
                return ":white_check_mark: Qualified";
            case BeatmapStatus.Loved:
                return ":heart: Loved";
            default:
                return "unknown";
        }
    }
}