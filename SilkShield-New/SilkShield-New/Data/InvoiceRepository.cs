using System;
using System.Data.SQLite;
using SilkShield_New.Model;
using System.Collections.Generic;

namespace SilkShield_New.Data
{
    public class InvoiceRepository
    {
        // Accessing the connection string from your existing DatabaseHelper
        private readonly string _connectionString = new DatabaseHelper().ConnectionString;

        public bool UpdateInvoice(Invoice invoice, string originalInvoiceNumber)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 0) Resolve InvoiceId for the original invoice number
                        long invoiceId;
                        string findInvoiceSql = "SELECT InvoiceId FROM Invoices WHERE InvoiceNumber = @OriginalInvoiceNumber LIMIT 1";
                        using (var findCmd = new SQLiteCommand(findInvoiceSql, connection, transaction))
                        {
                            findCmd.Parameters.AddWithValue("@OriginalInvoiceNumber", originalInvoiceNumber);
                            var idObj = findCmd.ExecuteScalar();
                            if (idObj == null || idObj == DBNull.Value)
                            {
                                // Invoice not found
                                throw new InvalidOperationException($"Invoice with number '{originalInvoiceNumber}' was not found.");
                            }
                            invoiceId = Convert.ToInt64(idObj);
                        }

                        // 1) Update the Main Invoice Header using the actual schema (Customer column name, InvoiceId PK)
                        string updateHeaderSql = @"UPDATE Invoices SET 
                                                InvoiceNumber = @InvoiceNumber,
                                                InvoiceDate = @InvoiceDate,
                                                Customer = @Customer,
                                                Location = @Location,
                                                BuildingType = @BuildingType,
                                                CurtainLayerType = @CurtainLayerType,
                                                CurtainStyle = @CurtainStyle,
                                                PelmetBoard = @PelmetBoard,
                                                Motorized = @Motorized,
                                                TransportLaborCost = @TransportLaborCost,
                                                Discount = @Discount,
                                                PaymentMethod = @PaymentMethod,
                                                TotalAmount = @TotalAmount
                                                WHERE InvoiceId = @InvoiceId";

                        using (var cmd = new SQLiteCommand(updateHeaderSql, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@InvoiceNumber", invoice.InvoiceNumber);
                            cmd.Parameters.AddWithValue("@InvoiceDate", invoice.InvoiceDate.ToString("yyyy-MM-dd HH:mm:ss"));
                            cmd.Parameters.AddWithValue("@Customer", invoice.CustomerName ?? string.Empty);
                            cmd.Parameters.AddWithValue("@Location", invoice.Location ?? string.Empty);
                            cmd.Parameters.AddWithValue("@BuildingType", invoice.BuildingType ?? string.Empty);
                            cmd.Parameters.AddWithValue("@CurtainLayerType", invoice.CurtainLayerType ?? string.Empty);
                            cmd.Parameters.AddWithValue("@CurtainStyle", invoice.CurtainStyle ?? string.Empty);
                            cmd.Parameters.AddWithValue("@PelmetBoard", invoice.PelmetBoard ? "Yes" : "No");
                            cmd.Parameters.AddWithValue("@Motorized", invoice.Motorized ? "Yes" : "No");
                            cmd.Parameters.AddWithValue("@TransportLaborCost", invoice.TransportLaborCost);
                            cmd.Parameters.AddWithValue("@Discount", invoice.Discount);
                            cmd.Parameters.AddWithValue("@PaymentMethod", invoice.PaymentMethod ?? string.Empty);
                            cmd.Parameters.AddWithValue("@TotalAmount", invoice.GrandTotal);
                            cmd.Parameters.AddWithValue("@InvoiceId", invoiceId);
                            int rows = cmd.ExecuteNonQuery();
                            if (rows == 0)
                            {
                                throw new InvalidOperationException("Invoice header update affected 0 rows.");
                            }
                        }

                        // 2) Delete old line items by InvoiceId (schema uses InvoiceId FK)
                        string deleteItemsSql = "DELETE FROM InvoiceItems WHERE InvoiceId = @InvoiceId";
                        using (var deleteCmd = new SQLiteCommand(deleteItemsSql, connection, transaction))
                        {
                            deleteCmd.Parameters.AddWithValue("@InvoiceId", invoiceId);
                            deleteCmd.ExecuteNonQuery();
                        }

                        // 3) Insert updated line items referencing InvoiceId with correct column names
                        string insertItemSql = @"INSERT INTO InvoiceItems (InvoiceId, ItemName, Quantity, UnitPrice, Total, CurtainType) 
                                                    VALUES (@InvoiceId, @ItemName, @Quantity, @UnitPrice, @Total, @CurtainType)";

                        foreach (var item in invoice.Items)
                        {
                            using (var insertCmd = new SQLiteCommand(insertItemSql, connection, transaction))
                            {
                                insertCmd.Parameters.AddWithValue("@InvoiceId", invoiceId);
                                insertCmd.Parameters.AddWithValue("@ItemName", item.ItemName ?? string.Empty);
                                insertCmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                                insertCmd.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);
                                insertCmd.Parameters.AddWithValue("@Total", item.Total);
                                insertCmd.Parameters.AddWithValue("@CurtainType", item.SelectedMaterial ?? string.Empty);
                                insertCmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw; // surface exception to caller for diagnostics
                    }
                }
            }
        }
    }
}