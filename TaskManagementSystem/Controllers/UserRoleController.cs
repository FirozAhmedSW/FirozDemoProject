using Microsoft.AspNetCore.Mvc;
using TaskManagementSystem.DataContext;
using TaskManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Models.Permission;

namespace TaskManagementSystem.Controllers
{
    public class UserRoleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserRoleController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: UserRole
        public IActionResult Index()
        {
            var userRoles = _context.UserRoles
                                    .Include(ur => ur.User)
                                    .Include(ur => ur.Role)
                                    .Where(ur => !ur.IsDeleted)
                                    .ToList();
            return View(userRoles);
        }

        // GET: UserRole/Create
        public IActionResult Create()
        {
            ViewBag.Users = _context.Users.Where(u => u.IsActive && !u.IsDeleted).ToList();
            ViewBag.Roles = _context.Roles.Where(r => r.IsActive && !r.IsDeleted).ToList();
            return View();
        }

        // POST: UserRole/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(int UserId, int RoleId)
        {
            if (UserId == 0 || RoleId == 0)
            {
                ModelState.AddModelError("", "Please select both User and Role.");
                ViewBag.Users = _context.Users.Where(u => u.IsActive && !u.IsDeleted).ToList();
                ViewBag.Roles = _context.Roles.Where(r => r.IsActive && !r.IsDeleted).ToList();
                return View();
            }

            // Prevent duplicate assignment
            var exists = _context.UserRoles.Any(ur => ur.UserId == UserId && ur.RoleId == RoleId && !ur.IsDeleted);
            if (exists)
            {
                ModelState.AddModelError("", "This user already has this role.");
                ViewBag.Users = _context.Users.Where(u => u.IsActive && !u.IsDeleted).ToList();
                ViewBag.Roles = _context.Roles.Where(r => r.IsActive && !r.IsDeleted).ToList();
                return View();
            }

            var userRole = new UserRole
            {
                UserId = UserId,
                RoleId = RoleId,
                IsActive = true
            };

            _context.UserRoles.Add(userRole);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // GET: UserRole/Delete/5
        public IActionResult Delete(int id)
        {
            var userRole = _context.UserRoles.Include(ur => ur.User).Include(ur => ur.Role).FirstOrDefault(ur => ur.Id == id);
            if (userRole == null) return NotFound();
            return View(userRole);
        }

        // POST: UserRole/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var userRole = _context.UserRoles.Find(id);
            if (userRole != null)
            {
                userRole.IsDeleted = true;
                userRole.IsActive = false;
                userRole.UpdatedAt = DateTime.Now;
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
