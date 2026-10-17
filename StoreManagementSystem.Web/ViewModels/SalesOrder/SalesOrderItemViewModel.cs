
using Microsoft.AspNetCore.Mvc.Rendering;

namespace StoreManagementSystem.Web.ViewModels.SalesOrder
{
    public class SalesOrderItemViewModel
    {
        // =========================================================
        // IDENTIFICATION
        // =========================================================

        public int Id { get; set; }

        // =========================================================
        // PRODUCT INFORMATION
        // =========================================================

        public int ProductId { get; set; }

        // Preferred product display field
        public string ProductTitle { get; set; } = string.Empty;

        // Retained for compatibility with existing views/code
        public string ProductSKU { get; set; } = string.Empty;

        public string ProductName { get; set; } = string.Empty;

        // =========================================================
        // QUANTITY
        // =========================================================

        public decimal Quantity { get; set; } = 1m;

        // =========================================================
        // COST / PURCHASE PRICE
        // =========================================================

        public decimal UnitPrice { get; set; }

        // =========================================================
        // SELLING PRICE
        // =========================================================

        public decimal SalePrice { get; set; }

        // =========================================================
        // TAX
        // =========================================================

        public decimal TaxPercent { get; set; }

        // =========================================================
        // DISCOUNT
        // =========================================================

        public decimal DiscountPercent { get; set; }

        // =========================================================
        // CALCULATED VALUES
        // =========================================================

        public decimal SubTotal =>
            Quantity * SalePrice;

        public decimal DiscountAmount =>
            SubTotal * DiscountPercent / 100m;

        public decimal TaxableAmount =>
            Math.Max(
                SubTotal - DiscountAmount,
                0m);

        public decimal TaxAmount =>
            TaxableAmount * TaxPercent / 100m;

        public decimal LineTotal =>
            TaxableAmount + TaxAmount;

        // =========================================================
        // GROSS PROFIT
        // Excludes tax and deducts the purchase cost.
        // =========================================================

        public decimal GrossProfit =>
            TaxableAmount - (Quantity * UnitPrice);

        // =========================================================
        // PRODUCT DROPDOWN
        // =========================================================

        public List<SelectListItem> Products { get; set; }
            = new List<SelectListItem>();
    }
}
