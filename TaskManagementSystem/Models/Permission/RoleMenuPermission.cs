using TaskManagementSystem.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagementSystem.Models.Permission
{
    public class RoleMenuPermission : Base
    {
        public int RoleId { get; set; }
        public int MenuId { get; set; }

        public bool CanView { get; set; } = false;
        public bool CanCreate { get; set; } = false;
        public bool CanEdit { get; set; } = false;
        public bool CanDelete { get; set; } = false;

        // Navigation properties
        [ForeignKey("RoleId")]
        public Role? Role { get; set; }

        [ForeignKey("MenuId")]
        public Menu? Menu { get; set; }
    }
}
