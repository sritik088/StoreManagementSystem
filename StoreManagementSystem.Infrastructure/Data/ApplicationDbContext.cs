using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Infrastructure.Identity;

namespace StoreManagementSystem.Infrastructure.Data
{
    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories => Set<Category>();

        public DbSet<SubCategory> SubCategories => Set<SubCategory>();

        public DbSet<Brand> Brands => Set<Brand>();

        public DbSet<Product> Products => Set<Product>();

        public DbSet<Supplier> Suppliers => Set<Supplier>();

        public DbSet<Unit> Units => Set<Unit>();

        public DbSet<Tax> Taxes => Set<Tax>();

        public DbSet<Warehouse> Warehouses => Set<Warehouse>();

        public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();

        public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();

        public DbSet<GoodsReceipt> GoodsReceipts => Set<GoodsReceipt>();

        public DbSet<GoodsReceiptItem> GoodsReceiptItems => Set<GoodsReceiptItem>();

        public DbSet<WarehouseStock> WarehouseStocks => Set<WarehouseStock>();

        public DbSet<StockLedger> StockLedgers => Set<StockLedger>();

        public DbSet<StockTransfer> StockTransfers => Set<StockTransfer>();

        public DbSet<StockTransferItem> StockTransferItems => Set<StockTransferItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureCategory(modelBuilder);
            ConfigureSubCategory(modelBuilder);
            ConfigureBrand(modelBuilder);
            ConfigureSupplier(modelBuilder);
            ConfigureUnit(modelBuilder);
            ConfigureTax(modelBuilder);
            ConfigureProduct(modelBuilder);
            ConfigureWarehouse(modelBuilder);
            ConfigurePurchaseOrder(modelBuilder);
            ConfigurePurchaseOrderItem(modelBuilder);
            ConfigureGoodsReceipt(modelBuilder);
            ConfigureGoodsReceiptItem(modelBuilder);
            ConfigureWarehouseStock(modelBuilder);
            ConfigureStockLedger(modelBuilder);
            ConfigureStockTransfer(modelBuilder);

        }

        private static void ConfigureCategory(ModelBuilder builder)
        {
            builder.Entity<Category>(entity =>
            {
                entity.Property(x => x.Name)
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(x => x.Description)
                      .HasMaxLength(500);
            });
        }

        private static void ConfigureSubCategory(ModelBuilder builder)
        {
            builder.Entity<SubCategory>(entity =>
            {
                entity.Property(x => x.Name)
                      .HasMaxLength(100)
                      .IsRequired();

                entity.HasOne(x => x.Category)
                      .WithMany(x => x.SubCategories)
                      .HasForeignKey(x => x.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void ConfigureBrand(ModelBuilder builder)
        {
            builder.Entity<Brand>(entity =>
            {
                entity.Property(x => x.Name)
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(x => x.Code)
                      .HasMaxLength(20);

                entity.Property(x => x.Country)
                      .HasMaxLength(100);

                entity.Property(x => x.Website)
                      .HasMaxLength(200);

                entity.Property(x => x.Description)
                      .HasMaxLength(500);
            });
        }

        private static void ConfigureProduct(ModelBuilder builder)
        {
            builder.Entity<Product>(entity =>
            {
                entity.HasKey(x => x.Id);

                // Product Information
                entity.Property(x => x.Name)
                      .HasMaxLength(150)
                      .IsRequired();

                entity.Property(x => x.SKU)
                      .HasMaxLength(50)
                      .IsRequired();

                entity.HasIndex(x => x.SKU)
                      .IsUnique();

                entity.Property(x => x.Barcode)
                      .HasMaxLength(50);

                entity.HasIndex(x => x.Barcode)
                      .IsUnique();

                entity.Property(x => x.HSNCode)
                      .HasMaxLength(20);

                entity.Property(x => x.ImageUrl)
                      .HasMaxLength(300);

                entity.Property(x => x.Description)
                      .HasMaxLength(500);

                // Pricing
                entity.Property(x => x.PurchasePrice)
                      .HasPrecision(18, 2);

                entity.Property(x => x.SellingPrice)
                      .HasPrecision(18, 2);

                entity.Property(x => x.DiscountPrice)
                      .HasPrecision(18, 2);

                // Stock
                entity.Property(x => x.OpeningStock)
                      .HasPrecision(18, 2);

                entity.Property(x => x.CurrentStock)
                      .HasPrecision(18, 2);

                entity.Property(x => x.ReorderLevel)
                      .HasPrecision(18, 2);

                entity.Property(x => x.MaximumStock)
                      .HasPrecision(18, 2);

                // Relationships

                entity.HasOne(x => x.Category)
                      .WithMany()
                      .HasForeignKey(x => x.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.SubCategory)
                      .WithMany()
                      .HasForeignKey(x => x.SubCategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Brand)
                      .WithMany(x => x.Products)
                      .HasForeignKey(x => x.BrandId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Supplier)
                      .WithMany(x => x.Products)
                      .HasForeignKey(x => x.SupplierId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Unit)
                      .WithMany(x => x.Products)
                      .HasForeignKey(x => x.UnitId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Tax)
                      .WithMany(x => x.Products)
                      .HasForeignKey(x => x.TaxId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Warehouse)
                      .WithMany(x => x.Products)
                      .HasForeignKey(x => x.WarehouseId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
        private static void ConfigureSupplier(ModelBuilder builder)
        {
            builder.Entity<Supplier>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name)
                      .HasMaxLength(150)
                      .IsRequired();

                entity.Property(x => x.SupplierCode)
                      .HasMaxLength(20);

                entity.Property(x => x.ContactPerson)
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(x => x.Phone)
                      .HasMaxLength(15)
                      .IsRequired();

                entity.Property(x => x.Email)
                      .HasMaxLength(100);

                entity.Property(x => x.Address)
                      .HasMaxLength(300);

                entity.Property(x => x.City)
                      .HasMaxLength(100);

                entity.Property(x => x.State)
                      .HasMaxLength(100);

                entity.Property(x => x.PostalCode)
                      .HasMaxLength(20);

                entity.Property(x => x.Country)
                      .HasMaxLength(100);

                entity.Property(x => x.GSTNumber)
                      .HasMaxLength(20);

                entity.Property(x => x.Website)
                      .HasMaxLength(100);

                entity.Property(x => x.Notes)
                      .HasMaxLength(500);

                entity.Property(x => x.IsActive)
                      .HasDefaultValue(true);

                entity.Property(x => x.IsDeleted)
                      .HasDefaultValue(false);

                entity.Property(x => x.CreatedDate)
                      .HasDefaultValueSql("GETDATE()");

                entity.HasIndex(x => x.Name)
                      .IsUnique();

                entity.HasIndex(x => x.SupplierCode)
                      .IsUnique();
            });
        }
        private static void ConfigureUnit(ModelBuilder builder)
        {
            builder.Entity<Unit>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name)
                  .HasMaxLength(100)
                  .IsRequired();

                entity.Property(x => x.ShortName)
                  .HasMaxLength(20);

                entity.Property(x => x.Description)
                  .HasMaxLength(200);
            });
        }
        private static void ConfigureTax(ModelBuilder builder)
        {
            builder.Entity<Tax>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name)
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(x => x.TaxPercentage)
                      .HasPrecision(5, 2);

                entity.Property(x => x.TaxType)
                      .HasMaxLength(100);

                entity.Property(x => x.Description)
                      .HasMaxLength(300);

                entity.Property(x => x.IsActive)
                      .HasDefaultValue(true);

                entity.Property(x => x.IsDeleted)
                      .HasDefaultValue(false);

                entity.Property(x => x.CreatedDate)
                      .HasDefaultValueSql("GETDATE()");

            });
        }
        private static void ConfigureWarehouse(ModelBuilder builder)
        {
            builder.Entity<Warehouse>(entity =>
              {
                  entity.HasKey(x => x.Id);

                  entity.Property(x => x.Name)
                     .HasMaxLength(100)
                     .IsRequired();

                  entity.Property(x => x.Code)
                      .HasMaxLength(20);

                  entity.Property(x => x.ManagerName)
                      .HasMaxLength(100);

                  entity.Property(x => x.ContactNumber)
                      .HasMaxLength(20);

                  entity.Property(x => x.Email)
                      .HasMaxLength(150);

                  entity.Property(x => x.Address)
                      .HasMaxLength(300);

                  entity.Property(x => x.City)
                      .HasMaxLength(100);

                  entity.Property(x => x.State)
                      .HasMaxLength(100);

                  entity.Property(x => x.Country)
                      .HasMaxLength(100);

                  entity.Property(x => x.PostalCode)
                      .HasMaxLength(10);

                  entity.Property(x => x.Description)
                      .HasMaxLength(300);
              });
        }
        private static void ConfigurePurchaseOrder(ModelBuilder builder)
        {
            builder.Entity<PurchaseOrder>(entity =>
{
    entity.HasKey(x => x.Id);

    entity.Property(x => x.PONumber)
           .HasMaxLength(30)
           .IsRequired();

    entity.HasIndex(x => x.PONumber)
           .IsUnique();

    entity.Property(x => x.SubTotal).HasPrecision(18, 2);

    entity.Property(x => x.Discount).HasPrecision(18, 2);

    entity.Property(x => x.TaxAmount).HasPrecision(18, 2);

    entity.Property(x => x.GrandTotal).HasPrecision(18, 2);

    entity.HasMany(x => x.Items)
     .WithOne(x => x.PurchaseOrder)
     .HasForeignKey(x => x.PurchaseOrderId)
     .OnDelete(DeleteBehavior.Cascade);
});

        }
        private static void ConfigurePurchaseOrderItem(ModelBuilder builder)
        {
            builder.Entity<PurchaseOrderItem>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Quantity).HasPrecision(18, 2);

                entity.Property(x => x.UnitPrice).HasPrecision(18, 2);

                entity.Property(x => x.Discount).HasPrecision(18, 2);

                entity.Property(x => x.TaxAmount).HasPrecision(18, 2);

                entity.Property(x => x.Total).HasPrecision(18, 2);

                entity.HasOne(x => x.Product)
                      .WithMany()
                      .HasForeignKey(x => x.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
        private static void ConfigureGoodsReceipt(ModelBuilder builder)
        {
            builder.Entity<GoodsReceipt>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.GRNNumber)
                      .HasMaxLength(30)
                      .IsRequired();

                entity.HasIndex(x => x.GRNNumber)
                      .IsUnique();

                entity.Property(x => x.SubTotal).HasPrecision(18, 2);
                entity.Property(x => x.Discount).HasPrecision(18, 2);
                entity.Property(x => x.TaxAmount).HasPrecision(18, 2);
                entity.Property(x => x.GrandTotal).HasPrecision(18, 2);

                entity.HasOne(x => x.PurchaseOrder)
                      .WithMany()
                      .HasForeignKey(x => x.PurchaseOrderId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(x => x.Items)
                      .WithOne(x => x.GoodsReceipt)
                      .HasForeignKey(x => x.GoodsReceiptId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
        private static void ConfigureGoodsReceiptItem(ModelBuilder builder)
        {
            builder.Entity<GoodsReceiptItem>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.OrderedQuantity).HasPrecision(18, 2);

                entity.Property(x => x.ReceivedQuantity).HasPrecision(18, 2);

                entity.Property(x => x.UnitPrice).HasPrecision(18, 2);

                entity.Property(x => x.Discount).HasPrecision(18, 2);

                entity.Property(x => x.TaxAmount).HasPrecision(18, 2);

                entity.Property(x => x.Total).HasPrecision(18, 2);

                entity.HasOne(x => x.Product)
                      .WithMany()
                      .HasForeignKey(x => x.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Warehouse)
                      .WithMany()
                      .HasForeignKey(x => x.WarehouseId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void ConfigureWarehouseStock(ModelBuilder builder)
        {
            builder.Entity<WarehouseStock>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasIndex(x => new
                {
                    x.ProductId,
                    x.WarehouseId
                }).IsUnique();

                entity.Property(x => x.QuantityOnHand)
                      .HasPrecision(18, 2);

                entity.Property(x => x.ReservedQuantity)
                      .HasPrecision(18, 2);

                entity.Property(x => x.MinimumStock)
                      .HasPrecision(18, 2);

                entity.Property(x => x.MaximumStock)
                      .HasPrecision(18, 2);

                entity.HasOne(x => x.Product)
                      .WithMany()
                      .HasForeignKey(x => x.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Warehouse)
                      .WithMany()
                      .HasForeignKey(x => x.WarehouseId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void ConfigureStockLedger(ModelBuilder builder)
        {
            builder.Entity<StockLedger>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Quantity)
                      .HasPrecision(18, 2);

                entity.Property(x => x.BalanceAfterTransaction)
                      .HasPrecision(18, 2);

                entity.Property(x => x.UnitCost)
                      .HasPrecision(18, 2);

                entity.Property(x => x.ReferenceNo)
                      .HasMaxLength(50);

                entity.Property(x => x.CreatedBy)
                      .HasMaxLength(100);
            });
        }
        private static void ConfigureStockTransfer(ModelBuilder builder)
        {
            builder.Entity<StockTransfer>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.TransferNumber)
                      .HasMaxLength(30)
                      .IsRequired();

                entity.HasIndex(x => x.TransferNumber)
                      .IsUnique();

                entity.HasMany(x => x.Items)
                      .WithOne(x => x.StockTransfer)
                      .HasForeignKey(x => x.StockTransferId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.FromWarehouse)
                      .WithMany()
                      .HasForeignKey(x => x.FromWarehouseId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.ToWarehouse)
                      .WithMany()
                      .HasForeignKey(x => x.ToWarehouseId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<StockTransferItem>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Quantity)
                      .HasPrecision(18, 2);

                entity.Property(x => x.UnitCost)
                      .HasPrecision(18, 2);
            });
        }
    }
}