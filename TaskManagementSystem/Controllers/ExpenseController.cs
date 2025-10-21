using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        // ✅ Index / Main Report Page
        [HttpGet]
        public IActionResult Index(string? month, DateTime? from, DateTime? to)
        {
            var expenses = _context.Expenses.AsQueryable();

            if (!string.IsNullOrEmpty(month))
            {
                if (DateTime.TryParse($"{month}-01", out var selectedMonth))
                {
                    var monthStart = new DateTime(selectedMonth.Year, selectedMonth.Month, 1);
                    var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                    expenses = expenses.Where(x => x.Date.HasValue && x.Date.Value >= monthStart && x.Date.Value <= monthEnd);
                    ViewBag.SelectedMonth = month;
                }
            }
            else if (from.HasValue || to.HasValue)
            {
                if (from.HasValue)
                    expenses = expenses.Where(x => x.Date.HasValue && x.Date.Value >= from.Value);
                if (to.HasValue)
                    expenses = expenses.Where(x => x.Date.HasValue && x.Date.Value <= to.Value);

                ViewBag.FromDate = from?.ToString("yyyy-MM-dd");
                ViewBag.ToDate = to?.ToString("yyyy-MM-dd");
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

            return View(expenses.OrderByDescending(x => x.Date).ToList());
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

                model.Date ??= DateTime.Now;
                model.CreatedByUserId = userId;
                model.CreatedByUserName = userName;

                _context.Expenses.Add(model);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        // ✅ GET: Edit Expense
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var expense = _context.Expenses.Find(id);
            if (expense == null) return NotFound();
            return View(expense);
        }

        // ✅ POST: Edit Expense
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Expense model)
        {
            if (id != model.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                var expense = _context.Expenses.Find(id);
                if (expense == null) return NotFound();

                expense.Date = model.Date ?? DateTime.Now;
                expense.Description = model.Description;
                expense.Amount = model.Amount;

                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        // ✅ POST: Delete Expense
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
