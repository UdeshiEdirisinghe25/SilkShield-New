using System.ComponentModel;

namespace SilkShield_New.Model
{
    public class InvoiceItem : INotifyPropertyChanged
    {
        private string _itemName;
        private double _quantity;
        private double _unitPrice;
        private double _total;

        public string ItemName
        {
            get => _itemName;
            set
            {
                _itemName = value;
                OnPropertyChanged(nameof(ItemName));
            }
        }

        public double Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    Total = _quantity * _unitPrice;
                    OnPropertyChanged(nameof(Quantity));
                }
            }
        }

        public double UnitPrice
        {
            get => _unitPrice;
            set
            {
                if (_unitPrice != value)
                {
                    _unitPrice = value;
                    Total = _quantity * _unitPrice;
                    OnPropertyChanged(nameof(UnitPrice));
                }
            }
        }

        public double Total
        {
            get => _total;
            set
            {
                _total = value;
                OnPropertyChanged(nameof(Total));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}