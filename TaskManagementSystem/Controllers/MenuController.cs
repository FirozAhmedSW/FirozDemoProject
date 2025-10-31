using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Models;
using TaskManagementSystem.DataContext;
using TaskManagementSystem.Models.Permission;

namespace TaskManagementSystem.Controllers
{
    public class MenuController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MenuController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Menu
        public async Task<IActionResult> Index()
        {
            var menus = await _context.Menus
                .ToListAsync();

            // Optionally, load parent title for display
            var menuList = menus.Select(m => new
            {
                m.Id,
                m.Title,
                ParentTitle = menus.FirstOrDefault(p => p.Id == m.ParentId)?.Title
            }).ToList();

            ViewBag.MenuParents = menuList;

            return View(menus);
        }

        // GET: Menu/Create
        public IActionResult Create()
        {
            LoadParentMenus();
            return View();
        }

        // POST: Menu/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Menu menu)
        {
            if (ModelState.IsValid)
            {
                menu.CreatedAt = DateTime.Now;
                menu.IsActive = true;
                menu.IsDeleted = false;
                menu.ParentId = 1;

                _context.Add(menu);
                await _context.SaveChangesAsync();
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
            if (id != menu.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    menu.UpdatedAt = DateTime.Now;
                    menu.IsActive = true;
                    menu.ParentId = 1;
                    _context.Update(menu);
                    await _context.SaveChangesAsync();
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
            if (id == null) return NotFound();

            var menu = await _context.Menus.FindAsync(id);
            if (menu == null) return NotFound();

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
            }
            return RedirectToAction(nameof(Index));
        }

        // Helper methods
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
