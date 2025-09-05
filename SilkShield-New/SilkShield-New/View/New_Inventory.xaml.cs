using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SilkShield_New.View
{
    /// <summary>
    /// Interaction logic for New_Inventory.xaml
    /// </summary>
    public partial class New_Inventory : Window
    {
        public New_Inventory()
        {
            InitializeComponent();
            this.DataContext = new NewInventoryViewModel();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
