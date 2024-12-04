using iTextSharp.text;
using iTextSharp.text.pdf;
using KursovaHomeGarden.Models;
using KursovaHomeGarden.Services;
using System.IO;

public class PdfService : IPdfService
{
    public byte[] GenerateReportPdf(ReportData reportData)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            Document document = new Document(PageSize.A4, 25, 25, 30, 30);
            PdfWriter writer = PdfWriter.GetInstance(document, ms);

            document.Open();

            // Add title
            Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
            Paragraph title = new Paragraph("Garden Management System Report", titleFont);
            title.Alignment = Element.ALIGN_CENTER;
            title.SpacingAfter = 20f;
            document.Add(title);

            // Add generation date
            Font normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);
            Paragraph dateGenerated = new Paragraph($"Generated: {reportData.GeneratedAt:f}", normalFont);
            dateGenerated.SpacingAfter = 20f;
            document.Add(dateGenerated);

            // Plant Popularity Section
            AddSectionTitle(document, "Plant Popularity");
            PdfPTable plantTable = new PdfPTable(2);
            plantTable.WidthPercentage = 100;
            plantTable.SpacingAfter = 20f;

            // Add headers
            AddTableHeader(plantTable, new string[] { "Plant Name", "Popularity Count" });

            // Add data
            foreach (var plant in reportData.PlantPopularity)
            {
                plantTable.AddCell(new PdfPCell(new Phrase(plant.PlantName, normalFont)));
                plantTable.AddCell(new PdfPCell(new Phrase(plant.Popularity.ToString(), normalFont)));
            }
            document.Add(plantTable);

            // Category Statistics Section
            AddSectionTitle(document, "Category Statistics");
            PdfPTable categoryTable = new PdfPTable(3);
            categoryTable.WidthPercentage = 100;
            categoryTable.SpacingAfter = 20f;

            // Add headers
            AddTableHeader(categoryTable, new string[] { "Category", "Average Price", "User Count" });

            // Add data
            foreach (var category in reportData.CategoryStatistics)
            {
                categoryTable.AddCell(new PdfPCell(new Phrase(category.CategoryName, normalFont)));
                categoryTable.AddCell(new PdfPCell(new Phrase(category.AveragePrice.ToString("C"), normalFont)));
                categoryTable.AddCell(new PdfPCell(new Phrase(category.UserCount.ToString(), normalFont)));
            }
            document.Add(categoryTable);

            // User Information Section
            AddSectionTitle(document, "User Information");
            PdfPTable userTable = new PdfPTable(3);
            userTable.WidthPercentage = 100;
            userTable.SpacingAfter = 20f;

            // Add headers
            AddTableHeader(userTable, new string[] { "Username", "Role", "Balance" });

            // Add data
            foreach (var user in reportData.Users)
            {
                userTable.AddCell(new PdfPCell(new Phrase(user.UserName, normalFont)));
                userTable.AddCell(new PdfPCell(new Phrase(user.Role, normalFont)));
                userTable.AddCell(new PdfPCell(new Phrase(user.AmountOfMoney?.ToString("C") ?? "N/A", normalFont)));
            }
            document.Add(userTable);
            Paragraph dateGeneratedBottom = new Paragraph($"Report Generated On: {reportData.GeneratedAt:f}", normalFont);
            dateGeneratedBottom.Alignment = Element.ALIGN_RIGHT;
            dateGeneratedBottom.SpacingBefore = 30f;
            document.Add(dateGeneratedBottom);
            document.Close();
            return ms.ToArray();
        }
    }

    private void AddSectionTitle(Document document, string title)
    {
        Font sectionFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
        Paragraph sectionTitle = new Paragraph(title, sectionFont);
        sectionTitle.SpacingBefore = 15f;
        sectionTitle.SpacingAfter = 10f;
        document.Add(sectionTitle);
    }

    private void AddTableHeader(PdfPTable table, string[] headers)
    {
        Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
        foreach (string header in headers)
        {
            PdfPCell cell = new PdfPCell(new Phrase(header, headerFont));
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.Padding = 5;
            table.AddCell(cell);
        }
    }
}