using TaskManagementSystem.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagementSystem.Models.Permission
{
    public class UserRole : Base
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public User? User { get; set; }

        [ForeignKey("RoleId")]
        public Role? Role { get; set; }
    }
}
