using Lazerboard.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lazerboard.Data.Database.Repositories.Interfaces;

namespace Lazerboard.Controllers;

public class GeneralController(IScoreRepository scoreRepository, ICountryRepository countryRepository) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var scoreQuery = scoreRepository.GetAllWithBeatmapAndUserData().Where(s => s.Date >= today);
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

    public IActionResult About()
    {
        return View();
    }
}