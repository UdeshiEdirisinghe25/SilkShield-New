using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SilkShield_New.View
{
    public class StatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                if (status.Equals("Paid", StringComparison.OrdinalIgnoreCase))
                {
                    return new SolidColorBrush(Color.FromArgb(255, 220, 255, 220));
                }
                else if (status.Equals("Unpaid", StringComparison.OrdinalIgnoreCase))
                {
                    return new SolidColorBrush(Color.FromArgb(255, 255, 220, 220));
                }
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}