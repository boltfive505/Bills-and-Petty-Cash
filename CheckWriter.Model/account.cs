namespace CheckWriter.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("account")]
    public partial class account
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public account()
        {
            checks = new HashSet<check>();
        }

        public long Id { get; set; }

        public long BankId { get; set; }

        [Required]
        [StringLength(2147483647)]
        public string AccountName { get; set; }

        [Required]
        [StringLength(2147483647)]
        public string AccountNumber { get; set; }

        public string AccountType { get; set; }

        [StringLength(2147483647)]
        public string Description { get; set; }

        public string ContactPerson { get; set; }

        public string ContactNumber { get; set; }

        public string Purpose { get; set; }

        public long IsActive { get; set; }

        public virtual bank bank { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<check> checks { get; set; }
    }
}
