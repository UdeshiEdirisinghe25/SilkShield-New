using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using System.Data.SQLite;
using SilkShield_New.Data;
using System.Windows;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SilkShield_New.ViewModel
{
    // A simple model for an item on an invoice.
    public class InvoiceItem : INotifyPropertyChanged
    {
        private string _description;
        private double _quantity;
        private double _unitPrice;
        private double _total;
        private string _itemName;
        private string _measuringUnit;
        private ObservableCollection<string> _availableMaterials;
        private string _selectedMaterial;

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

        public ObservableCollection<string> AvailableMaterials
        {
            get => _availableMaterials;
            set
            {
                _availableMaterials = value;
                OnPropertyChanged(nameof(AvailableMaterials));
            }
        }

        public string SelectedMaterial
        {
            get => _selectedMaterial;
            set
            {
                _selectedMaterial = value;
                OnPropertyChanged(nameof(SelectedMaterial));
            }
        }

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

    public class NewInvoice1ViewModel : INotifyPropertyChanged
    {
        #region Private Fields
        private string _invoiceNumber;
        private DateTime _invoiceDate;
        private string _customerName;
        private string _buildingType;
        private string _curtainLayerType;
        private string _curtainStyle;
        private string _paymentMethod;
        private ObservableCollection<InvoiceItem> _items;
        private double _grandTotal;
        private string _discountText;
        private string _transportLaborCostText;
        private double _discountPercentage;
        private double _transportLaborCost;
        private bool _isPelmetBoardChecked;
        private bool _isMotorizedChecked;
        private ObservableCollection<string> _availableItems;
        private readonly DatabaseHelper _dbHelper;
        #endregion

        #region Public Properties
        public string InvoiceNumber
        {
            get => _invoiceNumber;
            set
            {
                _invoiceNumber = value;
                OnPropertyChanged(nameof(InvoiceNumber));
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

        public string CustomerName
        {
            get => _customerName;
            set
            {
                _customerName = value;
                OnPropertyChanged(nameof(CustomerName));
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

        public string CurtainLayerType
        {
            get => _curtainLayerType;
            set
            {
                _curtainLayerType = value;
                OnPropertyChanged(nameof(CurtainLayerType));
            }
        }

        public string CurtainStyle
        {
            get => _curtainStyle;
            set
            {
                _curtainStyle = value;
                OnPropertyChanged(nameof(CurtainStyle));
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

        public ObservableCollection<InvoiceItem> Items
        {
            get => _items;
            set
            {
                _items = value;
                OnPropertyChanged(nameof(Items));
            }
        }

        public double GrandTotal
        {
            get => _grandTotal;
            set
            {
                _grandTotal = value;
                OnPropertyChanged(nameof(GrandTotal));
            }
        }

        public string TransportLaborCostText
        {
            get => _transportLaborCostText;
            set
            {
                if (_transportLaborCostText == value) return;
                _transportLaborCostText = value;
                if (double.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out double result))
                {
                    _transportLaborCost = Math.Max(0, result);
                }
                else
                {
                    _transportLaborCost = 0;
                }
                CalculateGrandTotal();
                OnPropertyChanged(nameof(TransportLaborCostText));
            }
        }

        public string DiscountText
        {
            get => _discountText;
            set
            {
                if (_discountText == value) return;
                _discountText = value;
                if (double.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out double result))
                {
                    _discountPercentage = Math.Max(0, result);
                }
                else
                {
                    _discountPercentage = 0;
                }
                CalculateGrandTotal();
                OnPropertyChanged(nameof(DiscountText));
            }
        }

        public bool IsPelmetBoardChecked
        {
            get => _isPelmetBoardChecked;
            set
            {
                _isPelmetBoardChecked = value;
                OnPropertyChanged(nameof(IsPelmetBoardChecked));
            }
        }

        public bool IsMotorizedChecked
        {
            get => _isMotorizedChecked;
            set
            {
                _isMotorizedChecked = value;
                OnPropertyChanged(nameof(IsMotorizedChecked));
            }
        }

        public ObservableCollection<string> AvailableItems
        {
            get => _availableItems;
            set
            {
                _availableItems = value;
                OnPropertyChanged(nameof(AvailableItems));
            }
        }
        #endregion

        #region ICommands
        public ICommand AddItemCommand { get; }
        public ICommand DeleteItemCommand { get; }
        public ICommand CreateInvoiceCommand { get; }
        public ICommand ClearFormCommand { get; }
        #endregion

        #region Constructor
        public NewInvoice1ViewModel()
        {
            _dbHelper = new DatabaseHelper();

            AddItemCommand = new RelayCommand(AddItem);
            DeleteItemCommand = new RelayCommand(DeleteItem);
            CreateInvoiceCommand = new RelayCommand(CreateInvoice);
            ClearFormCommand = new RelayCommand(ClearForm);

            Items = new ObservableCollection<InvoiceItem>();
            Items.CollectionChanged += (sender, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (InvoiceItem item in e.NewItems)
                    {
                        item.PropertyChanged += OnItemPropertyChanged;
                    }
                }
                if (e.OldItems != null)
                {
                    foreach (InvoiceItem item in e.OldItems)
                    {
                        item.PropertyChanged -= OnItemPropertyChanged;
                    }
                }
                CalculateGrandTotal();
            };

            LoadItemNamesFromDatabase();
            ClearForm(null);
        }
        #endregion

        #region Private Methods

        private async void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is InvoiceItem item)
            {
                if (e.PropertyName == nameof(InvoiceItem.ItemName))
                {
                    // Load the relevant materials for the new item name.
                    var materials = await LoadMaterialsByItemName(item.ItemName);
                    item.AvailableMaterials = new ObservableCollection<string>(materials);
                    
                    // Load the relevant measuring unit and set Unit Price to 0.
                    await UpdateMeasuringUnitAndResetPrice(item);
                    
                    // Set this to null to load the Unit Price when a Material is selected.
                    item.SelectedMaterial = null;
                }

                if (e.PropertyName == nameof(InvoiceItem.SelectedMaterial))
                {
                    // Only load the Unit Price when a Material is selected.
                    await UpdateUnitPrice(item);
                }

                // Recalculate the total when Quantity or Unit Price changes.
                if (e.PropertyName == nameof(InvoiceItem.Quantity) || e.PropertyName == nameof(InvoiceItem.UnitPrice))
                {
                    item.CalculateTotal();
                    CalculateGrandTotal();
                }
            }
        }
        
        // Method to load the measuring unit based on the item name and set Unit Price to 0.
        private async Task UpdateMeasuringUnitAndResetPrice(InvoiceItem item)
        {
            if (string.IsNullOrEmpty(item.ItemName))
            {
                item.MeasuringUnit = string.Empty;
                item.UnitPrice = 0;
                return;
            }

            string query = "SELECT MeasuringUnit FROM inventory WHERE ItemName = @ItemName LIMIT 1";
            try
            {
                using (var connection = _dbHelper.GetConnection() as SQLiteConnection)
                {
                    await connection.OpenAsync();
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ItemName", item.ItemName);
                        var result = await command.ExecuteScalarAsync();
                        item.MeasuringUnit = result?.ToString() ?? string.Empty;
                        item.UnitPrice = 0; // Unit Price remains 0.
                        item.CalculateTotal();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading MeasuringUnit: {ex.Message}", "Database Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        // Method to load the Unit Price based on the selected material.
        private async Task UpdateUnitPrice(InvoiceItem item)
        {
            if (string.IsNullOrEmpty(item.ItemName) || string.IsNullOrEmpty(item.SelectedMaterial))
            {
                item.UnitPrice = 0;
                return;
            }
            
            string query = "SELECT UnitPrice FROM inventory WHERE ItemName = @ItemName AND Material = @Material";
            
            try
            {
                using (var connection = _dbHelper.GetConnection() as SQLiteConnection)
                {
                    await connection.OpenAsync();
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ItemName", item.ItemName);
                        command.Parameters.AddWithValue("@Material", item.SelectedMaterial);
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                item.UnitPrice = reader.GetDouble(0);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading UnitPrice: {ex.Message}", "Database Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CalculateGrandTotal()
        {
            double subTotal = Items?.Sum(item => item.Total) ?? 0;
            double totalBeforeDiscount = subTotal + _transportLaborCost;
            double discountAmount = totalBeforeDiscount * (_discountPercentage / 100.0);
            GrandTotal = Math.Max(0, totalBeforeDiscount - discountAmount);
        }

        private void AddItem(object obj)
        {
            var newItem = new InvoiceItem();
            Items.Add(newItem);
        }

        private void DeleteItem(object obj)
        {
            if (obj is InvoiceItem item && Items.Count > 1)
            {
                Items.Remove(item);
            }
        }

        private void CreateInvoice(object obj)
        {
            if (string.IsNullOrWhiteSpace(CustomerName))
            {
                MessageBox.Show("Please enter customer name before creating invoice.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!Items.Any(item => item.Total > 0))
            {
                MessageBox.Show("Please add at least one item with a value before creating invoice.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show(
    $"Invoice {InvoiceNumber} created successfully!\n" +
    $"Customer: {CustomerName}\n" +
    $"Total Amount: LKR {GrandTotal:N2}",
    "Invoice Created",
    MessageBoxButton.OK,
    MessageBoxImage.Information
);

        }

        private void ClearForm(object obj)
        {
            InvoiceNumber = "INV-1001";
            InvoiceDate = DateTime.Now;
            CustomerName = string.Empty;
            BuildingType = string.Empty;
            CurtainLayerType = "Double layer";
            CurtainStyle = "Ripple";
            PaymentMethod = "Cash";

            _transportLaborCost = 0;
            _discountPercentage = 0;
            TransportLaborCostText = "0.00";
            DiscountText = "0";

            IsPelmetBoardChecked = false;
            IsMotorizedChecked = false;

            Items.Clear();
            Items.Add(new InvoiceItem());
        }

        private void LoadItemNamesFromDatabase()
        {
            AvailableItems = new ObservableCollection<string>();
            string query = "SELECT DISTINCT ItemName FROM inventory ORDER BY ItemName ASC"; //  Order by ItemName

            try
            {
                using (var connection = _dbHelper.GetConnection() as SQLiteConnection)
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                AvailableItems.Add(reader.GetString(0));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading inventory data: " + ex.Message, "Database Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async Task<List<string>> LoadMaterialsByItemName(string itemName)
        {
            var materials = new List<string>();
            if (string.IsNullOrEmpty(itemName)) return materials;

            string query = "SELECT DISTINCT Material FROM inventory WHERE ItemName = @ItemName ORDER BY Material ASC"; //  Order by Material

            try
            {
                using (var connection = _dbHelper.GetConnection() as SQLiteConnection)
                {
                    await connection.OpenAsync();
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ItemName", itemName);
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                materials.Add(reader["Material"].ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading materials for {itemName}: {ex.Message}", "Database Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return materials;
        }


        private async Task<Product> LoadProductData(string itemName)
        {
            string query = "SELECT MeasuringUnit, UnitPrice FROM inventory WHERE ItemName = @ItemName";
            try
            {
                using (var connection = _dbHelper.GetConnection() as SQLiteConnection)
                {
                    await connection.OpenAsync();
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ItemName", itemName);
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return new Product
                                {
                                    UnitOfMeasure = reader.GetString(0),
                                    UnitPrice = reader.GetDouble(1)
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading product data: " + ex.Message, "Database Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return null;
        }
        #endregion

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);
        public void Execute(object parameter) => _execute(parameter);
    }

    public class Product
    {
        public string UnitOfMeasure { get; set; }
        public double UnitPrice { get; set; }
    }
}
