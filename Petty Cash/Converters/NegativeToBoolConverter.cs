using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Petty_Cash.Converters
{
    public class NegativeToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = true;
            if (value == null)
            {
                flag = false;
            }
            else
            {
                var type = value.GetType();
                if (type == typeof(int))
                    flag = (int)value >= 0;
                else if (type == typeof(long))
                    flag = (long)value >= 0;
                else if (type == typeof(decimal))
                    flag = (decimal)value >= 0;
                else if (type == typeof(double))
                    flag = (double)value >= 0;
            }
            return flag;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
