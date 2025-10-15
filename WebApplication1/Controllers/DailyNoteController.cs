using Microsoft.AspNetCore.Mvc;
using WebApplication1.DataContext;
using WebApplication1.Models;
using System.Linq;

namespace WebApplication1.Controllers
{
    public class DailyNoteController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DailyNoteController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Show all notes for logged-in user
        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var notes = _context.DailyNotes
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToList();

            return View(notes);
        }

        // GET: Create note
        public IActionResult Create()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            return View();
        }

        // POST: Create note
        [HttpPost]
        public IActionResult Create(DailyNote note)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                note.UserId = userId.Value;
                note.CreatedAt = DateTime.Now;
                _context.DailyNotes.Add(note);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(note);
        }
    }
}
