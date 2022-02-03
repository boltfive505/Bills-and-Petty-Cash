namespace Petty_Cash.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("department")]
    public partial class department
    {
        public long Id { get; set; }

        [Required]
        [StringLength(2147483647)]
        public string DepartmentName { get; set; }

        [StringLength(2147483647)]
        public string JobDescription { get; set; }
    }
}
