using Microsoft.AspNetCore.Mvc;
using System.IO;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Font;
using iText.IO.Font.Constants;

namespace WebApplication1.Controllers
{
    public class DashboardController : Controller
    {
        // Sample Reports page
        public IActionResult Reports()
        {
            var reports = new List<dynamic>
            {
                new { Id = 1, Name = "Fire Incident", Date = DateTime.Now.AddDays(-2), Status = "Open" },
                new { Id = 2, Name = "Equipment Check", Date = DateTime.Now.AddDays(-1), Status = "Closed" },
                new { Id = 3, Name = "Training", Date = DateTime.Now, Status = "Open" }
            };

            return View(reports);
        }

        // Generate PDF using iText7
        public IActionResult ReportsPdf()
        {
            var reports = new List<dynamic>
            {
                new { Id = 1, Name = "Fire Incident", Date = DateTime.Now.AddDays(-2), Status = "Open" },
                new { Id = 2, Name = "Equipment Check", Date = DateTime.Now.AddDays(-1), Status = "Closed" },
                new { Id = 3, Name = "Training", Date = DateTime.Now, Status = "Open" }
            };

            using var ms = new MemoryStream();
            using (var writer = new PdfWriter(ms))
            {
                using var pdf = new PdfDocument(writer);
                var document = new Document(pdf);

                // ✅ Bold font for header
                PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

                // Title
                document.Add(new Paragraph("Reports")
                    .SetFont(boldFont)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(18));

                document.Add(new Paragraph("\n"));

                // Table: 4 columns
                Table table = new Table(4, true);

                // Header row bold
                table.AddHeaderCell(new Cell().Add(new Paragraph("ID").SetFont(boldFont)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Name").SetFont(boldFont)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Date").SetFont(boldFont)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Status").SetFont(boldFont)));

                // Data rows
                foreach (var r in reports)
                {
                    table.AddCell(r.Id.ToString());
                    table.AddCell(r.Name);
                    table.AddCell(r.Date.ToString("yyyy-MM-dd"));
                    table.AddCell(r.Status);
                }

                document.Add(table);
                document.Close();
            }

            // Return PDF as File
            return File(ms.ToArray(), "application/pdf", "Reports.pdf");
        }
    }
}
