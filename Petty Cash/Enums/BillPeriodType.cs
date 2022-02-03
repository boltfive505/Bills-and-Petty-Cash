using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Petty_Cash
{
    [TypeConverter(typeof(Classes.EnumDescriptionTypeConverter))]
    public enum BillPeriodType
    {
        [Description("Monthly")]
        Monthly,
        [Description("End of Quarter")]
        EndOfQuarter,
        [Description("Yearly")]
        Annually,
        [Description("One-Time")]
        OneTime
    }
}
