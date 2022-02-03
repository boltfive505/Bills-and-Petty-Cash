using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Petty_Cash.Reports.Objects
{
    [Serializable]
    public class BankCheck
    {
        public string Payee { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }

        public string AmountInWords
        {
            get { return Classes.FigureToWords.CurrencyToText(Amount); }
        }
    }
}
