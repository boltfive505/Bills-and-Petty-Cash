using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Petty_Cash.Objects.CheckWriter;
using Petty_Cash.Reports.Objects;

namespace Petty_Cash.Reports
{
    public static class ReportsHelper
    {
        public static void PrintBankCheck(CheckWriterCheckViewModel check)
        {
            List<BankCheck> checks = new List<BankCheck>();
            checks.Add(new BankCheck() { Payee = check.PayeeName, Amount = check.Amount, Date = check.CheckDate });
            report_form.ShowReport("bank check report", "BankCheckDataset", checks);
        }
    }
}
