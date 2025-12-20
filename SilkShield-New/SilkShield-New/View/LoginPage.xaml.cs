using System.Windows;
using System.Windows.Controls;
using FontAwesome.WPF;
using System.Data.SQLite;
using SilkShield_New.Data;
namespace SilkShield_New.View
{
    public partial class LoginPage : Window
    {
        private bool _isPasswordVisible = false;
        public LoginPage()
        {
            InitializeComponent();
            // The Login button's click event handler is now linked to the method in this code-behind file.
            LoginButton.Click += LoginButton_Click;
        }
        private void TogglePasswordVisibility(object sender, RoutedEventArgs e)
        {
            _isPasswordVisible = !_isPasswordVisible;
            if (_isPasswordVisible)
            {
                // Copy the password from the PasswordBox to the TextBox
                VisiblePasswordTextBox.Text = PasswordBox.Password;
                // Make the PasswordBox invisible and the TextBox visible
                PasswordBox.Visibility = Visibility.Collapsed;
                VisiblePasswordTextBox.Visibility = Visibility.Visible;
                // Set the focus to the visible TextBox
                VisiblePasswordTextBox.Focus();
                VisiblePasswordTextBox.CaretIndex = VisiblePasswordTextBox.Text.Length; // Place cursor at end
                                                                                        // Change the button icon to an open eye
                PasswordToggleIcon.Icon = FontAwesome.WPF.FontAwesomeIcon.Eye;
            }
            else
            {
                // Copy the password from the TextBox to the PasswordBox
                PasswordBox.Password = VisiblePasswordTextBox.Text;
                // Make the TextBox invisible and the PasswordBox visible
                VisiblePasswordTextBox.Visibility = Visibility.Collapsed;
                PasswordBox.Visibility = Visibility.Visible;
                // Set the focus back to the PasswordBox
                PasswordBox.Focus();
                // Change the button icon back to a slashed eye
                PasswordToggleIcon.Icon = FontAwesome.WPF.FontAwesomeIcon.EyeSlash;
            }
        }
        // Login button's click event handler
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the username and password from the UI elements.
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;
            // Check for empty fields first.
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both username and password.");
                return;
            }
            // Using the DatabaseHelper to check user credentials
            var dbHelper = new DatabaseHelper();
            using (SQLiteConnection connection = dbHelper.GetConnection())
            {
                try
                {
                    connection.Open();
                    // Select the password for the given username.
                    string query = "SELECT Password FROM user WHERE username = @username";
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@username", username);
                        object result = command.ExecuteScalar();
                        if (result != null)
                        {
                            string storedPassword = result.ToString();
                            // Check if the entered password matches the stored password.
                            if (storedPassword == password)
                            {
                                MainWindow main = new MainWindow();
                                main.Show();
                                Application.Current.MainWindow = main; 
                                Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose; 
                                this.Close();
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
        private void ClearFields()
        {
            UsernameTextBox.Text = string.Empty;
            PasswordBox.Password = string.Empty;
            VisiblePasswordTextBox.Text = string.Empty;
        }

        private void UsernameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}