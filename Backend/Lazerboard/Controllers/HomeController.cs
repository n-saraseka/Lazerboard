using Microsoft.AspNetCore.Mvc;

namespace Lazerboard.Controllers;

public class GeneralController : Controller
{
    public IActionResult About()
    {
        return View();
    }
}