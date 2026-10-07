
using StoreManagementSystem.Domain.Enums;

namespace StoreManagementSystem.Web.ViewModels.StockLedger
{
    // ============================================================
    // INDIVIDUAL STOCK LEDGER ROW
    // ============================================================

    public class StockLedgerViewModel
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public string MainCategoryName { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;

        public string SubCategoryName { get; set; } = string.Empty;

        public int WarehouseId { get; set; }

        public string WarehouseName { get; set; } = string.Empty;

        public StockTransactionType TransactionType { get; set; }

        public decimal Quantity { get; set; }

        public decimal BalanceAfterTransaction { get; set; }

        public decimal UnitCost { get; set; }

        public string ReferenceNo { get; set; } = string.Empty;

        public DateTime TransactionDate { get; set; }

        public decimal TotalValue =>
            Math.Abs(Quantity) * UnitCost;
    }


    // ============================================================
    // WAREHOUSE GROUP
    // ============================================================

    public class StockLedgerWarehouseGroupViewModel
    {
        public int WarehouseId { get; set; }

        public string WarehouseName { get; set; } = string.Empty;

        public List<StockLedgerViewModel> Products { get; set; } = new();

        public int ProductCount =>
            Products.Count;

        public decimal ClosingBalance =>
            Products.Sum(x => x.BalanceAfterTransaction);
    }


    // ============================================================
    // MAIN PAGE VIEW MODEL
    // ============================================================

    public class StockLedgerIndexViewModel
    {
        // --------------------------------------------------------
        // SELECTED DATE
        // --------------------------------------------------------

        public DateTime SelectedDate { get; set; }


        // --------------------------------------------------------
        // WAREHOUSE GROUPS
        // --------------------------------------------------------

        public List<StockLedgerWarehouseGroupViewModel> Warehouses
        {
            get;
            set;
        } = new();


        // --------------------------------------------------------
        // SUMMARY
        // --------------------------------------------------------

        public int TotalWarehouses =>
            Warehouses.Count;

        public int TotalProducts =>
            Warehouses.Sum(x => x.Products.Count);

        public decimal TotalClosingQuantity =>
            Warehouses.Sum(x => x.ClosingBalance);

        public decimal TotalClosingValue =>
            Warehouses
                .SelectMany(x => x.Products)
                .Sum(x =>
                    x.BalanceAfterTransaction *
                    x.UnitCost);
    }
}
