using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SilkShield_New.Model;
using SilkShield_New.View;

namespace SilkShield_New.View
{
    public partial class MainWindow : Window
    {
        private DashboardWindow dashboardView;
        private Customer_Manage customerView;
        //private InventoryView inventoryView;
        //private Invoice invoiceView;
        //private InvoiceHistory invoiceHistory;

        public MainWindow()
        {
            InitializeComponent();

            dashboardView = new DashboardWindow();
            customerView = new Customer_Manage();
            //inventoryView = new InventoryView();
            //invoiceView = new Invoice();
            //invoiceHistory = new InvoiceHistory();

            MainContentArea.Content = dashboardView;

            HighlightButton(dashboard);//default
        }

        private void HighlightButton(Button activeButton)
        {
            // Reset all sidebar buttons
            foreach (var child in Sidebar.Children)
            {
                if (child is Button btn)
                {
                    btn.Style = (Style)FindResource("NavButtonStyle");
                }
            }

            // Apply active style to selected button
            activeButton.Style = (Style)FindResource("ActiveNavButtonStyle");
        }

        private void dashboard_click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = dashboardView;
            HighlightButton(dashboard);
        }

        
        private void customer_click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = customerView;
            HighlightButton(customer);
        }

        // private void Invoice_click(object sender, RoutedEventArgs e)
        //{
          //  MainContentArea.Content = InvoiceView;
            //HighlightButton(invoice);
        //}
        


        private void TopBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Allow dragging the window
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeRestore_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
            else
                this.WindowState = WindowState.Maximized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Logout_Click(object sender, RoutedEventArgs e)
        {

            // LoginPage.show();
            //this.Close();

        }


    }
}
