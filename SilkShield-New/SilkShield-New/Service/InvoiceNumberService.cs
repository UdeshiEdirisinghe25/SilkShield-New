using System;
using System.Data.SQLite;
using SilkShield_New.Data;

namespace SilkShield_New.Service
{
    public class InvoiceNumberService
    {
        private readonly DatabaseHelper _dbHelper;

        public InvoiceNumberService()
        {
            _dbHelper = new DatabaseHelper();
        }

        public string GetNewInvoiceNumber(DateTime invoiceDate)
        {
            int currentYear = invoiceDate.Year % 100;
            int currentQuarter = (invoiceDate.Month - 1) / 3 + 1;

            int lastNumber = GetLastNumberFromDb(currentYear, currentQuarter);
            int newNumber = lastNumber + 1;

            return $"NR{currentYear:D2}-{currentQuarter:D2}-{newNumber:D2}";
        }

        private int GetLastNumberFromDb(int year, int quarter)
        {
            int lastNumber = 0;
            using (var connection = _dbHelper.GetConnection())
            {
                connection.Open();

                string checkQuery = "SELECT COUNT(*) FROM InvoiceCounter WHERE Year = @Year AND Quarter = @Quarter";
                using (var checkCmd = new SQLiteCommand(checkQuery, connection))
                {
                    checkCmd.Parameters.AddWithValue("@Year", year);
                    checkCmd.Parameters.AddWithValue("@Quarter", quarter);
                    long count = (long)checkCmd.ExecuteScalar();
                    if (count > 0)
                    {
                        string selectQuery = "SELECT LastNumber FROM InvoiceCounter WHERE Year = @Year AND Quarter = @Quarter";
                        using (var selectCmd = new SQLiteCommand(selectQuery, connection))
                        {
                            selectCmd.Parameters.AddWithValue("@Year", year);
                            selectCmd.Parameters.AddWithValue("@Quarter", quarter);
                            var result = selectCmd.ExecuteScalar();
                            if (result != null && result != DBNull.Value)
                            {
                                lastNumber = Convert.ToInt32(result);
                            }
                        }
                    }
                    else
                    {
                        string insertQuery = "INSERT INTO InvoiceCounter (Year, Quarter, LastNumber) VALUES (@Year, @Quarter, @LastNumber)";
                        using (var insertCmd = new SQLiteCommand(insertQuery, connection))
                        {
                            insertCmd.Parameters.AddWithValue("@Year", year);
                            insertCmd.Parameters.AddWithValue("@Quarter", quarter);
                            insertCmd.Parameters.AddWithValue("@LastNumber", 0);
                            insertCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            return lastNumber;
        }

        public void IncrementInvoiceCounter(string invoiceNumber)
        {
            string[] parts = invoiceNumber.Split('-');
            if (parts.Length == 3 && parts[0].StartsWith("NR"))
            {
                int year = int.Parse(parts[0].Substring(2));
                int quarter = int.Parse(parts[1]);
                int newNumber = int.Parse(parts[2]);

                using (var connection = _dbHelper.GetConnection())
                {
                    connection.Open();
                    string updateQuery = "UPDATE InvoiceCounter SET LastNumber = @LastNumber WHERE Year = @Year AND Quarter = @Quarter";
                    using (var updateCmd = new SQLiteCommand(updateQuery, connection))
                    {
                        updateCmd.Parameters.AddWithValue("@LastNumber", newNumber);
                        updateCmd.Parameters.AddWithValue("@Year", year);
                        updateCmd.Parameters.AddWithValue("@Quarter", quarter);
                        updateCmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}