namespace CheckWriter.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("check")]
    public partial class check
    {
        public long Id { get; set; }

        public long AccountId { get; set; }

        [Required]
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

        public virtual account account { get; set; }
    }
}
