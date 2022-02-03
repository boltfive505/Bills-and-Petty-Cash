using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Petty_Cash.Converters
{
    public class VisibilityFromBoolConverter : IValueConverter
    {
        public Visibility HiddenValue { get; set; }

        public VisibilityFromBoolConverter()
        {
            HiddenValue = Visibility.Hidden;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = (bool)value;
            return flag ? Visibility.Visible : HiddenValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
