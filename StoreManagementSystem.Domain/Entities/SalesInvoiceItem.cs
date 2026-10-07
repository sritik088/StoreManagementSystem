using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StoreManagementSystem.Domain.Entities
{
    public class SalesInvoiceItem
    {
        public int Id { get; set; }

        [Required]
        public int SalesInvoiceId { get; set; }

        [ForeignKey(nameof(SalesInvoiceId))]
        public SalesInvoice SalesInvoice { get; set; } = null!;

        [Required]
        public int SalesOrderItemId { get; set; }

        [ForeignKey(nameof(SalesOrderItemId))]
        public SalesOrderItem SalesOrderItem { get; set; } = null!;

        [Required]
        public int ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxPercent { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountPercent { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal LineTotal { get; set; }
    }
}