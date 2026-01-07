using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input; 
using SilkShield_New.Data;
using SilkShield_New.Model;

namespace SilkShield_New.ViewModel
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        public static DashboardViewModel Instance { get; private set; }

        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(nameof(CurrentView)); }
        }

        private int _upcomingCount;
        private int _ongoingCount;
        private int _pendingCount;
        private ObservableCollection<InvoiceDTO> _recentInvoices;

        public ICommand RefreshCommand { get; }

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
            Instance = this;
            RecentInvoices = new ObservableCollection<InvoiceDTO>();

           
            RefreshCommand = new RelayCommand(o => LoadDashboard());

            LoadDashboard();
        }

        public void LoadDashboard()
        {
            var customerDAL = new CustomerDAL();
            var dbHelper = new DatabaseHelper();

            var summary = customerDAL.GetProjectSummary();
            UpcomingCount = summary.Upcoming;
            OngoingCount = summary.Ongoing;
            PendingCount = summary.Pending;

            var invoices = dbHelper.GetRecentInvoices(3);
            RecentInvoices.Clear();
            foreach (var invoice in invoices)
            {
                RecentInvoices.Add(invoice);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}