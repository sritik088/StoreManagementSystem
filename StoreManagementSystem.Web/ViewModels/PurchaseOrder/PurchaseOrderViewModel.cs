using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace StoreManagementSystem.Web.ViewModels.PurchaseOrder
{
    public class PurchaseOrderViewModel
    {
        public int Id { get; set; }

        public string PONumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Supplier")]
        public int SupplierId { get; set; }

        [Required]
        [Display(Name = "Order Date")]
        [DataType(DataType.Date)]
        public DateTime OrderDate { get; set; } = DateTime.Today;

        [Required]
        [Display(Name = "Expected Date")]
        [DataType(DataType.Date)]
        public DateTime ExpectedDate { get; set; } = DateTime.Today.AddDays(7);

        public decimal SubTotal { get; set; }

        public decimal Discount { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal GrandTotal { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        public string Status { get; set; } = "Draft";

        // Dropdown
        public IEnumerable<SelectListItem> Suppliers { get; set; }
            = Enumerable.Empty<SelectListItem>();

        // Detail Grid
        public List<PurchaseOrderItemViewModel> Items { get; set; }
            = new();
    }
}
