using System.Collections.Generic;
using System.Data.SQLite;
using SilkShield_New.Data; 

public class InvoiceDataService
{
    private readonly DatabaseHelper _dbHelper;

    //public InvoiceDataService()
    //{
    //    _dbHelper = DatabaseHelper.Instance; // Singleton Instance 
    //}

    public List<string> GetMaterialsByItemName(string itemName)
    {
        var materials = new List<string>();
        string query = "SELECT DISTINCT Material FROM inventory WHERE ItemName = @ItemName";

        using (var connection = _dbHelper.GetConnection())
        {
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ItemName", itemName);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        materials.Add(reader["Material"].ToString());
                    }
                }
            }
        }
        return materials;
    }
}