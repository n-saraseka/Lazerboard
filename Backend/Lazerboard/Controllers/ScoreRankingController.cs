using Lazerboard.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lazerboard.Data.Database.Repositories.Interfaces;

namespace Lazerboard.Controllers;

public class ScoreRankingController(ICountryRepository countryRepository) : Controller
{
    public async Task<IActionResult> ScoreRanking(CancellationToken cancellationToken = default)
    {
        var countries = await countryRepository.GetAll().OrderBy(c => c.Name).ToListAsync(cancellationToken);

        var viewModel = new ScoreRankingViewModel
        {
            Countries = countries
        };
        
        return View(viewModel);
    }
    
    public async Task<IActionResult> ManiaMillions(CancellationToken cancellationToken = default)
    {
        var countries = await countryRepository.GetAll().OrderBy(c => c.Name).ToListAsync(cancellationToken);

        var viewModel = new ScoreRankingViewModel
        {
            Countries = countries
        };
        
        return View(viewModel);
    }
}