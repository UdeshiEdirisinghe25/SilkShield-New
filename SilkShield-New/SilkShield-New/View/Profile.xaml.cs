using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using SilkShield_New.ViewModel;

namespace SilkShield_New.View
{
    /// <summary>
    /// Interaction logic for Profile.xaml
    /// </summary>
    public partial class Profile : UserControl
    {
        private readonly ProfileViewModel _viewModel;
        private bool _isCurrentPasswordVisible = false;
        private bool _isNewPasswordVisible = false;
        private bool _isRepeatNewPasswordVisible = false;

        public Profile()
        {
            InitializeComponent();
            _viewModel = DataContext as ProfileViewModel;

            // Subscribe to ViewModel property changes to clear UI
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            }

            // Password box event handlers for eye button functionality
            CurrentPasswordBox.PasswordChanged += (s, e) =>
            {
                if (_viewModel != null && !_isCurrentPasswordVisible)
                    _viewModel.CurrentPassword = CurrentPasswordBox.Password;
                UpdatePlaceholderVisibility(CurrentPasswordBox);
            };

            NewPasswordBox.PasswordChanged += (s, e) =>
            {
                if (_viewModel != null && !_isNewPasswordVisible)
                    _viewModel.NewPassword = NewPasswordBox.Password;
                UpdatePlaceholderVisibility(NewPasswordBox);
            };

            RepeatNewPasswordBox.PasswordChanged += (s, e) =>
            {
                if (_viewModel != null && !_isRepeatNewPasswordVisible)
                    _viewModel.ConfirmNewPassword = RepeatNewPasswordBox.Password;
                UpdatePlaceholderVisibility(RepeatNewPasswordBox);
            };

            // TextBox event handlers (visible password mode)
            CurrentPasswordTextBox.TextChanged += (s, e) =>
            {
                if (_viewModel != null && _isCurrentPasswordVisible)
                    _viewModel.CurrentPassword = CurrentPasswordTextBox.Text;
            };

            NewPasswordTextBox.TextChanged += (s, e) =>
            {
                if (_viewModel != null && _isNewPasswordVisible)
                    _viewModel.NewPassword = NewPasswordTextBox.Text;
            };

            RepeatNewPasswordTextBox.TextChanged += (s, e) =>
            {
                if (_viewModel != null && _isRepeatNewPasswordVisible)
                    _viewModel.ConfirmNewPassword = RepeatNewPasswordTextBox.Text;
            };

            // Focus events
            CurrentPasswordBox.GotFocus += (s, e) => UpdatePlaceholderVisibility(CurrentPasswordBox);
            CurrentPasswordBox.LostFocus += (s, e) => UpdatePlaceholderVisibility(CurrentPasswordBox);
            NewPasswordBox.GotFocus += (s, e) => UpdatePlaceholderVisibility(NewPasswordBox);
            NewPasswordBox.LostFocus += (s, e) => UpdatePlaceholderVisibility(NewPasswordBox);
            RepeatNewPasswordBox.GotFocus += (s, e) => UpdatePlaceholderVisibility(RepeatNewPasswordBox);
            RepeatNewPasswordBox.LostFocus += (s, e) => UpdatePlaceholderVisibility(RepeatNewPasswordBox);
        }

