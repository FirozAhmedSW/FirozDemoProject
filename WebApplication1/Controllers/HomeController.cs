using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TaskManagementSystem.Models;

namespace TaskManagementSystem.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        // ✅ Session এ Data Set করা
       // HttpContext.Session.SetString("UserName", "Firoz");

        // ✅ Session থেকে Data নেওয়া
        var name = HttpContext.Session.GetString("UserName");

        // ✅ View কে পাঠানো
        ViewBag.UserName = name;

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
