using System.Windows;

namespace SilkShield_New.View
{
    public partial class EditCustomer : Window
    {
        public EditCustomer()
        {
            InitializeComponent();
        }

        // Logic to close the window when "Save" is clicked in the ViewModel
        public void Save_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true; // This closes the pop-up and returns 'true'
            this.Close();
        }
    }
}