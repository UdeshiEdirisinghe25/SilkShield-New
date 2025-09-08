using System.Windows;
using System.Windows.Controls;
using SilkShield_New.ViewModel;

namespace SilkShield_New.View
{
    public partial class DashboardWindow : UserControl
    {
        public DashboardWindow()
        {
            InitializeComponent();
            DataContext = new DashboardViewModel();
        }
    }

}
