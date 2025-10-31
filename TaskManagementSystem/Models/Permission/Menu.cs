using TaskManagementSystem.Common;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagementSystem.Models.Permission
{
    public class Menu : Base
    {
        public string Title { get; set; } = string.Empty;
        public string? ControllerName { get; set; }
        public string? ActionName { get; set; }
        public int? ParentId { get; set; }
        public string? Icon { get; set; }
        public int? DisplayOrder { get; set; }

        [ForeignKey("ParentId")]
        public Menu? Parent { get; set; }
        public ICollection<Menu>? Children { get; set; }

        // Navigation property
        public ICollection<RoleMenuPermission>? RoleMenuPermissions { get; set; }
    }
}
