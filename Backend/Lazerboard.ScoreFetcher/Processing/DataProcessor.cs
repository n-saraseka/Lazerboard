using Microsoft.Extensions.Logging;
using Npgsql;
using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Entities.Enums;
using Lazerboard.Data.Database.Repositories.Interfaces;
using Lazerboard.Data.OsuEntities.OsuApiEntities;
using Lazerboard.ScoreFetcher.OsuEntityToDtoService;

namespace Lazerboard.ScoreFetcher.Processing;

public class DataProcessor(IBeatmapsetRepository beatmapsetRepository,
    IBeatmapRepository beatmapRepository,
    ICountryRepository countryRepository,
    IUserRepository userRepository,
    IScoreRepository scoreRepository,
    IUnlistedScoreRepository unlistedScoreRepository,
    IScoreProcessor scoreProcessor,
    IOsuEntityToDtoService entityToDtoService,
    ILogger<IDataProcessor> logger): IDataProcessor
{
    /// <summary>
    /// Check for existing beatmapset data, save new beatmapset DTOs to the database and update the old ones if necessary.
    /// </summary>
    /// <param name="beatmapsets">The <see cref="APIBeatmapset"/>s</param>
    /// <param name="scanEventType">The <see cref="ScanEventType"/>s</param>
    /// <param name="ct">A <see cref="CancellationToken"/></param>
    public async Task ProcessBeatmapsetsAsync(IList<APIBeatmapset> beatmapsets, ScanEventType scanEventType, CancellationToken ct)
    {
        if (beatmapsets.Count == 0) return;
        var existingBeatmapsets = await GetExistingBeatmapsetsAsync(beatmapsets.Select(bs => bs.Id).ToList(), ct);
        var existingBeatmapsetIds = existingBeatmapsets.Select(s => s.Id).ToList();

        var currentDateTime = DateTimeOffset.Now;
        var newBeatmapsets = beatmapsets.Where(bs => !existingBeatmapsetIds.Contains(bs.Id));
        var beatmapsetDtos = newBeatmapsets
            .Select(bs =>
            {
                var dto = entityToDtoService.BeatmapsetEntityToDto(bs);
                
                switch (scanEventType)
                {
                    case ScanEventType.RescanStarted:
                        dto.StartedScanningAt = currentDateTime;
                        break;
                    case ScanEventType.MainSeedingStarted:
                        dto.MainStartedProcessingAt = currentDateTime;
                        break;
                    case ScanEventType.SecondarySeedingStarted:
                        dto.SecondaryStartedProcessingAt = currentDateTime;
                        break;
                }

                return dto;
            })
            .DistinctBy(bs => bs.Id);

        existingBeatmapsets = existingBeatmapsets.Select(bs =>
        {
            switch (scanEventType)
            {
                case ScanEventType.RescanStarted:
                    bs.StartedScanningAt = currentDateTime;
                    break;
                case ScanEventType.MainSeedingStarted:
                    bs.MainStartedProcessingAt = currentDateTime;
                    break;
                case ScanEventType.SecondarySeedingStarted:
                    bs.SecondaryStartedProcessingAt = currentDateTime;
                    break;
            }

            var matchingApiBeatmapset = beatmapsets.First(b => b.Id == bs.Id);
            bs.RankedDate = matchingApiBeatmapset.RankedDate;
            return bs;
        }).ToList();
        
        beatmapsetRepository.CreateBulk(beatmapsetDtos);
        beatmapsetRepository.UpdateBulk(existingBeatmapsets);
        try
        {
            await beatmapsetRepository.SaveChangesAsync(ct);
        }
        catch (NpgsqlException exception)
        {
            logger.Log(LogLevel.Error, exception, "Method: ProcessBeatmapsetsAsync | Beatmapsets: {beatmapsets}", beatmapsetDtos);
        }
    }

    /// <summary>
    /// Check for existing beatmap data and save new beatmap DTOs to the database.
    /// </summary>
    /// <param name="beatmaps">The <see cref="APIBeatmap"/>s</param>
    /// <param name="ct">A <see cref="CancellationToken"/></param>
    public async Task ProcessBeatmapsAsync(IList<APIBeatmap> beatmaps, CancellationToken ct)
    {
        if (beatmaps.Count == 0) return;
        var existingBeatmaps = await GetExistingBeatmapsAsync(beatmaps.Select(b => b.Id).ToList(), ct);
        var existingIds = existingBeatmaps.Select(b => b.Id).ToList();
        
        var beatmapDtos = beatmaps
            .Select(entityToDtoService.BeatmapEntityToDto)
            .DistinctBy(b => b.Id)
            .ToList();
        var newBeatmaps = beatmapDtos.Where(b => !existingIds.Contains(b.Id));
        var oldMaps = beatmapDtos.Where(b => existingIds.Contains(b.Id));
        
        var updatedBeatmapData =
            existingBeatmaps.ToDictionary(b => b.Id, b => oldMaps.First(map => map.Id == b.Id));

        foreach (var beatmap in existingBeatmaps)
        {
            beatmap.DifficultyName = updatedBeatmapData[beatmap.Id].DifficultyName;
            beatmap.Difficulty = updatedBeatmapData[beatmap.Id].Difficulty;
            beatmap.Status = updatedBeatmapData[beatmap.Id].Status;
        }
        
        beatmapRepository.CreateBulk(newBeatmaps);
        beatmapRepository.UpdateBulk(existingBeatmaps);
        try
        {
            await beatmapRepository.SaveChangesAsync(ct);
        }
        catch (NpgsqlException exception)
        {
            logger.Log(LogLevel.Error, exception, "Method: ProcessBeatmapsAsync | Beatmaps: {@beatmaps}", beatmapDtos);
        }
    }
    
    public Task<List<Beatmap>> GetExistingBeatmapsAsync(IList<int> ids, CancellationToken ct) =>
        beatmapRepository.GetBulkAsync(ids, ct);
    
    public Task<List<Beatmapset>> GetExistingBeatmapsetsAsync(IList<int> ids, CancellationToken ct) =>
        beatmapsetRepository.GetBulkAsync(ids, ct);

    public Task<List<User>> GetExistingUsersAsync(IList<int> ids, CancellationToken ct) =>
        userRepository.GetBulkAsync(ids, ct);

    /// <summary>
    /// Check for existing country data and save new country DTOs to the database.
    /// </summary>
    /// <param name="countries">The <see cref="APICountry"/> objects</param>
    /// <param name="ct">A <see cref="CancellationToken"/></param>
    public async Task ProcessCountriesAsync(IList<APICountry> countries, CancellationToken ct)
    {
        if (countries.Count == 0) return;
        var existingCountries = await countryRepository.GetBulkAsync(countries.Select(c => c.Code), ct);
        var newCountries = countries.Where(co => !existingCountries.Select(c => c.Id).Contains(co.Code));
        var countryDtos = newCountries.Select(entityToDtoService.CountryEntityToDto).DistinctBy(c => c.Id).ToList();

        if (countryDtos.Count == 0) return;
        
        countryRepository.CreateBulk(countryDtos);
        try
        {
            await countryRepository.SaveChangesAsync(ct);
        }
        catch (NpgsqlException exception)
        {
            logger.Log(LogLevel.Error, exception, "Method: ProcessCountriesAsync | Countries: {@countries}", countryDtos);
        }
    }

    /// <summary>
    /// Check for existing user data and save new user DTOs to the database.
    /// </summary>
    /// <param name="users">The <see cref="APIUser"/>s</param>
    /// <param name="ct">A <see cref="CancellationToken"/></param>
    public async Task ProcessUsersAsync(IList<APIUser> users, CancellationToken ct)
    {
        if (users.Count == 0) return;
        var existingUsers = await GetExistingUsersAsync(users.Select(u => u.Id).ToList(), ct);
        var existingIds = existingUsers.Select(u => u.Id).ToList();
        
        var userDtos = users.Select(entityToDtoService.UserEntityToDto).ToList();
        
        var newUsers = userDtos
            .Where(u => !existingIds.Contains(u.Id))
            .DistinctBy(u => u.Id);
        var oldUsers = userDtos
            .Where(u => existingIds.Contains(u.Id))
            .DistinctBy(u => u.Id)
            .ToList();
        var updatedUserData =
            existingUsers.ToDictionary(u => u.Id, u => oldUsers.First(user => user.Id == u.Id));

        foreach (var user in existingUsers)
        {
            user.Username = updatedUserData[user.Id].Username;
            user.CountryCode = updatedUserData[user.Id].CountryCode;
        }
        
        userRepository.CreateBulk(newUsers);
        userRepository.UpdateBulk(existingUsers);
        try
        {
            await userRepository.SaveChangesAsync(ct);
        }
        catch (NpgsqlException exception)
        {
            logger.Log(LogLevel.Error, exception, "Method: ProcessUsersAsync; Users: {users}", newUsers);
        }
    }
    
    /// <summary>
    /// Check for existing user data for users without CountryCode's and save new user DTOs to the database.
    /// </summary>
    /// <param name="users">The <see cref="User"/>s</param>
    /// <param name="ct">A <see cref="CancellationToken"/></param>
    public async Task ProcessRemovedUsersAsync(IList<User> users, CancellationToken ct)
    {
        if (users.Count == 0) return;
        var existingUsers = await userRepository.GetBulkAsync(users.Select(u => u.Id), ct);
        var newUsers = users
            .Where(u => !existingUsers.Select(s => s.Id).Contains(u.Id))
            .DistinctBy(u => u.Id);
        
        userRepository.CreateBulk(newUsers);
        try
        {
            await userRepository.SaveChangesAsync(ct);
        }
        catch (NpgsqlException exception)
        {
            logger.Log(LogLevel.Error, exception, "Method: ProcessRemovedUsersAsync; Users: {@users}", newUsers);
        }
    }
    
    /// <summary>
    /// Check for existing score data and save new score DTOs, assigning a rank to each one.
    /// </summary>
    /// <param name="scores">The <see cref="APIScore"/>s</param>
    /// <param name="source">The <see cref="ScoreSource"/></param>
    /// of top 100 for said mode should get removed or not</param>
    /// <param name="ct">A <see cref="CancellationToken"/></param>
    public async Task<int> ProcessScoresAsync(IList<APIScore> scores, 
        ScoreSource source,
        CancellationToken ct)
    {
        if (scores.Count == 0) return 0;
        scores = scores.DistinctBy(s => s.Id).ToList(); // Deduplicate by ID just in case.
        
        // We double-check these just in case scores were incorrectly marked as significant before.
        var checkResults = await scoreProcessor.CheckIfSignificantBulkAsync(scores, ct);
        var significantScores = scores.Where(s => checkResults[s.Id]).ToList();
        
        logger.Log(LogLevel.Information, "Processing {count} significant scores...", significantScores.Count);
        var beatmapIds = significantScores.Select(s => s.BeatmapId).Distinct().ToList();
        var groupedScores = significantScores.GroupBy(s => new { s.BeatmapId, s.Mode });
        var existingScores = await scoreRepository.GetByBeatmapIdsAsync(beatmapIds, ct);
        var groupedExistingScores = existingScores.GroupBy(s => new { s.BeatmapId, s.Mode }).ToList();
        var modeData = await beatmapRepository.GetModeDataAsync(beatmapIds, ct);

        var updatedCount = 0;
        var createdCount = 0;
        var deletedCount = 0;
        
        foreach (var group in groupedScores)
        {
            var groupScores = group
                .OrderByDescending(b => b.TotalScore)
                .ThenBy(b => b.Date)
                .Select(s =>
                {
                    var dto = entityToDtoService.ScoreEntityToDto(s, source, modeData[s.BeatmapId]);
                    return dto;
                })
                .DistinctBy(s => s.Id)
                .ToList();
            var matchingGroup = groupedExistingScores.FirstOrDefault(g => 
                g.Key.Mode == group.Key.Mode && g.Key.BeatmapId == group.Key.BeatmapId);
            
            if (matchingGroup == null)
            {
                groupScores = groupScores.Select((s, i) =>
                {
                    s.Rank = i + 1;
                    return s;
                }).ToList();
                // This is to prevent edge cases where there are somehow more than 100 scores per mode and combination,
                // even though there were none before. We can't verify scores ranked above 100.
                groupScores = groupScores.Where(s => s.Rank <= 100).ToList();
                scoreRepository.CreateBulk(groupScores);
                createdCount += groupScores.Count;
            }
            else
            {
                var beatmapScores = matchingGroup.ToList();

                var personalBestsForRemoval = new List<Score>();

                foreach (var score in groupScores.ToList())
                {
                    var previousScores = beatmapScores.Where(s => s.UserId == score.UserId && s.Id != score.Id)
                        .OrderByDescending(b => b.TotalScore)
                        .ThenBy(b => b.Date)
                        .ToList();

                    if (previousScores.Any())
                    {
                        var previousBest = previousScores.First();
                        if (previousScores.Count > 1) // There are multiple user scores in the DB somehow. Remove them as well
                        {
                            var worseUserScores = previousScores.Where((s, i) => i > 0).ToList();
                            personalBestsForRemoval.AddRange(worseUserScores);
                        } 
                        if (previousBest.TotalScore < score.TotalScore
                            || (previousBest.TotalScore == score.TotalScore && previousBest.Date > score.Date))
                        {
                            personalBestsForRemoval.Add(previousBest);
                        }
                        else
                        {
                            groupScores.Remove(score);
                        }
                    }
                }
                scoreRepository.DeleteBulk(personalBestsForRemoval);
                beatmapScores = beatmapScores.Where(s => !personalBestsForRemoval.Select(pb => pb.Id).Contains(s.Id)).ToList();
                deletedCount += personalBestsForRemoval.Count;
                
                var groupIds = groupScores.Select(s => s.Id).Distinct().ToList();
                
                var newScores = groupScores
                    .Where(b => !beatmapScores
                        .Select(s => s.Id)
                        .Contains(b.Id))
                    .ToList();
                
                var oldScores = beatmapScores
                    .Where(b => !newScores
                        .Select(s => s.Id)
                        .Contains(b.Id))
                    .ToList();
                
                var merged = newScores
                    .Concat(oldScores)
                    .OrderByDescending(b => b.TotalScore)
                    .ThenBy(b => b.Date)
                    .Select((s, i) =>
                    {
                        s.Rank = i + 1;
                        return s;
                    })
                    .ToList();
                
                // This branch of logic is only relevant for leaderboard rescans.
                // An old score may have been removed from the top 100 leaderboard between scans
                // due to the user getting restricted or for other reasons. If that happens, we should remove it.
                if (source == ScoreSource.LeaderboardScan)
                {
                    var removedScores = oldScores.Where(s => s.Rank <= 100 && !groupIds.Contains(s.Id)).ToList();
                    if (removedScores.Count > 0)
                    {
                        var removedScoreIds = removedScores.Select(s => s.Id).Distinct().ToList();
                        
                        scoreRepository.DeleteBulk(removedScores);
                        deletedCount += removedScores.Count;
                        
                        // If a score has all the necessary data, it doesn't get removed completely,
                        // but rather becomes unlisted. It may get restored if the user is unrestricted
                        var scoresWithNullData = GetScoreIdsWithNullData(removedScores);
                        var scoresToUnlist = removedScores
                            .Where(s => !scoresWithNullData.Contains(s.Id))
                            .Select(GetUnlsitedScoreFromScore)
                            .ToList();
                        var existingUnlistedScores = await unlistedScoreRepository.GetBulkAsync(scoresToUnlist.Select(s => s.Id), ct);
                        var existingScoreIds = existingUnlistedScores.Select(s => s.Id).ToList();
                        var newScoresToUnlist = scoresToUnlist.Where(s => !existingScoreIds.Contains(s.Id)).ToList();
                        if (newScoresToUnlist.Count > 0)
                        {
                            unlistedScoreRepository.CreateBulk(scoresToUnlist);
                            logger.Log(LogLevel.Information, "Unlisted {unlistedCount} scores", scoresToUnlist.Count);
                        }
                        
                        merged = merged
                            .Where(s => !removedScoreIds.Contains(s.Id))
                            .OrderByDescending(b => b.TotalScore)
                            .ThenBy(b => b.Date)
                            .Select((s, i) =>
                            {
                                s.Rank = i + 1;
                                return s;
                            })
                            .ToList();
                        
                        oldScores = oldScores.Where(s => !removedScoreIds.Contains(s.Id)).ToList();
                    }
                }

                // We remove any scores that land outside the top 100.
                // That's done to save up on storage. It's going to get really bad on new maps in the long run
                var scoresOutsideOfBuffer = merged.Where(s => s.Rank > 100).ToList();
                
                if (scoresOutsideOfBuffer.Count > 0)
                {
                    var scoreIds = scoresOutsideOfBuffer.Select(s => s.Id).Distinct().ToList();
                    var oldScoresOutsideTop100 = oldScores.Where(s => scoreIds.Contains(s.Id)).ToList();
                    
                    scoreRepository.DeleteBulk(oldScoresOutsideTop100);
                    
                    // This is pretty ugly and excessive, but you never know.
                    oldScores = oldScores.Where(s => !scoreIds.Contains(s.Id)).ToList();
                    newScores = newScores.Where(s => !scoreIds.Contains(s.Id)).ToList();
                    
                    deletedCount += oldScoresOutsideTop100.Count;
                }

                if (newScores.Count > 0)
                {
                    scoreRepository.CreateBulk(newScores);
                }
                
                if (oldScores.Count > 0)
                {
                    scoreRepository.UpdateBulk(oldScores);
                }
            
                updatedCount += oldScores.Count;
                createdCount += newScores.Count;
            }
        }

        try
        {
            await scoreRepository.SaveChangesAsync(ct);

            logger.Log(LogLevel.Information, "New scores: {createdCount}; Updated scores: {updatedCount}; Deleted scores: {deletedCount}", 
                createdCount, updatedCount, deletedCount);

            return createdCount;
        }
        catch (NpgsqlException exception)
        {
            logger.Log(LogLevel.Error, exception, "Couldn't process scores! Scores: {@scores}", scores);
            return 0;
        }
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
    private UnlistedScore GetUnlsitedScoreFromScore(Score score) => new UnlistedScore
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
    /// Get max beatmapset ID from the database
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The highest <see cref="Score"/> ID</returns>
    public Task<int> GetSecondHighestBeatmapsetIdAsync(CancellationToken cancellationToken) =>
        scoreRepository.GetSecondHighestBeatmapsetIdAsync(cancellationToken);
}