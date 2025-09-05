using System.Windows;
using System.Windows.Controls;
using SilkShield_New.ViewModel;

namespace SilkShield_New.View
{
    public partial class Customer_Manage : UserControl
    {
        public Customer_Manage()
        {
            InitializeComponent();
            this.DataContext = new Customer_ManageViewModel();
        }

    }
}