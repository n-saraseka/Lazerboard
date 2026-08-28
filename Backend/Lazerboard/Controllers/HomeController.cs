using Lazerboard.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lazerboard.Data.Database.Repositories.Interfaces;

namespace Lazerboard.Controllers;

public class GeneralController(ICountryRepository countryRepository) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        var countries = await countryRepository.GetAll().OrderBy(c => c.Name).ToListAsync(cancellationToken);

        var viewModel = new IndexViewModel
        {
            Countries = countries
        };
        
        return View(viewModel);
    }

    public IActionResult About()
    {
        return View();
    }
}