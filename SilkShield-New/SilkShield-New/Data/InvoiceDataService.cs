using System;
using System.IO;
using System.Collections.Generic;
using System.Data.SQLite;
using SilkShield_New.Model;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Threading.Tasks;
using SilkShield_New.Data;

public class InvoiceDataService
{
    private readonly DatabaseHelper _dbHelper;

    public InvoiceDataService()
    {
        _dbHelper = new DatabaseHelper();
    }

    public void AddInvoice(Invoice invoice)
    {
        // This is where your database saving logic will go.
    }

    public async Task<List<string>> GetDistinctItemNamesAsync()
    {
        return await Task.Run(() => GetDistinctItemNames());
    }

    public async Task<List<string>> GetMaterialsByItemNameAsync(string itemName)
    {
        return await Task.Run(() => GetMaterialsByItemName(itemName));
    }

    public async Task<string> GetMeasuringUnitAsync(string itemName)
    {
        return await Task.Run(() => GetMeasuringUnit(itemName));
    }

    public async Task<double> GetUnitPriceAsync(string itemName, string material)
    {
        return await Task.Run(() => GetUnitPrice(itemName, material));
    }

    public string GetMeasuringUnit(string itemName)
    {
        string query = "SELECT MeasuringUnit FROM inventory WHERE ItemName = @ItemName LIMIT 1";
        using (var connection = _dbHelper.GetConnection())
        {
            connection.Open();
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ItemName", itemName);
                var result = command.ExecuteScalar();
                return result?.ToString() ?? string.Empty;
            }
        }
    }

    public double GetUnitPrice(string itemName, string material)
    {
        string query = "SELECT UnitPrice FROM inventory WHERE ItemName = @ItemName AND Material = @Material";
        using (var connection = _dbHelper.GetConnection())
        {
            connection.Open();
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ItemName", itemName);
                command.Parameters.AddWithValue("@Material", material);
                var result = command.ExecuteScalar();
                return result != null ? Convert.ToDouble(result) : 0;
            }
        }
    }

    public List<string> GetDistinctItemNames()
    {
        var itemNames = new List<string>();
        string query = "SELECT DISTINCT ItemName FROM inventory ORDER BY ItemName ASC";
        using (var connection = _dbHelper.GetConnection())
        {
            connection.Open();
            using (var command = new SQLiteCommand(query, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        itemNames.Add(reader.GetString(0));
                    }
                }
            }
        }
        return itemNames;
    }

    public List<string> GetMaterialsByItemName(string itemName)
    {
        var materials = new List<string>();
        string query = "SELECT DISTINCT Material FROM inventory WHERE ItemName = @ItemName";
        using (var connection = _dbHelper.GetConnection())
        {
            connection.Open();
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ItemName", itemName);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        materials.Add(reader["Material"].ToString());
                    }
                }
            }
        }
        return materials;
    }

    public void GenerateInvoicePdf(Invoice invoice, string filePath)
    {
        Document doc = new Document(PageSize.A4, 25, 25, 30, 30);
        try
        {
            // Get the path to the executable's directory
            string appPath = AppDomain.CurrentDomain.BaseDirectory;

            // Construct the full path to the image file inside the Resources folder
            string backgroundImagePath = Path.Combine(appPath, "Resources", "InvoiceBack.jpg");

            // Make sure the image file exists
            if (!File.Exists(backgroundImagePath))
            {
                throw new FileNotFoundException("Background image file 'InvoiceBack.jpg' not found in the Resources folder.", backgroundImagePath);
            }

            PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));
            writer.PageEvent = new ImageBackgroundEventHandler(backgroundImagePath);

            doc.Open();

            // Add company logo and details
            PdfPTable headerTable = new PdfPTable(2);
            headerTable.WidthPercentage = 100;
            headerTable.SetWidths(new float[] { 1, 1 });
            headerTable.DefaultCell.Border = PdfPCell.NO_BORDER;
            headerTable.DefaultCell.VerticalAlignment = Element.ALIGN_TOP;

            try
            {
                string logoPath = Path.Combine(appPath, "Resources", "WhatsApp Image 2025-08-31 at 17.43.13_87fec46d.jpg");
                Image logo = Image.GetInstance(logoPath);
                logo.ScaleToFit(100f, 50f);
                logo.Alignment = Element.ALIGN_LEFT;
                PdfPCell logoCell = new PdfPCell(logo);
                logoCell.Border = PdfPCell.NO_BORDER;
                logoCell.HorizontalAlignment = Element.ALIGN_LEFT;
                headerTable.AddCell(logoCell);
            }
            catch (Exception ex)
            {
                PdfPCell emptyCell = new PdfPCell(new Phrase(""));
                emptyCell.Border = PdfPCell.NO_BORDER;
                headerTable.AddCell(emptyCell);
                Console.WriteLine($"Error adding logo: {ex.Message}");
            }

            PdfPCell contactCell = new PdfPCell(new Phrase(""));
            contactCell.Border = PdfPCell.NO_BORDER;
            contactCell.HorizontalAlignment = Element.ALIGN_CENTER;
            contactCell.AddElement(new Paragraph("Silk Shield", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.BLACK)));
            contactCell.AddElement(new Paragraph("076 0526709 / 076 7886453"));
            contactCell.AddElement(new Paragraph("251/1 VIHARA MAWATHA, HUNUPITIYA, WATTALA"));
            contactCell.AddElement(new Paragraph("SHIELDSILK@GMAIL.COM"));
            headerTable.AddCell(contactCell);

            doc.Add(headerTable);
            doc.Add(Chunk.NEWLINE);

            // Add invoice header details
            doc.Add(new Paragraph("INVOICE", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 22, BaseColor.BLACK)));
            doc.Add(new Paragraph($"Invoice No: {invoice.InvoiceNumber}"));
            doc.Add(new Paragraph($"Date: {invoice.InvoiceDate:yyyy-MM-dd}"));
            doc.Add(new Paragraph($"Customer Name: {invoice.CustomerName}"));
            doc.Add(new Paragraph($"Location: {invoice.Location}"));
            doc.Add(Chunk.NEWLINE);

            // Add the new properties related to the project
            doc.Add(new Paragraph("Project Details", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK)));

            string buildingType = invoice.BuildingType?.ToString()?.Split(':')[1].Trim() ?? string.Empty;
            string curtainLayerType = invoice.CurtainLayerType?.ToString()?.Split(':')[1].Trim() ?? string.Empty;
            string curtainStyle = invoice.CurtainStyle?.ToString()?.Split(':')[1].Trim() ?? string.Empty;
            string paymentMethod = invoice.PaymentMethod?.ToString()?.Split(':')[1].Trim() ?? string.Empty;

            doc.Add(new Paragraph($"Building Type: {buildingType}"));
            doc.Add(new Paragraph($"Curtain Layer Type: {curtainLayerType}"));
            doc.Add(new Paragraph($"Curtain Style: {curtainStyle}"));
            doc.Add(new Paragraph($"Pelmet Board: {(invoice.PelmetBoard ? "Yes" : "No")}"));
            doc.Add(new Paragraph($"Motorized: {(invoice.Motorized ? "Yes" : "No")}"));
            doc.Add(Chunk.NEWLINE);

            // Add a table for invoice items
            PdfPTable table = new PdfPTable(4);
            table.WidthPercentage = 100;
            table.SetWidths(new float[] { 3, 1, 1, 1 });
            table.AddCell(new PdfPCell(new Phrase("Description", FontFactory.GetFont(FontFactory.HELVETICA_BOLD))));
            table.AddCell(new PdfPCell(new Phrase("Quantity", FontFactory.GetFont(FontFactory.HELVETICA_BOLD))));
            table.AddCell(new PdfPCell(new Phrase("Unit Price (LKR)", FontFactory.GetFont(FontFactory.HELVETICA_BOLD))));
            table.AddCell(new PdfPCell(new Phrase("Total (LKR)", FontFactory.GetFont(FontFactory.HELVETICA_BOLD))));

            foreach (var item in invoice.Items)
            {
                table.AddCell($"{item.ItemName} - {item.SelectedMaterial}");
                table.AddCell(item.Quantity.ToString());
                table.AddCell(item.UnitPrice.ToString("N2"));
                table.AddCell(item.Total.ToString("N2"));
            }

            doc.Add(table);
            doc.Add(Chunk.NEWLINE);

            // Add totals
            double subTotal = invoice.Items.Sum(i => i.Total);
            doc.Add(new Paragraph($"Subtotal: LKR {subTotal:N2}", FontFactory.GetFont(FontFactory.HELVETICA_BOLD)));
            doc.Add(new Paragraph($"Transport & Labor Cost: LKR {invoice.TransportLaborCost:N2}"));
            doc.Add(new Paragraph($"Discount: {invoice.Discount}%"));
            doc.Add(new Paragraph("----------------------------------------------------------"));
            doc.Add(new Paragraph($"Grand Total: LKR {invoice.GrandTotal:N2}", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14)));
            doc.Add(new Paragraph($"Payment Method: {paymentMethod}"));

            // Add the Terms and Conditions page
            AddTermsAndConditionsPage(doc);

            doc.Close();
        }
        catch (Exception ex)
        {
            throw new Exception("PDF generation failed.", ex);
        }
    }

    private void AddTermsAndConditionsPage(Document doc)
    {
        // Make sure the content starts on a new page
        doc.NewPage();

        // Details of Fabric and Related Accessories
        doc.Add(new Paragraph("Details of Fabric and Related Accessories", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK)));
        doc.Add(new Paragraph("• Imported, premium-quality sheers. Lab tested and certified as First Class material."));
        doc.Add(new Paragraph("• OEKO-TEX® STANDARD certifies that products are tested for harmful substances to protect your health."));
        doc.Add(Chunk.NEWLINE);

        // Terms and Conditions
        doc.Add(new Paragraph("Terms and Conditions", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK)));
        doc.Add(new Paragraph("• Prices are valid for 15 days from the date of quotation."));
        doc.Add(new Paragraph("• Made-to-Measure Policies: Custom orders (e.g., bespoke curtains/blinds) are typically non-refundable unless faulty."));
        doc.Add(new Paragraph("• Production Timing: Changes are only possible if notified before production begins (often within 24 hours of order placement)."));
        doc.Add(Chunk.NEWLINE);

        // Warranty Terms
        doc.Add(new Paragraph("Warranty Terms", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK)));
        doc.Add(new Paragraph("• 3 Years complete warranty for all accessories."));
        doc.Add(new Paragraph("5 Years warranty on fabric for dry cleaning."));
        doc.Add(Chunk.NEWLINE);

        // Payment Terms
        doc.Add(new Paragraph("Payment Terms", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK)));
        doc.Add(new Paragraph("• Deposit Requirement: We do require a 70% deposit upon order confirmation, and the balance payment should be done after delivery/installation."));
        doc.Add(new Paragraph("• Project completion period within 14 Days from date of advance payment."));
        doc.Add(Chunk.NEWLINE);

        // Bank Details
        doc.Add(new Paragraph("Bank Details -", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK)));
        doc.Add(new Paragraph("Account Name - K. M. G. C. Perera"));
        doc.Add(new Paragraph("Account Number - 106057933770"));
        doc.Add(new Paragraph("Bank & Branch - Sampath Bank Kadawatha"));
        doc.Add(Chunk.NEWLINE);
        doc.Add(Chunk.NEWLINE);

        // Thank You Note
        Paragraph thankYou = new Paragraph("THANK YOU FOR CHOOSING US!", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, BaseColor.BLACK));
        thankYou.Alignment = Element.ALIGN_CENTER;
        doc.Add(thankYou);

        // Add company details at the bottom of the last page
        Paragraph companyDetails = new Paragraph("SILKSHIELD PRIVATE LIMITED", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.BLACK));
        companyDetails.Alignment = Element.ALIGN_CENTER;
        doc.Add(companyDetails);

        companyDetails = new Paragraph("076 0526709/076 7886453", FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK));
        companyDetails.Alignment = Element.ALIGN_CENTER;
        doc.Add(companyDetails);

        companyDetails = new Paragraph("251/1 VIHARA MAWATHA, HUNUPITIYA, WATTALA", FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK));
        companyDetails.Alignment = Element.ALIGN_CENTER;
        doc.Add(companyDetails);

        companyDetails = new Paragraph("SHIELDSILK@GMAIL.COM", FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK));
        companyDetails.Alignment = Element.ALIGN_CENTER;
        doc.Add(companyDetails);
    }
}