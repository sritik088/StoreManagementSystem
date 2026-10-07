using System;
using System.Collections.Generic;

namespace StoreManagementSystem.Web.ViewModels.Analytics
{
    public class AnalyticsDashboardViewModel
    {
        // =========================================================
        // DATE RANGE
        // =========================================================

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }


        // =========================================================
        // MAIN FINANCIAL KPIs
        // =========================================================

        public decimal NetSales { get; set; }

        public decimal Purchases { get; set; }

        public decimal CostOfGoodsSold { get; set; }

        public decimal GrossProfit { get; set; }

        public decimal GrossMarginPercent { get; set; }

        public decimal InventoryValue { get; set; }

        public int TotalOrders { get; set; }

        public decimal AverageOrderValue { get; set; }


        // =========================================================
        // PREVIOUS PERIOD
        // =========================================================
        // Kept for existing comparison/insight logic.
        // These are NOT required by the four KPI doughnut charts.
        // =========================================================

        public decimal PreviousNetSales { get; set; }

        public decimal PreviousPurchases { get; set; }

        public decimal PreviousGrossProfit { get; set; }

        public int PreviousOrders { get; set; }


        // =========================================================
        // FOUR KPI DOUGHNUT CHART DATA
        // =========================================================
        //
        // Main Category-wise:
        //
        // 1. Net Sales
        // 2. Purchases
        // 3. Gross Profit
        // 4. Inventory Value
        //
        // =========================================================

        public List<string> CategoryChartLabels { get; set; } = new();

        public List<decimal> CategorySalesValues { get; set; } = new();

        public List<decimal> CategoryPurchaseValues { get; set; } = new();

        public List<decimal> CategoryGrossProfitValues { get; set; } = new();

        public List<decimal> CategoryInventoryValues { get; set; } = new();

        public List<decimal> CategoryCOGSValues { get; set; } = new();


        // =========================================================
        // LEGACY FINANCIAL PIE
        // =========================================================
        //
        // Kept only for compatibility with older code.
        // DO NOT display this chart in the new dashboard.
        //
        // =========================================================

        public List<string> FinancialPieLabels { get; set; } = new();

        public List<decimal> FinancialPieValues { get; set; } = new();


        // =========================================================
        // COMPATIBILITY ALIASES
        // =========================================================

        public decimal TotalSales
        {
            get => NetSales;
            set => NetSales = value;
        }

        public decimal TotalPurchases
        {
            get => Purchases;
            set => Purchases = value;
        }

        public decimal TotalCOGS
        {
            get => CostOfGoodsSold;
            set => CostOfGoodsSold = value;
        }

        public decimal GrossMargin
        {
            get => GrossMarginPercent;
            set => GrossMarginPercent = value;
        }

        public decimal TotalDamageValue { get; set; }


        // =========================================================
        // STOCK SUMMARY
        // =========================================================

        public decimal TotalStockQuantity { get; set; }

        public decimal ReservedStockQuantity { get; set; }

        public decimal TotalAvailableQuantity =>
            TotalStockQuantity - ReservedStockQuantity;

        public decimal TotalReservedQuantity
        {
            get => ReservedStockQuantity;
            set => ReservedStockQuantity = value;
        }

        public int InStockProducts { get; set; }

        public int LowStockProducts { get; set; }

        public int OutOfStockProducts { get; set; }


        // =========================================================
        // KPI CHANGES
        // =========================================================

        public decimal SalesChangePercent { get; set; }

        public decimal PurchaseChangePercent { get; set; }

        public decimal ProfitChangePercent { get; set; }

        public decimal OrdersChangePercent { get; set; }


        public decimal SalesChange
        {
            get => SalesChangePercent;
            set => SalesChangePercent = value;
        }

        public decimal PurchaseChange
        {
            get => PurchaseChangePercent;
            set => PurchaseChangePercent = value;
        }

        public decimal ProfitChange
        {
            get => ProfitChangePercent;
            set => ProfitChangePercent = value;
        }

        public decimal OrdersChange
        {
            get => OrdersChangePercent;
            set => OrdersChangePercent = value;
        }


        // =========================================================
        // STOCK MOVEMENTS
        // =========================================================

        public decimal PurchaseReceiptQuantity { get; set; }

        public decimal SalesIssueQuantity { get; set; }

        public decimal SalesReturnQuantity { get; set; }

        public decimal PurchaseReturnQuantity { get; set; }

        public decimal TransferInQuantity { get; set; }

        public decimal TransferOutQuantity { get; set; }

        public decimal AdjustmentQuantity { get; set; }


        // =========================================================
        // DAMAGE
        // =========================================================

        public decimal DamageQuantity { get; set; }

        public decimal DamageValue
        {
            get => TotalDamageValue;
            set => TotalDamageValue = value;
        }


        // =========================================================
        // DAILY / TREND DATA
        // =========================================================

