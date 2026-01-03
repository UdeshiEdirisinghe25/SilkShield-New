using Microsoft.Win32;
using SilkShield_New.Data;
using SilkShield_New.Model;
using SilkShield_New.Service;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SilkShield_New.ViewModel
{
    public class InvoiceEditViewModel : NewInvoice1ViewModel
    {
        private readonly InvoiceDataService _dataService;
        private readonly InvoiceRepository _repository;
        private readonly InvoiceNumberService _invoiceNumberService;

        private string _originalInvoiceNumber;

        // Commands bound from InvoiceEdit.xaml
        public ICommand UpdateInvoiceCommand { get; }
        public ICommand CancelCommand { get; }

        public InvoiceEditViewModel(Invoice selectedInvoice) : base()
        {
            _dataService = new InvoiceDataService();
            _repository = new InvoiceRepository();
            _invoiceNumberService = new InvoiceNumberService(); // Initialized this service

            // Initialize commands - changed to async call
            UpdateInvoiceCommand = new RelayCommand(async (p) => await SaveUpdatedInvoice(), (p) => CanUpdate());
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

            CommandManager.InvalidateRequerySuggested();
        }

        private bool CanUpdate()
        {
            if (string.IsNullOrWhiteSpace(this.InvoiceNumber)) return false;
            if (this.Items == null || this.Items.Count == 0) return false;
            return true;
        }

        // Added 'async Task' to allow 'await'
        public async Task SaveUpdatedInvoice()
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

                // Save to DB first
                bool success = _repository.UpdateInvoice(updatedInvoice, _originalInvoiceNumber);

                if (success)
                {
                    // Initialize the SaveFileDialog (Fixes CS0103)
                    SaveFileDialog saveFileDialog = new SaveFileDialog
                    {
                        Filter = "PDF Files (*.pdf)|*.pdf",
                        FileName = $"Invoice_{updatedInvoice.InvoiceNumber}"
                    };

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        string filePath = saveFileDialog.FileName;

                        // 1. Generate PDF in background using the updatedInvoice object
                        // Note: Using updatedInvoice instead of undefined invoiceData
                        await Task.Run(() => _dataService.GenerateInvoicePdf(updatedInvoice, filePath));

                        // 2. Show Success Message
                        MessageBox.Show(
                            $"Invoice {updatedInvoice.InvoiceNumber} successfully updated and saved!\n" +
                            $"PDF saved to: {filePath}",
                            "Invoice Updated",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information
                        );

                        // 3. Navigation and Refresh
                        var mainWindow = System.Windows.Application.Current.MainWindow as SilkShield_New.View.MainWindow;
                        if (mainWindow != null)
                        {
                            mainWindow.History_Click(null, null);
                            try
                            {
                                mainWindow.RefreshHistoryIfActive();
                            }
                            catch { }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating invoice: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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