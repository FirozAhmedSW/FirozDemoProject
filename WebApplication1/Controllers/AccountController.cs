using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;

public class AccountController : Controller
{
    // GET: Login Page
    public IActionResult Login()
    {
        return View();
    }

    // POST: Login
    [HttpPost]
    public IActionResult Login(string username, string password)
    {
        if (!string.IsNullOrEmpty(username) && password == "1234")
        {
            // ✅ Set Session
            HttpContext.Session.SetString("UserName", username);
            return RedirectToAction("Dashboard");
        }

        ViewBag.Error = "Invalid credentials!";
        return View();
    }

    // GET: Dashboard
    public IActionResult Dashboard()
    {
        var user = HttpContext.Session.GetString("UserName");
        if (string.IsNullOrEmpty(user))
        {
            return RedirectToAction("Login");
        }

        ViewBag.UserName = user;
        return View();
    }

    // GET: Logout
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}
