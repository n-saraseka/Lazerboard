using OsuScoreStats.DbService.Entities;
using OsuScoreStats.DbService.Repositories.Interfaces;
using Beatmap = OsuScoreStats.DbService.Entities.Beatmap;

namespace OsuScoreStats.Api;

public class BeatmapMethods(IBeatmapRepository beatmapRepository, 
    IBeatmapsetRepository beatmapsetRepository, 
    IScoreRepository scoreRepository)
{
    /// <summary>
    /// Get a <see cref="Beatmap"/> from the API
    /// </summary>
    /// <param name="beatmapId"><see cref="Beatmap"/> ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Populated <see cref="Beatmap"/> object (or null)</returns>
    public async Task<Beatmap?> GetBeatmapAsync(int beatmapId, CancellationToken ct = default) => 
        await beatmapRepository.GetByIdAsync(beatmapId, ct);
    
    /// <summary>
    /// Get beatmaps from the API
    /// </summary>
    /// <param name="beatmapIds">Array containing <see cref="Beatmap"/> IDs</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List containing populated <see cref="Beatmap"/> objects</returns>
    public async Task<IEnumerable<Beatmap>> GetBeatmapsAsync(int[] beatmapIds, CancellationToken ct = default) => 
        await beatmapRepository.GetBulkAsync(beatmapIds, ct);

    /// <summary>
    /// Get beatmapsets from the API
    /// </summary>
    /// <param name="beatmapsetIds">Array containing <see cref="Beatmapset"/> IDs</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>IEnumerable containing populated <see cref="Beatmapset"/> objects</returns>
    public async Task<IEnumerable<Beatmapset>> GetBeatmapsetsAsync(int[] beatmapsetIds, CancellationToken ct = default) => 
        await beatmapsetRepository.GetBulkAsync(beatmapsetIds, ct);
    
    /// <summary>
    /// Get collected scores on the beatmap from the API
    /// </summary>
    /// <param name="id">The <see cref="Beatmap"/> ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns></returns>
    public async Task<List<Score>> GetBeatmapScoresAsync(int id, CancellationToken ct = default) =>
        await scoreRepository.GetByBeatmapIdWithUserDataAsync(id, ct);
}