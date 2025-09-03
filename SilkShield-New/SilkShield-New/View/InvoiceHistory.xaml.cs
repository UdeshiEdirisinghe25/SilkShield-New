using System.Windows;

namespace SilkShield_New.View
{
    public partial class InvoiceHistory : Window
    {
        public InvoiceHistory()
        {
            InitializeComponent();
        }

        private void ClearSearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
        }

        // Common clear button for both From and To dates
        private void ClearDates_Click(object sender, RoutedEventArgs e)
        {
            FromDatePicker.SelectedDate = null;
            ToDatePicker.SelectedDate = null;
        }
    }
}
