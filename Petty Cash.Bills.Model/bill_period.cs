namespace Petty_Cash.Bills.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class bill_period
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public bill_period()
        {
            bill_payment = new HashSet<bill_payment>();
            period_inclusion = new HashSet<period_inclusion>();
        }

        public long Id { get; set; }

        [Required]
        public string BillerName { get; set; }

        public long CategoryId { get; set; }

        [StringLength(2147483647)]
        public string Description { get; set; }

        public string AccountName { get; set; }

        public string AccountNumber { get; set; }

        public string ContactPerson { get; set; }

        public string ContactNumber { get; set; }

        public string Address { get; set; }

        public string UnitNumber { get; set; }

        public string PhoneNumber { get; set; }

        public decimal? ContractAmount { get; set; }

        public long? ContractExpiration { get; set; }

        public string ContractFile { get; set; }

        public string ReferenceLink { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string BillDescription { get; set; }

        public string BillInstructions { get; set; }

        public string VideoInstructionsFile { get; set; }

        public int IsVat { get; set; }

        public decimal? TaxRate { get; set; }

        public string TaxType { get; set; }

        public string TinNumber { get; set; }

        [Required]
        [StringLength(2147483647)]
        public string PeriodType { get; set; }

        public long? DueDateStart { get; set; }

        public long? DueDateEnd { get; set; }

        public long? DueMonth { get; set; }

        public long? DueDays { get; set; }

        public long IsActive { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<bill_payment> bill_payment { get; set; }

        public virtual category category { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<period_inclusion> period_inclusion { get; set; }
    }
}
