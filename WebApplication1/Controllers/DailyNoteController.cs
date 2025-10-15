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

        // ========================
        // INDEX - Show all notes for logged-in user
        // ========================
        public IActionResult Index(string searchText = "", int page = 1, int pageSize = 6)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var query = _context.DailyNotes
                .Where(n => n.UserId == userId && !n.IsDeleted);

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query = query.Where(n => n.Note.Contains(searchText));
            }

            var totalNotes = query.Count();
            var totalPages = (int)Math.Ceiling(totalNotes / (double)pageSize);

            var notes = query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchText = searchText;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_DailyNotesTable", notes);
            }

            return View(notes);
        }


        // ========================
        // CREATE - GET
        // ========================
        public IActionResult Create()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            return View();
        }

        // ========================
        // CREATE - POST
        // ========================
        [HttpPost]
        public IActionResult Create(DailyNote note)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            // Prevent NULL value in DB
            note.Note = note.Note ?? string.Empty;

            note.UserId = userId.Value;
            note.CreatedAt = DateTime.Now;
            note.IsDeleted = false;

            _context.DailyNotes.Add(note);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }


        // ========================
        // EDIT - GET
        // ========================
        public IActionResult Edit(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var note = _context.DailyNotes.FirstOrDefault(n => n.Id == id && n.UserId == userId && !n.IsDeleted);
            if (note == null) return NotFound();

            return View(note);
        }

        // ========================
        // EDIT - POST
        // ========================
        [HttpPost]
        public IActionResult Edit(DailyNote note)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var existingNote = _context.DailyNotes.FirstOrDefault(n => n.Id == note.Id && n.UserId == userId && !n.IsDeleted);
            if (existingNote == null) return NotFound();

            if (ModelState.IsValid)
            {
                existingNote.Note = note.Note;
                existingNote.UpdatedAt = DateTime.Now;

                _context.DailyNotes.Update(existingNote);
                _context.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(note);
        }

        // ========================
        // DELETE - Soft Delete
        // ========================
        public IActionResult Delete(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var note = _context.DailyNotes.FirstOrDefault(n => n.Id == id && n.UserId == userId && !n.IsDeleted);
            if (note == null) return NotFound();

            note.IsDeleted = true;
            note.UpdatedAt = DateTime.Now;

            _context.DailyNotes.Update(note);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // ========================
        // DETAILS
        // ========================
        public IActionResult Details(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var note = _context.DailyNotes.FirstOrDefault(n => n.Id == id && n.UserId == userId && !n.IsDeleted);
            if (note == null) return NotFound();

            return View(note);
        }
    }
}
