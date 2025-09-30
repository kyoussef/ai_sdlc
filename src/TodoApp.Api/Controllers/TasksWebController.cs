using Microsoft.AspNetCore.Mvc;

namespace TodoApp.Api.Controllers;

/// <summary>
/// Serves the Razor UI shell that progressively enhances the task list experience.
/// </summary>
public class TasksWebController : Controller
{
    /// <summary>
    /// Returns the tasks index view, which bootstraps the JavaScript-driven client experience.
    /// </summary>
    /// <returns>The Razor view that hosts the task management UI.</returns>
    [HttpGet("/tasks")]
    public IActionResult Index()
    {
        return View("~/Views/Tasks/Index.cshtml");
    }
}
