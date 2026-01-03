using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Data.SQLite;
using SilkShield_New.Model;
using SilkShield_New.Data;

namespace SilkShield_New.ViewModel
{
    public class EditCustomerViewModel : INotifyPropertyChanged
    {
        private readonly CustomerDAL _customerDal;
        private readonly DatabaseHelper _dbHelper;

        // CRITICAL FIX: The XAML uses {Binding NewCustomer.PropertyName}.
        // This property returns "this" (the ViewModel itself) so the XAML can find the fields.
        public EditCustomerViewModel NewCustomer => this;

        #region Flattened Properties
        private int _customerID;
        public int CustomerID
        {
            get => _customerID;
            set { _customerID = value; OnPropertyChanged(nameof(CustomerID)); }
        }

        private string _customerName;
        public string CustomerName
        {
            get => _customerName;
            set { _customerName = value; OnPropertyChanged(nameof(CustomerName)); }
        }

        private string _customerType;
        public string CustomerType
        {
            get => _customerType;
            set { _customerType = value; OnPropertyChanged(nameof(CustomerType)); }
        }

        private string _phoneNumber;
        public string PhoneNumber
        {
            get => _phoneNumber;
            set { _phoneNumber = value; OnPropertyChanged(nameof(PhoneNumber)); }
        }

        private string _email;
        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(nameof(Email)); }
        }

        private string _address;
        public string Address
        {
            get => _address;
            set { _address = value; OnPropertyChanged(nameof(Address)); }
        }

        private string _visitedStatus;
        public string VisitedStatus
        {
            get => _visitedStatus;
            set { _visitedStatus = value; OnPropertyChanged(nameof(VisitedStatus)); }
        }

        private string _projectConfirmation;
        public string ProjectConfirmation
        {
            get => _projectConfirmation;
            set { _projectConfirmation = value; OnPropertyChanged(nameof(ProjectConfirmation)); }
        }

        private string _quotationStatus;
        public string QuotationStatus
        {
            get => _quotationStatus;
            set { _quotationStatus = value; OnPropertyChanged(nameof(QuotationStatus)); }
        }

        private string _propertyDetails;
        public string Property_Details
        {
            get => _propertyDetails;
            set { _propertyDetails = value; OnPropertyChanged(nameof(Property_Details)); }
        }

        private string _projectStartDate;
        public string Project_Start_Date
        {
            get => _projectStartDate;
            set { _projectStartDate = value; OnPropertyChanged(nameof(Project_Start_Date)); }
        }

        private string _expectedCompletion;
        public string Expected_Dateof_Completion
        {
            get => _expectedCompletion;
            set { _expectedCompletion = value; OnPropertyChanged(nameof(Expected_Dateof_Completion)); }
        }

        private string _specialPreferences;
        public string Special_Preferances
        {
            get => _specialPreferences;
            set { _specialPreferences = value; OnPropertyChanged(nameof(Special_Preferances)); }
        }

        private string _notes;
        public string Notes
        {
            get => _notes;
            set { _notes = value; OnPropertyChanged(nameof(Notes)); }
        }
        #endregion

        public ICommand UpdateCustomerCommand { get; }
        public ICommand CancelCommand { get; }

        public EditCustomerViewModel(Customer customer)
        {
            _customerDal = new CustomerDAL();
            _dbHelper = new DatabaseHelper();

            if (customer != null)
            {
                // Retrieve/Map data into the local properties
                this.CustomerID = customer.CustomerID;
                this.CustomerName = customer.CustomerName;
                this.CustomerType = customer.CustomerType;
                this.PhoneNumber = customer.PhoneNumber;
                this.Email = customer.Email;
                this.Address = customer.Address;
                this.VisitedStatus = customer.VisitedStatus;
                this.ProjectConfirmation = customer.ProjectConfirmation;
                this.QuotationStatus = customer.QuotationStatus;
                this.Property_Details = customer.Property_Details;
                this.Project_Start_Date = customer.Project_Start_Date;
                this.Expected_Dateof_Completion = customer.Expected_Dateof_Completion;
                this.Special_Preferances = customer.Special_Preferances;
                this.Notes = customer.Notes;
            }

            UpdateCustomerCommand = new RelayCommand(_ => UpdateCustomer());
            CancelCommand = new RelayCommand(_ => Cancel());

            // Notify the UI that NewCustomer (the proxy) is ready
            OnPropertyChanged(nameof(NewCustomer));
        }

        private void UpdateCustomer()
        {
            if (string.IsNullOrWhiteSpace(this.CustomerName))
            {
                MessageBox.Show("Customer Name is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var conn = _dbHelper.GetConnection())
                {
                    conn.Open();
                    string sql = @"UPDATE customer_details SET 
                                    CustomerType=@CT, VisitedStatus=@VS, CustomerName=@CN, Address=@AD, 
                                    ProjectConfirmation=@PC, Email=@EM, PhoneNumber=@PN, QuotationStatus=@QS, 
                                    Property_Details=@PD, Project_Start_Date=@PSD, Expected_Dateof_Completion=@EDC, 
                                    Special_Preferances=@SP, Notes=@NT 
                                   WHERE CustomerID=@ID";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@CT", CustomerType ?? "");
                        cmd.Parameters.AddWithValue("@VS", VisitedStatus ?? "");
                        cmd.Parameters.AddWithValue("@CN", CustomerName ?? "");
                        cmd.Parameters.AddWithValue("@AD", Address ?? "");
                        cmd.Parameters.AddWithValue("@PC", ProjectConfirmation ?? "");
                        cmd.Parameters.AddWithValue("@EM", Email ?? "");
                        cmd.Parameters.AddWithValue("@PN", PhoneNumber ?? "");
                        cmd.Parameters.AddWithValue("@QS", QuotationStatus ?? "");
                        cmd.Parameters.AddWithValue("@PD", Property_Details ?? "");
                        cmd.Parameters.AddWithValue("@PSD", Project_Start_Date ?? "");
                        cmd.Parameters.AddWithValue("@EDC", Expected_Dateof_Completion ?? "");
                        cmd.Parameters.AddWithValue("@SP", Special_Preferances ?? "");
                        cmd.Parameters.AddWithValue("@NT", Notes ?? "");
                        cmd.Parameters.AddWithValue("@ID", CustomerID);

                        int result = cmd.ExecuteNonQuery();
                
                if (result > 0)
                {
                    MessageBox.Show("Customer updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // --- REFRESH LOGIC START ---
                    RefreshParentUI();
                    // --- REFRESH LOGIC END ---

                    CloseWindow();
                }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }

        }
        private void RefreshParentUI()
        {
            
            if (Application.Current.MainWindow is SilkShield_New.View.MainWindow mainWin)
            {
                // 2. Call the public method you already wrote!
                mainWin.RefreshCustomerManageIfActive();

                
            }
        }

        private void Cancel() => CloseWindow();

        private void CloseWindow()
        {
            foreach (Window win in Application.Current.Windows)
            {
                if (win.DataContext == this) win.Close();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}