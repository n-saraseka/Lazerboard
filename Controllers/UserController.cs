using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OsuScoreStats.DbService;
using OsuScoreStats.DbService.Repositories;
using OsuScoreStats.DbService.Repositories.Interfaces;
using OsuScoreStats.ViewModels;

namespace OsuScoreStats.Controllers;

public class UserController(IUserRepository userRepository) : Controller
{
    public async Task<IActionResult> UserPage(int id, CancellationToken cancellationToken = default)
    {
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