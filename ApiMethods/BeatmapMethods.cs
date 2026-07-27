using Microsoft.EntityFrameworkCore;
using osu.Game.Beatmaps;
using OsuScoreStats.DbService;
using OsuScoreStats.DbService.Entities;
using OsuScoreStats.DbService.Repositories;
using OsuScoreStats.DbService.Repositories.Interfaces;
using Beatmap = OsuScoreStats.DbService.Entities.Beatmap;

namespace OsuScoreStats.ApiMethods;

public class BeatmapMethods(IBeatmapRepository beatmapRepository, IBeatmapsetRepository beatmapsetRepository)
{
    /// <summary>
    /// Get a beatmap from the API
    /// </summary>
    /// <param name="beatmapId">Beatmap ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Populated APIBeatmap object (or null)</returns>
    public async Task<Beatmap?> GetBeatmapAsync(int beatmapId, CancellationToken ct = default) => 
        await beatmapRepository.GetByIdAsync(beatmapId, ct);
    
    /// <summary>
    /// Get beatmaps from the API
    /// </summary>
    /// <param name="beatmapIds">Array containing beatmap IDs</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List containing populated APIBeatmap objects</returns>
    public async Task<IEnumerable<Beatmap>> GetBeatmapsAsync(int[] beatmapIds, CancellationToken ct = default) => 
        await beatmapRepository.GetBulkAsync(beatmapIds, ct);

    /// <summary>
    /// Get beatmapsets from the API
    /// </summary>
    /// <param name="beatmapsetIds">Array containing beatmapset IDs</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>IEnumerable containing populated APIBeatmap objects</returns>
    public async Task<IEnumerable<Beatmapset>> GetBeatmapsetsAsync(int[] beatmapsetIds, CancellationToken ct = default) => 
        await beatmapsetRepository.GetBulkAsync(beatmapsetIds, ct);
}