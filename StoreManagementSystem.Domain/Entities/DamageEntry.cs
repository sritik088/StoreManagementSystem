namespace StoreManagementSystem.Domain.Entities
{
    public class DamageEntry
    {
        public int Id { get; set; }

        public string DamageNumber { get; set; } = string.Empty;

        public DateTime DamageDate { get; set; } = DateTime.Now;

        public int DamageId { get; set; }
        public Damage? Damage { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int WarehouseId { get; set; }
        public Warehouse? Warehouse { get; set; }

        public decimal Quantity { get; set; }

        public decimal UnitCost { get; set; }

        public decimal TotalValue { get; set; }

        public string? Remarks { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? UpdatedDate { get; set; }
    }
}