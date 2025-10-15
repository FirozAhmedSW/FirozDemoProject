using System.ComponentModel.DataAnnotations;
using WebApplication1.Common;

namespace WebApplication1.Models
{
    public class DailyNote : Base
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Please enter your note.")]
        [StringLength(500, ErrorMessage = "Note cannot be longer than 500 characters.")]
        public string Note { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

}
