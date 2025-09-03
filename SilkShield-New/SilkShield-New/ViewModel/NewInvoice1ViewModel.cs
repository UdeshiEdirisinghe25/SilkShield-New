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
        private ObservableCollection<InvoiceItem> _items;
        private double _grandTotal;
        private double _discount;
        private double _transportLaborCost;
        private bool _isPelmetBoardChecked;
        private bool _isMotorizedChecked;

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

        public NewInvoice1ViewModel()
        {
            Items = new ObservableCollection<InvoiceItem>();
            Items.CollectionChanged += (sender, e) => CalculateGrandTotal();

            foreach (var item in Items)
            {
                item.PropertyChanged += OnItemPropertyChanged;
            }

            var initialItem = new InvoiceItem();
            initialItem.PropertyChanged += OnItemPropertyChanged;
            Items.Add(initialItem);

            AddItemCommand = new RelayCommand(AddItem);
            DeleteItemCommand = new RelayCommand(DeleteItem);
            CreateInvoiceCommand = new RelayCommand(CreateInvoice);
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
            newItem.PropertyChanged += OnItemPropertyChanged;
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
    }
}