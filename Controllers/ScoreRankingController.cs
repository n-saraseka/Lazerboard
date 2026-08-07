using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OsuScoreStats.Api.Dtos;
using OsuScoreStats.DbService.Repositories.Interfaces;
using OsuScoreStats.OsuApi.Enums;
using OsuScoreStats.ViewModels;

namespace OsuScoreStats.Controllers;

public class ScoreRankingController(IScoreRepository scoreRepository, ICountryRepository countryRepository) : Controller
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
        
        var query = scoreRepository.GetAllWithBeatmapAndUserData()
            .Where(s => s.Mode == Mode.Mania && s.TotalScore == 1000000);

        var group = query
            .GroupBy(s => s.User)
            .Select(g => new UserRanking 
            {
                User = g.Key,
                ScoresCount = g.Count() 
            })
            .OrderByDescending(s => s.ScoresCount);

        var count = await group.CountAsync(cancellationToken);

        var result = await group
            .Take(10)
            .ToListAsync(cancellationToken);
        result = result.OrderByDescending(r => r.ScoresCount).ToList();
        foreach (var userRanking in result)
        {
            userRanking.Rank = result.IndexOf(userRanking) + 1;
        }

        var viewModel = new ScoreRankingViewModel
        {
            Countries = countries,
            UserRanking = new UserRankingResponse
            {
                Count = count,
                UserRankings = result
            }
        };
        
        return View(viewModel);
    }
}