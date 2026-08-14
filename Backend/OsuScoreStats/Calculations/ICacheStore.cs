using osu.Game.Beatmaps;

namespace OsuScoreStats.Calculations;

public interface ICacheStore
{
    Task<Beatmap> GetBeatmapFileAsync(int beatmapId, CancellationToken ct);
}