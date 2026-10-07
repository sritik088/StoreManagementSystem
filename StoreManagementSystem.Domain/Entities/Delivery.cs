using StoreManagementSystem.Domain.Enums;

namespace StoreManagementSystem.Domain.Entities
{
    public class Delivery
    {
        public int Id { get; set; }

        public string DeliveryNumber { get; set; }
            = string.Empty;

        public int SalesOrderId { get; set; }

        public SalesOrder? SalesOrder { get; set; }

        public int WarehouseId { get; set; }

        public Warehouse? Warehouse { get; set; }

        public DateTime DeliveryDate { get; set; }
            = DateTime.Now;

        public DeliveryStatus Status { get; set; }
            = DeliveryStatus.Pending;

        public string? Remarks { get; set; }

        public ICollection<DeliveryItem> Items { get; set; }
            = new List<DeliveryItem>();
    }
}