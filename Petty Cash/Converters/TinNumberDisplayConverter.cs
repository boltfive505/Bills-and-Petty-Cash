using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace Petty_Cash.Converters
{
    [ValueConversion(typeof(string), typeof(string))]
    public class TinNumberDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string str = System.Convert.ToString(value);
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }
            else
            {
                Match m = Regex.Match(str, @"^(?<n1>\d{3})?(?<n2>\d{3})?(?<n3>\d{3})?(?<n4>\d{3,5})?");
                return string.Format("{0}-{1}-{2}-{3}", m.Groups["n1"].Value, m.Groups["n2"].Value, m.Groups["n3"].Value, m.Groups["n4"].Value);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
