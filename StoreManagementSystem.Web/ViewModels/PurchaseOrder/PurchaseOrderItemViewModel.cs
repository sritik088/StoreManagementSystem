using System.ComponentModel.DataAnnotations;

namespace StoreManagementSystem.Web.ViewModels.PurchaseOrder
{
    public class PurchaseOrderItemViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please select a product.")]
        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        [Range(0.01, 999999)]
        public decimal Quantity { get; set; } = 1;

        [Range(0, 999999)]
        public decimal UnitPrice { get; set; }

        // =====================================================
        // DISCOUNT
        // =====================================================

        // Discount percentage entered by the user in the UI
        [Range(0, 100)]
        public decimal DiscountPercent { get; set; }

        // Actual discount amount in ₹
        // This is what should be stored in the database
        public decimal Discount { get; set; }

        // =====================================================
        // TAX
        // =====================================================

        // Tax percentage entered by the user in the UI
        [Range(0, 100)]
        public decimal TaxPercent { get; set; }

        // Actual tax amount in ₹
        // This is what should be stored in the database
        public decimal TaxAmount { get; set; }

        // =====================================================
        // TOTAL
        // =====================================================

        public decimal Total { get; set; }
    }
}