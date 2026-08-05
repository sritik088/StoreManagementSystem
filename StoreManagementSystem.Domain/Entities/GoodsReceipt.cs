using StoreManagementSystem.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace StoreManagementSystem.Domain.Entities
{
    public class GoodsReceipt
    {
        public int Id { get; set; }

        [Required]
        [StringLength(30)]
        public string GRNNumber { get; set; } = string.Empty;

        public int PurchaseOrderId { get; set; }

        public PurchaseOrder PurchaseOrder { get; set; } = null!;

        [Required]
        public DateTime ReceiptDate { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string? InvoiceNumber { get; set; }

        public DateTime? InvoiceDate { get; set; }

        [StringLength(100)]
        public string? VehicleNumber { get; set; }

        [StringLength(100)]
        public string? TransportName { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        public decimal SubTotal { get; set; }

        public decimal Discount { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal GrandTotal { get; set; }

        public GoodsReceiptStatus Status { get; set; }
            = GoodsReceiptStatus.Received;

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedDate { get; set; }
            = DateTime.Now;

        public DateTime? UpdatedDate { get; set; }

        public int SupplierId { get; set; }

        public Supplier? Supplier { get; set; }

        public ICollection<GoodsReceiptItem> Items
            = new List<GoodsReceiptItem>();
    }
}