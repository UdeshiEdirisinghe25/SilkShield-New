using System;
using System.Collections.Generic;
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

                string createInventoryTableQuery = @"
                    CREATE TABLE IF NOT EXISTS inventory (
                        ItemID INTEGER PRIMARY KEY AUTOINCREMENT,
                        ItemName TEXT NOT NULL,
                        Category TEXT,
                        Material TEXT,
                        MeasuringUnit TEXT,
                        UnitPrice REAL,
                        StockStatus TEXT
                    );";

                using (var command = new SQLiteCommand(createInvoicesTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }

                using (var command = new SQLiteCommand(createInvoiceItemsTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }

                using (var command = new SQLiteCommand(createInventoryTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public List<InventoryItem> GetAllInventoryItems()
        {
            var items = new List<InventoryItem>();

            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    string query = "SELECT ItemID, ItemName, Category, Material, MeasuringUnit, UnitPrice, StockStatus FROM inventory";

                    using (var command = new SQLiteCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        int itemIDOrdinal = reader.GetOrdinal("ItemID");
                        int itemNameOrdinal = reader.GetOrdinal("ItemName");
                        int categoryOrdinal = reader.GetOrdinal("Category");
                        int materialOrdinal = reader.GetOrdinal("Material");
                        int measuringUnitOrdinal = reader.GetOrdinal("MeasuringUnit");
                        int unitPriceOrdinal = reader.GetOrdinal("UnitPrice");
                        int stockStatusOrdinal = reader.GetOrdinal("StockStatus");

                        while (reader.Read())
                        {
                            items.Add(new InventoryItem
                            {
                                ItemID = reader.IsDBNull(itemIDOrdinal) ? 0 : reader.GetInt32(itemIDOrdinal),
                                ItemName = reader.IsDBNull(itemNameOrdinal) ? "" : reader.GetString(itemNameOrdinal),
                                Category = reader.IsDBNull(categoryOrdinal) ? "" : reader.GetString(categoryOrdinal),
                                Material = reader.IsDBNull(materialOrdinal) ? "" : reader.GetString(materialOrdinal),
                                MeasuringUnit = reader.IsDBNull(measuringUnitOrdinal) ? "" : reader.GetString(measuringUnitOrdinal),
                                UnitPrice = reader.IsDBNull(unitPriceOrdinal) ? 0.0 : reader.GetDouble(unitPriceOrdinal),
                                StockStatus = reader.IsDBNull(stockStatusOrdinal) ? "" : reader.GetString(stockStatusOrdinal)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAllInventoryItems error: {ex.Message}");
            }

            return items;
        }

        public List<InventoryItem> SearchInventoryItems(string searchText)
        {
            var items = new List<InventoryItem>();

            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    string query = @"SELECT ItemID, ItemName, Category, Material, MeasuringUnit, UnitPrice, StockStatus 
                                     FROM inventory 
                                     WHERE ItemName LIKE @searchText";

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@searchText", $"%{searchText}%");

                        using (var reader = command.ExecuteReader())
                        {
                            int itemIDOrdinal = reader.GetOrdinal("ItemID");
                            int itemNameOrdinal = reader.GetOrdinal("ItemName");
                            int categoryOrdinal = reader.GetOrdinal("Category");
                            int materialOrdinal = reader.GetOrdinal("Material");
                            int measuringUnitOrdinal = reader.GetOrdinal("MeasuringUnit");
                            int unitPriceOrdinal = reader.GetOrdinal("UnitPrice");
                            int stockStatusOrdinal = reader.GetOrdinal("StockStatus");

                            while (reader.Read())
                            {
                                items.Add(new InventoryItem
                                {
                                    ItemID = reader.IsDBNull(itemIDOrdinal) ? 0 : reader.GetInt32(itemIDOrdinal),
                                    ItemName = reader.IsDBNull(itemNameOrdinal) ? "" : reader.GetString(itemNameOrdinal),
                                    Category = reader.IsDBNull(categoryOrdinal) ? "" : reader.GetString(categoryOrdinal),
                                    Material = reader.IsDBNull(materialOrdinal) ? "" : reader.GetString(materialOrdinal),
                                    MeasuringUnit = reader.IsDBNull(measuringUnitOrdinal) ? "" : reader.GetString(measuringUnitOrdinal),
                                    UnitPrice = reader.IsDBNull(unitPriceOrdinal) ? 0.0 : reader.GetDouble(unitPriceOrdinal),
                                    StockStatus = reader.IsDBNull(stockStatusOrdinal) ? "" : reader.GetString(stockStatusOrdinal)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SearchInventoryItems error: {ex.Message}");
            }

            return items;
        }

        // Add this method back to your DatabaseHelper class.
        public bool DeleteInventoryItem(int itemId)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    string query = "DELETE FROM inventory WHERE ItemID = @itemId";

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@itemId", itemId);
                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Delete error: {ex.Message}");
                return false;
            }
        }
    }

    public class InventoryItem
    {
        public int ItemID { get; set; }
        public string ItemName { get; set; }
        public string Category { get; set; }
        public string Material { get; set; }
        public string MeasuringUnit { get; set; }
        public double UnitPrice { get; set; }
        public string StockStatus { get; set; }

        public string Price => $"Rs. {UnitPrice:N2}";
        public string StatusText => StockStatus == "In Stock" ? "Active" : "Inactive";
        public string Status => StockStatus == "In Stock" ? "Active" : "Inactive";
    }
}