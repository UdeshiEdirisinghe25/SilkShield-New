using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SilkShield_New
{
    public static class TextBoxPlaceholder
    {
        // Define the attached property
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
                // When TextBox loads, apply placeholder if needed
                textBox.Loaded += (s, ev) => UpdatePlaceholder(textBox);

                // Update placeholder whenever text changes
                textBox.TextChanged += (s, ev) => UpdatePlaceholder(textBox);

                // Remove placeholder on focus
                textBox.GotFocus += (s, ev) =>
                {
                    if (textBox.Foreground == Brushes.Gray)
                    {
                        textBox.Text = string.Empty;
                        textBox.Foreground = Brushes.Black;
                    }
                };

                // Restore placeholder when focus is lost
                textBox.LostFocus += (s, ev) => UpdatePlaceholder(textBox);
            }
        }

        private static void UpdatePlaceholder(TextBox textBox)
        {
            if (string.IsNullOrEmpty(textBox.Text))
            {
                textBox.Foreground = Brushes.Gray;
                textBox.Text = GetPlaceholder(textBox);
            }
            else if (textBox.Foreground == Brushes.Gray && textBox.Text == GetPlaceholder(textBox))
            {
                textBox.Foreground = Brushes.Black;
            }
        }
    }
}
