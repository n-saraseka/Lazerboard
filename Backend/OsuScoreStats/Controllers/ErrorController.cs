using Microsoft.AspNetCore.Mvc;

namespace OsuScoreStats.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class ErrorController : Controller
{
    /// <summary>
    /// The placeholder page to return if received HTTP response status code 500
    /// </summary>
    /// <returns>The appropriate view</returns>
    [Route("500")]
    public IActionResult ApplicationError()
    {
        return View();
    }
    
    /// <summary>
    /// The placeholder page to return if received HTTP response status code 404
    /// </summary>
    /// <returns>The appropriate view</returns>
    [Route("404")]
    public IActionResult PageNotFound()
    {
        return View();
    }
}