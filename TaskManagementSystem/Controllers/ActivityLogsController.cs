using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.DataContext;

namespace TaskManagementSystem.Controllers
{
    public class ActivityLogsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const int PageSize = 10; // প্রতি পেজে ১০টা ডেটা

        public ActivityLogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? user, DateTime? from, DateTime? to, int page = 1)
        {
            var query = _context.ActivityLogs.AsQueryable();

            // 🟢 User filter (Contains দিয়ে আংশিক ম্যাচ করবে)
            if (!string.IsNullOrEmpty(user))
            {
                query = query.Where(x => x.UserName != null && x.UserName.Contains(user));
            }

            // 🟢 Date range filter
            if (from.HasValue && to.HasValue)
            {
                // সময়টাও যেন ঠিকমতো কভার হয়, তাই to.Value.AddDays(1)
                var toDate = to.Value.AddDays(1);
                query = query.Where(x => x.CreatedAt >= from && x.CreatedAt < toDate);
            }

            // 🔢 Pagination
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

            var logs = await query
                .OrderByDescending(l => l.CreatedAt)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            // ViewData পাঠানো
            ViewData["CurrentUser"] = user ?? "";
            ViewData["From"] = from?.ToString("yyyy-MM-dd");
            ViewData["To"] = to?.ToString("yyyy-MM-dd");
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;

            return View(logs);
        }
    }
}
