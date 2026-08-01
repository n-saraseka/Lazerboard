using Microsoft.EntityFrameworkCore;
using OsuScoreStats.DbService.Repositories.Interfaces;
using OsuScoreStats.ScoreFetcher;

namespace OsuScoreStats.Migrations;

public class Backpopulator(IBeatmapRepository beatmapRepo, IApiFetcher apiFetcher): IBackpopulator
{
    public async Task BackpopulateAsync(CancellationToken token)
    {
        await AddMissingHealthAttributesAsync(token);
    }
    
    private async Task AddMissingHealthAttributesAsync(CancellationToken token)
    {
        var beatmaps = await beatmapRepo.GetAll().Where(b => b.Health == null).ToListAsync(token);
        if (beatmaps.Count > 0)
        {
            Console.WriteLine("Adding missing health attributes");
            var apiBeatmaps = await apiFetcher.GetBeatmapsAsync(beatmaps.Select(b => b.Id), token);
            foreach (var beatmap in beatmaps)
            {
                var respectiveApiBeatmap = apiBeatmaps.FirstOrDefault(b => b.Id == beatmap.Id);
                beatmap.Health = respectiveApiBeatmap?.Health ?? 0;
                beatmap.DrainLength = respectiveApiBeatmap?.DrainLength ?? 0;
                beatmapRepo.Update(beatmap);
                if (token.IsCancellationRequested)
                {
                    await beatmapRepo.SaveChangesAsync(token);
                }
            }
            await beatmapRepo.SaveChangesAsync(token);
        }
    }
}