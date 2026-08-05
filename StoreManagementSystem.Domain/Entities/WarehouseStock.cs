using System.ComponentModel.DataAnnotations;

namespace StoreManagementSystem.Domain.Entities
{
    public class WarehouseStock
    {
        public int Id { get; set; }

        [Required]
        public int WarehouseId { get; set; }

        public Warehouse? Warehouse { get; set; }

        [Required]
        public int ProductId { get; set; }

        public Product? Product { get; set; }

        public decimal QuantityOnHand { get; set; }

        public decimal ReservedQuantity { get; set; }

        public decimal AvailableQuantity =>
            QuantityOnHand - ReservedQuantity;

        public decimal MinimumStock { get; set; }

        public decimal MaximumStock { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}