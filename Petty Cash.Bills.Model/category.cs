namespace Petty_Cash.Bills.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("category")]
    public partial class category
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public category()
        {
            bill_period = new HashSet<bill_period>();
            IsActive = 1;
        }

        public long Id { get; set; }

        [StringLength(2147483647)]
        public string CategoryName { get; set; }

        [StringLength(2147483647)]
        public string Description { get; set; }

        public long IsActive { get; set; } = 1;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<bill_period> bill_period { get; set; }
    }
}
