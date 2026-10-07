using Microsoft.AspNetCore.Mvc.Rendering;

using StoreManagementSystem.Domain.Enums;

namespace StoreManagementSystem.Web.ViewModels.SalesOrder
{
    public class SalesOrderViewModel
    {
        public int Id { get; set; }

        // =========================================================
        // HEADER
        // =========================================================

        public string OrderNumber { get; set; } = string.Empty;

        public DateTime OrderDate { get; set; }
            = DateTime.Today;

        public int WarehouseId { get; set; }

        public string CustomerName { get; set; }
            = string.Empty;

        public string CustomerPhone { get; set; }
            = string.Empty;

        public string Remarks { get; set; }
            = string.Empty;

        // =========================================================
        // STATUS
        // =========================================================

        public SalesOrderStatus Status { get; set; }
            = SalesOrderStatus.Draft;

        // =========================================================
        // TOTALS
        // =========================================================

        public decimal SubTotal { get; set; }

        public decimal DiscountAmount { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal GrandTotal { get; set; }

        public decimal GrossProfit { get; set; }

        // =========================================================
        // ITEMS
        // =========================================================

        public List<SalesOrderItemViewModel> Items { get; set; }
            = new List<SalesOrderItemViewModel>();

        // =========================================================
        // WAREHOUSES
        // =========================================================

        public List<SelectListItem> Warehouses { get; set; }
            = new List<SelectListItem>();
    }
}