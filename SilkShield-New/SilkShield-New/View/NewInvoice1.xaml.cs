using System.Windows;
using SilkShield_New.ViewModel;

namespace SilkShield_New.View
{
    public partial class NewInvoice1 : Window
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
