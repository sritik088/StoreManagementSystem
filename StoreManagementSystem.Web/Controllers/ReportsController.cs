
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Enums;
using StoreManagementSystem.Infrastructure.Data;
using StoreManagementSystem.Web.ViewModels.Reports;

namespace StoreManagementSystem.Web.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ISystemSettingService _settingService;

        public ReportsController(
            ApplicationDbContext context,
            ISystemSettingService settingService)
        {
            _context = context;
            _settingService = settingService;
        }

        // =========================================================
        // ORGANISATION / REPORT SETTINGS
        // =========================================================

        private async Task LoadOrganisationSettingsAsync()
        {
            ViewData["OrganisationName"] =
                await _settingService.GetOrganisationNameAsync();

            ViewData["Branch"] =
                await _settingService.GetBranchAsync();

            ViewData["Address"] =
                await _settingService.GetAddressAsync();

            ViewData["Phone"] =
                await _settingService.GetPhoneAsync();

            ViewData["Email"] =
                await _settingService.GetEmailAsync();

            ViewData["GSTIN"] =
                await _settingService.GetGSTINAsync();

            ViewData["ReportHeading"] =
                await _settingService.GetReportHeadingAsync();

            ViewData["FromDateLabel"] =
                await _settingService.GetFromDateLabelAsync();

            ViewData["ToDateLabel"] =
                await _settingService.GetToDateLabelAsync();

            ViewData["AuthorisedSignatory"] =
                await _settingService.GetAuthorisedSignatoryAsync();

            ViewData["AuthorisedDesignation"] =
                await _settingService.GetAuthorisedDesignationAsync();
        }

        // =========================================================
        // SALES ORDER SETTINGS
        // =========================================================

        private async Task LoadSalesOrderSettingsAsync()
        {
            ViewData["SalesOrderEnabled"] =
                await _settingService.IsSalesOrderEnabledAsync();

            ViewData["SalesOrderLabel"] =
                await _settingService.GetSalesOrderLabelAsync();

            ViewData["SalesOrderPluralLabel"] =
                await _settingService.GetSalesOrderPluralLabelAsync();
        }

        // =========================================================
        // 1. DASHBOARD
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            await LoadSalesOrderSettingsAsync();

            var salesOrderEnabled =
                (bool)ViewData["SalesOrderEnabled"]!;

            var model =
                new DashboardReportViewModel
                {
                    TotalProducts =
                        await _context.Products
                            .CountAsync(x =>
                                !x.IsDeleted),

                    TotalSuppliers =
                        await _context.Suppliers
                            .CountAsync(x =>
                                !x.IsDeleted),

                    TotalCustomers =
                        await _context.Customers
                            .CountAsync(),

                    TotalWarehouses =
                        await _context.Warehouses
                            .CountAsync(x =>
                                !x.IsDeleted)
                };

            if (salesOrderEnabled)
            {
                // Sales Order calculations can be added here
                // when DashboardReportViewModel contains them.
            }

            return View(model);
        }

        // =========================================================
        // 2. PURCHASE REPORT
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Purchases(
            DateTime? fromDate,
            DateTime? toDate,
            int? supplierId,
            string? status)
        {
            await LoadOrganisationSettingsAsync();

            var query =
                _context.PurchaseOrders
                    .Include(x => x.Supplier)
                    .Where(x =>
                        !x.IsDeleted)
                    .AsQueryable();

            if (fromDate.HasValue)
            {
                query =
                    query.Where(x =>
                        x.OrderDate.Date >=
                        fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                query =
                    query.Where(x =>
                        x.OrderDate.Date <=
                        toDate.Value.Date);
            }

            if (supplierId.HasValue &&
                supplierId.Value > 0)
            {
                query =
                    query.Where(x =>
                        x.SupplierId ==
                        supplierId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query =
                    query.Where(x =>
                        x.Status.ToString() ==
                        status);
            }

            var orders =
                await query
                    .OrderByDescending(x =>
                        x.OrderDate)
                    .ToListAsync();

            var model =
                new PurchaseReportViewModel
                {
                    FromDate =
                        fromDate,

                    ToDate =
                        toDate,

                    SupplierId =
                        supplierId,

                    Status =
                        status,

                    TotalOrders =
                        orders.Count,

                    TotalPurchase =
                        orders.Sum(x =>
                            x.GrandTotal),

                    TotalTax =
                        orders.Sum(x =>
                            x.TaxAmount),

                    TotalDiscount =
                        orders.Sum(x =>
                            x.Discount),

                    Orders =
                        orders
                            .Select(x =>
                                new PurchaseReportRowViewModel
                                {
                                    Id =
                                        x.Id,

                                    PONumber =
                                        x.PONumber,

                                    OrderDate =
                                        x.OrderDate,

                                    ExpectedDate =
                                        x.ExpectedDate,

                                    SupplierName =
                                        x.Supplier?.Name ??
                                        "Unknown",

                                    SubTotal =
                                        x.SubTotal,

                                    Discount =
                                        x.Discount,

                                    TaxAmount =
                                        x.TaxAmount,

                                    GrandTotal =
                                        x.GrandTotal,

                                    Status =
                                        x.Status.ToString()
                                })
                            .ToList(),

                    Suppliers =
                        await _context.Suppliers
                            .Where(x =>
                                !x.IsDeleted)
                            .OrderBy(x =>
                                x.Name)
                            .Select(x =>
                                new SupplierReportFilterViewModel
                                {
                                    Id =
                                        x.Id,

                                    Name =
                                        x.Name
                                })
                            .ToListAsync()
                };

            return View(model);
        }

        // =========================================================
        // 3. SALES REPORT
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Sales(
            DateTime? fromDate,
            DateTime? toDate)
        {
            // ---------------------------------------------------------
            // LOAD SALES ORDER SETTINGS
            // ---------------------------------------------------------

            await LoadSalesOrderSettingsAsync();

            var salesOrderEnabled =
                (bool)ViewData["SalesOrderEnabled"]!;

            // ---------------------------------------------------------
            // IF SALES ORDER MODULE IS OFF
            // ---------------------------------------------------------

            if (!salesOrderEnabled)
            {
                TempData["Error"] =
                    "The Sales Order module is currently disabled.";

                return RedirectToAction(nameof(Dashboard));
            }

            // ---------------------------------------------------------
            // DATE RANGE
            // ---------------------------------------------------------

            var today = DateTime.Today;

            var startDate =
                fromDate?.Date
                ?? new DateTime(
                    today.Year,
                    today.Month,
                    1);

            var endDate =
                toDate?.Date
                ?? today;

            if (startDate > endDate)
            {
                endDate = startDate;
            }

            var endDateExclusive =
                endDate.AddDays(1);

            // ---------------------------------------------------------
            // LOAD ONLY CONFIRMED SALES
            //
            // Draft     = NOT included
            // Confirmed = INCLUDED
            // Cancelled = NOT included
            // Deleted   = NOT included
            // ---------------------------------------------------------

            var orders =
                await _context.SalesOrders
                    .AsNoTracking()
                    .Include(x => x.SalesOrderItems)
                    .Where(x =>
                        !x.IsDeleted &&
                        x.Status == SalesOrderStatus.Confirmed &&
                        x.OrderDate >= startDate &&
                        x.OrderDate < endDateExclusive)
                    .OrderByDescending(x => x.OrderDate)
                    .ToListAsync();

            // ---------------------------------------------------------
            // TOTALS
            // ---------------------------------------------------------

            decimal totalSales = 0m;
            decimal totalDiscount = 0m;
            decimal netSales = 0m;
            decimal totalTax = 0m;
            decimal grandTotal = 0m;
            decimal costOfGoodsSold = 0m;
            decimal grossProfit = 0m;

            foreach (var order in orders)
            {
                foreach (var item in order.SalesOrderItems)
                {
                    // -------------------------------------------------
                    // GROSS SALES
                    // -------------------------------------------------

                    decimal grossSales =
                        item.Quantity *
                        item.SalePrice;

                    // -------------------------------------------------
                    // DISCOUNT
                    // -------------------------------------------------

                    decimal discount =
                        grossSales *
                        item.DiscountPercent /
                        100m;

                    // -------------------------------------------------
                    // NET SALES
                    // -------------------------------------------------

                    decimal itemNetSales =
                        Math.Max(
                            grossSales - discount,
                            0m);

                    // -------------------------------------------------
                    // TAX
                    //
                    // Tax is calculated after discount.
                    // -------------------------------------------------

                    decimal tax =
                        itemNetSales *
                        item.TaxPercent /
                        100m;

                    // -------------------------------------------------
                    // GRAND TOTAL
                    // -------------------------------------------------

                    decimal itemGrandTotal =
                        itemNetSales +
                        tax;

                    // -------------------------------------------------
                    // COGS
                    //
                    // UnitPrice = actual purchase/GRN cost
                    // stored when the Sale was created.
                    // -------------------------------------------------

                    decimal itemCOGS =
                        item.Quantity *
                        item.UnitPrice;

                    // -------------------------------------------------
                    // GROSS PROFIT
                    //
                    // Tax is NOT included in gross profit.
                    // -------------------------------------------------

                    decimal itemGrossProfit =
                        itemNetSales -
                        itemCOGS;

                    totalSales += grossSales;
                    totalDiscount += discount;
                    netSales += itemNetSales;
                    totalTax += tax;
                    grandTotal += itemGrandTotal;
                    costOfGoodsSold += itemCOGS;
                    grossProfit += itemGrossProfit;
                }
            }

            // ---------------------------------------------------------
            // GROSS MARGIN
            // ---------------------------------------------------------

            decimal grossMarginPercent =
                netSales > 0m
                    ? (grossProfit / netSales) * 100m
                    : 0m;

            // ---------------------------------------------------------
            // ORGANISATION / REPORT SETTINGS
            // ---------------------------------------------------------

            var model =
                new SalesReportViewModel
                {
                    FromDate =
                        startDate,

                    ToDate =
                        endDate,

                    // Gross sales before discount
                    TotalSales =
                        Math.Round(
                            totalSales,
                            2),

                    TotalDiscount =
                        Math.Round(
                            totalDiscount,
                            2),

                    // Net sales after discount,
                    // before tax
                    NetSales =
                        Math.Round(
                            netSales,
                            2),

                    TotalTax =
                        Math.Round(
                            totalTax,
                            2),

                    // Net Sales + Tax
                    GrandTotal =
                        Math.Round(
                            grandTotal,
                            2),

                    // Purchase cost of confirmed sales
                    CostOfGoodsSold =
                        Math.Round(
                            costOfGoodsSold,
                            2),

                    // Net Sales - COGS
                    GrossProfit =
                        Math.Round(
                            grossProfit,
                            2),

                    GrossMarginPercent =
                        Math.Round(
                            grossMarginPercent,
                            2),

                    TotalInvoices =
                        orders.Count,

                    OrganisationName =
                        await _settingService.GetOrganisationNameAsync(),

                    Branch =
                        await _settingService.GetBranchAsync(),

                    Address =
                        await _settingService.GetAddressAsync(),

                    Phone =
                        await _settingService.GetPhoneAsync(),

                    Email =
                        await _settingService.GetEmailAsync(),

                    GSTIN =
                        await _settingService.GetGSTINAsync(),

                    ReportHeading =
                        await _settingService.GetReportHeadingAsync(),

                    FromDateLabel =
                        await _settingService.GetFromDateLabelAsync(),

                    ToDateLabel =
                        await _settingService.GetToDateLabelAsync(),

                    AuthorisedSignatory =
                        await _settingService.GetAuthorisedSignatoryAsync(),

                    AuthorisedDesignation =
                        await _settingService.GetAuthorisedDesignationAsync()
                };

            return View(model);
        }
        // =========================================================
        // 4. STOCK REPORT
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Stock(
            int? productId,
            int? warehouseId,
            string? stockStatus)
        {
            await LoadOrganisationSettingsAsync();

            var model =
                new StockReportViewModel
                {
                    ProductId =
                        productId,

                    WarehouseId =
                        warehouseId,

                    StockStatus =
                        stockStatus,

                    // =================================================
                    // ORGANISATION SETTINGS
                    // =================================================

                    OrganisationName =
                        await _settingService
                            .GetOrganisationNameAsync(),

                    Branch =
                        await _settingService
                            .GetBranchAsync(),

                    Address =
                        await _settingService
                            .GetAddressAsync(),

                    Phone =
                        await _settingService
                            .GetPhoneAsync(),

                    Email =
                        await _settingService
                            .GetEmailAsync(),

                    GSTIN =
                        await _settingService
                            .GetGSTINAsync(),

                    // =================================================
                    // REPORT SETTINGS
                    // =================================================

                    ReportHeading =
                        await _settingService
                            .GetReportHeadingAsync(),

                    FromDateLabel =
                        await _settingService
                            .GetFromDateLabelAsync(),

                    ToDateLabel =
                        await _settingService
                            .GetToDateLabelAsync(),

                    // =================================================
                    // AUTHORISATION / SIGNATURE
                    // =================================================

                    AuthorisedSignatory =
                        await _settingService
                            .GetAuthorisedSignatoryAsync(),

                    AuthorisedDesignation =
                        await _settingService
                            .GetAuthorisedDesignationAsync()
                };

            // =========================================================
            // STOCK QUERY
            // =========================================================

            var stockQuery =
                _context.WarehouseStocks
                    .AsNoTracking()
                    .Include(x => x.Product)
                    .Include(x => x.Warehouse)
                    .Where(x =>
                        x.Product != null &&
                        !x.Product.IsDeleted &&
                        x.Warehouse != null &&
                        !x.Warehouse.IsDeleted)
                    .AsQueryable();

            if (productId.HasValue &&
                productId.Value > 0)
            {
                stockQuery =
                    stockQuery.Where(x =>
                        x.ProductId ==
                        productId.Value);
            }

            if (warehouseId.HasValue &&
                warehouseId.Value > 0)
            {
                stockQuery =
                    stockQuery.Where(x =>
                        x.WarehouseId ==
                        warehouseId.Value);
            }

            var stocks =
                await stockQuery
                    .OrderBy(x =>
                        x.Product!.SKU)
                    .ThenBy(x =>
                        x.Warehouse!.Name)
                    .ToListAsync();

            // =========================================================
            // STOCK LEDGER
            // =========================================================

            var ledgerQuery =
                _context.StockLedgers
                    .AsNoTracking()
                    .AsQueryable();

            if (productId.HasValue &&
                productId.Value > 0)
            {
                ledgerQuery =
                    ledgerQuery.Where(x =>
                        x.ProductId ==
                        productId.Value);
            }

            if (warehouseId.HasValue &&
                warehouseId.Value > 0)
            {
                ledgerQuery =
                    ledgerQuery.Where(x =>
                        x.WarehouseId ==
                        warehouseId.Value);
            }

            var ledgerEntries =
                await ledgerQuery
                    .OrderBy(x =>
                        x.TransactionDate)
                    .ThenBy(x =>
                        x.Id)
                    .ToListAsync();

            // =========================================================
            // GRN / PURCHASE RECEIPTS
            // =========================================================

            var grnItems =
                await _context.GoodsReceiptItems
                    .AsNoTracking()
                    .Include(x =>
                        x.GoodsReceipt)
                    .Where(x =>
                        x.GoodsReceipt != null &&
                        !x.GoodsReceipt.IsDeleted &&
                        x.GoodsReceipt.Status ==
                            GoodsReceiptStatus.Received &&
                        x.ReceivedQuantity > 0)
                    .ToListAsync();

            // =========================================================
            // REPORT ITEMS
            // =========================================================

            var reportItems =
                new List<StockReportItemViewModel>();

            foreach (var stock in stocks)
            {
                if (stock.Product == null ||
                    stock.Warehouse == null)
                {
                    continue;
                }

                var productLedger =
                    ledgerEntries
                        .Where(x =>
                            x.ProductId ==
                                stock.ProductId &&
                            x.WarehouseId ==
                                stock.WarehouseId)
                        .ToList();

                // =====================================================
                // OPENING STOCK
                // =====================================================

                decimal openingStock =
                    productLedger
                        .Where(x =>
                            x.TransactionType ==
                            StockTransactionType.OpeningStock)
                        .Sum(x =>
                            x.Quantity);

                // =====================================================
                // PURCHASE RECEIPT
                // =====================================================

                decimal purchaseReceiptQuantity =
                    productLedger
                        .Where(x =>
                            x.TransactionType ==
                            StockTransactionType.PurchaseReceipt)
                        .Sum(x =>
                            x.Quantity);

                // =====================================================
                // SALES ISSUE
                // =====================================================

                decimal salesIssueQuantity =
                    productLedger
                        .Where(x =>
                            x.TransactionType ==
                            StockTransactionType.SalesIssue)
                        .Sum(x =>
                            x.Quantity);

                // =====================================================
                // SALES RETURN
                // =====================================================

                decimal salesReturnQuantity =
                    productLedger
                        .Where(x =>
                            x.TransactionType ==
                            StockTransactionType.SalesReturn)
                        .Sum(x =>
                            x.Quantity);

                // =====================================================
                // PURCHASE RETURN
                // =====================================================

                decimal purchaseReturnQuantity =
                    productLedger
                        .Where(x =>
                            x.TransactionType ==
                            StockTransactionType.PurchaseReturn)
                        .Sum(x =>
                            x.Quantity);

                // =====================================================
                // TRANSFER IN
                // =====================================================

                decimal transferInQuantity =
                    productLedger
                        .Where(x =>
                            x.TransactionType ==
                            StockTransactionType.StockTransferIn)
                        .Sum(x =>
                            x.Quantity);

                // =====================================================
                // TRANSFER OUT
                // =====================================================

                decimal transferOutQuantity =
                    productLedger
                        .Where(x =>
                            x.TransactionType ==
                            StockTransactionType.StockTransferOut)
                        .Sum(x =>
                            x.Quantity);

                // =====================================================
                // STOCK ADJUSTMENT
                // =====================================================

                decimal adjustmentQuantity =
                    productLedger
                        .Where(x =>
                            x.TransactionType ==
                            StockTransactionType.StockAdjustment)
                        .Sum(x =>
                            x.Quantity);

                // =====================================================
                // RECEIVED QUANTITY
                // =====================================================

                decimal receivedQuantity =
                    purchaseReceiptQuantity +
                    salesReturnQuantity +
                    transferInQuantity +
                    Math.Max(
                        0,
                        adjustmentQuantity);

                // =====================================================
                // SOLD QUANTITY
                // =====================================================

                decimal soldQuantity =
                    salesIssueQuantity;

                // =====================================================
                // LEDGER BALANCE
                // =====================================================

                decimal ledgerBalance =
                    openingStock +
                    purchaseReceiptQuantity +
                    salesReturnQuantity +
                    transferInQuantity -
                    salesIssueQuantity -
                    purchaseReturnQuantity -
                    transferOutQuantity +
                    adjustmentQuantity;

                // =====================================================
                // PURCHASE / GRN COST
                // =====================================================

                var purchases =
                    grnItems
                        .Where(x =>
                            x.ProductId ==
                                stock.ProductId &&
                            x.WarehouseId ==
                                stock.WarehouseId)
                        .ToList();

                decimal totalReceivedQuantity =
                    purchases.Sum(x =>
                        x.ReceivedQuantity);

                decimal totalPurchaseCost =
                    purchases.Sum(x =>
                        x.ReceivedQuantity *
                        x.UnitPrice);

                decimal purchasePrice =
                    totalReceivedQuantity > 0
                        ? totalPurchaseCost /
                          totalReceivedQuantity
                        : 0m;

                // =====================================================
                // CURRENT STOCK
                // =====================================================

                decimal quantityOnHand =
                    stock.QuantityOnHand;

                decimal reorderLevel =
                    stock.MinimumStock;

                // =====================================================
                // STOCK STATUS
                // =====================================================

                string status;

                if (quantityOnHand <= 0)
                {
                    status =
                        "OutOfStock";
                }
                else if (quantityOnHand <=
                         reorderLevel)
                {
                    status =
                        "LowStock";
                }
                else
                {
                    status =
                        "InStock";
                }

                // =====================================================
                // STOCK VALUE
                // =====================================================

                decimal stockValue =
                    quantityOnHand *
                    purchasePrice;

                // =====================================================
                // ADD REPORT ROW
                // =====================================================

                reportItems.Add(
                    new StockReportItemViewModel
                    {
                        Id =
                            stock.Id,

                        ProductId =
                            stock.ProductId,

                        ProductName =
                            stock.Product.SKU,

                        SKU =
                            stock.Product.SKU,

                        Barcode =
                            stock.Product.Barcode,

                        WarehouseId =
                            stock.WarehouseId,

                        WarehouseName =
                            stock.Warehouse.Name,

                        OpeningStock =
                            openingStock,

                        ReceivedQuantity =
                            receivedQuantity,

                        SoldQuantity =
                            soldQuantity,

                        QuantityOnHand =
                            quantityOnHand,

                        ReorderLevel =
                            reorderLevel,

                        PurchasePrice =
                            purchasePrice,

                        StockValue =
                            stockValue,

                        StockStatus =
                            status
                    });
            }

            // =========================================================
            // STOCK STATUS FILTER
            // =========================================================

            if (!string.IsNullOrWhiteSpace(stockStatus))
            {
                reportItems =
                    reportItems
                        .Where(x =>
                            x.StockStatus ==
                            stockStatus)
                        .ToList();
            }

            // =========================================================
            // SUMMARY
            // =========================================================

            model.TotalProducts =
                reportItems
                    .Select(x =>
                        x.ProductId)
                    .Distinct()
                    .Count();

            model.TotalQuantity =
                reportItems.Sum(x =>
                    x.QuantityOnHand);

            model.TotalStockValue =
                reportItems.Sum(x =>
                    x.StockValue);

            model.LowStockProducts =
                reportItems.Count(x =>
                    x.StockStatus ==
                    "LowStock");

            model.OutOfStockProducts =
                reportItems.Count(x =>
                    x.StockStatus ==
                    "OutOfStock");

            model.Stocks =
                reportItems;

            return View(model);
        }

        // =========================================================
        // 5. INVENTORY VALUATION
        // =========================================================

        [HttpGet("/InventoryValuation")]
public async Task<IActionResult> InventoryValuation(
    int? mainCategoryId,
    int? categoryId,
    int? productId,
    int? warehouseId,
    DateTime? fromDate,
    DateTime? toDate)
        {
            await LoadOrganisationSettingsAsync();

            var model =
                new InventoryValuationViewModel
                {
                    MainCategoryId =
                        mainCategoryId,

                    CategoryId =
                        categoryId,

                    ProductId =
                        productId,

                    WarehouseId =
                        warehouseId,

                    FromDate =
                        fromDate,

                    ToDate =
                        toDate
                };

            // =====================================================
            // MAIN CATEGORY DROPDOWN
            // =====================================================

            model.MainCategories =
                await _context.MainCategories
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted)
                    .OrderBy(x =>
                        x.Name)
                    .Select(x =>
                        new InventoryMainCategoryFilterViewModel
                        {
                            Id =
                                x.Id,

                            Name =
                                x.Name
                        })
                    .ToListAsync();

            // =====================================================
            // CATEGORY DROPDOWN
            // =====================================================

            model.Categories =
                await _context.Categories
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted)
                    .OrderBy(x =>
                        x.Name)
                    .Select(x =>
                        new InventoryCategoryFilterViewModel
                        {
                            Id =
                                x.Id,

                            Name =
                                x.Name,

                            MainCategoryId =
                                x.MainCategoryId
                        })
                    .ToListAsync();

            // =====================================================
            // PRODUCT DROPDOWN
            // =====================================================

            model.Products =
                await _context.Products
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted)
                    .OrderBy(x =>
                        x.SKU)
                    .Select(x =>
                        new InventoryProductFilterViewModel
                        {
                            Id =
                                x.Id,

                            Name =
                                x.SKU
                        })
                    .ToListAsync();

            // =====================================================
            // WAREHOUSE DROPDOWN
            // =====================================================

            model.Warehouses =
                await _context.Warehouses
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted)
                    .OrderBy(x =>
                        x.Name)
                    .Select(x =>
                        new InventoryWarehouseFilterViewModel
                        {
                            Id =
                                x.Id,

                            Name =
                                x.Name
                        })
                    .ToListAsync();

            // =====================================================
            // LOAD MAIN CATEGORIES
            // =====================================================

            var mainCategories =
                await _context.MainCategories
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted)
                    .ToDictionaryAsync(
                        x => x.Id,
                        x => x.Name);

            // =====================================================
            // LOAD CATEGORIES
            // =====================================================

            var categories =
                await _context.Categories
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted)
                    .ToDictionaryAsync(
                        x => x.Id,
                        x => new
                        {
                            x.Id,
                            x.Name,
                            x.MainCategoryId
                        });

            // =====================================================
            // LOAD SUBCATEGORIES
            // =====================================================

            var subCategories =
                await _context.SubCategories
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted)
                    .ToDictionaryAsync(
                        x => x.Id,
                        x => new
                        {
                            x.Name,
                            x.CategoryId
                        });

            // =====================================================
            // WAREHOUSE STOCK QUERY
            // =====================================================

            var stockQuery =
                _context.WarehouseStocks
                    .AsNoTracking()
                    .Include(x => x.Product)
                    .Include(x => x.Warehouse)
                    .Where(x =>
                        x.Product != null &&
                        !x.Product.IsDeleted &&
                        x.Warehouse != null &&
                        !x.Warehouse.IsDeleted)
                    .AsQueryable();

            // =====================================================
            // PRODUCT FILTER
            // =====================================================

            if (productId.HasValue &&
                productId.Value > 0)
            {
                stockQuery =
                    stockQuery.Where(x =>
                        x.ProductId ==
                        productId.Value);
            }

            // =====================================================
            // WAREHOUSE FILTER
            // =====================================================

            if (warehouseId.HasValue &&
                warehouseId.Value > 0)
            {
                stockQuery =
                    stockQuery.Where(x =>
                        x.WarehouseId ==
                        warehouseId.Value);
            }

            // =====================================================
            // MAIN CATEGORY FILTER
            // =====================================================

            if (mainCategoryId.HasValue &&
                mainCategoryId.Value > 0)
            {
                var mainCategoryProductIds =
                    await _context.Products
                        .AsNoTracking()
                        .Where(p =>
                            !p.IsDeleted &&
                            _context.Categories.Any(c =>
                                c.Id ==
                                    p.CategoryId &&
                                !c.IsDeleted &&
                                c.MainCategoryId ==
                                    mainCategoryId.Value))
                        .Select(p =>
                            p.Id)
                        .ToListAsync();

                stockQuery =
                    stockQuery.Where(x =>
                        mainCategoryProductIds
                            .Contains(x.ProductId));
            }

            // =====================================================
            // CATEGORY FILTER
            // =====================================================

            if (categoryId.HasValue &&
                categoryId.Value > 0)
            {
                stockQuery =
                    stockQuery.Where(x =>
                        x.Product!.CategoryId ==
                        categoryId.Value);
            }

            // =====================================================
            // GET STOCK
            // =====================================================

            var stocks =
                await stockQuery
                    .OrderBy(x =>
                        x.Product!.SKU)
                    .ThenBy(x =>
                        x.Warehouse!.Name)
                    .ToListAsync();

            // =====================================================
            // LOAD GRN ITEMS
            // =====================================================

            var grnItemsQuery =
                _context.GoodsReceiptItems
                    .AsNoTracking()
                    .Include(x =>
                        x.GoodsReceipt)
                    .Where(x =>
                        x.GoodsReceipt != null &&
                        !x.GoodsReceipt.IsDeleted &&
                        x.GoodsReceipt.Status ==
                            GoodsReceiptStatus.Received &&
                        x.ReceivedQuantity > 0)
                    .AsQueryable();

            // =====================================================
            // FROM DATE FILTER
            // =====================================================

            if (fromDate.HasValue)
            {
                var fromDateValue =
                    fromDate.Value.Date;

                grnItemsQuery =
                    grnItemsQuery.Where(x =>
                        x.GoodsReceipt!.ReceiptDate >=
                        fromDateValue);
            }

            // =====================================================
            // TO DATE FILTER
            // =====================================================

            if (toDate.HasValue)
            {
                var toDateValue =
                    toDate.Value.Date.AddDays(1);

                grnItemsQuery =
                    grnItemsQuery.Where(x =>
                        x.GoodsReceipt!.ReceiptDate <
                        toDateValue);
            }

            var grnItems =
                await grnItemsQuery
                    .ToListAsync();

            // =====================================================
            // BUILD INVENTORY VALUATION
            // =====================================================

            foreach (var stock in stocks)
            {
                if (stock.Product == null ||
                    stock.Warehouse == null)
                {
                    continue;
                }

                var product =
                    stock.Product;

                int mainCategoryIdValue =
                    0;

                string mainCategoryName =
                    "-";

                int categoryIdValue =
                    product.CategoryId;

                string categoryName =
                    "-";

                int subCategoryIdValue =
                    product.SubCategoryId;

                string subCategoryName =
                    "-";

                if (subCategories.TryGetValue(
                        product.SubCategoryId,
                        out var subCategory))
                {
                    subCategoryName =
                        subCategory.Name;

                    if (categories.TryGetValue(
                            subCategory.CategoryId,
                            out var categoryFromSubCategory))
                    {
                        categoryIdValue =
                            categoryFromSubCategory.Id;

                        categoryName =
                            categoryFromSubCategory.Name;

                        mainCategoryIdValue =
                            categoryFromSubCategory
                                .MainCategoryId;

                        if (mainCategories.TryGetValue(
                                mainCategoryIdValue,
                                out var mainCategoryNameValue))
                        {
                            mainCategoryName =
                                mainCategoryNameValue;
                        }
                    }
                }

                if (categoryName == "-" &&
                    categories.TryGetValue(
                        product.CategoryId,
                        out var directCategory))
                {
                    categoryIdValue =
                        directCategory.Id;

                    categoryName =
                        directCategory.Name;

                    mainCategoryIdValue =
                        directCategory.MainCategoryId;

                    if (mainCategories.TryGetValue(
                            mainCategoryIdValue,
                            out var directMainCategoryName))
                    {
                        mainCategoryName =
                            directMainCategoryName;
                    }
                }

                var productWarehousePurchases =
                    grnItems
                        .Where(x =>
                            x.ProductId ==
                                stock.ProductId &&
                            x.WarehouseId ==
                                stock.WarehouseId &&
                            x.ReceivedQuantity > 0)
                        .ToList();

                DateTime? receiptDate =
                    productWarehousePurchases
                        .Where(x =>
                            x.GoodsReceipt != null)
                        .Select(x =>
                            (DateTime?)
                            x.GoodsReceipt!.ReceiptDate)
                        .OrderByDescending(x =>
                            x)
                        .FirstOrDefault();

                decimal totalReceivedQuantity =
                    productWarehousePurchases
                        .Sum(x =>
                            x.ReceivedQuantity);

                decimal totalPurchaseCost =
                    productWarehousePurchases
                        .Sum(x =>
                            x.ReceivedQuantity *
                            x.UnitPrice);

                decimal purchasePrice =
                    totalReceivedQuantity > 0
                        ? totalPurchaseCost /
                          totalReceivedQuantity
                        : 0m;

                decimal quantityOnHand =
                    stock.QuantityOnHand;

                decimal inventoryValue =
                    quantityOnHand *
                    purchasePrice;

                model.Items.Add(
                    new InventoryValuationItemViewModel
                    {
                        Id =
                            stock.Id,

                        ProductId =
                            stock.ProductId,

                        ProductName =
                            product.SKU,

                        SKU =
                            product.SKU,

                        Barcode =
                            product.Barcode,

                        MainCategoryId =
                            mainCategoryIdValue,

                        MainCategoryName =
                            mainCategoryName,

                        CategoryId =
                            categoryIdValue,

                        CategoryName =
                            categoryName,

                        SubCategoryId =
                            subCategoryIdValue,

                        SubCategoryName =
                            subCategoryName,

                        WarehouseId =
                            stock.WarehouseId,

                        WarehouseName =
                            stock.Warehouse.Name,

                        ReceiptDate =
                            receiptDate,

                        QuantityOnHand =
                            quantityOnHand,

                        PurchasePrice =
                            purchasePrice,

                        InventoryValue =
                            inventoryValue
                    });
            }

            // =====================================================
            // SUMMARY
            // =====================================================

            model.TotalProducts =
                model.Items
                    .Select(x =>
                        x.ProductId)
                    .Distinct()
                    .Count();

            model.TotalQuantity =
                model.Items.Sum(x =>
                    x.QuantityOnHand);

            model.TotalInventoryValue =
                model.Items.Sum(x =>
                    x.InventoryValue);

            model.TotalPurchaseCost =
                model.Items.Sum(x =>
                    x.QuantityOnHand *
                    x.PurchasePrice);

            return View(model);
        }



        // =========================================================
        // 6. LOW STOCK REPORT
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> LowStock(
            int? productId,
            int? warehouseId,
            int? categoryId)
        {
            // =========================================================
            // LOAD ORGANISATION / REPORT SETTINGS
            // =========================================================

            await LoadOrganisationSettingsAsync();

            // =========================================================
            // BASE STOCK QUERY
            // =========================================================

            var query =
                _context.WarehouseStocks
                    .AsNoTracking()
                    .Include(x => x.Product)
                        .ThenInclude(x => x.Category)
                    .Include(x => x.Warehouse)
                    .Where(x =>
                        x.Product != null &&
                        !x.Product.IsDeleted &&
                        x.Warehouse != null &&
                        !x.Warehouse.IsDeleted)
                    .AsQueryable();

            // =========================================================
            // PRODUCT FILTER
            // =========================================================

            if (productId.HasValue &&
                productId.Value > 0)
            {
                query =
                    query.Where(x =>
                        x.ProductId ==
                        productId.Value);
            }

            // =========================================================
            // WAREHOUSE FILTER
            // =========================================================

            if (warehouseId.HasValue &&
                warehouseId.Value > 0)
            {
                query =
                    query.Where(x =>
                        x.WarehouseId ==
                        warehouseId.Value);
            }

            // =========================================================
            // CATEGORY FILTER
            // =========================================================

            if (categoryId.HasValue &&
                categoryId.Value > 0)
            {
                query =
                    query.Where(x =>
                        x.Product != null &&
                        x.Product.CategoryId ==
                        categoryId.Value);
            }

            // =========================================================
            // LOW STOCK CONDITION
            //
            // Current Stock <= Reorder / Minimum Stock
            //
            // Example:
            // Current = 5
            // Minimum = 10
            // Result = Low Stock
            //
            // Current = 0
            // Minimum = 10
            // Result = Out of Stock
            // =========================================================

            query =
                query.Where(x =>
                    x.QuantityOnHand <=
                    x.MinimumStock);

            // =========================================================
            // GET STOCK RECORDS
            // =========================================================

            var stocks =
                await query
                    .OrderBy(x =>
                        x.Product!.SKU)
                    .ThenBy(x =>
                        x.Warehouse!.Name)
                    .ToListAsync();

            // =========================================================
            // CREATE REPORT MODEL
            // =========================================================

            var model =
                new LowStockReportViewModel
                {
                    // =====================================================
                    // FILTER VALUES
                    // =====================================================

                    ProductId =
                        productId,

                    WarehouseId =
                        warehouseId,

                    CategoryId =
                        categoryId,

                    // =====================================================
                    // SUMMARY
                    // =====================================================

                    TotalLowStockProducts =
                        stocks
                            .Select(x =>
                                x.ProductId)
                            .Distinct()
                            .Count(),

                    TotalCurrentStock =
                        stocks.Sum(x =>
                            x.QuantityOnHand),

                    TotalReorderLevel =
                        stocks.Sum(x =>
                            x.MinimumStock),

                    TotalShortageQuantity =
                        stocks.Sum(x =>
                            Math.Max(
                                0,
                                x.MinimumStock -
                                x.QuantityOnHand)),

                    // =====================================================
                    // ORGANISATION DETAILS
                    // =====================================================

                    OrganisationName =
                        await _settingService
                            .GetOrganisationNameAsync(),

                    Branch =
                        await _settingService
                            .GetBranchAsync(),

                    Address =
                        await _settingService
                            .GetAddressAsync(),

                    Phone =
                        await _settingService
                            .GetPhoneAsync(),

                    Email =
                        await _settingService
                            .GetEmailAsync(),

                    GSTIN =
                        await _settingService
                            .GetGSTINAsync(),

                    // =====================================================
                    // REPORT SETTINGS
                    // =====================================================

                    ReportHeading =
                        await _settingService
                            .GetReportHeadingAsync(),

                    FromDateLabel =
                        await _settingService
                            .GetFromDateLabelAsync(),

                    ToDateLabel =
                        await _settingService
                            .GetToDateLabelAsync(),

                    // =====================================================
                    // AUTHORISED SIGNATURE
                    // =====================================================

                    AuthorisedSignatory =
                        await _settingService
                            .GetAuthorisedSignatoryAsync(),

                    AuthorisedDesignation =
                        await _settingService
                            .GetAuthorisedDesignationAsync()
                };

            // =========================================================
            // BUILD REPORT ITEMS
            // =========================================================

            foreach (var stock in stocks)
            {
                var product =
                    stock.Product;

                if (product == null)
                {
                    continue;
                }

                // =====================================================
                // SHORTAGE
                // =====================================================

                var shortageQuantity =
                    Math.Max(
                        0,
                        stock.MinimumStock -
                        stock.QuantityOnHand);

                // =====================================================
                // STOCK STATUS
                // =====================================================

                string stockStatus;

                if (stock.QuantityOnHand <= 0)
                {
                    stockStatus =
                        "Out of Stock";
                }
                else
                {
                    stockStatus =
                        "Low Stock";
                }

                // =====================================================
                // UNIT NAME
                //
                // Product.Unit may vary depending on your entity model.
                // Keep N/A here so this report does not break if Unit
                // navigation is not available.
                // =====================================================

                string unitName =
                    "N/A";

                // =====================================================
                // ADD REPORT ITEM
                // =====================================================

                model.Items.Add(
                    new LowStockItemViewModel
                    {
                        ProductId =
                            stock.ProductId,

                        ProductName =
                            product.SKU ??
                            "N/A",

                        SKU =
                            product.SKU ??
                            "N/A",

                        Barcode =
                            product.Barcode ??
                            "N/A",

                        CategoryName =
                            product.Category?.Name
                            ?? "N/A",

                        WarehouseName =
                            stock.Warehouse?.Name
                            ?? "N/A",

                        UnitName =
                            unitName,

                        CurrentStock =
                            stock.QuantityOnHand,

                        ReorderLevel =
                            stock.MinimumStock,

                        ShortageQuantity =
                            shortageQuantity,

                        SuggestedOrderQuantity =
                            shortageQuantity,

                        StockStatus =
                            stockStatus
                    });
            }

            // =========================================================
            // LOAD DROPDOWNS
            // =========================================================

            await LoadLowStockDropdowns(model);

            // =========================================================
            // RETURN VIEW
            //
            // This resolves to:
            //
            // Views/Reports/LowStock.cshtml
            // =========================================================

            return View(model);
        }


        // =========================================================
        // LOW STOCK DROPDOWNS
        // =========================================================

        private async Task LoadLowStockDropdowns(
            LowStockReportViewModel model)
        {
            // =========================================================
            // PRODUCTS
            // =========================================================

            model.Products =
                await _context.Products
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted)
                    .OrderBy(x =>
                        x.SKU)
                    .Select(x =>
                        new SelectListItem
                        {
                            Value =
                                x.Id.ToString(),

                            Text =
                                x.SKU
                        })
                    .ToListAsync();

            // =========================================================
            // WAREHOUSES
            // =========================================================

            model.Warehouses =
                await _context.Warehouses
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted)
                    .OrderBy(x =>
                        x.Name)
                    .Select(x =>
                        new SelectListItem
                        {
                            Value =
                                x.Id.ToString(),

                            Text =
                                x.Name
                        })
                    .ToListAsync();

            // =========================================================
            // CATEGORIES
            // =========================================================

            model.Categories =
                await _context.Categories
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted)
                    .OrderBy(x =>
                        x.Name)
                    .Select(x =>
                        new SelectListItem
                        {
                            Value =
                                x.Id.ToString(),

                            Text =
                                x.Name
                        })
                    .ToListAsync();
        }

        // =========================================================
        // 7. STOCK TRANSFER REPORT
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> StockTransfer(
            DateTime? fromDate,
            DateTime? toDate)
        {
            await LoadOrganisationSettingsAsync();

            var today =
                DateTime.Today;

            var startDate =
                fromDate?.Date ??
                today;

            var endDate =
                toDate?.Date ??
                today;

            if (startDate > endDate)
            {
                endDate =
                    startDate;
            }

            var startDateTime =
                startDate.Date;

            var endDateTime =
                endDate.Date.AddDays(1);

            var transferOuts =
                await _context.StockLedgers
                    .AsNoTracking()
                    .Include(x =>
                        x.Product)
                    .Include(x =>
                        x.Warehouse)
                    .Where(x =>
                        x.TransactionType ==
                            StockTransactionType.StockTransferOut &&
                        x.TransactionDate >=
                            startDateTime &&
                        x.TransactionDate <
                            endDateTime)
                    .OrderBy(x =>
                        x.TransactionDate)
                    .ThenBy(x =>
                        x.Id)
                    .ToListAsync();

            var transferIns =
                await _context.StockLedgers
                    .AsNoTracking()
                    .Include(x =>
                        x.Product)
                    .Include(x =>
                        x.Warehouse)
                    .Where(x =>
                        x.TransactionType ==
                            StockTransactionType.StockTransferIn &&
                        x.TransactionDate >=
                            startDateTime &&
                        x.TransactionDate <
                            endDateTime)
                    .OrderBy(x =>
                        x.TransactionDate)
                    .ThenBy(x =>
                        x.Id)
                    .ToListAsync();

            var mainCategories =
                await _context.MainCategories
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted)
                    .ToDictionaryAsync(
                        x => x.Id,
                        x => x.Name);

            var categories =
                await _context.Categories
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted)
                    .ToDictionaryAsync(
                        x => x.Id,
                        x => new
                        {
                            x.Name,
                            x.MainCategoryId
                        });

            var subCategories =
                await _context.SubCategories
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted)
                    .ToDictionaryAsync(
                        x => x.Id,
                        x => new
                        {
                            x.Name,
                            x.CategoryId
                        });

            var reportItems =
                new List<StockTransferReportRowViewModel>();

            foreach (var transferOut in transferOuts)
            {
                var transferIn =
                    transferIns
                        .Where(x =>
                            x.ProductId ==
                                transferOut.ProductId &&
                            x.ReferenceNo ==
                                transferOut.ReferenceNo &&
                            x.WarehouseId !=
                                transferOut.WarehouseId)
                        .OrderBy(x =>
                            x.TransactionDate)
                        .ThenBy(x =>
                            x.Id)
                        .FirstOrDefault();

                if (transferIn == null)
                {
                    continue;
                }

                var product =
                    transferOut.Product;

                if (product == null)
                {
                    continue;
                }

                string mainCategoryName =
                    "-";

                string categoryName =
                    "-";

                string subcategoryName =
                    "-";

                if (product.CategoryId > 0 &&
                    categories.TryGetValue(
                        product.CategoryId,
                        out var category))
                {
                    categoryName =
                        category.Name;

                    if (mainCategories.TryGetValue(
                            category.MainCategoryId,
                            out var mainCategory))
                    {
                        mainCategoryName =
                            mainCategory;
                    }
                }

                if (product.SubCategoryId > 0 &&
                    subCategories.TryGetValue(
                        product.SubCategoryId,
                        out var subCategory))
                {
                    subcategoryName =
                        subCategory.Name;

                    if (categories.TryGetValue(
                            subCategory.CategoryId,
                            out var categoryFromSubCategory))
                    {
                        categoryName =
                            categoryFromSubCategory.Name;

                        if (mainCategories.TryGetValue(
                                categoryFromSubCategory.MainCategoryId,
                                out var mainCategoryFromSubCategory))
                        {
                            mainCategoryName =
                                mainCategoryFromSubCategory;
                        }
                    }
                }

                var transferNo =
                    transferOut.ReferenceNo ??
                    "-";

                var productName =
                    product.SKU ??
                    "-";

                var fromWarehouseName =
                    transferOut.Warehouse?.Name ??
                    "-";

                var toWarehouseName =
                    transferIn.Warehouse?.Name ??
                    "-";

                var quantity =
                    Math.Abs(
                        Convert.ToDecimal(
                            transferOut.Quantity));

                var unitCost =
                    Convert.ToDecimal(
                        transferOut.UnitCost);

                // =====================================================
                // OUTWARD
                // =====================================================

                reportItems.Add(
                    new StockTransferReportRowViewModel
                    {
                        ProductId =
                            transferOut.ProductId,

                        TransferNo =
                            transferNo,

                        TransferType =
                            "Outward",

                        TransferDate =
                            transferOut.TransactionDate,

                        ProductName =
                            productName,

                        MainCategoryName =
                            mainCategoryName,

                        CategoryName =
                            categoryName,

                        SubcategoryName =
                            subcategoryName,

                        FromWarehouseId =
                            transferOut.WarehouseId,

                        FromWarehouseName =
                            fromWarehouseName,

                        ToWarehouseId =
                            transferIn.WarehouseId,

                        ToWarehouseName =
                            toWarehouseName,

                        Quantity =
                            quantity,

                        UnitCost =
                            unitCost
                    });

                // =====================================================
                // INWARD
                // =====================================================

                reportItems.Add(
                    new StockTransferReportRowViewModel
                    {
                        ProductId =
                            transferIn.ProductId,

                        TransferNo =
                            transferIn.ReferenceNo ??
                            transferNo,

                        TransferType =
                            "Inward",

                        TransferDate =
                            transferIn.TransactionDate,

                        ProductName =
                            transferIn.Product?.SKU ??
                            productName,

                        MainCategoryName =
                            mainCategoryName,

                        CategoryName =
                            categoryName,

                        SubcategoryName =
                            subcategoryName,

                        FromWarehouseId =
                            transferOut.WarehouseId,

                        FromWarehouseName =
                            fromWarehouseName,

                        ToWarehouseId =
                            transferIn.WarehouseId,

                        ToWarehouseName =
                            toWarehouseName,

                        Quantity =
                            Math.Abs(
                                Convert.ToDecimal(
                                    transferIn.Quantity)),

                        UnitCost =
                            Convert.ToDecimal(
                                transferIn.UnitCost)
                    });
            }

            reportItems =
                reportItems
                    .OrderByDescending(x =>
                        x.TransferDate)
                    .ThenBy(x =>
                        x.TransferNo)
                    .ThenBy(x =>
                        x.ProductName)
                    .ThenBy(x =>
                        x.TransferType == "Outward"
                            ? 0
                            : 1)
                    .ToList();

            // =========================================================
            // BUILD REPORT MODEL
            // =========================================================

            var model =
                new StockTransferReportViewModel
                {
                    FromDate =
                        startDate,

                    ToDate =
                        endDate,

                    Transfers =
                        reportItems,

                    // =================================================
                    // ORGANISATION SETTINGS
                    // =================================================

                    OrganisationName =
                        await _settingService
                            .GetOrganisationNameAsync(),

                    Branch =
                        await _settingService
                            .GetBranchAsync(),

                    Address =
                        await _settingService
                            .GetAddressAsync(),

                    Phone =
                        await _settingService
                            .GetPhoneAsync(),

                    Email =
                        await _settingService
                            .GetEmailAsync(),

                    GSTIN =
                        await _settingService
                            .GetGSTINAsync(),

                    // =================================================
                    // REPORT SETTINGS
                    // =================================================

                    ReportHeading =
                        await _settingService
                            .GetReportHeadingAsync(),

                    FromDateLabel =
                        await _settingService
                            .GetFromDateLabelAsync(),

                    ToDateLabel =
                        await _settingService
                            .GetToDateLabelAsync(),

                    // =================================================
                    // AUTHORISATION / SIGNATURE
                    // =================================================

                    AuthorisedSignatory =
                        await _settingService
                            .GetAuthorisedSignatoryAsync(),

                    AuthorisedDesignation =
                        await _settingService
                            .GetAuthorisedDesignationAsync()
                };

            return View(model);
        }
        // =========================================================
        // 8. PROFIT & LOSS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> ProfitLoss(
            DateTime? fromDate,
            DateTime? toDate)
        {
            // ---------------------------------------------------------
            // LOAD SALES ORDER SETTINGS
            // ---------------------------------------------------------

            await LoadSalesOrderSettingsAsync();

            var salesOrderEnabled =
                ViewData["SalesOrderEnabled"] is bool enabled && enabled;

            // ---------------------------------------------------------
            // DATE RANGE
            // ---------------------------------------------------------

            var today = DateTime.Today;

            var startDate =
                fromDate?.Date
                ?? new DateTime(
                    today.Year,
                    today.Month,
                    1);

            var endDate =
                toDate?.Date
                ?? today;

            // Prevent invalid date range
            if (endDate < startDate)
            {
                endDate = startDate;
            }

            // ---------------------------------------------------------
            // DEFAULT VALUES
            // ---------------------------------------------------------

            decimal totalSales = 0m;
            decimal costOfGoodsSold = 0m;
            decimal grossProfit = 0m;
            decimal expenses = 0m;
            decimal netProfit = 0m;

            // ---------------------------------------------------------
            // SALES / COGS
            // ---------------------------------------------------------

            if (salesOrderEnabled)
            {
                // -----------------------------------------------------
                // ONLY CONFIRMED, NON-DELETED SALES ORDERS
                // -----------------------------------------------------

                var salesOrders =
                    await _context.SalesOrders
                        .AsNoTracking()
                        .Include(x => x.SalesOrderItems)
                        .Where(x =>
                            !x.IsDeleted &&
                            x.Status ==
                                StoreManagementSystem.Domain.Enums.SalesOrderStatus.Confirmed &&
                            x.OrderDate >= startDate &&
                            x.OrderDate < endDate.AddDays(1))
                        .ToListAsync();

                // -----------------------------------------------------
                // CALCULATE SALES / COGS / GROSS PROFIT
                //
                // Sales Order rules:
                //
                // Gross Sales
                //     = Quantity × SalePrice
                //
                // Discount
                //     = Gross Sales × Discount %
                //
                // Net Sales
                //     = Gross Sales - Discount
                //
                // COGS
                //     = Quantity × UnitPrice
                //
                // Gross Profit
                //     = Net Sales - COGS
                //
                // Tax is NOT included in Gross Profit.
                // -----------------------------------------------------

                foreach (var order in salesOrders)
                {
                    foreach (var item in order.SalesOrderItems)
                    {
                        var grossSales =
                            item.Quantity *
                            item.SalePrice;

                        var discountAmount =
                            grossSales *
                            item.DiscountPercent /
                            100m;

                        var netSales =
                            Math.Max(
                                grossSales -
                                discountAmount,
                                0m);

                        var itemCost =
                            item.Quantity *
                            item.UnitPrice;

                        var itemGrossProfit =
                            netSales -
                            itemCost;

                        totalSales += netSales;

                        costOfGoodsSold += itemCost;

                        grossProfit += itemGrossProfit;
                    }
                }

                // -----------------------------------------------------
                // ROUND FINANCIAL VALUES
                // -----------------------------------------------------

                totalSales =
                    Math.Round(
                        totalSales,
                        2,
                        MidpointRounding.AwayFromZero);

                costOfGoodsSold =
                    Math.Round(
                        costOfGoodsSold,
                        2,
                        MidpointRounding.AwayFromZero);

                grossProfit =
                    Math.Round(
                        grossProfit,
                        2,
                        MidpointRounding.AwayFromZero);
            }

            // ---------------------------------------------------------
            // EXPENSES
            // ---------------------------------------------------------
            //
            // Expense module is not implemented yet.
            // Keep this at zero until an Expense module is added.
            // ---------------------------------------------------------

            expenses = 0m;

            // ---------------------------------------------------------
            // NET PROFIT
            // ---------------------------------------------------------

            netProfit =
                grossProfit -
                expenses;

            netProfit =
                Math.Round(
                    netProfit,
                    2,
                    MidpointRounding.AwayFromZero);

            // ---------------------------------------------------------
            // VIEW MODEL
            // ---------------------------------------------------------

            var model =
                new ProfitLossViewModel
                {
                    // -------------------------------------------------
                    // FINANCIAL VALUES
                    // -------------------------------------------------

                    TotalSales =
                        totalSales,

                    CostOfGoodsSold =
                        costOfGoodsSold,

                    GrossProfit =
                        grossProfit,

                    Expenses =
                        expenses,

                    NetProfit =
                        netProfit,

                    // -------------------------------------------------
                    // DATE FILTERS
                    // -------------------------------------------------

                    FromDate =
                        startDate,

                    ToDate =
                        endDate,

                    // -------------------------------------------------
                    // REPORT SETTINGS
                    // -------------------------------------------------

                    OrganisationName =
                        await _settingService
                            .GetOrganisationNameAsync(),

                    Branch =
                        await _settingService
                            .GetBranchAsync(),

                    Address =
                        await _settingService
                            .GetAddressAsync(),

                    Phone =
                        await _settingService
                            .GetPhoneAsync(),

                    Email =
                        await _settingService
                            .GetEmailAsync(),

                    GSTIN =
                        await _settingService
                            .GetGSTINAsync(),

                    ReportHeading =
                        await _settingService
                            .GetReportHeadingAsync(),

                    FromDateLabel =
                        await _settingService
                            .GetFromDateLabelAsync(),

                    ToDateLabel =
                        await _settingService
                            .GetToDateLabelAsync(),

                    AuthorisedSignatory =
                        await _settingService
                            .GetAuthorisedSignatoryAsync(),

                    AuthorisedDesignation =
                        await _settingService
                            .GetAuthorisedDesignationAsync()
                };

            return View(model);
        }

        // =========================================================
        // 9. GST REPORT
        // =========================================================

        // GST report action can be added here.

        // =========================================================
        // 10. CUSTOMER REPORT
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Customers()
        {
            await LoadOrganisationSettingsAsync();

            var model =
                new CustomerReportViewModel
                {
                    TotalCustomers =
                        await _context.Customers
                            .CountAsync()
                };

            return View(model);
        }

        // =========================================================
        // 11. SUPPLIER REPORT
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Suppliers()
        {
            await LoadOrganisationSettingsAsync();

            // =====================================================
            // TOTAL ACTIVE SUPPLIERS
            // =====================================================

            int totalSuppliers =
                await _context.Suppliers
                    .CountAsync(x =>
                        !x.IsDeleted &&
                        x.IsActive);

            // =====================================================
            // TOTAL PURCHASE
            // =====================================================

            decimal totalPurchase =
                await _context.PurchaseOrders
                    .Where(x =>
                        !x.IsDeleted)
                    .SumAsync(x =>
                        x.GrandTotal);

            // =====================================================
            // TOTAL PAID
            // =====================================================

            // Payment module does not exist yet.
            decimal totalPaid =
                0m;

            // =====================================================
            // OUTSTANDING
            // =====================================================

            decimal totalOutstanding =
                totalPurchase -
                totalPaid;

            // =====================================================
            // VIEW MODEL
            // =====================================================

            var model =
                new SupplierReportViewModel
                {
                    TotalSuppliers =
                        totalSuppliers,

                    TotalPurchase =
                        totalPurchase,

                    TotalPaid =
                        totalPaid,

                    TotalOutstanding =
                        totalOutstanding,

                    // =================================================
                    // ORGANISATION DETAILS
                    // =================================================

                    OrganisationName =
                        await _settingService.GetOrganisationNameAsync(),

                    Branch =
                        await _settingService.GetBranchAsync(),

                    Address =
                        await _settingService.GetAddressAsync(),

                    Phone =
                        await _settingService.GetPhoneAsync(),

                    Email =
                        await _settingService.GetEmailAsync(),

                    GSTIN =
                        await _settingService.GetGSTINAsync(),

                    // =================================================
                    // REPORT SETTINGS
                    // =================================================

                    ReportHeading =
                        await _settingService.GetReportHeadingAsync(),

                    FromDateLabel =
                        await _settingService.GetFromDateLabelAsync(),

                    ToDateLabel =
                        await _settingService.GetToDateLabelAsync(),

                    // =================================================
                    // AUTHORISED SIGNATURE
                    // =================================================

                    AuthorisedSignatory =
                        await _settingService.GetAuthorisedSignatoryAsync(),

                    AuthorisedDesignation =
                        await _settingService.GetAuthorisedDesignationAsync()
                };

            return View(model);
        }
    }
}

