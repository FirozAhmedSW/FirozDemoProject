using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TaskManagementSystem.DataContext;
using TaskManagementSystem.Models;

namespace TaskManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // ✅ Session থেকে username নেওয়া
            var name = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = name;

            // ✅ Database থেকে শেষ ৫টি Activity Log নেওয়া
            var recentLogs = await _context.ActivityLogs
                .OrderByDescending(x => x.CreatedAt)
                .Take(5)
                .ToListAsync();

            // ✅ View এ পাঠানো
            ViewBag.RecentLogs = recentLogs;

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
}
