using System;
using System.IO;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using KursovaHomeGarden.Models.Plant;
using Microsoft.Data.SqlClient;
using System.Security.Claims;

public class PdfGeneratorService
{
    public byte[] GeneratePlantDetailsPdf(Plant plant, List<dynamic> careHistory)
    {
        using (var memoryStream = new MemoryStream())
        {
            // Create a PDF document
            Document document = new Document(PageSize.A4, 50, 50, 25, 25);
            PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);

            // Open the document
            document.Open();

            // Set fonts
            Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.DARK_GRAY);
            Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, BaseColor.DARK_GRAY);
            Font normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 12, BaseColor.BLACK);
            Font boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK);

            // Title
            Paragraph title = new Paragraph(plant.name, titleFont);
            title.Alignment = Element.ALIGN_CENTER;
            document.Add(title);
            document.Add(Chunk.NEWLINE);

            // Basic Plant Information
            PdfPTable infoTable = new PdfPTable(2);
            infoTable.WidthPercentage = 100;
            infoTable.SetWidths(new float[] { 1, 2 });

            AddTableRow(infoTable, "Category", plant.Category.category_name, boldFont, normalFont);
            AddTableRow(infoTable, "Price", plant.price.ToString("C"), boldFont, normalFont);
            AddTableRow(infoTable, "Care Level", plant.CareLevel.level_name, boldFont, normalFont);

            if (plant.SunlightRequirement != null)
            {
                AddTableRow(infoTable, "Sunlight Requirement", plant.SunlightRequirement.light_intensity, boldFont, normalFont);
            }

            document.Add(infoTable);
            document.Add(Chunk.NEWLINE);

            // Description
            Paragraph descriptionHeader = new Paragraph("Description", headerFont);
            document.Add(descriptionHeader);
            Paragraph description = new Paragraph(plant.description ?? "No description available", normalFont);
            document.Add(description);
            document.Add(Chunk.NEWLINE);

            // Care Instructions
            Paragraph careInstructionsHeader = new Paragraph("Care Instructions", headerFont);
            document.Add(careInstructionsHeader);

            if (plant.ActionFrequencies != null && plant.ActionFrequencies.Any())
            {
                PdfPTable careTable = new PdfPTable(5);
                careTable.WidthPercentage = 100;
                careTable.SetWidths(new float[] { 1, 1, 1, 1, 1 });

                // Table Headers
                string[] headers = { "Season", "Action", "Interval", "Volume", "Notes" };
                foreach (var header in headers)
                {
                    PdfPCell headerCell = new PdfPCell(new Phrase(header, boldFont));
                    headerCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                    careTable.AddCell(headerCell);
                }

                // Table Rows
                foreach (var action in plant.ActionFrequencies)
                {
                    careTable.AddCell(new Phrase(action.Season?.season_name ?? "-", normalFont));
                    careTable.AddCell(new Phrase(action.ActionType?.type_name ?? "-", normalFont));
                    careTable.AddCell(new Phrase(action.Interval ?? "-", normalFont));
                    careTable.AddCell(new Phrase(action.volume?.ToString() ?? "-", normalFont));
                    careTable.AddCell(new Phrase(action.notes ?? "-", normalFont));
                }

                document.Add(careTable);
            }
            else
            {
                document.Add(new Paragraph("No specific care instructions available.", normalFont));
            }

            document.Add(Chunk.NEWLINE);

            // Care History
            Paragraph careHistoryHeader = new Paragraph("Care History", headerFont);
            document.Add(careHistoryHeader);

            if (careHistory != null && careHistory.Any())
            {
                PdfPTable historyTable = new PdfPTable(3);
                historyTable.WidthPercentage = 100;
                historyTable.SetWidths(new float[] { 1, 1, 1 });

                // Table Headers
                string[] historyHeaders = { "Action Type", "Date Performed", "Next Care Due" };
                foreach (var header in historyHeaders)
                {
                    PdfPCell headerCell = new PdfPCell(new Phrase(header, boldFont));
                    headerCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                    historyTable.AddCell(headerCell);
                }

                // Table Rows
                foreach (var care in careHistory)
                {
                    historyTable.AddCell(new Phrase(care.TypeName ?? "-", normalFont));
                    historyTable.AddCell(new Phrase(Convert.ToDateTime(care.ActionDate).ToString("dd.MM.yyyy"), normalFont));
                    historyTable.AddCell(new Phrase(Convert.ToDateTime(care.NextCareDate).ToString("dd.MM.yyyy"), normalFont));
                }

                document.Add(historyTable);
            }
            else
            {
                document.Add(new Paragraph("No care history recorded yet.", normalFont));
            }

            // Close the document
            document.Close();

            return memoryStream.ToArray();
        }
    }

    private void AddTableRow(PdfPTable table, string label, string value, Font labelFont, Font valueFont)
    {
        table.AddCell(new PdfPCell(new Phrase(label, labelFont)) { Border = PdfPCell.NO_BORDER });
        table.AddCell(new PdfPCell(new Phrase(value, valueFont)) { Border = PdfPCell.NO_BORDER });
    }



}
