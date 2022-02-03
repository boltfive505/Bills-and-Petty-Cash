using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Petty_Cash.Objects.CheckWriter;
using PropertyChanged;

namespace Petty_Cash.Objects.Bills
{
    public class BillPaymentViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public int Id { get; set; }
        public BillPeriodViewModel Period { get; set; }
        public DateTime PeriodDate { get; set; }
        public DateTime? PaymentDate { get; set; }
        public decimal? Amount { get; set; }
        public string BillFileName { get; set; }
        public string ReceiptFileName { get; set; }
        public string Notes { get; set; }
        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }
        public decimal BillAmount { get; set; }
        public string BillReferenceNumber { get; set; }
        public string PaymentType { get; set; }
        public decimal? VatAmount { get; set; }
        public decimal? NetVatAmount { get; set; }

        public bool IsPaid { get { return (Amount ?? 0) > 0; } }
        public CheckWriterCheckViewModel Check { get; set; } //only used for pay bills, set to null after use

        private void OnBillAmountChanged()
        {
            //compute vat
            if (BillAmount != 0 && (Period?.TaxType ?? BillTaxType.Not_Applicable) != BillTaxType.Not_Applicable)
            {
                decimal vat, netvat;
                Classes.BillsPeriodContextHelper.ComputeVat(BillAmount, Period.TaxRate ?? 0, out vat, out netvat);
                VatAmount = vat;
                NetVatAmount = netvat;
            }
            else
            {
                VatAmount = null;
                NetVatAmount = null;
            }
        }

        private void OnCheckChanged()
        {
            if (Check != null)
            {
                this.Amount = Check.Amount;
            }
        }
    }
}
