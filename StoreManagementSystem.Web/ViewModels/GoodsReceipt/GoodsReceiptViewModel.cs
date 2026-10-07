using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using StoreManagementSystem.Domain.Enums;

namespace StoreManagementSystem.Web.ViewModels.GoodsReceipt
{
    public class GoodsReceiptViewModel
    {
        public int Id { get; set; }

        public string? GRNNumber { get; set; }

        // =====================================================
        // RECEIPT TYPE
        // =====================================================

        public GoodsReceiptType ReceiptType { get; set; }
            = GoodsReceiptType.Purchase;

        // =====================================================
        // PURCHASE
        // =====================================================

        [Required(ErrorMessage = "Please select a Purchase Order.")]
        public int PurchaseOrderId { get; set; }

        public int SupplierId { get; set; }

        public string? SupplierName { get; set; }

        // =====================================================
        // GIFT
        // =====================================================

        [StringLength(200)]
        public string? DonorName { get; set; }

        [StringLength(500)]
        public string? GiftReason { get; set; }

        // =====================================================
        // COMMON
        // =====================================================

        [Required(ErrorMessage = "Please select receipt date.")]
        [DataType(DataType.Date)]
        public DateTime ReceiptDate { get; set; }
            = DateTime.Today;

        public decimal SubTotal { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal Discount { get; set; }

        public decimal GrandTotal { get; set; }

        public string? Remarks { get; set; }

        // =====================================================
        // DROPDOWNS
        // =====================================================

        public IEnumerable<SelectListItem> PurchaseOrders { get; set; }
            = Enumerable.Empty<SelectListItem>();

        public IEnumerable<SelectListItem> Warehouses { get; set; }
            = Enumerable.Empty<SelectListItem>();

        public IEnumerable<SelectListItem> Products { get; set; }
            = Enumerable.Empty<SelectListItem>();

        // =====================================================
        // GRN ITEMS
        // =====================================================

        public List<GoodsReceiptItemViewModel> Items { get; set; }
            = new List<GoodsReceiptItemViewModel>();
    }
}