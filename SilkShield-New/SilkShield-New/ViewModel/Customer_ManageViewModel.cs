using System.Collections.ObjectModel;
using System.Windows;
using SilkShield_New.Data;
using SilkShield_New.Model;

namespace SilkShield_New.ViewModel
{
    public class Customer_ManageViewModel
    {
        private CustomerDAL _customerDal;

        public ObservableCollection<Customer> Customers { get; set; }

        public RelayCommand AddCustomerCommand { get; set; }
        public RelayCommand ViewCustomerCommand { get; set; }
        public RelayCommand EditCustomerCommand { get; set; }
        public RelayCommand DeleteCustomerCommand { get; set; }

        public Customer_ManageViewModel()
        {
            _customerDal = new CustomerDAL();
            Customers = new ObservableCollection<Customer>(_customerDal.GetAllCustomers());

            AddCustomerCommand = new RelayCommand(_ => AddCustomer());
            ViewCustomerCommand = new RelayCommand(param => ViewCustomer(param as Customer));
            EditCustomerCommand = new RelayCommand(param => EditCustomer(param as Customer));
            DeleteCustomerCommand = new RelayCommand(param => DeleteCustomer(param as Customer));
        }

        private void LoadCustomers()
        {
            Customers.Clear();
            foreach (var cust in _customerDal.GetAllCustomers())
                Customers.Add(cust);
        }

        private void AddCustomer()
        {
            // Create a new window to host the UserControl
            var dialogWindow = new Window();

            // Create an instance of the UserControl
            var addCustomerControl = new View.AddNewCustomer();

            // Set the UserControl as the content of the new window
            dialogWindow.Content = addCustomerControl;
            dialogWindow.SizeToContent = SizeToContent.WidthAndHeight;
            dialogWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            // Show the new window as a dialog
            dialogWindow.ShowDialog();

            // Reload your customer data after the dialog is closed
            LoadCustomers();
        }

        private void ViewCustomer(Customer customer)
        {
            if (customer != null)
                MessageBox.Show($"View clicked for: {customer.CustomerName}");
        }

        private void EditCustomer(Customer customer)
        {
            if (customer != null)
                MessageBox.Show($"Edit clicked for: {customer.CustomerName}");
        }

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
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Error deleting customer: {ex.Message}",
                        "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}