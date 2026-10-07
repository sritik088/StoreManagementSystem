using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace StoreManagementSystem.Web.ViewModels.Product
{
    public class ProductViewModel
    {
        public int Id { get; set; }

        // =========================
        // Main Category
        // =========================

        [Required(ErrorMessage = "Please select a main category.")]
        [Display(Name = "Main Category")]
        public int MainCategoryId { get; set; }

        public IEnumerable<SelectListItem> MainCategories { get; set; }
            = new List<SelectListItem>();

        // =========================
        // Category
        // =========================

        [Required(ErrorMessage = "Please select a category.")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        public IEnumerable<SelectListItem> Categories { get; set; }
            = new List<SelectListItem>();

        // =========================
        // Sub Category
        // =========================

        [Required(ErrorMessage = "Please select a sub category.")]
        [Display(Name = "Sub Category")]
        public int SubCategoryId { get; set; }

        public IEnumerable<SelectListItem> SubCategories { get; set; }
            = new List<SelectListItem>();

        // =========================
        // Identification
        // =========================

        [Required]
        [StringLength(50)]
        public string SKU { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Barcode { get; set; }

        // =========================
        // Supplier
        // =========================

        [Required(ErrorMessage = "Please select a supplier.")]
        [Display(Name = "Supplier")]
        public int SupplierId { get; set; }

        public IEnumerable<SelectListItem> Suppliers { get; set; }
            = new List<SelectListItem>();

        // =========================
        // Warehouse
        // =========================

        [Required(ErrorMessage = "Please select a warehouse.")]
        [Display(Name = "Warehouse")]
        public int WarehouseId { get; set; }

        public IEnumerable<SelectListItem> Warehouses { get; set; }
            = new List<SelectListItem>();


    }
}