using System.Collections.ObjectModel;
using System.ComponentModel;
using SilkShield_New.Data;
using SilkShield_New.Model;

namespace SilkShield_New.ViewModel
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        private DatabaseHelper _dbHelper;

        private ObservableCollection<InvoiceDTO> _recentInvoices;
        public ObservableCollection<InvoiceDTO> RecentInvoices
        {
            get { return _recentInvoices; }
            set
            {
                _recentInvoices = value;
                OnPropertyChanged(nameof(RecentInvoices));
            }
        }

        public DashboardViewModel()
        {
            _dbHelper = new DatabaseHelper();
            LoadRecentInvoices();
        }

        private void LoadRecentInvoices()
        {
            var invoices = _dbHelper.GetRecentInvoices(5);
            RecentInvoices = new ObservableCollection<InvoiceDTO>(invoices);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
