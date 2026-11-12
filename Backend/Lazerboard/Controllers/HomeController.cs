using Lazerboard.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lazerboard.Data.Database.Repositories.Interfaces;

namespace Lazerboard.Controllers;

public class GeneralController(ICountryRepository countryRepository) : Controller
{
    public IActionResult About()
    {
        return View();
    }
}