        #region ViewModel Property Changed Handler

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Clear UI controls when ViewModel properties are cleared
            if (e.PropertyName == nameof(_viewModel.CurrentPassword) && string.IsNullOrEmpty(_viewModel.CurrentPassword))
            {
                // Clear both PasswordBox and TextBox
                CurrentPasswordBox.Password = "";
                CurrentPasswordTextBox.Text = "";
                UpdatePlaceholderVisibility(CurrentPasswordBox);
            }
            else if (e.PropertyName == nameof(_viewModel.NewPassword) && string.IsNullOrEmpty(_viewModel.NewPassword))
            {
                // Clear both PasswordBox and TextBox
                NewPasswordBox.Password = "";
                NewPasswordTextBox.Text = "";
                UpdatePlaceholderVisibility(NewPasswordBox);
            }
            else if (e.PropertyName == nameof(_viewModel.ConfirmNewPassword) && string.IsNullOrEmpty(_viewModel.ConfirmNewPassword))
            {
                // Clear both PasswordBox and TextBox
                RepeatNewPasswordBox.Password = "";
                RepeatNewPasswordTextBox.Text = "";
                UpdatePlaceholderVisibility(RepeatNewPasswordBox);
            }
        }

        #endregion

        #region Password Visibility Toggle Methods

        private void CurrentPasswordToggleButton_Click(object sender, RoutedEventArgs e)
        {
            _isCurrentPasswordVisible = !_isCurrentPasswordVisible;
            if (_isCurrentPasswordVisible)
            {
                CurrentPasswordTextBox.Text = CurrentPasswordBox.Password;
                CurrentPasswordBox.Visibility = Visibility.Collapsed;
                CurrentPasswordTextBox.Visibility = Visibility.Visible;
                CurrentPasswordEyeIcon.Icon = FontAwesome.WPF.FontAwesomeIcon.Eye;
                CurrentPasswordTextBox.Focus();
                CurrentPasswordTextBox.CaretIndex = CurrentPasswordTextBox.Text.Length;
            }
            else
            {
                CurrentPasswordBox.Password = CurrentPasswordTextBox.Text;
                CurrentPasswordTextBox.Visibility = Visibility.Collapsed;
                CurrentPasswordBox.Visibility = Visibility.Visible;
                CurrentPasswordEyeIcon.Icon = FontAwesome.WPF.FontAwesomeIcon.EyeSlash;
                CurrentPasswordBox.Focus();
                UpdatePlaceholderVisibility(CurrentPasswordBox);
            }
        }

        private void NewPasswordToggleButton_Click(object sender, RoutedEventArgs e)
        {
            _isNewPasswordVisible = !_isNewPasswordVisible;
            if (_isNewPasswordVisible)
            {
                NewPasswordTextBox.Text = NewPasswordBox.Password;
                NewPasswordBox.Visibility = Visibility.Collapsed;
                NewPasswordTextBox.Visibility = Visibility.Visible;
                NewPasswordEyeIcon.Icon = FontAwesome.WPF.FontAwesomeIcon.Eye;
                NewPasswordTextBox.Focus();
                NewPasswordTextBox.CaretIndex = NewPasswordTextBox.Text.Length;
            }
            else
            {
                NewPasswordBox.Password = NewPasswordTextBox.Text;
                NewPasswordTextBox.Visibility = Visibility.Collapsed;
                NewPasswordBox.Visibility = Visibility.Visible;
                NewPasswordEyeIcon.Icon = FontAwesome.WPF.FontAwesomeIcon.EyeSlash;
                NewPasswordBox.Focus();
                UpdatePlaceholderVisibility(NewPasswordBox);
            }
        }

        private void RepeatNewPasswordToggleButton_Click(object sender, RoutedEventArgs e)
        {
            _isRepeatNewPasswordVisible = !_isRepeatNewPasswordVisible;
            if (_isRepeatNewPasswordVisible)
            {
                RepeatNewPasswordTextBox.Text = RepeatNewPasswordBox.Password;
                RepeatNewPasswordBox.Visibility = Visibility.Collapsed;
                RepeatNewPasswordTextBox.Visibility = Visibility.Visible;
                RepeatNewPasswordEyeIcon.Icon = FontAwesome.WPF.FontAwesomeIcon.Eye;
                RepeatNewPasswordTextBox.Focus();
                RepeatNewPasswordTextBox.CaretIndex = RepeatNewPasswordTextBox.Text.Length;
            }
            else
            {
                RepeatNewPasswordBox.Password = RepeatNewPasswordTextBox.Text;
                RepeatNewPasswordTextBox.Visibility = Visibility.Collapsed;
                RepeatNewPasswordBox.Visibility = Visibility.Visible;
                RepeatNewPasswordEyeIcon.Icon = FontAwesome.WPF.FontAwesomeIcon.EyeSlash;
                RepeatNewPasswordBox.Focus();
                UpdatePlaceholderVisibility(RepeatNewPasswordBox);
            }
        }

        #endregion

        #region Navigation Methods

        private void LogOutLabel_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Navigate to Login page
            var loginWindow = new LoginPage(); 
            loginWindow.Show();
            this.Close();
        }

        #endregion

        #region Helper Methods

        private void UpdatePlaceholderVisibility(PasswordBox passwordBox)
        {
            if (passwordBox.Template != null)
            {
                if (passwordBox.Template.FindName("PlaceholderText", passwordBox) is TextBlock placeholderText)
                {
                    if (string.IsNullOrEmpty(passwordBox.Password) && !passwordBox.IsFocused)
                    {
                        placeholderText.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        placeholderText.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        #endregion

        // Window closing event - cleanup
        protected override void OnClosed(EventArgs e)
        {
            if (_viewModel != null)
            {
                // Unsubscribe from events
                _viewModel.PropertyChanged -= ViewModel_PropertyChanged;

                // Clear sensitive data
                _viewModel.CurrentPassword = "";
                _viewModel.NewPassword = "";
                _viewModel.ConfirmNewPassword = "";
            }
            base.OnClosed(e);
        }
    }
}