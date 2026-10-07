using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Infrastructure.Identity;
using System.Reflection.Emit;

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

        // =========================================================
        // MASTER DATA
        // =========================================================

        public DbSet<MainCategory> MainCategories
            => Set<MainCategory>();

        public DbSet<Category> Categories
            => Set<Category>();

        public DbSet<SubCategory> SubCategories
            => Set<SubCategory>();

       

        public DbSet<Product> Products
            => Set<Product>();

        public DbSet<Supplier> Suppliers
            => Set<Supplier>();

        public DbSet<Unit> Units
            => Set<Unit>();

        public DbSet<Damage> Damages 
            => Set<Damage>();

        public DbSet<DamageEntry> DamageEntries
            => Set<DamageEntry>();


        public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();


        public DbSet<Warehouse> Warehouses
            => Set<Warehouse>();

        // =========================================================
        // PURCHASE
        // =========================================================

        public DbSet<PurchaseOrder> PurchaseOrders
            => Set<PurchaseOrder>();

        public DbSet<PurchaseOrderItem> PurchaseOrderItems
            => Set<PurchaseOrderItem>();

        public DbSet<GoodsReceipt> GoodsReceipts
            => Set<GoodsReceipt>();

        public DbSet<GoodsReceiptItem> GoodsReceiptItems
            => Set<GoodsReceiptItem>();

        // =========================================================
        // INVENTORY
        // =========================================================

        public DbSet<WarehouseStock> WarehouseStocks
            => Set<WarehouseStock>();

        public DbSet<StockLedger> StockLedgers
            => Set<StockLedger>();

        public DbSet<StockTransfer> StockTransfers
            => Set<StockTransfer>();

        public DbSet<StockTransferItem> StockTransferItems
            => Set<StockTransferItem>();

        // =========================================================
        // SALES
        // =========================================================

        public DbSet<Customer> Customers
            => Set<Customer>();

        public DbSet<SalesOrder> SalesOrders
            => Set<SalesOrder>();

        public DbSet<SalesOrderItem> SalesOrderItems
            => Set<SalesOrderItem>();

        // =========================================================
        // DELIVERY
        // =========================================================

        public DbSet<Delivery> Deliveries
            => Set<Delivery>();

        public DbSet<DeliveryItem> DeliveryItems
            => Set<DeliveryItem>();

        // =========================================================
        // ACCOUNTS PAYABLE
        // =========================================================

        public DbSet<SupplierPayment> SupplierPayments
        => Set<SupplierPayment>();


        public DbSet<SalesInvoice> SalesInvoices
               => Set<SalesInvoice>();

        public DbSet<SalesInvoiceItem> SalesInvoiceItems
            => Set<SalesInvoiceItem>();

        public DbSet<SidebarMenuSection> SidebarMenuSections
    => Set<SidebarMenuSection>();

        public DbSet<SidebarMenuItem> SidebarMenuItems
            => Set<SidebarMenuItem>();

        // =========================================================
        // MODEL CREATION
        // =========================================================

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureMainCategory(modelBuilder);
            ConfigureCategory(modelBuilder);
            ConfigureSubCategory(modelBuilder);
            
            ConfigureSupplier(modelBuilder);
            ConfigureUnit(modelBuilder);
            
            ConfigureProduct(modelBuilder);
            ConfigureWarehouse(modelBuilder);

            ConfigurePurchaseOrder(modelBuilder);
            ConfigurePurchaseOrderItem(modelBuilder);

            ConfigureGoodsReceipt(modelBuilder);
            ConfigureGoodsReceiptItem(modelBuilder);

            ConfigureWarehouseStock(modelBuilder);
            ConfigureStockLedger(modelBuilder);
            ConfigureStockTransfer(modelBuilder);

            ConfigureCustomer(modelBuilder);
            ConfigureSalesOrder(modelBuilder);
            ConfigureDelivery(modelBuilder);

            ConfigureApplicationUser(modelBuilder);
            ConfigureDamage(modelBuilder);
            ConfigureDamageEntry(modelBuilder);
            ConfigureSystemSetting(modelBuilder);
            ConfigureSupplierPayment(modelBuilder);
            ConfigureSalesInvoice(modelBuilder);
            ConfigureSidebarMenuSection(modelBuilder);
            ConfigureSidebarMenuItem(modelBuilder);

        }

        // =========================================================
        // APPLICATION USER
        // =========================================================

        private static void ConfigureApplicationUser(
            ModelBuilder builder)
        {
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(x => x.FirstName)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.LastName)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.Address)
                    .HasMaxLength(300);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.CreatedDate)
                    .HasDefaultValueSql("GETDATE()");

                entity.HasOne(x => x.Warehouse)
                    .WithMany()
                    .HasForeignKey(x => x.WarehouseId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.WarehouseId);
            });
        }



        // =========================================================
        // MAIN CATEGORY
        // =========================================================

        private static void ConfigureMainCategory(
            ModelBuilder builder)
        {
            builder.Entity<MainCategory>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.HasMany(x => x.Categories)
                      .WithOne(x => x.MainCategory)
                      .HasForeignKey(x => x.MainCategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }

        // =========================================================
        // SIDEBAR MENU SECTION
        // =========================================================

        private static void ConfigureSidebarMenuSection(
            ModelBuilder builder)
        {
            builder.Entity<SidebarMenuSection>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.DisplayOrder)
                    .IsRequired();

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.HasMany(x => x.Items)
                    .WithOne(x => x.Section)
                    .HasForeignKey(x => x.SectionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            
        }


        // =========================================================
        // SIDEBAR MENU ITEM
        // =========================================================

        private static void ConfigureSidebarMenuItem(
            ModelBuilder builder)
        {
            builder.Entity<SidebarMenuItem>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(x => x.Controller)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.Action)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.Icon)
                    .HasMaxLength(100);

                entity.Property(x => x.Permission)
                    .HasMaxLength(150);

                entity.Property(x => x.DisplayOrder)
                    .IsRequired();

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                
            });
        }
        

        // =========================================================
        // CATEGORY
        // =========================================================

        private static void ConfigureCategory(ModelBuilder builder)
        {
            builder.Entity<Category>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.HasOne(x => x.MainCategory)
                      .WithMany(x => x.Categories)
                      .HasForeignKey(x => x.MainCategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }

        // =========================================================
        // SUB CATEGORY
        // =========================================================

        private static void ConfigureSubCategory(ModelBuilder builder)
        {
            builder.Entity<SubCategory>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name)
                      .HasMaxLength(100)
                      .IsRequired();

                entity.HasOne(x => x.Category)
                      .WithMany(x => x.SubCategories)
                      .HasForeignKey(x => x.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }


        // =========================================================
        // SUPPLIER
        // =========================================================

        private static void ConfigureSupplier(
            ModelBuilder builder)
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

        // =========================================================
        // UNIT
        // =========================================================

        private static void ConfigureUnit(
            ModelBuilder builder)
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


        // =========================================================
        // PRODUCT
        // =========================================================

        private static void ConfigureProduct(
            ModelBuilder builder)
        {
            builder.Entity<Product>(entity =>
            {
                entity.HasKey(x => x.Id);

                // -------------------------------------------------
                // SKU
                // -------------------------------------------------

                entity.HasIndex(x => x.SKU)
                      .IsUnique();

                // -------------------------------------------------
                // Barcode
                // -------------------------------------------------

                entity.HasIndex(x => x.Barcode)
                      .IsUnique()
                      .HasFilter("[Barcode] IS NOT NULL");

                // -------------------------------------------------
                // Main Category
                // -------------------------------------------------

                entity.HasOne(x => x.MainCategory)
                      .WithMany(x => x.Products)
                      .HasForeignKey(x => x.MainCategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                // -------------------------------------------------
                // Category
                // -------------------------------------------------

                entity.HasOne(x => x.Category)
                      .WithMany(x => x.Products)
                      .HasForeignKey(x => x.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                // -------------------------------------------------
                // SubCategory
                // -------------------------------------------------

                entity.HasOne(x => x.SubCategory)
                      .WithMany(x => x.Products)
                      .HasForeignKey(x => x.SubCategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                // -------------------------------------------------
                // Supplier
                // -------------------------------------------------

                entity.HasOne(x => x.Supplier)
                      .WithMany(x => x.Products)
                      .HasForeignKey(x => x.SupplierId)
                      .OnDelete(DeleteBehavior.Restrict);

                // -------------------------------------------------
                // Warehouse
                // -------------------------------------------------

                entity.HasOne(x => x.Warehouse)
                      .WithMany(x => x.Products)
                      .HasForeignKey(x => x.WarehouseId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }

        // =========================================================
        // WAREHOUSE
        // =========================================================

        private static void ConfigureWarehouse(
            ModelBuilder builder)
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

        // =========================================================
        // PURCHASE ORDER
        // =========================================================

        private static void ConfigurePurchaseOrder(
            ModelBuilder builder)
        {
            builder.Entity<PurchaseOrder>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.PONumber)
                      .HasMaxLength(30)
                      .IsRequired();

                entity.HasIndex(x => x.PONumber)
                      .IsUnique();

                entity.Property(x => x.SubTotal)
                      .HasPrecision(18, 2);

                entity.Property(x => x.Discount)
                      .HasPrecision(18, 2);

                entity.Property(x => x.TaxAmount)
                      .HasPrecision(18, 2);

                entity.Property(x => x.GrandTotal)
                      .HasPrecision(18, 2);

                entity.HasMany(x => x.Items)
                      .WithOne(x => x.PurchaseOrder)
                      .HasForeignKey(x => x.PurchaseOrderId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        // =========================================================
        // PURCHASE ORDER ITEM
        // =========================================================

        private static void ConfigurePurchaseOrderItem(
            ModelBuilder builder)
        {
            builder.Entity<PurchaseOrderItem>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Quantity)
                      .HasPrecision(18, 2);

                entity.Property(x => x.UnitPrice)
                      .HasPrecision(18, 2);

                entity.Property(x => x.Discount)
                      .HasPrecision(18, 2);

                entity.Property(x => x.TaxAmount)
                      .HasPrecision(18, 2);

                entity.Property(x => x.Total)
                      .HasPrecision(18, 2);

                entity.HasOne(x => x.Product)
                      .WithMany()
                      .HasForeignKey(x => x.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }

        // =========================================================
        // GOODS RECEIPT
        // =========================================================

        // =========================================================
        // GOODS RECEIPT
        // =========================================================

        private static void ConfigureGoodsReceipt(
            ModelBuilder builder)
        {
            builder.Entity<GoodsReceipt>(entity =>
            {
                entity.HasKey(x => x.Id);

                // =====================================================
                // GRN NUMBER
                // =====================================================

                entity.Property(x => x.GRNNumber)
                      .HasMaxLength(30)
                      .IsRequired();

                entity.HasIndex(x => x.GRNNumber)
                      .IsUnique();

                // =====================================================
                // RECEIPT TYPE
                // =====================================================

                entity.Property(x => x.ReceiptType)
                      .IsRequired();

                // =====================================================
                // TOTALS
                // =====================================================

                entity.Property(x => x.SubTotal)
                      .HasPrecision(18, 2);

                entity.Property(x => x.Discount)
                      .HasPrecision(18, 2);

                entity.Property(x => x.TaxAmount)
                      .HasPrecision(18, 2);

                entity.Property(x => x.GrandTotal)
                      .HasPrecision(18, 2);

                // =====================================================
                // GIFT FIELDS
                // =====================================================

                entity.Property(x => x.DonorName)
                      .HasMaxLength(200);

                entity.Property(x => x.GiftReason)
                      .HasMaxLength(500);

                entity.Property(x => x.Remarks)
                      .HasMaxLength(500);

                // =====================================================
                // PURCHASE ORDER
                // OPTIONAL
                // Gift GRN has no PO
                // =====================================================

                entity.HasOne(x => x.PurchaseOrder)
                      .WithMany()
                      .HasForeignKey(x => x.PurchaseOrderId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.Restrict);

                // =====================================================
                // SUPPLIER
                // OPTIONAL
                // Gift GRN has no Supplier
                // =====================================================

                entity.HasOne(x => x.Supplier)
                      .WithMany()
                      .HasForeignKey(x => x.SupplierId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.Restrict);

                // =====================================================
                // ITEMS
                // =====================================================

                entity.HasMany(x => x.Items)
                      .WithOne(x => x.GoodsReceipt)
                      .HasForeignKey(x => x.GoodsReceiptId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        // =========================================================
        // GOODS RECEIPT ITEM
        // =========================================================

        // =========================================================
        // GOODS RECEIPT ITEM
        // =========================================================

        private static void ConfigureGoodsReceiptItem(
            ModelBuilder builder)
        {
            builder.Entity<GoodsReceiptItem>(entity =>
            {
                entity.HasKey(x => x.Id);

                // =====================================================
                // DECIMAL PRECISION
                // =====================================================

                entity.Property(x => x.OrderedQuantity)
                      .HasPrecision(18, 2);

                entity.Property(x => x.ReceivedQuantity)
                      .HasPrecision(18, 2);

                entity.Property(x => x.UnitPrice)
                      .HasPrecision(18, 2);

                entity.Property(x => x.Discount)
                      .HasPrecision(18, 2);

                entity.Property(x => x.TaxAmount)
                      .HasPrecision(18, 2);

                entity.Property(x => x.Total)
                      .HasPrecision(18, 2);

                // =====================================================
                // PRODUCT
                // =====================================================

                entity.HasOne(x => x.Product)
                      .WithMany(p => p.GoodsReceiptItems)
                      .HasForeignKey(x => x.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);

                // =====================================================
                // WAREHOUSE
                // =====================================================

                entity.HasOne(x => x.Warehouse)
                      .WithMany(w => w.GoodsReceiptItems)
                      .HasForeignKey(x => x.WarehouseId)
                      .OnDelete(DeleteBehavior.Restrict);

                // =====================================================
                // GOODS RECEIPT
                // =====================================================

                entity.HasOne(x => x.GoodsReceipt)
                      .WithMany(g => g.Items)
                      .HasForeignKey(x => x.GoodsReceiptId)
                      .OnDelete(DeleteBehavior.Cascade);

                // =====================================================
                // PURCHASE ORDER ITEM
                // OPTIONAL
                // Gift GRN has no PO item
                // =====================================================

                entity.HasOne(x => x.PurchaseOrderItem)
                      .WithMany()
                      .HasForeignKey(x => x.PurchaseOrderItemId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }

        // =========================================================
        // WAREHOUSE STOCK
        // =========================================================

        private static void ConfigureWarehouseStock(
    ModelBuilder builder)
        {
            builder.Entity<WarehouseStock>(entity =>
            {
                // =========================
                // PRIMARY KEY
                // =========================

                entity.HasKey(x => x.Id);


                // =========================
                // UNIQUE PRODUCT + WAREHOUSE
                // =========================

                entity.HasIndex(x => new
                {
                    x.ProductId,
                    x.WarehouseId
                })
                .IsUnique();


                // =========================
                // DECIMAL PRECISION
                // =========================

                entity.Property(x => x.QuantityOnHand)
                    .HasPrecision(18, 2);

                entity.Property(x => x.ReservedQuantity)
                    .HasPrecision(18, 2);

                entity.Property(x => x.MinimumStock)
                    .HasPrecision(18, 2);

                entity.Property(x => x.MaximumStock)
                    .HasPrecision(18, 2);


                // =========================
                // PRODUCT RELATIONSHIP
                // =========================

                entity.HasOne(x => x.Product)
                    .WithMany(p => p.WarehouseStocks)
                    .HasForeignKey(x => x.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);


                // =========================
                // WAREHOUSE RELATIONSHIP
                // =========================

                entity.HasOne(x => x.Warehouse)
                    .WithMany(w => w.WarehouseStocks)
                    .HasForeignKey(x => x.WarehouseId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }



        // =========================================================
        // STOCK LEDGER
        // =========================================================

        private static void ConfigureStockLedger(
            ModelBuilder builder)
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
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(x => x.CreatedBy)
                      .HasMaxLength(100);

                entity.Property(x => x.Remarks)
                      .HasMaxLength(500);

                entity.HasOne(x => x.Product)
                      .WithMany(p => p.StockLedgers)
                      .HasForeignKey(x => x.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Warehouse)
                      .WithMany(w => w.StockLedgers)
                      .HasForeignKey(x => x.WarehouseId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }

        // =========================================================
        // STOCK TRANSFER
        // =========================================================

        private static void ConfigureStockTransfer(
            ModelBuilder builder)
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

                entity.HasOne(x => x.Product)
                      .WithMany()
                      .HasForeignKey(x => x.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }

        // =========================================================
        // CUSTOMER
        // =========================================================

        private static void ConfigureCustomer(
            ModelBuilder builder)
        {
            builder.Entity<Customer>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name)
                      .HasMaxLength(150)
                      .IsRequired();

                entity.Property(x => x.Mobile)
                      .HasMaxLength(20);

                entity.Property(x => x.Email)
                      .HasMaxLength(150);

                entity.Property(x => x.GSTNumber)
                      .HasMaxLength(20);

                entity.Property(x => x.PANNumber)
                      .HasMaxLength(20);

                entity.Property(x => x.CreditLimit)
                      .HasPrecision(18, 2);

                entity.HasIndex(x => x.Mobile);

                entity.HasIndex(x => x.GSTNumber);
            });
        }

        // =========================================================
        // SALES ORDER
        // =========================================================

        private static void ConfigureSalesOrder(ModelBuilder builder)
        {
            builder.Entity<SalesOrder>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasIndex(x => x.OrderNumber)
                      .IsUnique();

                // =========================================================
                // ORDER TOTALS
                // =========================================================

                entity.Property(x => x.SubTotal)
                      .HasPrecision(18, 2);

                entity.Property(x => x.DiscountAmount)
                      .HasPrecision(18, 2);

                entity.Property(x => x.TaxAmount)
                      .HasPrecision(18, 2);

                entity.Property(x => x.GrandTotal)
                      .HasPrecision(18, 2);

                entity.Property(x => x.GrossProfit)
                      .HasPrecision(18, 2);

                // =========================================================
                // WAREHOUSE
                // =========================================================

                entity.HasOne(x => x.Warehouse)
                      .WithMany()
                      .HasForeignKey(x => x.WarehouseId)
                      .OnDelete(DeleteBehavior.Restrict);

                // =========================================================
                // ITEMS
                // =========================================================

                entity.HasMany(x => x.SalesOrderItems)
                      .WithOne(x => x.SalesOrder)
                      .HasForeignKey(x => x.SalesOrderId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<SalesOrderItem>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Quantity)
                      .HasPrecision(18, 2);

                // Cost price
                entity.Property(x => x.UnitPrice)
                      .HasPrecision(18, 2);

                // Customer selling price
                entity.Property(x => x.SalePrice)
                      .HasPrecision(18, 2);

                entity.Property(x => x.DiscountPercent)
                      .HasPrecision(5, 2);

                entity.Property(x => x.TaxPercent)
                      .HasPrecision(5, 2);

                entity.Property(x => x.LineTotal)
                      .HasPrecision(18, 2);

                entity.Property(x => x.GrossProfit)
                      .HasPrecision(18, 2);

                entity.HasOne(x => x.Product)
                      .WithMany()
                      .HasForeignKey(x => x.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }

        // =========================================================
        // DAMAGE MASTER
        // =========================================================

        private static void ConfigureDamage(
            ModelBuilder builder)
        {
            builder.Entity<Damage>(entity =>
            {
                // =========================
                // PRIMARY KEY
                // =========================

                entity.HasKey(x => x.Id);


                // =========================
                // DAMAGE CODE
                // =========================

                entity.Property(x => x.DamageCode)
                      .HasMaxLength(30)
                      .IsRequired();

                entity.HasIndex(x => x.DamageCode)
                      .IsUnique();


                // =========================
                // DAMAGE NAME
                // =========================

                entity.Property(x => x.DamageName)
                      .HasMaxLength(100)
                      .IsRequired();

                entity.HasIndex(x => x.DamageName)
                      .IsUnique();


                // =========================
                // DESCRIPTION
                // =========================

                entity.Property(x => x.Description)
                      .HasMaxLength(300);


                // =========================
                // STATUS
                // =========================

                entity.Property(x => x.IsActive)
                      .HasDefaultValue(true);

                entity.Property(x => x.IsDeleted)
                      .HasDefaultValue(false);


                // =========================
                // DATES
                // =========================

                entity.Property(x => x.CreatedDate)
                      .HasDefaultValueSql("GETDATE()");

                entity.Property(x => x.UpdatedDate)
                      .IsRequired(false);
            });
        }

        // =========================================================
        // DAMAGE ENTRY
        // =========================================================

        private static void ConfigureDamageEntry(
            ModelBuilder builder)
        {
            builder.Entity<DamageEntry>(entity =>
            {
                entity.HasKey(x => x.Id);

                // =========================
                // DAMAGE NUMBER
                // =========================

                entity.Property(x => x.DamageNumber)
                      .HasMaxLength(30)
                      .IsRequired();

                entity.HasIndex(x => x.DamageNumber)
                      .IsUnique();


                // =========================
                // QUANTITY & VALUE
                // =========================

                entity.Property(x => x.Quantity)
                      .HasPrecision(18, 2);

                entity.Property(x => x.UnitCost)
                      .HasPrecision(18, 2);

                entity.Property(x => x.TotalValue)
                      .HasPrecision(18, 2);


                // =========================
                // REMARKS
                // =========================

                entity.Property(x => x.Remarks)
                      .HasMaxLength(500);


                // =========================
                // DAMAGE MASTER
                // =========================

                entity.HasOne(x => x.Damage)
                      .WithMany()
                      .HasForeignKey(x => x.DamageId)
                      .OnDelete(DeleteBehavior.Restrict);


                // =========================
                // PRODUCT
                // =========================

                entity.HasOne(x => x.Product)
                      .WithMany()
                      .HasForeignKey(x => x.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);


                // =========================
                // WAREHOUSE
                // =========================

                entity.HasOne(x => x.Warehouse)
                      .WithMany()
                      .HasForeignKey(x => x.WarehouseId)
                      .OnDelete(DeleteBehavior.Restrict);


                // =========================
                // DATES
                // =========================

                entity.Property(x => x.DamageDate)
                      .IsRequired();

                entity.Property(x => x.CreatedDate)
                      .HasDefaultValueSql("GETDATE()");

                entity.Property(x => x.UpdatedDate)
                      .IsRequired(false);


                // =========================
                // SOFT DELETE
                // =========================

                entity.Property(x => x.IsDeleted)
                      .HasDefaultValue(false);
            });
        }

        private static void ConfigureSystemSetting(
    ModelBuilder builder)
        {
            builder.Entity<SystemSetting>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Key)
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(x => x.Value)
                      .HasMaxLength(200)
                      .IsRequired();

                entity.HasIndex(x => x.Key)
                      .IsUnique();
            });
        }
        
// =========================================================
// SUPPLIER PAYMENT
// =========================================================

private static void ConfigureSupplierPayment(
    ModelBuilder builder)
        {
            builder.Entity<SupplierPayment>(entity =>
            {
                entity.HasKey(x => x.Id);

                // =====================================================
                // PAYMENT NUMBER
                // =====================================================

                entity.Property(x => x.PaymentNumber)
                      .HasMaxLength(30)
                      .IsRequired();

                entity.HasIndex(x => x.PaymentNumber)
                      .IsUnique();

                // =====================================================
                // AMOUNT
                // =====================================================

                entity.Property(x => x.Amount)
                      .HasPrecision(18, 2);

                // =====================================================
                // PAYMENT MODE
                // =====================================================

                entity.Property(x => x.PaymentMode)
                      .HasMaxLength(30)
                      .IsRequired();

                // =====================================================
                // REFERENCE NUMBER
                // =====================================================

                entity.Property(x => x.ReferenceNumber)
                      .HasMaxLength(100);

                // =====================================================
                // BANK NAME
                // =====================================================

                entity.Property(x => x.BankName)
                      .HasMaxLength(100);

                // =====================================================
                // REMARKS
                // =====================================================

                entity.Property(x => x.Remarks)
                      .HasMaxLength(500);

                // =====================================================
                // CANCELLATION REMARKS
                // =====================================================

                entity.Property(x => x.CancellationRemarks)
                      .HasMaxLength(500);

                // =====================================================
                // CREATED BY
                // =====================================================

                entity.Property(x => x.CreatedBy)
                      .HasMaxLength(100);

                // =====================================================
                // SUPPLIER
                // =====================================================

                entity.HasOne(x => x.Supplier)
                      .WithMany()
                      .HasForeignKey(x => x.SupplierId)
                      .OnDelete(DeleteBehavior.Restrict);

                // =====================================================
                // INDEX
                // =====================================================

                entity.HasIndex(x => new
                {
                    x.SupplierId,
                    x.PaymentDate
                });
            });
        }

        private static void ConfigureSalesInvoice(ModelBuilder builder)
        {
            builder.Entity<SalesInvoice>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.InvoiceNumber)
                      .HasMaxLength(50)
                      .IsRequired();

                entity.HasIndex(x => x.InvoiceNumber)
                      .IsUnique();

                // One invoice per Sales Order
                entity.HasIndex(x => x.SalesOrderId)
                      .IsUnique();

                entity.Property(x => x.SubTotal)
                      .HasPrecision(18, 2);

                entity.Property(x => x.TaxAmount)
                      .HasPrecision(18, 2);

                entity.Property(x => x.DiscountAmount)
                      .HasPrecision(18, 2);

                entity.Property(x => x.GrandTotal)
                      .HasPrecision(18, 2);

                entity.Property(x => x.PaidAmount)
                      .HasPrecision(18, 2);

                entity.Property(x => x.BalanceDue)
                      .HasPrecision(18, 2);

                entity.Property(x => x.PaymentMode)
                      .HasMaxLength(30);

                entity.Property(x => x.PaymentReference)
                      .HasMaxLength(100);

                entity.Property(x => x.Status)
                      .HasMaxLength(30)
                      .IsRequired();

                entity.Property(x => x.Remarks)
                      .HasMaxLength(500);

                entity.Property(x => x.CreatedBy)
                      .HasMaxLength(100);

                entity.Property(x => x.CancelledBy)
                      .HasMaxLength(100);

                // Sales Invoice -> Sales Order
                entity.HasOne(x => x.SalesOrder)
                      .WithMany()
                      .HasForeignKey(x => x.SalesOrderId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Sales Invoice -> Warehouse
                entity.HasOne(x => x.Warehouse)
                      .WithMany()
                      .HasForeignKey(x => x.WarehouseId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Invoice -> Items
                entity.HasMany(x => x.Items)
                      .WithOne(x => x.SalesInvoice)
                      .HasForeignKey(x => x.SalesInvoiceId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<SalesInvoiceItem>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Quantity)
                      .HasPrecision(18, 2);

                entity.Property(x => x.UnitPrice)
                      .HasPrecision(18, 2);

                entity.Property(x => x.TaxPercent)
                      .HasPrecision(5, 2);

                entity.Property(x => x.DiscountPercent)
                      .HasPrecision(5, 2);

                entity.Property(x => x.LineTotal)
                      .HasPrecision(18, 2);

                // Invoice Item -> Sales Order Item
                entity.HasOne(x => x.SalesOrderItem)
                      .WithMany()
                      .HasForeignKey(x => x.SalesOrderItemId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Invoice Item -> Product
                entity.HasOne(x => x.Product)
                      .WithMany()
                      .HasForeignKey(x => x.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }


        // =========================================================
        // DELIVERY
        // =========================================================

        private static void ConfigureDelivery(
            ModelBuilder builder)
        {
            builder.Entity<Delivery>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.DeliveryNumber)
                      .HasMaxLength(30)
                      .IsRequired();

                entity.HasIndex(x => x.DeliveryNumber)
                      .IsUnique();

                entity.Property(x => x.Remarks)
                      .HasMaxLength(500);

                // Sales Order
                entity.HasOne(x => x.SalesOrder)
                      .WithMany()
                      .HasForeignKey(x => x.SalesOrderId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Warehouse
                entity.HasOne(x => x.Warehouse)
                      .WithMany()
                      .HasForeignKey(x => x.WarehouseId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Delivery -> Items
                entity.HasMany(x => x.Items)
                      .WithOne(x => x.Delivery)
                      .HasForeignKey(x => x.DeliveryId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<DeliveryItem>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Quantity)
                      .HasPrecision(18, 2);

                entity.Property(x => x.UnitPrice)
                      .HasPrecision(18, 2);

                // Sales Order Item
                entity.HasOne(x => x.SalesOrderItem)
                      .WithMany()
                      .HasForeignKey(x => x.SalesOrderItemId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Product
                entity.HasOne(x => x.Product)
                      .WithMany()
                      .HasForeignKey(x => x.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }



    }
}