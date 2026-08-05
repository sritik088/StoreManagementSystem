using System.ComponentModel.DataAnnotations;

namespace StoreManagementSystem.Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }

        // ==========================
        // Basic Information
        // ==========================

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string SKU { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Barcode { get; set; }

        [StringLength(20)]
        public string? HSNCode { get; set; }

        // ==========================
        // Relationships
        // ==========================

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public int SubCategoryId { get; set; }
        public SubCategory? SubCategory { get; set; }

        public int BrandId { get; set; }
        public Brand? Brand { get; set; }

        public int SupplierId { get; set; }
        public Supplier? Supplier { get; set; }

        public int UnitId { get; set; }
        public Unit? Unit { get; set; }

        public int TaxId { get; set; }
        public Tax? Tax { get; set; }

        public int WarehouseId { get; set; }
        public Warehouse? Warehouse { get; set; }

        // ==========================
        // Pricing
        // ==========================

        public decimal PurchasePrice { get; set; }

        public decimal SellingPrice { get; set; }

        public decimal DiscountPrice { get; set; }

        // ==========================
        // Inventory
        // ==========================

        public decimal OpeningStock { get; set; }

        public decimal CurrentStock { get; set; }

        public decimal ReorderLevel { get; set; }

        public decimal MaximumStock { get; set; }

        // ==========================
        // Product Image
        // ==========================

        [StringLength(300)]
        public string? ImageUrl { get; set; }

        // ==========================
        // Description
        // ==========================

        [StringLength(500)]
        public string? Description { get; set; }

        // ==========================
        // Status
        // ==========================

        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? UpdatedDate { get; set; }

        public ICollection<GoodsReceiptItem> GoodsReceiptItems { get; set; }
    = new List<GoodsReceiptItem>();

        public ICollection<WarehouseStock> WarehouseStocks { get; set; }
    = new List<WarehouseStock>();

        public ICollection<StockLedger> StockLedgers { get; set; }
    = new List<StockLedger>();
    }
}