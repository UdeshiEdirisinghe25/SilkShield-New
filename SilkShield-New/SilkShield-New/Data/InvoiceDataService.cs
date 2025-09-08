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
            PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));
            doc.Open();

            // Add invoice header details
            doc.Add(new Paragraph("INVOICE", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 22, BaseColor.BLACK)));
            doc.Add(new Paragraph($"Invoice No: {invoice.InvoiceNumber}"));
            doc.Add(new Paragraph($"Date: {invoice.InvoiceDate:yyyy-MM-dd}"));
            doc.Add(new Paragraph($"Customer Name: {invoice.CustomerName}"));
            doc.Add(new Paragraph($"Location: {invoice.Location}"));
            doc.Add(Chunk.NEWLINE);

            // Add the new properties related to the project
            doc.Add(new Paragraph("Project Details", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK)));

            // Extract the value from the ComboBoxItem before adding to the PDF
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

            doc.Close();
        }
        catch (Exception ex)
        {
            throw new Exception("PDF generation failed.", ex);
        }
    }
}