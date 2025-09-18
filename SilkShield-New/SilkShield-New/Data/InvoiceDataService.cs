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
        using (var connection = _dbHelper.GetConnection())
        {
            connection.Open();
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    // 1. Inserting data into the Invoices Table.
                    string invoiceQuery = @"
                    INSERT INTO Invoices (InvoiceNumber, InvoiceDate, Customer, BuildingType, PelmetBoard, Motorized, PaymentMethod, Discount, TotalAmount, Location, CurtainLayerType, CurtainStyle, TransportLaborCost)
                    VALUES (@InvoiceNumber, @InvoiceDate, @Customer, @BuildingType, @PelmetBoard, @Motorized, @PaymentMethod, @Discount, @TotalAmount, @Location, @CurtainLayerType, @CurtainStyle, @TransportLaborCost);
                    SELECT last_insert_rowid();"; // This will get the newly entered InvoiceId.

                    long invoiceId;
                    using (var command = new SQLiteCommand(invoiceQuery, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@InvoiceNumber", invoice.InvoiceNumber);
                        command.Parameters.AddWithValue("@InvoiceDate", invoice.InvoiceDate.ToString("yyyy-MM-dd HH:mm:ss"));
                        command.Parameters.AddWithValue("@Customer", invoice.CustomerName);
                        command.Parameters.AddWithValue("@BuildingType", invoice.BuildingType);
                        command.Parameters.AddWithValue("@PelmetBoard", invoice.PelmetBoard);
                        command.Parameters.AddWithValue("@Motorized", invoice.Motorized);
                        command.Parameters.AddWithValue("@PaymentMethod", invoice.PaymentMethod);
                        command.Parameters.AddWithValue("@Discount", invoice.Discount);
                        command.Parameters.AddWithValue("@TotalAmount", invoice.GrandTotal);
                        command.Parameters.AddWithValue("@Location", invoice.Location);
                        command.Parameters.AddWithValue("@CurtainLayerType", invoice.CurtainLayerType);
                        command.Parameters.AddWithValue("@CurtainStyle", invoice.CurtainStyle);
                        command.Parameters.AddWithValue("@TransportLaborCost", invoice.TransportLaborCost);

                        invoiceId = (long)command.ExecuteScalar();
                    }

                    // 2. Entering data into the InvoiceItems table.
                    string itemQuery = @"
                    INSERT INTO InvoiceItems (InvoiceId, ItemName, Quantity, UnitPrice, Total, CurtainType)
                    VALUES (@InvoiceId, @ItemName, @Quantity, @UnitPrice, @Total, @CurtainType);";

                    foreach (var item in invoice.Items)
                    {
                        using (var command = new SQLiteCommand(itemQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@InvoiceId", invoiceId); // get ID
                            command.Parameters.AddWithValue("@ItemName", item.ItemName);
                            command.Parameters.AddWithValue("@Quantity", item.Quantity);
                            command.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);
                            command.Parameters.AddWithValue("@Total", item.Total);
                            command.Parameters.AddWithValue("@CurtainType", item.SelectedMaterial); // Selected Material is used for Curtain Type.

                            command.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception("Data entry error. Transaction reversed..", ex);
                }
            }
        }
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
        // Path to your logo and background image files
        string backgroundImagePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "InvoiceBack.jpg");
        string logoPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "logo.png");

        if (!File.Exists(backgroundImagePath))
        {
            throw new FileNotFoundException($"Background image not found at: {backgroundImagePath}");
        }
        if (!File.Exists(logoPath))
        {
            throw new FileNotFoundException($"Logo image not found at: {logoPath}");
        }

        // New - Increase the top margin to create more space below the logo
        Document doc = new Document(PageSize.A4, 25, 25, 150, 30);
        try
        {
            // 1. Create the PDF writer and set the event handler for both the background image and the logo
            PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));
            writer.PageEvent = new ImageBackgroundEventHandler(backgroundImagePath, logoPath);

            // 2. Open the document
            doc.Open();

            // ⚠️ Remove the old logo code from here
            // The logo will now be added automatically by the PageEvent handler on every page.
            // The following lines MUST be removed:
            // Image logoImage = Image.GetInstance(logoPath);
            // logoImage.ScaleToFit(150f, 150f);
            // logoImage.Alignment = Element.ALIGN_CENTER;
            // doc.Add(logoImage);

            // Invoice Header
            doc.Add(new Paragraph("INVOICE", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 22, BaseColor.BLACK)));
            doc.Add(new Paragraph($"Invoice No: {invoice.InvoiceNumber}"));
            doc.Add(new Paragraph($"Date: {invoice.InvoiceDate:yyyy-MM-dd}"));
            doc.Add(new Paragraph($"Customer Name: {invoice.CustomerName}"));
            doc.Add(new Paragraph($"Location: {invoice.Location}"));
            doc.Add(Chunk.NEWLINE);

            // Project Details
            doc.Add(new Paragraph("Project Details", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK)));
            doc.Add(new Paragraph($"Building Type: {invoice.BuildingType}"));
            doc.Add(new Paragraph($"Curtain Layer Type: {invoice.CurtainLayerType}"));
            doc.Add(new Paragraph($"Curtain Style: {invoice.CurtainStyle}"));
            doc.Add(new Paragraph($"Pelmet Board: {(invoice.PelmetBoard ? "Yes" : "No")}"));
            doc.Add(new Paragraph($"Motorized: {(invoice.Motorized ? "Yes" : "No")}"));
            doc.Add(Chunk.NEWLINE);

            // Invoice Items Table
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

            // Totals
            double subTotal = invoice.Items.Sum(i => i.Total);

            Paragraph subtotalPara = new Paragraph($"Subtotal: LKR {subTotal:N2}", FontFactory.GetFont(FontFactory.HELVETICA_BOLD));
            subtotalPara.Alignment = Element.ALIGN_RIGHT;
            doc.Add(subtotalPara);

            Paragraph transportPara = new Paragraph($"Transport & Labor Cost: LKR {invoice.TransportLaborCost:N2}");
            transportPara.Alignment = Element.ALIGN_RIGHT;
            doc.Add(transportPara);

            Paragraph discountPara = new Paragraph($"Discount: {invoice.Discount}%");
            discountPara.Alignment = Element.ALIGN_RIGHT;
            doc.Add(discountPara);

            Paragraph separator = new Paragraph("----------------------------------------------------------", new Font(Font.FontFamily.HELVETICA, 10, Font.NORMAL));
            separator.Alignment = Element.ALIGN_RIGHT;
            doc.Add(separator);

            Paragraph grandTotalPara = new Paragraph($"Grand Total: LKR {invoice.GrandTotal:N2}", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14));
            grandTotalPara.Alignment = Element.ALIGN_RIGHT;
            doc.Add(grandTotalPara);

            Paragraph paymentMethodPara = new Paragraph($"Payment Method: {invoice.PaymentMethod}");
            paymentMethodPara.Alignment = Element.ALIGN_RIGHT;
            doc.Add(paymentMethodPara);

            //  Additional Page with Details 
            doc.NewPage();

            //  Text content from the third page of the PDF
            doc.Add(new Paragraph("Details of Fabric and Related Accessories", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, BaseColor.BLACK)));
            doc.Add(new Paragraph("• Imported, premium-quality sheers. Lab tested and certified as First Class material."));
            doc.Add(new Paragraph("• OEKO-TEX® STANDARD certifies that products are tested for harmful substances to protect your health."));
            doc.Add(Chunk.NEWLINE);

            doc.Add(new Paragraph("Terms and Conditions", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, BaseColor.BLACK)));
            doc.Add(new Paragraph("• Prices are valid for 15 days from the date of quotation."));
            doc.Add(new Paragraph("• Made-to-Measure Policies: Custom orders (e.g., bespoke curtains/blinds) are typically non-refundable unless faulty."));
            doc.Add(new Paragraph("• Production Timing: Changes are only possible if notified before production begins (often within 24 hours of order placement)."));
            doc.Add(Chunk.NEWLINE);

            doc.Add(new Paragraph("Warranty Terms", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, BaseColor.BLACK)));
            doc.Add(new Paragraph("• 3 Years complete warranty for all accessories."));
            doc.Add(new Paragraph("5 Years warranty on fabric for dry cleaning."));
            doc.Add(Chunk.NEWLINE);

            doc.Add(new Paragraph("Payment Terms", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, BaseColor.BLACK)));
            doc.Add(new Paragraph("• Deposit Requirement: We do require a 70% deposit upon order confirmation, and the balance payment should be done after delivery/installation."));
            doc.Add(new Paragraph("• Project completion period within 14 Days from date of advance payment."));
            doc.Add(Chunk.NEWLINE);

            doc.Add(new Paragraph("Bank Details -", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, BaseColor.BLACK)));
            doc.Add(new Paragraph($"Account Name: K. M. G. C. Perera"));
            doc.Add(new Paragraph($"Account Number: 106057933770"));
            doc.Add(new Paragraph($"Bank & Branch: Sampath Bank Kadawatha"));
            doc.Add(Chunk.NEWLINE);

            doc.Add(new Paragraph("THANK YOU FOR CHOOSING US!", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, BaseColor.BLACK)));
            doc.Add(new Paragraph("SILKSHIELD PRIVATE LIMITED", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK)));

            doc.Add(Chunk.NEWLINE);
            doc.Add(new Paragraph("076 0526709/076 7886453 | 251/1 VIHARA MAWATHA, HUNUPITIYA, WATTALA | SHIELDSILK@GMAIL.COM"));

            doc.Close();
        }
        catch (Exception ex)
        {
            throw new Exception("PDF generation failed.", ex);
        }
    }
}