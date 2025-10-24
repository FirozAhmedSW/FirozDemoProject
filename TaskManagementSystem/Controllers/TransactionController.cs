﻿using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.DataContext;
using TaskManagementSystem.Models;

namespace TaskManagementSystem.Controllers
{
    public class TransactionController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly IWebHostEnvironment _environment;

        public TransactionController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? month, DateTime? from, DateTime? to, int page = 1)
        {
            int pageSize = 10;
            var userName = HttpContext.Session.GetString("UserName");

            // 🔹 Base Query
            var query = _context.Transactions
                .Include(t => t.User)
                .Include(t => t.Person)
                .Where(t => !t.IsDeleted && t.CreatedByUserName == userName)
                .AsQueryable();

            // 🔹 Month Filter
            if (!string.IsNullOrEmpty(month))
            {
                var monthDate = DateTime.Parse(month + "-01");
                query = query.Where(t => t.Date.Month == monthDate.Month && t.Date.Year == monthDate.Year);
            }

            // 🔹 Date Range Filter
            if (from.HasValue)
                query = query.Where(t => t.Date >= from.Value);
            if (to.HasValue)
                query = query.Where(t => t.Date <= to.Value);

            // 🔹 Count + Pagination
            int totalRecords = await query.CountAsync();
            var transactions = await query
                .OrderByDescending(t => t.Date)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 🔹 Total Amount (All filtered data)
            var totalAmount = await query.SumAsync(t => (decimal?)t.Amount) ?? 0;

            // 🔹 ViewBag values
            ViewBag.SelectedMonth = month;
            ViewBag.FromDate = from?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = to?.ToString("yyyy-MM-dd");
            ViewBag.TotalRecords = totalRecords;
            ViewBag.PageSize = pageSize;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            ViewBag.TotalAmount = totalAmount;

            return View(transactions);
        }


        // GET: Transaction/Create
        public async Task<IActionResult> Create()
        {
            var userName = HttpContext.Session.GetString("UserName");
            // শুধুমাত্র active & current user-এর Persons
            ViewBag.Persons = await _context.Persons
                                            .Where(p => !p.IsDeleted && p.CreatedByUserName == userName)
                                            .ToListAsync();

            return View();
        }

        // POST: Transaction/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Transaction model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userName = HttpContext.Session.GetString("UserName");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                model.UserId = userId.Value;
                model.CreatedAt = DateTime.Now;
                model.CreatedByUserName = userName ?? "Unknown";
                model.IsActive = true;

                // PersonName optional set করা
                if (model.PersonId > 0)
                {
                    var person = await _context.Persons.FindAsync(model.PersonId);
                    model.PersonName = person?.Name;
                }

                _context.Add(model);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            // Validation fail হলে dropdown আবার populate
            ViewBag.Persons = await _context.Persons
                                            .Where(p => !p.IsDeleted && p.CreatedByUserName == userName)
                                            .ToListAsync();

            return View(model);
        }




        // 🟩 Edit GET
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.Transactions.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        // 🟩 Edit POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Transaction model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var existing = await _context.Transactions.FindAsync(id);
                if (existing == null) return NotFound();

                existing.PersonName = model.PersonName;
                existing.Amount = model.Amount;
                existing.Type = model.Type;
                existing.Description = model.Description;
                existing.Date = model.Date;
                existing.UpdatedAt = DateTime.Now;

                _context.Update(existing);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // 🟩 Details
        public async Task<IActionResult> Details(int id)
        {
            var transaction = await _context.Transactions
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

            if (transaction == null)
                return NotFound();

            return View(transaction);
        }


        // 🟩 Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Transactions.FindAsync(id);
            if (item != null)
            {
                item.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> TransactionReport(string? month, DateTime? from, DateTime? to)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userName = HttpContext.Session.GetString("UserName") ?? "Unknown User";

            if (userId == null)
                return RedirectToAction("Login", "Account");

            // 🔹 User-wise filter
            var transactions = _context.Transactions
                .Where(x => !x.IsDeleted && x.UserId == userId)
                .AsQueryable();

            // 🔹 Filter by date range (priority)
            if (from.HasValue || to.HasValue)
            {
                if (from.HasValue)
                    transactions = transactions.Where(x => x.Date >= from.Value);
                if (to.HasValue)
                    transactions = transactions.Where(x => x.Date <= to.Value);
            }
            else if (!string.IsNullOrEmpty(month)) // 🔹 Filter by month
            {
                if (DateTime.TryParse($"{month}-01", out var selectedMonth))
                {
                    var monthStart = new DateTime(selectedMonth.Year, selectedMonth.Month, 1);
                    var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                    transactions = transactions.Where(x => x.Date >= monthStart && x.Date <= monthEnd);
                }
            }
            else // 🔹 Default: current month
            {
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;
                transactions = transactions.Where(x => x.Date.Month == currentMonth && x.Date.Year == currentYear);
            }

            var transactionList = await transactions.OrderByDescending(x => x.Date).ToListAsync();
            if (!transactionList.Any())
            {
                TempData["Error"] = "No transactions found for the selected filter.";
                return RedirectToAction(nameof(Index));
            }

            // 🔹 Person-wise summary (Type-wise)
            var summaryData = transactionList
                .GroupBy(x => new { x.PersonName, x.Type })
                .Select(g => new
                {
                    Person = g.Key.PersonName ?? "-",
                    Type = g.Key.Type ?? "-",
                    TotalAmount = g.Sum(x => x.Amount)
                })
                .OrderBy(x => x.Person)
                .ThenBy(x => x.Type)
                .ToList();

            // ── PDF Setup ──
            using var ms = new MemoryStream();
            var doc = new Document(PageSize.A4.Rotate(), 20f, 20f, 40f, 60f);
            var writer = PdfWriter.GetInstance(doc, ms);
            writer.PageEvent = new PdfFooter(userName);
            doc.Open();

            // ── Fonts ──
            var fontPath = Path.Combine(_environment.WebRootPath, "fonts", "arial.ttf");
            if (!System.IO.File.Exists(fontPath))
                fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");

            var baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            var normalFont = new Font(baseFont, 10);
            var boldFont = new Font(baseFont, 12, Font.BOLD);
            var headerFont = new Font(baseFont, 20, Font.BOLD);

            // ── Header ──
            var logoPath = Path.Combine(_environment.WebRootPath, "Logo", "logo.jpg");
            var headerTable = new PdfPTable(2) { WidthPercentage = 100, SpacingAfter = 10f };
            headerTable.SetWidths(new float[] { 15f, 85f });

            if (System.IO.File.Exists(logoPath))
            {
                var logo = iTextSharp.text.Image.GetInstance(logoPath);
                logo.ScaleAbsolute(50f, 50f);
                headerTable.AddCell(new PdfPCell(logo)
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_RIGHT
                });
            }
            else
            {
                headerTable.AddCell(new PdfPCell(new Phrase("")) { Border = Rectangle.NO_BORDER });
            }

            headerTable.AddCell(new PdfPCell(new Phrase("Transaction Report", headerFont))
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE
            });

            doc.Add(headerTable);
            doc.Add(new Paragraph("\n"));

            // ── Subtitle / Filters ──
            var filterParts = new List<string>();
            if (!string.IsNullOrEmpty(month)) filterParts.Add($"Month: {month}");
            if (from.HasValue && to.HasValue) filterParts.Add($"Date: {from:dd-MMM-yyyy} to {to:dd-MMM-yyyy}");
            var subtitleText = $"Total Transactions: {transactionList.Count}" +
                               (filterParts.Count > 0 ? $" (Filters – {string.Join(", ", filterParts)})" : "");
            var subtitle = new Paragraph(subtitleText, normalFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 10f
            };
            doc.Add(subtitle);

            // ── Person-wise Summary Table ──
            var summaryTable = new PdfPTable(3) { WidthPercentage = 60f, SpacingAfter = 15f, HorizontalAlignment = Element.ALIGN_LEFT };
            summaryTable.SetWidths(new float[] { 40f, 30f, 30f });
            summaryTable.AddCell(new PdfPCell(new Phrase("Person", boldFont)) { Padding = 5f, BackgroundColor = BaseColor.LightGray });
            summaryTable.AddCell(new PdfPCell(new Phrase("Type", boldFont)) { Padding = 5f, BackgroundColor = BaseColor.LightGray });
            summaryTable.AddCell(new PdfPCell(new Phrase("Total Amount", boldFont)) { Padding = 5f, BackgroundColor = BaseColor.LightGray, HorizontalAlignment = Element.ALIGN_RIGHT });

            foreach (var s in summaryData)
            {
                summaryTable.AddCell(new PdfPCell(new Phrase(s.Person, normalFont)) { Padding = 5f });
                summaryTable.AddCell(new PdfPCell(new Phrase(s.Type, normalFont)) { Padding = 5f });
                summaryTable.AddCell(new PdfPCell(new Phrase(s.TotalAmount.ToString("0.00") + " ৳", normalFont)) { Padding = 5f, HorizontalAlignment = Element.ALIGN_RIGHT });
            }

            doc.Add(summaryTable);

            // ── Transaction Table (Optional, full listing) ──
            var table = new PdfPTable(6) { WidthPercentage = 100f };
            table.SetWidths(new float[] { 5f, 20f, 20f, 25f, 15f, 15f });
            string[] headers = { "SL", "Date", "Person", "Description", "Amount", "Type" };
            foreach (var h in headers)
                table.AddCell(new PdfPCell(new Phrase(h, boldFont))
                {
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    BackgroundColor = BaseColor.LightGray,
                    Padding = 5f
                });

            int sl = 1;
            foreach (var t in transactionList)
            {
                table.AddCell(new PdfPCell(new Phrase(sl.ToString(), normalFont)) { Padding = 5f });
                table.AddCell(new PdfPCell(new Phrase(t.Date.ToString("dd-MMM-yyyy"), normalFont)) { Padding = 5f });
                table.AddCell(new PdfPCell(new Phrase(t.PersonName ?? "-", normalFont)) { Padding = 5f });
                table.AddCell(new PdfPCell(new Phrase(t.Description ?? "-", normalFont)) { Padding = 5f });
                table.AddCell(new PdfPCell(new Phrase(t.Amount.ToString("0.00"), normalFont)) { Padding = 5f, HorizontalAlignment = Element.ALIGN_RIGHT });
                table.AddCell(new PdfPCell(new Phrase(t.Type ?? "-", normalFont)) { Padding = 5f });
                sl++;
            }

            doc.Add(table);
            doc.Close();

            return File(ms.ToArray(), "application/pdf", "TransactionReport.pdf");
        }

        public class PdfFooter : PdfPageEventHelper
        {
            private readonly string _userName;
            public PdfFooter(string userName)
            {
                _userName = userName;
            }

            public override void OnEndPage(PdfWriter writer, Document document)
            {
                var font = FontFactory.GetFont(FontFactory.HELVETICA, 9, Font.NORMAL, BaseColor.Gray);
                var cb = writer.DirectContent;
                float yPos = document.PageSize.GetBottom(30);

                ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT,
                    new Phrase("Developed by Md Firoz Ali", font),
                    document.LeftMargin, yPos, 0);

                ColumnText.ShowTextAligned(cb, Element.ALIGN_CENTER,
                    new Phrase($"Report generated by {_userName}", font),
                    document.PageSize.Width / 2, yPos, 0);

                ColumnText.ShowTextAligned(cb, Element.ALIGN_RIGHT,
                    new Phrase($"Page {writer.PageNumber}", font),
                    document.PageSize.Width - document.RightMargin, yPos, 0);
            }
        }



    }
}
