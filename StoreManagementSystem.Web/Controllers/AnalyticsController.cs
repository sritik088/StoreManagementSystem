using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Domain.Enums;
using StoreManagementSystem.Infrastructure.Data;
using StoreManagementSystem.Web.ViewModels.Analytics;

namespace StoreManagementSystem.Web.Controllers
{
    public class AnalyticsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ISystemSettingService _settingService;

        public AnalyticsController(
            ApplicationDbContext context,
            ISystemSettingService settingService)
        {
            _context = context;
            _settingService = settingService;
        }

        // ============================================================
        // ANALYTICS DASHBOARD
        // ============================================================

        public async Task<IActionResult> Index(
            DateTime? fromDate,
            DateTime? toDate)
        {
            // ========================================================
            // DATE RANGE
            // ========================================================

            var from = fromDate?.Date
                ?? new DateTime(
                    DateTime.Now.Year,
                    DateTime.Now.Month,
                    1);

            var to = toDate.HasValue
                ? toDate.Value.Date.AddDays(1)
                : DateTime.Now.Date.AddDays(1);

            if (to <= from)
            {
                to = from.AddDays(1);
            }

            // ========================================================
            // SALES SETTINGS
            // ========================================================

            var salesOrderEnabled =
                await _settingService.IsSalesOrderEnabledAsync();

            var salesOrderLabel =
                await _settingService.GetSalesOrderLabelAsync();

            var salesOrderPluralLabel =
                await _settingService.GetSalesOrderPluralLabelAsync();

            if (string.IsNullOrWhiteSpace(salesOrderLabel))
            {
                salesOrderLabel = "Sales Order";
            }

            if (string.IsNullOrWhiteSpace(salesOrderPluralLabel))
            {
                salesOrderPluralLabel = "Sales Orders";
            }

            // ========================================================
            // PREVIOUS PERIOD
            // ========================================================

            var periodDays =
                (to.Date - from.Date).Days;

            if (periodDays <= 0)
            {
                periodDays = 1;
            }

            var previousTo = from;

            var previousFrom =
                from.AddDays(-periodDays);

            // ========================================================
            // CONFIRMED SALES ORDERS
            // ========================================================
            //
            // IMPORTANT:
            //
            // Draft     = excluded
            // Cancelled = excluded
            // Confirmed  = included
            //
            // Your current SalesOrder workflow is:
            //
            // PO -> GRN -> Sale
            //
            // There is NO Delivery module required.
            //
            // ========================================================

            var currentSalesOrders =
                new List<SalesOrder>();

            var previousSalesOrders =
                new List<SalesOrder>();

            if (salesOrderEnabled)
            {
                currentSalesOrders =
                    await _context.SalesOrders
                        .AsNoTracking()
                        .Where(x =>
                            !x.IsDeleted &&
                            x.Status ==
                                SalesOrderStatus.Confirmed &&
                            x.OrderDate >= from &&
                            x.OrderDate < to)
                        .ToListAsync();

                previousSalesOrders =
                    await _context.SalesOrders
                        .AsNoTracking()
                        .Where(x =>
                            !x.IsDeleted &&
                            x.Status ==
                                SalesOrderStatus.Confirmed &&
                            x.OrderDate >= previousFrom &&
                            x.OrderDate < previousTo)
                        .ToListAsync();
            }

            // ========================================================
            // NET SALES
            // ========================================================
            //
            // Net Sales =
            //
            // SubTotal - DiscountAmount
            //
            // Tax is NOT revenue.
            //
            // ========================================================

            var netSales =
                salesOrderEnabled
                    ? currentSalesOrders.Sum(GetNetSales)
                    : 0m;

            var previousNetSales =
                salesOrderEnabled
                    ? previousSalesOrders.Sum(GetNetSales)
                    : 0m;

            // ========================================================
            // TOTAL ORDERS
            // ========================================================

            var totalOrders =
                salesOrderEnabled
                    ? currentSalesOrders.Count
                    : 0;

            var previousOrders =
                salesOrderEnabled
                    ? previousSalesOrders.Count
                    : 0;

            // ========================================================
            // AVERAGE ORDER VALUE
            // ========================================================

            var averageOrderValue =
                totalOrders > 0
                    ? netSales / totalOrders
                    : 0m;

            // ========================================================
            // SALES ORDER ITEMS
            // ========================================================

            var currentSalesItems =
                new List<SalesOrderItem>();

            var previousSalesItems =
                new List<SalesOrderItem>();

            if (salesOrderEnabled)
            {
                // ====================================================
                // CURRENT PERIOD
                // ====================================================

                currentSalesItems =
                    await _context.SalesOrderItems
                        .AsNoTracking()

                        .Include(x =>
                            x.Product)
                            .ThenInclude(x =>
                                x.Category)

                        .Include(x =>
                            x.Product)
                            .ThenInclude(x =>
                                x.MainCategory)

                        .Include(x =>
                            x.SalesOrder)

                        .Where(x =>
                            x.SalesOrder != null &&
                            !x.SalesOrder.IsDeleted &&
                            x.SalesOrder.Status ==
                                SalesOrderStatus.Confirmed &&
                            x.SalesOrder.OrderDate >= from &&
                            x.SalesOrder.OrderDate < to)
                        .ToListAsync();

                // ====================================================
                // PREVIOUS PERIOD
                // ====================================================

                previousSalesItems =
                    await _context.SalesOrderItems
                        .AsNoTracking()

                        .Include(x =>
                            x.Product)
                            .ThenInclude(x =>
                                x.Category)

                        .Include(x =>
                            x.Product)
                            .ThenInclude(x =>
                                x.MainCategory)

                        .Include(x =>
                            x.SalesOrder)

                        .Where(x =>
                            x.SalesOrder != null &&
                            !x.SalesOrder.IsDeleted &&
                            x.SalesOrder.Status ==
                                SalesOrderStatus.Confirmed &&
                            x.SalesOrder.OrderDate >= previousFrom &&
                            x.SalesOrder.OrderDate < previousTo)
                        .ToListAsync();
            }

            // ========================================================
            // GOODS RECEIPTS
            // ========================================================

            var allGoodsReceiptItems =
                await _context.GoodsReceiptItems
                    .AsNoTracking()
                    .Include(x =>
                        x.GoodsReceipt)
                    .Include(x =>
                        x.Product)
                        .ThenInclude(x =>
                            x.Category)
                    .Include(x =>
                        x.Product)
                        .ThenInclude(x =>
                            x.MainCategory)
                    .Include(x =>
                        x.Warehouse)
                    .Where(x =>
                        x.GoodsReceipt != null &&
                        !x.GoodsReceipt.IsDeleted &&
                        x.GoodsReceipt.Status ==
                            GoodsReceiptStatus.Received &&
                        x.ReceivedQuantity > 0)
                    .ToListAsync();

            // ========================================================
            // CURRENT PURCHASES
            // ========================================================

            var currentPurchaseItems =
                allGoodsReceiptItems
                    .Where(x =>
                        x.GoodsReceipt != null &&
                        x.GoodsReceipt.ReceiptDate >= from &&
                        x.GoodsReceipt.ReceiptDate < to &&
                        x.GoodsReceipt.ReceiptType ==
                            GoodsReceiptType.Purchase)
                    .ToList();

            var purchases =
                currentPurchaseItems.Sum(x =>
                    x.ReceivedQuantity *
                    x.UnitPrice);

            // ========================================================
            // PREVIOUS PURCHASES
            // ========================================================

            var previousPurchaseItems =
                allGoodsReceiptItems
                    .Where(x =>
                        x.GoodsReceipt != null &&
                        x.GoodsReceipt.ReceiptDate >= previousFrom &&
                        x.GoodsReceipt.ReceiptDate < previousTo &&
                        x.GoodsReceipt.ReceiptType ==
                            GoodsReceiptType.Purchase)
                    .ToList();

            var previousPurchases =
                previousPurchaseItems.Sum(x =>
                    x.ReceivedQuantity *
                    x.UnitPrice);

            // ========================================================
            // WEIGHTED AVERAGE INVENTORY COST
            // ========================================================

            var purchaseCosts =
                allGoodsReceiptItems
                    .Where(x =>
                        x.GoodsReceipt != null &&
                        !x.GoodsReceipt.IsDeleted &&
                        x.GoodsReceipt.Status ==
                            GoodsReceiptStatus.Received &&
                        x.GoodsReceipt.ReceiptType ==
                            GoodsReceiptType.Purchase &&
                        x.ReceivedQuantity > 0)
                    .GroupBy(x =>
                        x.ProductId)
                    .ToDictionary(
                        g => g.Key,
                        g =>
                        {
                            var quantity =
                                g.Sum(x =>
                                    x.ReceivedQuantity);

                            if (quantity <= 0)
                            {
                                return 0m;
                            }

                            var totalCost =
                                g.Sum(x =>
                                    x.ReceivedQuantity *
                                    x.UnitPrice);

                            return totalCost /
                                   quantity;
                        });

            // ========================================================
            // WAREHOUSE STOCK
            // ========================================================

            var warehouseStocks =
                await _context.WarehouseStocks
                    .AsNoTracking()
                    .Include(x =>
                        x.Product)
                        .ThenInclude(x =>
                            x.Category)
                    .Include(x =>
                        x.Product)
                        .ThenInclude(x =>
                            x.MainCategory)
                    .Include(x =>
                        x.Warehouse)
                    .Where(x =>
                        x.Warehouse != null &&
                        x.Product != null)
                    .ToListAsync();

            // ========================================================
            // INVENTORY VALUE
            // ========================================================

            var inventoryValue =
                warehouseStocks.Sum(x =>
                {
                    if (purchaseCosts.TryGetValue(
                        x.ProductId,
                        out var cost))
                    {
                        return x.QuantityOnHand *
                               cost;
                    }

                    return 0m;
                });

            // ========================================================
            // INVENTORY HEALTH
            // ========================================================

            var inStockProducts =
                warehouseStocks
                    .Where(x =>
                        x.QuantityOnHand > 0)
                    .Select(x =>
                        x.ProductId)
                    .Distinct()
                    .Count();

            var lowStockProducts =
                warehouseStocks
                    .Where(x =>
                        x.QuantityOnHand > 0 &&
                        x.QuantityOnHand <=
                            x.MinimumStock)
                    .Select(x =>
                        x.ProductId)
                    .Distinct()
                    .Count();

            var outOfStockProducts =
                warehouseStocks
                    .Where(x =>
                        x.QuantityOnHand <= 0)
                    .Select(x =>
                        x.ProductId)
                    .Distinct()
                    .Count();

            var totalStockQuantity =
                warehouseStocks.Sum(x =>
                    x.QuantityOnHand);

            var reservedStockQuantity =
                warehouseStocks.Sum(x =>
                    x.ReservedQuantity);

            // ========================================================
            // STOCK LEDGER
            // ========================================================

            var currentStockLedger =
                await _context.StockLedgers
                    .AsNoTracking()
                    .Include(x =>
                        x.Product)
                        .ThenInclude(x =>
                            x.MainCategory)
                    .Where(x =>
                        x.TransactionDate >= from &&
                        x.TransactionDate < to)
                    .ToListAsync();

            var previousStockLedger =
                await _context.StockLedgers
                    .AsNoTracking()
                    .Include(x =>
                        x.Product)
                        .ThenInclude(x =>
                            x.MainCategory)
                    .Where(x =>
                        x.TransactionDate >= previousFrom &&
                        x.TransactionDate < previousTo)
                    .ToListAsync();

            // ========================================================
            // STOCK QUANTITIES
            // ========================================================

            var purchaseReceiptQuantity =
                currentStockLedger
                    .Where(x =>
                        x.TransactionType ==
                            StockTransactionType.PurchaseReceipt)
                    .Sum(x =>
                        Math.Abs(x.Quantity));

            var salesIssueQuantity =
                salesOrderEnabled
                    ? currentStockLedger
                        .Where(x =>
                            x.TransactionType ==
                                StockTransactionType.SalesIssue)
                        .Sum(x =>
                            Math.Abs(x.Quantity))
                    : 0m;

            var salesReturnQuantity =
                salesOrderEnabled
                    ? currentStockLedger
                        .Where(x =>
                            x.TransactionType ==
                                StockTransactionType.SalesReturn)
                        .Sum(x =>
                            Math.Abs(x.Quantity))
                    : 0m;

            var purchaseReturnQuantity =
                currentStockLedger
                    .Where(x =>
                        x.TransactionType ==
                            StockTransactionType.PurchaseReturn)
                    .Sum(x =>
                        Math.Abs(x.Quantity));

            var transferInQuantity =
                currentStockLedger
                    .Where(x =>
                        x.TransactionType ==
                            StockTransactionType.StockTransferIn)
                    .Sum(x =>
                        Math.Abs(x.Quantity));

            var transferOutQuantity =
                currentStockLedger
                    .Where(x =>
                        x.TransactionType ==
                            StockTransactionType.StockTransferOut)
                    .Sum(x =>
                        Math.Abs(x.Quantity));

            var adjustmentQuantity =
                currentStockLedger
                    .Where(x =>
                        x.TransactionType ==
                            StockTransactionType.StockAdjustment)
                    .Sum(x =>
                        Math.Abs(x.Quantity));

            // ========================================================
            // COGS
            // ========================================================
            //
            // For the current Sales Order workflow, Gross Profit is
            // already calculated when the Sale is created/confirmed.
            //
            // SalesOrderItem.UnitPrice = purchase/GRN cost
            // SalesOrderItem.SalePrice = customer selling price
            //
            // Therefore the dashboard Gross Profit comes directly
            // from SalesOrder.GrossProfit.
            //
            // COGS is calculated from confirmed sales items.
            // ========================================================

            var costOfGoodsSold =
                salesOrderEnabled
                    ? currentSalesItems.Sum(x =>
                        x.Quantity *
                        x.UnitPrice)
                    : 0m;

            var previousCOGS =
                salesOrderEnabled
                    ? previousSalesItems.Sum(x =>
                        x.Quantity *
                        x.UnitPrice)
                    : 0m;

            // ========================================================
            // GROSS PROFIT
            // ========================================================
            //
            // IMPORTANT:
            //
            // Do NOT calculate:
            //
            // Net Sales - StockLedger SalesIssue
            //
            // because your current workflow has no Delivery module.
            //
            // Use SalesOrder.GrossProfit directly.
            //
            // ========================================================

            var grossProfit =
                salesOrderEnabled
                    ? currentSalesOrders.Sum(x =>
                        x.GrossProfit)
                    : 0m;

            var previousGrossProfit =
                salesOrderEnabled
                    ? previousSalesOrders.Sum(x =>
                        x.GrossProfit)
                    : 0m;

            // ========================================================
            // GROSS MARGIN
            // ========================================================

            var grossMarginPercent =
                salesOrderEnabled &&
                netSales > 0m
                    ? (grossProfit /
                       netSales) *
                      100m
                    : 0m;

            // ========================================================
            // DAMAGE
            // ========================================================

            var currentDamage =
                await _context.DamageEntries
                    .AsNoTracking()
                    .Include(x =>
                        x.Product)
                        .ThenInclude(x =>
                            x.Category)
                    .Include(x =>
                        x.Product)
                        .ThenInclude(x =>
                            x.MainCategory)
                    .Where(x =>
                        !x.IsDeleted &&
                        x.DamageDate >= from &&
                        x.DamageDate < to)
                    .ToListAsync();

            var damageQuantity =
                currentDamage.Sum(x =>
                    x.Quantity);

            var damageValue =
                currentDamage.Sum(x =>
                    x.TotalValue);

            // ========================================================
            // KPI CHANGES
            // ========================================================

            var salesChangePercent =
                salesOrderEnabled
                    ? CalculateChange(
                        netSales,
                        previousNetSales)
                    : 0m;

            var purchaseChangePercent =
                CalculateChange(
                    purchases,
                    previousPurchases);

            var profitChangePercent =
                salesOrderEnabled
                    ? CalculateChange(
                        grossProfit,
                        previousGrossProfit)
                    : 0m;

            var ordersChangePercent =
                salesOrderEnabled
                    ? CalculateChange(
                        totalOrders,
                        previousOrders)
                    : 0m;

            // ========================================================
            // MAIN VIEW MODEL
            // ========================================================

            var model =
                new AnalyticsDashboardViewModel
                {
                    FromDate =
                        from,

                    ToDate =
                        to.AddDays(-1),

                    NetSales =
                        Math.Round(
                            netSales,
                            2),

                    Purchases =
                        Math.Round(
                            purchases,
                            2),

                    CostOfGoodsSold =
                        Math.Round(
                            costOfGoodsSold,
                            2),

                    GrossProfit =
                        Math.Round(
                            grossProfit,
                            2),

                    GrossMarginPercent =
                        Math.Round(
                            grossMarginPercent,
                            2),

                    InventoryValue =
                        Math.Round(
                            inventoryValue,
                            2),

                    TotalOrders =
                        totalOrders,

                    AverageOrderValue =
                        Math.Round(
                            averageOrderValue,
                            2),

                    PreviousNetSales =
                        Math.Round(
                            previousNetSales,
                            2),

                    PreviousPurchases =
                        Math.Round(
                            previousPurchases,
                            2),

                    PreviousGrossProfit =
                        Math.Round(
                            previousGrossProfit,
                            2),

                    PreviousOrders =
                        previousOrders,

                    SalesChangePercent =
                        salesChangePercent,

                    PurchaseChangePercent =
                        purchaseChangePercent,

                    ProfitChangePercent =
                        profitChangePercent,

                    OrdersChangePercent =
                        ordersChangePercent,

                    InStockProducts =
                        inStockProducts,

                    LowStockProducts =
                        lowStockProducts,

                    OutOfStockProducts =
                        outOfStockProducts,

                    TotalStockQuantity =
                        totalStockQuantity,

                    ReservedStockQuantity =
                        reservedStockQuantity,

                    PurchaseReceiptQuantity =
                        purchaseReceiptQuantity,

                    SalesIssueQuantity =
                        salesIssueQuantity,

                    SalesReturnQuantity =
                        salesReturnQuantity,

                    PurchaseReturnQuantity =
                        purchaseReturnQuantity,

                    TransferInQuantity =
                        transferInQuantity,

                    TransferOutQuantity =
                        transferOutQuantity,

                    AdjustmentQuantity =
                        adjustmentQuantity,

                    DamageQuantity =
                        damageQuantity,

                    DamageValue =
                        damageValue
                };

            // ========================================================
            // SALES TREND
            // ========================================================

            if (salesOrderEnabled)
            {
                var salesTrend =
                    currentSalesOrders
                        .GroupBy(x =>
                            x.OrderDate.Date)
                        .OrderBy(x =>
                            x.Key)
                        .ToList();

                model.SalesTrendLabels =
                    salesTrend
                        .Select(x =>
                            x.Key.ToString("dd MMM"))
                        .ToList();

                model.SalesTrendValues =
                    salesTrend
                        .Select(x =>
                            x.Sum(GetNetSales))
                        .ToList();
            }

            // ========================================================
            // PURCHASE TREND
            // ========================================================

            var purchaseTrend =
                currentPurchaseItems
                    .GroupBy(x =>
                        x.GoodsReceipt!
                            .ReceiptDate.Date)
                    .OrderBy(x =>
                        x.Key)
                    .ToList();

            model.PurchaseTrendLabels =
                purchaseTrend
                    .Select(x =>
                        x.Key.ToString("dd MMM"))
                    .ToList();

            model.PurchaseTrendValues =
                purchaseTrend
                    .Select(x =>
                        x.Sum(y =>
                            y.ReceivedQuantity *
                            y.UnitPrice))
                    .ToList();

            // ========================================================
            // PROFIT TREND
            // ========================================================
            //
            // Directly use SalesOrder.GrossProfit.
            //
            // ========================================================

            if (salesOrderEnabled)
            {
                var profitTrend =
                    currentSalesOrders
                        .GroupBy(x =>
                            x.OrderDate.Date)
                        .OrderBy(x =>
                            x.Key)
                        .ToList();

                model.ProfitTrendLabels =
                    profitTrend
                        .Select(x =>
                            x.Key.ToString("dd MMM"))
                        .ToList();

                model.ProfitTrendValues =
                    profitTrend
                        .Select(x =>
                            x.Sum(y =>
                                y.GrossProfit))
                        .ToList();
            }

            // ========================================================
            // MAIN CATEGORY MASTER
            // ========================================================

            var masterMainCategoryNames =
                await _context.MainCategories
                    .AsNoTracking()
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(
                            x.Name))
                    .Select(x =>
                        x.Name)
                    .ToListAsync();

