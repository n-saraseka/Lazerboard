namespace Lazerboard.Data.ApiFetchers;

public interface INekohaApiFetcher
{
    Task<Stream> DownloadBeatmapAsync(int beatmapId, CancellationToken ct = default);
}