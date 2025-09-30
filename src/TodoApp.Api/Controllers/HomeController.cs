using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TodoApp.Api.Models;

namespace TodoApp.Api.Controllers;

/// <summary>
/// Provides MVC views for static and marketing-facing pages of the Todo application.
/// </summary>
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    /// <summary>
    /// Creates a new <see cref="HomeController"/> instance that logs diagnostic events.
    /// </summary>
    /// <param name="logger">Logger used for emitting view diagnostics.</param>
    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Displays the landing page.
    /// </summary>
    /// <returns>Landing page view.</returns>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Displays the privacy policy content.
    /// </summary>
    /// <returns>Privacy policy view.</returns>
    public IActionResult Privacy()
    {
        return View();
    }

    /// <summary>
    /// Renders a problem details page with the active request identifier.
    /// </summary>
    /// <returns>Error view containing request diagnostics.</returns>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
