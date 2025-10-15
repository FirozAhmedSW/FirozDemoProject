using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using System.Linq;
using WebApplication1.DataContext;
using System;

namespace WebApplication1.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ========================
        // LOGIN
        // ========================
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Username and Password are required!";
                return View();
            }

            var user = _context.Users
                .Where(u => !u.IsDeleted)
                .FirstOrDefault(u =>
                    (u.UserName ?? "").Equals(username) &&
                    (u.Password ?? "").Equals(password)
                );

            if (user != null)
            {
                HttpContext.Session.SetString("UserName", user.UserName ?? "");
                HttpContext.Session.SetInt32("UserId", user.Id);
                return RedirectToAction("Dashboard");
            }

            ViewBag.Error = "Invalid credentials!";
            return View();
        }


        // ========================
        // LOGOUT
        // ========================
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // ========================
        // DASHBOARD
        // ========================
        public IActionResult Dashboard()
        {
            var userName = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("Login");
            }

            ViewBag.UserName = userName;
            return View();
        }

        // ========================
        // USER LIST + SEARCH + PAGINATION
        // ========================
        public IActionResult Index(string searchText = "", int page = 1, int pageSize = 5)
        {
            // Check session
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserName")))
                return RedirectToAction("Login");

            var query = _context.Users
                .Where(u => !u.IsDeleted) // ✅ show only active users
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query = query.Where(u =>
                    u.UserName.Contains(searchText) ||
                    u.Email.Contains(searchText) ||
                    u.Address.Contains(searchText) ||
                    u.Contact.Contains(searchText) ||
                    u.About.Contains(searchText)
                );
            }

            var totalUsers = query.Count();
            var totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);
            var pagedUsers = query
                .OrderBy(u => u.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchText = searchText;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_UserTable", pagedUsers);
            }

            return View(pagedUsers);
        }

        // ========================
        // CREATE USER
        // ========================
        public IActionResult Create() => View();

        [HttpPost]
        public IActionResult Create(User user, IFormFile? Photo)
        {
            if (ModelState.IsValid)
            {
                if (Photo != null && Photo.Length > 0)
                {
                    // wwwroot/images ফোল্ডারে সেভ করব
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(Photo.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        Photo.CopyTo(stream);
                    }

                    user.PhotoPath = "/images/" + fileName;
                }

                _context.Users.Add(user);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(user);
        }


        // ========================
        // EDIT USER
        // ========================
        public IActionResult Edit(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id && !u.IsDeleted);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        public IActionResult Edit(User user, IFormFile? Photo)
        {
            var existingUser = _context.Users.Find(user.Id);
            if (existingUser == null) return NotFound();

            if (Photo != null && Photo.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(Photo.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    Photo.CopyTo(stream);
                }

                existingUser.PhotoPath = "/images/" + fileName;
            }

            // Update other fields
            existingUser.UserName = user.UserName;
            existingUser.Email = user.Email;

            // ✅ Update password only if not empty
            if (!string.IsNullOrEmpty(user.Password))
            {
                existingUser.Password = user.Password;
            }

            existingUser.Address = user.Address;
            existingUser.Contact = user.Contact;
            existingUser.About = user.About;
            existingUser.UpdatedAt = DateTime.Now;

            _context.Users.Update(existingUser);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }




        // ========================
        // SOFT DELETE USER
        // ========================
        public IActionResult Delete(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id && !u.IsDeleted);
            if (user == null) return NotFound();

            user.IsDeleted = true;
            user.UpdatedAt = DateTime.Now;
            user.UpdatedBy = HttpContext.Session.GetString("UserName") ?? "System";

            _context.Users.Update(user);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // ========================
        // DETAILS
        // ========================
        public IActionResult Details(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id && !u.IsDeleted);
            if (user == null) return NotFound();
            return View(user);
        }
    }
}
