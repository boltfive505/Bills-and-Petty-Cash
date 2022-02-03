namespace Tutorials.Model
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
            subCategories = new HashSet<category>();
            videos = new HashSet<video>();
            IsActive = 1;
        }

        public long Id { get; set; }

        [Required]
        [StringLength(2147483647)]
        public string CategoryName { get; set; }

        public string Description { get; set; }

        public long? ParentCategoryId { get; set; }

        public long IsActive { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<category> subCategories { get; set; }

        public virtual ICollection<video> videos { get; set; }

        public virtual category parentCategory { get; set; }
    }
}
