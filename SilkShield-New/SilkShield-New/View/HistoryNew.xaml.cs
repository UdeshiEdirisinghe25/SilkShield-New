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

            var viewModel = new HistoryViewModel();
            this.DataContext = viewModel;

            viewModel.LoadInvoices();
        }
    }
}