using Microsoft.AspNetCore.Mvc;
using OsuScoreStats.DbService.Repositories.Interfaces;
using OsuScoreStats.ViewModels;

namespace OsuScoreStats.Controllers;

public class BeatmapsetController(IBeatmapRepository beatmapRepository) : Controller
{
    public async Task<IActionResult> BeatmapsetPage(int id, int? selectedBeatmapId, CancellationToken cancellationToken = default)
    {
        var beatmaps = await beatmapRepository.GetByBeatmapsetIdAsync(id, cancellationToken);
        if (beatmaps.Count == 0) return NotFound();
        
        var firstBeatmap = beatmaps.First();
        var beatmapset = firstBeatmap.Beatmapset;
        
        var beatmapId = selectedBeatmapId ?? selectedBeatmapId ?? firstBeatmap.Id;

        var viewModel = new BeatmapsetViewModel
        {
            Beatmapset = beatmapset,
            Beatmaps = beatmaps,
            SelectedBeatmapId = beatmapId
        };
        
        return View(viewModel);
    }
}