using Microsoft.AspNetCore.Mvc.Rendering;

namespace StoreManagementSystem.Web.ViewModels.SalesOrder
{
    public class SalesOrderItemViewModel
    {
        public int Id { get; set; }

        // =========================================================
        // PRODUCT
        // =========================================================

        public int ProductId { get; set; }

        public string ProductSKU { get; set; } = string.Empty;

        public string ProductName { get; set; } = string.Empty;

        // =========================================================
        // QUANTITY
        // =========================================================

        public decimal Quantity { get; set; } = 1;

        // =========================================================
        // COST / PURCHASE PRICE
        //
        // This is the price at which the stock was purchased.
        // Example: ₹900
        // =========================================================

        public decimal UnitPrice { get; set; }

        // =========================================================
        // SELLING PRICE
        //
        // This is YOUR selling price.
        // Example: ₹1,100
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
            SubTotal *
            DiscountPercent /
            100m;

        public decimal TaxableAmount =>
            Math.Max(
                SubTotal -
                DiscountAmount,
                0m);

        public decimal TaxAmount =>
            TaxableAmount *
            TaxPercent /
            100m;

        public decimal LineTotal =>
            TaxableAmount +
            TaxAmount;

        // =========================================================
        // GROSS PROFIT
        //
        // Tax is not profit.
        //
        // Gross Profit =
        // Net Selling Value - Cost Value
        // =========================================================

        public decimal GrossProfit =>
            (SubTotal - DiscountAmount)
            -
            (Quantity * UnitPrice);

        // =========================================================
        // PRODUCT DROPDOWN
        // =========================================================

        public List<SelectListItem> Products { get; set; }
            = new List<SelectListItem>();
    }
}