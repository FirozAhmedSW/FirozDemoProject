using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using TaskManagementSystem.Common;

namespace TaskManagementSystem.Models
{
    public class Transaction : Base
    {
        [Required]
        [StringLength(100)]
        public string? PersonName { get; set; } // যার সাথে transaction হলো

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; } // টাকার পরিমাণ

        [Required]
        [StringLength(20)]
        public string? Type { get; set; } // "Given" বা "Received" বা "Loan"

        public string? Description { get; set; } // বিস্তারিত

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Now;

        // user relation
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User? User { get; set; }

        [StringLength(100)]
        public string? CreatedByUserName { get; set; }
    }
}
