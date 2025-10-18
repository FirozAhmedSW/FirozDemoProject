﻿using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.DataContext;
using TaskManagementSystem.Models;
using System.IO;


namespace TaskManagementSystem.Controllers
{
    public class ActivityLogsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private const int PageSize = 10;

        public ActivityLogsController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // 🟢 Index View (Filter + Pagination)
        public async Task<IActionResult> Index(string? user, DateTime? from, DateTime? to, int page = 1)
        {
            var query = _context.ActivityLogs.AsQueryable();

            if (!string.IsNullOrEmpty(user))
                query = query.Where(x => x.UserName != null && x.UserName.Contains(user));

            if (from.HasValue && to.HasValue)
            {
                var toDate = to.Value.AddDays(1);
                query = query.Where(x => x.CreatedAt >= from && x.CreatedAt < toDate);
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

            var logs = await query
                .OrderByDescending(l => l.CreatedAt)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewData["CurrentUser"] = user ?? "";
            ViewData["From"] = from?.ToString("yyyy-MM-dd");
            ViewData["To"] = to?.ToString("yyyy-MM-dd");
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;

            return View(logs);
        }

        // 🧾 PDF Report Generator
        [HttpGet]
        public async Task<IActionResult> Report(string? user, DateTime? from, DateTime? to)
        {
            var query = _context.ActivityLogs.AsQueryable();

            if (!string.IsNullOrEmpty(user))
                query = query.Where(x => x.UserName != null && x.UserName.Contains(user));

            if (from.HasValue && to.HasValue)
            {
                var toDate = to.Value.AddDays(1);
                query = query.Where(x => x.CreatedAt >= from && x.CreatedAt < toDate);
            }

            var logs = await query.OrderByDescending(x => x.CreatedAt).ToListAsync();

            if (!logs.Any())
            {
                TempData["Error"] = "No activity logs found for the selected filter.";
                return RedirectToAction(nameof(Index));
            }

            // ── Dynamic subtitle ──
            var titleParts = new List<string>();
            if (!string.IsNullOrEmpty(user)) titleParts.Add($"User: {user}");
            if (from.HasValue && to.HasValue)
                titleParts.Add($"Date Range: {from:dd-MMM-yyyy} to {to:dd-MMM-yyyy}");

            string filterText = titleParts.Count > 0
                ? " (Filters – " + string.Join(", ", titleParts) + ")"
                : string.Empty;

            int totalCount = logs.Count;
            string dynamicTitle = $"Activity Logs Report (Total: {totalCount}){filterText}";

            // ========= PDF Setup =========
            using var ms = new MemoryStream();
            var pageSize = new iTextSharp.text.Rectangle(842f, 595f); // A4 Landscape
            var doc = new Document(pageSize, 20f, 20f, 40f, 40f);

            var writer = PdfWriter.GetInstance(doc, ms);
            doc.Open();

            // ========= Custom Font Setup =========
            var fontPath = Path.Combine(_env.WebRootPath, "fonts", "arial.ttf");
            if (!System.IO.File.Exists(fontPath))
            {
                // Fallback system font
                fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
            }

            BaseFont baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            Font normalFont = new Font(baseFont, 10, Font.NORMAL);
            Font boldFont = new Font(baseFont, 12, Font.BOLD);
            Font headerFont = new Font(baseFont, 20, Font.BOLD);

            // ========= HEADER =========
            var logoPath = Path.Combine(_env.WebRootPath, "images", "fire_logo.png");
            var headerTable = new PdfPTable(2) { WidthPercentage = 100, SpacingAfter = 5f };
            headerTable.SetWidths(new float[] { 15f, 85f });

            if (System.IO.File.Exists(logoPath))
            {
                var logo = iTextSharp.text.Image.GetInstance(logoPath);
                logo.ScaleAbsolute(50f, 50f);
                var logoCell = new PdfPCell(logo)
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_RIGHT
                };
                headerTable.AddCell(logoCell);
            }
            else
            {
                headerTable.AddCell(new PdfPCell(new Phrase("")) { Border = Rectangle.NO_BORDER });
            }

            var deptCell = new PdfPCell(new Phrase("Department of Fire Service & Civil Defence", headerFont))
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE
            };
            headerTable.AddCell(deptCell);

            doc.Add(headerTable);
            doc.Add(new Paragraph("\n"));

            // ========= Subtitle =========
            var subTitle = new Paragraph(dynamicTitle, normalFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 15f
            };
            doc.Add(subTitle);

            // ========= Table =========
            var table = new PdfPTable(5) { WidthPercentage = 100f };
            table.SetWidths(new float[] { 5f, 20f, 25f, 20f, 30f });

            string[] headers = { "SL", "User Name", "Action", "IP Address", "Created At" };

            foreach (var h in headers)
            {
                table.AddCell(new PdfPCell(new Phrase(h, boldFont))
                {
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    BackgroundColor = BaseColor.LightGray,
                    Padding = 5f
                });
            }

            int sl = 1;
            foreach (var log in logs)
            {
                table.AddCell(new PdfPCell(new Phrase(sl.ToString(), normalFont)) { Padding = 5f });
                table.AddCell(new PdfPCell(new Phrase(log.UserName ?? "", normalFont)) { Padding = 5f });
                table.AddCell(new PdfPCell(new Phrase(log.ActionType ?? "", normalFont)) { Padding = 5f });
                table.AddCell(new PdfPCell(new Phrase(log.IpAddress ?? "", normalFont)) { Padding = 5f });
                table.AddCell(new PdfPCell(new Phrase(log.CreatedAt.ToString("dd-MMM-yyyy hh:mm tt"), normalFont)) { Padding = 5f });
                sl++;
            }

            doc.Add(table);
            doc.Close();

            return File(ms.ToArray(), "application/pdf", "ActivityLogReport.pdf");
        }
    }
}
