namespace StoreManagementSystem.Application.ViewModels
{
    public class SystemSettingViewModel
    {
        // =========================================================
        // ORGANISATION
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

        public string ReportHeading { get; set; } = string.Empty;

        public string FromDateLabel { get; set; } = "From";

        public string ToDateLabel { get; set; } = "To";


        // =========================================================
        // AUTHORISATION / SIGNATURE
        // =========================================================

        public string AuthorisedSignatory { get; set; } = string.Empty;

        public string AuthorisedDesignation { get; set; } = string.Empty;


        // =========================================================
        // SALES
        // =========================================================

        public bool SalesOrderEnabled { get; set; }

        public string SalesOrderLabel { get; set; } = "Sale";

        public string SalesOrderPluralLabel { get; set; } = "Sales";


        // =========================================================
        // LOCATION
        // =========================================================

        public string LocationLabel { get; set; } = "Warehouse";

        public string LocationPluralLabel { get; set; } = "Warehouses";

        public bool CustomersEnabled { get; set; }
    }
}