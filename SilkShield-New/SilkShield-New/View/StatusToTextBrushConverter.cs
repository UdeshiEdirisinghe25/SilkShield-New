using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SilkShield_New.View
{
    public class StatusToTextBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                if (status.Equals("Paid", StringComparison.OrdinalIgnoreCase))
                {
                    return new SolidColorBrush(Color.FromArgb(255, 0, 100, 0));
                }
                else if (status.Equals("Unpaid", StringComparison.OrdinalIgnoreCase))
                {
                    return new SolidColorBrush(Color.FromArgb(255, 139, 0, 0));
                }
            }
            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}