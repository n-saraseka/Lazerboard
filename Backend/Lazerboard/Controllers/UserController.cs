using Lazerboard.ViewModels;
using Microsoft.AspNetCore.Mvc; 
using Lazerboard.Data.Database.Repositories.Interfaces;

namespace Lazerboard.Controllers;

public class UserController(IUserRepository userRepository) : Controller
{
    public async Task<IActionResult> UserPage(int id, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdWithCountryAsync(id, cancellationToken);
        
        if (user == null)
            return NotFound();

        var viewModel = new UserViewModel
        {
            User = user,
        };
        
        return View(viewModel);
    }
}