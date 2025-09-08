using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using System.Windows;
using System.Threading.Tasks;
using System.Collections.Generic;
using SilkShield_New.Model;
using SilkShield_New.Data;

namespace SilkShield_New.ViewModel
{
    public class NewInvoice1ViewModel : INotifyPropertyChanged
    {
        #region Private Fields
        private string _invoiceNumber;
        private DateTime _invoiceDate;
        private string _customerName;
        private string _location; // BuildingType වෙනුවට Location ලෙස නම වෙනස් කර ඇත
        private string _curtainLayerType;
        private string _curtainStyle;
        private string _paymentMethod;
        private ObservableCollection<SilkShield_New.Model.InvoiceItem> _items;
        private double _grandTotal;
        private string _discountText;
        private string _transportLaborCostText;
        private double _discountPercentage;
        private double _transportLaborCost;
        private bool _isPelmetBoardChecked;
        private bool _isMotorizedChecked;
        private ObservableCollection<string> _availableItems;
        private readonly InvoiceDataService _invoiceDataService;
        #endregion

        #region Public Properties
        // මෙහි ඇති සියලුම public properties, ඉහත private fields වලට අනුකූලව වෙනස් කර ඇත.
        // උදා: BuildingType වෙනුවට Location භාවිතා කර ඇත.
        public string InvoiceNumber { get => _invoiceNumber; set { _invoiceNumber = value; OnPropertyChanged(nameof(InvoiceNumber)); } }
        public DateTime InvoiceDate { get => _invoiceDate; set { _invoiceDate = value; OnPropertyChanged(nameof(InvoiceDate)); } }
        public string CustomerName { get => _customerName; set { _customerName = value; OnPropertyChanged(nameof(CustomerName)); } }
        public string Location { get => _location; set { _location = value; OnPropertyChanged(nameof(Location)); } }
        public string CurtainLayerType { get => _curtainLayerType; set { _curtainLayerType = value; OnPropertyChanged(nameof(CurtainLayerType)); } }
        public string CurtainStyle { get => _curtainStyle; set { _curtainStyle = value; OnPropertyChanged(nameof(CurtainStyle)); } }
        public string PaymentMethod { get => _paymentMethod; set { _paymentMethod = value; OnPropertyChanged(nameof(PaymentMethod)); } }

        public ObservableCollection<SilkShield_New.Model.InvoiceItem> Items
        {
            get => _items;
            set
            {
                if (_items != null)
                {
                    _items.CollectionChanged -= Items_CollectionChanged;
                    foreach (var item in _items) item.PropertyChanged -= OnItemPropertyChanged;
                }
                _items = value;
                if (_items != null)
                {
                    _items.CollectionChanged += Items_CollectionChanged;
                    foreach (var item in _items) item.PropertyChanged += OnItemPropertyChanged;
                }
                OnPropertyChanged(nameof(Items));
            }
        }

        public double GrandTotal { get => _grandTotal; set { _grandTotal = value; OnPropertyChanged(nameof(GrandTotal)); } }
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
        public bool IsPelmetBoardChecked { get => _isPelmetBoardChecked; set { _isPelmetBoardChecked = value; OnPropertyChanged(nameof(IsPelmetBoardChecked)); } }
        public bool IsMotorizedChecked { get => _isMotorizedChecked; set { _isMotorizedChecked = value; OnPropertyChanged(nameof(IsMotorizedChecked)); } }
        public ObservableCollection<string> AvailableItems { get => _availableItems; set { _availableItems = value; OnPropertyChanged(nameof(AvailableItems)); } }
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
            _invoiceDataService = new InvoiceDataService();

            AddItemCommand = new RelayCommand(AddItem);
            DeleteItemCommand = new RelayCommand(DeleteItem);
            CreateInvoiceCommand = new RelayCommand(async (p) => await CreateInvoiceAsync());
            ClearFormCommand = new RelayCommand(ClearForm);

            Items = new ObservableCollection<SilkShield_New.Model.InvoiceItem>();
            LoadItemNamesFromDatabaseAsync();
            ClearForm(null);
        }
        #endregion

        #region Private Methods

