using System.ComponentModel;
using System.Windows.Input;
using System.Windows;
using System;
using System.Data.SQLite;
using System.Windows.Controls;

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

        public ICommand LoginCommand { get; private set; }

        public LoginPageViewModel()
        {
            LoginCommand = new RelayCommand(Login, CanLogin);
        }

        private void Login(object parameter)
        {
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            {
                // Use a custom message box instead of MessageBox.Show
                MessageBox.Show("Please enter both username and password.");
                return;
            }

            // Using the DatabaseHelper to check user credentials
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=C:/YourProjectFolder/Database.db;Version=3;"))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT COUNT(*) FROM user WHERE Username = @username AND Password = @password";

                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@username", Username);
                        command.Parameters.AddWithValue("@password", Password);

                        long count = (long)command.ExecuteScalar();

                        if (count > 0)
                        {
                            // Login successful
                            // Use a custom message box instead of MessageBox.Show
                            MessageBox.Show("Login Successful!");
                            // Navigate to the Dashboard window
                            // We need to handle this in your View.
                        }
                        else
                        {
                            // Login failed
                            // Use a custom message box instead of MessageBox.Show
                            MessageBox.Show("Invalid username or password.");
                        }
                    }
                }
                catch (SQLiteException ex)
                {
                    // Use a custom message box instead of MessageBox.Show
                    MessageBox.Show("Database error: " + ex.Message);
                }
            }
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

    public class CustomCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public CustomCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

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
