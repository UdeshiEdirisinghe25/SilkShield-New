using SilkShield_New.Model;
using SilkShield_New.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;

namespace SilkShield_New.View
{
    public partial class InvoiceEdit : UserControl
    {
        public InvoiceEdit(Invoice invoiceToEdit)
        {
            InitializeComponent();

            try
            {
                // Inject the selected invoice into the ViewModel
                this.DataContext = new InvoiceEditViewModel(invoiceToEdit);
            }
            catch (Exception ex)
            {
                // Surface the exception so we can see what's wrong instead of closing the window
                MessageBox.Show($"Failed to open invoice editor:\n{ex.Message}\n\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.DataContext = null;
            }
        }
    }
}