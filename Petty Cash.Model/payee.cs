namespace Petty_Cash.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("payee")]
    public partial class payee
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public payee()
        {
            transaction = new HashSet<transaction>();
            voucher = new HashSet<voucher>();
        }

        public long Id { get; set; }

        [Required]
        [StringLength(2147483647)]
        public string PayeeName { get; set; }

        [StringLength(2147483647)]
        public string Description { get; set; }

        public long IsActive { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<transaction> transaction { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<voucher> voucher { get; set; }
    }
}
