using System.ComponentModel.DataAnnotations;

namespace StoreManagementSystem.Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }

        // =========================
        // Main Category
        // =========================

        [Required]
        public int MainCategoryId { get; set; }

        public MainCategory? MainCategory { get; set; }

        // =========================
        // Category
        // =========================

        [Required]
        public int CategoryId { get; set; }

        public Category? Category { get; set; }

        // =========================
        // Sub Category
        // =========================

        [Required]
        public int SubCategoryId { get; set; }

        public SubCategory? SubCategory { get; set; }

        // =========================
        // Identification
        // =========================

        [Required]
        [StringLength(50)]
        public string SKU { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Barcode { get; set; }

        // =========================
        // Supplier
        // =========================

        [Required]
        public int SupplierId { get; set; }

        public Supplier? Supplier { get; set; }

        // =========================
        // Warehouse
        // =========================

        [Required]
        public int WarehouseId { get; set; }

        public Warehouse? Warehouse { get; set; }

        public bool IsDeleted { get; set; } = false;


        // =========================
        // Navigation Collections
        // =========================

        public ICollection<GoodsReceiptItem> GoodsReceiptItems { get; set; }
            = new List<GoodsReceiptItem>();

        public ICollection<WarehouseStock> WarehouseStocks { get; set; }
            = new List<WarehouseStock>();

        public ICollection<StockLedger> StockLedgers { get; set; }
    = new List<StockLedger>();
    }
}