using Lazerboard.Data.ApiFetchers;
using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Entities.Enums;
using Lazerboard.Data.Database.Repositories.Interfaces;
using Lazerboard.Data.OsuEntities.Enums;
using Lazerboard.Data.OsuEntities.OsuApiEntities;
using Microsoft.Extensions.Logging;
using osu.Framework.Extensions;

namespace Lazerboard.ScoreFetcher.Processing;

public class BeatmapUtils(ILogger<IBeatmapUtils> logger,
    IOsuApiFetcher osuApiFetcher,
    IDirectApiFetcher directApiFetcher,
    INekohaApiFetcher nekohaApiFetcher,
    IScoreFetchingUtils utils,
    IDataProcessor dataProcessor,
    IScoreProcessor scoreProcessor,
    IBeatmapsetRepository beatmapsetRepository,
    IBeatmapRepository beatmapRepository) : IBeatmapUtils
{
    private const int MaxDownloadAttempts = 5;
    
    /// <summary>
    /// Get <see cref="APIBeatmap"/> leaderboard scores
    /// </summary>
    /// <param name="beatmapId">The <see cref="APIBeatmap"/> ID</param>
    /// <param name="mode">The <see cref="Mode"/></param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    private async Task<List<APIScore>> GetBeatmapScoresAsync(int beatmapId, Mode mode, CancellationToken stoppingToken)
    {
        logger.Log(LogLevel.Information, "Processing beatmap {beatmapId}, mode: {mode}", beatmapId, mode);
        var beatmapScores = await osuApiFetcher.GetBeatmapScoresAsync(beatmapId, mode, 0, stoppingToken);
        
        return beatmapScores.Scores.DistinctBy(s => s.Id).ToList();
    }
    
    /// <summary>
    /// Process beatmapset maps and save the data
    /// </summary>
    /// <param name="beatmapset">The <see cref="APIBeatmapset"/></param>
    /// <param name="eventType">The <see cref="ScanEventType"/></param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    public async Task ProcessBeatmapsetAsync(APIBeatmapset beatmapset, ScanEventType eventType, CancellationToken stoppingToken)
    {
        logger.Log(LogLevel.Information, "Processing beatmapset ID: {beatmapsetID}", beatmapset.Id);

        await dataProcessor.ProcessBeatmapsAsync(beatmapset.Beatmaps, stoppingToken);
        
        foreach (var beatmap in beatmapset.Beatmaps)
        {
            foreach (var val in Enum.GetValues<Mode>())
            {
                if (beatmap.Mode != Mode.Osu && val != beatmap.Mode) continue;
                var scores = await GetBeatmapScoresAsync(beatmap.Id, val, stoppingToken);

                if (scores.Count == 0) continue;
                
                var scoresWithoutPp = scores.Where(s => s.PP == null).ToList();
                var scoresWithPp = scores.Where(s => s.PP != null).ToList();

                if (scoresWithoutPp.Count > 0)
                {
                    var flatWorkingBeatmap = await utils.GetFlatWorkingBeatmapAsync(beatmap.Id, stoppingToken);
                    foreach (var score in scoresWithoutPp)
                    {
                        await scoreProcessor.CalculateScoreAsync(score, flatWorkingBeatmap, stoppingToken);
                    }
                }
                
                var mergedScores = scoresWithPp.Concat(scoresWithoutPp).ToList();
                await utils.SaveScoreDataAsync(mergedScores, ScoreSource.LeaderboardScan, stoppingToken);
            }
        }
        
        await SaveFinishingTimestampAsync([beatmapset.Id], eventType, stoppingToken);
    }
    
    /// <summary>
    /// Process existing mapset and save the data
    /// </summary>
    /// <param name="beatmapset">The <see cref="APIBeatmapset"/></param>
    /// <param name="eventType">The <see cref="ScanEventType"/></param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    public async Task ProcessExistingMapsetAsync(Beatmapset beatmapset, ScanEventType eventType, CancellationToken stoppingToken)
    {
        logger.Log(LogLevel.Information, "Processing beatmapset ID: {beatmapsetID}", beatmapset.Id);
        
        foreach (var beatmap in beatmapset.Beatmaps)
        {
            foreach (var val in Enum.GetValues<Mode>())
            {
                if (beatmap.Mode != Mode.Osu && val != beatmap.Mode) continue;
                await ProcessLeaderboardAsync(beatmap.Id, val, stoppingToken);
            }
        }
        
        await SaveFinishingTimestampAsync([beatmapset.Id], eventType, stoppingToken);
    }

    /// <summary>
    /// Process leaderboard scores and save the data
    /// </summary>
    /// <param name="beatmapId">The <see cref="APIBeatmap"/> ID</param>
    /// <param name="mode">The <see cref="Mode"/></param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    public async Task ProcessLeaderboardAsync(int beatmapId, Mode mode, CancellationToken stoppingToken)
    {
        var scores = await GetBeatmapScoresAsync(beatmapId, mode, stoppingToken);

        if (scores.Count == 0) return;
                
        var scoresWithoutPp = scores.Where(s => s.PP == null).ToList();
        var scoresWithPp = scores.Where(s => s.PP != null).ToList();

        if (scoresWithoutPp.Count > 0)
        {
            var flatWorkingBeatmap = await utils.GetFlatWorkingBeatmapAsync(beatmapId, stoppingToken);
            foreach (var score in scoresWithoutPp)
            {
                await scoreProcessor.CalculateScoreAsync(score, flatWorkingBeatmap, stoppingToken);
            }
        }
                
        var mergedScores = scoresWithPp.Concat(scoresWithoutPp).ToList();
        await utils.SaveScoreDataAsync(mergedScores, ScoreSource.LeaderboardScan, stoppingToken);
    }
    
    /// <summary>
    /// Save the starting event timestamp for a list of <see cref="Beatmapset"/> IDs
    /// </summary>
    /// <param name="beatmapsetIds">The <see cref="Beatmapset"/> IDs</param>
    /// <param name="eventType">The <see cref="ScanEventType"/></param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    public async Task SaveStartingTimestampAsync(IList<int> beatmapsetIds, ScanEventType eventType, CancellationToken stoppingToken)
    {
        var dbBeatmapsets = await beatmapsetRepository.GetBulkAsync(beatmapsetIds, stoppingToken);
        var currentDateTime = DateTimeOffset.Now;
        foreach (var beatmapset in dbBeatmapsets)
        {
            switch (eventType)
            {
                case ScanEventType.RescanStarted:
                    beatmapset.StartedScanningAt = currentDateTime;
                    break;
                case ScanEventType.MainSeedingStarted:
                    beatmapset.MainStartedProcessingAt = currentDateTime;
                    break;
                case ScanEventType.SecondarySeedingStarted:
                    beatmapset.SecondaryStartedProcessingAt = currentDateTime;
                    break;
                case ScanEventType.ScanStarted:
                    beatmapset.StartedRescanningAt = currentDateTime;
                    break;
            }
        }
        beatmapsetRepository.UpdateBulk(dbBeatmapsets);
        await beatmapsetRepository.SaveChangesAsync(stoppingToken);
    }

    /// <summary>
    /// Save the finishing event timestamp for a list of <see cref="Beatmapset"/> IDs
    /// </summary>
    /// <param name="beatmapsetIds">The <see cref="Beatmapset"/> IDs</param>
    /// <param name="eventType">The <see cref="ScanEventType"/></param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    public async Task SaveFinishingTimestampAsync(IList<int> beatmapsetIds, ScanEventType eventType, CancellationToken stoppingToken)
    {
        var dbBeatmapsets = await beatmapsetRepository.GetBulkAsync(beatmapsetIds, stoppingToken);
        var currentDateTime = DateTimeOffset.Now;
        foreach (var beatmapset in dbBeatmapsets)
        {
            switch (eventType)
            {
                case ScanEventType.RescanStarted:
                    beatmapset.FinishedScanningAt = currentDateTime;
                    break;
                case ScanEventType.MainSeedingStarted:
                    beatmapset.MainFinishedProcessingAt = currentDateTime;
                    break;
                case ScanEventType.SecondarySeedingStarted:
                    beatmapset.SecondaryFinishedProcessingAt = currentDateTime;
                    break;
                case ScanEventType.ScanStarted:
                    beatmapset.FinishedRescanningAt = currentDateTime;
                    break;
            }
        }
        beatmapsetRepository.UpdateBulk(dbBeatmapsets);
        await beatmapsetRepository.SaveChangesAsync(stoppingToken);
    }

    /// <summary>
    /// Save the scan event timestamp for a list of <see cref="Beatmap"/> IDs
    /// </summary>
    /// <param name="beatmapIds">The <see cref="Beatmap"/> IDs</param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    public async Task SaveBeatmapScansTimestampsAsync(IList<int> beatmapIds, CancellationToken stoppingToken)
    {
        var dbBeatmaps = await beatmapRepository.GetBulkAsync(beatmapIds, stoppingToken);
        var currentDateTime = DateTime.UtcNow;
        
        dbBeatmaps = dbBeatmaps.Select(b =>
        {
            b.ScannedAt = currentDateTime;
            return b;
        }).ToList();
        
        beatmapRepository.UpdateBulk(dbBeatmaps);
        await beatmapRepository.SaveChangesAsync(stoppingToken);
    }

    /// <summary>
    /// Try downloading the .osu file from either osu.direct or osu! API
    /// </summary>
    /// <param name="beatmapId">The beatmap ID</param>
    /// <param name="filename">The .osu filename</param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    /// <returns>The filename, if download was a success</returns>
    /// <exception cref="TimeoutException">If the method couldn't download the beatmap from both providers after some amount of attempts</exception>
    public async Task<string> TryDownloadBeatmapAsync(int beatmapId, string filename, CancellationToken stoppingToken)
    {
        var attempts = 0; 
        // We assume the beatmap file is invalid as it has to be retrieved and verified first
        
        while (attempts < MaxDownloadAttempts)
        {
            var isInvalidFile = CheckIfFileIsInvalid(filename); // Download in case the file is empty or doesn't exist
            if (!isInvalidFile) return filename;
            
            try
            {
                return await DownloadFromNekohaAsync(beatmapId, filename, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, "Download from Nekoha failed for Beatmap ID {id}", beatmapId);
            }
            try
            {
                return await DownloadFromDotDirectAsync(beatmapId, filename, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, "Download from osu.direct failed for Beatmap ID {id}", beatmapId);
            }
            try
            {
                return await DownloadFromOsuApiAsync(beatmapId, filename, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, "Download from osu! API failed for Beatmap ID {id}", beatmapId);
            }
            logger.Log(LogLevel.Error, "Download from both providers failed for Beatmap ID {id}, attempt no. {attempt}", beatmapId, attempts);
            attempts++;
        }
        
        throw new TimeoutException(
            $"Failed to get beatmap {beatmapId} after {MaxDownloadAttempts} attempts");
    }

    private async Task<string> DownloadFromNekohaAsync(int beatmapId, string filename, CancellationToken stoppingToken)
    {
        await using var stream = await nekohaApiFetcher.DownloadBeatmapAsync(beatmapId, stoppingToken);
        var bytes = await stream.ReadAllRemainingBytesToArrayAsync(stoppingToken);
        await File.WriteAllBytesAsync(filename, bytes, stoppingToken);
        
        var isInvalidFile = CheckIfFileIsInvalid(filename);
        if (!isInvalidFile) return filename;
        throw new IOException($"File {filename} is invalid");
    }

    /// <summary>
    /// Download .osu file from osu.direct
    /// </summary>
    /// <param name="beatmapId">The beatmap ID</param>
    /// <param name="filename">The filename</param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    /// <returns>The filename, if download was a success</returns>
    /// <exception cref="IOException">If the downloaded file is invalid (empty or something else failed</exception>
    private async Task<string> DownloadFromDotDirectAsync(int beatmapId, string filename,
        CancellationToken stoppingToken)
    {
        await using var stream = await directApiFetcher.DownloadBeatmapAsync(beatmapId, stoppingToken);
        var bytes = await stream.ReadAllRemainingBytesToArrayAsync(stoppingToken);
        await File.WriteAllBytesAsync(filename, bytes, stoppingToken);
        
        var isInvalidFile = CheckIfFileIsInvalid(filename);
        if (!isInvalidFile) return filename;
        throw new IOException($"File {filename} is invalid");
    }

    /// <summary>
    /// Download .osu file from osu! API
    /// </summary>
    /// <param name="beatmapId">The beatmap ID</param>
    /// <param name="filename">The filename</param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    /// <returns>The filename, if download was a success</returns>
    /// <exception cref="IOException">If the downloaded file is invalid (empty or something else failed</exception>
    private async Task<string> DownloadFromOsuApiAsync(int beatmapId, string filename, CancellationToken stoppingToken)
    {
        await using var stream = await osuApiFetcher.DownloadBeatmapAsync(beatmapId, stoppingToken);
        var bytes = await stream.ReadAllRemainingBytesToArrayAsync(stoppingToken);
        await File.WriteAllBytesAsync(filename, bytes, stoppingToken);
        
        var isInvalidFile = CheckIfFileIsInvalid(filename);
        if (!isInvalidFile) return filename;
        throw new IOException($"File {filename} is invalid");
    }

    /// <summary>
    /// Check if file is invalid.
    /// </summary>
    /// <param name="filename">The filename</param>
    /// <returns>True if file length is 0, false otherwise</returns>
    private bool CheckIfFileIsInvalid(string filename)
    {
        if (!File.Exists(filename)) return true;
        var fileInfo = new FileInfo(filename);
        return fileInfo.Length == 0;
    }
}