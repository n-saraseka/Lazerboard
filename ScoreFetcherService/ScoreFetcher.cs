using Microsoft.EntityFrameworkCore;
using OsuScoreStats.Calculators;
using OsuScoreStats.DbService;
using OsuScoreStats.DbService.Repositories;
using OsuScoreStats.OsuApi;
using OsuScoreStats.OsuApi.Enums;
using OsuScoreStats.OsuApi.OsuApiEntities;
using OsuScoreStats.OsuEntityToDtoService;
using Score = OsuScoreStats.OsuApi.OsuApiEntities.Score;
using User = OsuScoreStats.OsuApi.OsuApiEntities.User;

namespace OsuScoreStats.ScoreFetcherService;

public class ScoreFetcher(OsuApiService osuApiService, 
    ICalculator scoreCalculator, 
    IDbContextFactory<ScoreDataContext> dbContextFactory,
    IOsuEntityToDtoService entityToDtoService) : IScoreFetcher
{
    /// <summary>
    /// Get beatmapsets from API and save beatmapset and beatmap data
    /// </summary>
    /// <param name="cursor">Cursor string</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Populated BeatmapsetsResponse object</returns>
    public async Task<BeatmapsetsResponse> ProcessBeatmapsetSearchAsync(string? cursor, CancellationToken ct = default)
    {
        var beatmapsetsResponse = await osuApiService.GetBeatmapsetsAsync(cursor, ct);
        
        var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        
        var beatmapsets = beatmapsetsResponse.Beatmapsets;
        await ProcessBeatmapsetsAsync(beatmapsets, dbContext, ct);
        var beatmaps = beatmapsets.SelectMany(bs => bs.Beatmaps);
        await ProcessBeatmapsAsync(beatmaps, dbContext, ct);
        
        return beatmapsetsResponse;
    }

    public async Task<BeatmapScores> GetBeatmapScoresAsync(APIBeatmap beatmap, Mode? mode, int legacyOnly = 0, CancellationToken ct = default)
    {
        var scores = await osuApiService.GetBeatmapScoresAsync(beatmap.Id, mode, legacyOnly, ct); 
        var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        
        var users = scores.Scores.Select(s => s.User).Distinct().ToList();
        await GetUsersFromApiAsync(users.Select(u => u.Id), ct);
        
        var scoreRepository = new ScoreRepository(dbContext);

        var scoreIds = scores.Scores.Select(s => s.Id).Distinct().ToList();
        var existingScores = await scoreRepository.GetBulkAsync(scoreIds, ct);
        var newScores = scores.Scores.Where(score => !existingScores.Select(s => s.Id).Contains(score.Id)).ToList();

        var unrankedScores = newScores.Where(s => s.PP == null);
        var rankedScores =  newScores.Where(s => s.PP != null);

        var scoreTasks = new List<Task>();
        
        if (rankedScores.Count() > 0) 
            scoreTasks.Add(ProcessRankedScoresAsync(rankedScores, ct));
        if (unrankedScores.Count() > 0)
            scoreTasks.Add(ProcessUnrankedScoresAsync(unrankedScores, ct));
        
        await Task.WhenAll(scoreTasks);

        return scores;
    }
    
    /// <summary>
    /// Get scores from the API firehose
    /// </summary>
    /// <param name="cursor">Cursor string</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Populated ScoresResponse object</returns>
    public async Task<ScoresResponse> GetScoresAsync(string? cursor, CancellationToken ct = default)
    {
        return await osuApiService.GetScoresAsync(cursor, ct);
    }
    
    /// <summary>
    /// Process data from unranked scores, including PP calculation. Calculates highest PP scores for each mode
    /// </summary>
    /// <param name="scores">Unranked scores to process</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns></returns>
    public async Task ProcessUnrankedScoresAsync(IEnumerable<Score> scores, CancellationToken ct = default)
    {
        var scoresList = scores.ToList();
        
        var start = scoresList[0].Date;
        var end = scoresList[scoresList.Count - 1].Date;
        var scoresCounter = scoresList.Count;

        for (int i = 0; i < scoresCounter; i++)
        {
            scoresList[i].PP = await scoreCalculator.CalculateAsync(scoresList[i], ct);
            await Task.Delay(1000, ct);
        }
        
        var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        
        ScoreRepository scoreRepository = new(dbContext);
        var scoreDtos = scoresList.Select(entityToDtoService.ScoreEntityToDto);
        scoreRepository.CreateBulk(scoreDtos);
        await scoreRepository.SaveChangesAsync(ct);
        
        Console.WriteLine($"Saved {scoresCounter} unranked scores between {start} and {end} to the DB.");
    }
    
    /// <summary>
    /// Process data from ranked scores. Calculates highest PP scores for each mode
    /// </summary>
    /// <param name="scores">Ranked scores to process</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns></returns>
    public async Task ProcessRankedScoresAsync(IEnumerable<Score> scores, CancellationToken ct = default)
    {
        var scoresList = scores.ToList();
        
        var start = scoresList[0].Date;
        var end = scoresList[scoresList.Count - 1].Date;
        var scoresCounter = scoresList.Count;
        
        var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        
        ScoreRepository scoreRepository = new(dbContext);
        var scoreDtos = scoresList.Select(entityToDtoService.ScoreEntityToDto);
        scoreRepository.CreateBulk(scoreDtos);
        await scoreRepository.SaveChangesAsync(ct);
        
        Console.WriteLine($"Saved {scoresCounter} ranked scores between {start} and {end} to the DB.");
    }
    
    /// <summary>
    /// Get user data from API and process the respective data
    /// </summary>
    /// <param name="userIds">IEnumerable containing user IDs</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns></returns>
    public async Task GetUsersFromApiAsync(IEnumerable<int> userIds, CancellationToken ct = default)
    {
        const int batchSize = 50;
        var users = new List<User>();
        
        if (userIds.Count() > 0)
        {
            for (int i = 0; i < userIds.Count(); i += batchSize)
            {
                var batch = userIds.Skip(i).Take(batchSize).ToList();
                User[] userData = await osuApiService.GetUsersAsync(batch, ct);
                users.AddRange(userData);
            }
        }
        
        var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        
        var countries = users.Select(u => u.Country).DistinctBy(c => c.Code);
        await ProcessCountriesAsync(countries, dbContext, ct);
        await ProcessUsersAsync(users, dbContext, ct);
    }
    
    /// <summary>
    /// Get beatmaps from API and process the data
    /// </summary>
    /// <param name="beatmapIds">IEnumerable containing beatmap IDs</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns></returns>
    public async Task GetBeatmapsFromApiAsync(IEnumerable<int> beatmapIds, CancellationToken ct = default)
    {
        const int batchSize = 50;
        var beatmaps = new List<APIBeatmap>();
        
        var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        var beatmapRepository = new BeatmapRepository(dbContext);
        var existingBeatmaps = await beatmapRepository.GetBulkAsync(beatmapIds, ct);
        
        var newBeatmapIds = beatmapIds.Where(id => !existingBeatmaps.Select(b => b.Id).Contains(id));
        
        if (newBeatmapIds.Count() > 0)
        {
            for (int i = 0; i < newBeatmapIds.Count(); i += batchSize)
            {
                var batch = newBeatmapIds.Skip(i).Take(batchSize).ToList();
                APIBeatmap[] beatmapData = await osuApiService.GetBeatmapsAsync(batch, ct);
                beatmaps.AddRange(beatmapData);
            }
        }
        
        var beatmapsets = beatmaps.Select(b => b.Beatmapset).DistinctBy(b => b.Id).ToList();
        await ProcessBeatmapsetsAsync(beatmapsets, dbContext, ct);
        await ProcessBeatmapsAsync(beatmaps, dbContext, ct);
    }
    
    private async Task ProcessBeatmapsetsAsync(IEnumerable<Beatmapset> beatmapsets, DbContext dbContext, CancellationToken ct)
    {
        var beatmapsetRepository = new BeatmapsetRepository(dbContext);
        
        var existingBeatmapsets = await beatmapsetRepository.GetBulkAsync(beatmapsets.Select(bs => bs.Id), ct);
        var newBeatmapsets = beatmapsets.Where(bs => !existingBeatmapsets.Select(s => s.Id).Contains(bs.Id));
        var beatmapsetDtos = newBeatmapsets.Select(entityToDtoService.BeatmapsetEntityToDto);
        
        beatmapsetRepository.CreateBulk(beatmapsetDtos);
        await beatmapsetRepository.SaveChangesAsync(ct);
        
        Console.WriteLine($"Saved {beatmapsetDtos.Count()} new beatmapsets to the DB.");
    }

    private async Task ProcessBeatmapsAsync(IEnumerable<APIBeatmap> beatmaps, DbContext dbContext, CancellationToken ct)
    {
        var beatmapRepository = new BeatmapRepository(dbContext);
        
        var existingBeatmaps = await beatmapRepository.GetBulkAsync(beatmaps.Select(b => b.Id), ct);
        var newBeatmaps = beatmaps.Where(b => !existingBeatmaps.Select(s => s.Id).Contains(b.Id));
        var beatmapDtos = newBeatmaps.Select(entityToDtoService.BeatmapEntityToDto);
        
        beatmapRepository.CreateBulk(beatmapDtos);
        await beatmapRepository.SaveChangesAsync(ct);
        
        Console.WriteLine($"Saved {beatmapDtos.Count()} new beatmaps to the DB.");
    }

    private async Task ProcessCountriesAsync(IEnumerable<Country> countries, DbContext dbContext, CancellationToken ct)
    {
        var countryRepository = new CountryRepository(dbContext);
        
        var existingCountries = await countryRepository.GetBulkAsync(countries.Select(c => c.Code), ct);
        var newCountries = countries.Where(co => !existingCountries.Select(c => c.Id).Contains(co.Code));
        var countryDtos = newCountries.Select(entityToDtoService.CountryEntityToDto);
        
        countryRepository.CreateBulk(countryDtos);
        await countryRepository.SaveChangesAsync(ct);
        
        Console.WriteLine($"Saved {countryDtos.Count()} new countries to the DB.");
    }

    private async Task ProcessUsersAsync(IEnumerable<User> users, DbContext dbContext, CancellationToken ct)
    {
        var userRepository = new UserRepository(dbContext);
        
        var existingUsers = await userRepository.GetBulkAsync(users.Select(u => u.Id), ct);
        var userDtos = users.Select(entityToDtoService.UserEntityToDto);
        var newUsers = userDtos.Where(u => !existingUsers.Select(s => s.Id).Contains(u.Id));
        
        userRepository.CreateBulk(newUsers);
        Console.WriteLine($"Saved {newUsers.Count()} new users to the DB.");
        
        await userRepository.SaveChangesAsync(ct);
    }
}