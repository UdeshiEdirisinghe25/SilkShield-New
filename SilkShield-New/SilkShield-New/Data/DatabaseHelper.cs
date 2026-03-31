using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SQLite;
using System.IO;
using SilkShield_New.Model;

namespace SilkShield_New.Data
{
    public class DatabaseHelper
    {
        private readonly string _dbPath;
        private readonly string _connectionString;

        public string ConnectionString => _connectionString;
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

                // Ensure tables exist with expected columns. CREATE TABLE IF NOT EXISTS is used,
                // but we also ensure any missing columns are added for existing DB files.
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
                    Location TEXT,
                    InvoiceNumber TEXT,
                    CurtainLayerType TEXT,
                    CurtainStyle TEXT,
                    TransportLaborCost REAL,
                    IncludeDetailsPage INTEGER DEFAULT 1
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
                      ItemID INTEGER PRIMARY KEY,
                      ItemName TEXT NOT NULL,
                      Category TEXT NOT NULL,
                      Material TEXT NOT NULL,
                      MeasuringUnit TEXT NOT NULL,
                      UnitPrice REAL NOT NULL,
                      StockStatus TEXT NOT NULL
                    );";

                using (var command = new SQLiteCommand(createInventoryTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }

                using (var command = new SQLiteCommand(createInvoicesTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }

                using (var command = new SQLiteCommand(createInvoiceItemsTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Make sure expected columns exist on older DB files; add them if missing.
                EnsureColumnExists(connection, "Invoices", "CurtainStyle", "TEXT");
                EnsureColumnExists(connection, "Invoices", "TransportLaborCost", "REAL");
                EnsureColumnExists(connection, "Invoices", "IncludeDetailsPage", "INTEGER");
                EnsureColumnExists(connection, "InvoiceItems", "CurtainType", "TEXT");
                EnsureColumnExists(connection, "InvoiceItems", "MeasuringUnit", "TEXT");

                // Migration: set existing NULLs to empty string (no-op if column just created)
                try
                {
                    using (var cmd = new SQLiteCommand("UPDATE InvoiceItems SET MeasuringUnit = '' WHERE MeasuringUnit IS NULL;", connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
                catch
                {
                    // ignore migration failures (defensive)
                }
            }
        }

        private void EnsureColumnExists(SQLiteConnection connection, string tableName, string columnName, string columnType)
        {
            try
            {
                string pragma = $"PRAGMA table_info({tableName});";
                using (var cmd = new SQLiteCommand(pragma, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    bool found = false;
                    while (reader.Read())
                    {
                        if (reader["name"] != null && string.Equals(reader["name"].ToString(), columnName, StringComparison.OrdinalIgnoreCase))
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        string alter = $"ALTER TABLE {tableName} ADD COLUMN {columnName} {columnType};";
                        using (var alterCmd = new SQLiteCommand(alter, connection))
                        {
                            alterCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch
            {
                // Defensive: if schema adjustment fails, avoid throwing at runtime in production.
            }
        }

        //get invoices to history
        public List<Invoice> GetAllInvoices()
        {
            var invoices = new List<Invoice>();

            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();

                    string query = @"
                        SELECT 
                            InvoiceNumber,
                            InvoiceDate,
                            Customer,
                            Location,
                            BuildingType,
                            CurtainLayerType,
                            CurtainStyle,
                            PelmetBoard,
                            Motorized,
                            PaymentMethod,
                            Discount,
                            TotalAmount,
                            TransportLaborCost,
                            IncludeDetailsPage
                        FROM Invoices
                        ORDER BY InvoiceDate DESC";

                    using (var command = new SQLiteCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            invoices.Add(new Invoice
                            {
                                InvoiceNumber = SafeGetString(reader, "InvoiceNumber"),
                                InvoiceDate = ParseDateSafe(SafeGetString(reader, "InvoiceDate")),
                                CustomerName = SafeGetString(reader, "Customer"),
                                Location = SafeGetString(reader, "Location"),
                                BuildingType = SafeGetString(reader, "BuildingType"),
                                CurtainLayerType = SafeGetString(reader, "CurtainLayerType"),
                                CurtainStyle = SafeGetString(reader, "CurtainStyle"),
                                PelmetBoard = ParseBooleanFlexible(reader, "PelmetBoard"),
                                Motorized = ParseBooleanFlexible(reader, "Motorized"),
                                PaymentMethod = SafeGetString(reader, "PaymentMethod"),
                                Discount = SafeGetDouble(reader, "Discount"),
                                TotalAmount = SafeGetDecimal(reader, "TotalAmount"),
                                TransportLaborCost = SafeGetDouble(reader, "TransportLaborCost"),
                                IncludeDetailsPage = ParseBooleanFlexible(reader, "IncludeDetailsPage")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAllInvoices error: {ex.Message}");
            }

            return invoices;
        }

        private static string SafeGetString(SQLiteDataReader reader, string columnName)
        {
            try
            {
                int ord = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ord) ? string.Empty : reader.GetString(ord);
            }
            catch
            {
                return string.Empty;
            }
        }

        private static double SafeGetDouble(SQLiteDataReader reader, string columnName)
        {
            try
            {
                int ord = reader.GetOrdinal(columnName);
                if (reader.IsDBNull(ord)) return 0;
                var val = reader.GetValue(ord);
                if (val is double) return (double)val;
                if (val is float) return Convert.ToDouble(val);
                if (val is long || val is int) return Convert.ToDouble(val);
                if(val is string && double.TryParse((string)val, out double parsed)) return parsed;
                return Convert.ToDouble(val);

            }
            catch
            {
                return 0;
            }
        }

        private static decimal SafeGetDecimal(SQLiteDataReader reader, string columnName)
        {
            try
            {
                int ord = reader.GetOrdinal(columnName);
                if (reader.IsDBNull(ord)) return 0;
                var val = reader.GetValue(ord);
                return Convert.ToDecimal(val);
            }
            catch
            {
                return 0;
            }
        }

        private static DateTime ParseDateSafe(string dateString)
        {
            if (DateTime.TryParse(dateString, out var dt)) return dt;
            return DateTime.Now;
        }

        private static bool ParseBooleanFlexible(SQLiteDataReader reader, string columnName)
        {
            try
            {
                int ord = reader.GetOrdinal(columnName);
                if (reader.IsDBNull(ord)) return false;
                var val = reader.GetValue(ord);
                if (val == null) return false;

                // Handle several representations: "Yes"/"No", "1"/"0", true/false
                if (val is long || val is int)
                {
                    return Convert.ToInt64(val) != 0;
                }

                string s = val.ToString().Trim();
                if (string.Equals(s, "yes", StringComparison.OrdinalIgnoreCase)) return true;
                if (string.Equals(s, "true", StringComparison.OrdinalIgnoreCase)) return true;
                if (s == "1") return true;
                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteInvoice(string invoiceNumber)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    string query = "DELETE FROM Invoices WHERE InvoiceNumber = @no";

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@no", invoiceNumber);
                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DeleteInvoice error: {ex.Message}");
                return false;
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

        public bool UpdateInventoryItem(InventoryItem item)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    string query = @"UPDATE inventory SET
                                     ItemName = @itemName,
                                     Category = @category,
                                     Material = @material,
                                     MeasuringUnit = @measuringUnit,
                                     UnitPrice = @unitPrice,
                                     StockStatus = @stockStatus
                                     WHERE ItemID = @itemID";

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@itemName", item.ItemName);
                        command.Parameters.AddWithValue("@category", item.Category);
                        command.Parameters.AddWithValue("@material", item.Material);
                        command.Parameters.AddWithValue("@measuringUnit", item.MeasuringUnit);
                        command.Parameters.AddWithValue("@unitPrice", item.UnitPrice);
                        command.Parameters.AddWithValue("@stockStatus", item.StockStatus);
                        command.Parameters.AddWithValue("@itemID", item.ItemID);

                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateInventoryItem error: {ex.Message}");
                return false;
            }
        }

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

        // DatabaseHelper.cs file එක ඇතුළට
        public List<InvoiceDTO> GetRecentInvoices(int limit)
        {
            var invoices = new List<InvoiceDTO>();
            using (SQLiteConnection connection = GetConnection())
            {
                try
                {
                    connection.Open();
                    string sql = $"SELECT InvoiceId, Customer, InvoiceDate, TotalAmount FROM Invoices ORDER BY InvoiceDate DESC LIMIT {limit};";

                    using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                invoices.Add(new InvoiceDTO
                                {
                                    InvoiceId = reader.GetInt32(reader.GetOrdinal("InvoiceId")),
                                    Customer = reader.GetString(reader.GetOrdinal("Customer")),
                                    InvoiceDate = DateTime.Parse(reader.GetString(reader.GetOrdinal("InvoiceDate"))),
                                    TotalAmount = reader.GetDouble(reader.GetOrdinal("TotalAmount"))
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error getting recent invoices: {ex.Message}");
                }
            }
            return invoices;
        }
    }
}

        public class InventoryItem : INotifyPropertyChanged
        {
            private int _itemID;
            private string _itemName;
            private string _category;
            private string _material;
            private string _measuringUnit;
            private double _unitPrice; // PRIVATE FIELD
            private string _stockStatus;

            public int ItemID { get; set; }

            public string ItemName
            {
                get => _itemName;
                set
                {
                    if (_itemName != value)
                    {
                        _itemName = value;
                        OnPropertyChanged(nameof(ItemName));
                    }
                }
            }

            public string Category { get; set; }
            public string Material { get; set; }
            public string MeasuringUnit { get; set; }

            // *UPDATED PROPERTY*
            public double UnitPrice
            {
                get => _unitPrice;
                set
                {
                    if (_unitPrice != value)
                    {
                        _unitPrice = value;
                        OnPropertyChanged(nameof(UnitPrice));
                        OnPropertyChanged(nameof(Price)); // Price property eka update karanna
                    }
                }
            }

            public string StockStatus
            {
                get => _stockStatus;
                set
                {
                    if (_stockStatus != value)
                    {
                        _stockStatus = value;
                        OnPropertyChanged(nameof(StockStatus));
                        OnPropertyChanged(nameof(Status));
                        OnPropertyChanged(nameof(StatusText));
                    }
                }
            }

            // Read-only properties
            public string Price => $"Rs. {UnitPrice:N2}";
            public string StatusText => StockStatus;
            public string Status => StockStatus;

            // INotifyPropertyChanged implementation
            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
