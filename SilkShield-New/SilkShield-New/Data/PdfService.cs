using System.Collections.Generic;
using System.Data.SQLite;

namespace SilkShield_New.Data
{
    public class ProductDataService
    {
        private readonly DatabaseHelper _dbHelper;

        public ProductDataService()
        {
            _dbHelper = new DatabaseHelper();
        }

        // Example method to get all products
        public List<Product> GetAllProducts()
        {
            var products = new List<Product>();
            string query = "SELECT Material, UnitOfMeasure, UnitPrice FROM inventory";

            using (var connection = _dbHelper.GetConnection())
            {
                connection.Open();
                using (var command = new SQLiteCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            products.Add(new Product
                            {
                                Material = reader["Material"].ToString(),
                                UnitOfMeasure = reader["UnitOfMeasure"].ToString(),
                                UnitPrice = (double)reader["UnitPrice"]
                            });
                        }
                    }
                }
            }
            return products;
        }
    }
}