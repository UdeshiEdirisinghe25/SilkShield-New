using System;
using System.Windows;
using System.Windows.Controls;
using SilkShield_New.ViewModel;

namespace SilkShield_New.View
{
    public partial class AddNewCustomer : UserControl
    {
        public AddNewCustomer()
        {
            InitializeComponent();


            if (DataContext == null)
                DataContext = new AddNewCustomerViewModel();
        }

        
    }
}