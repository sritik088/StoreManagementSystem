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

        // =========================================================
        // RECEIPT TYPE
        // =========================================================

        public GoodsReceiptType ReceiptType { get; set; }
            = GoodsReceiptType.Purchase;

        // =========================================================
        // PURCHASE ORDER
        // Nullable because Gift GRN does not have PO
        // =========================================================

        public int? PurchaseOrderId { get; set; }

        public PurchaseOrder? PurchaseOrder { get; set; }

        // =========================================================
        // RECEIPT DATE
        // =========================================================

        [Required]
        public DateTime ReceiptDate { get; set; } = DateTime.Now;

        // =========================================================
        // INVOICE
        // =========================================================

        [StringLength(100)]
        public string? InvoiceNumber { get; set; }

        public DateTime? InvoiceDate { get; set; }

        // =========================================================
        // TRANSPORT
        // =========================================================

        [StringLength(100)]
        public string? VehicleNumber { get; set; }

        [StringLength(100)]
        public string? TransportName { get; set; }

        // =========================================================
        // GIFT DETAILS
        // =========================================================

        [StringLength(200)]
        public string? DonorName { get; set; }

        [StringLength(500)]
        public string? GiftReason { get; set; }

        // =========================================================
        // REMARKS
        // =========================================================

        [StringLength(500)]
        public string? Remarks { get; set; }

        // =========================================================
        // TOTALS
        // =========================================================

        public decimal SubTotal { get; set; }

        public decimal Discount { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal GrandTotal { get; set; }

        // =========================================================
        // STATUS
        // =========================================================

        public GoodsReceiptStatus Status { get; set; }
            = GoodsReceiptStatus.Received;

        // =========================================================
        // AUDIT
        // =========================================================

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedDate { get; set; }
            = DateTime.Now;

        public DateTime? UpdatedDate { get; set; }

        // =========================================================
        // SUPPLIER
        // Nullable because Gift GRN may not have supplier
        // =========================================================

        public int? SupplierId { get; set; }

        public Supplier? Supplier { get; set; }

        // =========================================================
        // ITEMS
        // =========================================================

        public ICollection<GoodsReceiptItem> Items { get; set; }
            = new List<GoodsReceiptItem>();
    }
}