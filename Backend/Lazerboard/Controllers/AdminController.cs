using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lazerboard.Controllers;

public class AdminController : Controller
{
    [Authorize(Roles = "Administrator")]
    public IActionResult AdminPage()
    {
        return View();
    }
}