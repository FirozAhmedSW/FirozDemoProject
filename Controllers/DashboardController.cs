using Microsoft.AspNetCore.Mvc;
using System.IO;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Font;
using iText.IO.Font.Constants;

namespace TaskManagementSystem.Controllers
{
    public class DashboardController : Controller
    {
        // Sample Reports page
        public IActionResult Reports()
        {
            var reports = GetSampleReports();
            return View(reports);
        }

        // Generate PDF using iText7
        public IActionResult ReportsPdf()
        {
            var reports = GetSampleReports();

            using var ms = new MemoryStream();
            using (var writer = new PdfWriter(ms))
            {
                using var pdf = new PdfDocument(writer);
                var document = new Document(pdf);

                // Bold font for headers
                PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

                // Title
                document.Add(new Paragraph("Reports")
                    .SetFont(boldFont)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(18)
                    .SetMarginBottom(10));

                // Table with 4 columns, full width
                Table table = new Table(UnitValue.CreatePercentArray(new float[] { 1, 4, 3, 2 }))
                    .UseAllAvailableWidth();

                // Header row
                table.AddHeaderCell(new Cell().Add(new Paragraph("ID").SetFont(boldFont).SetFontSize(12)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Name").SetFont(boldFont).SetFontSize(12)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Date").SetFont(boldFont).SetFontSize(12)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Status").SetFont(boldFont).SetFontSize(12)));

                // Data rows
                foreach (var r in reports)
                {
                    table.AddCell(new Cell().Add(new Paragraph(r.Id.ToString())));
                    table.AddCell(new Cell().Add(new Paragraph(r.Name)));
                    table.AddCell(new Cell().Add(new Paragraph(r.Date.ToString("yyyy-MM-dd"))));
                    table.AddCell(new Cell().Add(new Paragraph(r.Status)));
                }

                document.Add(table);
                document.Close();
            }

            ms.Position = 0; // Reset stream position
            return File(ms.ToArray(), "application/pdf", "Reports.pdf");
        }

        // ✅ Sample data helper method
        private List<dynamic> GetSampleReports()
        {
            return new List<dynamic>
            {
                new { Id = 1, Name = "Fire Incident", Date = DateTime.Now.AddDays(-2), Status = "Open" },
                new { Id = 2, Name = "Equipment Check", Date = DateTime.Now.AddDays(-1), Status = "Closed" },
                new { Id = 3, Name = "Training", Date = DateTime.Now, Status = "Open" }
            };
        }
    }
}
