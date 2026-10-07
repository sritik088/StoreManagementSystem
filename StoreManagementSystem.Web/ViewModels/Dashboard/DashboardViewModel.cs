using System;
using System.Collections.Generic;

namespace StoreManagementSystem.Web.ViewModels.Dashboard
{
    public class DashboardViewModel
    {
        // =========================================================
        // FILTERS
        // =========================================================

        public int? MainCategoryFilterId { get; set; }

        public string? PurchaseOrderStatusFilter { get; set; }

        public List<DashboardMainCategoryFilterViewModel> MainCategories { get; set; }
            = new();

        public List<DashboardPurchaseStatusFilterViewModel> PurchaseOrderStatuses { get; set; }
            = new();


        // =========================================================
        // BASIC COUNTS
        // =========================================================

        public int TotalProducts { get; set; }

        public int TotalCategories { get; set; }

        public int TotalSubCategories { get; set; }

        public int TotalSuppliers { get; set; }

        public int TotalCustomers { get; set; }

        public int TotalWarehouses { get; set; }


        // =========================================================
        // SALES
        // =========================================================

        public int TotalSalesOrders { get; set; }

        public decimal TodaySales { get; set; }

        public decimal MonthlySales { get; set; }

        public decimal TotalRevenue { get; set; }


        // =========================================================
        // PURCHASE
        // =========================================================

        public int TotalPurchaseOrders { get; set; }

        public decimal TotalPurchaseAmount { get; set; }


        // =========================================================
        // INVENTORY
        // =========================================================

        public decimal TotalStockQuantity { get; set; }

        public decimal AvailableStockQuantity { get; set; }

        public decimal ReservedStockQuantity { get; set; }

        public decimal InventoryValue { get; set; }

        public int InStockProducts { get; set; }

        public int LowStockProducts { get; set; }

        public int OutOfStockProducts { get; set; }


        // =========================================================
        // PURCHASE ORDER STATUS
        // =========================================================

        public int DraftPurchaseOrders { get; set; }

        public int ApprovedPurchaseOrders { get; set; }

        public int PartiallyReceivedPurchaseOrders { get; set; }

        public int FullyReceivedPurchaseOrders { get; set; }

        public int CancelledPurchaseOrders { get; set; }

        public decimal TotalPurchaseOrderValue { get; set; }

        public decimal PendingPurchaseOrderValue { get; set; }


        // =========================================================
        // SALES ORDER STATUS
        // =========================================================

        public int DraftSalesOrders { get; set; }

        public int ConfirmedSalesOrders { get; set; }

        public int ProcessingSalesOrders { get; set; }

        public int DeliveredSalesOrders { get; set; }

        public int CancelledSalesOrders { get; set; }

        public decimal TotalSalesOrderValue { get; set; }

        public decimal PendingSalesOrderValue { get; set; }


        // =========================================================
        // STOCK TRANSFER
        // =========================================================

        public int TotalStockTransfers { get; set; }

        public decimal TransferInQuantity { get; set; }

        public decimal TransferOutQuantity { get; set; }

        public decimal TotalTransferredQuantity { get; set; }


        // =========================================================
        // MAIN CATEGORY ANALYTICS
        // =========================================================

        public List<MainCategoryBusinessAnalyticsViewModel> MainCategoryAnalytics { get; set; }
            = new();


        // =========================================================
        // LIVE STOCK
        // =========================================================

        public List<DashboardLiveStockViewModel> LiveStock { get; set; }
            = new();
    }


    // =============================================================
    // MAIN CATEGORY FILTER
    // =============================================================

    public class DashboardMainCategoryFilterViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }


    // =============================================================
    // PURCHASE ORDER STATUS FILTER
    // =============================================================

    public class DashboardPurchaseStatusFilterViewModel
    {
        public string Value { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
    }


    // =============================================================
    // MAIN CATEGORY BUSINESS ANALYTICS
    // =============================================================

    public class MainCategoryBusinessAnalyticsViewModel
    {
        public int MainCategoryId { get; set; }

        public string MainCategoryName { get; set; } = "Uncategorized";

        public decimal OpeningBalance { get; set; }

        public decimal PurchaseBalance { get; set; }

        public decimal SaleBalance { get; set; }

        public decimal Damage { get; set; }

        public decimal StockBalance { get; set; }

        public decimal StockValue { get; set; }

        public decimal SalesValue { get; set; }

        public decimal PurchaseValue { get; set; }

        public decimal PercentageOfTotal { get; set; }


        public decimal TotalAvailableBeforeOutflow =>
            OpeningBalance + PurchaseBalance;

        public decimal TotalOutflow =>
            SaleBalance + Damage;

        public decimal CalculatedClosingBalance =>
            OpeningBalance +
            PurchaseBalance -
            SaleBalance -
            Damage;

        public decimal StockDifference =>
            StockBalance -
            CalculatedClosingBalance;
    }


    // =============================================================
    // LIVE STOCK
    // =============================================================

    public class DashboardLiveStockViewModel
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public string SKU { get; set; } = string.Empty;

        public string Barcode { get; set; } = string.Empty;

        public string MainCategoryName { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;

        public string WarehouseName { get; set; } = string.Empty;

        public decimal QuantityOnHand { get; set; }

        public decimal AvailableQuantity { get; set; }

        public decimal ReservedQuantity { get; set; }

        public decimal MinimumStock { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}