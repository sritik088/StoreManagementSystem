namespace StoreManagementSystem.Domain.Entities
{
    public class GoodsReceiptItem
    {
        public int Id { get; set; }

        // =========================================================
        // GOODS RECEIPT
        // =========================================================

        public int GoodsReceiptId { get; set; }

        public GoodsReceipt? GoodsReceipt { get; set; }

        // =========================================================
        // PURCHASE ORDER ITEM
        // Nullable because Gift GRN has no PO item
        // =========================================================

        public int? PurchaseOrderItemId { get; set; }

        public PurchaseOrderItem? PurchaseOrderItem { get; set; }

        // =========================================================
        // PRODUCT
        // =========================================================

        public int ProductId { get; set; }

        public Product? Product { get; set; }

        // =========================================================
        // WAREHOUSE
        // =========================================================

        public int WarehouseId { get; set; }

        public Warehouse? Warehouse { get; set; }

        // =========================================================
        // QUANTITIES
        // =========================================================

        public decimal OrderedQuantity { get; set; }

        public decimal ReceivedQuantity { get; set; }

        // =========================================================
        // PRICE / TAX / DISCOUNT
        // =========================================================

        public decimal UnitPrice { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal Discount { get; set; }

        public decimal Total { get; set; }
    }
}