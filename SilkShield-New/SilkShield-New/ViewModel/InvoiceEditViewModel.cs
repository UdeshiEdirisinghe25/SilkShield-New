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
using System.ComponentModel;

namespace SilkShield_New.ViewModel
{
    public class InvoiceEditViewModel : NewInvoice1ViewModel
    {
        private readonly InvoiceRepository _repository;
        private string _originalInvoiceNumber;
        private bool _isLoading = false;

        public ICommand UpdateInvoiceCommand { get; }
        public ICommand CancelCommand { get; }

        public InvoiceEditViewModel(Invoice selectedInvoice) : base()
        {
            _repository = new InvoiceRepository();
            UpdateInvoiceCommand = new RelayCommand(async (p) => await SaveUpdatedInvoice(), (p) => CanUpdate());
            CancelCommand = new RelayCommand((p) => CancelEdit());

            if (selectedInvoice == null) return;

            _isLoading = true;
            _originalInvoiceNumber = selectedInvoice.InvoiceNumber;

            // Map Header
            this.InvoiceNumber = selectedInvoice.InvoiceNumber;
            this.InvoiceDate = selectedInvoice.InvoiceDate;
            this.CustomerName = selectedInvoice.CustomerName;
            this.Location = selectedInvoice.Location;
            this.BuildingType = selectedInvoice.BuildingType;
            this.CurtainLayerType = selectedInvoice.CurtainLayerType;
            this.CurtainStyle = selectedInvoice.CurtainStyle;
            this.PaymentMethod = selectedInvoice.PaymentMethod;
            this.IsPelmetBoardChecked = selectedInvoice.PelmetBoard;
            this.IsMotorizedChecked = selectedInvoice.Motorized;
            this.TransportLaborCostText = selectedInvoice.TransportLaborCost.ToString();
            this.DiscountText = selectedInvoice.Discount.ToString();
            // Restore saved checkbox state
            this.IncludeDetailsPage = selectedInvoice.IncludeDetailsPage;

            // Map Items
            var itemsFromDb = _invoiceDataService.GetInvoiceItemsByNumber(selectedInvoice.InvoiceNumber);
            this.Items.Clear();

            foreach (var item in itemsFromDb)
            {
                var newItem = new InvoiceItem
                {
                    ItemName = item.ItemName,
                    SelectedMaterial = item.SelectedMaterial,
                    MeasuringUnit = item.MeasuringUnit,
                    UnitPrice = item.UnitPrice,
                    Quantity = item.Quantity
                };
                // FIXED: Don't set Total directly (it's read-only). Call CalculateTotal() instead.
                newItem.CalculateTotal();
                this.Items.Add(newItem);
            }

            _isLoading = false;
            CalculateGrandTotal();
        }

        // FIXED: Added 'override' and fixed protection level error (CS0115 / CS0122)
        protected override void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_isLoading) return;
            base.OnItemPropertyChanged(sender, e);
        }

        private bool CanUpdate() => !string.IsNullOrWhiteSpace(this.CustomerName) && this.Items?.Count > 0;

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
                    Items = this.Items,
                    // Persist include flag with the updated invoice object
                    IncludeDetailsPage = this.IncludeDetailsPage
                };

                if (_repository.UpdateInvoice(updatedInvoice, _originalInvoiceNumber))

                     DashboardViewModel.Instance?.LoadDashboard();
                {
                    SaveFileDialog sfd = new SaveFileDialog { Filter = "PDF Files|*.pdf", FileName = $"Invoice_{this.InvoiceNumber}" };
                    if (sfd.ShowDialog() == true)
                    {
                        // Pass the current VM flag when generating the PDF
                        await Task.Run(() => _invoiceDataService.GenerateInvoicePdf(updatedInvoice, sfd.FileName, this.IncludeDetailsPage));
                        MessageBox.Show("Invoice updated successfully!");
                        ReturnToHistory();
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void CancelEdit() => ReturnToHistory();

        private void ReturnToHistory()
        {
            var mw = Application.Current.MainWindow as SilkShield_New.View.MainWindow;
            if (mw != null) { mw.History_Click(null, null); try { mw.RefreshHistoryIfActive(); } catch { } }
        }
    }
}