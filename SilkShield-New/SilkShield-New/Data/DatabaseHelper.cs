using System;
using System.Data.SQLite;
using System.IO;
using SilkShield_New.Model;
using System.Collections.Generic;


namespace SilkShield_New.Data
{

    public class DatabaseHelper
    {
        private readonly string _dbPath;
        private readonly string _connectionString;

        public DatabaseHelper()
        {
            _dbPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Database",
                "SilkShieldDB.sqlite"
            );

            _connectionString = $"Data Source={_dbPath};Version=3;";

            InitializeDatabase();
        }

        public SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(_connectionString);
        }

        private void InitializeDatabase()
        {
            if (!File.Exists(_dbPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_dbPath));
                SQLiteConnection.CreateFile(_dbPath);
            }

            using (var connection = GetConnection())
            {
                connection.Open();

                string createInvoicesTableQuery = @"
                    CREATE TABLE IF NOT EXISTS Invoices (
                        InvoiceId INTEGER PRIMARY KEY AUTOINCREMENT,
                        InvoiceDate TEXT NOT NULL,
                        Customer TEXT NOT NULL,
                        BuildingType TEXT,
                        PelmetBoard TEXT,
                        Motorized TEXT,
                        PaymentMethod TEXT,
                        Discount REAL,
                        TotalAmount REAL,
                        Location TEXT

                    );";

                string createInvoiceItemsTableQuery = @"
                    CREATE TABLE IF NOT EXISTS InvoiceItems (
                        ItemId INTEGER PRIMARY KEY AUTOINCREMENT,
                        InvoiceId INTEGER NOT NULL,
                        ItemName TEXT,
                        Quantity REAL,
                        UnitPrice REAL,
                        Total REAL,
                        CurtainType TEXT,
                        FOREIGN KEY(InvoiceId) REFERENCES Invoices(InvoiceId)
                    );";

                using (var command = new SQLiteCommand(createInvoicesTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }

                using (var command = new SQLiteCommand(createInvoiceItemsTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }



            }
        }


        public List<InvoiceDTO> GetRecentInvoices(int limit = 5)
        {
            var invoices = new List<InvoiceDTO>();

            using (var connection = GetConnection())
            {
                connection.Open();

                string query = @"
                       SELECT InvoiceId, Customer, InvoiceDate, TotalAmount
                        FROM Invoices
                        ORDER BY datetime(InvoiceDate) DESC
                        LIMIT @Limit;
                        ";

                using (var command = new SQLiteCommand(query, connection))
                     {
                    command.Parameters.AddWithValue("@Limit", limit);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            invoices.Add(new InvoiceDTO
                            {
                                InvoiceId = Convert.ToInt32(reader["InvoiceId"]),
                                Customer = reader["Customer"].ToString(),
                                InvoiceDate = DateTime.Parse(reader["InvoiceDate"].ToString()),
                                TotalAmount = Convert.ToDouble(reader["TotalAmount"])
                            });
                        }
                    }
                }
            }

            return invoices;
        }

    }
}