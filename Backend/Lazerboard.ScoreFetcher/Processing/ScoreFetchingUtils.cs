using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.OsuEntities.OsuApiEntities;

namespace Lazerboard.ScoreFetcher.Processing;

public class ScoreFetchingUtils(IDataProcessor dataProcessor, IApiFetcher apiFetcher, IScoreProcessor scoreProcessor) : IScoreFetchingUtils
{
    /// <summary>
    /// Save all beatmapset data from <see cref="APIBeatmapset"/>s (beatmapset creators and beatmapsets)
    /// </summary>
    /// <param name="beatmapsets">A populated <see cref="IReadOnlyCollection{APIBeatmapset}"/></param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    public async Task SaveAllBeatmapsetDataAsync(IReadOnlyCollection<APIBeatmapset> beatmapsets, CancellationToken stoppingToken)
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
        });
            
        await dataProcessor.ProcessRemovedUsersAsync(removedUsers, stoppingToken);
        var countries = apiUsers.Select(u => u.Country).Distinct().ToList();
        await dataProcessor.ProcessCountriesAsync(countries, stoppingToken);
        await dataProcessor.ProcessUsersAsync(apiUsers, stoppingToken);
        await dataProcessor.ProcessBeatmapsetsAsync(beatmapsets, stoppingToken);
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
        var checkResults = await scoreProcessor.CheckIfSignificantBulkAsync(scores, stoppingToken);
        var significantScores = scores.Where(s => checkResults[s.Id]).ToList();
        
        // Calculate PP for scores that don't have it.
        var scoresWithoutPp = significantScores.Where(s => s.PP == null).ToList();
        foreach (var score in scoresWithoutPp)
        {
            await scoreProcessor.CalculateScoreAsync(score, stoppingToken);
        }

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
}