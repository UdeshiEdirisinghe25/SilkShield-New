using SilkShield_New.Data;
using SilkShield_New.Model;
using SilkShield_New.View;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace SilkShield_New.ViewModel
{
    public class AddNewCustomerViewModel
    {
        private CustomerDAL _customerDal;

        public Customer NewCustomer { get; set; }

        public ICommand SaveCustomerCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        public AddNewCustomerViewModel()
        {
            _customerDal = new CustomerDAL();
            NewCustomer = new Customer();

            SaveCustomerCommand = new RelayCommand(_ => SaveCustomer());
            CancelCommand = new RelayCommand(_ => Cancel());
        }

        public void SaveCustomer()
        {
            string name = NewCustomer.CustomerName?.Trim();
            string phone = NewCustomer.PhoneNumber?.Trim();

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(phone))
            {
                MessageBox.Show("Customer Name and Phone Number are required fields.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool isSaved = _customerDal.InsertCustomer(NewCustomer);

            // Inside SaveCustomer() in AddNewCustomerViewModel.cs
            if (isSaved)
            {
                MessageBox.Show("Customer saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                var mainWindow = Application.Current.MainWindow as SilkShield_New.View.MainWindow;

                if (Application.Current.MainWindow is SilkShield_New.View.MainWindow mainWin)
                {
                    mainWin.customer_click(null, null);

                    mainWin.RefreshCustomerManageIfActive();
                }

                if (DashboardViewModel.Instance != null)
                {
                    DashboardViewModel.Instance.LoadDashboard();
                }

                CloseWindow();
            }
            else
            {
                MessageBox.Show("Customer could not be saved. Please check the data.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel()
        {
            // Try to close popup window if this VM is hosted in a Window
            var popupWindow = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.DataContext == this);
            if (popupWindow != null)
            {
                popupWindow.Close();
                return;
            }

            // Otherwise, if this VM is used inside MainWindow (as a UserControl), navigate back to Customer_Manage
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                // If the current MainWindow.Content is the view that uses this VM, replace it
                try
                {
                    if (mainWindow.Content is FrameworkElement fe && fe.DataContext == this)
                    {
                        mainWindow.Content = new Customer_Manage();
                        return;
                    }
                }
                catch
                {
                }
            }
        }

        private void CloseWindow()
        {
            // Use ToList() to avoid "Collection Modified" errors if multiple windows close
            var window = Application.Current.Windows
                .Cast<Window>()
                .FirstOrDefault(w => w.DataContext == this);

            window?.Close();
        }
    }
}