using Lazerboard.Data.Database;
using Microsoft.Extensions.Logging;
using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Entities.Enums;
using Lazerboard.Data.Database.Work;
using Lazerboard.Data.OsuEntities.Enums;
using Lazerboard.Data.OsuEntities.OsuApiEntities;
using Lazerboard.ScoreFetcher.OsuEntityToDtoService;
using Microsoft.Extensions.DependencyInjection;

namespace Lazerboard.ScoreFetcher.Processing;

public class DataProcessor(IServiceProvider serviceProvider,
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

        List<Beatmapset> existingBeatmapsets;
        using (var scope = serviceProvider.CreateScope())
        {
            await using var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            existingBeatmapsets = await unitOfWork.Beatmapsets.GetBulkAsync(beatmapsets.Select(bs => bs.Id).ToList(), ct);
        }
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

        using (var scope = serviceProvider.CreateScope())
        {
            await using var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            await unitOfWork.BeginTransactionAsync(ct);
            unitOfWork.Beatmapsets.CreateBulk(beatmapsetDtos);
            unitOfWork.Beatmapsets.UpdateBulk(existingBeatmapsets);
            await unitOfWork.CommitTransactionAsync(ct);
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
        
        List<Beatmap> existingBeatmaps;
        using (var scope = serviceProvider.CreateScope())
        {
            await using var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            existingBeatmaps = await unitOfWork.Beatmaps.GetBulkAsync(beatmaps.Select(b => b.Id).ToList(), ct);
        }
        var existingIds = existingBeatmaps.Select(b => b.Id).ToList();
        
        var beatmapDtos = beatmaps
            .Select(entityToDtoService.BeatmapEntityToDto)
            .DistinctBy(b => b.Id)
            .ToList();
        var newBeatmaps = beatmapDtos.Where(b => !existingIds.Contains(b.Id)).ToList();
        var beatmapsToUpdate = beatmapDtos.Where(b => existingIds.Contains(b.Id)).ToList();

        using (var scope = serviceProvider.CreateScope())
        {
            await using var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            await unitOfWork.BeginTransactionAsync(ct);
            unitOfWork.Beatmaps.CreateBulk(newBeatmaps);
            unitOfWork.Beatmaps.UpdateBulk(beatmapsToUpdate);
            await unitOfWork.CommitTransactionAsync(ct);
        }
    }

    /// <summary>
    /// Check for existing country data and save new country DTOs to the database.
    /// </summary>
    /// <param name="countries">The <see cref="APICountry"/> objects</param>
    /// <param name="ct">A <see cref="CancellationToken"/></param>
    public async Task ProcessCountriesAsync(IList<APICountry> countries, CancellationToken ct)
    {
        if (countries.Count == 0) return;

        List<Country> existingCountries;
        using (var scope = serviceProvider.CreateScope())
        {
            await using var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            existingCountries = await unitOfWork.Countries.GetBulkAsync(countries.Select(c => c.Code), ct);
        }
        
        var newCountries = countries.Where(co => !existingCountries.Select(c => c.Id).Contains(co.Code));
        var countryDtos = newCountries.Select(entityToDtoService.CountryEntityToDto).DistinctBy(c => c.Id).ToList();

        if (countryDtos.Count == 0) return;

        using (var scope = serviceProvider.CreateScope())
        {
            await using var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            await unitOfWork.BeginTransactionAsync(ct);
            unitOfWork.Countries.CreateBulk(countryDtos);
            await unitOfWork.CommitTransactionAsync(ct);
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

        List<User> existingUsers;
        using (var scope = serviceProvider.CreateScope())
        {
            await using var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            existingUsers = await unitOfWork.Users.GetBulkWithCountriesAsync(users.Select(u => u.Id).ToList(), ct);
        }
        var existingIds = existingUsers.Select(u => u.Id).ToList();
        
        var userDtos = users.Select(entityToDtoService.UserEntityToDto).ToList();
        
        var newUsers = userDtos
            .Where(u => !existingIds.Contains(u.Id))
            .DistinctBy(u => u.Id);
        var oldUsers = userDtos
            .Where(u => existingIds.Contains(u.Id))
            .DistinctBy(u => u.Id);

        using (var scope = serviceProvider.CreateScope())
        {
            await using var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            await unitOfWork.BeginTransactionAsync(ct);
            unitOfWork.Users.CreateBulk(newUsers);
            unitOfWork.Users.UpdateBulk(oldUsers);
            await unitOfWork.CommitTransactionAsync(ct);
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
        
        List<User> existingUsers;
        using (var scope = serviceProvider.CreateScope())
        {
            await using var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            existingUsers = await unitOfWork.Users.GetBulkWithCountriesAsync(users.Select(u => u.Id).ToList(), ct);
        }
        var existingIds = existingUsers.Select(u => u.Id).ToList();
        
        var newUsers = users
            .Where(u => !existingIds.Contains(u.Id))
            .DistinctBy(u => u.Id);
        var oldUsers = users
            .Where(u => existingIds.Contains(u.Id))
            .DistinctBy(u => u.Id);
        
        using (var scope = serviceProvider.CreateScope())
        {
            await using var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            await unitOfWork.BeginTransactionAsync(ct);
            unitOfWork.Users.CreateBulk(newUsers);
            unitOfWork.Users.UpdateBulk(oldUsers);
            await unitOfWork.CommitTransactionAsync(ct);
        }
    }
    
    /// <summary>
    /// Check for existing score data and save new score DTOs, assigning a rank to each one.
    /// </summary>
    /// <param name="scores">The <see cref="APIScore"/>s</param>
    /// <param name="source">The <see cref="ScoreSource"/></param>
    /// <param name="ct">A <see cref="CancellationToken"/></param>
    public async Task<int> ProcessScoresAsync(IList<APIScore> scores, 
        ScoreSource source,
        CancellationToken ct)
    {
        if (scores.Count == 0) return 0;
        scores = scores.DistinctBy(s => s.Id).ToList(); // Deduplicate by ID just in case.
        
        logger.Log(LogLevel.Information, "Processing {count} significant scores...", scores.Count);
        var beatmapIds = scores.Select(s => s.BeatmapId).Distinct().ToList();
        var groupedScores = scores.GroupBy(s => new { s.BeatmapId, s.Mode });

        List<Score> existingScores;
        Dictionary<int, Mode> modeData;
        using (var scope = serviceProvider.CreateScope())
        {
            await using var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            existingScores = await unitOfWork.Scores.GetByBeatmapIdsAsync(beatmapIds, ct);
            modeData = await unitOfWork.Beatmaps.GetModeDataAsync(beatmapIds, ct);
        }
        var groupedExistingScores = existingScores.GroupBy(s => new { s.BeatmapId, s.Mode }).ToList();

        var updatedCount = 0;
        var createdCount = 0;
        var deletedCount = 0;
        
        foreach (var group in groupedScores)
        {
            using var scope = serviceProvider.CreateScope();
            await using var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            await unitOfWork.BeginTransactionAsync(ct);
            
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
                unitOfWork.Scores.CreateBulk(groupScores);
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
                unitOfWork.Scores.DeleteBulk(personalBestsForRemoval);
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
                        
                        unitOfWork.Scores.DeleteBulk(removedScores);
                        deletedCount += removedScores.Count;
                        
                        // If a score has all the necessary data, it doesn't get removed completely,
                        // but rather becomes unlisted. It may get restored if the user is unrestricted
                        var scoresWithNullData = GetScoreIdsWithNullData(removedScores);
                        var scoresToUnlist = removedScores
                            .Where(s => !scoresWithNullData.Contains(s.Id))
                            .Select(GetUnlsitedScoreFromScore)
                            .ToList();
                        var existingUnlistedScores = await unitOfWork.UnlistedScores.GetBulkAsync(scoresToUnlist.Select(s => s.Id), ct);
                        var existingScoreIds = existingUnlistedScores.Select(s => s.Id).ToList();
                        var newScoresToUnlist = scoresToUnlist.Where(s => !existingScoreIds.Contains(s.Id)).ToList();
                        if (newScoresToUnlist.Count > 0)
                        {
                            unitOfWork.UnlistedScores.CreateBulk(scoresToUnlist);
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
                    
                    unitOfWork.Scores.DeleteBulk(oldScoresOutsideTop100);
                    
                    // This is pretty ugly and excessive, but you never know.
                    oldScores = oldScores.Where(s => !scoreIds.Contains(s.Id)).ToList();
                    newScores = newScores.Where(s => !scoreIds.Contains(s.Id)).ToList();
                    
                    deletedCount += oldScoresOutsideTop100.Count;
                }

                if (newScores.Count > 0)
                {
                    unitOfWork.Scores.CreateBulk(newScores);
                }
                
                if (oldScores.Count > 0)
                {
                    unitOfWork.Scores.UpdateBulk(oldScores);
                }
            
                updatedCount += oldScores.Count;
                createdCount += newScores.Count;
            }

            await unitOfWork.CommitTransactionAsync(ct);
        }

        logger.Log(LogLevel.Information, "New scores: {createdCount}; Updated scores: {updatedCount}; Deleted scores: {deletedCount}", 
            createdCount, updatedCount, deletedCount);

        return createdCount;
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
}