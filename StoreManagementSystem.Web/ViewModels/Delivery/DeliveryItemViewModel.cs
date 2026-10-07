namespace StoreManagementSystem.Web.ViewModels.Delivery
{
    public class DeliveryItemViewModel
    {
        public int SalesOrderItemId { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; }
            = string.Empty;

        public string SKU { get; set; }
            = string.Empty;

        public decimal OrderedQuantity { get; set; }

        public decimal AlreadyDeliveredQuantity
        { get; set; }

        public decimal RemainingQuantity { get; set; }

        public decimal AvailableStock { get; set; }

        public decimal DeliveryQuantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal Total =>
            DeliveryQuantity * UnitPrice;
    }
}