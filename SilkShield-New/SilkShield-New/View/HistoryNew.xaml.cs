using SilkShield_New.ViewModel;
using System.Windows;
using System.Windows.Controls;


namespace SilkShield_New.View
{
    public partial class HistoryNew : UserControl
    {
        public HistoryNew()
        {
            InitializeComponent();
            this.DataContext = new HistoryNewViewModel(); // Important for the refresh to work!
        }

    }
}

