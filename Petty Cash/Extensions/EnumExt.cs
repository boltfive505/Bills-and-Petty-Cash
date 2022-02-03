using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Petty_Cash
{
    public static class EnumExt
    {
        public static TEnum ToEnum<TEnum>(this string value) where TEnum : struct
        {
            TEnum e = default(TEnum);
            Enum.TryParse(value, true, out e);
            return e;
        }

        public static TEnum ToEnum<TEnum>(this long value) where TEnum : struct
        {
            return (TEnum)Enum.ToObject(typeof(TEnum), value);
        }
    }
}
