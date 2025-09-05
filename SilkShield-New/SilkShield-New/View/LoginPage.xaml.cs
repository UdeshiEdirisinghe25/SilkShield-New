using System.Windows;
using System.Windows.Controls;
using FontAwesome.WPF;
using SilkShield_New.ViewModel;

namespace SilkShield_New.View
{
    public partial class LoginPage : Window
    {
        // This variable keeps track of the password visibility state
        private bool _isPasswordVisible = false;
        private readonly LoginPageViewModel _viewModel;

        public LoginPage()
        {
            InitializeComponent();
            _viewModel = new LoginPageViewModel();
            this.DataContext = _viewModel;

            // Bind the PasswordBox text to the ViewModel's Password property
            PasswordBox.PasswordChanged += (sender, e) =>
            {
                _viewModel.Password = PasswordBox.Password;
            };

            // Bind the VisiblePasswordTextBox text to the ViewModel's Password property
            VisiblePasswordTextBox.TextChanged += (sender, e) =>
            {
                _viewModel.Password = VisiblePasswordTextBox.Text;
            };
        }

        private void TogglePasswordVisibility(object sender, RoutedEventArgs e)
        {
            // Toggle the state of the password visibility
            _isPasswordVisible = !_isPasswordVisible;

            if (_isPasswordVisible)
            {
                // If the password should be visible:
                // 1. Copy the password from the PasswordBox to the TextBox
                VisiblePasswordTextBox.Text = PasswordBox.Password;
                // 2. Make the PasswordBox invisible and the TextBox visible
                PasswordBox.Visibility = Visibility.Collapsed;
                VisiblePasswordTextBox.Visibility = Visibility.Visible;

                // 3. Change the button icon to an open eye
                var icon = PasswordToggleIcon;
                if (icon != null)
                {
                    icon.Icon = FontAwesomeIcon.Eye;
                }
            }
            else
            {
                // If the password should be hidden:
                // 1. Copy the password from the TextBox to the PasswordBox
                PasswordBox.Password = VisiblePasswordTextBox.Text;
                // 2. Make the TextBox invisible and the PasswordBox visible
                VisiblePasswordTextBox.Visibility = Visibility.Collapsed;
                PasswordBox.Visibility = Visibility.Visible;

                // 3. Change the button icon back to a slashed eye
                var icon = PasswordToggleIcon;
                if (icon != null)
                {
                    icon.Icon = FontAwesomeIcon.EyeSlash;
                }
            }
        }
    }
}
