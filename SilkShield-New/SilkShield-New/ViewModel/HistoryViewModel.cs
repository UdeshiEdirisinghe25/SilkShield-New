using System;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Windows.Controls;

namespace SilkShield_New.ViewModel
{
    public class HistoryViewModel
    {
        private string _connectionString = "Data Source=SilkShield.db;Version=3;";

        // Collection to bind to UI
        public ObservableCollection<Invoice> Invoices { get; set; } = new ObservableCollection<Invoice>();

        // Load invoices from DB
        public void LoadInvoices()
        {
            Invoices.Clear();
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                string query = "SELECT InvoiceId, Customer, InvoiceDate, TotalAmount FROM Invoices";

                using (var cmd = new SQLiteCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Invoices.Add(new Invoice
                        {
                            InvoiceId = reader.GetInt32(0),
                            Customer = reader.GetString(1),
                            InvoiceDate = reader.GetDateTime(2),
                            TotalAmount = reader.GetDecimal(3)
                        });
                    }
                }
            }
        }

        // Update an invoices table in database
        public void UpdateInvoice(Invoice invoices)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                string query = @"UPDATE Invoices SET
                                    InvoiceDate=@date,
                                    Customer=@customer,
                                    TotalAmount=@total
                                 WHERE InvoiceId=@id";

                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@date", invoices.InvoiceDate);
                    cmd.Parameters.AddWithValue("@customer", invoices.Customer);
                    cmd.Parameters.AddWithValue("@total", invoices.TotalAmount);
                    cmd.Parameters.AddWithValue("@id", invoices.InvoiceId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        

    }

    // Minimal model class
    public class Invoice
    {
        public int InvoiceId { get; set; }
        public string Customer { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
