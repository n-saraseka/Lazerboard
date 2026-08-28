using Lazerboard.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Lazerboard.Data.Database.Repositories.Interfaces;
using Lazerboard.Data.OsuEntities.Enums;

namespace Lazerboard.Controllers;

public class BeatmapsetController(IBeatmapRepository beatmapRepository) : Controller
{
    public async Task<IActionResult> BeatmapsetPage(int id, [FromQuery] Mode? mode, CancellationToken cancellationToken = default)
    {
        var beatmaps = await beatmapRepository.GetByBeatmapsetIdAsync(id, cancellationToken);
        if (beatmaps.Count == 0) return NotFound();
        
        var firstBeatmap = beatmaps.First();
        var beatmapset = firstBeatmap.Beatmapset;
        
        var selectedMode = mode ?? firstBeatmap.Mode;

        var viewModel = new BeatmapsetViewModel
        {
            Beatmapset = beatmapset,
            Beatmaps = beatmaps,
            SelectedBeatmapId = firstBeatmap.Id,
            SelectedMode = selectedMode,
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
        
        var beatmapset = respectiveBeatmap.Beatmapset;

        var selectedMode = respectiveBeatmap.Mode != Mode.Osu
            ? respectiveBeatmap.Mode
            : mode ?? respectiveBeatmap.Mode;

        var viewModel = new BeatmapsetViewModel
        {
            Beatmapset = beatmapset,
            Beatmaps = beatmaps,
            SelectedBeatmapId = id,
            SelectedMode = selectedMode,
        };
        
        return View(viewModel);
    }
}