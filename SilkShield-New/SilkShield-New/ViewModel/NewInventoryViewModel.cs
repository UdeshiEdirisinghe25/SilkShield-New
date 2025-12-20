using SilkShield_New.Data;
using SilkShield_New.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SQLite;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace SilkShield_New.ViewModels
{
    public class NewInventoryViewModel : INotifyPropertyChanged
    {
        // Database helper
        private readonly DatabaseHelper _dbHelper;

        // Properties for textboxes
        private string _itemName;
        public string ItemName
        {
            get => _itemName;
            set { _itemName = value; OnPropertyChanged(); }
        }

        private string _material;
        public string Material
        {
            get => _material;
            set { _material = value; OnPropertyChanged(); }
        }

        private string _unitPrice;
        public string UnitPrice
        {
            get => _unitPrice;
            set { _unitPrice = value; OnPropertyChanged(); }
        }

        // Properties for ComboBoxes
        public ObservableCollection<string> CategoryList { get; set; }
        private string _selectedCategory;
        public string SelectedCategory
        {
            get => _selectedCategory;
            set { _selectedCategory = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> MeasuringUnitList { get; set; }
        private string _selectedMeasuringUnit;
        public string SelectedMeasuringUnit
        {
            get => _selectedMeasuringUnit;
            set { _selectedMeasuringUnit = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> StockStatusList { get; set; }
        private string _selectedStockStatus;
        public string SelectedStockStatus
        {
            get => _selectedStockStatus;
            set { _selectedStockStatus = value; OnPropertyChanged(); }
        }

        // Commands
        public RelayCommand SaveCommand { get; }
        public RelayCommand ClearCommand { get; }
        public RelayCommand CancelCommand { get; }

        // Constructor
        public NewInventoryViewModel()
        {
            _dbHelper = new DatabaseHelper(); // Initialize database helper

            // Initialize ComboBox lists
            CategoryList = new ObservableCollection<string> { "Fabric", "Blinds", "Accessories" };
            MeasuringUnitList = new ObservableCollection<string> { "Meters (m)", "Feet (ft)", "Pieces", "Square Feet (sft)", "Square Meter", "Roll" };
            StockStatusList = new ObservableCollection<string> { "In Stock", "Out of Stock" };

            // Initialize commands
            SaveCommand = new RelayCommand(SaveInventoryItem);
            ClearCommand = new RelayCommand(ClearForm);
            CancelCommand = new RelayCommand(CloseWindow);
        }

        // Save inventory item to SQLite
        private void SaveInventoryItem(object obj)
        {
            try
            {
                using (var conn = _dbHelper.GetConnection())
                {
                    conn.Open();

                    string sql = "INSERT INTO inventory (ItemName, Category, Material, MeasuringUnit, UnitPrice, StockStatus) " +
                                 "VALUES (@ItemName, @Category, @Material, @MeasuringUnit, @UnitPrice, @Stock);";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@ItemName", ItemName);
                        cmd.Parameters.AddWithValue("@Category", SelectedCategory);
                        cmd.Parameters.AddWithValue("@Material", Material);
                        cmd.Parameters.AddWithValue("@MeasuringUnit", SelectedMeasuringUnit);
                        cmd.Parameters.AddWithValue("@UnitPrice", UnitPrice);
                        cmd.Parameters.AddWithValue("@Stock", SelectedStockStatus);

                        cmd.ExecuteNonQuery();
                    }
                }

                // Clear fields after saving
                ClearForm(null);

                MessageBox.Show("Inventory item saved successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving inventory: " + ex.Message,"Error");
            }
        }

        // Clear all fields
        private void ClearForm(object obj)
        {
            ItemName = string.Empty;
            Material = string.Empty;
            UnitPrice = string.Empty;
            SelectedCategory = null;
            SelectedMeasuringUnit = null;
            SelectedStockStatus = null;
        }

        // Close the window
        private void CloseWindow(object obj)
        {
            Application.Current.Windows[0]?.Close();
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
