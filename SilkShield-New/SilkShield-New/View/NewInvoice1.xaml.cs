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

namespace SilkShield_New.View
{
    public partial class NewInvoice1 : Window, INotifyPropertyChanged
    {
        public ObservableCollection<InvoiceItem> Items { get; } = new ObservableCollection<InvoiceItem>();

        private decimal _grandTotal;
        public decimal GrandTotal
        {
            get => _grandTotal;
            set { _grandTotal = value; OnPropertyChanged(nameof(GrandTotal)); }
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
            if (e.PropertyName == nameof(InvoiceItem.Total) ||
                e.PropertyName == nameof(InvoiceItem.Quantity) ||
                e.PropertyName == nameof(InvoiceItem.UnitPrice))
            {
                UpdateGrandTotal();
            }
        }

        private void UpdateGrandTotal()
        {
            GrandTotal = Items.Sum(i => i.Total);
        }

        // + Add Item
        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            var newItem = new InvoiceItem { ItemName = "New Item", Quantity = 1, UnitPrice = 0 };
            Items.Add(newItem);

            // Delay edit mode until DataGrid refreshes
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ItemDataGrid.SelectedItem = newItem;
                ItemDataGrid.ScrollIntoView(newItem);
                ItemDataGrid.CurrentCell = new DataGridCellInfo(newItem, ItemDataGrid.Columns[0]);
                ItemDataGrid.BeginEdit();
            }), DispatcherPriority.Background);
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

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string n) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
