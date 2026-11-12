using Lazerboard.Data.Database.Repositories.Interfaces;
using Lazerboard.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lazerboard.Controllers;

public class ScorePageController(ICountryRepository countryRepository) : Controller
{
    public async Task<IActionResult> GlobalScores(CancellationToken cancellationToken = default)
    {
        var countries = await countryRepository.GetAll().OrderBy(c => c.Name).ToListAsync(cancellationToken);

        var viewModel = new ScoresViewModel
        {
            Countries = countries
        };
        
        return View(viewModel);
    }
    
    public async Task<IActionResult> MillionScores(CancellationToken cancellationToken = default)
    {
        var countries = await countryRepository.GetAll().OrderBy(c => c.Name).ToListAsync(cancellationToken);

        var viewModel = new ScoresViewModel
        {
            Countries = countries
        };
        
        return View(viewModel);
    }
}