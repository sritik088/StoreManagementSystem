namespace StoreManagementSystem.Web.ViewModels.Reports
{
    public class SupplierReportViewModel
    {
        // =====================================================
        // SYSTEM / REPORT SETTINGS
        // =====================================================

        public string OrganisationName { get; set; } = string.Empty;

        public string Branch { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string GSTIN { get; set; } = string.Empty;

        public string ReportHeading { get; set; } = "Supplier Report";

        public string FromDateLabel { get; set; } = "From Date";

        public string ToDateLabel { get; set; } = "To Date";

        public string AuthorisedSignatory { get; set; } = string.Empty;

        public string AuthorisedDesignation { get; set; } = string.Empty;


        // =====================================================
        // SUPPLIER SUMMARY
        // =====================================================

        public int TotalSuppliers { get; set; }

        public decimal TotalPurchase { get; set; }

        public decimal TotalPaid { get; set; }

        public decimal TotalOutstanding { get; set; }
    }
}