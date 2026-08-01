using Microsoft.AspNetCore.Mvc;
using OsuScoreStats.DbService.Repositories.Interfaces;
using OsuScoreStats.ViewModels;

namespace OsuScoreStats.Controllers;

public class BeatmapsetController(IBeatmapRepository beatmapRepository, IScoreRepository scoreRepository) : Controller
{
    public async Task<IActionResult> BeatmapsetPage(int id, CancellationToken cancellationToken = default)
    {
        var beatmaps = await beatmapRepository.GetByBeatmapsetIdAsync(id, cancellationToken);
        if (beatmaps.Count == 0) return NotFound();
        
        var firstBeatmap = beatmaps.First();
        var beatmapset = firstBeatmap.Beatmapset;
        
        var scores = await scoreRepository.GetByBeatmapIdAsync(firstBeatmap.Id, cancellationToken);
        scores = scores.OrderBy(s => s.Rank).ToList();

        var viewModel = new BeatmapsetViewModel
        {
            Beatmapset = beatmapset,
            Beatmaps = beatmaps,
            SelectedBeatmapId = firstBeatmap.Id,
            Scores = scores
        };
        
        return View(viewModel);
    }
    
    public async Task<IActionResult> BeatmapPage(int id, CancellationToken cancellationToken = default)
    {
        var beatmap = await beatmapRepository.GetByIdAsync(id, cancellationToken);
        if (beatmap == null) return NotFound();
        
        var beatmaps = await beatmapRepository.GetByBeatmapsetIdAsync(beatmap.BeatmapsetId, cancellationToken);
        if (beatmaps.Count == 0) return NotFound();
        
        var firstBeatmap = beatmaps.First();
        var beatmapset = firstBeatmap.Beatmapset;
        
        var scores = await scoreRepository.GetByBeatmapIdAsync(id, cancellationToken);
        scores = scores.OrderBy(s => s.Rank).ToList();

        var viewModel = new BeatmapsetViewModel
        {
            Beatmapset = beatmapset,
            Beatmaps = beatmaps,
            SelectedBeatmapId = id,
            Scores = scores
        };
        
        return View(viewModel);
    }
}