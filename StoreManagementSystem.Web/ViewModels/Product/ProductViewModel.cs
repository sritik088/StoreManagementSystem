using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace StoreManagementSystem.Web.ViewModels.Product
{
    public class ProductViewModel
    {
        public int Id { get; set; }

        // ==========================
        // Basic Information
        // ==========================

        [Required]
        [Display(Name = "Product Name")]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Display(Name = "SKU")]
        [StringLength(50)]
        public string SKU { get; set; } = string.Empty;

        [Display(Name = "Barcode")]
        [StringLength(50)]
        public string? Barcode { get; set; }

        [Display(Name = "HSN Code")]
        [StringLength(20)]
        public string? HSNCode { get; set; }

        // ==========================
        // Dropdowns
        // ==========================

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Required]
        [Display(Name = "Sub Category")]
        public int SubCategoryId { get; set; }

        [Required]
        [Display(Name = "Brand")]
        public int BrandId { get; set; }

        [Required]
        [Display(Name = "Supplier")]
        public int SupplierId { get; set; }

        [Required]
        [Display(Name = "Unit")]
        public int UnitId { get; set; }

        [Required]
        [Display(Name = "Tax")]
        public int TaxId { get; set; }

        [Required]
        [Display(Name = "Warehouse")]
        public int WarehouseId { get; set; }

        // ==========================
        // Pricing
        // ==========================

        [Display(Name = "Purchase Price")]
        public decimal PurchasePrice { get; set; }

        [Display(Name = "Selling Price")]
        public decimal SellingPrice { get; set; }

        [Display(Name = "Discount Price")]
        public decimal DiscountPrice { get; set; }

        // ==========================
        // Stock
        // ==========================

        [Display(Name = "Opening Stock")]
        public decimal OpeningStock { get; set; }

        [Display(Name = "Current Stock")]
        public decimal CurrentStock { get; set; }

        [Display(Name = "Reorder Level")]
        public decimal ReorderLevel { get; set; }

        [Display(Name = "Maximum Stock")]
        public decimal MaximumStock { get; set; }

        // ==========================
        // Image
        // ==========================

        public string? ImageUrl { get; set; }

        [Display(Name = "Product Image")]
        public IFormFile? ImageFile { get; set; }

        // ==========================
        // Description
        // ==========================

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        // ==========================
        // Dropdown Data Sources
        // ==========================

        public IEnumerable<SelectListItem> Categories { get; set; }
            = Enumerable.Empty<SelectListItem>();

        public IEnumerable<SelectListItem> SubCategories { get; set; }
            = Enumerable.Empty<SelectListItem>();

        public IEnumerable<SelectListItem> Brands { get; set; }
            = Enumerable.Empty<SelectListItem>();

        public IEnumerable<SelectListItem> Suppliers { get; set; }
            = Enumerable.Empty<SelectListItem>();

        public IEnumerable<SelectListItem> Units { get; set; }
            = Enumerable.Empty<SelectListItem>();

        public IEnumerable<SelectListItem> Taxes { get; set; }
            = Enumerable.Empty<SelectListItem>();

        public IEnumerable<SelectListItem> Warehouses { get; set; }
            = Enumerable.Empty<SelectListItem>();
    }
}
