using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using SilkShield_New.Model;
using SilkShield_New.Data;

namespace SilkShield_New.ViewModel
{
    public class InvoiceEditViewModel : NewInvoice1ViewModel
    {
        private readonly InvoiceDataService _dataService;
        private readonly InvoiceRepository _repository;
        private string _originalInvoiceNumber;

        // Commands bound from InvoiceEdit.xaml
        public ICommand UpdateInvoiceCommand { get; }
        public ICommand CancelCommand { get; }

        public InvoiceEditViewModel(Invoice selectedInvoice) : base()
        {
            _dataService = new InvoiceDataService();
            _repository = new InvoiceRepository();

            // Initialize commands
            UpdateInvoiceCommand = new RelayCommand((p) => SaveUpdatedInvoice(), (p) => CanUpdate());
            CancelCommand = new RelayCommand((p) => CancelEdit());

            if (selectedInvoice == null) return;

            _originalInvoiceNumber = selectedInvoice.InvoiceNumber;

            // --- 1. SET HEADER DATA ---
            this.InvoiceNumber = selectedInvoice.InvoiceNumber;
            this.InvoiceDate = selectedInvoice.InvoiceDate;
            this.CustomerName = selectedInvoice.CustomerName;
            this.Location = selectedInvoice.Location;
            this.BuildingType = selectedInvoice.BuildingType;
            this.CurtainLayerType = selectedInvoice.CurtainLayerType;
            this.CurtainStyle = selectedInvoice.CurtainStyle;

            // PaymentMethod (matches base class)
            this.PaymentMethod = selectedInvoice.PaymentMethod;

            // --- 2. SET CHECKBOXES ---
            this.IsPelmetBoardChecked = selectedInvoice.PelmetBoard;
            this.IsMotorizedChecked = selectedInvoice.Motorized;

            // --- 3. SET COSTS & DISCOUNT ---
            this.TransportLaborCostText = selectedInvoice.TransportLaborCost.ToString();
            this.DiscountText = selectedInvoice.Discount.ToString();

            // --- 4. RETRIEVE AND SET ITEMS ---
            var itemsFromDb = _dataService.GetInvoiceItemsByNumber(selectedInvoice.InvoiceNumber);

            this.Items.Clear();
            foreach (var item in itemsFromDb)
            {
                this.Items.Add(item);
            }

            // Force CommandManager to requery CanExecute
            CommandManager.InvalidateRequerySuggested();
        }

        private bool CanUpdate()
        {
            // Basic guard: must have a valid invoice number and at least one item
            if (string.IsNullOrWhiteSpace(this.InvoiceNumber)) return false;
            if (this.Items == null || this.Items.Count == 0) return false;
            return true;
        }

        public void SaveUpdatedInvoice()
        {
            try
            {
                var updatedInvoice = new Invoice
                {
                    InvoiceNumber = this.InvoiceNumber,
                    InvoiceDate = this.InvoiceDate,
                    CustomerName = this.CustomerName,
                    Location = this.Location,
                    BuildingType = this.BuildingType,
                    CurtainLayerType = this.CurtainLayerType,
                    CurtainStyle = this.CurtainStyle,
                    PelmetBoard = this.IsPelmetBoardChecked,
                    Motorized = this.IsMotorizedChecked,
                    PaymentMethod = this.PaymentMethod,

                    TransportLaborCost = double.TryParse(this.TransportLaborCostText, out double t) ? t : 0,
                    Discount = double.TryParse(this.DiscountText, out double d) ? d : 0,

                    Items = new ObservableCollection<InvoiceItem>(this.Items)
                };

                bool success = _repository.UpdateInvoice(updatedInvoice, _originalInvoiceNumber);

                if (success)
                {
                    MessageBox.Show("Invoice updated successfully!");
                    CancelEdit();
                }
            }
            catch (Exception ex)
            {
                // Show full exception to help debugging (remove stack trace in production)
                MessageBox.Show($"Failed to update invoice:\n{ex.Message}\n\n{ex.InnerException?.Message}", "Update Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelEdit()
        {
            var mainWindow = Application.Current.MainWindow as SilkShield_New.View.MainWindow;
            if (mainWindow != null)
            {
                mainWindow.History_Click(null, null);
            }
        }
    }
}