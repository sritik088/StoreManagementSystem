using System.ComponentModel.DataAnnotations;


namespace StoreManagementSystem.Web.ViewModels.StockTransfer
{
    public class StockTransferItemViewModel
    {
        public int Id { get; set; }

        // =========================================================
        // PRODUCT
        // =========================================================

        [Required(ErrorMessage = "Please select a product.")]
        public int ProductId { get; set; }

        // Display-only product information
        public string ProductName { get; set; } = string.Empty;

        public string SKU { get; set; } = string.Empty;

        // =========================================================
        // SUBCATEGORY
        // =========================================================

        // Automatically loaded from Product
        public int SubCategoryId { get; set; }

        public string SubCategoryName { get; set; } = string.Empty;

        // =========================================================
        // AVAILABLE QUANTITY
        // =========================================================

        // Available stock in From Warehouse
        public decimal AvailableQuantity { get; set; }

        // =========================================================
        // QUANTITY
        // =========================================================

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(
            0.01,
            double.MaxValue,
            ErrorMessage = "Quantity must be greater than zero.")]
        public decimal Quantity { get; set; }

        // =========================================================
        // UNIT COST
        // =========================================================

        [Range(
            0,
            double.MaxValue,
            ErrorMessage = "Unit cost cannot be negative.")]
        public decimal UnitCost { get; set; }

        // =========================================================
        // TOTAL
        // =========================================================

        public decimal Total
        {
            get
            {
                return Quantity * UnitCost;
            }
        }
    }
}