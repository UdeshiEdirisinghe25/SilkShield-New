using System.ComponentModel;

namespace SilkShield_New.Model
{
    /// <summary>
    /// A simple model for an item on the invoice. This class
    /// implements INotifyPropertyChanged to enable data binding.
    /// </summary>
    public class InvoiceItem : INotifyPropertyChanged
    {
        private string _description;
        private double _quantity;
        private double _unitPrice;
        private double _total;
        private string _itemName;
        private string _measuringUnit;

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

        public string Description
        {
            get => _description;
            set
            {
                if (_description == value) return;
                _description = value;
                OnPropertyChanged(nameof(Description));
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

        public double Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity == value) return;
                _quantity = value;
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

        /// <summary>
        /// Calculates the total for this invoice item based on Quantity and UnitPrice.
        /// </summary>
        public void CalculateTotal()
        {
            Total = Quantity * UnitPrice;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
