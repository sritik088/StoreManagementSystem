namespace StoreManagementSystem.Domain.Entities
{
    public class SalesOrderItem
    {
        public int Id { get; set; }

        public int SalesOrderId { get; set; }

        public SalesOrder? SalesOrder { get; set; }

        public int ProductId { get; set; }

        public Product? Product { get; set; }

        // =========================================================
        // QUANTITY
        // =========================================================

        public decimal Quantity { get; set; }

        // =========================================================
        // COST PRICE
        // =========================================================

        /// <summary>
        /// Actual purchase/GRN cost per unit.
        /// This is NOT the customer's selling price.
        /// </summary>
        public decimal UnitPrice { get; set; }

        // =========================================================
        // SELLING PRICE
        // =========================================================

        /// <summary>
        /// Price at which the product is sold to the customer.
        /// Entered by the user in Sales Order.
        /// </summary>
        public decimal SalePrice { get; set; }

        // =========================================================
        // DISCOUNT / TAX
        // =========================================================

        public decimal DiscountPercent { get; set; }

        public decimal TaxPercent { get; set; }

        // =========================================================
        // CALCULATED VALUES
        // =========================================================

        public decimal LineTotal { get; set; }

        /// <summary>
        /// Gross profit for this particular line.
        /// </summary>
        public decimal GrossProfit { get; set; }
    }
}