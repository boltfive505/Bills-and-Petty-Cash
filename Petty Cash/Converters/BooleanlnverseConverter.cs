using System;
using System.Globalization;
using System.Windows.Data;

namespace Petty_Cash.Converters
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public class BooleanlnverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = (bool)value;
            return !flag;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
