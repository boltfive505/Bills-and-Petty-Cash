using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Petty_Cash
{
    public static class StringExt
    {
        public static string Copy(this string str)
        {
            if (str == null) return null;
            return string.Copy(str);
        }
    }
}
