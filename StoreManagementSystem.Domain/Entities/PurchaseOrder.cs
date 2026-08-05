using System.ComponentModel.DataAnnotations;
using StoreManagementSystem.Domain.Enums;

namespace StoreManagementSystem.Domain.Entities
{
    public class PurchaseOrder
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(30)]
        public string PONumber { get; set; } = string.Empty;

        public int SupplierId { get; set; }

        public Supplier? Supplier { get; set; }

        public DateTime OrderDate { get; set; }

        public DateTime ExpectedDate { get; set; }

        public decimal SubTotal { get; set; }

        public decimal Discount { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal GrandTotal { get; set; }

        [MaxLength(500)]
        public string? Remarks { get; set; }

        public PurchaseOrderStatus Status { get; set; }

        public ICollection<PurchaseOrderItem> Items { get; set; }
            = new List<PurchaseOrderItem>();

        public bool IsDeleted { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }
}
