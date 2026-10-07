using Microsoft.AspNetCore.Mvc.Rendering;
using StoreManagementSystem.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace StoreManagementSystem.Web.ViewModels.PurchaseOrder
{
    public class PurchaseOrderViewModel
    {
        public int Id { get; set; }

        // ============================================================
        // PURCHASE ORDER
        // ============================================================

        public string? PONumber { get; set; }

        // ============================================================
        // SUPPLIER
        // ============================================================

        [Required(ErrorMessage = "Please select a supplier.")]
        public int SupplierId { get; set; }

        public List<SelectListItem> Suppliers { get; set; }
            = new List<SelectListItem>();

        // ============================================================
        // PRODUCTS
        // ============================================================

        public List<SelectListItem> Products { get; set; }
            = new List<SelectListItem>();

        // ============================================================
        // DATES
        // ============================================================

        [Required]
        [DataType(DataType.Date)]
        public DateTime OrderDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime ExpectedDate { get; set; }

        // ============================================================
        // STATUS
        // ============================================================

        public string Status { get; set; }
            = PurchaseOrderStatus.Draft.ToString();

        // ============================================================
        // ITEMS
        // ============================================================

        [MinLength(1, ErrorMessage = "At least one product is required.")]
        public List<PurchaseOrderItemViewModel> Items { get; set; }
            = new List<PurchaseOrderItemViewModel>();

        // ============================================================
        // TOTALS
        // ============================================================

        public decimal SubTotal { get; set; }

        public decimal Discount { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal GrandTotal { get; set; }

        // ============================================================
        // REMARKS
        // ============================================================

        public string? Remarks { get; set; }
    }
}