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

        public async Task<IActionResult> Index(string searchString = "", int page = 1, int pageSize = 5)
        {
            var userName = HttpContext.Session.GetString("UserName") ?? "Unknown";

            var query = _context.Persons
                                .Where(p => !p.IsDeleted && p.CreatedByUserName == userName);

            if (!string.IsNullOrWhiteSpace(searchString))
                query = query.Where(p => p.Name.Contains(searchString));

            var totalItems = await query.CountAsync();
            var persons = await query
                            .OrderByDescending(p => p.CreatedAt)
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.SearchString = searchString;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_PersonTablePartial", persons);

            return View(persons);
        }



        // GET: Person/Create
        public IActionResult Create()
        {
            return View();
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

                return RedirectToAction("Index");
            }

            return View(model);
        }

        // GET: Person/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var person = await _context.Persons
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

            if (person == null)
                return NotFound();

            return View(person);
        }

        // GET: Person/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var person = await _context.Persons.FindAsync(id);
            if (person == null || person.IsDeleted)
                return NotFound();

            return View(person);
        }

        // POST: Person/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Person model)
        {
            if (id != model.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                var person = await _context.Persons.FindAsync(id);
                if (person == null || person.IsDeleted)
                    return NotFound();

                // শুধু Name, Address, Phone update
                person.Name = model.Name;
                person.Address = model.Address;
                person.Phone = model.Phone;

                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAjax([FromForm] Person model)
        {
            var userName = HttpContext.Session.GetString("UserName") ?? "Unknown";

            if (string.IsNullOrWhiteSpace(model.Name))
                return BadRequest("Name is required");

            model.CreatedAt = DateTime.Now;
            model.IsActive = true;
            model.CreatedByUserName = userName;

            _context.Persons.Add(model);
            await _context.SaveChangesAsync();

            return Json(new { id = model.Id, name = model.Name });
        }



        // POST: Person/Delete/5 (soft delete)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var person = await _context.Persons.FindAsync(id);
            if (person != null && !person.IsDeleted)
            {
                person.IsDeleted = true; // Soft delete
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }




    }
}
