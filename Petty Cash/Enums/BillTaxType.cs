using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Petty_Cash
{
    [TypeConverter(typeof(Classes.EnumDescriptionTypeConverter))]
    public enum BillTaxType
    {
        [Description("Not Applicable")]
        Not_Applicable,
        [Description("VAT")]
        Vat,
        [Description("Withholding Tax")]
        Withholding_Tax
    }
}
