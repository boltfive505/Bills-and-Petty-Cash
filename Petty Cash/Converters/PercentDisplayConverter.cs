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
    public class PercentDisplayConverter : IValueConverter
    {
        public bool AllowZero { get; set; } = false;
        public bool IsDecimalRate { get; set; } = false;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            decimal? num = (decimal?)value;
            if (num == null || (!AllowZero && num == 0)) return Binding.DoNothing;

            return string.Format("{0}%", num.Value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
