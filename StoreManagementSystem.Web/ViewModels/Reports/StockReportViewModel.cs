
namespace StoreManagementSystem.Web.ViewModels.Reports
{
    public class StockReportViewModel
    {
        // ==========================================
        // SYSTEM / REPORT SETTINGS
        // ==========================================

        public string OrganisationName { get; set; } = string.Empty;

        public string Branch { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string GSTIN { get; set; } = string.Empty;

        public string ReportHeading { get; set; } = "Stock Report";

        public string FromDateLabel { get; set; } = "From Date";

        public string ToDateLabel { get; set; } = "To Date";

        public string AuthorisedSignatory { get; set; } = string.Empty;

        public string AuthorisedDesignation { get; set; } = string.Empty;


        // ==========================================
        // FILTERS
        // ==========================================

        public int? ProductId { get; set; }

        public int? WarehouseId { get; set; }

        public string? StockStatus { get; set; }


        // ==========================================
        // SUMMARY
        // ==========================================

        public int TotalProducts { get; set; }

        public decimal TotalQuantity { get; set; }

        public decimal TotalStockValue { get; set; }

        public int LowStockProducts { get; set; }

        public int OutOfStockProducts { get; set; }


        // ==========================================
        // STOCK LIST
        // ==========================================

        public List<StockReportItemViewModel> Stocks { get; set; }
            = new List<StockReportItemViewModel>();
    }


    public class StockReportItemViewModel
    {
        public int Id { get; set; }


        // ==========================================
        // PRODUCT
        // ==========================================

        public int ProductId { get; set; }

        public string ProductName { get; set; }
            = string.Empty;

        public string SKU { get; set; }
            = string.Empty;

        public string? Barcode { get; set; }


        // ==========================================
        // WAREHOUSE
        // ==========================================

        public int WarehouseId { get; set; }

        public string WarehouseName { get; set; }
            = string.Empty;


        // ==========================================
        // STOCK
        // ==========================================

        public decimal OpeningStock { get; set; }

        public decimal ReceivedQuantity { get; set; }

        public decimal SoldQuantity { get; set; }

        public decimal QuantityOnHand { get; set; }

        public decimal ReorderLevel { get; set; }


        // ==========================================
        // PRICING
        // ==========================================

        public decimal PurchasePrice { get; set; }

        public decimal StockValue { get; set; }


        // ==========================================
        // STATUS
        // ==========================================

        public string StockStatus { get; set; }
            = string.Empty;
    }
}

