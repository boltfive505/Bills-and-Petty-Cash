using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace CheckWriter.Model
{
    [DbConfigurationType(typeof(System.Data.SQLite.EF6.Configuration.SqliteDbConfiguration))]
    public partial class CheckWriterModel : DbContext
    {
        public CheckWriterModel()
            : base("Data Source=data/check writer.db;version=3;Foreign Keys=True;Fail If Missing=true")
        {
        }

        public virtual DbSet<account> account { get; set; }
        public virtual DbSet<bank> bank { get; set; }
        public virtual DbSet<check> check { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<account>()
                .HasMany(e => e.checks)
                .WithRequired(e => e.account)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<bank>()
                .HasMany(e => e.accounts)
                .WithRequired(e => e.bank)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<check>()
                .Property(e => e.Amount)
                .HasPrecision(53, 0);
        }

        public void Initialize()
        {
            Database.SetInitializer<CheckWriterModel>(null);
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
            Configuration.AutoDetectChangesEnabled = false;
            Database.Initialize(false);
            this.check.Load();
            Configuration.AutoDetectChangesEnabled = true;
        }
    }
}
