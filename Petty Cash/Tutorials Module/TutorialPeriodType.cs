using System;
using System.ComponentModel;

namespace Petty_Cash.Tutorials_Module
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum TutorialPeriodType
    {
        [Description("N/A")]
        None,
        Daily,
        Weekly,
        Monthly
    }
}
