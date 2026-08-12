using osu.Game.Beatmaps;

namespace OsuScoreStats.Shared.Calculations;

public interface ICacheStore
{
    void CheckCache();
    Task<Beatmap> GetBeatmapFileAsync(int beatmapId, CancellationToken ct);
}