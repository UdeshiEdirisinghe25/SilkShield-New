using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace SilkShield_New.Model
{
    public class Invoice : INotifyPropertyChanged
    {
        public string InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string CustomerName { get; set; }
        public string Location { get; set; }
        public string BuildingType { get; set; }
        public string CurtainLayerType { get; set; }
        public string CurtainStyle { get; set; }
        public bool PelmetBoard { get; set; }
        public bool Motorized { get; set; }
        public string PaymentMethod { get; set; }
        public double TransportLaborCost { get; set; }
        public double Discount { get; set; }
        public decimal TotalAmount { get; set; }

        // New: persist checkbox per invoice
        public bool IncludeDetailsPage { get; set; } = true;

        public ObservableCollection<InvoiceItem> Items { get; set; } = new ObservableCollection<InvoiceItem>();

        public double GrandTotal
        {
            get
            {
                double subtotal = Items?.Sum(i => i.Total) ?? 0;
                double discountAmount = subtotal * (Discount / 100);
                return subtotal + TransportLaborCost - discountAmount;

            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}