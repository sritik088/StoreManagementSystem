using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StoreManagementSystem.Domain.Entities
{
    public class SalesInvoice
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string InvoiceNumber { get; set; } = string.Empty;

        [Required]
        public DateTime InvoiceDate { get; set; } = DateTime.Now;

        [Required]
        public int SalesOrderId { get; set; }

        [ForeignKey(nameof(SalesOrderId))]
        public SalesOrder SalesOrder { get; set; } = null!;

        public int WarehouseId { get; set; }

        [ForeignKey(nameof(WarehouseId))]
        public Warehouse? Warehouse { get; set; }

        [StringLength(100)]
        public string? CustomerName { get; set; }

        [StringLength(20)]
        public string? CustomerPhone { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal GrandTotal { get; set; }

        [StringLength(30)]
        public string PaymentMode { get; set; } = "Cash";

        [Column(TypeName = "decimal(18,2)")]
        public decimal PaidAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BalanceDue { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Issued";

        [StringLength(100)]
        public string? PaymentReference { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string? CreatedBy { get; set; }

        public DateTime? CancelledDate { get; set; }

        [StringLength(100)]
        public string? CancelledBy { get; set; }

        public ICollection<SalesInvoiceItem> Items { get; set; }
            = new List<SalesInvoiceItem>();
    }
}