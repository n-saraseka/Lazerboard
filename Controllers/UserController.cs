using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OsuScoreStats.DbService;
using OsuScoreStats.DbService.Repositories;
using OsuScoreStats.ViewModels;

namespace OsuScoreStats.Controllers;

public class UserController(IDbContextFactory<ScoreDataContext> dbContextFactory) : Controller
{
    public async Task<IActionResult> UserPage(int id, CancellationToken cancellationToken = default)
    {
        var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var userRepository = new UserRepository(dbContext);

        var user = await userRepository.GetByIdAsync(id, cancellationToken);
        
        if (user == null)
            return NotFound();

        var viewModel = new UserViewModel
        {
            Id = user.Id,
            Username = user.Username,
            CountryCode = user.CountryCode
        };
        
        return View(viewModel);
    }
}