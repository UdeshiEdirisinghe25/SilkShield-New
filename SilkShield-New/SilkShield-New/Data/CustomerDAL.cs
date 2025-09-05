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
                            ProjectConfirmation, Email, PhoneNumber, QuotationStatus
                        ) VALUES (
                            @CustomerType, @VisitedStatus, @CustomerName, @Address,
                            @ProjectConfirmation, @Email, @PhoneNumber, @QuotationStatus
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
                                    CustomerID = Convert.ToInt32(reader["CustomerID"]), // <-- FIXED
                                    CustomerName = reader["CustomerName"].ToString(),
                                    CustomerType = reader["CustomerType"].ToString(),
                                    PhoneNumber = reader["PhoneNumber"].ToString(),
                                    Email = reader["Email"].ToString(),
                                    Address = reader["Address"].ToString(),
                                    VisitedStatus = reader["VisitedStatus"].ToString(),
                                    ProjectConfirmation = reader["ProjectConfirmation"].ToString(),
                                    QuotationStatus = reader["QuotationStatus"].ToString()
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


        /// <param name="customerId">The ID of the customer to delete.</param>
        /// <returns>True if the customer was successfully deleted, otherwise false.</returns>

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
                    throw; // optional: rethrow to show in UI
                }
            }
        }

    }
}
