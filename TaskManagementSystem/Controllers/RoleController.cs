using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.DataContext;
using TaskManagementSystem.Models.Permission;
using TaskManagementSystem.Services;

namespace TaskManagementSystem.Controllers
{
    public class RoleController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ActivityLogger _activityLogger;
        private const int PageSize = 10;

        public RoleController(ApplicationDbContext context, ActivityLogger activityLogger)
        {
            _context = context;
            _activityLogger = activityLogger;
        }

        // GET: Roles with search and pagination
        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            var userName = HttpContext.Session.GetString("UserName") ?? "Unknown";
            if (string.IsNullOrWhiteSpace(search))
            {
                await _activityLogger.LogAsync(
                    userName,
                    "View Role List",
                    $"User '{userName}' viewed the role list."
                );
            }
            else
            {
                await _activityLogger.LogAsync(
                    userName,
                    "Search Role",
                    $"User '{userName}' searched roles with keyword: '{search}'."
                );
            }
            var query = _context.Roles.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(r =>
                    (r.Name ?? "").Contains(search) ||
                    (r.Description ?? "").Contains(search)
                );
            }
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

            var roles = await query
                .OrderBy(r => r.Name)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.Search = search;

            return View(roles);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id, bool isActive)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
                return NotFound();

            role.IsActive = isActive;
            role.UpdatedAt = DateTime.Now;

            _context.Roles.Update(role);
            await _context.SaveChangesAsync();

            // ✅ ইউজারনেম Session থেকে নেওয়া
            var userName = HttpContext.Session.GetString("UserName") ?? "Unknown";

            // ✅ Action টাইপ ও বার্তা তৈরি
            string actionType = isActive ? "Activate Role" : "Deactivate Role";
            string logMessage = $"User '{userName}' {(isActive ? "activated" : "deactivated")} role: '{role.Name}' (ID: {role.Id}).";

            // ✅ Activity Log রেকর্ড
            await _activityLogger.LogAsync(userName, actionType, logMessage);

            return Ok(new { success = true });
        }


        // GET: Create Role
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Role role)
        {
            if (ModelState.IsValid)
            {
                role.CreatedAt = DateTime.Now;

                // যদি checkbox uncheck থাকে, IsActive default false হবে
                _context.Add(role);
                await _context.SaveChangesAsync();

                // ✅ ইউজারনেম Session থেকে নেওয়া
                var userName = HttpContext.Session.GetString("UserName") ?? "Unknown";

                // ✅ লগ বার্তা তৈরি
                string actionType = "Create Role";
                string logMessage = $"User '{userName}' created a new role: '{role.Name}' (ID: {role.Id}).";

                // ✅ Activity Log রেকর্ড
                await _activityLogger.LogAsync(userName, actionType, logMessage);

                return RedirectToAction(nameof(Index));
            }

            return View(role);
        }



        // GET: Edit Role
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var role = await _context.Roles.FindAsync(id);
            if (role == null) return NotFound();
            return View(role);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Role role)
        {
            if (id != role.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    role.UpdatedAt = DateTime.Now;

                    _context.Update(role);
                    await _context.SaveChangesAsync();

                    // ✅ ইউজারনেম Session থেকে নেওয়া
                    var userName = HttpContext.Session.GetString("UserName") ?? "Unknown";

                    // ✅ লগ বার্তা তৈরি
                    string actionType = "Edit Role";
                    string logMessage = $"User '{userName}' edited role: '{role.Name}' (ID: {role.Id}).";

                    // ✅ Activity Log রেকর্ড
                    await _activityLogger.LogAsync(userName, actionType, logMessage);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Roles.Any(r => r.Id == id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(role);
        }


        // GET: Delete Role
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var role = await _context.Roles.FindAsync(id);
            if (role == null) return NotFound();
            return View(role);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role != null)
            {
                _context.Roles.Remove(role);
                await _context.SaveChangesAsync();

                // ✅ ইউজারনেম Session থেকে নেওয়া
                var userName = HttpContext.Session.GetString("UserName") ?? "Unknown";

                // ✅ লগ বার্তা তৈরি
                string actionType = "Delete Role";
                string logMessage = $"User '{userName}' deleted role: '{role.Name}' (ID: {role.Id}).";

                // ✅ Activity Log রেকর্ড
                await _activityLogger.LogAsync(userName, actionType, logMessage);
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
