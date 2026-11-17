namespace Lazerboard.ScoreFetcher.Processing;

public interface IBeatmapDownloadUtils
{
    Task<string> TryDownloadBeatmapAsync(int beatmapId, string filename, CancellationToken stoppingToken);
}