using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SilkShield_New.Model
{
    public class Invoice : INotifyPropertyChanged
    {
        private int _invoiceId;
        private DateTime _invoiceDate;
        private string _customer;
        private string _buildingType;
        private string _pelmetBoard;
        private string _motorized;
        private string _paymentMethod;
        private double _discount;
        private double _totalAmount;
        private ObservableCollection<InvoiceItem> _items;

        public int InvoiceId
        {
            get => _invoiceId;
            set
            {
                _invoiceId = value;
                OnPropertyChanged(nameof(InvoiceId));
            }
        }

        public DateTime InvoiceDate
        {
            get => _invoiceDate;
            set
            {
                _invoiceDate = value;
                OnPropertyChanged(nameof(InvoiceDate));
            }
        }

        public string Customer
        {
            get => _customer;
            set
            {
                _customer = value;
                OnPropertyChanged(nameof(Customer));
            }
        }

        public string BuildingType
        {
            get => _buildingType;
            set
            {
                _buildingType = value;
                OnPropertyChanged(nameof(BuildingType));
            }
        }

        public string PelmetBoard
        {
            get => _pelmetBoard;
            set
            {
                _pelmetBoard = value;
                OnPropertyChanged(nameof(PelmetBoard));
            }
        }

        public string Motorized
        {
            get => _motorized;
            set
            {
                _motorized = value;
                OnPropertyChanged(nameof(Motorized));
            }
        }

        public string PaymentMethod
        {
            get => _paymentMethod;
            set
            {
                _paymentMethod = value;
                OnPropertyChanged(nameof(PaymentMethod));
            }
        }

        public double Discount
        {
            get => _discount;
            set
            {
                _discount = value;
                CalculateTotalAmount();
                OnPropertyChanged(nameof(Discount));
            }
        }

        public double TotalAmount
        {
            get => _totalAmount;
            set
            {
                _totalAmount = value;
                OnPropertyChanged(nameof(TotalAmount));
            }
        }

        public ObservableCollection<InvoiceItem> Items
        {
            get => _items;
            set
            {
                _items = value;
                OnPropertyChanged(nameof(Items));
            }
        }

        public Invoice()
        {
            Items = new ObservableCollection<InvoiceItem>();
            Items.CollectionChanged += (sender, e) => CalculateTotalAmount();
        }

        private void CalculateTotalAmount()
        {
            double subtotal = 0;
            foreach (var item in Items)
            {
                subtotal += item.Total;
            }
            TotalAmount = subtotal - Discount;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
