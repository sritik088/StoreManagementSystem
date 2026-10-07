using System.ComponentModel.DataAnnotations;

namespace StoreManagementSystem.Application.ViewModels
{
    public class WarehouseStockViewModel
    {
        public int Id { get; set; }


        // ==========================================
        // WAREHOUSE
        // ==========================================

        public int WarehouseId { get; set; }

        [Display(Name = "Warehouse")]
        public string WarehouseName { get; set; } = string.Empty;


        // ==========================================
        // PRODUCT
        // ==========================================

        [Display(Name = "SKU")]
        public string SKU { get; set; } = string.Empty;

        [Display(Name = "Barcode")]
        public string Barcode { get; set; } = string.Empty;


        // ==========================================
        // CLOSING BALANCE
        // ==========================================

        [Display(Name = "Closing Balance")]
        public decimal ClosingBalance { get; set; }


        // ==========================================
        // STOCK LIMITS
        // These are NOT displayed in the table.
        // They are only used for Status.
        // ==========================================

        public decimal MinimumStock { get; set; }

        public decimal MaximumStock { get; set; }


        // ==========================================
        // STATUS
        // ==========================================

        public string StockStatus
        {
            get
            {
                if (ClosingBalance <= 0)
                    return "Out of Stock";

                if (ClosingBalance <= MinimumStock)
                    return "Low Stock";

                if (MaximumStock > 0 &&
                    ClosingBalance >= MaximumStock)
                    return "Over Stock";

                return "In Stock";
            }
        }


        // ==========================================
        // STATUS CSS CLASS
        // ==========================================

        public string StockStatusClass
        {
            get
            {
                return StockStatus switch
                {
                    "Out of Stock" =>
                        "bg-danger",

                    "Low Stock" =>
                        "bg-warning text-dark",

                    "Over Stock" =>
                        "bg-info text-dark",

                    _ =>
                        "bg-success"
                };
            }
        }
    }
}