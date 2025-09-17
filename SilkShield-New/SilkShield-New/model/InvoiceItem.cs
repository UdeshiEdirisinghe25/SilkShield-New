using System.ComponentModel;
using System.Collections.ObjectModel;
using SilkShield_New.Data;

namespace SilkShield_New.Model
{
    public class InvoiceItem : INotifyPropertyChanged
    {
        private string _itemName;
        private string _selectedMaterial;
        private string _measuringUnit;
        private double _quantity = 1.0; 
        private double _unitPrice;
        private double _total;

        private ObservableCollection<string> _availableMaterials;

        public string ItemName
        {
            get => _itemName;
            set
            {
                if (_itemName != value)
                {
                    _itemName = value;
                    OnPropertyChanged(nameof(ItemName));
                    LoadAvailableMaterials();
                }
            }
        }

        public string SelectedMaterial
        {
            get => _selectedMaterial;
            set { _selectedMaterial = value; OnPropertyChanged(nameof(SelectedMaterial)); }
        }

        public string MeasuringUnit
        {
            get => _measuringUnit;
            set { _measuringUnit = value; OnPropertyChanged(nameof(MeasuringUnit)); }
        }

        public double Quantity
        {
            get => _quantity;
            set { _quantity = value; CalculateTotal(); OnPropertyChanged(nameof(Quantity)); }
        }

        public double UnitPrice
        {
            get => _unitPrice;
            set { _unitPrice = value; CalculateTotal(); OnPropertyChanged(nameof(UnitPrice)); }
        }

        public double Total
        {
            get => _total;
            // The private setter is correct here.
            private set { _total = value; OnPropertyChanged(nameof(Total)); }
        }

        public ObservableCollection<string> AvailableMaterials
        {
            get => _availableMaterials;
            set
            {
                _availableMaterials = value;
                OnPropertyChanged(nameof(AvailableMaterials));
            }
        }

        // --- FIXED ACCESS LEVEL ---
        public void CalculateTotal() => Total = Quantity * UnitPrice;

        private void LoadAvailableMaterials()
        {
            if (!string.IsNullOrEmpty(ItemName))
            {
                var dataService = new InvoiceDataService();
                AvailableMaterials = new ObservableCollection<string>(dataService.GetMaterialsByItemName(ItemName));
            }
            else
            {
                AvailableMaterials = new ObservableCollection<string>();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}