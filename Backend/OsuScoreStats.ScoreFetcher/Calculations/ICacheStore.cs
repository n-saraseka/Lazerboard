using osu.Game.Beatmaps;

namespace OsuScoreStats.ScoreFetcher.Calculations;

public interface ICacheStore
{
    Task<Beatmap> GetBeatmapFileAsync(int beatmapId, CancellationToken ct);
}