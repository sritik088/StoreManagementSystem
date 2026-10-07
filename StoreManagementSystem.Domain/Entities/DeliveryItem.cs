namespace StoreManagementSystem.Domain.Entities
{
    public class DeliveryItem
    {
        public int Id { get; set; }

        public int DeliveryId { get; set; }

        public Delivery? Delivery { get; set; }

        public int SalesOrderItemId { get; set; }

        public SalesOrderItem? SalesOrderItem { get; set; }

        public int ProductId { get; set; }

        public Product? Product { get; set; }

        public decimal Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal Total =>
            Quantity * UnitPrice;
    }
}