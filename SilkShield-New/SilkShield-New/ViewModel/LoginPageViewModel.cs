using System.ComponentModel;
using System.Windows.Input;
using System.Windows;
using System;
using System.Data.SQLite;
using System.Windows.Controls;
using SilkShield_New.Data; // Ensure this is imported to use DatabaseHelper

namespace SilkShield_New.ViewModel
{
    public class LoginPageViewModel : INotifyPropertyChanged
    {
        private string _username;
        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                OnPropertyChanged("Username");
                // Update the CanExecute status of the command
                (LoginCommand as CustomCommand)?.RaiseCanExecuteChanged();
            }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged("Password");
                // Update the CanExecute status of the command
                (LoginCommand as CustomCommand)?.RaiseCanExecuteChanged();
            }
        }

        // An event to notify the View when login is successful, so it can handle navigation.
        public event EventHandler LoginSuccess;

        public ICommand LoginCommand { get; private set; }

        public LoginPageViewModel()
        {
            LoginCommand = new RelayCommand(Login, CanLogin);
        }

        private void Login(object parameter)
        {
            // Use the DatabaseHelper to get a connection.
            var dbHelper = new DatabaseHelper();
            using (SQLiteConnection connection = dbHelper.GetConnection())
            {
                try
                {
                    connection.Open();
                    // Select the password for the given username.
                    string query = "SELECT Password FROM user WHERE Username = @username";

                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@username", Username);
                        object result = command.ExecuteScalar();

                        if (result != null)
                        {
                            string storedPassword = result.ToString();
                            // Check if the entered password matches the stored password.
                            if (storedPassword == Password)
                            {
                                // Login successful, notify the View.
                                MessageBox.Show("Login Successful!");
                                LoginSuccess?.Invoke(this, EventArgs.Empty);
                            }
                            else
                            {
                                // Passwords do not match.
                                MessageBox.Show("Invalid username or password.");
                                ClearFields();
                            }
                        }
                        else
                        {
                            // Username not found.
                            MessageBox.Show("Invalid username or password.");
                            ClearFields();
                        }
                    }
                }
                catch (SQLiteException ex)
                {
                    MessageBox.Show("Database error: " + ex.Message);
                    ClearFields();
                }
            }
        }

        // Method to clear the input fields after a failed login.
        private void ClearFields()
        {
            Username = null;
            Password = null;
        }

        private bool CanLogin(object parameter)
        {
            // The button will be enabled only if both username and password are not empty.
            return !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // You may need to have this class in a separate file if it is not already.
    public class CustomCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public CustomCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);

        public void Execute(object parameter) => _execute(parameter);

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
