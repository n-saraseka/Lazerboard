using Lazerboard.Data.OsuEntities.OsuApiEntities;

namespace Lazerboard.Data.ApiFetchers;

public interface IDirectApiFetcher
{
    Task<APIBeatmapset[]> GetBeatmapsetsAsync(int offset, CancellationToken ct = default);
    Task<Stream> DownloadBeatmapAsync(int beatmapId, CancellationToken ct = default);
}