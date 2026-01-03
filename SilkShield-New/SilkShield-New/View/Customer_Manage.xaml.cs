using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SilkShield_New.Model;
using SilkShield_New.ViewModel;

namespace SilkShield_New.View
{
    public partial class Customer_Manage : UserControl
    {
        public event RoutedEventHandler AddNewCustomerRequested;
        public Customer_Manage()
        {
            InitializeComponent();
            this.DataContext = new Customer_ManageViewModel();
        }



        private void DataGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
                {
                    RoutedEvent = UIElement.MouseWheelEvent,
                    Source = sender
                };

                var parent = ((Control)sender).Parent as UIElement;
                parent?.RaiseEvent(eventArg);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AddNewCustomerRequested?.Invoke(this, e);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // Create and show the AddNewCustomer window as a dialog
            var addCustomerWindow = new AddNewCustomer
            {
                Owner = Window.GetWindow(this),
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            addCustomerWindow.ShowDialog();
        }

        
    }
}