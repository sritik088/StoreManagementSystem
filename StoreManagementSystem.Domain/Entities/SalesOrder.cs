using StoreManagementSystem.Domain.Enums;

namespace StoreManagementSystem.Domain.Entities
{
    public class SalesOrder
    {
        public int Id { get; set; }

        public string OrderNumber { get; set; } = string.Empty;

        public DateTime OrderDate { get; set; }

        public int WarehouseId { get; set; }

        public Warehouse? Warehouse { get; set; }

        public string? CustomerName { get; set; }

        public string? CustomerPhone { get; set; }

        public string? Remarks { get; set; }

        public SalesOrderStatus Status { get; set; }
            = SalesOrderStatus.Draft;

        // =========================================================
        // TOTALS
        // =========================================================

        public decimal SubTotal { get; set; }

        public decimal DiscountAmount { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal GrandTotal { get; set; }

        // =========================================================
        // GROSS PROFIT
        // =========================================================

        /// <summary>
        /// Total gross profit for this Sales Order.
        /// Tax is excluded from profit.
        /// </summary>
        public decimal GrossProfit { get; set; }

        // =========================================================
        // AUDIT
        // =========================================================

        public bool IsDeleted { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        // =========================================================
        // ITEMS
        // =========================================================

        public ICollection<SalesOrderItem> SalesOrderItems { get; set; }
            = new List<SalesOrderItem>();
    }
}