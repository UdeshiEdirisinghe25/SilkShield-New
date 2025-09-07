using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SilkShield_New.View
{
    /// <summary>
    /// Interaction logic for Profile.xaml
    /// </summary>
    public partial class Profile : Window
    {
        public Profile()
        {
            InitializeComponent();

            // Event handlers for Pasword Boxes
            // CurrentPasswordBox.PasswordChanged += CurrentPasswordBox_PasswordChanged;
            NewPasswordBox.PasswordChanged += NewPasswordBox_PasswordChanged;
            RepeatNewPasswordBox.PasswordChanged += RepeatNewPasswordBox_PasswordChanged;

            CurrentPasswordBox.GotFocus += PasswordBox_GotFocus;
            CurrentPasswordBox.LostFocus += CurrentPasswordBox_LostFocus;
            NewPasswordBox.GotFocus += PasswordBox_GotFocus;
            NewPasswordBox.LostFocus += NewPasswordBox_LostFocus;
            RepeatNewPasswordBox.GotFocus += PasswordBox_GotFocus;
            RepeatNewPasswordBox.LostFocus += RepeatNewPasswordBox_LostFocus;
        }

        private bool _isCurrentPasswordVisible = false;
        private bool _isNewPasswordVisible = false;
        private bool _isRepeatNewPasswordVisible = false;

        // Placeholder visibility control කරන්න
        private void CurrentPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholderVisibility(CurrentPasswordBox);
        }

        private void NewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholderVisibility(NewPasswordBox);
        }

        private void RepeatNewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholderVisibility(RepeatNewPasswordBox);
        }

        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                UpdatePlaceholderVisibility(passwordBox);
            }
        }

        private void CurrentPasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholderVisibility(CurrentPasswordBox);
        }

        private void NewPasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholderVisibility(NewPasswordBox);
        }

        private void RepeatNewPasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholderVisibility(RepeatNewPasswordBox);
        }

        private void UpdatePlaceholderVisibility(PasswordBox passwordBox)
        {
            if (passwordBox.Template != null)
            {
                var placeholderText = passwordBox.Template.FindName("PlaceholderText", passwordBox) as TextBlock;
                if (placeholderText != null)
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
    }
}