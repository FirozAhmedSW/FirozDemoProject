namespace WebApplication1.Models
{
    public class DailyNote
    {
        public int Id { get; set; }

        public int UserId { get; set; } // Foreign key for User
        public string Note { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
