using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OsuScoreStats.DbService.Repositories.Interfaces;
using OsuScoreStats.OsuApi.Enums;
using OsuScoreStats.ViewModels;

namespace OsuScoreStats.Controllers;

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
}