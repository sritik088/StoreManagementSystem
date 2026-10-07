using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Enums;
using StoreManagementSystem.Infrastructure.Data;
using StoreManagementSystem.Web.ViewModels.Dashboard;

namespace StoreManagementSystem.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ISystemSettingService _settingService;

        public DashboardController(
            ApplicationDbContext context,
            ISystemSettingService settingService)
        {
            _context = context;
            _settingService = settingService;
        }


        // =========================================================
        // DASHBOARD
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index(
            int? mainCategoryId,
            string? purchaseOrderStatus)
        {
            // =====================================================
            // DATE RANGE
            // =====================================================

            var today = DateTime.Today;

            var monthStart = new DateTime(
                today.Year,
                today.Month,
                1);

            var tomorrow = today.AddDays(1);


            // =====================================================
            // SALES ORDER SETTINGS
            // =====================================================

            var salesOrderEnabled =
                await _settingService.IsSalesOrderEnabledAsync();

            var salesOrderLabel =
                await _settingService.GetSalesOrderLabelAsync();

            var salesOrderPluralLabel =
                await _settingService.GetSalesOrderPluralLabelAsync();

          


            // =====================================================
            // SEND SETTINGS TO VIEW
            // =====================================================

            ViewData["SalesOrderEnabled"] =
                salesOrderEnabled;

            ViewData["SalesOrderLabel"] =
                salesOrderLabel;

            ViewData["SalesOrderPluralLabel"] =
                salesOrderPluralLabel;

            


            // =====================================================
            // MODEL
            // =====================================================

            var model = new DashboardViewModel
            {
                MainCategoryFilterId =
                    mainCategoryId,

                PurchaseOrderStatusFilter =
                    purchaseOrderStatus
            };


            // =====================================================
            // MAIN CATEGORY DROPDOWN
            // =====================================================

            model.MainCategories =
                await _context.MainCategories
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .OrderBy(x => x.Name)
                    .Select(x => new DashboardMainCategoryFilterViewModel
                    {
                        Id = x.Id,
                        Name = x.Name
                    })
                    .ToListAsync();


            // =====================================================
            // PURCHASE ORDER STATUS DROPDOWN
            // =====================================================

            model.PurchaseOrderStatuses =
                Enum.GetValues<PurchaseOrderStatus>()
                    .Select(x => new DashboardPurchaseStatusFilterViewModel
                    {
                        Value = x.ToString(),
                        Name = x.ToString()
                    })
                    .ToList();


            // =====================================================
            // BASIC COUNTS
            // =====================================================

            model.TotalProducts =
                await _context.Products
                    .CountAsync(x => !x.IsDeleted);

            model.TotalCategories =
                await _context.Categories
                    .CountAsync(x => !x.IsDeleted);

            model.TotalSubCategories =
                await _context.SubCategories
                    .CountAsync(x => !x.IsDeleted);

            model.TotalSuppliers =
                await _context.Suppliers
                    .CountAsync(x => !x.IsDeleted);

            model.TotalCustomers =
                await _context.Customers
                    .CountAsync();

            model.TotalWarehouses =
                await _context.Warehouses
                    .CountAsync(x => !x.IsDeleted);


            // =========================================================
            // SALES ORDERS
            //
            // IMPORTANT:
            // If Sales Order module is OFF, SalesOrders table is
            // NOT queried.
            // =========================================================

            var allSalesOrders =
                new List<Domain.Entities.SalesOrder>();

            if (salesOrderEnabled)
            {
                allSalesOrders =
                    await _context.SalesOrders
                        .AsNoTracking()
                        .ToListAsync();
            }


            // =========================================================
            // SALES ORDER SUMMARY
            // =========================================================

            if (salesOrderEnabled)
            {
                model.TotalSalesOrders =
                    allSalesOrders.Count;

                model.TotalRevenue =
                    allSalesOrders.Sum(x =>
                        x.GrandTotal);

                model.TodaySales =
                    allSalesOrders
                        .Where(x =>
                            x.OrderDate.Date == today)
                        .Sum(x =>
                            x.GrandTotal);

                model.MonthlySales =
                    allSalesOrders
                        .Where(x =>
                            x.OrderDate >= monthStart &&
                            x.OrderDate < tomorrow)
                        .Sum(x =>
                            x.GrandTotal);


                // =================================================
                // SALES ORDER STATUS SUMMARY
                //
                // Uses actual enum names from SalesOrder.Status.
                // =================================================

                foreach (var order in allSalesOrders)
                {
                    var statusName =
                        order.Status
                            .ToString()
                            .ToLower();

                    switch (statusName)
                    {
                        case "draft":
                            model.DraftSalesOrders++;
                            break;

                        case "confirmed":
                            model.ConfirmedSalesOrders++;
                            break;

                        case "processing":
                            model.ProcessingSalesOrders++;
                            break;

                        case "delivered":
                            model.DeliveredSalesOrders++;
                            break;

                        case "cancelled":
                        case "canceled":
                            model.CancelledSalesOrders++;
                            break;
                    }
                }


                model.TotalSalesOrderValue =
                    allSalesOrders.Sum(x =>
                        x.GrandTotal);


                model.PendingSalesOrderValue =
                    allSalesOrders
                        .Where(x =>
                        {
                            var status =
                                x.Status
                                    .ToString()
                                    .ToLower();

                            return status == "draft" ||
                                   status == "confirmed" ||
                                   status == "processing";
                        })
                        .Sum(x =>
                            x.GrandTotal);
            }
            else
            {
                // =================================================
                // SALES ORDER MODULE OFF
                // =================================================

                model.TotalSalesOrders = 0;
                model.TotalRevenue = 0m;
                model.TodaySales = 0m;
                model.MonthlySales = 0m;

                model.DraftSalesOrders = 0;
                model.ConfirmedSalesOrders = 0;
                model.ProcessingSalesOrders = 0;
                model.DeliveredSalesOrders = 0;
                model.CancelledSalesOrders = 0;

                model.TotalSalesOrderValue = 0m;
                model.PendingSalesOrderValue = 0m;
            }


            // =========================================================
            // PURCHASE ORDERS
            // =========================================================

            var purchaseOrderQuery =
                _context.PurchaseOrders
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .AsQueryable();


            // =========================================================
            // PURCHASE ORDER STATUS FILTER
            // =========================================================

            if (!string.IsNullOrWhiteSpace(
                    purchaseOrderStatus))
            {
                if (Enum.TryParse<PurchaseOrderStatus>(
                        purchaseOrderStatus,
                        true,
                        out var selectedStatus))
                {
                    purchaseOrderQuery =
                        purchaseOrderQuery.Where(x =>
                            x.Status == selectedStatus);
                }
            }


            var purchaseOrders =
                await purchaseOrderQuery
                    .ToListAsync();


            model.TotalPurchaseOrders =
                purchaseOrders.Count;

            model.TotalPurchaseAmount =
                purchaseOrders.Sum(x =>
                    x.GrandTotal);

            model.TotalPurchaseOrderValue =
                purchaseOrders.Sum(x =>
                    x.GrandTotal);


            model.PendingPurchaseOrderValue =
                purchaseOrders
                    .Where(x =>
                        x.Status ==
                            PurchaseOrderStatus.Draft ||

                        x.Status ==
                            PurchaseOrderStatus.Approved ||

                        x.Status ==
                            PurchaseOrderStatus.PartiallyReceived)
                    .Sum(x =>
                        x.GrandTotal);


            // =========================================================
            // PURCHASE ORDER STATUS COUNTS
            // =========================================================

            model.DraftPurchaseOrders =
                purchaseOrders.Count(x =>
                    x.Status ==
                    PurchaseOrderStatus.Draft);

            model.ApprovedPurchaseOrders =
                purchaseOrders.Count(x =>
                    x.Status ==
                    PurchaseOrderStatus.Approved);

            model.PartiallyReceivedPurchaseOrders =
                purchaseOrders.Count(x =>
                    x.Status ==
                    PurchaseOrderStatus.PartiallyReceived);

            model.FullyReceivedPurchaseOrders =
                purchaseOrders.Count(x =>
                    x.Status ==
                    PurchaseOrderStatus.FullyReceived);

            model.CancelledPurchaseOrders =
                purchaseOrders.Count(x =>
                    x.Status ==
                    PurchaseOrderStatus.Cancelled);


            // =========================================================
            // WAREHOUSE STOCK
            // =========================================================

            var stockQuery =
                _context.WarehouseStocks
                    .AsNoTracking()
                    .Include(x => x.Product)
                        .ThenInclude(x => x!.Category)
                    .Include(x => x.Product)
                        .ThenInclude(x => x!.MainCategory)
                    .Include(x => x.Warehouse)
                    .Where(x =>
                        x.Product != null &&
                        !x.Product.IsDeleted &&
                        x.Warehouse != null &&
                        !x.Warehouse.IsDeleted)
                    .AsQueryable();


            // =========================================================
            // MAIN CATEGORY FILTER
            // =========================================================

            if (mainCategoryId.HasValue &&
                mainCategoryId.Value > 0)
            {
                stockQuery =
                    stockQuery.Where(x =>
                        x.Product!.MainCategoryId ==
                        mainCategoryId.Value);
            }


            var stocks =
                await stockQuery
                    .OrderBy(x => x.Product!.SKU)
                    .ThenBy(x => x.Warehouse!.Name)
                    .ToListAsync();


            // =========================================================
            // STOCK SUMMARY
            // =========================================================

            model.TotalStockQuantity =
                stocks.Sum(x =>
                    x.QuantityOnHand);

            model.AvailableStockQuantity =
                stocks.Sum(x =>
                    x.AvailableQuantity);

            model.ReservedStockQuantity =
                stocks.Sum(x =>
                    x.ReservedQuantity);


            model.InStockProducts =
                stocks
                    .Where(x =>
                        x.QuantityOnHand >
                        x.MinimumStock)
                    .Select(x =>
                        x.ProductId)
                    .Distinct()
                    .Count();


            model.LowStockProducts =
                stocks
                    .Where(x =>
                        x.QuantityOnHand > 0 &&
                        x.QuantityOnHand <=
                        x.MinimumStock)
                    .Select(x =>
                        x.ProductId)
                    .Distinct()
                    .Count();


            model.OutOfStockProducts =
                stocks
                    .Where(x =>
                        x.QuantityOnHand <= 0)
                    .Select(x =>
                        x.ProductId)
                    .Distinct()
                    .Count();


            // =========================================================
            // GRN PURCHASE COSTS
            // =========================================================

            var grnItems =
                await _context.GoodsReceiptItems
                    .AsNoTracking()
                    .Include(x => x.GoodsReceipt)
                    .Where(x =>
                        x.GoodsReceipt != null &&
                        !x.GoodsReceipt.IsDeleted &&
                        x.GoodsReceipt.Status ==
                            GoodsReceiptStatus.Received &&
                        x.ReceivedQuantity > 0)
                    .ToListAsync();


            var purchaseCosts =
                grnItems
                    .GroupBy(x =>
                        x.ProductId)
                    .ToDictionary(
                        g => g.Key,
                        g =>
                        {
                            var quantity =
                                g.Sum(x =>
                                    x.ReceivedQuantity);

                            var cost =
                                g.Sum(x =>
                                    x.ReceivedQuantity *
                                    x.UnitPrice);

                            return quantity > 0
                                ? cost / quantity
                                : 0m;
                        });


            // =========================================================
            // INVENTORY VALUE
            // =========================================================

            model.InventoryValue = 0m;

            foreach (var stock in stocks)
            {
                if (purchaseCosts.TryGetValue(
                        stock.ProductId,
                        out var purchasePrice))
                {
                    model.InventoryValue +=
                        stock.QuantityOnHand *
                        purchasePrice;
                }
            }


            // =========================================================
            // LIVE STOCK
            // =========================================================

            foreach (var stock in stocks)
            {
                if (stock.Product == null ||
                    stock.Warehouse == null)
                {
                    continue;
                }


                string status;

                if (stock.QuantityOnHand <= 0)
                {
                    status = "OutOfStock";
                }
                else if (stock.QuantityOnHand <=
                         stock.MinimumStock)
                {
                    status = "LowStock";
                }
                else
                {
                    status = "InStock";
                }


                model.LiveStock.Add(
                    new DashboardLiveStockViewModel
                    {
                        ProductId =
                            stock.ProductId,

                        ProductName =
                            stock.Product.SKU ??
                            "N/A",

                        SKU =
                            stock.Product.SKU ??
                            "N/A",

                        Barcode =
                            stock.Product.Barcode ??
                            "N/A",

                        MainCategoryName =
                            stock.Product.MainCategory?.Name
                            ?? "Uncategorized",

                        CategoryName =
                            stock.Product.Category?.Name
                            ?? "Uncategorized",

                        WarehouseName =
                            stock.Warehouse.Name,

                        QuantityOnHand =
                            stock.QuantityOnHand,

                        AvailableQuantity =
                            stock.AvailableQuantity,

                        ReservedQuantity =
                            stock.ReservedQuantity,

                        MinimumStock =
                            stock.MinimumStock,

                        Status =
                            status
                    });
            }


            // =========================================================
            // STOCK TRANSFERS
            // =========================================================

            model.TotalStockTransfers =
                await _context.StockTransfers
                    .AsNoTracking()
                    .CountAsync(x =>
                        !x.IsDeleted);


            var transferLedger =
                await _context.StockLedgers
                    .AsNoTracking()
                    .Where(x =>
                        x.TransactionType ==
                            StockTransactionType.StockTransferIn ||

                        x.TransactionType ==
                            StockTransactionType.StockTransferOut)
                    .ToListAsync();


            model.TransferInQuantity =
                transferLedger
                    .Where(x =>
                        x.TransactionType ==
                        StockTransactionType.StockTransferIn)
                    .Sum(x =>
                        Math.Abs(x.Quantity));


            model.TransferOutQuantity =
                transferLedger
                    .Where(x =>
                        x.TransactionType ==
                        StockTransactionType.StockTransferOut)
                    .Sum(x =>
                        Math.Abs(x.Quantity));


            model.TotalTransferredQuantity =
                model.TransferInQuantity;


            // =========================================================
            // MAIN CATEGORY ANALYTICS
            // =========================================================

            var categoryQuery =
                _context.Products
                    .AsNoTracking()
                    .Include(x => x.MainCategory)
                    .Where(x => !x.IsDeleted)
                    .AsQueryable();


            if (mainCategoryId.HasValue &&
                mainCategoryId.Value > 0)
            {
                categoryQuery =
                    categoryQuery.Where(x =>
                        x.MainCategoryId ==
                        mainCategoryId.Value);
            }


            var products =
                await categoryQuery
                    .ToListAsync();


            var productIds =
                products
                    .Select(x => x.Id)
                    .ToHashSet();


            // =========================================================
            // STOCK LEDGER
            // =========================================================

            var ledger =
                await _context.StockLedgers
                    .AsNoTracking()
                    .Include(x => x.Product)
                        .ThenInclude(x => x!.MainCategory)
                    .Where(x =>
                        x.Product != null &&
                        !x.Product.IsDeleted &&
                        productIds.Contains(x.ProductId))
                    .ToListAsync();


            // =========================================================
            // DAMAGE
            // =========================================================

            var damages =
                await _context.DamageEntries
                    .AsNoTracking()
                    .Include(x => x.Product)
                        .ThenInclude(x => x!.MainCategory)
                    .Where(x =>
                        !x.IsDeleted &&
                        x.Product != null &&
                        !x.Product.IsDeleted &&
                        productIds.Contains(x.ProductId) &&
                        x.DamageDate >= monthStart &&
                        x.DamageDate < tomorrow)
                    .ToListAsync();


            // =========================================================
            // SALES ORDER ITEMS
            //
            // IMPORTANT:
            // If Sales Order module is OFF, SalesOrderItems table
            // is NOT queried.
            // =========================================================

            var salesItems =
                new List<Domain.Entities.SalesOrderItem>();

            if (salesOrderEnabled)
            {
                salesItems =
                    await _context.SalesOrderItems
                        .AsNoTracking()
                        .Include(x => x.SalesOrder)
                        .Include(x => x.Product)
                            .ThenInclude(x => x!.MainCategory)
                        .Where(x =>
                            x.SalesOrder != null &&
                            x.SalesOrder.OrderDate >= monthStart &&
                            x.SalesOrder.OrderDate < tomorrow &&
                            x.Product != null &&
                            !x.Product.IsDeleted &&
                            productIds.Contains(x.ProductId))
                        .ToListAsync();
            }


            // =========================================================
            // MAIN CATEGORY GROUPS
            // =========================================================

            var mainCategoryGroups =
                products
                    .GroupBy(x =>
                        new
                        {
                            Id =
                                x.MainCategoryId,

                            Name =
                                x.MainCategory != null
                                    ? x.MainCategory.Name
                                    : "Uncategorized"
                        })
                    .ToList();


            var totalCategoryStock =
                stocks.Sum(x =>
                    productIds.Contains(x.ProductId)
                        ? x.QuantityOnHand
                        : 0m);


            // =========================================================
            // CATEGORY CALCULATION
            // =========================================================

            foreach (var group in mainCategoryGroups)
            {
                var groupProductIds =
                    group
                        .Select(x => x.Id)
                        .ToHashSet();


                // =================================================
                // OPENING
                // =================================================

                var openingBalance =
                    ledger
                        .Where(x =>
                            groupProductIds.Contains(
                                x.ProductId) &&

                            x.TransactionDate <
                                monthStart)
                        .Sum(x =>
                            GetSignedLedgerQuantity(
                                x.TransactionType,
                                x.Quantity));


                // =================================================
                // PURCHASE
                // =================================================

                var purchaseLedger =
                    ledger
                        .Where(x =>
                            groupProductIds.Contains(
                                x.ProductId) &&

                            x.TransactionType ==
                                StockTransactionType.PurchaseReceipt &&

                            x.TransactionDate >=
                                monthStart &&

                            x.TransactionDate <
                                tomorrow)
                        .ToList();


                var purchaseBalance =
                    purchaseLedger.Sum(x =>
                        Math.Abs(x.Quantity));


                // =================================================
                // SALES
                // =================================================

                decimal saleBalance = 0m;

                if (salesOrderEnabled)
                {
                    saleBalance =
                        ledger
                            .Where(x =>
                                groupProductIds.Contains(
                                    x.ProductId) &&

                                x.TransactionType ==
                                    StockTransactionType.SalesIssue &&

                                x.TransactionDate >=
                                    monthStart &&

                                x.TransactionDate <
                                    tomorrow)
                            .Sum(x =>
                                Math.Abs(x.Quantity));
                }


                // =================================================
                // DAMAGE
                // =================================================

                var damageQuantity =
                    damages
                        .Where(x =>
                            groupProductIds.Contains(
                                x.ProductId))
                        .Sum(x =>
                            x.Quantity);


                // =================================================
                // CURRENT STOCK
                // =================================================

                var stockBalance =
                    stocks
                        .Where(x =>
                            groupProductIds.Contains(
                                x.ProductId))
                        .Sum(x =>
                            x.QuantityOnHand);


                // =================================================
                // STOCK VALUE
                // =================================================

                decimal stockValue = 0m;

                foreach (var stock in stocks.Where(x =>
                    groupProductIds.Contains(
                        x.ProductId)))
                {
                    if (purchaseCosts.TryGetValue(
                            stock.ProductId,
                            out var price))
                    {
                        stockValue +=
                            stock.QuantityOnHand *
                            price;
                    }
                }


                // =================================================
                // PURCHASE VALUE
                // =================================================

                decimal purchaseValue =
                    purchaseLedger.Sum(x =>
                    {
                        var price =
                            x.UnitCost;

                        if (price <= 0 &&
                            purchaseCosts.TryGetValue(
                                x.ProductId,
                                out var averagePrice))
                        {
                            price =
                                averagePrice;
                        }

                        return Math.Abs(x.Quantity) *
                               price;
                    });


                // =================================================
                // SALES VALUE
                // =================================================

                decimal salesValue = 0m;

                if (salesOrderEnabled)
                {
                    salesValue =
                        salesItems
                            .Where(x =>
                                groupProductIds.Contains(
                                    x.ProductId))
                            .Sum(x =>
                                x.LineTotal);
                }


                // =================================================
                // PERCENTAGE
                // =================================================

                decimal percentage = 0m;

                if (totalCategoryStock > 0)
                {
                    percentage =
                        stockBalance /
                        totalCategoryStock *
                        100m;
                }


                // =================================================
                // ADD MAIN CATEGORY
                // =================================================

                model.MainCategoryAnalytics.Add(
                    new MainCategoryBusinessAnalyticsViewModel
                    {
                        MainCategoryId =
                            group.Key.Id,

                        MainCategoryName =
                            group.Key.Name,

                        OpeningBalance =
                            openingBalance,

                        PurchaseBalance =
                            purchaseBalance,

                        SaleBalance =
                            saleBalance,

                        Damage =
                            damageQuantity,

                        StockBalance =
                            stockBalance,

                        StockValue =
                            stockValue,

                        SalesValue =
                            salesValue,

                        PurchaseValue =
                            purchaseValue,

                        PercentageOfTotal =
                            percentage
                    });
            }


            // =========================================================
            // SORT MAIN CATEGORY ANALYTICS
            // =========================================================

            model.MainCategoryAnalytics =
                model.MainCategoryAnalytics
                    .OrderBy(x =>
                        x.MainCategoryName)
                    .ToList();


            // =========================================================
            // RETURN VIEW
            // =========================================================

            return View(model);
        }


        // =========================================================
        // SIGNED LEDGER QUANTITY
        // =========================================================

        private static decimal GetSignedLedgerQuantity(
            StockTransactionType transactionType,
            decimal quantity)
        {
            var absoluteQuantity =
                Math.Abs(quantity);

            return transactionType switch
            {
                StockTransactionType.OpeningStock =>
                    quantity,

                StockTransactionType.PurchaseReceipt =>
                    absoluteQuantity,

                StockTransactionType.SalesIssue =>
                    -absoluteQuantity,

                StockTransactionType.SalesReturn =>
                    absoluteQuantity,

                StockTransactionType.PurchaseReturn =>
                    -absoluteQuantity,

                StockTransactionType.StockTransferIn =>
                    absoluteQuantity,

                StockTransactionType.StockTransferOut =>
                    -absoluteQuantity,

                StockTransactionType.StockAdjustment =>
                    quantity,

                _ =>
                    quantity
            };
        }
    }
}