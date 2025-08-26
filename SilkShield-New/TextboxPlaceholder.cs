using System.Windows;
using System.Windows.Controls;

namespace SilkShield_New
{
    public static class TextBoxPlaceholder
    {
        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.RegisterAttached(
                "Placeholder",
                typeof(string),
                typeof(TextBoxPlaceholder),
                new PropertyMetadata(string.Empty, OnPlaceholderChanged));

        public static string GetPlaceholder(TextBox textBox)
        {
            return (string)textBox.GetValue(PlaceholderProperty);
        }

        public static void SetPlaceholder(TextBox textBox, string value)
        {
            textBox.SetValue(PlaceholderProperty, value);
        }

        private static void OnPlaceholderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                // When TextBox is loaded
                textBox.Loaded += (s, ev) => ShowPlaceholder(textBox);

                // When user types
                textBox.TextChanged += (s, ev) => ShowPlaceholder(textBox);
            }
        }

        private static void ShowPlaceholder(TextBox textBox)
        {
            if (string.IsNullOrEmpty(textBox.Text))
            {
                textBox.Foreground = System.Windows.Media.Brushes.Gray;
                textBox.Text = GetPlaceholder(textBox);

                textBox.GotFocus += RemovePlaceholder;
            }
        }

        private static void RemovePlaceholder(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox.Text == GetPlaceholder(textBox))
            {
                textBox.Text = string.Empty;
                textBox.Foreground = System.Windows.Media.Brushes.Black;
            }

            textBox.LostFocus += AddPlaceholderBack;
        }

        private static void AddPlaceholderBack(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;

            if (string.IsNullOrEmpty(textBox.Text))
            {
                textBox.Foreground = System.Windows.Media.Brushes.Gray;
                textBox.Text = GetPlaceholder(textBox);
            }
        }
    }
}
