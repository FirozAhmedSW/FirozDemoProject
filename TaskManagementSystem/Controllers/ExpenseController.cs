using Microsoft.AspNetCore.Mvc;
using TaskManagementSystem.DataContext;
using TaskManagementSystem.Models;

namespace TaskManagementSystem.Controllers
{
    public class ExpenseController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExpenseController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ Index (Main Report Page)
        [HttpGet]
        public IActionResult Index(string? month, DateTime? from, DateTime? to)
        {
            var expenses = _context.Expenses.AsQueryable();

            if (!string.IsNullOrEmpty(month))
            {
                // Month format: "yyyy-MM"
                var selectedMonth = DateTime.Parse($"{month}-01");
                var monthStart = new DateTime(selectedMonth.Year, selectedMonth.Month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                expenses = expenses.Where(x => x.Date >= monthStart && x.Date <= monthEnd);

                ViewBag.SelectedMonth = month;
            }
            else if (from.HasValue && to.HasValue)
            {
                expenses = expenses.Where(x => x.Date >= from.Value && x.Date <= to.Value);
                ViewBag.FromDate = from.Value.ToString("yyyy-MM-dd");
                ViewBag.ToDate = to.Value.ToString("yyyy-MM-dd");
            }
            else
            {
                // Default current month report
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;
                expenses = expenses.Where(x => x.Date.Month == currentMonth && x.Date.Year == currentYear);
                ViewBag.SelectedMonth = DateTime.Now.ToString("yyyy-MM");
            }

            ViewBag.TotalAmount = expenses.Sum(x => x.Amount);
            return View(expenses.ToList());
        }

        // ✅ Create form
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Expense model)
        {
            if (ModelState.IsValid)
            {
                var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
                var userName = HttpContext.Session.GetString("UserName") ?? "Unknown";

                // যদি user তারিখ না দেয় (default 0001-01-01 থাকে), তাহলে current date সেট করে দেবে
                if (model.Date == default(DateTime) || model.Date == DateTime.MinValue)
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
