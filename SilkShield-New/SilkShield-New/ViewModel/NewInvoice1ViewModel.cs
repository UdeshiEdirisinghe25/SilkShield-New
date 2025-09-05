using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using SilkShield_New.Model;
using System.Globalization;

namespace SilkShield_New.ViewModel
{
    public class NewInvoice1ViewModel : INotifyPropertyChanged
    {
        // Properties for invoice details
        private string _invoiceNumber;
        private DateTime _invoiceDate;
        private string _customerName;
        private string _buildingType;
        private string _curtainLayerType;
        private string _curtainStyle;
        private string _paymentMethod;
        private ObservableCollection<InvoiceItem> _items;
        private double _grandTotal;

        // New properties for UI bindings and their internal logic
        private double _discountPercentage; // Stores the actual discount percentage
        private string _discountText; // For the TextBox binding
        private double _transportLaborCost; // Stores the combined cost
        private string _transportLaborCostText; // For the TextBox binding

        // Properties for ComboBox selections (using strings for better binding)
        private string _isPelmetBoardCheckedText;
        private string _isMotorizedCheckedText;

        // ICommands
        public ICommand AddItemCommand { get; }
        public ICommand DeleteItemCommand { get; }
        public ICommand CreateInvoiceCommand { get; }
        public ICommand ClearFormCommand { get; }

        public NewInvoice1ViewModel()
        {
            // Initialize commands
            AddItemCommand = new RelayCommand(AddItem);
            DeleteItemCommand = new RelayCommand(DeleteItem);
            CreateInvoiceCommand = new RelayCommand(CreateInvoice);
            ClearFormCommand = new RelayCommand(ClearForm);

            // Initialize collections
            Items = new ObservableCollection<InvoiceItem>();
            Items.CollectionChanged += (sender, e) => CalculateGrandTotal();

            // Listen for PropertyChanged events on each item in the collection
            Items.CollectionChanged += (sender, e) => {
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
            };

            // Add an initial empty row to the DataGrid
            var initialItem = new InvoiceItem();
            Items.Add(initialItem);

            // Set initial values
            InvoiceNumber = "INV-1001";
            InvoiceDate = DateTime.Now;
            CurtainLayerType = "Double layer";
            CurtainStyle = "Ripple";
            PaymentMethod = "Cash";
            IsPelmetBoardChecked = false; // Internal boolean
            IsMotorizedChecked = false; // Internal boolean

            // Set initial UI text values
            TransportLaborCostText = "0.00";
            DiscountText = "0";
            IsPelmetBoardCheckedText = "No";
            IsMotorizedCheckedText = "No";

            // Calculate initial grand total
            CalculateGrandTotal();
        }

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

        // This is the property for the Transport and Labor Cost TextBox
        public string TransportLaborCostText
        {
            get => _transportLaborCost == 0 ? "0" : _transportLaborCost.ToString("0.##");
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    _transportLaborCost = 0;
                }
                else if (double.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out double result))
                {
                    _transportLaborCost = Math.Max(0, result);
                }
                CalculateGrandTotal();
                OnPropertyChanged(nameof(TransportLaborCostText));
            }
        }

        // This is the property for the Discount TextBox
        public string DiscountText
        {
            get => _discountPercentage == 0 ? "0" : _discountPercentage.ToString("0.##");
            set
            {
                if (string.IsNullOrWhiteSpace(value) ||
                    !double.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out double result))
                {
                    _discountPercentage = 0;
                }
                else
                {
                    _discountPercentage = Math.Max(0, result);
                }
                CalculateGrandTotal();
                OnPropertyChanged(nameof(DiscountText));
            }
        }

        // Internal boolean properties for the ComboBoxes
        public bool IsPelmetBoardChecked
        {
            get => _isPelmetBoardCheckedText == "Yes";
            set => _isPelmetBoardCheckedText = value ? "Yes" : "No";
        }

        public bool IsMotorizedChecked
        {
            get => _isMotorizedCheckedText == "Yes";
            set => _isMotorizedCheckedText = value ? "Yes" : "No";
        }

        // Properties to bind to the ComboBoxes in XAML
        public string IsPelmetBoardCheckedText
        {
            get => _isPelmetBoardCheckedText;
            set
            {
                _isPelmetBoardCheckedText = value;
                OnPropertyChanged(nameof(IsPelmetBoardCheckedText));
            }
        }

        public string IsMotorizedCheckedText
        {
            get => _isMotorizedCheckedText;
            set
            {
                _isMotorizedCheckedText = value;
                OnPropertyChanged(nameof(IsMotorizedCheckedText));
            }
        }

        #endregion

        #region Private Methods

        private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(InvoiceItem.Total))
            {
                CalculateGrandTotal();
            }
        }

        private void CalculateGrandTotal()
        {
            double subTotal = Items?.Where(item => item?.Total > 0)?.Sum(item => item.Total) ?? 0;

            // First, add the transport cost to the subtotal.
            double totalBeforeDiscount = subTotal + _transportLaborCost;

            // Then, apply the percentage discount to that new total.
            double discountAmount = totalBeforeDiscount * (_discountPercentage / 100.0);

            // Ensure the discount doesn't exceed the total before discount.
            discountAmount = Math.Min(totalBeforeDiscount, discountAmount);

            double calculatedTotal = totalBeforeDiscount - discountAmount;

            GrandTotal = Math.Max(0, calculatedTotal);
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
            // Validation before creating invoice
            if (string.IsNullOrWhiteSpace(CustomerName))
            {
                System.Windows.MessageBox.Show("Please enter customer name before creating invoice.", "Validation Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            if (!Items.Any(item => item.Total > 0))
            {
                System.Windows.MessageBox.Show("Please add at least one item with value before creating invoice.", "Validation Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            System.Windows.MessageBox.Show($"Invoice {InvoiceNumber} created successfully!\n" +
                                         $"Customer: {CustomerName}\n" +
                                         $"Total Amount: {GrandTotal:C}", "Invoice Created",
                                         System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }

        private void ClearForm(object obj)
        {
            // Reset properties
            InvoiceNumber = "INV-1001";
            InvoiceDate = DateTime.Now;
            CustomerName = string.Empty;
            BuildingType = string.Empty;
            CurtainLayerType = "Double layer";
            CurtainStyle = "Ripple";
            PaymentMethod = "Cash";

            // Reset text-based inputs and internal values
            TransportLaborCostText = "0.00";
            DiscountText = "0";

            IsPelmetBoardCheckedText = "No";
            IsMotorizedCheckedText = "No";

            Items.Clear();
            Items.Add(new InvoiceItem());
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

    // Note: The RelayCommand class provided in your initial request is correct and does not need modification.
    // It's a standard implementation for handling ICommand in MVVM.
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
}
