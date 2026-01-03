using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SilkShield_New.Model;
using SilkShield_New.View;
using SilkShield_New.ViewModel;

namespace SilkShield_New.View
{
    public partial class MainWindow : Window
    {
        private DashboardWindow dashboardView;
        private Customer_Manage customerView;
        private Inventory inventoryView;
        private NewInvoice1 invoiceView;
        private Profile ProfileView;
        private New_Inventory newinventory;
        private AddNewCustomer newCustomerView;
        private HistoryNew historyNewView;


        public MainWindow()
        {
            InitializeComponent();

            dashboardView = new DashboardWindow();
            customerView = new Customer_Manage();
            inventoryView = new Inventory();
            invoiceView = new NewInvoice1();
            ProfileView = new Profile();
            newinventory = new New_Inventory();
            newCustomerView = new AddNewCustomer();
            historyNewView = new HistoryNew();

            customerView.AddNewCustomerRequested += CustomerView_AddNewCustomerRequested;

            inventoryView.NewInventoryRequested += InventoryView_NewInventoryRequested;


            MainContentArea.Content = dashboardView;

            HighlightButton(dashboard);//default
        }


        public void NavigateToEditInvoice(Invoice invoiceToEdit)
        {
            // Create the view ONLY when we have an invoice to show
            var editView = new InvoiceEdit(invoiceToEdit);
            MainContentArea.Content = editView;
            HighlightButton(history);
        }

        private void HighlightButton(Button activeButton)
        {

            dashboard.Style = (Style)FindResource("NavButtonStyle");
            customer.Style = (Style)FindResource("NavButtonStyle");
            invoice.Style = (Style)FindResource("PromoButtonStyle");
            inventory.Style = (Style)FindResource("NavButtonStyle");
            history.Style = (Style)FindResource("NavButtonStyle");
            // Add other buttons if needed...


            activeButton.Style = (Style)FindResource("ActiveNavButtonStyle");
        }

        private void InventoryView_NewInventoryRequested(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = newinventory;
        }


        private void dashboard_click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = dashboardView;
            HighlightButton(dashboard);
        }


        public void customer_click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = customerView;
            HighlightButton(customer);
        }

        private void Invoice_Click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = invoiceView;
            HighlightButton(invoice);
        }
        public void Inventory_Click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = inventoryView;
            HighlightButton(inventory);
        }

        private void UserProfile_Click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = ProfileView;
            HighlightButton(Profile);
        }



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
            dashboardView = null;
            customerView = null;
            inventoryView = null;
            invoiceView = null;
            ProfileView = null;
            newinventory = null;
            newCustomerView = null;
            historyNewView = null;

            LoginPage loginPage = new LoginPage();
            loginPage.Show();
            Application.Current.MainWindow = loginPage; Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose; this.Close();
        }
        private void CustomerView_AddNewCustomerRequested(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = newCustomerView;
        }

        public void History_Click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = historyNewView;
            HighlightButton(history);
        }
        // Call this from other viewmodels to refresh Customer_Manage when it's active
        public void RefreshCustomerManageIfActive()
        {
            // MainContentArea is a private field generated from XAML.
            // Expose a small public helper that checks current content and refreshes the VM.
            if (this.MainContentArea?.Content is Customer_Manage manageView &&
                manageView.DataContext is Customer_ManageViewModel manageVm)
            {
                manageVm.RefreshCustomers();
            }
        }

        public void RefreshHistoryIfActive()
        {
            // 1. Check if the current content is the History view
            if (this.MainContentArea?.Content is HistoryNew historyView &&
                historyView.DataContext is HistoryNewViewModel historyVm)
            {
                // 2. Call the public refresh method in the History ViewModel
                historyVm.RefreshData();
            }
        }
        public void RefreshInventoryIfActive()
        {
            // 1. Check if the current view is the Inventory Manage screen
            if (this.MainContentArea?.Content is Inventory invView)
            {
                // 2. ERROR FIX: Ensure you cast to InventoryViewModel, NOT EditInventoryViewModel
                if (invView.DataContext is InventoryViewModel invVm)
                {
                    // 3. This call will now work because LoadInventoryData exists in InventoryViewModel
                    invVm.LoadInventoryData();
                }
            }
        }

    }
}