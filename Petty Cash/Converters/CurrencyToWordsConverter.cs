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
    public class CurrencyToWordsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "";
            decimal num = (decimal)value;
            return Classes.FigureToWords.CurrencyToText(num);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
