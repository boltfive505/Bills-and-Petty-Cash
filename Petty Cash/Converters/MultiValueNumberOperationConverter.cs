using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Petty_Cash.Converters
{
    public enum NumberOperation
    {
        Add, Subtract, Multiply, Divide
    }

    public class MultiValueNumberOperationConverter : IMultiValueConverter
    {
        public NumberOperation Operation { get; set; } = NumberOperation.Add;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            decimal result = 0;
            if (values.Length > 0)
            {
                result = System.Convert.ToDecimal(values[0]);
                foreach (var i in values.Skip(1))
                {
                    decimal val = System.Convert.ToDecimal(i);
                    switch (Operation)
                    {
                        case NumberOperation.Add:
                            result += val;
                            break;
                        case NumberOperation.Subtract:
                            result -= val;
                            break;
                        case NumberOperation.Multiply:
                            result *= val;
                            break;
                        case NumberOperation.Divide:
                            result /= val;
                            break;
                    }
                }
            }
            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
