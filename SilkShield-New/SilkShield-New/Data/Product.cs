namespace SilkShield_New.Data
{
    /// <summary>
    /// A simple data model representing a product from the inventory.
    /// </summary>
    public class Product
    {
        public string Material { get; set; }
        public string UnitOfMeasure { get; set; }
        public double UnitPrice { get; set; }
    }
}
