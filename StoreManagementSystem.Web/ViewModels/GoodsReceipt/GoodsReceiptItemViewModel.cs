using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace StoreManagementSystem.Web.ViewModels.GoodsReceipt
{
    public class GoodsReceiptViewModel
    {
        public int Id { get; set; }

        public string GRNNumber { get; set; } = "";

        [Required]
        public int PurchaseOrderId { get; set; }

        public string PONumber { get; set; } = "";

        public int SupplierId { get; set; }

        public string SupplierName { get; set; } = "";

        public DateTime ReceiptDate { get; set; }
            = DateTime.Today;

        public decimal SubTotal { get; set; }

        public decimal Discount { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal GrandTotal { get; set; }

        public string? Remarks { get; set; }

        public IEnumerable<SelectListItem> PurchaseOrders
            = Enumerable.Empty<SelectListItem>();

        public List<GoodsReceiptItemViewModel> Items
            = new();
    }
}
