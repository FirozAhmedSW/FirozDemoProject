using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Models.Permission;
using TaskManagementSystem.DataContext;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;
using TaskManagementSystem.Services;

namespace TaskManagementSystem.Controllers
{
    public class MenuController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ActivityLogger _activityLogger;
        private readonly int PageSize = 10; // Pagination page size

        public MenuController(ApplicationDbContext context, ActivityLogger activityLogger)
        {
            _context = context;
            _activityLogger = activityLogger;
        }

        // GET: Menu
        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            var userName = HttpContext.Session.GetString("UserName") ?? "Unknown";

            if (string.IsNullOrWhiteSpace(search))
            {
                await _activityLogger.LogAsync(userName, "View Menu", $"User '{userName}' viewed the menu list.");
            }
            else
            {
                await _activityLogger.LogAsync(userName, "Search Menu", $"User '{userName}' searched for '{search}'.");
            }

            var query = _context.Menus.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(m =>
                    (m.Title ?? "").Contains(search) ||
                    (m.ControllerName ?? "").Contains(search) ||
                    (m.ActionName ?? "").Contains(search)
                );
            }
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

            var menus = await query
                .OrderBy(m => m.Id)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.MenuParents = 1;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.Search = search;

            return View(menus);
        }
        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id, bool isActive)
        {
            var menu = await _context.Menus.FindAsync(id);
            if (menu == null) return NotFound();

            menu.IsActive = isActive;
            menu.UpdatedAt = DateTime.Now;

            _context.Menus.Update(menu);
            await _context.SaveChangesAsync();

            // ✅ ইউজারনেম Session থেকে নেওয়া
            var userName = HttpContext.Session.GetString("UserName") ?? "Unknown";

            // ✅ Active / Inactive অনুযায়ী Log message তৈরি
            string actionType = isActive ? "Activate Menu" : "Deactivate Menu";
            string logMessage = $"User '{userName}' {(isActive ? "activated" : "deactivated")} menu: '{menu.Title}' (ID: {menu.Id}).";

            await _activityLogger.LogAsync(userName, actionType, logMessage);

            return Ok(new { success = true });
        }


        // GET: Menu/Create
        public IActionResult Create()
        {
            LoadParentMenus();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Menu menu)
        {
            if (ModelState.IsValid)
            {
                menu.CreatedAt = DateTime.Now;
                menu.IsActive = true;
                menu.IsDeleted = false;
                _context.Menus.Add(menu);
                await _context.SaveChangesAsync();

                var userName = HttpContext.Session.GetString("UserName") ?? "Unknown";
                string actionType = "Create Menu";
                string logMessage = $"User '{userName}' created a new menu: '{menu.Title}' (ID: {menu.Id}).";
                await _activityLogger.LogAsync(userName, actionType, logMessage);

                return RedirectToAction(nameof(Index));
            }

            LoadParentMenus();
            return View(menu);
        }


        // GET: Menu/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var menu = await _context.Menus.FindAsync(id);
            if (menu == null) return NotFound();

            LoadParentMenus(menu.Id);
            return View(menu);
        }

        // POST: Menu/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Menu menu)
        {
            if (id != menu.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    menu.UpdatedAt = DateTime.Now;

                    _context.Menus.Update(menu);
                    await _context.SaveChangesAsync();
                    var userName = HttpContext.Session.GetString("UserName") ?? "Unknown";
                    string actionType = "Edit Menu";
                    string logMessage = $"User '{userName}' edited menu: '{menu.Title}' (ID: {menu.Id}).";
                    await _activityLogger.LogAsync(userName, actionType, logMessage);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MenuExists(menu.Id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            LoadParentMenus(menu.Id);
            return View(menu);
        }

        // GET: Menu/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var menu = await _context.Menus.FindAsync(id);
            if (menu == null)
                return NotFound();

            return View(menu);
        }

        // POST: Menu/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var menu = await _context.Menus.FindAsync(id);
            if (menu != null)
            {
                _context.Menus.Remove(menu);
                await _context.SaveChangesAsync();
                var userName = HttpContext.Session.GetString("UserName") ?? "Unknown";
                string actionType = "Delete Menu";
                string logMessage = $"User '{userName}' deleted menu: '{menu.Title}' (ID: {menu.Id}).";
                await _activityLogger.LogAsync(userName, actionType, logMessage);
            }

            return RedirectToAction(nameof(Index));
        }


        // ====================
        // Helper Methods
        // ====================
        private void LoadParentMenus(int? excludeId = null)
        {
            var parents = _context.Menus
                .Where(m => m.ParentId == null);

            if (excludeId.HasValue)
                parents = parents.Where(m => m.Id != excludeId.Value);

            ViewBag.Parents = parents.ToList();
        }

        private bool MenuExists(int id)
        {
            return _context.Menus.Any(e => e.Id == id);
        }
    }
}
