using System;
using System.Windows;
using System.Windows.Input;
using SilkShield_New.Data;
using SilkShield_New.Model;

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

        private void SaveCustomer()
        {
            string name = NewCustomer.CustomerName?.Trim();
            string phone = NewCustomer.PhoneNumber?.Trim();

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(phone))
            {
                MessageBox.Show("Customer Name and Phone Number are required fields.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool isSaved = _customerDal.InsertCustomer(NewCustomer);

            if (isSaved)
            {
                MessageBox.Show("Customer saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseWindow();
            }
            else
            {
                MessageBox.Show("Customer could not be saved. Please check the data.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel()
        {
            CloseWindow();
        }

        private void CloseWindow()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    window.Close();
                    break;
                }
            }
        }



    }
}