using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Models.Permission;
using TaskManagementSystem.DataContext;
using TaskManagementSystem.Models;

namespace TaskManagementSystem.Controllers
{
    public class RoleMenuPermissionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RoleMenuPermissionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: RoleMenuPermission
        public async Task<IActionResult> Index()
        {
            var permissions = await _context.RoleMenuPermissions
                .Include(rmp => rmp.Role)
                .Include(rmp => rmp.Menu)
                .OrderBy(rmp => rmp.RoleId)
                .ThenBy(rmp => rmp.MenuId)
                .ToListAsync();

            return View(permissions);
        }

        // GET: RoleMenuPermission/Create
        public IActionResult Create()
        {
            LoadDropdowns();
            return View();
        }

        // POST: RoleMenuPermission/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoleMenuPermission permission)
        {
            if (ModelState.IsValid)
            {
                // Check if already exists
                var exists = await _context.RoleMenuPermissions
                    .AnyAsync(rmp => rmp.RoleId == permission.RoleId && rmp.MenuId == permission.MenuId);

                if (exists)
                {
                    ModelState.AddModelError("", "Permission already exists for this role and menu.");
                    LoadDropdowns(permission.RoleId, permission.MenuId);
                    return View(permission);
                }

                permission.IsActive = true;
                permission.IsDeleted = false;
                permission.CreatedAt = DateTime.Now;

                _context.Add(permission);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            LoadDropdowns(permission.RoleId, permission.MenuId);
            return View(permission);
        }

        // GET: RoleMenuPermission/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var permission = await _context.RoleMenuPermissions.FindAsync(id);
            if (permission == null) return NotFound();

            LoadDropdowns(permission.RoleId, permission.MenuId);
            return View(permission);
        }

        // POST: RoleMenuPermission/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RoleMenuPermission permission)
        {
            if (id != permission.Id) return NotFound();

            if (ModelState.IsValid)
            {
                permission.UpdatedAt = DateTime.Now;
                _context.Update(permission);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            LoadDropdowns(permission.RoleId, permission.MenuId);
            return View(permission);
        }

        // GET: RoleMenuPermission/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var permission = await _context.RoleMenuPermissions
                .Include(rmp => rmp.Role)
                .Include(rmp => rmp.Menu)
                .FirstOrDefaultAsync(rmp => rmp.Id == id);

            if (permission == null) return NotFound();

            return View(permission);
        }

        // POST: RoleMenuPermission/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var permission = await _context.RoleMenuPermissions.FindAsync(id);
            if (permission != null)
            {
                _context.RoleMenuPermissions.Remove(permission);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // Helper
        private void LoadDropdowns(int? selectedRoleId = null, int? selectedMenuId = null)
        {
            ViewBag.Roles = new SelectList(_context.Roles.Where(r => r.IsActive), "Id", "Name", selectedRoleId);
            ViewBag.Menus = new SelectList(_context.Menus.Where(m => m.IsActive), "Id", "Title", selectedMenuId);
        }
    }
}
