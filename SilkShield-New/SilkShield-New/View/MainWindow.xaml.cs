using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SilkShield_New.View
{
    public partial class MainWindow : Window
    {
        private DashboardWindow dashboardView;
        private CustomerDetailsView customerView;
        private InventoryView inventoryView;
        private Invoice invoiceView;
        private InvoiceHistory invoiceHistory;

        public MainWindow()
        {
            InitializeComponent();

            dashboardView = new DashboardWindow();
            customerView = new CustomerDetailsView();
            inventoryView = new InventoryView();
            invoiceView = new Invoice();
            invoiceHistory = new InvoiceHistory();

            MainContentArea.Content = dashboardView;

            HighlightButton(dashboard);
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

        private void invoice_click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = invoiceView;

        }

        private void inv_history_click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = invoiceHistory;
            HighlightButton(InvoiceHistory);
        }

        private void customer_click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = customerView;
            HighlightButton(customer);
        }

        private void inventory_click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = inventoryView;
            HighlightButton(inventory);
        }


        private void TopBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Allow dragging the window
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }





    }
}
