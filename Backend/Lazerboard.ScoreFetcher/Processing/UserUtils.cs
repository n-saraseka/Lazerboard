using Lazerboard.Data.ApiFetchers;
using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Repositories.Interfaces;
using Lazerboard.ScoreFetcher.BackgroundServices.ScanServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Lazerboard.ScoreFetcher.Processing;

public class UserUtils(IUserRepository userRepository,
    IOsuApiFetcher osuApiFetcher,
    IScoreRepository scoreRepository,
    IUnlistedScoreRepository unlistedScoreRepository,
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
    /// <param name="stoppingToken">A <see cref="stoppingToken"/></param>
    public async Task ProcessRestrictedUsersAsync(IList<User> users, CancellationToken stoppingToken)
    {
        var userIds = users.Select(u => u.Id).Distinct().ToList();
        var existingUsers = await osuApiFetcher.GetUsersAsync(userIds, stoppingToken);
        var existingUserIds = existingUsers.Select(u => u.Id).ToList();
        
        var unrestrictedUsers = users
            .Where(u => existingUserIds.Contains(u.Id))
            .ToDictionary(u => u.Id, u => existingUsers.First(user => u.Id == user.Id));
        var unrestrictedUserIds = unrestrictedUsers.Select(s => s.Key).ToList();

        if (unrestrictedUserIds.Count > 0)
        {
            await ReinstateUserScoresAsync(unrestrictedUserIds, stoppingToken);
        }
        
        var currentDateTime = DateTime.UtcNow;
        users = users.Select(u =>
        {
            u.IsRestricted = !existingUserIds.Contains(u.Id);
            u.LastCheckedAt = currentDateTime;

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
                var newScores = scoresBatch.Select(GetScoreFromUnlistedScore).ToList();
                
                unlistedScoreRepository.DeleteBulk(scoresBatch);
                scoreRepository.CreateBulk(newScores);
                
                var beatmapIds = scoresBatch.Select(s => s.BeatmapId).Distinct().ToList();
                await ReprocessBeatmapRanksAsync(beatmapIds, stoppingToken);
                
                await unlistedScoreRepository.SaveChangesAsync(stoppingToken);
                reinstatedCount += newScores.Count;
                
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
        
        var deletedCount = 0;
        var unlistedCount = 0;
        while (batch.Count > 0)
        {
            scoreRepository.DeleteBulk(batch);
            
            var scoreIdsToRemove = GetScoreIdsWithNullData(batch);
            var scoresToUnlist = batch
                .Where(s => !scoreIdsToRemove.Contains(s.Id))
                .Select(GetUnlsitedScoreFromScore)
                .ToList();
            unlistedScoreRepository.CreateBulk(scoresToUnlist);
            
            var beatmapIds = batch.Select(s => s.BeatmapId).Distinct().ToList();
            await ReprocessBeatmapRanksAsync(beatmapIds, stoppingToken);
            
            await scoreRepository.SaveChangesAsync(stoppingToken);
            deletedCount += scoreIdsToRemove.Count;
            unlistedCount += scoresToUnlist.Count;
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
    private UnlistedScore GetUnlsitedScoreFromScore(Score score) => new()
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
    /// Update score ranks on beatmaps that had scores removed, unlisted or reinstated
    /// </summary>
    /// <param name="beatmapIds">List of <see cref="Beatmap"/> IDs</param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    private async Task ReprocessBeatmapRanksAsync(IList<int> beatmapIds, CancellationToken stoppingToken)
    {
        if (beatmapIds.Count == 0) return;
        for (var i = 0; beatmapIds.Count > 0; i++)
        {
            beatmapIds = beatmapIds.Skip(BeatmapBatchSize * i).ToList();
            var batch = beatmapIds.Take(BeatmapBatchSize).ToList();
            
            var scores = await scoreRepository.GetByBeatmapIdsAsync(batch, stoppingToken);
            var groupedScores = scores.GroupBy(s => new { s.BeatmapId, s.Mode }).ToList();
            foreach (var group in groupedScores)
            {
                var groupScores = group
                    .OrderByDescending(s => s.TotalScore)
                    .ThenBy(s => s.Date)
                    .Select((s, index) =>
                    {
                        s.Rank = index + 1;
                        return s;
                    })
                    .ToList();
                scoreRepository.UpdateBulk(groupScores);
            }
        }
    }
}