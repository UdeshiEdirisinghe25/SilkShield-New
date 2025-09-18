using System;
using System.Collections.Generic;
using System.Data.SQLite;
using SilkShield_New.Model;

namespace SilkShield_New.Data
{
    public class CustomerDAL
    {
        private readonly DatabaseHelper _dbHelper;

        public CustomerDAL()
        {
            _dbHelper = new DatabaseHelper();
        }

        public bool InsertCustomer(Customer customer)
        {
            using (SQLiteConnection connection = _dbHelper.GetConnection())
            {
                try
                {
                    connection.Open();

                    string sql = @"
                INSERT INTO customer_details (
                    CustomerType, VisitedStatus, CustomerName, Address, 
                    ProjectConfirmation, Email, PhoneNumber, QuotationStatus,
                    Property_Details, Project_Start_Date, Expected_Dateof_Completion, Special_Preferances, Notes
                ) VALUES (
                    @CustomerType, @VisitedStatus, @CustomerName, @Address,
                    @ProjectConfirmation, @Email, @PhoneNumber, @QuotationStatus,
                    @Property_Details, @Project_Start_Date, @Expected_Dateof_Completion, @Special_Preferances, @Notes
                )";

                    using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@CustomerType", customer.CustomerType);
                        command.Parameters.AddWithValue("@VisitedStatus", customer.VisitedStatus);
                        command.Parameters.AddWithValue("@CustomerName", customer.CustomerName);
                        command.Parameters.AddWithValue("@Address", customer.Address);
                        command.Parameters.AddWithValue("@ProjectConfirmation", customer.ProjectConfirmation);
                        command.Parameters.AddWithValue("@Email", customer.Email);
                        command.Parameters.AddWithValue("@PhoneNumber", customer.PhoneNumber);
                        command.Parameters.AddWithValue("@QuotationStatus", customer.QuotationStatus);
                        command.Parameters.AddWithValue("@Property_Details", customer.Property_Details);
                        command.Parameters.AddWithValue("@Project_Start_Date", customer.Project_Start_Date);
                        command.Parameters.AddWithValue("@Expected_Dateof_Completion", customer.Expected_Dateof_Completion);
                        command.Parameters.AddWithValue("@Special_Preferances", customer.Special_Preferances);
                        command.Parameters.AddWithValue("@Notes", customer.Notes);

                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error inserting customer: {ex.Message}");
                    return false;
                }
            }
        }


        public List<Customer> GetAllCustomers()
        {
            List<Customer> customers = new List<Customer>();
            string sql = "SELECT * FROM customer_details";

            using (SQLiteConnection connection = _dbHelper.GetConnection())
            {
                try
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                customers.Add(new Customer
                                {
                                    CustomerID = Convert.ToInt32(reader["CustomerID"]),
                                    CustomerName = reader["CustomerName"].ToString(),
                                    CustomerType = reader["CustomerType"].ToString(),
                                    PhoneNumber = reader["PhoneNumber"].ToString(),
                                    Email = reader["Email"].ToString(),
                                    Address = reader["Address"].ToString(),
                                    VisitedStatus = reader["VisitedStatus"].ToString(),
                                    ProjectConfirmation = reader["ProjectConfirmation"].ToString(),
                                    QuotationStatus = reader["QuotationStatus"].ToString(),
                                    Property_Details = reader["Property_Details"].ToString(),
                                    Project_Start_Date = reader["Project_Start_Date"].ToString(),
                                    Expected_Dateof_Completion = reader["Expected_Dateof_Completion"].ToString(),
                                    Special_Preferances = reader["Special_Preferances"].ToString(),
                                    Notes = reader["Notes"].ToString()
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching customers: {ex.Message}");
                }
            }

            return customers;
        }

        public void DeleteCustomer(int customerId)
        {
            using (SQLiteConnection con = _dbHelper.GetConnection())
            {
                try
                {
                    con.Open();
                    string query = "DELETE FROM customer_details WHERE CustomerID = @id";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@id", customerId);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting customer: {ex.Message}");
                    throw;
                }
            }
        }
        public ProjectSummary GetProjectSummary()
        {
            var summary = new ProjectSummary();
            string connectionString = _dbHelper.GetConnection().ConnectionString;

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // 1. Pending Projects ගණන ලබා ගැනීම
                string pendingQuery = "SELECT COUNT(*) FROM customer_details WHERE ProjectConfirmation = 'Pending'";
                using (var cmd = new SQLiteCommand(pendingQuery, connection))
                {
                    summary.Pending = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // 2. Ongoing සහ Upcoming Projects ගණන C# කේතය හරහා ගණනය කිරීම
                // මෙය සිදු කරන්නේ date format ගැටලුව මගහැරීමටයි.
                string confirmedProjectsQuery = "SELECT Project_Start_Date, Expected_Dateof_Completion FROM customer_details WHERE ProjectConfirmation = 'Confirmed'";

                int ongoingCount = 0;
                int upcomingCount = 0;
                DateTime today = DateTime.Now.Date;

                using (var cmd = new SQLiteCommand(confirmedProjectsQuery, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string startDateStr = reader["Project_Start_Date"].ToString();
                            string endDateStr = reader["Expected_Dateof_Completion"].ToString();

                            if (DateTime.TryParse(startDateStr, out DateTime startDate) && DateTime.TryParse(endDateStr, out DateTime endDate))
                            {
                                if (startDate.Date <= today && endDate.Date >= today)
                                {
                                    ongoingCount++;
                                }
                                else if (startDate.Date > today)
                                {
                                    upcomingCount++;
                                }
                            }
                        }
                    }
                }

                summary.Ongoing = ongoingCount;
                summary.Upcoming = upcomingCount;
            }

            return summary;
        }

    }
}
