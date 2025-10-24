using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.DataContext;
using TaskManagementSystem.Models;

namespace TaskManagementSystem.Controllers
{
    public class PersonController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PersonController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Person/Create
        public IActionResult Create()
        {
            return View(); // ✅ শুধু একটি Person পাঠাচ্ছি, List নয়
        }

        // POST: Person/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Person model)
        {
            var userName = HttpContext.Session.GetString("UserName") ?? "Unknown";

            if (ModelState.IsValid)
            {
                model.CreatedAt = DateTime.Now;
                model.IsActive = true;
                model.CreatedByUserName = userName;

                _context.Persons.Add(model);
                await _context.SaveChangesAsync();

                // Transaction/Create এ redirect
                return RedirectToAction("Create", "Transaction");
            }

            return View(model); // যদি validation fail হয়
        }

        // GET: Person/Index
        public async Task<IActionResult> Index()
        {
            var userName = HttpContext.Session.GetString("UserName") ?? "Unknown";
            var persons = await _context.Persons
                                        .Where(p => !p.IsDeleted && p.CreatedByUserName == userName)
                                        .ToListAsync();

            return View(persons); // ✅ Index view এর জন্য List<Person> পাঠানো হচ্ছে
        }
    }
}
