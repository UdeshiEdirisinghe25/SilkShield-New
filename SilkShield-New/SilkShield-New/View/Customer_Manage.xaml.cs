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
            // Create an instance of AddNewCustomer UserControl
            var addCustomerControl = new AddNewCustomer();

            // Find the parent window that hosts this UserControl
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                // Assuming the main window has a ContentControl named 'MainContent' for dynamic navigation
                var contentControl = parentWindow.FindName("MainContent") as ContentControl;
                if (contentControl != null)
                {
                    contentControl.Content = addCustomerControl;
                }
                else
                {
                    // If no ContentControl exists, just replace the Window content
                    parentWindow.Content = addCustomerControl;
                }
            }
        }

        
    }
}