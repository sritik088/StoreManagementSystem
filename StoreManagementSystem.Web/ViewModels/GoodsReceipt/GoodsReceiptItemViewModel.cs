using System.ComponentModel.DataAnnotations;

namespace StoreManagementSystem.Web.ViewModels.GoodsReceipt
{
    public class GoodsReceiptItemViewModel
    {
        public int Id { get; set; }

        public int? PurchaseOrderItemId { get; set; }

        [Required]
        public int ProductId { get; set; }

        public string? ProductName { get; set; }

        public string? SKU { get; set; }

        public decimal OrderedQuantity { get; set; }

        public decimal PreviouslyReceivedQuantity { get; set; }

        [Range(0, 999999)]
        public decimal ReceivedQuantity { get; set; }

        public decimal RemainingQuantity { get; set; }

        [Required]
        public int WarehouseId { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal Discount { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal Total { get; set; }
    }
}