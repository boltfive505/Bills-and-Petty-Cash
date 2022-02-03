namespace Petty_Cash.Bills.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class period_inclusion
    {
        public long Id { get; set; }

        public long PeriodId { get; set; }

        [Required]
        [StringLength(2147483647)]
        public string InclusionType { get; set; }

        public long Value { get; set; }

        public long IsIncluded { get; set; }

        public virtual bill_period bill_period { get; set; }
    }
}
