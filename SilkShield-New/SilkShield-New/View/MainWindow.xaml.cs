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

        

       


    }
}
