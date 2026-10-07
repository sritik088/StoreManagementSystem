namespace StoreManagementSystem.Web.ViewModels.Reports
{
    public class ProfitLossViewModel
    {
        // =========================================================
        // SYSTEM / REPORT SETTINGS
        // =========================================================

        public string OrganisationName { get; set; } = string.Empty;

        public string Branch { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string GSTIN { get; set; } = string.Empty;

        public string ReportHeading { get; set; }
            = "Profit & Loss Report";

        public string FromDateLabel { get; set; }
            = "From Date";

        public string ToDateLabel { get; set; }
            = "To Date";

        public string AuthorisedSignatory { get; set; }
            = string.Empty;

        public string AuthorisedDesignation { get; set; }
            = string.Empty;


        // =========================================================
        // PROFIT & LOSS VALUES
        // =========================================================

        public decimal TotalSales { get; set; }

        public decimal CostOfGoodsSold { get; set; }

        public decimal GrossProfit { get; set; }

        public decimal Expenses { get; set; }

        public decimal NetProfit { get; set; }


        // =========================================================
        // REPORT DATE FILTERS
        // =========================================================

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }
    }
}