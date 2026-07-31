using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OsuScoreStats.DbService.Repositories.Interfaces;
using OsuScoreStats.ViewModels;

namespace OsuScoreStats.Controllers;

public class HomeController(IScoreRepository scoreRepository) : Controller
{
    public async Task<IActionResult> Index(int id, CancellationToken cancellationToken = default)
    {
        var scoreQuery = scoreRepository.GetAllWithBeatmapAndUserData().Where(s => s.Date >= DateTime.Today.ToUniversalTime());
        var scoresCount =  await scoreQuery.CountAsync(cancellationToken);
        var scores = await scoreQuery
            .OrderByDescending(s => s.Date)
            .Take(25)
            .ToListAsync(cancellationToken);
        var pages = (int)Math.Ceiling(scoresCount / 25d);

        var viewModel = new IndexViewModel
        {
            Scores = scores,
            Pages = pages
        };
        
        return View(viewModel);
    }
}