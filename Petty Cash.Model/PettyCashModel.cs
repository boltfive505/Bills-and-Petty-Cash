using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;

namespace Petty_Cash.Model
{
    [DbConfigurationType(typeof(System.Data.SQLite.EF6.Configuration.SqliteDbConfiguration))]
    public partial class PettyCashModel : DbContext
    {
        public PettyCashModel()
            : base("data source=data/petty cash.db;fail if missing=True;foreign keys=True;")
        {
        }

        public virtual DbSet<category> category { get; set; }
        public virtual DbSet<company> company { get; set; }
        public virtual DbSet<department> department { get; set; }
        public virtual DbSet<payee> payee { get; set; }
        public virtual DbSet<transaction> transaction { get; set; }
        public virtual DbSet<voucher> voucher { get; set; }
        //check writer
        public virtual DbSet<bank> bank { get; set; }
        public virtual DbSet<bank_account> bank_account { get; set; }
        public virtual DbSet<bank_check> bank_check { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Add(new DecimalPropertyConvention(12, 2));

            modelBuilder.Entity<bank>()
                .HasMany(e => e.bank_account)
                .WithRequired(e => e.bank)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<bank_account>()
                .HasMany(e => e.bank_check)
                .WithRequired(e => e.bank_account)
                .HasForeignKey(e => e.AccountId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<bank_check>()
                .Property(e => e.Amount)
                .HasPrecision(53, 0);

            modelBuilder.Entity<bank_check>()
                .HasMany(e => e.voucher)
                .WithOptional(e => e.bank_check)
                .HasForeignKey(e => e.CheckId);
        }

        public void Initialize()
        {
            Database.SetInitializer<PettyCashModel>(null);
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
            Configuration.AutoDetectChangesEnabled = false;
            Database.Initialize(false);
            this.transaction.Load();
        }
    }
}
