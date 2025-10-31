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
        public async Task<IActionResult> Index(int? userId)
        {
            // 1️⃣ All users for dropdown
            ViewBag.Users = await _context.Users
                .Where(u => !u.IsDeleted && u.IsActive)
                .Select(u => new { u.Id, u.UserName, u.RoleId })
                .ToListAsync();

            if (userId == null)
            {
                ViewBag.SelectedUserId = null;
                ViewBag.SelectedUserRole = null;
                return View(new List<RoleMenuPermission>());
            }

            // 2️⃣ Find selected user
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted && u.IsActive);

            if (user == null)
            {
                ViewBag.SelectedUserId = userId;
                ViewBag.SelectedUserRole = "Not Found";
                return View(new List<RoleMenuPermission>());
            }

            // 3️⃣ Load permissions for that user’s role
            var permissions = await _context.RoleMenuPermissions
                .Include(rmp => rmp.Menu)
                .Include(rmp => rmp.Role)
                .Where(rmp => rmp.RoleId == user.RoleId && !rmp.IsDeleted && rmp.IsActive)
                .OrderBy(rmp => rmp.Menu.Title)
                .ToListAsync();

            ViewBag.SelectedUserId = userId;
            ViewBag.SelectedUserRole = user.Role?.Name ?? "No Role";

            return View(permissions);
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePermissions(int userId, List<RoleMenuPermission> Permissions)
        {
            foreach (var perm in Permissions)
            {
                var dbPerm = await _context.RoleMenuPermissions.FirstOrDefaultAsync(p => p.Id == perm.Id);
                if (dbPerm != null)
                {
                    dbPerm.CanView = perm.CanView;
                    dbPerm.CanCreate = perm.CanCreate;
                    dbPerm.CanEdit = perm.CanEdit;
                    dbPerm.CanDelete = perm.CanDelete;
                    dbPerm.UpdatedAt = DateTime.Now;
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Permissions updated successfully!";
            return RedirectToAction("Index", new { userId = userId });
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
