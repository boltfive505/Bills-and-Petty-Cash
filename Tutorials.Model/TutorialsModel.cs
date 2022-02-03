using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Data.SQLite;

namespace Tutorials.Model
{
    [DbConfigurationType(typeof(System.Data.SQLite.EF6.Configuration.SqliteDbConfiguration))]
    public partial class TutorialsModel : DbContext
    {
        public TutorialsModel()
            : base("Data Source=data/tutorials.db;version=3;Foreign Keys=True;Fail If Missing=true")
        {
        }

        public virtual DbSet<video> video { get; set; }
        public virtual DbSet<category> category { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<video>()
                .HasRequired(e => e.category)
                .WithMany(e => e.videos)
                .HasForeignKey(e => e.CategoryId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<category>()
                .HasMany(e => e.subCategories)
                .WithOptional(e => e.parentCategory)
                .HasForeignKey(e => e.ParentCategoryId)
                .WillCascadeOnDelete(false);
        }

        public void Initialize()
        {
            Database.SetInitializer<TutorialsModel>(null);
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
            Configuration.AutoDetectChangesEnabled = false;
            Database.Initialize(false);
            this.category.Load();
            Configuration.AutoDetectChangesEnabled = true;
        }
    }
}
