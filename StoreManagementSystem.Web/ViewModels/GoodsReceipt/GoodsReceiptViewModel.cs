using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace StoreManagementSystem.Web.ViewModels.GoodsReceipt
{
    public class GoodsReceiptItemViewModel
    {
        public int Id { get; set; }

        public int PurchaseOrderItemId { get; set; }

        [Required]
        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public string SKU { get; set; } = string.Empty;

        public string UnitName { get; set; } = string.Empty;

        public decimal OrderedQuantity { get; set; }

        public decimal PreviouslyReceivedQuantity { get; set; }

        public decimal RemainingQuantity =>
            OrderedQuantity - PreviouslyReceivedQuantity;

        [Range(0.01, 999999)]
        public decimal ReceivedQuantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal Discount { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal LineTotal { get; set; }

        [Required]
        public int WarehouseId { get; set; }

        public IEnumerable<SelectListItem> Warehouses
            = Enumerable.Empty<SelectListItem>();
    }
}
