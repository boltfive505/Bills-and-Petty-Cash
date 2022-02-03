using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPOI.SS.UserModel;

namespace Petty_Cash
{
    public static class NPOIExt
    {
        public static void SetCellObjectValue(this ICell cell, object value)
        {
            if (value == null)
            {
                cell.SetCellType(CellType.Blank);
                return;
            }
            Type type = value.GetType();
            if (Nullable.GetUnderlyingType(type) != null) 
                type = Nullable.GetUnderlyingType(type);
            if (type == typeof(int) || type == typeof(long) || type == typeof(double) || type == typeof(decimal))
            {
                cell.SetCellValue(Convert.ToDouble(value));
            }
            else if (type == typeof(DateTime))
            {
                cell.SetCellValue(Convert.ToDateTime(value));
            }
            else
            {
                cell.SetCellValue(Convert.ToString(value));
            }
        }
    }
}