            // ========================================================
            // MAIN CATEGORY NAMES FROM DATA
            // ========================================================

            var transactionMainCategoryNames =
                warehouseStocks
                    .Where(x =>
                        x.Product?.MainCategory != null)
                    .Select(x =>
                        x.Product!
                            .MainCategory!
                            .Name)

                    .Concat(
                        currentPurchaseItems
                            .Where(x =>
                                x.Product?
                                    .MainCategory != null)
                            .Select(x =>
                                x.Product!
                                    .MainCategory!
                                    .Name))

                    .Concat(
                        currentSalesItems
                            .Where(x =>
                                x.Product?
                                    .MainCategory != null)
                            .Select(x =>
                                x.Product!
                                    .MainCategory!
                                    .Name))

                    .Concat(
                        currentDamage
                            .Where(x =>
                                x.Product?
                                    .MainCategory != null)
                            .Select(x =>
                                x.Product!
                                    .MainCategory!
                                    .Name))

                    .Where(x =>
                        !string.IsNullOrWhiteSpace(x))
                    .ToList();

            var mainCategoryNames =
                masterMainCategoryNames
                    .Concat(
                        transactionMainCategoryNames)
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(x))
                    .Distinct(
                        StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x =>
                        x)
                    .ToList();

            // ========================================================
            // MAIN CATEGORY NET SALES
            // ========================================================

            var mainCategoryNetSalesByName =
                new Dictionary<string, decimal>(
                    StringComparer.OrdinalIgnoreCase);

            if (salesOrderEnabled)
            {
                foreach (var item in currentSalesItems)
                {
                    var categoryName =
                        item.Product?
                            .MainCategory?
                            .Name;

                    if (string.IsNullOrWhiteSpace(
                        categoryName))
                    {
                        continue;
                    }

                    var grossSales =
                        item.Quantity *
                        item.SalePrice;

                    var discount =
                        grossSales *
                        item.DiscountPercent /
                        100m;

                    var netSalesLine =
                        Math.Max(
                            grossSales -
                            discount,
                            0m);

                    if (mainCategoryNetSalesByName
                        .ContainsKey(categoryName))
                    {
                        mainCategoryNetSalesByName[
                            categoryName] +=
                            netSalesLine;
                    }
                    else
                    {
                        mainCategoryNetSalesByName[
                            categoryName] =
                            netSalesLine;
                    }
                }
            }

            // ========================================================
            // BUILD MAIN CATEGORY ANALYTICS
            // ========================================================

            foreach (var mainCategoryName
                in mainCategoryNames)
            {
                var mainCategoryStocks =
                    warehouseStocks
                        .Where(x =>
                            string.Equals(
                                x.Product?
                                    .MainCategory?
                                    .Name,
                                mainCategoryName,
                                StringComparison
                                    .OrdinalIgnoreCase))
                        .ToList();

                var mainCategoryPurchases =
                    currentPurchaseItems
                        .Where(x =>
                            string.Equals(
                                x.Product?
                                    .MainCategory?
                                    .Name,
                                mainCategoryName,
                                StringComparison
                                    .OrdinalIgnoreCase))
                        .ToList();

                var mainCategorySales =
                    salesOrderEnabled
                        ? currentSalesItems
                            .Where(x =>
                                string.Equals(
                                    x.Product?
                                        .MainCategory?
                                        .Name,
                                    mainCategoryName,
                                    StringComparison
                                        .OrdinalIgnoreCase))
                            .ToList()
                        : new List<SalesOrderItem>();

                var mainCategoryDamage =
                    currentDamage
                        .Where(x =>
                            string.Equals(
                                x.Product?
                                    .MainCategory?
                                    .Name,
                                mainCategoryName,
                                StringComparison
                                    .OrdinalIgnoreCase))
                        .ToList();

                // ====================================================
                // QUANTITIES
                // ====================================================

                var stockBalance =
                    mainCategoryStocks.Sum(x =>
                        x.QuantityOnHand);

                var purchaseBalance =
                    mainCategoryPurchases.Sum(x =>
                        x.ReceivedQuantity);

                var saleBalance =
                    salesOrderEnabled
                        ? mainCategorySales.Sum(x =>
                            x.Quantity)
                        : 0m;

                var damage =
                    mainCategoryDamage.Sum(x =>
                        x.Quantity);

                // ====================================================
                // INVENTORY VALUE
                // ====================================================

                var stockValue =
                    mainCategoryStocks.Sum(x =>
                    {
                        if (purchaseCosts.TryGetValue(
                            x.ProductId,
                            out var cost))
                        {
                            return
                                x.QuantityOnHand *
                                cost;
                        }

                        return 0m;
                    });

                // ====================================================
                // PURCHASE VALUE
                // ====================================================

                var purchaseValue =
                    mainCategoryPurchases.Sum(x =>
                        x.ReceivedQuantity *
                        x.UnitPrice);

                // ====================================================
                // NET SALES
                // ====================================================

                var mainCategorySalesValue =
                    salesOrderEnabled &&
                    mainCategoryNetSalesByName.TryGetValue(
                        mainCategoryName,
                        out var categorySales)
                        ? categorySales
                        : 0m;

                // ====================================================
                // CATEGORY COGS
                // ====================================================
                //
                // Sales Order Item UnitPrice is the purchase/GRN cost.
                //
                // ====================================================

                var mainCategoryCOGS =
                    salesOrderEnabled
                        ? mainCategorySales.Sum(x =>
                            x.Quantity *
                            x.UnitPrice)
                        : 0m;

                // ====================================================
                // CATEGORY GROSS PROFIT
                // ====================================================

                var mainCategoryGrossProfit =
                    salesOrderEnabled
                        ? mainCategorySales.Sum(x =>
                        {
                            var grossSales =
                                x.Quantity *
                                x.SalePrice;

                            var discount =
                                grossSales *
                                x.DiscountPercent /
                                100m;

                            var netSalesLine =
                                Math.Max(
                                    grossSales -
                                    discount,
                                    0m);

                            var cost =
                                x.Quantity *
                                x.UnitPrice;

                            return
                                netSalesLine -
                                cost;
                        })
                        : 0m;

                // ====================================================
                // OPENING BALANCE
                // ====================================================

                var openingBalance =
                    Math.Max(
                        stockBalance -
                        purchaseBalance +
                        saleBalance +
                        damage,
                        0m);

                // ====================================================
                // ADD CATEGORY
                // ====================================================

                model.CategoryAnalytics.Add(
                    new CategoryAnalyticsRowViewModel
                    {
                        MainCategoryName =
                            mainCategoryName,

                        CategoryName =
                            mainCategoryName,

                        OpeningBalance =
                            openingBalance,

                        PurchaseBalance =
                            purchaseBalance,

                        SaleBalance =
                            saleBalance,

                        Damage =
                            damage,

                        StockBalance =
                            stockBalance,

                        StockValue =
                            stockValue,

                        SalesValue =
                            Math.Round(
                                mainCategorySalesValue,
                                2),

                        PurchaseValue =
                            Math.Round(
                                purchaseValue,
                                2),

                        COGS =
                            Math.Round(
                                mainCategoryCOGS,
                                2),

                        GrossProfit =
                            Math.Round(
                                mainCategoryGrossProfit,
                                2)
                    });
            }

            // ========================================================
            // CATEGORY GROSS PROFIT RECONCILIATION
            // ========================================================

            if (salesOrderEnabled &&
                model.CategoryAnalytics.Count > 0)
            {
                var categoryGrossProfit =
                    model.CategoryAnalytics.Sum(
                        x => x.GrossProfit);

                var reconciliationDifference =
                    grossProfit -
                    categoryGrossProfit;

                if (Math.Abs(
                        reconciliationDifference) > 0m &&
                    Math.Abs(
                        reconciliationDifference) <= 0.01m)
                {
                    var largestSalesCategory =
                        model.CategoryAnalytics
                            .Where(x =>
                                x.SalesValue != 0m)
                            .OrderByDescending(x =>
                                Math.Abs(
                                    x.SalesValue))
                            .FirstOrDefault();

                    if (largestSalesCategory != null)
                    {
                        largestSalesCategory.GrossProfit +=
                            reconciliationDifference;
                    }
                }
            }

            // ========================================================
            // INVENTORY PERCENTAGE
            // ========================================================

            var totalMainCategoryStockValue =
                model.CategoryAnalytics.Sum(
                    x => x.StockValue);

            foreach (var category
                in model.CategoryAnalytics)
            {
                category.PercentageOfTotal =
                    totalMainCategoryStockValue > 0
                        ? category.StockValue /
                          totalMainCategoryStockValue *
                          100m
                        : 0m;
            }

            // ========================================================
            // FOUR MAIN KPI CHARTS
            // ========================================================

            model.CategoryChartLabels =
                model.CategoryAnalytics
                    .Select(x =>
                        x.MainCategoryName)
                    .ToList();

            model.CategorySalesValues =
                model.CategoryAnalytics
                    .Select(x =>
                        x.SalesValue)
                    .ToList();

            model.CategoryPurchaseValues =
                model.CategoryAnalytics
                    .Select(x =>
                        x.PurchaseValue)
                    .ToList();

            model.CategoryGrossProfitValues =
                model.CategoryAnalytics
                    .Select(x =>
                        x.GrossProfit)
                    .ToList();

            model.CategoryInventoryValues =
                model.CategoryAnalytics
                    .Select(x =>
                        x.StockValue)
                    .ToList();

            model.CategoryCOGSValues =
                model.CategoryAnalytics
                    .Select(x =>
                        x.COGS)
                    .ToList();

            // ========================================================
            // INVENTORY VALUE COMPATIBILITY
            // ========================================================

            model.InventoryValueByCategoryLabels =
                model.CategoryAnalytics
                    .Where(x =>
                        x.StockValue > 0)
                    .Select(x =>
                        x.MainCategoryName)
                    .ToList();

            model.InventoryValueByCategoryValues =
                model.CategoryAnalytics
                    .Where(x =>
                        x.StockValue > 0)
                    .Select(x =>
                        x.StockValue)
                    .ToList();

            // ========================================================
            // WAREHOUSE ANALYTICS
            // ========================================================

            var warehouseGroups =
                warehouseStocks
                    .Where(x =>
                        x.Warehouse != null)
                    .GroupBy(x =>
                        new
                        {
                            x.WarehouseId,

                            WarehouseName =
                                x.Warehouse!.Name
                        })
                    .OrderBy(x =>
                        x.Key.WarehouseName)
                    .ToList();

            foreach (var warehouseGroup
                in warehouseGroups)
            {
                var warehouseId =
                    warehouseGroup.Key.WarehouseId;

                var warehouseName =
                    warehouseGroup.Key.WarehouseName;

                var warehouseStockQuantity =
                    warehouseGroup.Sum(x =>
                        x.QuantityOnHand);

                var warehouseStockValue =
                    warehouseGroup.Sum(x =>
                    {
                        if (purchaseCosts.TryGetValue(
                            x.ProductId,
                            out var cost))
                        {
                            return
                                x.QuantityOnHand *
                                cost;
                        }

                        return 0m;
                    });

                var warehousePurchaseValue =
                    currentPurchaseItems
                        .Where(x =>
                            x.WarehouseId ==
                            warehouseId)
                        .Sum(x =>
                            x.ReceivedQuantity *
                            x.UnitPrice);

                var warehouseSalesValue =
                    salesOrderEnabled
                        ? currentSalesOrders
                            .Where(x =>
                                x.WarehouseId ==
                                warehouseId)
                            .Sum(GetNetSales)
                        : 0m;

                model.Warehouses.Add(
                    new WarehouseAnalyticsRowViewModel
                    {
                        WarehouseId =
                            warehouseId,

                        WarehouseName =
                            warehouseName,

                        StockQuantity =
                            warehouseStockQuantity,

                        StockValue =
                            warehouseStockValue,

                        PurchaseValue =
                            warehousePurchaseValue,

                        SalesValue =
                            warehouseSalesValue
                    });

                model.WarehouseLabels.Add(
                    warehouseName);

                model.WarehouseStockValues.Add(
                    warehouseStockValue);
            }

            // ========================================================
            // INSIGHTS
            // ========================================================

            BuildInsights(
                model,
                salesOrderEnabled);

            // ========================================================
            // VIEW BAG
            // ========================================================

            ViewBag.SalesOrderEnabled =
                salesOrderEnabled;

            ViewBag.SalesOrderLabel =
                salesOrderLabel;

            ViewBag.SalesOrderPluralLabel =
                salesOrderPluralLabel;

            return View(model);
        }

        // ============================================================
        // NET SALES
        // ============================================================
        //
        // SubTotal already represents:
        //
        // Quantity × SalePrice
        //
        // Net Sales =
        //
        // SubTotal - DiscountAmount
        //
        // Tax is excluded.
        //
        // ============================================================

        private static decimal GetNetSales(
            SalesOrder order)
        {
            return Math.Max(
                order.SubTotal -
                order.DiscountAmount,
                0m);
        }

        // ============================================================
        // CHANGE %
        // ============================================================

        private static decimal CalculateChange(
            decimal current,
            decimal previous)
        {
            if (previous == 0)
            {
                return current == 0
                    ? 0m
                    : 100m;
            }

            return
                ((current - previous) /
                 Math.Abs(previous)) *
                100m;
        }

        // ============================================================
        // INSIGHTS
        // ============================================================

        private static void BuildInsights(
            AnalyticsDashboardViewModel model,
            bool salesOrderEnabled)
        {
            // ========================================================
            // SALES
            // ========================================================

            if (salesOrderEnabled)
            {
                if (model.SalesChangePercent > 0)
                {
                    model.Insights.Add(
                        new AnalyticsInsightViewModel
                        {
                            Title =
                                "Sales Growth",

                            Description =
                                $"Sales increased by {model.SalesChangePercent:N1}% compared with the previous period.",

                            Type =
                                "success",

                            Icon =
                                "bi-graph-up-arrow"
                        });
                }
                else if (model.SalesChangePercent < 0)
                {
                    model.Insights.Add(
                        new AnalyticsInsightViewModel
                        {
                            Title =
                                "Sales Movement",

                            Description =
                                $"Sales decreased by {Math.Abs(model.SalesChangePercent):N1}% compared with the previous period.",

                            Type =
                                "warning",

                            Icon =
                                "bi-graph-down-arrow"
                        });
                }

                // ====================================================
                // GROSS PROFIT
                // ====================================================

                if (model.GrossProfit > 0)
                {
                    model.Insights.Add(
                        new AnalyticsInsightViewModel
                        {
                            Title =
                                "Gross Profit",

                            Description =
                                $"Gross profit for the selected period is ₹{model.GrossProfit:N2}.",

                            Type =
                                "success",

                            Icon =
                                "bi-currency-rupee"
                        });
                }
                else if (model.GrossProfit < 0)
                {
                    model.Insights.Add(
                        new AnalyticsInsightViewModel
                        {
                            Title =
                                "Gross Profit",

                            Description =
                                $"Gross profit for the selected period is -₹{Math.Abs(model.GrossProfit):N2}.",

                            Type =
                                "danger",

                            Icon =
                                "bi-graph-down"
                        });
                }
            }

            // ========================================================
            // PURCHASES
            // ========================================================

            if (model.PurchaseChangePercent > 0)
            {
                model.Insights.Add(
                    new AnalyticsInsightViewModel
                    {
                        Title =
                            "Purchase Increase",

                        Description =
                            $"Purchases increased by {model.PurchaseChangePercent:N1}% compared with the previous period.",

                        Type =
                            "info",

                        Icon =
                            "bi-cart-plus"
                    });
            }
            else if (model.PurchaseChangePercent < 0)
            {
                model.Insights.Add(
                    new AnalyticsInsightViewModel
                    {
                        Title =
                            "Purchase Decrease",

                        Description =
                            $"Purchases decreased by {Math.Abs(model.PurchaseChangePercent):N1}% compared with the previous period.",

                        Type =
                            "info",

                        Icon =
                            "bi-cart-dash"
                    });
            }

            // ========================================================
            // LOW STOCK
            // ========================================================

            if (model.LowStockProducts > 0)
            {
                model.Insights.Add(
                    new AnalyticsInsightViewModel
                    {
                        Title =
                            "Low Stock",

                        Description =
                            $"{model.LowStockProducts} product(s) are currently at or below minimum stock level.",

                        Type =
                            "warning",

                        Icon =
                            "bi-exclamation-triangle"
                    });
            }

            // ========================================================
            // OUT OF STOCK
            // ========================================================

            if (model.OutOfStockProducts > 0)
            {
                model.Insights.Add(
                    new AnalyticsInsightViewModel
                    {
                        Title =
                            "Out of Stock",

                        Description =
                            $"{model.OutOfStockProducts} product(s) currently have no stock.",

                        Type =
                            "danger",

                        Icon =
                            "bi-x-circle"
                    });
            }

            // ========================================================
            // RESERVED STOCK
            // ========================================================

            if (model.ReservedStockQuantity > 0)
            {
                model.Insights.Add(
                    new AnalyticsInsightViewModel
                    {
                        Title =
                            "Reserved Stock",

                        Description =
                            $"{model.ReservedStockQuantity:N0} units are currently reserved.",

                        Type =
                            "info",

                        Icon =
                            "bi-box-seam"
                    });
            }

            // ========================================================
            // DAMAGE
            // ========================================================

            if (model.DamageValue > 0)
            {
                model.Insights.Add(
                    new AnalyticsInsightViewModel
                    {
                        Title =
                            "Damage",

                        Description =
                            $"Damage value for the selected period is ₹{model.DamageValue:N2}.",

                        Type =
                            "warning",

                        Icon =
                            "bi-exclamation-octagon"
                    });
            }

            // ========================================================
            // DEFAULT
            // ========================================================

            if (model.Insights.Count == 0)
            {
                model.Insights.Add(
                    new AnalyticsInsightViewModel
                    {
                        Title =
                            "Business Status",

                        Description =
                            "No major business movement was detected for the selected period.",

                        Type =
                            "info",

                        Icon =
                            "bi-info-circle"
                    });
            }
        }
    }
}