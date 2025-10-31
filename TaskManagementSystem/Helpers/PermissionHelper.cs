using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.DataContext;
using TaskManagementSystem.Models.Permission;
using TaskManagementSystem.Models; // User model

namespace TaskManagementSystem.Helpers
{
    public class PermissionHelper
    {
        private readonly ApplicationDbContext _context;

        public PermissionHelper(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get RoleMenuPermission by roleId and menu title
        public RoleMenuPermission? GetPermission(int roleId, string menuTitle)
        {
            var menu = _context.Menus.FirstOrDefault(m => m.ControllerName == menuTitle && m.IsActive && !m.IsDeleted);
            if (menu == null) return null;

            var permission = _context.RoleMenuPermissions
                .FirstOrDefault(rmp => rmp.RoleId == roleId && rmp.MenuId == menu.Id && rmp.IsActive && !rmp.IsDeleted);
            return permission;
        }

        // Get menus that a role can view
        public List<Menu> GetMenusByRole(int roleId)
        {
            return _context.RoleMenuPermissions.Include(rmp => rmp.Menu)
                .Where(rmp => rmp.RoleId == roleId && rmp.CanView && !rmp.IsDeleted && rmp.IsActive
                              && rmp.Menu != null && !rmp.Menu.IsDeleted && rmp.Menu.IsActive)
                .Select(rmp => rmp.Menu!).Distinct().ToList();
        }

        // Get Role object by UserId
        public Role? GetRoleByUserId(int userId)
        {
            var user = _context.Users.Include(u => u.Role)
                        .FirstOrDefault(u => u.Id == userId && u.IsActive && !u.IsDeleted);
            return user?.Role;
        }
    }
}
