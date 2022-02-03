using System;

namespace Petty_Cash
{
    public static class DateTimeExt
    {
        private static DateTime _unixDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local);

        public static DateTime MinUnixDate
        {
            get { return _unixDate; }
        }

        public static DateTime ToUnixDate(this int value)
        {
            return new DateTime(_unixDate.Year, _unixDate.Month, _unixDate.Day).AddSeconds(value);
        }

        public static DateTime ToUnixDate(this long value)
        {
            return new DateTime(_unixDate.Year, _unixDate.Month, _unixDate.Day).AddSeconds(value);
        }

        public static int ToUnixLong(this DateTime value)
        {
            if (_unixDate > value)
                throw new ArgumentException("DateTime value is older that Unix Time (January 1, 1970)");
            return (int)(value - _unixDate).TotalSeconds;
        }

        public static DateTime? ToUnixDate(this int? value)
        {
            if (value == null) return null;
            return (DateTime?)ToUnixDate((int)value);
        }

        public static DateTime? ToUnixDate(this long? value)
        {
            if (value == null) return null;
            return (DateTime?)ToUnixDate((int)value);
        }

        public static int? ToUnixLong(this DateTime? value)
        {
            if (value == null) return null;
            return (int?)ToUnixLong((DateTime)value);
        }

        public static DateTime MinDateTime(DateTime value1, DateTime value2)
        {
            if (value1 > value2)
                return value1;
            else if (value1 < value2)
                return value2;
            else //equals
                return value1;
        }

        public static DateTime MaxDateTime(DateTime value1, DateTime value2)
        {
            if (value1 > value2)
                return value2;
            else if (value1 < value2)
                return value1;
            else //equals
                return value1;
        }

        public static DateTime RemoveSeconds(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, 0);
        }

        public static DateTime? RemoveSeconds(this DateTime? value)
        {
            if (value == null) return null;
            return RemoveSeconds(value.Value);
        }
    }
}
