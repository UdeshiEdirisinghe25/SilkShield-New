using System.Windows;
using System.Windows.Controls;
using SilkShield_New.ViewModel;

namespace SilkShield_New.View
{
    public partial class NewInvoice1 : UserControl
    {
        public NewInvoice1()
        {
            InitializeComponent();
            this.DataContext = new NewInvoice1ViewModel();
        }
    }
}
