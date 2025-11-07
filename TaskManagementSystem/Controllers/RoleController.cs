using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.DataContext;
using TaskManagementSystem.Models.Permission;

namespace TaskManagementSystem.Controllers
{
    public class RoleController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const int PageSize = 10;

        public RoleController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Roles with search and pagination
        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            var query = _context.Roles.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r => (r.Name ?? "").Contains(search) || (r.Description ?? "").Contains(search));
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

        // POST: Toggle IsActive
        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id, bool isActive)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null) return NotFound();

            role.IsActive = isActive;
            role.UpdatedAt = DateTime.Now;

            _context.Roles.Update(role);
            await _context.SaveChangesAsync();

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

                // If IsActive is unchecked, it will default to false
                // Checkbox sends "true" only when checked
                _context.Add(role);
                await _context.SaveChangesAsync();
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
            if (id != role.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    role.UpdatedAt = DateTime.Now;
                    _context.Update(role);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Roles.Any(r => r.Id == id)) return NotFound();
                    else throw;
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
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
