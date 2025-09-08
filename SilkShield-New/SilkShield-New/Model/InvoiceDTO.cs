using System;

namespace SilkShield_New.Model
{
    public class InvoiceDTO
    {
        public int InvoiceId { get; set; }
        public string Customer { get; set; }
        public DateTime InvoiceDate { get; set; }
        public double TotalAmount { get; set; }
    }
}
