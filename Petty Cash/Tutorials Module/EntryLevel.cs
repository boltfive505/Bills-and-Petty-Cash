using System;
using System.ComponentModel;

namespace Petty_Cash.Tutorials_Module
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum EntryLevel
    {
        Beginner,
        Intermediate,
        Advanced,
        Expert
    }
}
