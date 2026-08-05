using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace StoreManagementSystem.Web.ViewModels.PurchaseOrder
{
    public class PurchaseOrderItemViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please select a product.")]
        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        [Range(0.01, 999999)]
        public decimal Quantity { get; set; }

        [Range(0.01, 999999)]
        public decimal UnitPrice { get; set; }

        public decimal Discount { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal Total { get; set; }

        // Dropdown
        public IEnumerable<SelectListItem> Products { get; set; }
            = Enumerable.Empty<SelectListItem>();
    }
}
