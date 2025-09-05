using System;
using System.Data.SQLite;
using System.IO;

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
                        TotalAmount REAL
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
    }
}
