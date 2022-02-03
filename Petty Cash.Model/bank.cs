namespace Petty_Cash.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("bank")]
    public partial class bank
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public bank()
        {
            bank_account = new HashSet<bank_account>();
        }

        public long Id { get; set; }

        [Required]
        [StringLength(2147483647)]
        public string BankName { get; set; }

        [StringLength(2147483647)]
        public string Branch { get; set; }

        [StringLength(2147483647)]
        public string Description { get; set; }

        public long IsActive { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<bank_account> bank_account { get; set; }
    }
}
