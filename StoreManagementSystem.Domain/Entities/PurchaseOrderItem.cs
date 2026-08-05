namespace StoreManagementSystem.Domain.Entities
{
    public class PurchaseOrderItem
    {
        public int Id { get; set; }

        public int PurchaseOrderId { get; set; }

        public PurchaseOrder? PurchaseOrder { get; set; }

        public int ProductId { get; set; }

        public Product? Product { get; set; }

        public decimal Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal Discount { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal Total { get; set; }
    }
}
