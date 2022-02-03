namespace Petty_Cash.Bills.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class bill_payment
    {
        public long Id { get; set; }

        public long PeriodId { get; set; }

        public long PeriodDate { get; set; }

        public long? PaymentDate { get; set; }

        public decimal? Amount { get; set; }

        [StringLength(2147483647)]
        public string BillFile { get; set; }

        [StringLength(2147483647)]
        public string ReceiptFile { get; set; }

        [StringLength(2147483647)]
        public string Notes { get; set; }

        public long PeriodFrom { get; set; }

        public long PeriodTo { get; set; }

        public decimal BillAmount { get; set; }

        public string BillReferenceNumber { get; set; }

        public string PaymentType { get; set; }

        public decimal? VatAmount { get; set; }

        public decimal? NetVatAmount { get; set; }

        public virtual bill_period bill_period { get; set; }
    }
}
