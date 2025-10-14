using WebApplication1.DataContext;
using WebApplication1.Services;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using WebApplication1.DataContext;
using WebApplication1.Services;

namespace EmployeePortal
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1️⃣ Add services to the container.
            builder.Services.AddControllersWithViews();

            // 2️⃣ Add ApplicationDbContext and configure SQL Server connection string
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("ECommerceDBConnection")));

            // 3️⃣ Register EmployeeService for Dependency Injection
            builder.Services.AddScoped<EmployeeService>();

            // 4️⃣ Add Session service
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            var app = builder.Build();

            // 5️⃣ Configure the HTTP request pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            // ✅ Enable session middleware
            app.UseSession();

            app.UseAuthorization();

            // 6️⃣ Rotativa PDF setup (points to wwwroot)
            var rotativaPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            //RotativaConfiguration.Setup(rotativaPath);

            // 7️⃣ Default route
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Login}/{id?}");

            app.Run();
        }
    }
}
