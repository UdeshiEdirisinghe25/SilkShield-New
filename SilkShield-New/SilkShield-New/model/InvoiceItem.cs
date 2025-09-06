using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SilkShield_New.Model
{
    public class InvoiceItem : INotifyPropertyChanged
    {
        private string _itemName;
        private double _quantity;
        private double _unitPrice;
        private double _total;
        private string _material;
        private string _measuringUnit; // New measuring unit property

        public string ItemName
        {
            get => _itemName;
            set
            {
                if (_itemName == value) return;
                _itemName = value;
                OnPropertyChanged(nameof(ItemName));
            }
        }

        public double Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity == value) return;
                _quantity = value;
                CalculateTotal();
                OnPropertyChanged(nameof(Quantity));
            }
        }

        public double UnitPrice
        {
            get => _unitPrice;
            set
            {
                if (_unitPrice == value) return;
                _unitPrice = value;
                CalculateTotal();
                OnPropertyChanged(nameof(UnitPrice));
            }
        }

        public double Total
        {
            get => _total;
            private set
            {
                if (_total == value) return;
                _total = value;
                OnPropertyChanged(nameof(Total));
            }
        }

        public string Material
        {
            get => _material;
            set
            {
                if (_material == value) return;
                _material = value;
                OnPropertyChanged(nameof(Material));
            }
        }

        public string MeasuringUnit
        {
            get => _measuringUnit;
            set
            {
                if (_measuringUnit == value) return;
                _measuringUnit = value;
                OnPropertyChanged(nameof(MeasuringUnit));
            }
        }

        private void CalculateTotal()
        {
            Total = Quantity * UnitPrice;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
