using Lazerboard.Data.OsuEntities.OsuApiEntities;

namespace Lazerboard.Data.ApiFetchers;

public interface IDirectApiFetcher
{
    Task<List<APIBeatmapset>> GetBeatmapsetsAsync(int offset, CancellationToken ct = default);
}