using SilkShield_New.Data;
using SilkShield_New.Model;
using SilkShield_New.Service;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace SilkShield_New.ViewModel
{
    public class HistoryNewViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseHelper _db = new DatabaseHelper();

        public ObservableCollection<Invoice> Invoices { get; set; }

        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ClearFiltersCommand { get; set; } 
        public ICommand SearchCommand { get; set; }




        public HistoryNewViewModel()
        {
            LoadInvoices();

            EditCommand = new RelayCommand(EditInvoice);
            DeleteCommand = new RelayCommand(DeleteInvoice);
            ClearFiltersCommand = new RelayCommand(ClearFilters); // Initialize here
            SearchCommand = new RelayCommand(ExecuteSearch);

        }

        public void LoadInvoices()
        {
            Invoices = new ObservableCollection<Invoice>(_db.GetAllInvoices());
            OnPropertyChanged(nameof(Invoices));
        }

        private void EditInvoice(object obj)
        {
            if (obj is Invoice invoice)
            {
                MessageBox.Show(
                    $"Edit Invoice: {invoice.InvoiceNumber}",
                    "Edit",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // 👉 Open Edit Window here later
            }
        }

        private void DeleteInvoice(object obj)
        {
            if (obj is Invoice invoice)
            {
                var result = MessageBox.Show(
                    "Are you sure you want to delete this invoice?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    _db.DeleteInvoice(invoice.InvoiceNumber);
                    Invoices.Remove(invoice);
                }
            }
        }

        private DateTime? _fromDate;
        public DateTime? FromDate
        {
            get => _fromDate;
            set
            {
                if (_fromDate != value)
                {
                    _fromDate = value;
                    OnPropertyChanged(nameof(FromDate));
                    ApplyFilters(); // Apply filters when date changes
                }
            }
        }

        private DateTime? _toDate;
        public DateTime? ToDate
        {
            get => _toDate;
            set
            {
                if (_toDate != value)
                {
                    _toDate = value;
                    OnPropertyChanged(nameof(ToDate));
                    ApplyFilters(); // Apply filters when date changes
                }
            }
        }
        private void ApplyFilters()
        {
            var filtered = _db.GetAllInvoices(); // Get all invoices from database

            if (FromDate.HasValue)
                filtered = filtered.Where(i => i.InvoiceDate.Date >= FromDate.Value.Date).ToList();

            if (ToDate.HasValue)
                filtered = filtered.Where(i => i.InvoiceDate.Date <= ToDate.Value.Date).ToList();

            if (!string.IsNullOrEmpty(SearchText))
            {
                filtered = filtered.Where(i =>
                    (i.CustomerName != null && i.CustomerName.ToLower().Contains(SearchText.ToLower())) ||
                    (i.InvoiceNumber != null && i.InvoiceNumber.ToLower().Contains(SearchText.ToLower()))
                ).ToList();
            }

            if (filtered.Count == 0)
            {
                System.Windows.MessageBox.Show(
                    "No invoices found! Maybe they’re on a coffee break! 😎☕",
                    "Search Result",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information
                );
            }


            Invoices = new ObservableCollection<Invoice>(filtered);
            OnPropertyChanged(nameof(Invoices));

        }

        private void ExecuteSearch(object obj)
        {
            ApplyFilters();
        }





        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged(nameof(SearchText));
                    ApplyFilters(); // Update filtered list on search
                }
            }
        }

        private void ClearFilters(object obj = null)
        {
            FromDate = null;
            ToDate = null;
            SearchText = string.Empty;
            ApplyFilters(); // Reset filters
        }





        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string prop) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
