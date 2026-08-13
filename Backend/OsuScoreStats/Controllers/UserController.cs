using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OsuScoreStats.DbService.Repositories.Interfaces;
using OsuScoreStats.ViewModels;

namespace OsuScoreStats.Controllers;

public class UserController(IUserRepository userRepository, IScoreRepository scoreRepository, IConfiguration config) : Controller
{
    public async Task<IActionResult> UserPage(int id, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdWithCountryAsync(id, cancellationToken);
        
        if (user == null)
            return NotFound();

        var scoreQuery = scoreRepository.GetAllWithBeatmapAndUserData().Where(s => s.UserId == user.Id);
        var scoresCount = await scoreQuery.CountAsync(cancellationToken);
        var scores = await scoreQuery
            .OrderByDescending(s => s.Date)
            .Take(25)
            .ToListAsync(cancellationToken);
        var pages = (int)Math.Ceiling(scoresCount / 25d);

        var viewModel = new UserViewModel
        {
            User = user,
            Scores = scores,
            Count = scoresCount,
            Pages = pages
        };
        
        ViewBag.FrontendUrl = config["FrontendUrl"];
        
        return View(viewModel);
    }
}