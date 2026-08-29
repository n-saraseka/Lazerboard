using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Repositories.Interfaces;
using Lazerboard.Data.OsuEntities.OsuApiEntities;
using Lazerboard.ScoreFetcher.OsuEntityToDtoService;

namespace Lazerboard.ScoreFetcher.Processing;

public class DataProcessor(IBeatmapsetRepository beatmapsetRepository,
    IBeatmapRepository beatmapRepository,
    ICountryRepository countryRepository,
    IUserRepository userRepository,
    IScoreRepository scoreRepository,
    IOsuEntityToDtoService entityToDtoService, 
    ILogger<IDataProcessor> logger): IDataProcessor
{
    /// <summary>
    /// Check for existing beatmapset data and save new beatmapset DTOs to the database.
    /// </summary>
    /// <param name="beatmapsets">The <see cref="APIBeatmapset"/>s</param>
    /// <param name="ct">A <see cref="CancellationToken"/></param>
    public async Task ProcessBeatmapsetsAsync(IEnumerable<APIBeatmapset> beatmapsets, CancellationToken ct)
    {
        if (beatmapsets.Count() == 0) return;
        var existingBeatmapsets = await GetExistingBeatmapsetsAsync(beatmapsets.Select(bs => bs.Id), ct);
        var newBeatmapsets = beatmapsets.Where(bs => !existingBeatmapsets.Select(s => s.Id).Contains(bs.Id));
        var beatmapsetDtos = newBeatmapsets
            .Select(entityToDtoService.BeatmapsetEntityToDto)
            .DistinctBy(bs => bs.Id);
        
        beatmapsetRepository.CreateBulk(beatmapsetDtos);
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
    public async Task ProcessBeatmapsAsync(IEnumerable<APIBeatmap> beatmaps, CancellationToken ct)
    {
        if (beatmaps.Count() == 0) return;
        var existingBeatmaps = await GetExistingBeatmapsAsync(beatmaps.Select(b => b.Id), ct);
        var newBeatmaps = beatmaps.Where(b => !existingBeatmaps.Select(s => s.Id).Contains(b.Id));
        var beatmapDtos = newBeatmaps
            .Select(entityToDtoService.BeatmapEntityToDto)
            .DistinctBy(b => b.Id);
        
        beatmapRepository.CreateBulk(beatmapDtos);
        try
        {
            await beatmapRepository.SaveChangesAsync(ct);
        }
        catch (NpgsqlException exception)
        {
            logger.Log(LogLevel.Error, exception, "Method: ProcessBeatmapsAsync | Beatmaps: {@beatmaps}", beatmapDtos);
        }
    }
    
    public Task<List<Beatmap>> GetExistingBeatmapsAsync(IEnumerable<int> ids, CancellationToken ct) =>
        beatmapRepository.GetBulkAsync(ids, ct);
    
    public Task<List<Beatmapset>> GetExistingBeatmapsetsAsync(IEnumerable<int> ids, CancellationToken ct) =>
        beatmapsetRepository.GetBulkAsync(ids, ct);

    public Task<List<User>> GetExistingUsersAsync(IEnumerable<int> ids, CancellationToken ct) =>
        userRepository.GetBulkAsync(ids, ct);

    /// <summary>
    /// Check for existing country data and save new country DTOs to the database.
    /// </summary>
    /// <param name="countries">The <see cref="APICountry"/> objects</param>
    /// <param name="ct">A <see cref="CancellationToken"/></param>
    public async Task ProcessCountriesAsync(IEnumerable<APICountry> countries, CancellationToken ct)
    {
        if (countries.Count() == 0) return;
        var existingCountries = await countryRepository.GetBulkAsync(countries.Select(c => c.Code), ct);
        var newCountries = countries.Where(co => !existingCountries.Select(c => c.Id).Contains(co.Code));
        var countryDtos = newCountries.Select(entityToDtoService.CountryEntityToDto).DistinctBy(c => c.Id);
        
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
    public async Task ProcessUsersAsync(IEnumerable<APIUser> users, CancellationToken ct)
    {
        if (users.Count() == 0) return;
        var existingUsers = await GetExistingUsersAsync(users.Select(u => u.Id), ct);
        var userDtos = users.Select(entityToDtoService.UserEntityToDto);
        var newUsers = userDtos
            .Where(u => !existingUsers.Select(s => s.Id).Contains(u.Id))
            .DistinctBy(u => u.Id);
        
        userRepository.CreateBulk(newUsers);
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
    public async Task ProcessRemovedUsersAsync(IEnumerable<User> users, CancellationToken ct)
    {
        if (users.Count() == 0) return;
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
    /// <param name="ct">A <see cref="CancellationToken"/></param>
    public async Task ProcessScoresAsync(IEnumerable<APIScore> scores, CancellationToken ct)
    {
        if (scores.Count() == 0) return;
        logger.Log(LogLevel.Information, "Processing {count} significant scores...", scores.Count());
        var beatmapIds = scores.Select(s => s.BeatmapId).Distinct();
        var groupedScores = scores.GroupBy(s => new { s.BeatmapId, s.Mode });
        var existingScores = await scoreRepository.GetByBeatmapIdsAsync(beatmapIds, ct);
        var groupedExistingScores = existingScores.GroupBy(s => new { s.BeatmapId, s.Mode }).ToList();

        var updatedCount = 0;
        var createdCount = 0;
        var deletedCount = 0;
        
        foreach (var group in groupedScores)
        {
            var groupScores = group
                .OrderByDescending(b => b.TotalScore)
                .ThenBy(b => b.Date)
                .Select(entityToDtoService.ScoreEntityToDto)
                .DistinctBy(s => s.Id)
                .ToList();
            var matchingGroup = groupedExistingScores.FirstOrDefault(g => 
                g.Key.Mode == group.Key.Mode && g.Key.BeatmapId == group.Key.BeatmapId);
            if (matchingGroup == null)
            {
                foreach (var score in groupScores) score.Rank = groupScores.IndexOf(score) + 1;
                // This is to prevent edge cases where there are somehow more than 100 scores per mode and combination,
                // even though there were none before. We can't verify scores ranked above 100.
                groupScores = groupScores.Where(s => s.Rank <= 100).ToList();
                scoreRepository.CreateBulk(groupScores);
                createdCount += groupScores.Count;
            }
            else
            {
                var beatmapScores = matchingGroup.ToList();

                var personalBests = new List<Score>();

                foreach (var score in groupScores.ToList())
                {
                    var matchingScores = beatmapScores.Where(s => s.UserId == score.UserId && s.Mode == score.Mode).ToList();
                    personalBests.AddRange(matchingScores);
                }
                scoreRepository.DeleteBulk(personalBests);
                beatmapScores = beatmapScores.Where(s => !personalBests.Select(pb => pb.Id).Contains(s.Id)).ToList();
                deletedCount += personalBests.Count;
                
                var newScores = 
                    groupScores.Where(b => !beatmapScores
                            .Select(s => s.Id)
                            .Contains(b.Id));
                var merged = beatmapScores
                    .Concat(newScores)
                    .OrderByDescending(b => b.TotalScore)
                    .ThenBy(b => b.Date)
                    .ToList();
            
                foreach (var score in merged) score.Rank = merged.IndexOf(score) +1;

                if (newScores.Count() > 0)
                {
                    scoreRepository.CreateBulk(newScores);
                }

                if (beatmapScores.Count > 0)
                {
                    scoreRepository.UpdateBulk(beatmapScores);
                }
            
                updatedCount += beatmapScores.Count;
                createdCount += newScores.Count();
            }
        }

        try
        {
            await scoreRepository.SaveChangesAsync(ct);

            logger.Log(LogLevel.Information, "New scores: {createdCount}; Updated scores: {updatedCount}; Deleted scores: {deletedCount}", 
                createdCount, updatedCount, deletedCount);
        }
        catch (NpgsqlException exception)
        {
            logger.Log(LogLevel.Error, exception, "Method: ProcessScoresAsync | Scores: {@scores}", scores);
        }
    }

    /// <summary>
    /// Get all <see cref="Score.BeatmapId"/>s with scores matching the given IDs
    /// </summary>
    /// <param name="beatmapIds">The <see cref="Beatmap"/> IDs</param>
    /// <param name="ct">A <see cref="CancellationToken"/></param>
    /// <returns>A <see cref="List{int}"/> with all beatmap IDs</returns>
    public Task<List<int>> GetBeatmapIdsWithScoresAsync(IList<int> beatmapIds, CancellationToken ct) =>
        scoreRepository.GetAll().Where(s => beatmapIds.Contains(s.BeatmapId)).Select(s => s.BeatmapId).Distinct().ToListAsync(ct);
}