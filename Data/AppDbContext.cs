// File: Zoomag/Data/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using Zoomag.Models;

namespace Zoomag.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Unit> Unit { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<Supply> Supply { get; set; }
        public DbSet<Sale> Sale { get; set; }
        public DbSet<SupplyItem> SupplyItem { get; set; }
        public DbSet<SaleItem> SaleItem { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // В реальных проектах строку подключения получают из IConfiguration (appsettings.json)
                optionsBuilder.UseSqlServer(
                    "Server=(localdb)\\mssqllocaldb;Database=ValeevaZoomagDb;Trusted_Connection=true;TrustServerCertificate=true;",
                    options => options.EnableRetryOnFailure(3));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Связь Sale -> Product (многие-ко-многим с дополнительными полями)
            modelBuilder.Entity<SaleItem>()
                .ToTable("SalesProducts")
                .HasKey(si => new { si.SaleId, si.ProductId });

            modelBuilder.Entity<SaleItem>()
                .HasOne(si => si.Sale)
                .WithMany(s => s.SaleItems)
                .HasForeignKey(si => si.SaleId)
                .HasConstraintName("FK_SaleItem_Sale");

            modelBuilder.Entity<SaleItem>()
                .HasOne(si => si.Product)
                .WithMany(p => p.SaleItems)
                .HasForeignKey(si => si.ProductId)
                .HasConstraintName("FK_SaleItem_Product");

            // Связь Supply -> Product (многие-ко-многим с дополнительными полями)
            modelBuilder.Entity<SupplyItem>()
                .ToTable("SupplyProducts") // Явно указываем имя таблицы для консистентности
                .HasKey(si => new { si.SupplyId, si.ProductId });

            modelBuilder.Entity<SupplyItem>()
                .HasOne(si => si.Supply)
                .WithMany(s => s.SupplyItems)
                .HasForeignKey(si => si.SupplyId)
                .HasConstraintName("FK_SupplyItem_Supply");

            modelBuilder.Entity<SupplyItem>()
                .HasOne(si => si.Product)
                .WithMany(p => p.SupplyItems)
                .HasForeignKey(si => si.ProductId)
                .HasConstraintName("FK_SupplyItem_Product");

            // Индексы
            modelBuilder.Entity<SaleItem>()
                .HasIndex(si => si.SaleId)
                .HasDatabaseName("IX_SaleItem_SaleId");
            modelBuilder.Entity<SaleItem>()
                .HasIndex(si => si.ProductId)
                .HasDatabaseName("IX_SaleItem_ProductId");
            modelBuilder.Entity<SupplyItem>()
                .HasIndex(si => si.SupplyId)
                .HasDatabaseName("IX_SupplyItem_SupplyId");
            modelBuilder.Entity<SupplyItem>()
                .HasIndex(si => si.ProductId)
                .HasDatabaseName("IX_SupplyItem_ProductId");

            // Вычисляемый столбец для TotalAmount в Supply
            modelBuilder.Entity<Supply>()
                .Property(s => s.TotalAmount)
                .HasComputedColumnSql("(SELECT SUM(si.Total) FROM SupplyItem si WHERE si.SupplyId = Id)", false);
        }
    }
}
