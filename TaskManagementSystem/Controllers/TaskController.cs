using Microsoft.AspNetCore.Mvc;
using TaskManagementSystem.DataContext;
using TaskManagementSystem.Models;

namespace TaskManagementSystem.Controllers
{
    public class TaskController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public TaskController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // =======================
        // INDEX - list tasks
        // =======================
        public IActionResult Index(string search)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var tasksQuery = _context.Tasks
                .Where(t => t.UserId == userId && !t.IsDeleted);

            if (!string.IsNullOrEmpty(search))
            {
                tasksQuery = tasksQuery.Where(t =>
                    t.Title.Contains(search) ||
                    t.Description.Contains(search) ||
                    t.CreatedAt.ToString().Contains(search));
            }

            var tasks = tasksQuery
                .OrderByDescending(t => t.CreatedAt)
                .ToList();

            ViewData["Search"] = search; // Keep search text in view

            return View(tasks);
        }


        // =======================
        // CREATE - GET
        // =======================
        public IActionResult Create()
        {
            return View();
        }

        // =======================
        // CREATE - POST
        // =======================
        [HttpPost]
        public IActionResult Create(TaskItem task, IFormFile? ImageFile)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    string uploadFolder = Path.Combine(_environment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadFolder))
                        Directory.CreateDirectory(uploadFolder);

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                    string filePath = Path.Combine(uploadFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        ImageFile.CopyTo(stream);
                    }

                    task.ImagePath = "/uploads/" + fileName;
                }

                task.UserId = userId.Value;
                task.CreatedAt = DateTime.Now;

                _context.Tasks.Add(task);
                _context.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(task);
        }

        // =======================
        // EDIT - GET
        // =======================
        public IActionResult Edit(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var task = _context.Tasks.FirstOrDefault(t => t.Id == id && t.UserId == userId && !t.IsDeleted);
            if (task == null) return NotFound();

            return View(task);
        }

        // =======================
        // EDIT - POST
        // =======================
        [HttpPost]
        public IActionResult Edit(TaskItem task, IFormFile? ImageFile)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var existing = _context.Tasks.FirstOrDefault(t => t.Id == task.Id && t.UserId == userId && !t.IsDeleted);
            if (existing == null) return NotFound();

            if (ModelState.IsValid)
            {
                existing.Title = task.Title;
                existing.Description = task.Description;
                existing.UpdatedAt = DateTime.Now;

                if (ImageFile != null && ImageFile.Length > 0)
                {
                    string uploadFolder = Path.Combine(_environment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadFolder))
                        Directory.CreateDirectory(uploadFolder);

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                    string filePath = Path.Combine(uploadFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        ImageFile.CopyTo(stream);
                    }

                    existing.ImagePath = "/uploads/" + fileName;
                }

                _context.Tasks.Update(existing);
                _context.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(task);
        }

        // =======================
        // DELETE
        // =======================
        public IActionResult Delete(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var task = _context.Tasks.FirstOrDefault(t => t.Id == id && t.UserId == userId && !t.IsDeleted);
            if (task == null) return NotFound();

            task.IsDeleted = true;
            task.UpdatedAt = DateTime.Now;

            _context.Tasks.Update(task);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // =======================
        // DETAILS
        // =======================
        public IActionResult Details(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var task = _context.Tasks.FirstOrDefault(t => t.Id == id && t.UserId == userId && !t.IsDeleted);
            if (task == null) return NotFound();

            return View(task);
        }
    }
}
