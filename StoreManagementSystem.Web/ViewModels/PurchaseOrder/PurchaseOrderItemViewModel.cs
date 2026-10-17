
using System.ComponentModel.DataAnnotations;

namespace StoreManagementSystem.Web.ViewModels.PurchaseOrder
{
    public class PurchaseOrderItemViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please select a product.")]
        public int ProductId { get; set; }

        // Product information displayed separately
        public string ProductName { get; set; } = string.Empty;

        public string SKU { get; set; } = string.Empty;

        [Range(0.01, 999999)]
        public decimal Quantity { get; set; } = 1;

        [Range(0, 999999)]
        public decimal UnitPrice { get; set; }

        // Discount
        [Range(0, 100)]
        public decimal DiscountPercent { get; set; }

        public decimal Discount { get; set; }

        // Tax
        [Range(0, 100)]
        public decimal TaxPercent { get; set; }

        public decimal TaxAmount { get; set; }

        // Total
        public decimal Total { get; set; }
    }
}
