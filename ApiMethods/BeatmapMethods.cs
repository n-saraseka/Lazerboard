using Microsoft.EntityFrameworkCore;
using OsuScoreStats.DbService;
using OsuScoreStats.DbService.Entities;
using OsuScoreStats.DbService.Repositories;

namespace OsuScoreStats.ApiMethods;

public class BeatmapMethods(IDbContextFactory<ScoreDataContext> dbContextFactory)
{
    /// <summary>
    /// Get a beatmap from the API
    /// </summary>
    /// <param name="beatmapId">Beatmap ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Populated APIBeatmap object (or null)</returns>
    public async Task<Beatmap?> GetBeatmapAsync(int beatmapId, CancellationToken ct = default)
    {
        var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        var beatmapRepository = new BeatmapRepository(dbContext);
        
        return await beatmapRepository.GetByIdAsync(beatmapId, ct);
    }
    
    /// <summary>
    /// Get beatmaps from the API
    /// </summary>
    /// <param name="beatmapIds">Array containing beatmap IDs</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List containing populated APIBeatmap objects</returns>
    public async Task<IEnumerable<Beatmap>> GetBeatmapsAsync(int[] beatmapIds, CancellationToken ct = default)
    {
        var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        var beatmapRepository = new BeatmapRepository(dbContext);
        
        return await beatmapRepository.GetBulkAsync(beatmapIds, ct);
    }

    /// <summary>
    /// Get beatmapsets from the API
    /// </summary>
    /// <param name="beatmapsetIds">Array containing beatmapset IDs</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>IEnumerable containing populated APIBeatmap objects</returns>
    public async Task<IEnumerable<Beatmapset>> GetBeatmapsetsAsync(int[] beatmapsetIds, CancellationToken ct = default)
    {
        var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        var beatmapsetRepository = new BeatmapsetRepository(dbContext);
        
        return await beatmapsetRepository.GetBulkAsync(beatmapsetIds, ct);
    }
}