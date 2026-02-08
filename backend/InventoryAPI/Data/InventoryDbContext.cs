using Microsoft.EntityFrameworkCore;
using InventoryAPI.Models;

namespace InventoryAPI.Data
{
    public class InventoryDbContext : DbContext
    {
        public InventoryDbContext(DbContextOptions<InventoryDbContext> options)
            : base(options)
        {
        }

        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<StockTransaction> StockTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure InventoryItem
            modelBuilder.Entity<InventoryItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.SKU).IsUnique();
                entity.Property(e => e.Price).HasPrecision(18, 2);
                
                entity.HasMany(e => e.StockTransactions)
                    .WithOne(e => e.InventoryItem)
                    .HasForeignKey(e => e.InventoryItemId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure StockTransaction
            modelBuilder.Entity<StockTransaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.TransactionDate);
            });

            // Seed initial data
            modelBuilder.Entity<InventoryItem>().HasData(
                new InventoryItem
                {
                    Id = 1,
                    Name = "Laptop Dell XPS 15",
                    Description = "High-performance laptop for business use",
                    SKU = "LAP-001",
                    Category = "Electronics",
                    Price = 1299.99m,
                    Quantity = 15,
                    MinimumStock = 5,
                    Location = "Warehouse A",
                    CreatedAt = DateTime.UtcNow
                },
                new InventoryItem
                {
                    Id = 2,
                    Name = "Office Chair Ergonomic",
                    Description = "Adjustable ergonomic office chair",
                    SKU = "FUR-001",
                    Category = "Furniture",
                    Price = 249.99m,
                    Quantity = 30,
                    MinimumStock = 10,
                    Location = "Warehouse B",
                    CreatedAt = DateTime.UtcNow
                },
                new InventoryItem
                {
                    Id = 3,
                    Name = "Wireless Mouse Logitech",
                    Description = "Wireless mouse with USB receiver",
                    SKU = "ACC-001",
                    Category = "Accessories",
                    Price = 29.99m,
                    Quantity = 3,
                    MinimumStock = 20,
                    Location = "Warehouse A",
                    CreatedAt = DateTime.UtcNow
                }
            );
        }
    }
}