        public List<string> DailySalesLabels { get; set; } = new();

        public List<decimal> DailySales { get; set; } = new();

        public List<string> DailyPurchaseLabels { get; set; } = new();

        public List<decimal> DailyPurchases { get; set; } = new();

        public List<string> DailyProfitLabels { get; set; } = new();

        public List<decimal> DailyProfit { get; set; } = new();


        // =========================================================
        // BUSINESS PERFORMANCE TREND
        // =========================================================

        public List<string> SalesTrendLabels { get; set; } = new();

        public List<decimal> SalesTrendValues { get; set; } = new();

        public List<string> PurchaseTrendLabels { get; set; } = new();

        public List<decimal> PurchaseTrendValues { get; set; } = new();

        public List<string> ProfitTrendLabels { get; set; } = new();

        public List<decimal> ProfitTrendValues { get; set; } = new();


        // =========================================================
        // MAIN CATEGORY ANALYTICS
        // =========================================================

        public List<CategoryAnalyticsRowViewModel> CategoryAnalytics
        {
            get;
            set;
        } = new();


        // =========================================================
        // INVENTORY VALUE BY CATEGORY
        // =========================================================

        public List<string> InventoryValueByCategoryLabels
        {
            get;
            set;
        } = new();

        public List<decimal> InventoryValueByCategoryValues
        {
            get;
            set;
        } = new();


        // =========================================================
        // WAREHOUSE ANALYTICS
        // =========================================================

        public List<WarehouseAnalyticsRowViewModel> Warehouses
        {
            get;
            set;
        } = new();


        public List<WarehouseAnalyticsRowViewModel> WarehouseAnalytics
        {
            get => Warehouses;

            set =>
                Warehouses =
                    value ??
                    new List<WarehouseAnalyticsRowViewModel>();
        }


        // =========================================================
        // INVENTORY VALUE BY WAREHOUSE
        // =========================================================
        //
        // Current warehouse inventory value.
        //
        // WarehouseStock.QuantityOnHand
        //              ×
        // Historical weighted average purchase cost
        //
        // =========================================================

        public List<string> WarehouseLabels
        {
            get;
            set;
        } = new();

        public List<decimal> WarehouseStockValues
        {
            get;
            set;
        } = new();


        // =========================================================
        // INSIGHTS
        // =========================================================

        public List<AnalyticsInsightViewModel> Insights
        {
            get;
            set;
        } = new();
    }


    // =============================================================
    // MAIN CATEGORY ANALYTICS
    // =============================================================

    public class CategoryAnalyticsRowViewModel
    {
        public string MainCategoryName { get; set; }
            = "Uncategorized";

        public string CategoryName { get; set; }
            = "Uncategorized";


        // =========================================================
        // QUANTITIES
        // =========================================================

        public decimal OpeningBalance { get; set; }

        public decimal PurchaseBalance { get; set; }

        public decimal SaleBalance { get; set; }

        public decimal Damage { get; set; }

        public decimal StockBalance { get; set; }


        // =========================================================
        // FINANCIAL VALUES
        // =========================================================

        public decimal StockValue { get; set; }

        public decimal SalesValue { get; set; }

        public decimal PurchaseValue { get; set; }

        public decimal COGS { get; set; }

        public decimal GrossProfit { get; set; }

        public decimal PercentageOfTotal { get; set; }


        // =========================================================
        // COMPATIBILITY
        // =========================================================

        public decimal OpeningStock
        {
            get => OpeningBalance;
            set => OpeningBalance = value;
        }

        public decimal Purchases
        {
            get => PurchaseBalance;
            set => PurchaseBalance = value;
        }

        public decimal Sales
        {
            get => SaleBalance;
            set => SaleBalance = value;
        }


        // =========================================================
        // CALCULATED STOCK
        // =========================================================

        public decimal ClosingStock =>
            CalculatedClosingBalance;

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
    // WAREHOUSE ANALYTICS
    // =============================================================

    public class WarehouseAnalyticsRowViewModel
    {
        public int WarehouseId { get; set; }

        public string WarehouseName { get; set; } =
            string.Empty;

        public decimal StockQuantity { get; set; }

        public decimal StockValue { get; set; }

        public decimal SalesValue { get; set; }

        public decimal PurchaseValue { get; set; }


        // =========================================================
        // COMPATIBILITY ALIASES
        // =========================================================

        public decimal Sales
        {
            get => SalesValue;
            set => SalesValue = value;
        }

        public decimal Purchases
        {
            get => PurchaseValue;
            set => PurchaseValue = value;
        }
    }


    // =============================================================
    // INSIGHT
    // =============================================================

    public class AnalyticsInsightViewModel
    {
        public string Title { get; set; } =
            string.Empty;

        public string Description { get; set; } =
            string.Empty;

        public string Type { get; set; } =
            "info";

        public string Icon { get; set; } =
            "bi-info-circle";
    }
}