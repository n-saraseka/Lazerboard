using Microsoft.AspNetCore.Mvc;
using OsuScoreStats.DbService.Repositories.Interfaces;
using OsuScoreStats.OsuApi.Enums;
using OsuScoreStats.ViewModels;

namespace OsuScoreStats.Controllers;

public class BeatmapsetController(IBeatmapRepository beatmapRepository, IScoreRepository scoreRepository) : Controller
{
    public async Task<IActionResult> BeatmapsetPage(int id, [FromQuery] Mode? mode, CancellationToken cancellationToken = default)
    {
        var beatmaps = await beatmapRepository.GetByBeatmapsetIdAsync(id, cancellationToken);
        if (beatmaps.Count == 0) return NotFound();
        
        var firstBeatmap = beatmaps.First();
        var beatmapset = firstBeatmap.Beatmapset;
        
        var selectedMode = mode ?? firstBeatmap.Mode;
        
        var scores = await scoreRepository.GetByBeatmapIdWithUserDataAsync(firstBeatmap.Id, selectedMode, cancellationToken);

        var viewModel = new BeatmapsetViewModel
        {
            Beatmapset = beatmapset,
            Beatmaps = beatmaps,
            SelectedBeatmapId = firstBeatmap.Id,
            Scores = scores,
            SelectedMode = selectedMode
        };
        
        return View(viewModel);
    }
    
    public async Task<IActionResult> BeatmapPage(int id, [FromQuery] Mode? mode, CancellationToken cancellationToken = default)
    {
        var beatmap = await beatmapRepository.GetByIdAsync(id, cancellationToken);
        if (beatmap == null) return NotFound();
        
        var beatmaps = await beatmapRepository.GetByBeatmapsetIdAsync(beatmap.BeatmapsetId, cancellationToken);
        if (beatmaps.Count == 0) return NotFound();
        
        var respectiveBeatmap = beatmaps.First(b => b.Id == id);
        
        var firstBeatmap = beatmaps.First();
        var beatmapset = firstBeatmap.Beatmapset;
        
        var selectedMode = mode ?? firstBeatmap.Mode;
        
        var scores = await scoreRepository.GetByBeatmapIdWithUserDataAsync(id, selectedMode, cancellationToken);

        var viewModel = new BeatmapsetViewModel
        {
            Beatmapset = beatmapset,
            Beatmaps = beatmaps,
            SelectedBeatmapId = id,
            Scores = scores,
            SelectedMode = selectedMode
        };
        
        return View(viewModel);
    }
}