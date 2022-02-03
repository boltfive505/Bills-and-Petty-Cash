using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bolt5.CustomMonthlyCalendar;

namespace purchase_request.Objects.Calendar
{
    public class PeriodCalendarDisplayCollection<T> : INotifyPropertyChanged, IMonthlyCalendarDayItem where T : IPeriodGetter
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public DateTime Day { get; set; }
        public IEnumerable<PeriodCalendarDisplay<T>> Items { get; set; }
    }
}
