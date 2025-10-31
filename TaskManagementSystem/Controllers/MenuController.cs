using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Models; // Your DbContext namespace
using TaskManagementSystem.DataContext;
using TaskManagementSystem.Models.Permission; // Menu model namespace

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
                .Include(m => m.Parent)
                .OrderBy(m => m.DisplayOrder)
                .ToListAsync();
            return View(menus);
        }

        // GET: Menu/Create
        public IActionResult Create()
        {
            ViewBag.Parents = _context.Menus.Where(m => m.ParentId == null).ToList();
            return View();
        }

        // POST: Menu/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Menu menu)
        {
            if (ModelState.IsValid)
            {
                _context.Add(menu);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Parents = _context.Menus.Where(m => m.ParentId == null).ToList();
            return View(menu);
        }

        // GET: Menu/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var menu = await _context.Menus.FindAsync(id);
            if (menu == null) return NotFound();

            ViewBag.Parents = _context.Menus.Where(m => m.ParentId == null && m.Id != id).ToList();
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
                _context.Update(menu);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Parents = _context.Menus.Where(m => m.ParentId == null && m.Id != id).ToList();
            return View(menu);
        }

        // GET: Menu/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var menu = await _context.Menus
                .Include(m => m.Parent)
                .FirstOrDefaultAsync(m => m.Id == id);
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
    }
}
