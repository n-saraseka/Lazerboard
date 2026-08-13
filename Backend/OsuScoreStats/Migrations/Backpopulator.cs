using Microsoft.EntityFrameworkCore;
using Npgsql;
using OsuScoreStats.DbService.Entities;
using OsuScoreStats.DbService.Repositories.Interfaces;
using OsuScoreStats.Processing;

namespace OsuScoreStats.Migrations;

public class Backpopulator(IBeatmapsetRepository beatmapsetRepo,
    IBeatmapRepository beatmapRepo,
    IUserRepository userRepo, 
    IApiFetcher apiFetcher, 
    IDataProcessor dataProcessor,
    ILogger<IBackpopulator> logger): IBackpopulator
{
    public async Task BackpopulateAsync(CancellationToken token)
    {
        await AddMissingHealthAttributesAsync(token);
        await AddMissingUserAttributesToBeatmapsAsync(token);
    }

    private async Task AddMissingUserAttributesToBeatmapsAsync(CancellationToken token)
    {
        var beatmapsets = await beatmapsetRepo.GetAll().Where(b => b.UserId == null).ToListAsync(token);
        var beatmapsetIds = beatmapsets.Select(b => b.Id).ToList();
        var beatmaps = await beatmapRepo.GetAll().Where(b => beatmapsetIds.Contains(b.BeatmapsetId)).ToListAsync(token);
        if (beatmaps.Count > 0)
        {
            logger.Log(LogLevel.Information, "Adding missing user attributes. Beatmapsets count: {count}", beatmapsets.Count);
            Console.WriteLine("Adding missing user attributes");
            var apiBeatmaps = await apiFetcher.GetBeatmapsAsync(beatmaps.Select(b => b.Id).ToList(), token);
            var apiBeatmapsets = apiBeatmaps.Select(b => b.Beatmapset).DistinctBy(b => b.Id).ToList();
            
            var userIds = apiBeatmapsets.Select(b => b.UserId).Distinct().ToList();
            var apiUsers = await apiFetcher.GetUsersAsync(userIds, token);
            var apiCountries = apiUsers.Select(u => u.Country).DistinctBy(c => c.Code).ToList();
            await dataProcessor.ProcessCountriesAsync(apiCountries, token);
            await dataProcessor.ProcessUsersAsync(apiUsers, token);
            
            var apiUserIds = apiUsers.Select(u => u.Id).ToList();
            var deletedOrRestrictedUserIds = userIds.Where(id => !apiUserIds.Contains(id)).ToList();

            var deletedOrRestrictedUsers = deletedOrRestrictedUserIds.Select(id => new User
            {
                Id = id,
                Username = apiBeatmapsets.First(b => b.UserId == id).Creator
            });
            
            var existingUsers = await userRepo.GetBulkAsync(deletedOrRestrictedUserIds, token);
            var existingUserIds = existingUsers.Select(u => u.Id).ToList();
            var newUsers = deletedOrRestrictedUsers.Where(u => !existingUserIds.Contains(u.Id)).ToList();
            userRepo.CreateBulk(newUsers);
            try
            {
                await userRepo.SaveChangesAsync(token);
            }
            catch (NpgsqlException ex)
            {
                logger.Log(LogLevel.Error, ex, "Method: IUserRepository.SaveChangesAsync; Users: {users}", newUsers);
            }
            
            foreach (var beatmapset in beatmapsets)
            {
                var respectiveApiBeatmapset = apiBeatmapsets.FirstOrDefault(b => b.Id == beatmapset.Id);
                beatmapset.Creator = respectiveApiBeatmapset?.Creator;
                beatmapset.UserId = respectiveApiBeatmapset?.UserId ?? 0;
                beatmapsetRepo.Update(beatmapset);
                if (!token.IsCancellationRequested) continue;
                try
                {
                    await beatmapsetRepo.SaveChangesAsync(token);
                }
                catch (NpgsqlException ex)
                {
                    logger.Log(LogLevel.Error, ex, "Method: IBeatmapsetRepository.SaveChangesAsync; Beatmapsets: {@beatmapsets}", beatmapsets);
                }
            }

            try
            {
                await beatmapsetRepo.SaveChangesAsync(token);
            }
            catch (NpgsqlException ex)
            {
                logger.Log(LogLevel.Error, ex, "Method: IBeatmapsetRepository.SaveChangesAsync; Beatmapsets: {@beatmapsets}", beatmapsets);
            }
        }
    }
    
    private async Task AddMissingHealthAttributesAsync(CancellationToken token)
    {
        var beatmaps = await beatmapRepo.GetAll().Where(b => b.Health == null).ToListAsync(token);
        if (beatmaps.Count > 0)
        {
            logger.Log(LogLevel.Information, "Adding missing health attributes. Beatmap count: {count}", beatmaps.Count);
            var apiBeatmaps = await apiFetcher.GetBeatmapsAsync(beatmaps.Select(b => b.Id).ToList(), token);
            foreach (var beatmap in beatmaps)
            {
                var respectiveApiBeatmap = apiBeatmaps.FirstOrDefault(b => b.Id == beatmap.Id);
                beatmap.Health = respectiveApiBeatmap?.Health ?? 0;
                beatmap.DrainLength = respectiveApiBeatmap?.DrainLength ?? 0;
                beatmapRepo.Update(beatmap);
                if (!token.IsCancellationRequested) continue;
                try
                {
                    await beatmapRepo.SaveChangesAsync(token);
                }
                catch (NpgsqlException ex)
                {
                    logger.Log(LogLevel.Error, ex, "Method: IBeatmapRepository.SaveChangesAsync; Beatmaps: {@beatmaps}", beatmaps);
                }
            }
            try
            {
                await beatmapRepo.SaveChangesAsync(token);
            }
            catch (NpgsqlException ex)
            {
                logger.Log(LogLevel.Error, ex, "Method: IBeatmapRepository.SaveChangesAsync; Beatmaps: {@beatmaps}", beatmaps);
            }
        }
    }
}