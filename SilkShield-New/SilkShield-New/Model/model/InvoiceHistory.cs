using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilkShield_New.Model
{
    public class InvoiceHistory
    {
        public string InvoiceNumber { get; set; } // මෙය එකතු කරන්න
        public int InvoiceId { get; set; }
        public string Customer { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
