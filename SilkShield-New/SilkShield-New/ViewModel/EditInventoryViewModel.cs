using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using SilkShield_New.Data;
using SilkShield_New.Model;
using SilkShield_New.View; // Required to see MainWindow
using SilkShield_New.ViewModel; // Required to see InventoryViewModel

namespace SilkShield_New.ViewModel
{
    public class EditInventoryViewModel : INotifyPropertyChanged
    {
        private readonly InvoiceRepository _repo;

        // Flattened properties for binding
        private int _itemID;
        public int ItemID
        {
            get => _itemID;
            set { _itemID = value; OnPropertyChanged(nameof(ItemID)); }
        }

        private string _itemName;
        public string ItemName
        {
            get => _itemName;
            set { _itemName = value; OnPropertyChanged(nameof(ItemName)); }
        }

        private string _selectedCategory;
        public string SelectedCategory
        {
            get => _selectedCategory;
            set { _selectedCategory = value; OnPropertyChanged(nameof(SelectedCategory)); }
        }

        private string _material;
        public string Material
        {
            get => _material;
            set { _material = value; OnPropertyChanged(nameof(Material)); }
        }

        private string _selectedMeasuringUnit;
        public string SelectedMeasuringUnit
        {
            get => _selectedMeasuringUnit;
            set { _selectedMeasuringUnit = value; OnPropertyChanged(nameof(SelectedMeasuringUnit)); }
        }

        private string _unitPrice;
        public string UnitPrice
        {
            get => _unitPrice;
            set { _unitPrice = value; OnPropertyChanged(nameof(UnitPrice)); }
        }

        private string _selectedStockStatus;
        public string SelectedStockStatus
        {
            get => _selectedStockStatus;
            set { _selectedStockStatus = value; OnPropertyChanged(nameof(SelectedStockStatus)); }
        }


            public ObservableCollection<string> CategoryList { get; } = new ObservableCollection<string> { "Fabric", "Blinds", "Accessories" };
            public ObservableCollection<string> MeasuringUnitList { get; } = new ObservableCollection<string> { "Meter (m)", "Square Meter (sqm)", "Square Feet (sft)", "Feet (ft)", "Pieces", "Roll" };
            public ObservableCollection<string> StockStatusList { get; } = new ObservableCollection<string> { "In Stock","Low Stock", "Out of Stock" };

           

        // Commands
        public ICommand UpdateCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand CancelCommand { get; }

        public EditInventoryViewModel(InventoryItem selectedItem)
        {
            _repo = new InvoiceRepository();

            if (selectedItem != null)
            {
                ItemID = selectedItem.ItemID;
                ItemName = selectedItem.ItemName;
                SelectedCategory = selectedItem.Category;
                Material = selectedItem.Material;
                SelectedMeasuringUnit = selectedItem.MeasuringUnit;
                UnitPrice = selectedItem.UnitPrice.ToString(CultureInfo.InvariantCulture);
                SelectedStockStatus = selectedItem.StockStatus;
            }

            UpdateCommand = new RelayCommand(_ => Save());
            ClearCommand = new RelayCommand(_ => Clear());
            CancelCommand = new RelayCommand(_ => Cancel());
        }



        private void Save()
        {
            if (string.IsNullOrWhiteSpace(ItemName))
            {
                MessageBox.Show("Item Name is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!double.TryParse(UnitPrice, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsedPrice))
            {
                MessageBox.Show("Unit Price must be a valid number.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var item = new InventoryItem
                {
                    ItemID = this.ItemID,
                    ItemName = this.ItemName ?? string.Empty,
                    Category = this.SelectedCategory ?? string.Empty,
                    Material = this.Material ?? string.Empty,
                    MeasuringUnit = this.SelectedMeasuringUnit ?? string.Empty,
                    UnitPrice = parsedPrice,
                    StockStatus = this.SelectedStockStatus ?? string.Empty
                };

                bool ok = _repo.UpdateInventoryItem(item);
                if (ok)
                {
                    MessageBox.Show("Inventory item updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    // 1. Switch the UI back to the Inventory list first
                    CloseWindow();

                    // 2. Now that the Inventory view is active, refresh the data
                    RefreshParentUI();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating item: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        
        private void Clear()
        {
            ItemName = string.Empty;
            SelectedCategory = string.Empty;
            Material = string.Empty;
            SelectedMeasuringUnit = string.Empty;
            UnitPrice = string.Empty;
            SelectedStockStatus = string.Empty;
        }

        private void Cancel() => CloseWindow();

        private void CloseWindow()
        {
            if (Application.Current.MainWindow is SilkShield_New.View.MainWindow mainWin)
            {

                mainWin.Inventory_Click(null, null);
            }
        }

        private void RefreshParentUI()
        {
            // Specifically targeting your MainWindow to call the refresh helper
            if (Application.Current.MainWindow is SilkShield_New.View.MainWindow mainWin)
            {
                mainWin.RefreshInventoryIfActive();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}