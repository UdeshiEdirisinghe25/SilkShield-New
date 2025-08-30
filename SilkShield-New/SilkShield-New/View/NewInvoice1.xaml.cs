using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using SilkShield_New.Models;
using System.Runtime.CompilerServices;

namespace SilkShield_New.View
{
    // InvoiceItem class එකට INotifyPropertyChanged එකතු කරලා තියෙනවා.
    public class InvoiceItem : INotifyPropertyChanged
    {
        private string _itemName;
        private int _quantity;
        private decimal _unitPrice;
        private decimal _total;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public string ItemName
        {
            get => _itemName;
            set
            {
                _itemName = value;
                OnPropertyChanged();
            }
        }

        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    OnPropertyChanged();
                    UpdateTotal();
                }
            }
        }

        public decimal UnitPrice
        {
            get => _unitPrice;
            set
            {
                if (_unitPrice != value)
                {
                    _unitPrice = value;
                    OnPropertyChanged();
                    UpdateTotal();
                }
            }
        }

        public decimal Total
        {
            get => _total;
            private set
            {
                _total = value;
                OnPropertyChanged();
            }
        }

        private void UpdateTotal()
        {
            Total = Quantity * UnitPrice;
        }
    }

    public partial class NewInvoice1 : Window, INotifyPropertyChanged
    {
        public ObservableCollection<InvoiceItem> Items { get; } = new ObservableCollection<InvoiceItem>();

        private decimal _grandTotal;
        public decimal GrandTotal
        {
            get => _grandTotal;
            set
            {
                _grandTotal = value;
                OnPropertyChanged(nameof(GrandTotal));
            }
        }

        private decimal _discount;
        public decimal Discount
        {
            get => _discount;
            set
            {
                _discount = value;
                OnPropertyChanged(nameof(Discount));
                UpdateGrandTotal(); // Discount වෙනස් වූ විට GrandTotal update කරන්න.
            }
        }

        public NewInvoice1()
        {
            InitializeComponent();
            DataContext = this;
            ItemDataGrid.ItemsSource = Items;

            Items.CollectionChanged += Items_CollectionChanged;
        }

        private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (InvoiceItem it in e.NewItems)
                    it.PropertyChanged += Item_PropertyChanged;

            if (e.OldItems != null)
                foreach (InvoiceItem it in e.OldItems)
                    it.PropertyChanged -= Item_PropertyChanged;

            UpdateGrandTotal();
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(InvoiceItem.Total))
            {
                UpdateGrandTotal();
            }
        }

        private void UpdateGrandTotal()
        {
            decimal subtotal = Items.Sum(i => i.Total);
            GrandTotal = subtotal - Discount;
        }

        // + Add Item
        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            var newItem = new InvoiceItem { ItemName = "New Item", Quantity = 1, UnitPrice = 0 };
            Items.Add(newItem);
        }

        // ENTER -> commit row edits
        private void ItemDataGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var grid = (DataGrid)sender;
                grid.CommitEdit(DataGridEditingUnit.Cell, true);
                grid.CommitEdit(DataGridEditingUnit.Row, true);
                e.Handled = true;
            }
        }

        // Discount text box changes
        private void Discount_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (decimal.TryParse(DiscountTextBox.Text, out decimal discountValue))
            {
                Discount = discountValue;
            }
            else
            {
                // Invalid input, set discount to 0
                Discount = 0;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string n) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
