using Microsoft.AspNetCore.Mvc;
using TaskManagementSystem.DataContext;
using TaskManagementSystem.Models;
using System.Linq;

namespace TaskManagementSystem.Controllers
{
    public class DailyNoteController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DailyNoteController(ApplicationDbContext context)
        {
            _context = context;
        }


    }
}
