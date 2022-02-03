namespace Petty_Cash.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class bank_account
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public bank_account()
        {
            bank_check = new HashSet<bank_check>();
        }

        public long Id { get; set; }

        public long BankId { get; set; }

        [StringLength(2147483647)]
        public string AccountName { get; set; }

        [StringLength(2147483647)]
        public string AccountNumber { get; set; }

        [StringLength(2147483647)]
        public string AccountType { get; set; }

        [StringLength(2147483647)]
        public string Description { get; set; }

        [StringLength(2147483647)]
        public string ContactPerson { get; set; }

        [StringLength(2147483647)]
        public string ContactNumber { get; set; }

        [StringLength(2147483647)]
        public string Purpose { get; set; }

        public long IsActive { get; set; }

        public virtual bank bank { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<bank_check> bank_check { get; set; }
    }
}
