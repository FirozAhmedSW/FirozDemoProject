using WebApplication1.Common;

namespace WebApplication1.Models
{
    public class User : Base
    {
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Address { get; set; }
        public string? Contact { get; set; }
        public string? About { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public string? PhotoPath { get; set; }
    }

}
