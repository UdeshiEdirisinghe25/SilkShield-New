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
        }

        public SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(_connectionString);
        }
    }
}
