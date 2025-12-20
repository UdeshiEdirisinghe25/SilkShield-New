using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows;
using SilkShield_New.Data;
using SilkShield_New.Model;
using System.Linq;

namespace SilkShield_New.ViewModel
{
    public class Customer_ManageViewModel : INotifyPropertyChanged
    {
        private CustomerDAL _customerDal;
        private string _searchText;

        public ObservableCollection<Customer> Customers { get; set; }
        public ICollectionView CustomersView { get; set; }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged(nameof(SearchText));
                    CustomersView.Refresh();
                }
            }
        }

        public RelayCommand AddCustomerCommand { get; set; }
        public RelayCommand ViewCustomerCommand { get; set; }
        public RelayCommand EditCustomerCommand { get; set; }
        public RelayCommand DeleteCustomerCommand { get; set; }

        public Customer_ManageViewModel()
        {
            _customerDal = new CustomerDAL();
            Customers = new ObservableCollection<Customer>(_customerDal.GetAllCustomers());

            // Create ICollectionView for filtering
            CustomersView = CollectionViewSource.GetDefaultView(Customers);
            CustomersView.Filter = FilterCustomers;

            AddCustomerCommand = new RelayCommand(_ => AddCustomer());
            ViewCustomerCommand = new RelayCommand(param => ViewCustomer(param as Customer));
            EditCustomerCommand = new RelayCommand(param => EditCustomer(param as Customer));
            DeleteCustomerCommand = new RelayCommand(param => DeleteCustomer(param as Customer));
        }

        private bool FilterCustomers(object obj)
        {
            if (obj is Customer customer)
            {
                if (string.IsNullOrEmpty(SearchText))
                    return true;
                return customer.CustomerName.ToLower().Contains(SearchText.ToLower()) ||
                       customer.Email.ToLower().Contains(SearchText.ToLower());
            }
            return false;
        }

        private void LoadCustomers()
        {
            Customers.Clear();
            foreach (var cust in _customerDal.GetAllCustomers())
                Customers.Add(cust);
        }

        private void AddCustomer()
        {

            var addView = new View.AddNewCustomer();
            var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
           

        }

        private void ViewCustomer(Customer customer)
        {
            if (customer != null)
            {
                var dialogWindow = new Window();
                var viewCustomerControl = new View.ViewCustomerWindow();
                viewCustomerControl.DataContext = customer;
                dialogWindow.Content = viewCustomerControl;
                dialogWindow.SizeToContent = SizeToContent.WidthAndHeight;
                dialogWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                dialogWindow.ShowDialog();
            }
        }

        private void EditCustomer(Customer customer) { }

        private void DeleteCustomer(Customer customer)
        {
            if (customer == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete {customer.CustomerName}?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _customerDal.DeleteCustomer(customer.CustomerID);
                    LoadCustomers();
                    CustomersView.Refresh();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Error deleting customer: {ex.Message}",
                        "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
