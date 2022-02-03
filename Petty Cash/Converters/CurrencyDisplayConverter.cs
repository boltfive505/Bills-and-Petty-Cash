using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Petty_Cash.Converters
{
    [ValueConversion(typeof(decimal?), typeof(string))]
    public class CurrencyDisplayConverter : IValueConverter
    {
        public bool ShowCurrency { get; set; } = true;
        public bool AllowZero { get; set; } = false;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            decimal? num = (decimal?)value;
            if (num == null || (!AllowZero && num == 0)) return null;

            bool isNegative = num.Value < 0;
            return string.Format("{0}{1} {2:N2}", isNegative ? "-" : "", ShowCurrency ? "₱" : "", Math.Abs(num.Value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
