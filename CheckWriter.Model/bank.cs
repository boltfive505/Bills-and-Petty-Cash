namespace CheckWriter.Model
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
            accounts = new HashSet<account>();
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
        public virtual ICollection<account> accounts { get; set; }
    }
}
