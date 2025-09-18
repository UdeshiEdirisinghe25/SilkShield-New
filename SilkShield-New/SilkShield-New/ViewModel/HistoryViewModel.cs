using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Input;
using System.Runtime.CompilerServices;

namespace SilkShield_New.ViewModel
{
    public class HistoryViewModel : INotifyPropertyChanged
    {
        private string _connectionString = "Data Source=SilkShieldDB.sqlite;Version=3;";

        // Properties for filtering and searching
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
            }
        }

        private DateTime? _fromDate;
        public DateTime? FromDate
        {
            get => _fromDate;
            set
            {
                _fromDate = value;
                OnPropertyChanged();
            }
        }

        private DateTime? _toDate;
        public DateTime? ToDate
        {
            get => _toDate;
            set
            {
                _toDate = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<InvoiceHistory> Invoices { get; set; } = new ObservableCollection<InvoiceHistory>();

        // Commands for UI actions
        public ICommand SearchCommand { get; }
        public ICommand ClearFiltersCommand { get; }
        public ICommand ViewInvoiceCommand { get; }
        public ICommand EditInvoiceCommand { get; }
        public ICommand DeleteInvoiceCommand { get; }

        public HistoryViewModel()
        {
            SearchCommand = new RelayCommand(ExecuteSearch);
            ClearFiltersCommand = new RelayCommand(ExecuteClearFilters);
            ViewInvoiceCommand = new RelayCommand(ExecuteViewInvoice);
            EditInvoiceCommand = new RelayCommand(ExecuteEditInvoice);
            DeleteInvoiceCommand = new RelayCommand(ExecuteDeleteInvoice);

            LoadInvoices();
        }

        private void ExecuteSearch(object obj)
        {
            LoadInvoices();
        }

        private void ExecuteClearFilters(object obj)
        {
            SearchText = string.Empty;
            FromDate = null;
            ToDate = null;
            LoadInvoices();
        }

        private void ExecuteViewInvoice(object obj)
        {
            if (obj is InvoiceHistory invoice)
            {
                MessageBox.Show($"Viewing Invoice ID: {invoice.InvoiceId}\nCustomer: {invoice.Customer}");
            }
        }

        private void ExecuteEditInvoice(object obj)
        {
            if (obj is InvoiceHistory invoice)
            {
                MessageBox.Show($"Editing Invoice ID: {invoice.InvoiceId}\nCustomer: {invoice.Customer}");
            }
        }

        private void ExecuteDeleteInvoice(object obj)
        {
            if (obj is InvoiceHistory invoice)
            {
                var result = MessageBox.Show($"Are you sure you want to delete Invoice ID {invoice.InvoiceId}?", "Confirm Deletion", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var conn = new SQLiteConnection(_connectionString))
                        {
                            conn.Open();
                            string query = "DELETE FROM Invoices WHERE InvoiceId = @id";
                            using (var cmd = new SQLiteCommand(query, conn))
                            {
                                cmd.Parameters.AddWithValue("@id", invoice.InvoiceId);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        Invoices.Remove(invoice);
                        MessageBox.Show("Invoice deleted successfully.");
                    }
                    catch (SQLiteException ex)
                    {
                        MessageBox.Show($"Database error: {ex.Message}", "Error");
                    }
                }
            }
        }

        public void LoadInvoices()
        {
            Invoices.Clear();
            try
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    string query = "SELECT InvoiceId, Customer, InvoiceDate, TotalAmount FROM Invoices WHERE 1=1";

                    if (!string.IsNullOrEmpty(SearchText))
                    {
                        query += " AND Customer LIKE @searchText";
                    }
                    if (FromDate.HasValue)
                    {
                        query += " AND InvoiceDate >= @fromDate";
                    }
                    if (ToDate.HasValue)
                    {
                        query += " AND InvoiceDate <= @toDate";
                    }

                    using (var cmd = new SQLiteCommand(query, conn))
                    {
                        if (!string.IsNullOrEmpty(SearchText))
                        {
                            cmd.Parameters.AddWithValue("@searchText", $"%{SearchText}%");
                        }
                        if (FromDate.HasValue)
                        {
                            cmd.Parameters.AddWithValue("@fromDate", FromDate.Value);
                        }
                        if (ToDate.HasValue)
                        {
                            cmd.Parameters.AddWithValue("@toDate", ToDate.Value);
                        }

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Invoices.Add(new InvoiceHistory
                                {
                                    InvoiceId = reader.GetInt32(0),
                                    Customer = reader.GetString(1),
                                    InvoiceDate = reader.GetDateTime(2),
                                    TotalAmount = reader.GetDecimal(3)
                                });
                            }
                        }
                    }
                }
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show($"A database error occurred: {ex.Message}", "Error");
            }
        }

        // INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
// Minimal model class
public class InvoiceHistory
{
    public int InvoiceId { get; set; }
    public string Customer { get; set; }
    public DateTime InvoiceDate { get; set; }
    public decimal TotalAmount { get; set; }
}

