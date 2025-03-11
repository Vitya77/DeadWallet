using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using DeadWaller.Models;

namespace DeadWaller.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        // Example using of logger
        _logger.LogInformation("This is an information log message.");
        _logger.LogWarning("This is a warning log message.");
        _logger.LogError("This is an error log message.");
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
