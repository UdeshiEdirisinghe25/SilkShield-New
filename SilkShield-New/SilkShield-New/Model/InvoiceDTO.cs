using System;

namespace SilkShield_New.Model
{
    // InvoiceDTO.cs file එක ඇතුළට
    public class InvoiceDTO
    {
        public int InvoiceId { get; set; }
        public string Customer { get; set; }

        // මෙම property එක DashboardViewModel එකේ `RecentInvoices` list එකේ CustomerName එකට bind වෙනවා.
        public string CustomerName => Customer;

        public DateTime InvoiceDate { get; set; }
        public double TotalAmount { get; set; }

        // මෙම property එක DashboardViewModel එකේ `RecentInvoices` list එකේ Amount එකට bind වෙනවා.
        public double Amount => TotalAmount;
    }
}