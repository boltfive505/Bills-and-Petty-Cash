using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace Petty_Cash.Bills.Model
{
    [DbConfigurationType(typeof(System.Data.SQLite.EF6.Configuration.SqliteDbConfiguration))]
    public partial class BillsModel : DbContext
    {
        public BillsModel()
            : base("data source=data/bills.db;fail if missing=True;foreign keys=True;")
        {
        }

        public virtual DbSet<bill_payment> bill_payment { get; set; }
        public virtual DbSet<bill_period> bill_period { get; set; }
        public virtual DbSet<category> categories { get; set; }
        public virtual DbSet<period_inclusion> period_inclusion { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<bill_payment>()
                .Property(e => e.Amount)
                .HasPrecision(53, 0);

            modelBuilder.Entity<bill_period>()
                .HasMany(e => e.bill_payment)
                .WithRequired(e => e.bill_period)
                .HasForeignKey(e => e.PeriodId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<bill_period>()
                .HasMany(e => e.period_inclusion)
                .WithRequired(e => e.bill_period)
                .HasForeignKey(e => e.PeriodId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<category>()
                .HasMany(e => e.bill_period)
                .WithRequired(e => e.category)
                .WillCascadeOnDelete(false);
        }

        public void Initialize()
        {
            Database.SetInitializer<BillsModel>(null);
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
            Configuration.AutoDetectChangesEnabled = false;
            Database.Initialize(false);
            this.bill_period.Load();
        }
    }
}
