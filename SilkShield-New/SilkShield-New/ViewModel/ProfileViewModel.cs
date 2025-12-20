using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Data.SQLite;
using SilkShield_New.Data;
using System.Windows.Media;

namespace SilkShield_New.ViewModel
{
    public class ProfileViewModel : INotifyPropertyChanged
    {
        private DatabaseHelper _dbHelper;
        private int _currentUserId = 1; // Set this based on your current user logic

        // Private fields for properties
        private string _username = "";
        private string _currentPassword = "";
        private string _newPassword = "";
        private string _confirmNewPassword = "";
        private string _validationMessage = "";
        private bool _isLoading = false;

        public ProfileViewModel()
        {
            _dbHelper = new DatabaseHelper();
            ApplyChangesCommand = new CustCommand(ApplyChanges, CanApplyChanges);

            // Load user data when ViewModel is created
            LoadUserData();
        }

        #region Properties

        public string Username
        {
            get => _username;
            set
            {
                if (_username != value)
                {
                    _username = value;
                    OnPropertyChanged();
                    ((CustCommand)ApplyChangesCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string CurrentPassword
        {
            get => _currentPassword;
            set
            {
                if (_currentPassword != value)
                {
                    _currentPassword = value;
                    OnPropertyChanged();
                    ((CustCommand)ApplyChangesCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string NewPassword
        {
            get => _newPassword;
            set
            {
                if (_newPassword != value)
                {
                    _newPassword = value;
                    OnPropertyChanged();
                    ValidateNewPassword();
                    ((CustCommand)ApplyChangesCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string ConfirmNewPassword
        {
            get => _confirmNewPassword;
            set
            {
                if (_confirmNewPassword != value)
                {
                    _confirmNewPassword = value;
                    OnPropertyChanged();
                    ValidatePasswordMatch();
                    ((CustCommand)ApplyChangesCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string ValidationMessage
        {
            get => _validationMessage;
            set
            {
                if (_validationMessage != value)
                {
                    _validationMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                    ((CustCommand)ApplyChangesCommand).RaiseCanExecuteChanged();
                }
            }
        }

        #endregion

        #region Commands

        public ICommand ApplyChangesCommand { get; }

        #endregion

        #region Methods

        private async void LoadUserData()
        {
            try
            {
                IsLoading = true;

                await System.Threading.Tasks.Task.Run(() =>
                {
                    using (var connection = _dbHelper.GetConnection())
                    {
                        connection.Open();
                        string query = "SELECT Username FROM user WHERE UserID = @userId";

                        using (var command = new SQLiteCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@userId", _currentUserId);

                            using (var reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        Username = reader["Username"].ToString();
                                    });
                                }
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                ValidationMessage = $"Error loading user data: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool ValidatePassword(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 8)
                return false;

            bool hasUpper = Regex.IsMatch(password, @"[A-Z]");
            bool hasLower = Regex.IsMatch(password, @"[a-z]");
            bool hasNumber = Regex.IsMatch(password, @"\d");

            return hasUpper && hasLower && hasNumber;
        }

        private string GetPasswordValidationMessage(string password)
        {
            if (string.IsNullOrEmpty(password))
                return "Password cannot be empty.";

            if (password.Length < 8)
                return "Password must be at least 8 characters long.";

            if (!Regex.IsMatch(password, @"[A-Z]"))
                return "Password must contain at least one uppercase letter.";

            if (!Regex.IsMatch(password, @"[a-z]"))
                return "Password must contain at least one lowercase letter.";

            if (!Regex.IsMatch(password, @"\d"))
                return "Password must contain at least one number.";

            return "";
        }

        private void ValidateNewPassword()
        {
            if (!string.IsNullOrEmpty(NewPassword))
            {
                ValidationMessage = GetPasswordValidationMessage(NewPassword);
            }
            else
            {
                ValidationMessage = "";
            }
        }

        private void ValidatePasswordMatch()
        {
            if (!string.IsNullOrEmpty(ConfirmNewPassword) && !string.IsNullOrEmpty(NewPassword))
            {
                if (NewPassword != ConfirmNewPassword)
                {
                    ValidationMessage = "Passwords do not match.";
                }
                else if (ValidatePassword(NewPassword))
                {
                    ValidationMessage = "Password is valid.";
                }
            }
        }

        private bool VerifyCurrentPassword(string inputPassword)
        {
            try
            {
                using (var connection = _dbHelper.GetConnection())
                {
                    connection.Open();
                    string query = "SELECT Password FROM user WHERE UserID = @userId";

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@userId", _currentUserId);

                        var storedPassword = command.ExecuteScalar()?.ToString();
                        return storedPassword == inputPassword;
                    }
                }
            }
            catch (Exception ex)
            {
                ValidationMessage = $"Error verifying password: {ex.Message}";
                return false;
            }
        }

        private bool UpdateUserPassword(string newUsername, string newPassword)
        {
            try
            {
                using (var connection = _dbHelper.GetConnection())
                {
                    connection.Open();
                    string query = "UPDATE user SET Username = @username, Password = @password WHERE UserID = @userId";

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@username", newUsername);
                        command.Parameters.AddWithValue("@password", newPassword);
                        command.Parameters.AddWithValue("@userId", _currentUserId);

                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                ValidationMessage = $"Error updating user data: {ex.Message}";
                return false;
            }
        }

        private bool CanApplyChanges()
        {
            return !IsLoading &&
                   !string.IsNullOrEmpty(Username) &&
                   !string.IsNullOrEmpty(CurrentPassword) &&
                   !string.IsNullOrEmpty(NewPassword) &&
                   !string.IsNullOrEmpty(ConfirmNewPassword) &&
                   ValidatePassword(NewPassword) &&
                   NewPassword == ConfirmNewPassword;
        }

        private async void ApplyChanges()
        {
            try
            {
                IsLoading = true;
                ValidationMessage = "";

                bool isCurrentPasswordValid = await System.Threading.Tasks.Task.Run(() =>
                    VerifyCurrentPassword(CurrentPassword));

                if (!isCurrentPasswordValid)
                {
                    ValidationMessage = "Current password is incorrect.";
                    return;
                }

                // Update the Database 
                bool isUpdated = await System.Threading.Tasks.Task.Run(() =>
                    UpdateUserPassword(Username, NewPassword));

                if (isUpdated)
                {
                    ValidationMessage = "Profile updated successfully!";

                    // Clear password fields FIRST - this will trigger UI updates through PropertyChanged
                    CurrentPassword = "";
                    NewPassword = "";
                    ConfirmNewPassword = "";

                    // Show success message
                    MessageBox.Show("Profile updated successfully!", "Success",
                                   MessageBoxButton.OK, MessageBoxImage.Information);

                    // Clear validation message after successful operation
                    ValidationMessage = "";
                }
                else
                {
                    ValidationMessage = "Failed to update profile. Please try again.";
                }
            }
            catch (Exception ex)
            {
                ValidationMessage = $"An error occurred: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    public class CustCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public CustCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object parameter) => _execute();

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}