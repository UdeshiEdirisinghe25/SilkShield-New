using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilkShield_New.Model
{
    public class InvoiceHistoryModel
    {
        public int InvoiceId { get; set; }
        public string Customer {  get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }

    }
}
