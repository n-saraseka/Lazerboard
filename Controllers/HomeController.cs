using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OsuScoreStats.DbService.Repositories.Interfaces;
using OsuScoreStats.ViewModels;

namespace OsuScoreStats.Controllers;

public class HomeController(IScoreRepository scoreRepository, ICountryRepository countryRepository) : Controller
{
    public async Task<IActionResult> Index(int id, CancellationToken cancellationToken = default)
    {
        var scoreQuery = scoreRepository.GetAllWithBeatmapAndUserData().Where(s => DateOnly.FromDateTime(s.Date) >= DateOnly.FromDateTime(DateTime.Today));
        var scoresCount =  await scoreQuery.CountAsync(cancellationToken);
        var scores = await scoreQuery
            .OrderByDescending(s => s.PP)
            .Take(25)
            .ToListAsync(cancellationToken);
        var pages = (int)Math.Ceiling(scoresCount / 25d);
        var countries = await countryRepository.GetAll().OrderBy(c => c.Name).ToListAsync(cancellationToken);

        var viewModel = new IndexViewModel
        {
            Scores = scores,
            Pages = pages,
            Countries = countries
        };
        
        return View(viewModel);
    }
}