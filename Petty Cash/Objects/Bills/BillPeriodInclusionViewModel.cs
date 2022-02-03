using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Petty_Cash.Objects.Bills
{
    public class BillPeriodInclusionViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public int Id { get; set; }
        public int Value { get; set; }
        public bool IsIncluded { get; set; } = true;
    }
}
