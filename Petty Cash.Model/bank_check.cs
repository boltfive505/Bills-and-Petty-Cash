namespace Petty_Cash.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class bank_check
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public bank_check()
        {
            voucher = new HashSet<voucher>();
        }

        public long Id { get; set; }

        public long AccountId { get; set; }

        [StringLength(2147483647)]
        public string CheckNumber { get; set; }

        public decimal Amount { get; set; }

        [Required]
        [StringLength(2147483647)]
        public string PayeeName { get; set; }

        public long CheckDate { get; set; }

        [StringLength(2147483647)]
        public string Description { get; set; }

        public long UpdatedDate { get; set; }

        public long? PrintedDate { get; set; }

        [StringLength(2147483647)]
        public string PayToBillName { get; set; }

        public long AlreadyUsed { get; set; }

        public virtual bank_account bank_account { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<voucher> voucher { get; set; }
    }
}
