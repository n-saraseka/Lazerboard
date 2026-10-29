using Lazerboard.Data.ApiFetchers;
using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Repositories.Interfaces;
using Lazerboard.Data.OsuEntities.Enums;
using Lazerboard.ScoreFetcher.BackgroundServices.ScanServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Lazerboard.ScoreFetcher.Processing;

public class UserUtils(IUserRepository userRepository,
    IOsuApiFetcher osuApiFetcher,
    IScoreRepository scoreRepository,
    IUnlistedScoreRepository unlistedScoreRepository,
    IBeatmapUtils beatmapUtils,
    IScoreProcessor scoreProcessor,
    ILogger<IUserUtils> logger) : IUserUtils
{
    private const int BeatmapBatchSize = 20;
    private const int ScoreBatchSize = 2500;
    private const int UserBatchSize = 50;

    /// <summary>
    /// Process a batch of users, determine whether they are restricted or not, and process their scores
    /// </summary>
    /// <param name="users">A list of <see cref="User"/>s</param>
    /// <param name="isUserScan">Whether the method was called from a <see cref="UserScanService"/> or not</param>
    /// <param name="stoppingToken">A <see cref="stoppingToken"/></param>
    public async Task ProcessExistingUsersAsync(IList<User> users, bool isUserScan, CancellationToken stoppingToken)
    {
        var userIds = users.Select(u => u.Id).Distinct().ToList();
        
        var existingUsers = await osuApiFetcher.GetUsersAsync(userIds, stoppingToken);
        var existingUserIds = existingUsers.Select(u => u.Id).ToList();
        
        var restrictedUsers = users.Where(u => !existingUserIds.Contains(u.Id)).ToList();
        var unrestrictedUsers = users
            .Where(u => existingUserIds.Contains(u.Id))
            .ToDictionary(u => u.Id, u => existingUsers.First(user => u.Id == user.Id));
        var restrictedUserIds = restrictedUsers.Select(u => u.Id).ToList();

        await RemoveUserScoresAsync(restrictedUserIds, stoppingToken);
        
        var currentDateTime = DateTime.UtcNow;
        users = users.Select(u =>
        {
            u.IsRestricted = !existingUserIds.Contains(u.Id);
            
            if (isUserScan)
            {
                u.LastScannedAt = currentDateTime;
            }
            else
            {
                u.LastCheckedAt = currentDateTime;
            }

            if (!u.IsRestricted)
            {
                u.Username = unrestrictedUsers[u.Id].Username;
                u.CountryCode = unrestrictedUsers[u.Id].CountryCode;
            }
            
            return u;
        }).ToList();
        userRepository.UpdateBulk(users);
        await userRepository.SaveChangesAsync(stoppingToken);
        logger.Log(LogLevel.Information, "Marked {userCount} users as restricted", restrictedUsers.Count);
    }

    /// <summary>
    /// Process a batch of restricted users and reinstate scores if a user is unrestricted
    /// </summary>
    /// <param name="users">A list of <see cref="User"/>s</param>
    /// <param name="isUserScan">Whether the method was called from a <see cref="UserScanService"/> or not</param>
    /// <param name="stoppingToken">A <see cref="stoppingToken"/></param>
    public async Task ProcessRestrictedUsersAsync(IList<User> users, bool isUserScan, CancellationToken stoppingToken)
    {
        var userIds = users.Select(u => u.Id).Distinct().ToList();
        var existingUsers = await osuApiFetcher.GetUsersAsync(userIds, stoppingToken);
        var existingUserIds = existingUsers.Select(u => u.Id).ToList();
        
        var unrestrictedUsers = users
            .Where(u => existingUserIds.Contains(u.Id))
            .ToDictionary(u => u.Id, u => existingUsers.First(user => u.Id == user.Id));
        var unrestrictedUserIds = unrestrictedUsers.Select(s => s.Key).ToList();
        var restrictedUserIds = userIds.Where(id => !unrestrictedUserIds.Contains(id)).ToList();
        
        // Clean up restricted user ID scores if failed to do so for some reason earlier.
        var userIdsWithoutCleanedUpScores = await scoreRepository
            .GetByUserIds(restrictedUserIds)
            .Select(s => s.UserId)
            .Distinct()
            .ToListAsync(stoppingToken);

        if (userIdsWithoutCleanedUpScores.Count > 0)
        {
            await RemoveUserScoresAsync(userIdsWithoutCleanedUpScores, stoppingToken);
        }

        if (unrestrictedUserIds.Count > 0)
        {
            await ReinstateUserScoresAsync(unrestrictedUserIds, stoppingToken);
        }
        
        var currentDateTime = DateTime.UtcNow;
        users = users.Select(u =>
        {
            u.IsRestricted = !existingUserIds.Contains(u.Id);
            
            if (isUserScan)
            {
                u.LastScannedAt = currentDateTime;
            }
            else
            {
                u.LastCheckedAt = currentDateTime;
            }

            if (!u.IsRestricted)
            {
                u.Username = unrestrictedUsers[u.Id].Username;
                u.CountryCode = unrestrictedUsers[u.Id].CountryCode;
            }
            
            return u;
        }).ToList();
        userRepository.UpdateBulk(users);
        await userRepository.SaveChangesAsync(stoppingToken);
        logger.Log(LogLevel.Information, "{userCount} users got unrestricted", unrestrictedUserIds.Count);
    }

    private async Task ReinstateUserScoresAsync(IList<int> userIds, CancellationToken stoppingToken)
    {
        var reinstatedCount = 0;
        for (var i = 0; i < userIds.Count; i += UserBatchSize)
        {
            var usersBatch = userIds.Skip(i).Take(UserBatchSize).ToList();
            var scoresQuery = unlistedScoreRepository.GetByUserIds(usersBatch);
            var scoresBatch = await scoresQuery
                .Take(ScoreBatchSize)
                .ToListAsync(stoppingToken);
            
            while (scoresBatch.Count > 0)
            {
                reinstatedCount += await ReprocessReaddedBeatmapRanksAsync(scoresBatch, stoppingToken);
                scoresBatch = await scoresQuery.Take(ScoreBatchSize).ToListAsync(stoppingToken);
            }
        }
        
        logger.Log(LogLevel.Information, "{reinstatedCount} scores got reinstated", reinstatedCount);
    }

    /// <summary>
    /// Remove or unlist user scores based on data
    /// </summary>
    /// <param name="userIds">A list of <see cref="User"/> IDs</param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    /// <returns>Number of removed scores</returns>
    private async Task RemoveUserScoresAsync(IList<int> userIds, CancellationToken stoppingToken)
    {
        var query = scoreRepository.GetByUserIds(userIds);
        var batch = await query.Take(ScoreBatchSize).ToListAsync(stoppingToken);
        if (batch.Count == 0) return;
        
        while (batch.Count > 0)
        {
            scoreRepository.DeleteBulk(batch);
            var beatmapModes = batch
                .GroupBy(s => s.BeatmapId)
                .ToDictionary(g => g.Key, g => g.Select(s => s.Mode).ToList());
            await ReprocessBeatmapRanksAsync(beatmapModes, stoppingToken);
            batch = await query.Take(ScoreBatchSize).ToListAsync(stoppingToken);
        }
    }
    
    /// <summary>
    /// Update score ranks on beatmaps that had scores removed or unlisted
    /// </summary>
    /// <param name="beatmapModes">A dictionary of <see cref="Beatmap.Id"/>s and the relevant score <see cref="Mode"/>s</param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    private async Task ReprocessBeatmapRanksAsync(Dictionary<int, List<Mode>> beatmapModes, CancellationToken stoppingToken)
    {
        if (beatmapModes.Count == 0) return;
        for (var i = 0; i < beatmapModes.Count; i += BeatmapBatchSize)
        {
            var batch = beatmapModes.Skip(i).Take(BeatmapBatchSize).ToList();
            foreach (var beatmapMode in batch)
            {
                foreach (var mode in beatmapMode.Value)
                {
                    logger.Log(LogLevel.Information, "Reprocessing beatmap ranks for beatmap {beatmapId}, mode {mode}", beatmapMode.Key, mode);
                    // We handle removing user scores inside the DataProcessor.
                    await beatmapUtils.ProcessLeaderboardAsync(beatmapMode.Key, mode, stoppingToken);
                }
            }
        }
    }
    
    /// <summary>
    /// Update score ranks on beatmaps that had scores reinstated
    /// </summary>
    /// <param name="scores">A list of <see cref="UnlistedScore"/>s</param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    /// <returns>The reinstated scores count</returns>
    private async Task<int> ReprocessReaddedBeatmapRanksAsync(List<UnlistedScore> scores, CancellationToken stoppingToken)
    {
        if (scores.Count == 0) return 0;

        var beatmapIds = scores.Select(s => s.BeatmapId).Distinct().ToList();
        var beatmapModes = scores
            .GroupBy(s => s.BeatmapId)
            .ToDictionary(g => g.Key, g => g.Select(s => s.Mode).ToList());
        
        for (var i = 0; i < beatmapModes.Count; i += BeatmapBatchSize)
        {
            var batch = beatmapModes.Skip(i).Take(BeatmapBatchSize).ToList();
            
            foreach (var beatmapMode in batch)
            {
                foreach (var mode in beatmapMode.Value)
                {
                    logger.Log(LogLevel.Information, "Reprocessing beatmap ranks for beatmap {beatmapId}, mode {mode}", beatmapMode.Key, mode);
                    // We handle reinstating user scores inside beatmap utils (relevant scores will be fetched through the API).
                    await beatmapUtils.ProcessLeaderboardAsync(beatmapMode.Key, mode, stoppingToken);
                }
            }
        }
        
        var existingScores = await scoreRepository.GetByBeatmapIdsAsync(beatmapIds, stoppingToken);
        var existingScoreIds = existingScores.Select(s => s.Id).ToList();
        
        var unlistedScoresThatWereReinstated = scores.Where(s => existingScoreIds.Contains(s.Id)).ToList();
        unlistedScoreRepository.DeleteBulk(unlistedScoresThatWereReinstated);
        await unlistedScoreRepository.SaveChangesAsync(stoppingToken);
        
        return unlistedScoresThatWereReinstated.Count;
    }
}
