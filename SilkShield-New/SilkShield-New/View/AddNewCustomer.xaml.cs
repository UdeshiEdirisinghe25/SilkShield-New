using System.Windows;
using SilkShield_New.ViewModel;

namespace SilkShield_New.View
{
    public partial class AddNewCustomer : Window
    {
        public AddNewCustomer()
        {
            InitializeComponent();

            // Set the ViewModel as the DataContext
            this.DataContext = new AddNewCustomerViewModel();
        }
    }
}