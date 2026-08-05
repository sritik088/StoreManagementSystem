namespace StoreManagementSystem.Domain.Entities
{
    public class GoodsReceiptItem
    {
        public int Id { get; set; }

        public int GoodsReceiptId { get; set; }

        public GoodsReceipt? GoodsReceipt { get; set; }

        public int PurchaseOrderItemId { get; set; }

        public PurchaseOrderItem? PurchaseOrderItem { get; set; }

        public int ProductId { get; set; }

        public Product? Product { get; set; }

        public int WarehouseId { get; set; }

        public Warehouse? Warehouse { get; set; }

        public decimal OrderedQuantity { get; set; }

        public decimal ReceivedQuantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal Discount { get; set; }

        public decimal Total { get; set; }
    }
}