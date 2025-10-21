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

        // ✅ Index / Main Report Page with Filters + Pagination
        [HttpGet]
        public IActionResult Index(string? month, DateTime? from, DateTime? to, int page = 1, int pageSize = 10)
        {
            var expenses = _context.Expenses.AsQueryable();

            // 🔹 Filter by From/To date (priority)
            if (from.HasValue || to.HasValue)
            {
                if (from.HasValue)
                    expenses = expenses.Where(x => x.Date.HasValue && x.Date.Value >= from.Value);
                if (to.HasValue)
                    expenses = expenses.Where(x => x.Date.HasValue && x.Date.Value <= to.Value);

                ViewBag.FromDate = from?.ToString("yyyy-MM-dd");
                ViewBag.ToDate = to?.ToString("yyyy-MM-dd");
                ViewBag.SelectedMonth = null;
            }
            // 🔹 Filter by month if no date range
            else if (!string.IsNullOrEmpty(month))
            {
                if (DateTime.TryParse($"{month}-01", out var selectedMonth))
                {
                    var monthStart = new DateTime(selectedMonth.Year, selectedMonth.Month, 1);
                    var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                    expenses = expenses.Where(x => x.Date.HasValue && x.Date.Value >= monthStart && x.Date.Value <= monthEnd);
                    ViewBag.SelectedMonth = month;
                }
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
                ViewBag.FromDate = null;
                ViewBag.ToDate = null;
            }

            // 🔹 Pagination
            var totalRecords = expenses.Count();
            var totalPages = Math.Max(1, (int)Math.Ceiling((double)totalRecords / pageSize)); // Always at least 1

            page = Math.Max(1, Math.Min(page, totalPages)); // Ensure page in range

            var expensesList = expenses
                .OrderByDescending(x => x.Date)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.TotalAmount = expenses.Any() ? expenses.Sum(x => x.Amount) : 0;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalRecords = totalRecords;
            ViewBag.TotalPages = totalPages;

            return View(expensesList);
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

                if (!model.Date.HasValue)
                    model.Date = DateTime.Now;

                model.CreatedByUserId = userId;
                model.CreatedByUserName = userName;

                _context.Expenses.Add(model);
                _context.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(model);
        }

        // ✅ GET: Edit
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var expense = _context.Expenses.Find(id);
            if (expense == null) return NotFound();
            return View(expense);
        }

        // ✅ POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Expense model)
        {
            if (ModelState.IsValid)
            {
                var expense = _context.Expenses.Find(model.Id);
                if (expense == null) return NotFound();

                expense.Description = model.Description;
                expense.Amount = model.Amount;
                expense.Date = model.Date ?? DateTime.Now;

                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        // ✅ POST: Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var expense = _context.Expenses.Find(id);
            if (expense == null) return NotFound();

            _context.Expenses.Remove(expense);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
