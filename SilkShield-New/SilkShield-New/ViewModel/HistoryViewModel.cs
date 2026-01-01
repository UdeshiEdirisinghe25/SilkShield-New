using SilkShield_New.Data;
using SilkShield_New.Model;
using SilkShield_New.Service;
using SilkShield_New.View;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace SilkShield_New.ViewModel
{
    public class HistoryNewViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseHelper _db = new DatabaseHelper();
        private ObservableCollection<Invoice> _invoices;
        private string _searchText;
        private DateTime? _fromDate;
        private DateTime? _toDate;
        private bool _noInvoicesFound;
        private int _totalInvoiceCount;

        // --- Properties ---
        public ObservableCollection<Invoice> Invoices
        {
            get => _invoices;
            set
            {
                _invoices = value;
                OnPropertyChanged(nameof(Invoices));
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged(nameof(SearchText));
                    ApplyFilters(); // Live filtering as you type
                }
            }
        }

        public DateTime? FromDate
        {
            get => _fromDate;
            set
            {
                _fromDate = value;
                OnPropertyChanged(nameof(FromDate));
                ApplyFilters();
            }
        }

        public DateTime? ToDate
        {
            get => _toDate;
            set
            {
                _toDate = value;
                OnPropertyChanged(nameof(ToDate));
                ApplyFilters();
            }
        }

        // New: indicates whether the last filter returned no invoices
        public bool NoInvoicesFound
        {
            get => _noInvoicesFound;
            set
            {
                if (_noInvoicesFound == value) return;
                _noInvoicesFound = value;
                OnPropertyChanged(nameof(NoInvoicesFound));
            }
        }

        // New: total count of invoices shown (after filters)
        public int TotalInvoiceCount
        {
            get => _totalInvoiceCount;
            set
            {
                if (_totalInvoiceCount == value) return;
                _totalInvoiceCount = value;
                OnPropertyChanged(nameof(TotalInvoiceCount));
            }
        }

        // --- Commands ---
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ClearFiltersCommand { get; }
        public ICommand SearchCommand { get; }

        public HistoryNewViewModel()
        {
            // Initialize Commands
            EditCommand = new RelayCommand(EditInvoice);
            DeleteCommand = new RelayCommand(DeleteInvoice);
            ClearFiltersCommand = new RelayCommand(ClearFilters);
            SearchCommand = new RelayCommand(ExecuteSearch);

            // Initial Data Load
            LoadInvoices();
            _refreshTimer = new DispatcherTimer();
            _refreshTimer.Interval = TimeSpan.FromSeconds(1);
            _refreshTimer.Tick += (sender, e) =>
            {
                ApplyFilters(); // Refresh list while keeping user's search text
            };
            _refreshTimer.Start();
        }

        // --- Methods ---
        public void LoadInvoices()
        {
            var data = _db.GetAllInvoices();
            Invoices = new ObservableCollection<Invoice>(data);
            TotalInvoiceCount = data?.Count ?? 0;
            NoInvoicesFound = TotalInvoiceCount == 0;
        }

        private void ApplyFilters()
        {
            // We get fresh data from the DB to filter
            var allData = _db.GetAllInvoices();

            var filtered = allData.AsEnumerable();

            // Date Range Filter
            if (FromDate.HasValue)
                filtered = filtered.Where(i => i.InvoiceDate.Date >= FromDate.Value.Date);

            if (ToDate.HasValue)
                filtered = filtered.Where(i => i.InvoiceDate.Date <= ToDate.Value.Date);

            // Text Search Filter (Number or Name)
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string search = SearchText.ToLower();
                filtered = filtered.Where(i =>
                    (i.CustomerName != null && i.CustomerName.ToLower().Contains(search)) ||
                    (i.InvoiceNumber != null && i.InvoiceNumber.ToLower().Contains(search))
                );
            }

            var resultList = filtered.ToList();
            Invoices = new ObservableCollection<Invoice>(resultList);

            // Set counts so the view can show total and no-results message
            TotalInvoiceCount = resultList.Count;
            NoInvoicesFound = resultList.Count == 0;
        }

        private void ExecuteSearch(object obj)
        {
            // 1. Run the existing filter logic
            ApplyFilters();

            // 2. Show the popup if nothing was found
            if (NoInvoicesFound)
            {
                MessageBox.Show("No invoices found for the given search criteria.", "Search", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private DispatcherTimer _refreshTimer; // This makes it accessible to the whole class

        private void ClearFilters(object obj = null)
        {
            _fromDate = null;
            _toDate = null;
            _searchText = string.Empty;

            OnPropertyChanged(nameof(FromDate));
            OnPropertyChanged(nameof(ToDate));
            OnPropertyChanged(nameof(SearchText));

            ApplyFilters();
        }

        private void EditInvoice(object obj)
        {
            if (obj is Invoice selectedInvoice)
            {
                // Find the MainWindow and call the specific navigation method
                var mainWindow = Application.Current.MainWindow as SilkShield_New.View.MainWindow;

                if (mainWindow != null)
                {
                    mainWindow.NavigateToEditInvoice(selectedInvoice);
                }
            }
        }

        private void DeleteInvoice(object obj)
        {
            if (obj is Invoice invoice)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete invoice {invoice.InvoiceNumber}?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    bool success = _db.DeleteInvoice(invoice.InvoiceNumber);
                    if (success)
                    {
                        Invoices.Remove(invoice);
                        TotalInvoiceCount = Invoices?.Count ?? 0;
                        NoInvoicesFound = TotalInvoiceCount == 0;
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete invoice from database.");
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string prop) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}