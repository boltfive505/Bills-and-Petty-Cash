namespace Petty_Cash.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("voucher")]
    public partial class voucher
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public voucher()
        {
            transaction = new HashSet<transaction>();
        }

        public long Id { get; set; }

        public long? PayeeId { get; set; }

        public long VoucherDate { get; set; }

        public decimal? AmountReceived { get; set; }

        public decimal? AmountReplenished { get; set; }

        public long IsLiquidated { get; set; }

        [StringLength(2147483647)]
        public string Particulars { get; set; }

        public long? CheckId { get; set; }

        public virtual bank_check bank_check { get; set; }

        public virtual payee payee { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<transaction> transaction { get; set; }
    }
}
