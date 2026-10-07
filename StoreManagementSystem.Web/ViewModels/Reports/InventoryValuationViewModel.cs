
namespace StoreManagementSystem.Web.ViewModels.Reports
{
    public class InventoryValuationViewModel
    {
        // =========================================================
        // ORGANISATION / REPORT SETTINGS
        // =========================================================

        public string OrganisationName { get; set; } = string.Empty;

        public string Branch { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string GSTIN { get; set; } = string.Empty;

        public string ReportHeading { get; set; } =
            "Inventory Valuation Report";

        public string FromDateLabel { get; set; } =
            "From Date";

        public string ToDateLabel { get; set; } =
            "To Date";

        public string AuthorisedSignatory { get; set; } =
            string.Empty;

        public string AuthorisedDesignation { get; set; } =
            string.Empty;


        // =========================================================
        // FILTERS
        // =========================================================

        public int? MainCategoryId { get; set; }

        public int? CategoryId { get; set; }

        public int? ProductId { get; set; }

        public int? WarehouseId { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }


        // =========================================================
        // DROPDOWN DATA
        // =========================================================

        public List<InventoryMainCategoryFilterViewModel> MainCategories
        {
            get;
            set;
        } = new();

        public List<InventoryCategoryFilterViewModel> Categories
        {
            get;
            set;
        } = new();

        public List<InventoryProductFilterViewModel> Products
        {
            get;
            set;
        } = new();

        public List<InventoryWarehouseFilterViewModel> Warehouses
        {
            get;
            set;
        } = new();


        // =========================================================
        // SUMMARY
        // =========================================================

        public int TotalProducts { get; set; }

        public decimal TotalQuantity { get; set; }

        public decimal TotalInventoryValue { get; set; }

        public decimal TotalPurchaseCost { get; set; }


        // =========================================================
        // REPORT ITEMS
        // =========================================================

        public List<InventoryValuationItemViewModel> Items
        {
            get;
            set;
        } = new();
    }


    // =========================================================
    // MAIN CATEGORY DROPDOWN
    // =========================================================

    public class InventoryMainCategoryFilterViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }


    // =========================================================
    // CATEGORY DROPDOWN
    // =========================================================

    public class InventoryCategoryFilterViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int MainCategoryId { get; set; }
    }


    // =========================================================
    // PRODUCT DROPDOWN
    // =========================================================

    public class InventoryProductFilterViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }


    // =========================================================
    // WAREHOUSE DROPDOWN
    // =========================================================

    public class InventoryWarehouseFilterViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }


    // =========================================================
    // INVENTORY TABLE ITEM
    // =========================================================

    public class InventoryValuationItemViewModel
    {
        // =========================================================
        // BASIC
        // =========================================================

        public int Id { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public string SKU { get; set; } = string.Empty;

        public string? Barcode { get; set; }


        // =========================================================
        // MAIN CATEGORY
        // =========================================================

        public int MainCategoryId { get; set; }

        public string MainCategoryName { get; set; } = string.Empty;


        // =========================================================
        // CATEGORY
        // =========================================================

        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;


        // =========================================================
        // SUBCATEGORY
        // =========================================================

        public int SubCategoryId { get; set; }

        public string SubCategoryName { get; set; } = string.Empty;


        // =========================================================
        // WAREHOUSE
        // =========================================================

        public int WarehouseId { get; set; }

        public string WarehouseName { get; set; } = string.Empty;


        // =========================================================
        // GRN DATE/TIME
        // =========================================================

        public DateTime? ReceiptDate { get; set; }


        // =========================================================
        // INVENTORY
        // =========================================================

        public decimal QuantityOnHand { get; set; }

        public decimal PurchasePrice { get; set; }

        public decimal InventoryValue { get; set; }
    }
}

