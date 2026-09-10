using Lazerboard.Data.ApiFetchers;
using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Entities.Enums;
using Lazerboard.Data.OsuEntities.OsuApiEntities;
using Lazerboard.Data.Redis.Repositories.Interfaces;
using Lazerboard.ScoreFetcher.Calculations;
using Microsoft.Extensions.Logging;
using osu.Game.Beatmaps;

namespace Lazerboard.ScoreFetcher.Processing;

public class ScoreFetchingUtils(IDataProcessor dataProcessor, 
    IOsuApiFetcher apiFetcher, 
    IScoreProcessor scoreProcessor,
    ICacheStore cacheStore,
    IOsuApiFetcher osuApiFetcher,
    IBeatmapCacheRepository beatmapCacheRepository,
    ILogger<IScoreFetchingUtils> logger) : IScoreFetchingUtils
{
    /// <summary>
    /// Save all beatmapset data from <see cref="APIBeatmapset"/>s (beatmapset creators and beatmapsets)
    /// </summary>
    /// <param name="beatmapsets">A populated <see cref="IList{APIBeatmapset}"/></param>
    /// <param name="eventType">The <see cref="ScanEventType"/></param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    public async Task SaveAllBeatmapsetDataAsync(IList<APIBeatmapset> beatmapsets, ScanEventType eventType, CancellationToken stoppingToken)
    {
        var beatmapsetUserIds = beatmapsets.Select(bs => bs.UserId).Distinct().ToList();
        
        var existingUsers = await dataProcessor.GetExistingUsersAsync(beatmapsetUserIds, stoppingToken);
        var existingUserIds = existingUsers.Select(u => u.Id).ToList();
        
        var newUserIds = beatmapsetUserIds.Where(id => !existingUserIds.Contains(id)).ToList();
        var apiUsers = await apiFetcher.GetUsersAsync(newUserIds, stoppingToken);
        var apiUserIds = apiUsers.Select(u => u.Id).Distinct();
            
        var removedUserIds = beatmapsetUserIds.Where(b => !apiUserIds.Contains(b)).ToList();
        var removedUsers = removedUserIds.Select(id => new User
        {
            Id = id,
            Username = beatmapsets.First(b => b.UserId == id).Creator
        }).ToList();
            
        await dataProcessor.ProcessRemovedUsersAsync(removedUsers, stoppingToken);
        var countries = apiUsers.Select(u => u.Country).Distinct().ToList();
        await dataProcessor.ProcessCountriesAsync(countries, stoppingToken);
        await dataProcessor.ProcessUsersAsync(apiUsers, stoppingToken);
        await dataProcessor.ProcessBeatmapsetsAsync(beatmapsets, eventType, stoppingToken);
    }

    /// <summary>
    /// Get significant scores from <see cref="APIScore"/>s
    /// </summary>
    /// <param name="scores">A populated <see cref="IList{APIScore}"/></param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    /// <returns>List of significant <see cref="APIScore"/>s</returns>
    public async Task<List<APIScore>> GetSignificantScoresAsync(IList<APIScore> scores, CancellationToken stoppingToken)
    {
        if (scores.Count == 0) return [];
        
        var deduplicatedScores = scores
            .GroupBy(s => new { s.BeatmapId, s.Mode, s.UserId })
            .Select(group => group.OrderByDescending(s => s.TotalScore).ThenBy(s => s.Date).First())
            .ToList();
        
        var checkResults = await scoreProcessor.CheckIfSignificantBulkAsync(deduplicatedScores, stoppingToken);
        var significantScores = deduplicatedScores.Where(s => checkResults[s.Id]).ToList();

        return significantScores;
    }

    /// <summary>
    /// Save <see cref="User"/> and <see cref="Country"/> data from <see cref="APIScore"/>s
    /// </summary>
    /// <param name="scores">A populated <see cref="IList{APIScore}"/></param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    public async Task SaveUserDataFromScoresAsync(IList<APIScore> scores, CancellationToken stoppingToken)
    {
        if (scores.Count == 0) return;
        List<APIUser> users;

        // If scores don't have user data, fetch it additionally (in case we use the firehose)
        if (scores[0].User.Id == 0)
        {
            var userIds = scores.Select(s => s.UserId).Distinct().ToList();
            
            var existingUsers = await dataProcessor.GetExistingUsersAsync(userIds, stoppingToken);
            var existingUserIds = existingUsers.Select(u => u.Id).ToList();
            
            var newUserIds = userIds.Where(id => !existingUserIds.Contains(id)).ToList();
            users = await apiFetcher.GetUsersAsync(newUserIds, stoppingToken);
        }
        else
        {
            users = scores.Select(s => s.User).Distinct().ToList();
        }
        
        var countries = users.Select(u => u.Country).Distinct().ToList();
            
        await dataProcessor.ProcessCountriesAsync(countries, stoppingToken);
        await dataProcessor.ProcessUsersAsync(users, stoppingToken);
    }
    
    /// <summary>
    /// Save data from scores to the database
    /// </summary>
    /// <param name="scores">List of <see cref="APIScore"/>s</param>
    /// <param name="source">The <see cref="ScoreSource"/></param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    public async Task SaveScoreDataAsync(IList<APIScore> scores, ScoreSource source, CancellationToken stoppingToken)
    {
        await SaveUserDataFromScoresAsync(scores,  stoppingToken);
        await dataProcessor.ProcessScoresAsync(scores, source, stoppingToken);
    }
    
    /// <summary>
    /// Get the <see cref="FlatWorkingBeatmap"/> for <see cref="APIBeatmap"/> ID
    /// </summary>
    /// <param name="beatmapId">The <see cref="APIBeatmap"/> ID</param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    /// <returns>The <see cref="FlatWorkingBeatmap"/></returns>
    public async Task<FlatWorkingBeatmap> GetFlatWorkingBeatmapAsync(int beatmapId, CancellationToken stoppingToken)
    {
        var filename = await cacheStore.GetBeatmapFileStringAsync(beatmapId, osuApiFetcher, beatmapCacheRepository, stoppingToken);
        return new FlatWorkingBeatmap(filename);
    }
}