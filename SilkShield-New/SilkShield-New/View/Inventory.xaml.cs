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
using SilkShield_New.ViewModel;
using SilkShield_New.Data;

namespace SilkShield_New.View
{
    /// <summary>
    /// Interaction logic for Inventory.xaml
    /// </summary>
    public partial class Inventory : Window
    {
        private InventoryViewModel _viewModel;

        public Inventory()
        {
            InitializeComponent();
            InitializeViewModel();
        }

        private void InitializeViewModel()
        {
            _viewModel = new InventoryViewModel();
            this.DataContext = _viewModel;

            // Subscribe to property changes to handle selection highlighting
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(InventoryViewModel.SelectedItem))
            {
                // Highlight the selected item
                HighlightSelectedItem();
            }
        }

        private void HighlightSelectedItem()
        {
            if (_viewModel.SelectedItem != null)
            {
                // Find the item in DataGrid and scroll to it
                for (int i = 0; i < dataGridView1.Items.Count; i++)
                {
                    var item = dataGridView1.Items[i] as InventoryItem;
                    if (item != null && item.ItemID == _viewModel.SelectedItem.ItemID)
                    {
                        dataGridView1.SelectedIndex = i;
                        dataGridView1.ScrollIntoView(dataGridView1.SelectedItem);

                        // Apply highlight styling
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            var row = dataGridView1.ItemContainerGenerator.ContainerFromIndex(i) as DataGridRow;
                            if (row != null)
                            {
                                row.Background = new SolidColorBrush(Color.FromRgb(139, 69, 19)); // Maroon color
                                row.Foreground = Brushes.White;

                                // Reset after 3 seconds
                                var timer = new System.Windows.Threading.DispatcherTimer();
                                timer.Interval = TimeSpan.FromSeconds(3);
                                timer.Tick += (s, e) =>
                                {
                                    row.Background = Brushes.Transparent;
                                    row.Foreground = new SolidColorBrush(Color.FromRgb(31, 41, 55)); // Original text color
                                    timer.Stop();
                                };
                                timer.Start();
                            }
                        }), System.Windows.Threading.DispatcherPriority.Loaded);
                        break;
                    }
                }
            }
        }

        // The SearchTextBox_KeyDown method has been removed.
        // The real-time filtering is now handled solely by the ViewModel.

        // Optional: Handle window closing
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }
            base.OnClosing(e);
        }
    }
}