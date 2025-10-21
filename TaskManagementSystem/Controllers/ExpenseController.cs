using Microsoft.AspNetCore.Mvc;
using TaskManagementSystem.DataContext;
using TaskManagementSystem.Models;
using System.Linq;

namespace TaskManagementSystem.Controllers
{
    public class ExpenseController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExpenseController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ Index / Main Report Page
        [HttpGet]
        public IActionResult Index(string? month, DateTime? from, DateTime? to)
        {
            var expenses = _context.Expenses.AsQueryable();

            if (!string.IsNullOrEmpty(month))
            {
                // Parse month in "yyyy-MM" format
                if (DateTime.TryParse($"{month}-01", out var selectedMonth))
                {
                    var monthStart = new DateTime(selectedMonth.Year, selectedMonth.Month, 1);
                    var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                    expenses = expenses.Where(x => x.Date.HasValue && x.Date.Value >= monthStart && x.Date.Value <= monthEnd);
                    ViewBag.SelectedMonth = month;
                }
            }
            else if (from.HasValue && to.HasValue)
            {
                expenses = expenses.Where(x => x.Date.HasValue && x.Date.Value >= from.Value && x.Date.Value <= to.Value);
                ViewBag.FromDate = from.Value.ToString("yyyy-MM-dd");
                ViewBag.ToDate = to.Value.ToString("yyyy-MM-dd");
            }
            else
            {
                // Default: current month
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;
                expenses = expenses.Where(x => x.Date.HasValue &&
                                               x.Date.Value.Month == currentMonth &&
                                               x.Date.Value.Year == currentYear);
                ViewBag.SelectedMonth = DateTime.Now.ToString("yyyy-MM");
            }

            ViewBag.TotalAmount = expenses.Any() ? expenses.Sum(x => x.Amount) : 0;
            return View(expenses.ToList());
        }

        // ✅ GET: Create Form
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // ✅ POST: Create Expense
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Expense model)
        {
            if (ModelState.IsValid)
            {
                var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
                var userName = HttpContext.Session.GetString("UserName") ?? "Unknown";

                // Assign current date if null
                if (!model.Date.HasValue)
                {
                    model.Date = DateTime.Now;
                }

                model.CreatedByUserId = userId;
                model.CreatedByUserName = userName;

                _context.Expenses.Add(model);
                _context.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(model);
        }
    }
}
