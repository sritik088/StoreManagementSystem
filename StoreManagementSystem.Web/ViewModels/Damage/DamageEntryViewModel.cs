using System.ComponentModel.DataAnnotations;

namespace StoreManagementSystem.Web.ViewModels.Damage
{
    public class DamageEntryViewModel
    {
        public int Id { get; set; }

        public string DamageNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Damage Date")]
        public DateTime DamageDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Please select damage type.")]
        [Display(Name = "Damage Type")]
        public int DamageId { get; set; }

        [Required(ErrorMessage = "Please select product.")]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Please select warehouse.")]
        [Display(Name = "Warehouse")]
        public int WarehouseId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
        public decimal Quantity { get; set; }

        [Display(Name = "Unit Cost")]
        public decimal UnitCost { get; set; }

        [Display(Name = "Total Value")]
        public decimal TotalValue { get; set; }

        public decimal AvailableQuantity { get; set; }

        public string? Remarks { get; set; }

        public List<DamageDropdownItem> Damages { get; set; } = new();

        public List<ProductDropdownItem> Products { get; set; } = new();

        public List<WarehouseDropdownItem> Warehouses { get; set; } = new();
    }

    public class DamageDropdownItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class ProductDropdownItem
    {
        public int Id { get; set; }
        public string SKU { get; set; } = string.Empty;
    }

    public class WarehouseDropdownItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}