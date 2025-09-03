using System.Windows;
using SilkShield_New.ViewModel;

namespace SilkShield_New.View
{
    public partial class NewInvoice1 : Window
    {
        public NewInvoice1()
        {
            InitializeComponent();
            this.DataContext = new NewInvoice1ViewModel();
        }
    }
}