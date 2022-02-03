using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using purchase_request.Objects.Calendar;

namespace Petty_Cash.Classes
{
    public static class PeriodCalendarHelper
    {
        public static IEnumerable<PeriodCalendarDisplayCollection<T>> GetPeriodListByDisplayMonth<T>(IEnumerable<T> list, int year, int month) where T : IPeriodGetter
        {
            List<PeriodCalendarDisplay<T>> periodDisplays = new List<PeriodCalendarDisplay<T>>();
            foreach (var l in list)
            {
                periodDisplays.AddRange(l.GetPeriodDates(year, month)
                    .Select(d => new PeriodCalendarDisplay<T>()
                    {
                        PeriodDate = d,
                        Item = l
                    }));
            }
            return periodDisplays.GroupBy(i => i.PeriodDate)
                .Select(g => new PeriodCalendarDisplayCollection<T>()
                {
                    Day = g.Key,
                    Items = g
                });
        }
    }
}
