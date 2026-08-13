using osu.Game.Beatmaps;

namespace OsuScoreStats.Calculations;

public interface ICacheStore
{
    void CheckCache();
    Task<Beatmap> GetBeatmapFileAsync(int beatmapId, CancellationToken ct);
}