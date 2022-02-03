using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Petty_Cash.Classes
{
    public static class DateRangeHelper
    {
        public static void GetDateRange(DateRangeType type, out DateTime start, out DateTime end)
        {
            //get via reflection
            MethodInfo method = typeof(DateRangeHelper).GetMethod(string.Format("Get{0}Range", type.ToString()), BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null)
                throw new ArgumentException("(DateRangeHelper) DateRangeType type \""+type.ToString()+"\" is not yet supported.");
            object[] args = new object[] { DateTime.MinValue, DateTime.MaxValue };
            method.Invoke(null, args);
            start = (DateTime)args[0];
            end = (DateTime)args[1];
        }

        private static void GetThisMonthRange(out DateTime start, out DateTime end)
        {
            end = DateTime.Now.Date;
            start = new DateTime(end.Year, end.Month, 1);
        }

        private static void GetLastMonthRange(out DateTime start, out DateTime end)
        {
            DateTime lastMonth = DateTime.Now.Date.AddMonths(-1);
            start = new DateTime(lastMonth.Year, lastMonth.Month, 1);
            end = new DateTime(lastMonth.Year, lastMonth.Month, DateTime.DaysInMonth(lastMonth.Year, lastMonth.Month));
        }

        private static void GetTodayRange(out DateTime start, out DateTime end)
        {
            DateTime now = DateTime.Now;
            start = now;
            end = now;
        }
    }
}
