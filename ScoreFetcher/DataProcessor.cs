using OsuScoreStats.DbService.Entities;
using OsuScoreStats.DbService.Repositories.Interfaces;
using OsuScoreStats.OsuApi.OsuApiEntities;
using OsuScoreStats.OsuEntityToDtoService;

namespace OsuScoreStats.ScoreFetcher;

public class DataProcessor(IBeatmapsetRepository beatmapsetRepository,
    IBeatmapRepository beatmapRepository,
    ICountryRepository countryRepository,
    IUserRepository userRepository,
    IScoreRepository scoreRepository,
    IOsuEntityToDtoService entityToDtoService): IDataProcessor
{
    /// <summary>
    /// Check for existing beatmapset data and save new beatmapset DTOs to the database.
    /// </summary>
    /// <param name="beatmapsets">The <see cref="APIBeatmapset"/>s</param>
    /// <param name="ct">A <see cref="CancellationToken"/></param>
    public async Task ProcessBeatmapsetsAsync(IEnumerable<APIBeatmapset> beatmapsets, CancellationToken ct)
    {
        var existingBeatmapsets = await GetExistingBeatmapsetsAsync(beatmapsets.Select(bs => bs.Id), ct);
        var newBeatmapsets = beatmapsets.Where(bs => !existingBeatmapsets.Select(s => s.Id).Contains(bs.Id));
        var beatmapsetDtos = newBeatmapsets
            .Select(entityToDtoService.BeatmapsetEntityToDto)
            .DistinctBy(bs => bs.Id);
        
        beatmapsetRepository.CreateBulk(beatmapsetDtos);
        await beatmapsetRepository.SaveChangesAsync(ct);
        
        Console.WriteLine($"Saved {beatmapsetDtos.Count()} new beatmapsets to the DB.");
    }

    /// <summary>
    /// Check for existing beatmap data and save new beatmap DTOs to the database.
    /// </summary>
    /// <param name="beatmaps">The <see cref="APIBeatmap"/>s</param>
    /// <param name="ct">A <see cref="CancellationToken"/></param>
    public async Task ProcessBeatmapsAsync(IEnumerable<APIBeatmap> beatmaps, CancellationToken ct)
    {
        var existingBeatmaps = await GetExistingBeatmapsAsync(beatmaps.Select(b => b.Id), ct);
        var newBeatmaps = beatmaps.Where(b => !existingBeatmaps.Select(s => s.Id).Contains(b.Id));
        var beatmapDtos = newBeatmaps
            .Select(entityToDtoService.BeatmapEntityToDto)
            .DistinctBy(b => b.Id);
        
        beatmapRepository.CreateBulk(beatmapDtos);
        await beatmapRepository.SaveChangesAsync(ct);
        
        Console.WriteLine($"Saved {beatmapDtos.Count()} new beatmaps to the DB.");
    }
    
    public Task<List<Beatmap>> GetExistingBeatmapsAsync(IEnumerable<int> ids, CancellationToken ct) =>
        beatmapRepository.GetBulkAsync(ids, ct);
    
    public Task<List<Beatmapset>> GetExistingBeatmapsetsAsync(IEnumerable<int> ids, CancellationToken ct) =>
        beatmapsetRepository.GetBulkAsync(ids, ct);

    /// <summary>
    /// Check for existing country data and save new country DTOs to the database.
    /// </summary>
    /// <param name="countries">The <see cref="APICountry"/> objects</param>
    /// <param name="ct">A <see cref="CancellationToken"/></param>
    public async Task ProcessCountriesAsync(IEnumerable<APICountry> countries, CancellationToken ct)
    {
        var existingCountries = await countryRepository.GetBulkAsync(countries.Select(c => c.Code), ct);
        var newCountries = countries.Where(co => !existingCountries.Select(c => c.Id).Contains(co.Code));
        var countryDtos = newCountries.Select(entityToDtoService.CountryEntityToDto).DistinctBy(c => c.Id);
        
        countryRepository.CreateBulk(countryDtos);
        await countryRepository.SaveChangesAsync(ct);
        
        Console.WriteLine($"Saved {countryDtos.Count()} new countries to the DB.");
    }

    /// <summary>
    /// Check for existing user data and save new user DTOs to the database.
    /// </summary>
    /// <param name="users">The <see cref="APIUser"/>s</param>
    /// <param name="ct">A <see cref="CancellationToken"/></param>
    public async Task ProcessUsersAsync(IEnumerable<APIUser> users, CancellationToken ct)
    {
        var existingUsers = await userRepository.GetBulkAsync(users.Select(u => u.Id), ct);
        var userDtos = users.Select(entityToDtoService.UserEntityToDto);
        var newUsers = userDtos
            .Where(u => !existingUsers.Select(s => s.Id).Contains(u.Id))
            .DistinctBy(u => u.Id);
        
        userRepository.CreateBulk(newUsers);
        await userRepository.SaveChangesAsync(ct);
        Console.WriteLine($"Saved {newUsers.Count()} new users to the DB.");
    }
    
    /// <summary>
    /// Check for existing user data for users without CountryCode's and save new user DTOs to the database.
    /// </summary>
    /// <param name="users">The <see cref="User"/>s</param>
    /// <param name="ct">A <see cref="CancellationToken"/></param>
    public async Task ProcessRemovedUsersAsync(IEnumerable<User> users, CancellationToken ct)
    {
        var existingUsers = await userRepository.GetBulkAsync(users.Select(u => u.Id), ct);
        var newUsers = users
            .Where(u => !existingUsers.Select(s => s.Id).Contains(u.Id))
            .DistinctBy(u => u.Id);
        
        userRepository.CreateBulk(newUsers);
        await userRepository.SaveChangesAsync(ct);
        Console.WriteLine($"Saved {newUsers.Count()} new users to the DB.");
    }
    
    /// <summary>
    /// Check for existing score data and save new score DTOs, assigning a rank to each one.
    /// </summary>
    /// <param name="scores">The <see cref="APIScore"/>s</param>
    /// <param name="ct">A <see cref="CancellationToken"/></param>
    public async Task ProcessScoresAsync(IEnumerable<APIScore> scores, CancellationToken ct)
    {
        var beatmapIds = scores.Select(s => s.BeatmapId).Distinct();
        var groupedByBeatmapId = scores.GroupBy(s => s.BeatmapId);
        var existingGroupedScores = await scoreRepository.GetByBeatmapIdsAsync(beatmapIds, ct);

        var updatedCount = 0;
        var createdCount = 0;
        
        foreach (var group in groupedByBeatmapId)
        {
            var beatmapId = group.Key;
            var groupScores = group
                .OrderByDescending(b => b.TotalScore)
                .ThenBy(b => b.Date)
                .Select(entityToDtoService.ScoreEntityToDto)
                .DistinctBy(s => s.Id)
                .ToList();
            var matchingGroup = existingGroupedScores.FirstOrDefault(g => g.Key == beatmapId);
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
                    var matchingScore = beatmapScores.FirstOrDefault(b => b.Id == score.Id);
                    if (matchingScore != null)
                    {
                        matchingScore = score;
                        groupScores.Remove(score);
                    }
                }
                
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
            
                scoreRepository.CreateBulk(newScores);
                scoreRepository.UpdateBulk(beatmapScores);
            
                updatedCount += beatmapScores.Count;
                createdCount += newScores.Count();
            }
        }
        
        await scoreRepository.SaveChangesAsync(ct);
        
        Console.WriteLine($"Saved {createdCount} new scores to the DB.");
        Console.WriteLine($"Updated {updatedCount} scores from the DB.");
    }
}