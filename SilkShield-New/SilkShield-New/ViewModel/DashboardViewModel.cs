using System.Collections.ObjectModel;
using System.ComponentModel;
using SilkShield_New.Data;
using SilkShield_New.Model;

namespace SilkShield_New.ViewModel
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        private int _upcomingCount;
        private int _ongoingCount;
        private int _pendingCount;
        private ObservableCollection<InvoiceDTO> _recentInvoices;

        public int UpcomingCount
        {
            get => _upcomingCount;
            set { _upcomingCount = value; OnPropertyChanged(nameof(UpcomingCount)); }
        }

        public int OngoingCount
        {
            get => _ongoingCount;
            set { _ongoingCount = value; OnPropertyChanged(nameof(OngoingCount)); }
        }

        

        public int PendingCount
        {
            get => _pendingCount;
            set { _pendingCount = value; OnPropertyChanged(nameof(PendingCount)); }
        }

        public ObservableCollection<InvoiceDTO> RecentInvoices
        {
            get => _recentInvoices;
            set { _recentInvoices = value; OnPropertyChanged(nameof(RecentInvoices)); }
        }

        public DashboardViewModel()
        {
            LoadDashboard();
        }

        private void LoadDashboard()
        {
            var customerDAL = new CustomerDAL();
            var dbHelper = new DatabaseHelper();

            // Get counts
            var summary = customerDAL.GetProjectSummary();
            UpcomingCount = summary.Upcoming;
            OngoingCount = summary.Ongoing;
            PendingCount = summary.Pending;

            // Get recent invoices
            var invoices = dbHelper.GetRecentInvoices(5);
            RecentInvoices = new ObservableCollection<InvoiceDTO>(invoices);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
