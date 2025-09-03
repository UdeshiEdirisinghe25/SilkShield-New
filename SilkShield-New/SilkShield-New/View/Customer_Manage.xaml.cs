using System.Windows;
using System.Windows.Controls;

namespace SilkShield_New.View
{
    public partial class Customer_Manage : UserControl
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
            var mainWindow = (MainWindow)Application.Current.MainWindow;

            // Replace content with AddNewCustomer view
            mainWindow.MainContentArea.Content = new AddNewCustomer();
        }
    }
}
