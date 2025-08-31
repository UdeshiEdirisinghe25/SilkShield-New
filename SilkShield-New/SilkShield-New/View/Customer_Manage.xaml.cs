using System.Windows;

namespace SilkShield_New.View
{
    public partial class Customer_Manage : Window
    {
        public Customer_Manage()
        {
            InitializeComponent();
            LoadCustomers();
        }

        private void LoadCustomers()
        {
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Open AddNewCustomer window
            AddNewCustomer addWindow = new AddNewCustomer();
            addWindow.ShowDialog();

            // Refresh the DataGrid after adding a new customer
            LoadCustomers();
        }
    }
}
