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
            this.DataContext = new ViewModel.NewInvoice1ViewModel();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        
    }
}
