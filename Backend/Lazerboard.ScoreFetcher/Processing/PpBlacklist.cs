using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Entities.Enums;
using Lazerboard.Data.Database.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace Lazerboard.ScoreFetcher.Processing;

public class PpBlacklist(
    IPpBlacklistRepository ppBlacklistRepository, 
    IScoreFetchingUtils utils, 
    IDataProcessor dataProcessor,
    IApiFetcher apiFetcher,
    ILogger<IPpBlacklist> logger) : IPpBlacklist
{
    /// <summary>
    /// Check if a beatmap ID belongs to the blacklist
    /// </summary>
    /// <param name="beatmapId">The <see cref="Beatmap"/> ID</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>True if ID is in blacklist, false otherwise</returns>
    public async Task<bool> CheckIfBlacklistedAsync(int beatmapId, CancellationToken cancellationToken = default)
    {
        var beatmapData = await ppBlacklistRepository.GetByIdAsync(beatmapId, cancellationToken);
        return beatmapData != null;
    }

    /// <summary>
    /// Check if multiple beatmap IDs belong to the blacklist
    /// </summary>
    /// <param name="beatmapIds">The <see cref="Beatmap"/> IDs</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A dictionary with check results</returns>
    public async Task<Dictionary<int, bool>> CheckIfBlacklistedBulkAsync(IList<int> beatmapIds,
        CancellationToken cancellationToken = default)
    {
        var beatmapsData = await ppBlacklistRepository.GetBulkAsync(beatmapIds, cancellationToken);
        var existingIds = beatmapsData.Select(b => b.BeatmapId).Distinct().ToList();
        
        var dict = new Dictionary<int, bool>();
        foreach (var id in beatmapIds)
        {
            dict[id] = existingIds.Contains(id);
        }
        return dict;
    }

    /// <summary>
    /// Add a beatmap ID to the blacklist
    /// </summary>
    /// <param name="beatmapId">The <see cref="Beatmap"/> ID</param>
    /// <param name="reason">The <see cref="BlacklistReason"/></param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    public async Task AddToBlacklistAsync(int beatmapId, BlacklistReason reason,
        CancellationToken cancellationToken = default)
    {
        var currentDateTime = DateTime.UtcNow;
        
        var checkResult = await CheckIfBlacklistedAsync(beatmapId, cancellationToken);
        if (checkResult) return;
        
        var entry = new PpBlacklistBeatmap
        {
            BeatmapId = beatmapId,
            Reason = reason,
            DateAdded = currentDateTime
        };
        
        ppBlacklistRepository.Create(entry);
        await ppBlacklistRepository.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Add beatmap IDs to the blacklist
    /// </summary>
    /// <param name="beatmapsReasons">A dictionary of <see cref="Beatmap"/> IDs and <see cref="BlacklistReason"/>s</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    public async Task AddToBlacklistBulkAsync(Dictionary<int, BlacklistReason> beatmapsReasons,
        CancellationToken cancellationToken = default)
    {
        var currentDateTime = DateTime.UtcNow;
        
        logger.Log(LogLevel.Information, "Blacklisted maps: {@maps}", beatmapsReasons.Keys.ToList());
        
        var checkResults = await CheckIfBlacklistedBulkAsync(beatmapsReasons.Keys.ToList(), cancellationToken);

        if (checkResults.All(r => r.Value)) return;
        
        var newEntries = checkResults.Where(r => !r.Value)
            .Select(r => new PpBlacklistBeatmap 
            {
                BeatmapId = r.Key,
                Reason = beatmapsReasons[r.Key],
                DateAdded = currentDateTime
            })
            .ToList();
        
        await AddBeatmapDataToDbAsync(beatmapsReasons.Keys.ToList(), cancellationToken);
        
        ppBlacklistRepository.CreateBulk(newEntries);
        await ppBlacklistRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task AddBeatmapDataToDbAsync(IList<int> beatmapIds, CancellationToken cancellationToken)
    {
        var existingBeatmaps = await dataProcessor.GetExistingBeatmapsAsync(beatmapIds, cancellationToken);
        var newBeatmapIds = beatmapIds.Where(id => !existingBeatmaps.Select(b => b.Id).Contains(id)).ToList();
        
        if (newBeatmapIds.Count > 0)
        {
            var beatmaps = await apiFetcher.GetBeatmapsAsync(newBeatmapIds, cancellationToken);
            var beatmapsets = beatmaps.Select(b => b.Beatmapset).Distinct().ToList();
            await utils.SaveAllBeatmapsetDataAsync(beatmapsets, cancellationToken);
            await dataProcessor.ProcessBeatmapsAsync(beatmaps, cancellationToken);
        }
    } 
}