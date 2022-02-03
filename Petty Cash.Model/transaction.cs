namespace Petty_Cash.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("transaction")]
    public partial class transaction
    {
        public long Id { get; set; }

        public long? VoucherId { get; set; }

        public long? PayeeId { get; set; }

        public long? CategoryId { get; set; }

        public long TransactionDate { get; set; }

        public decimal? AmountIn { get; set; }

        public decimal? AmountOut { get; set; }

        public decimal? AmountReturn { get; set; }

        [StringLength(2147483647)]
        public string Purpose { get; set; }

        [StringLength(2147483647)]
        public string PaymentFile { get; set; }

        public long? CompanyId { get; set; }

        public long IsVat { get; set; }

        public virtual category category { get; set; }

        public virtual company company { get; set; }

        public virtual payee payee { get; set; }

        public virtual voucher voucher { get; set; }
    }
}
