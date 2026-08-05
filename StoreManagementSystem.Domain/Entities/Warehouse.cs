using System.ComponentModel.DataAnnotations;

namespace StoreManagementSystem.Domain.Entities
{
    public class Warehouse
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Code { get; set; }

        [StringLength(100)]
        public string? ManagerName { get; set; }

        [StringLength(20)]
        public string? ContactNumber { get; set; }

        [StringLength(150)]
        public string? Email { get; set; }

        [StringLength(300)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? State { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }

        [StringLength(10)]
        public string? PostalCode { get; set; }

        [StringLength(300)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? UpdatedDate { get; set; }

        // Navigation Property
        public ICollection<Product> Products { get; set; }
            = new List<Product>();

        public ICollection<GoodsReceiptItem> GoodsReceiptItems { get; set; }
    = new List<GoodsReceiptItem>();

        public ICollection<WarehouseStock> WarehouseStocks { get; set; }
    = new List<WarehouseStock>();

        public ICollection<StockLedger> StockLedgers { get; set; }
    = new List<StockLedger>();
    }
}
