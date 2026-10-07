using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace StoreManagementSystem.Web.ViewModels.StockTransfer
{
    public class StockTransferViewModel
    {
        public int Id { get; set; }

        public string TransferNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Transfer Date")]
        public DateTime TransferDate { get; set; } = DateTime.Today;

        // =========================================================
        // FROM WAREHOUSE
        // =========================================================

        [Required(ErrorMessage = "Please select From Warehouse.")]
        public int FromWarehouseId { get; set; }

        // =========================================================
        // TO WAREHOUSE
        // =========================================================

        [Required(ErrorMessage = "Please select To Warehouse.")]
        public int ToWarehouseId { get; set; }

        // =========================================================
        // REMARKS
        // =========================================================

        public string? Remarks { get; set; }

        // =========================================================
        // WAREHOUSE DROPDOWN
        // =========================================================

        public IEnumerable<SelectListItem> Warehouses { get; set; }
            = new List<SelectListItem>();

        // =========================================================
        // TRANSFER ITEMS
        // IMPORTANT: get; set;
        // =========================================================

        public List<StockTransferItemViewModel> Items { get; set; }
            = new List<StockTransferItemViewModel>();

        // =========================================================
        // GRAND TOTAL
        // =========================================================

        public decimal TotalAmount
        {
            get
            {
                return Items?.Sum(x => x.Total) ?? 0m;
            }
        }
    }
}