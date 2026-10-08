using Lazerboard.Data.ApiFetchers;
using Lazerboard.Data.Database;
using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Work;
using Lazerboard.Data.OsuEntities.OsuApiEntities;
using Lazerboard.ScoreFetcher.BackgroundServices.ScanServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lazerboard.ScoreFetcher.Processing;

public class UserUtils(IServiceProvider serviceProvider,
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
        if (users.Count == 0) return;
        var userIds = users.Select(u => u.Id).Distinct().ToList();

        List<APIUser> existingUsers;

        using (var scope = serviceProvider.CreateScope())
        {
            var osuApiFetcher = scope.ServiceProvider.GetRequiredService<IOsuApiFetcher>();
            existingUsers = await osuApiFetcher.GetUsersAsync(userIds, stoppingToken);
        }
        
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

        using (var scope = serviceProvider.CreateScope())
        {
            await using var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            await unitOfWork.BeginTransactionAsync(stoppingToken);
            unitOfWork.Users.UpdateBulk(users);
            await unitOfWork.CommitTransactionAsync(stoppingToken);
        }
        
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
        if (users.Count == 0) return;
        var userIds = users.Select(u => u.Id).Distinct().ToList();

        List<APIUser> existingUsers;
        using (var scope = serviceProvider.CreateScope())
        {
            var osuApiFetcher = scope.ServiceProvider.GetRequiredService<IOsuApiFetcher>();
            existingUsers = await osuApiFetcher.GetUsersAsync(userIds, stoppingToken);
        }
        
        var existingUserIds = existingUsers.Select(u => u.Id).ToList();
        
        var unrestrictedUsers = users
            .Where(u => existingUserIds.Contains(u.Id))
            .ToDictionary(u => u.Id, u => existingUsers.First(user => u.Id == user.Id));
        var unrestrictedUserIds = unrestrictedUsers.Select(s => s.Key).ToList();
        var restrictedUserIds = userIds.Where(id => !unrestrictedUserIds.Contains(id)).ToList();
        
        // Clean up restricted user ID scores if failed to do so for some reason earlier.
        List<int> userIdsWithoutCleanedUpScores;
        using (var scope = serviceProvider.CreateScope())
        {
            await using var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            userIdsWithoutCleanedUpScores = await unitOfWork.Scores
                .GetByUserIds(restrictedUserIds)
                .Select(s => s.UserId)
                .Distinct()
                .ToListAsync(stoppingToken);
        }

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

        using (var scope = serviceProvider.CreateScope())
        {
            await using var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            await unitOfWork.BeginTransactionAsync(stoppingToken);
            unitOfWork.Users.UpdateBulk(users);
            await unitOfWork.CommitTransactionAsync(stoppingToken);
        }
        
        logger.Log(LogLevel.Information, "{userCount} users got unrestricted", unrestrictedUserIds.Count);
    }

    private async Task ReinstateUserScoresAsync(IList<int> userIds, CancellationToken stoppingToken)
    {
        var reinstatedCount = 0;
        for (var i = 0; i < userIds.Count; i += UserBatchSize)
        {
            var usersBatch = userIds.Skip(i).Take(UserBatchSize).ToList();

            IQueryable<UnlistedScore> scoresQuery; 
            using (var scope = serviceProvider.CreateScope())
            {
                await using var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                scoresQuery = unitOfWork.UnlistedScores.GetByUserIds(usersBatch);
            }
            var scoresBatch = await scoresQuery
                .Take(ScoreBatchSize)
                .ToListAsync(stoppingToken);
            
            while (scoresBatch.Count > 0)
            {
                reinstatedCount += await ProcessReinstatedScoresAsync(scoresBatch, stoppingToken);
                
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
        IQueryable<Score> query;
        List<Score> batch;
        using (var scope = serviceProvider.CreateScope())
        {
            await using var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            query = unitOfWork.Scores.GetByUserIds(userIds);
            batch = await query.Take(ScoreBatchSize).ToListAsync(stoppingToken);
        }
        if (batch.Count == 0) return;
        
        var deletedCount = 0;
        var unlistedCount = 0;
        while (batch.Count > 0)
        {
            var processingData = await ProcessRemovedScoresAsync(batch, stoppingToken);
            
            deletedCount += processingData.DeletedCount;
            unlistedCount += processingData.UnlistedCount;
            batch = await query.Take(ScoreBatchSize).ToListAsync(stoppingToken);
        }
        logger.Log(LogLevel.Information, "Deleted {deletedCount} restricted user scores", deletedCount);
        logger.Log(LogLevel.Information, "Unlisted {unlistedCount} restricted user scores", unlistedCount);
    }
    
    /// <summary>
    /// Get score IDs with null new column data
    /// </summary>
    /// <param name="scores">List of <see cref="Score"/>s</param>
    /// <returns>List of <see cref="Score"/> IDs</returns>
    private List<ulong> GetScoreIdsWithNullData(IList<Score> scores) =>
        scores.Where(s =>
                s.IsConvert == null || s.IsLazerScore == null || s.Statistics == null || s.IsPerfectCombo == null)
            .Select(s => s.Id)
            .ToList();
    
    /// <summary>
    /// Get a <see cref="UnlistedScore"/> from <see cref="Score"/> data
    /// </summary>
    /// <param name="score">The <see cref="Score"/></param>
    /// <returns>The corresponding <see cref="UnlistedScore"/></returns>
    private UnlistedScore GetUnlistedScoreFromScore(Score score) => new()
    {
        Id = score.Id,
        Date = score.Date,
        Mode = score.Mode,
        BeatmapId = score.BeatmapId,
        UserId = score.UserId,
        Grade = score.Grade,
        ModAcronyms = score.ModAcronyms,
        SpeedChange = score.SpeedChange,
        Accuracy = score.Accuracy,
        Combo = score.Combo,
        Misses = score.Misses,
        TotalScore = score.TotalScore,
        ClassicTotalScore = score.ClassicTotalScore,
        LegacyTotalScore = score.LegacyTotalScore,
        PP = score.PP,
        Rank = score.Rank,
        ScoreSource = score.ScoreSource,
        IsConvert = score.IsConvert,
        IsLazerScore = score.IsLazerScore,
        IsPerfectCombo = score.IsPerfectCombo,
        Statistics = score.Statistics
    };
    
    /// <summary>
    /// Get a <see cref="Score"/> from <see cref="UnlistedScore"/> data
    /// </summary>
    /// <param name="score">The <see cref="UnlistedScore"/></param>
    /// <returns>The corresponding <see cref="UnlistedScore"/></returns>
    private Score GetScoreFromUnlistedScore(UnlistedScore score) => new()
    {
        Id = score.Id,
        Date = score.Date,
        Mode = score.Mode,
        BeatmapId = score.BeatmapId,
        UserId = score.UserId,
        Grade = score.Grade,
        ModAcronyms = score.ModAcronyms,
        SpeedChange = score.SpeedChange,
        Accuracy = score.Accuracy,
        Combo = score.Combo,
        Misses = score.Misses,
        TotalScore = score.TotalScore,
        ClassicTotalScore = score.ClassicTotalScore,
        LegacyTotalScore = score.LegacyTotalScore,
        PP = score.PP,
        Rank = score.Rank,
        ScoreSource = score.ScoreSource,
        IsConvert = score.IsConvert,
        IsLazerScore = score.IsLazerScore,
        IsPerfectCombo = score.IsPerfectCombo,
        Statistics = score.Statistics
    };
    
    /// <summary>
    /// Update score ranks on beatmaps that had scores removed or unlisted
    /// </summary>
    /// <param name="scores">A list of <see cref="Score"/>s for removal</param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    private async Task<ScoresRemovalData> ProcessRemovedScoresAsync(List<Score> scores, CancellationToken stoppingToken)
    {
        if (scores.Count == 0) return new ScoresRemovalData
        {
            UnlistedCount = 0,
            DeletedCount = 0
        };

        var unlistedCount = 0;
        var deletedCount = 0;
        
        var scoreIdsToRemove = GetScoreIdsWithNullData(scores);
        var scoresToUnlist = scores
            .Where(s => !scoreIdsToRemove.Contains(s.Id))
            .Select(GetUnlistedScoreFromScore)
            .ToList();
        
        var batchIds = scoresToUnlist.Select(s => s.Id).ToList();
        using (var scope = serviceProvider.CreateScope())
        {
            await using var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var existingUnlistedScores = await unitOfWork.UnlistedScores.GetBulkAsync(batchIds, stoppingToken);
            var existingUnlistedIds = existingUnlistedScores.Select(s => s.Id);

            var newUnlistedScores = scoresToUnlist.Where(s => !existingUnlistedIds.Contains(s.Id)).ToList();

            await unitOfWork.BeginTransactionAsync(stoppingToken);
            unitOfWork.UnlistedScores.CreateBulk(newUnlistedScores);
            await unitOfWork.CommitTransactionAsync(stoppingToken);
            unlistedCount += newUnlistedScores.Count;
        }
        
        var beatmapModes = scores
            .GroupBy(s => s.BeatmapId)
            .ToDictionary(g => g.Key, g => g.Select(s => s.Mode).ToList());

        var beatmapIds = beatmapModes.Keys;
        
        for (var i = 0; i < beatmapModes.Count; i += BeatmapBatchSize)
        {
            var batch = beatmapIds.Skip(i).Take(BeatmapBatchSize).ToList();

            List<Score> beatmapScores;
            using (var scope = serviceProvider.CreateScope())
            {
                await using var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                beatmapScores = await unitOfWork.Scores.GetByBeatmapIdsAsync(batch, stoppingToken);
            }
            var groupedScores = beatmapScores.GroupBy(s => new { s.BeatmapId, s.Mode }).ToList();
            foreach (var group in groupedScores)
            {
                var beatmapId = group.Key.BeatmapId;
                var mode = group.Key.Mode;
                if (!beatmapModes[beatmapId].Contains(mode)) continue;
                logger.Log(LogLevel.Information, "Processing removed scores for beatmap {beatmapId}, mode {mode}", beatmapId, mode);
                
                var existingScoreIds = group.Select(s => s.Id).ToList();
                var scoresToDelete = scores.Where(s => existingScoreIds.Contains(s.Id)).ToList();
                
                await beatmapUtils.ProcessLeaderboardAsync(beatmapId, mode, stoppingToken);

                deletedCount += scoresToDelete.Count;
            }
        }

        return new ScoresRemovalData
        {
            DeletedCount = deletedCount,
            UnlistedCount = unlistedCount
        };
    }
    
    /// <summary>
    /// Process reinstated scores and update beatmap ranks on them
    /// </summary>
    /// <param name="scores">A list of <see cref="Score"/>s</param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    /// <returns>The reinstated scores count</returns>
    private async Task<int> ProcessReinstatedScoresAsync(List<UnlistedScore> scores, CancellationToken stoppingToken)
    {
        if (scores.Count == 0) return 0;
        
        var scoresToReinstate = scores.Select(GetScoreFromUnlistedScore).ToList();
        var checkResults = await scoreProcessor.CheckIfSignificantBulkAsync(scoresToReinstate, stoppingToken);
        var relevantScores = scoresToReinstate.Where(s => checkResults[s.Id]).ToList();

        if (relevantScores.Count == 0) return 0;

        var beatmapIds = relevantScores.Select(s => s.BeatmapId).Distinct().ToList();
        var beatmapModes = relevantScores
            .GroupBy(s => s.BeatmapId)
            .ToDictionary(g => g.Key, g => g.Select(s => s.Mode).ToList());
        
        for (var i = 0; i < beatmapIds.Count; i += BeatmapBatchSize)
        {
            var batch = beatmapIds.Skip(i).Take(BeatmapBatchSize).ToList();

            List<Score> scoresBatch;
            using (var scope = serviceProvider.CreateScope())
            {
                await using var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                scoresBatch = await unitOfWork.Scores.GetByBeatmapIdsAsync(batch, stoppingToken);
            }
            
            var groupedScores = scoresBatch.GroupBy(s => new { s.BeatmapId, s.Mode }).ToList();
            foreach (var group in groupedScores)
            {
                var beatmapId = group.Key.BeatmapId;
                var mode = group.Key.Mode;
                if (!beatmapModes[beatmapId].Contains(mode)) continue;
                using var scope = serviceProvider.CreateScope();
                
                logger.Log(LogLevel.Information, "Reprocessing beatmap ranks for beatmap {beatmapId}, mode {mode}", beatmapId, mode);
                
                var groupScores = group.ToList();
                var groupScoreIds = groupScores.Select(s => s.Id).ToList();
                
                var unlistedScoresToRemove = scores.Where(s => 
                    s.BeatmapId == beatmapId 
                    && s.Mode == mode).ToList();
                var newScores = relevantScores.Where(s => 
                    s.BeatmapId == beatmapId 
                    && s.Mode == mode
                    && !groupScoreIds.Contains(s.Id)).ToList();
                
                var allScores = groupScores
                    .Concat(newScores)
                    .OrderByDescending(s => s.TotalScore)
                    .ThenBy(s => s.Date)
                    .Select((s, index) =>
                    {
                        s.Rank = index + 1;
                        return s;
                    })
                    .ToList();
                
                await using var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                await unitOfWork.BeginTransactionAsync(stoppingToken);
                unitOfWork.Scores.UpdateBulk(groupScores);
                unitOfWork.Scores.CreateBulk(newScores);
                unitOfWork.UnlistedScores.DeleteBulk(unlistedScoresToRemove);
                await unitOfWork.CommitTransactionAsync(stoppingToken);
            }
        }
        return relevantScores.Count;
    }

    private class ScoresRemovalData
    {
        public int DeletedCount { get; set; }
        public int UnlistedCount { get; set; }
    }
}
