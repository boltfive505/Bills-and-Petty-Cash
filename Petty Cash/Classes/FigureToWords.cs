using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Petty_Cash.Classes
{
    public static class FigureToWords
    {
        private static string[] _oneToNineteen = new string[19]
        {
            "One",
            "Two",
            "Three",
            "Four",
            "Five",
            "Six",
            "Seven",
            "Eight",
            "Nine",
            "Ten",
            "Eleven",
            "Twelve",
            "Thirteen",
            "Fourteen",
            "Fifteen",
            "Sixteen",
            "Seventeen",
            "Eighteen",
            "Nineteen"
        };

        private static string[] _tensPlaces = new string[8]
        {
            "Twenty",
            "Thirty",
            "Forty",
            "Fifty",
            "Sixty",
            "Seventy",
            "Eighty",
            "Ninety"
        };

        public static string NumberToText(int n)
        {
            if (n < 0)
                return "Minus " + NumberToText(-n);
            else if (n == 0)
                return "";
            else if (n <= 19)
                return _oneToNineteen[n - 1] + " ";
            else if (n <= 99)
                return _tensPlaces[n / 10 - 2] + " " + NumberToText(n % 10);
            else if (n <= 199)
                return "One Hundred " + NumberToText(n % 100);
            else if (n <= 999)
                return NumberToText(n / 100) + "Hundred " + NumberToText(n % 100);
            else if (n <= 1999)
                return "One Thousand " + NumberToText(n % 1000);
            else if (n <= 999999)
                return NumberToText(n / 1000) + "Thousand " + NumberToText(n % 1000);
            else if (n <= 1999999)
                return "One Million " + NumberToText(n % 1000000);
            else if (n <= 999999999)
                return NumberToText(n / 1000000) + "Million " + NumberToText(n % 1000000);
            else if (n <= 1999999999)
                return "One Billion " + NumberToText(n % 1000000000);
            else
                return NumberToText(n / 1000000000) + "Billion " + NumberToText(n % 1000000000);
        }

        public static string CurrencyToText(decimal n)
        {
            string numberInText = NumberToText((int)n);
            if (n % 1 > 0)
            {
                //has decimal
                int decimalNum = (int)(Math.Round(n % 1, 2) * 100);
                numberInText += string.Format("and {0}/100", decimalNum);
            }
            else
            {
                numberInText += "Only";
            }
            return numberInText;
        }
    }
}
