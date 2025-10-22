using iText.Commons.Actions.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using TaskManagementSystem.Models;

namespace TaskManagementSystem.DataContext
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options){}

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Designation> Designations { get; set; }
        public DbSet<EmployeeType> EmployeeTypes { get; set; }
        public DbSet<User> Users { get; set; }

        public DbSet<ActivityLog> ActivityLogs { get; set; }

        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<Expense> Expenses { get; set; }

    }
}
