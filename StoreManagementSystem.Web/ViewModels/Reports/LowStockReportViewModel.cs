using Microsoft.AspNetCore.Mvc.Rendering;

namespace StoreManagementSystem.Web.ViewModels.Reports
{
    public class LowStockReportViewModel
    {
        // =========================================================
        // FILTERS
        // =========================================================

        public int? ProductId { get; set; }

        public int? WarehouseId { get; set; }

        public int? CategoryId { get; set; }


        // =========================================================
        // DROPDOWNS
        // =========================================================

        public List<SelectListItem> Products { get; set; } = new();

        public List<SelectListItem> Warehouses { get; set; } = new();

        public List<SelectListItem> Categories { get; set; } = new();


        // =========================================================
        // SUMMARY
        // =========================================================

        public int TotalLowStockProducts { get; set; }

        public decimal TotalCurrentStock { get; set; }

        public decimal TotalReorderLevel { get; set; }

        public decimal TotalShortageQuantity { get; set; }


        // =========================================================
        // REPORT ITEMS
        // =========================================================

        public List<LowStockItemViewModel> Items { get; set; } = new();


        // =========================================================
        // ORGANISATION DETAILS
        // =========================================================

        public string OrganisationName { get; set; } = string.Empty;

        public string Branch { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string GSTIN { get; set; } = string.Empty;


        // =========================================================
        // REPORT SETTINGS
        // =========================================================

        public string ReportHeading { get; set; } =
            "Low Stock Report";

        public string FromDateLabel { get; set; } =
            "From Date";

        public string ToDateLabel { get; set; } =
            "To Date";


        // =========================================================
        // AUTHORISED SIGNATURE
        // =========================================================

        public string AuthorisedSignatory { get; set; } =
            string.Empty;

        public string AuthorisedDesignation { get; set; } =
            string.Empty;
    }


    // =============================================================
    // LOW STOCK ITEM
    // =============================================================

    public class LowStockItemViewModel
    {
        // =========================================================
        // PRODUCT
        // =========================================================

        public int ProductId { get; set; }

        public string ProductName { get; set; } =
            string.Empty;

        public string SKU { get; set; } =
            string.Empty;

        public string Barcode { get; set; } =
            string.Empty;


        // =========================================================
        // CATEGORY / LOCATION
        // =========================================================

        public string CategoryName { get; set; } =
            string.Empty;

        public string WarehouseName { get; set; } =
            string.Empty;

        public string UnitName { get; set; } =
            string.Empty;


        // =========================================================
        // STOCK
        // =========================================================

        public decimal CurrentStock { get; set; }

        public decimal ReorderLevel { get; set; }

        public decimal ShortageQuantity { get; set; }

        public decimal SuggestedOrderQuantity { get; set; }


        // =========================================================
        // STATUS
        // =========================================================

        public string StockStatus { get; set; } =
            string.Empty;
    }
}