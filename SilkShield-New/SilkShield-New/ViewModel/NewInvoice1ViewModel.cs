using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using SilkShield_New.Model;

namespace SilkShield_New.ViewModel
{
    public class NewInvoice1ViewModel : INotifyPropertyChanged
    {
        private string _invoiceNumber;
        private DateTime _invoiceDate;
        private string _customerName;
        private string _buildingType;
        private string _curtainLayerType;
        private string _curtainStyle;
        private string _paymentMethod;
        private ObservableCollection<InvoiceItem> _items;
        private double _grandTotal;
        private double _discount;
        private double _transportLaborCost;
        private bool _isPelmetBoardChecked;
        private bool _isMotorizedChecked;

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

        public double Discount
        {
            get => _discount;
            set
            {
                if (_discount != value)
                {
                    _discount = value;
                    CalculateGrandTotal();
                    OnPropertyChanged(nameof(Discount));
                }
            }
        }

        public double TransportLaborCost
        {
            get => _transportLaborCost;
            set
            {
                if (_transportLaborCost != value)
                {
                    _transportLaborCost = value;
                    CalculateGrandTotal();
                    OnPropertyChanged(nameof(TransportLaborCost));
                }
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

        public ICommand AddItemCommand { get; }
        public ICommand DeleteItemCommand { get; }
        public ICommand CreateInvoiceCommand { get; }
        public ICommand ClearFormCommand { get; }

        public NewInvoice1ViewModel()
        {
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

            var initialItem = new InvoiceItem();
            Items.Add(initialItem);

            AddItemCommand = new RelayCommand(AddItem);
            DeleteItemCommand = new RelayCommand(DeleteItem);
            CreateInvoiceCommand = new RelayCommand(CreateInvoice);
            ClearFormCommand = new RelayCommand(ClearForm);

            // Initialize properties to their default values
            InvoiceNumber = "INV-1001";
            InvoiceDate = DateTime.Now;
            CustomerName = string.Empty;
            BuildingType = string.Empty;
            CurtainLayerType = "Double layer"; // Set initial value from ComboBox
            CurtainStyle = "Ripple"; // Set initial value from ComboBox
            PaymentMethod = "Cash"; // Set initial value from ComboBox
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(InvoiceItem.Total))
            {
                CalculateGrandTotal();
            }
        }

        private void CalculateGrandTotal()
        {
            double subTotal = Items.Sum(item => item.Total);
            GrandTotal = subTotal - _discount + _transportLaborCost;
        }

        private void AddItem(object obj)
        {
            var newItem = new InvoiceItem();
            Items.Add(newItem);
        }

        private void DeleteItem(object obj)
        {
            if (obj is InvoiceItem item)
            {
                Items.Remove(item);
            }
        }

        private void CreateInvoice(object obj)
        {
            MessageBox.Show("Invoice creation logic would go here!");
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
            Discount = 0;
            TransportLaborCost = 0;
            IsPelmetBoardChecked = false;
            IsMotorizedChecked = false;
            Items.Clear();
            Items.Add(new InvoiceItem());
        }
    }
}