        // CollectionChanged event එක නිවැරදිව handle කිරීම
        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (SilkShield_New.Model.InvoiceItem item in e.OldItems)
                {
                    item.PropertyChanged -= OnItemPropertyChanged;
                }
            }
            if (e.NewItems != null)
            {
                foreach (SilkShield_New.Model.InvoiceItem item in e.NewItems)
                {
                    item.PropertyChanged += OnItemPropertyChanged;
                }
            }
            CalculateGrandTotal();
        }

        private async void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is SilkShield_New.Model.InvoiceItem item)
            {
                if (e.PropertyName == nameof(item.ItemName))
                {
                    // async method කැඳවීම සඳහා await භාවිතා කරන්න.
                    var materials = await _invoiceDataService.GetMaterialsByItemNameAsync(item.ItemName);
                    item.AvailableMaterials = new ObservableCollection<string>(materials);
                    item.SelectedMaterial = materials.FirstOrDefault();
                }
                else if (e.PropertyName == nameof(item.SelectedMaterial))
                {
                    // async method කැඳවීම සඳහා await භාවිතා කරන්න.
                    if (!string.IsNullOrEmpty(item.SelectedMaterial))
                    {
                        item.UnitPrice = await _invoiceDataService.GetUnitPriceAsync(item.ItemName, item.SelectedMaterial);
                    }
                }
                else if (e.PropertyName == nameof(item.Quantity) || e.PropertyName == nameof(item.UnitPrice))
                {
                    item.CalculateTotal();
                    CalculateGrandTotal();
                }
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
            var newItem = new SilkShield_New.Model.InvoiceItem();
            Items.Add(newItem);
        }

        private void DeleteItem(object obj)
        {
            if (obj is SilkShield_New.Model.InvoiceItem item && Items.Count > 1)
            {
                Items.Remove(item);
            }
        }

        private async Task CreateInvoiceAsync()
        {
            if (string.IsNullOrWhiteSpace(CustomerName))
            {
                MessageBox.Show("Please enter customer name before creating invoice.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!Items.Any(item => item.Total > 0))
            {
                MessageBox.Show("Please add at least one item with a value before creating invoice.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var invoiceData = new SilkShield_New.Model.Invoice
                {
                    InvoiceNumber = InvoiceNumber,
                    InvoiceDate = InvoiceDate,
                    CustomerName = CustomerName,
                    Location = Location, // BuildingType වෙනුවට Location ලෙස නම වෙනස් කර ඇත
                    PelmetBoard = IsPelmetBoardChecked,
                    Motorized = IsMotorizedChecked,
                    PaymentMethod = PaymentMethod,
                    TransportLaborCost = _transportLaborCost,
                    Discount = _discountPercentage,
                    Items = Items
                };

                // SaveFileDialog භාවිතයෙන් ගොනු මාර්ගය තෝරා ගැනීමට ඉඩ දීම
                Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
                saveFileDialog.Filter = "PDF Files (*.pdf)|*.pdf";
                saveFileDialog.FileName = $"Invoice_{invoiceData.InvoiceNumber}.pdf";

                if (saveFileDialog.ShowDialog() == true)
                {
                    string filePath = saveFileDialog.FileName;
                    await Task.Run(() => _invoiceDataService.AddInvoice(invoiceData)); // පසුබිමෙන් save කිරීමට async ලෙස කැඳවීම
                    await Task.Run(() => _invoiceDataService.GenerateInvoicePdf(invoiceData, filePath)); // පසුබිමෙන් PDF සෑදීමට async ලෙස කැඳවීම

                    MessageBox.Show(
                        $"Invoice {invoiceData.InvoiceNumber} successfully created and saved!\n" +
                        $"PDF saved to: {filePath}",
                        "Invoice Created",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                    ClearForm(null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating invoice: {ex.Message}\n\nStack Trace: {ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearForm(object obj)
        {
            InvoiceNumber = "INV-1001";
            InvoiceDate = DateTime.Now;
            CustomerName = string.Empty;
            Location = string.Empty; // BuildingType වෙනුවට Location
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
            Items.Add(new SilkShield_New.Model.InvoiceItem());
        }

        // Database එකෙන් දත්ත load කිරීමට async method එකක් භාවිතා කරන්න
        private async void LoadItemNamesFromDatabaseAsync()
        {
            AvailableItems = new ObservableCollection<string>(await _invoiceDataService.GetDistinctItemNamesAsync());
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
}