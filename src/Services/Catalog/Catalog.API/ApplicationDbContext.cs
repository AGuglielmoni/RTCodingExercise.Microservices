namespace Catalog.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Plate> Plates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Plate>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Registration)
                      .IsRequired()
                      .HasMaxLength(50);
                entity.Property(p => p.PurchasePrice)
                      .HasColumnType("decimal(18,2)");
                entity.Property(p => p.SalePrice)
                      .HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<Plate>().HasData(
                new Plate
                {
                    Id = Guid.NewGuid(),
                    Registration = "ABC123",
                    PurchasePrice = 1000m,
                    SalePrice = 1200m,
                    IsForSale = true
                },
                new Plate
                {
                    Id = Guid.NewGuid(),
                    Registration = "XYZ789",
                    PurchasePrice = 1500m,
                    SalePrice = 1800m,
                    IsForSale = false
                }
            );
        }
    }
}
