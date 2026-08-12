using Microsoft.Extensions.Logging;
using Npgsql;
using OsuScoreStats.Shared.DbService.Entities;
using OsuScoreStats.Shared.DbService.Repositories.Interfaces;
using OsuScoreStats.Shared.OsuApi.OsuApiEntities;
using OsuScoreStats.Shared.OsuEntityToDtoService;

namespace OsuScoreStats.Shared.Processing;

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
    public async Task ProcessBeatmapsetsAsync(IList<APIBeatmapset> beatmapsets, CancellationToken ct)
    {
        if (beatmapsets.Count == 0) return;
        var existingBeatmapsets = await GetExistingBeatmapsetsAsync(beatmapsets.Select(bs => bs.Id).ToList(), ct);
        var newBeatmapsets = beatmapsets.Where(bs => !existingBeatmapsets.Select(s => s.Id).Contains(bs.Id));
        var beatmapsetDtos = newBeatmapsets
            .Select(entityToDtoService.BeatmapsetEntityToDto)
            .DistinctBy(bs => bs.Id)
            .ToList();
        
        beatmapsetRepository.CreateBulk(beatmapsetDtos);
        try
        {
            await beatmapsetRepository.SaveChangesAsync(ct);
            logger.Log(LogLevel.Information, "New beatmapsets: {createdCount}", beatmapsetDtos.Count);
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
        var newBeatmaps = beatmaps.Where(b => !existingBeatmaps.Select(s => s.Id).Contains(b.Id));
        var beatmapDtos = newBeatmaps
            .Select(entityToDtoService.BeatmapEntityToDto)
            .DistinctBy(b => b.Id)
            .ToList();
        
        beatmapRepository.CreateBulk(beatmapDtos);
        try
        {
            await beatmapRepository.SaveChangesAsync(ct);
            logger.Log(LogLevel.Information, "New beatmaps: {createdCount}", beatmapDtos.Count);
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
        
        countryRepository.CreateBulk(countryDtos);
        try
        {
            await countryRepository.SaveChangesAsync(ct);
            logger.Log(LogLevel.Information, "New countries: {createdCount}", countryDtos.Count);
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
        var userDtos = users.Select(entityToDtoService.UserEntityToDto);
        var newUsers = userDtos
            .Where(u => !existingUsers.Select(s => s.Id).Contains(u.Id))
            .DistinctBy(u => u.Id)
            .ToList();
        
        userRepository.CreateBulk(newUsers);
        try
        {
            await userRepository.SaveChangesAsync(ct);
            logger.Log(LogLevel.Information, "New users: {createdCount}", newUsers.Count);
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
            .DistinctBy(u => u.Id)
            .ToList();
        
        userRepository.CreateBulk(newUsers);
        try
        {
            await userRepository.SaveChangesAsync(ct);
            logger.Log(LogLevel.Information, "New users: {createdCount}", newUsers.Count);
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
    public async Task ProcessScoresAsync(IList<APIScore> scores, CancellationToken ct)
    {
        if (scores.Count == 0) return;
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
                scoreRepository.CreateBulk(groupScores);
                createdCount += groupScores.Count;
            }
            else
            {
                var beatmapScores = matchingGroup.ToList();

                foreach (var score in groupScores.ToList())
                {
                    var matchingScores = beatmapScores.Where(s => s.UserId == score.UserId && s.Mode == score.Mode).ToList();
                    if (matchingScores.Count > 0)
                    {
                        scoreRepository.DeleteBulk(matchingScores);
                        foreach (var s in matchingScores)
                            beatmapScores.Remove(s);
                        deletedCount++;
                    }
                }
                
                var newScores = 
                    groupScores.Where(b => !beatmapScores
                            .Select(s => s.Id)
                            .Contains(b.Id))
                            .ToList();
                var merged = beatmapScores
                    .Concat(newScores)
                    .OrderByDescending(b => b.TotalScore)
                    .ThenBy(b => b.Date)
                    .ToList();
            
                foreach (var score in merged) score.Rank = merged.IndexOf(score) +1;

                if (newScores.Count > 0)
                {
                    scoreRepository.CreateBulk(newScores);
                }

                if (beatmapScores.Count > 0)
                {
                    scoreRepository.UpdateBulk(beatmapScores);
                }
            
                updatedCount += beatmapScores.Count;
                createdCount += newScores.Count;
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
}