using SilkShield_New.ViewModel;
using System.Windows;
using System.Windows.Controls;


namespace SilkShield_New.View
{
    public partial class HistoryNew : Window
    {
        public HistoryNew()
        {
            InitializeComponent();

            var viewModel = new HistoryNewViewModel();
            this.DataContext = viewModel;

            viewModel.LoadInvoices();
        }
    }
}
