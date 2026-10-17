
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
    [Authorize(Roles = "SuperAdmin")]
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
                ViewData["SalesOrderEnabled"] is bool enabled && enabled;

            var model = new DashboardReportViewModel
            {
                TotalProducts =
                    await _context.Products.CountAsync(x => !x.IsDeleted),

                TotalSuppliers =
                    await _context.Suppliers.CountAsync(x => !x.IsDeleted),

                TotalCustomers =
                    await _context.Customers.CountAsync(),

                TotalWarehouses =
                    await _context.Warehouses.CountAsync(x => !x.IsDeleted)
            };

            if (salesOrderEnabled)
            {
                // Add sales summary properties here when available
                // in DashboardReportViewModel.
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

            var query = _context.PurchaseOrders
                .AsNoTracking()
                .Include(x => x.Supplier)
                .Where(x => !x.IsDeleted)
                .AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(x =>
                    x.OrderDate.Date >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                query = query.Where(x =>
                    x.OrderDate.Date <= toDate.Value.Date);
            }

            if (supplierId.HasValue && supplierId.Value > 0)
            {
                query = query.Where(x =>
                    x.SupplierId == supplierId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(x =>
                    x.Status.ToString() == status);
            }

            var orders = await query
                .OrderByDescending(x => x.OrderDate)
                .ToListAsync();

            var model = new PurchaseReportViewModel
            {
                FromDate = fromDate,
                ToDate = toDate,
                SupplierId = supplierId,
                Status = status,

                TotalOrders = orders.Count,
                TotalPurchase = orders.Sum(x => x.GrandTotal),
                TotalTax = orders.Sum(x => x.TaxAmount),
                TotalDiscount = orders.Sum(x => x.Discount),

                Orders = orders.Select(x => new PurchaseReportRowViewModel
                {
                    Id = x.Id,
                    PONumber = x.PONumber,
                    OrderDate = x.OrderDate,
                    ExpectedDate = x.ExpectedDate,
                    SupplierName = x.Supplier?.Name ?? "Unknown",
                    SubTotal = x.SubTotal,
                    Discount = x.Discount,
                    TaxAmount = x.TaxAmount,
                    GrandTotal = x.GrandTotal,
                    Status = x.Status.ToString()
                }).ToList(),

                Suppliers = await _context.Suppliers
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .OrderBy(x => x.Name)
                    .Select(x => new SupplierReportFilterViewModel
                    {
                        Id = x.Id,
                        Name = x.Name
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
            await LoadSalesOrderSettingsAsync();

            var salesOrderEnabled =
                ViewData["SalesOrderEnabled"] is bool enabled && enabled;

            if (!salesOrderEnabled)
            {
                TempData["Error"] =
                    "The Sales Order module is currently disabled.";

                return RedirectToAction(nameof(Dashboard));
            }

            var today = DateTime.Today;

            var startDate = fromDate?.Date
                ?? new DateTime(today.Year, today.Month, 1);

            var endDate = toDate?.Date ?? today;

            if (startDate > endDate)
            {
                endDate = startDate;
            }

            var endDateExclusive = endDate.AddDays(1);

            var orders = await _context.SalesOrders
                .AsNoTracking()
                .Include(x => x.SalesOrderItems)
                .Where(x =>
                    !x.IsDeleted &&
                    x.Status == SalesOrderStatus.Confirmed &&
                    x.OrderDate >= startDate &&
                    x.OrderDate < endDateExclusive)
                .OrderByDescending(x => x.OrderDate)
                .ToListAsync();

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
                    decimal grossSales =
                        item.Quantity * item.SalePrice;

                    decimal discount =
                        grossSales * item.DiscountPercent / 100m;

                    decimal itemNetSales =
                        Math.Max(grossSales - discount, 0m);

                    decimal tax =
                        itemNetSales * item.TaxPercent / 100m;

                    decimal itemGrandTotal =
                        itemNetSales + tax;

                    decimal itemCOGS =
                        item.Quantity * item.UnitPrice;

                    decimal itemGrossProfit =
                        itemNetSales - itemCOGS;

                    totalSales += grossSales;
                    totalDiscount += discount;
                    netSales += itemNetSales;
                    totalTax += tax;
                    grandTotal += itemGrandTotal;
                    costOfGoodsSold += itemCOGS;
                    grossProfit += itemGrossProfit;
                }
            }

            decimal grossMarginPercent =
                netSales > 0m
                    ? grossProfit / netSales * 100m
                    : 0m;

            var model = new SalesReportViewModel
            {
                FromDate = startDate,
                ToDate = endDate,

                TotalSales = Math.Round(totalSales, 2),
                TotalDiscount = Math.Round(totalDiscount, 2),
                NetSales = Math.Round(netSales, 2),
                TotalTax = Math.Round(totalTax, 2),
                GrandTotal = Math.Round(grandTotal, 2),
                CostOfGoodsSold = Math.Round(costOfGoodsSold, 2),
                GrossProfit = Math.Round(grossProfit, 2),
                GrossMarginPercent = Math.Round(grossMarginPercent, 2),
                TotalInvoices = orders.Count,

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
        // ProductName displays Product Title.
        // SKU and Barcode remain separate fields.
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Stock(
            int? productId,
            int? warehouseId,
            string? stockStatus)
        {
            await LoadOrganisationSettingsAsync();

            var model = new StockReportViewModel
            {
                ProductId = productId,
                WarehouseId = warehouseId,
                StockStatus = stockStatus,

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

            var stockQuery = _context.WarehouseStocks
                .AsNoTracking()
                .Include(x => x.Product)
                .Include(x => x.Warehouse)
                .Where(x =>
                    x.Product != null &&
                    !x.Product.IsDeleted &&
                    x.Warehouse != null &&
                    !x.Warehouse.IsDeleted)
                .AsQueryable();

            if (productId.HasValue && productId.Value > 0)
            {
                stockQuery = stockQuery.Where(x =>
                    x.ProductId == productId.Value);
            }

            if (warehouseId.HasValue && warehouseId.Value > 0)
            {
                stockQuery = stockQuery.Where(x =>
                    x.WarehouseId == warehouseId.Value);
            }

            var stocks = await stockQuery
                .OrderBy(x => x.Product!.Title)
                .ThenBy(x => x.Warehouse!.Name)
                .ToListAsync();

            var ledgerQuery = _context.StockLedgers
                .AsNoTracking()
                .AsQueryable();

            if (productId.HasValue && productId.Value > 0)
            {
                ledgerQuery = ledgerQuery.Where(x =>
                    x.ProductId == productId.Value);
            }

            if (warehouseId.HasValue && warehouseId.Value > 0)
            {
                ledgerQuery = ledgerQuery.Where(x =>
                    x.WarehouseId == warehouseId.Value);
            }

            var ledgerEntries = await ledgerQuery
                .OrderBy(x => x.TransactionDate)
                .ThenBy(x => x.Id)
                .ToListAsync();

            var grnItems = await _context.GoodsReceiptItems
                .AsNoTracking()
                .Include(x => x.GoodsReceipt)
                .Where(x =>
                    x.GoodsReceipt != null &&
                    !x.GoodsReceipt.IsDeleted &&
                    x.GoodsReceipt.Status == GoodsReceiptStatus.Received &&
                    x.ReceivedQuantity > 0)
                .ToListAsync();

            var reportItems = new List<StockReportItemViewModel>();

            foreach (var stock in stocks)
            {
                if (stock.Product == null || stock.Warehouse == null)
                {
                    continue;
                }

                var productLedger = ledgerEntries
                    .Where(x =>
                        x.ProductId == stock.ProductId &&
                        x.WarehouseId == stock.WarehouseId)
                    .ToList();

                decimal openingStock = productLedger
                    .Where(x => x.TransactionType ==
                                StockTransactionType.OpeningStock)
                    .Sum(x => x.Quantity);

                decimal purchaseReceiptQuantity = productLedger
                    .Where(x => x.TransactionType ==
                                StockTransactionType.PurchaseReceipt)
                    .Sum(x => x.Quantity);

                decimal salesIssueQuantity = productLedger
                    .Where(x => x.TransactionType ==
                                StockTransactionType.SalesIssue)
                    .Sum(x => x.Quantity);

                decimal salesReturnQuantity = productLedger
                    .Where(x => x.TransactionType ==
                                StockTransactionType.SalesReturn)
                    .Sum(x => x.Quantity);

                decimal purchaseReturnQuantity = productLedger
                    .Where(x => x.TransactionType ==
                                StockTransactionType.PurchaseReturn)
                    .Sum(x => x.Quantity);

                decimal transferInQuantity = productLedger
                    .Where(x => x.TransactionType ==
                                StockTransactionType.StockTransferIn)
                    .Sum(x => x.Quantity);

                decimal transferOutQuantity = productLedger
                    .Where(x => x.TransactionType ==
                                StockTransactionType.StockTransferOut)
                    .Sum(x => x.Quantity);

                decimal adjustmentQuantity = productLedger
                    .Where(x => x.TransactionType ==
                                StockTransactionType.StockAdjustment)
                    .Sum(x => x.Quantity);

                decimal receivedQuantity =
                    purchaseReceiptQuantity +
                    salesReturnQuantity +
                    transferInQuantity +
                    Math.Max(0m, adjustmentQuantity);

                decimal soldQuantity = salesIssueQuantity;

                // Retained for clarity if ledger reconciliation is needed.
                decimal ledgerBalance =
                    openingStock +
                    purchaseReceiptQuantity +
                    salesReturnQuantity +
                    transferInQuantity -
                    salesIssueQuantity -
                    purchaseReturnQuantity -
                    transferOutQuantity +
                    adjustmentQuantity;

                var purchases = grnItems
                    .Where(x =>
                        x.ProductId == stock.ProductId &&
                        x.WarehouseId == stock.WarehouseId)
                    .ToList();

                decimal totalReceivedQuantity =
                    purchases.Sum(x => x.ReceivedQuantity);

                decimal totalPurchaseCost =
                    purchases.Sum(x =>
                        x.ReceivedQuantity * x.UnitPrice);

                decimal purchasePrice =
                    totalReceivedQuantity > 0m
                        ? totalPurchaseCost / totalReceivedQuantity
                        : 0m;

                decimal quantityOnHand = stock.QuantityOnHand;
                decimal reorderLevel = stock.MinimumStock;

                string status;

                if (quantityOnHand <= 0m)
                {
                    status = "OutOfStock";
                }
                else if (quantityOnHand <= reorderLevel)
                {
                    status = "LowStock";
                }
                else
                {
                    status = "InStock";
                }

                decimal stockValue = quantityOnHand * purchasePrice;

                reportItems.Add(new StockReportItemViewModel
                {
                    Id = stock.Id,
                    ProductId = stock.ProductId,

                    // Updated: display title instead of SKU.
                    ProductName = stock.Product.Title,

                    // Keep identifiers separate.
                    SKU = stock.Product.SKU,
                    Barcode = stock.Product.Barcode,

                    WarehouseId = stock.WarehouseId,
                    WarehouseName = stock.Warehouse.Name,

                    OpeningStock = openingStock,
                    ReceivedQuantity = receivedQuantity,
                    SoldQuantity = soldQuantity,
                    QuantityOnHand = quantityOnHand,
                    ReorderLevel = reorderLevel,
                    PurchasePrice = purchasePrice,
                    StockValue = stockValue,
                    StockStatus = status
                });
            }

            if (!string.IsNullOrWhiteSpace(stockStatus))
            {
                reportItems = reportItems
                    .Where(x => x.StockStatus == stockStatus)
                    .ToList();
            }

            model.TotalProducts = reportItems
                .Select(x => x.ProductId)
                .Distinct()
                .Count();

            model.TotalQuantity =
                reportItems.Sum(x => x.QuantityOnHand);

            model.TotalStockValue =
                reportItems.Sum(x => x.StockValue);

            model.LowStockProducts =
                reportItems.Count(x => x.StockStatus == "LowStock");

            model.OutOfStockProducts =
                reportItems.Count(x => x.StockStatus == "OutOfStock");

            model.Stocks = reportItems;

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

            var model = new InventoryValuationViewModel
            {
                MainCategoryId = mainCategoryId,
                CategoryId = categoryId,
                ProductId = productId,
                WarehouseId = warehouseId,
                FromDate = fromDate,
                ToDate = toDate
            };

            model.MainCategories = await _context.MainCategories
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Name)
                .Select(x => new InventoryMainCategoryFilterViewModel
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .ToListAsync();

            model.Categories = await _context.Categories
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Name)
                .Select(x => new InventoryCategoryFilterViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    MainCategoryId = x.MainCategoryId
                })
                .ToListAsync();

            // Product dropdown displays Product Title.
            model.Products = await _context.Products
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Title)
                .Select(x => new InventoryProductFilterViewModel
                {
                    Id = x.Id,
                    Name = x.Title
                })
                .ToListAsync();

            model.Warehouses = await _context.Warehouses
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Name)
                .Select(x => new InventoryWarehouseFilterViewModel
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .ToListAsync();

            var mainCategories = await _context.MainCategories
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .ToDictionaryAsync(x => x.Id, x => x.Name);

            var categories = await _context.Categories
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .ToDictionaryAsync(x => x.Id, x => new
                {
                    x.Id,
                    x.Name,
                    x.MainCategoryId
                });

            var subCategories = await _context.SubCategories
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .ToDictionaryAsync(x => x.Id, x => new
                {
                    x.Name,
                    x.CategoryId
                });

            var stockQuery = _context.WarehouseStocks
                .AsNoTracking()
                .Include(x => x.Product)
                .Include(x => x.Warehouse)
                .Where(x =>
                    x.Product != null &&
                    !x.Product.IsDeleted &&
                    x.Warehouse != null &&
                    !x.Warehouse.IsDeleted)
                .AsQueryable();

            if (productId.HasValue && productId.Value > 0)
            {
                stockQuery = stockQuery.Where(x =>
                    x.ProductId == productId.Value);
            }

            if (warehouseId.HasValue && warehouseId.Value > 0)
            {
                stockQuery = stockQuery.Where(x =>
                    x.WarehouseId == warehouseId.Value);
            }

            if (mainCategoryId.HasValue && mainCategoryId.Value > 0)
            {
                var mainCategoryProductIds = await _context.Products
                    .AsNoTracking()
                    .Where(p =>
                        !p.IsDeleted &&
                        _context.Categories.Any(c =>
                            c.Id == p.CategoryId &&
                            !c.IsDeleted &&
                            c.MainCategoryId == mainCategoryId.Value))
                    .Select(p => p.Id)
                    .ToListAsync();

                stockQuery = stockQuery.Where(x =>
                    mainCategoryProductIds.Contains(x.ProductId));
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                stockQuery = stockQuery.Where(x =>
                    x.Product!.CategoryId == categoryId.Value);
            }

            var stocks = await stockQuery
                .OrderBy(x => x.Product!.Title)
                .ThenBy(x => x.Warehouse!.Name)
                .ToListAsync();

            var grnItemsQuery = _context.GoodsReceiptItems
                .AsNoTracking()
                .Include(x => x.GoodsReceipt)
                .Where(x =>
                    x.GoodsReceipt != null &&
                    !x.GoodsReceipt.IsDeleted &&
                    x.GoodsReceipt.Status == GoodsReceiptStatus.Received &&
                    x.ReceivedQuantity > 0)
                .AsQueryable();

            if (fromDate.HasValue)
            {
                var fromDateValue = fromDate.Value.Date;

                grnItemsQuery = grnItemsQuery.Where(x =>
                    x.GoodsReceipt!.ReceiptDate >= fromDateValue);
            }

            if (toDate.HasValue)
            {
                var toDateValue = toDate.Value.Date.AddDays(1);

                grnItemsQuery = grnItemsQuery.Where(x =>
                    x.GoodsReceipt!.ReceiptDate < toDateValue);
            }

            var grnItems = await grnItemsQuery.ToListAsync();

            foreach (var stock in stocks)
            {
                if (stock.Product == null || stock.Warehouse == null)
                {
                    continue;
                }

                var product = stock.Product;

                int mainCategoryIdValue = 0;
                string mainCategoryName = "-";

                int categoryIdValue = product.CategoryId;
                string categoryName = "-";

                int subCategoryIdValue = product.SubCategoryId;
                string subCategoryName = "-";

                if (subCategories.TryGetValue(
                    product.SubCategoryId, out var subCategory))
                {
                    subCategoryName = subCategory.Name;

                    if (categories.TryGetValue(
                        subCategory.CategoryId, out var categoryFromSubCategory))
                    {
                        categoryIdValue = categoryFromSubCategory.Id;
                        categoryName = categoryFromSubCategory.Name;
                        mainCategoryIdValue = categoryFromSubCategory.MainCategoryId;

                        if (mainCategories.TryGetValue(
                            mainCategoryIdValue, out var mainCategoryNameValue))
                        {
                            mainCategoryName = mainCategoryNameValue;
                        }
                    }
                }

                if (categoryName == "-" &&
                    categories.TryGetValue(product.CategoryId, out var directCategory))
                {
                    categoryIdValue = directCategory.Id;
                    categoryName = directCategory.Name;
                    mainCategoryIdValue = directCategory.MainCategoryId;

                    if (mainCategories.TryGetValue(
                        mainCategoryIdValue, out var directMainCategoryName))
                    {
                        mainCategoryName = directMainCategoryName;
                    }
                }

                var productWarehousePurchases = grnItems
                    .Where(x =>
                        x.ProductId == stock.ProductId &&
                        x.WarehouseId == stock.WarehouseId &&
                        x.ReceivedQuantity > 0)
                    .ToList();

                DateTime? receiptDate = productWarehousePurchases
                    .Where(x => x.GoodsReceipt != null)
                    .Select(x => (DateTime?)x.GoodsReceipt!.ReceiptDate)
                    .OrderByDescending(x => x)
                    .FirstOrDefault();

                decimal totalReceivedQuantity =
                    productWarehousePurchases.Sum(x => x.ReceivedQuantity);

                decimal totalPurchaseCost =
                    productWarehousePurchases.Sum(x =>
                        x.ReceivedQuantity * x.UnitPrice);

                decimal purchasePrice = totalReceivedQuantity > 0m
                    ? totalPurchaseCost / totalReceivedQuantity
                    : 0m;

                decimal quantityOnHand = stock.QuantityOnHand;
                decimal inventoryValue = quantityOnHand * purchasePrice;

                model.Items.Add(new InventoryValuationItemViewModel
                {
                    Id = stock.Id,
                    ProductId = stock.ProductId,

                    // Updated: display title instead of SKU.
                    ProductName = product.Title,

                    // Keep identifiers separate.
                    SKU = product.SKU,
                    Barcode = product.Barcode,

                    MainCategoryId = mainCategoryIdValue,
                    MainCategoryName = mainCategoryName,
                    CategoryId = categoryIdValue,
                    CategoryName = categoryName,
                    SubCategoryId = subCategoryIdValue,
                    SubCategoryName = subCategoryName,

                    WarehouseId = stock.WarehouseId,
                    WarehouseName = stock.Warehouse.Name,

                    ReceiptDate = receiptDate,
                    QuantityOnHand = quantityOnHand,
                    PurchasePrice = purchasePrice,
                    InventoryValue = inventoryValue
                });
            }

            model.TotalProducts = model.Items
                .Select(x => x.ProductId)
                .Distinct()
                .Count();

            model.TotalQuantity =
                model.Items.Sum(x => x.QuantityOnHand);

            model.TotalInventoryValue =
                model.Items.Sum(x => x.InventoryValue);

            model.TotalPurchaseCost =
                model.Items.Sum(x =>
                    x.QuantityOnHand * x.PurchasePrice);

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
            await LoadOrganisationSettingsAsync();

            var query = _context.WarehouseStocks
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

            if (productId.HasValue && productId.Value > 0)
            {
                query = query.Where(x =>
                    x.ProductId == productId.Value);
            }

            if (warehouseId.HasValue && warehouseId.Value > 0)
            {
                query = query.Where(x =>
                    x.WarehouseId == warehouseId.Value);
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(x =>
                    x.Product != null &&
                    x.Product.CategoryId == categoryId.Value);
            }

            query = query.Where(x =>
                x.QuantityOnHand <= x.MinimumStock);

            var stocks = await query
                .OrderBy(x => x.Product!.Title)
                .ThenBy(x => x.Warehouse!.Name)
                .ToListAsync();

            var model = new LowStockReportViewModel
            {
                ProductId = productId,
                WarehouseId = warehouseId,
                CategoryId = categoryId,

                TotalLowStockProducts = stocks
                    .Select(x => x.ProductId)
                    .Distinct()
                    .Count(),

                TotalCurrentStock =
                    stocks.Sum(x => x.QuantityOnHand),

                TotalReorderLevel =
                    stocks.Sum(x => x.MinimumStock),

                TotalShortageQuantity =
                    stocks.Sum(x =>
                        Math.Max(0m, x.MinimumStock - x.QuantityOnHand)),

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

            foreach (var stock in stocks)
            {
                var product = stock.Product;

                if (product == null)
                {
                    continue;
                }

                decimal shortageQuantity =
                    Math.Max(0m, stock.MinimumStock - stock.QuantityOnHand);

                string stockStatus =
                    stock.QuantityOnHand <= 0m
                        ? "Out of Stock"
                        : "Low Stock";

                model.Items.Add(new LowStockItemViewModel
                {
                    ProductId = stock.ProductId,

                    // Updated: display title instead of SKU.
                    ProductName = product.Title ?? "N/A",

                    // Keep identifiers separate.
                    SKU = product.SKU ?? "N/A",
                    Barcode = product.Barcode ?? "N/A",

                    CategoryName = product.Category?.Name ?? "N/A",
                    WarehouseName = stock.Warehouse?.Name ?? "N/A",
                    UnitName = "N/A",

                    CurrentStock = stock.QuantityOnHand,
                    ReorderLevel = stock.MinimumStock,
                    ShortageQuantity = shortageQuantity,
                    SuggestedOrderQuantity = shortageQuantity,
                    StockStatus = stockStatus
                });
            }

            await LoadLowStockDropdowns(model);

            return View(model);
        }

        // =========================================================
        // LOW STOCK DROPDOWNS
        // Product dropdown displays Product Title.
        // =========================================================

        private async Task LoadLowStockDropdowns(
            LowStockReportViewModel model)
        {
            model.Products = await _context.Products
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Title)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Title
                })
                .ToListAsync();

            model.Warehouses = await _context.Warehouses
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Name)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name
                })
                .ToListAsync();

            model.Categories = await _context.Categories
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Name)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name
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

            var today = DateTime.Today;

            var startDate = fromDate?.Date ?? today;
            var endDate = toDate?.Date ?? today;

            if (startDate > endDate)
            {
                endDate = startDate;
            }

            var startDateTime = startDate.Date;
            var endDateTime = endDate.Date.AddDays(1);

            var transferOuts = await _context.StockLedgers
                .AsNoTracking()
                .Include(x => x.Product)
                .Include(x => x.Warehouse)
                .Where(x =>
                    x.TransactionType == StockTransactionType.StockTransferOut &&
                    x.TransactionDate >= startDateTime &&
                    x.TransactionDate < endDateTime)
                .OrderBy(x => x.TransactionDate)
                .ThenBy(x => x.Id)
                .ToListAsync();

            var transferIns = await _context.StockLedgers
                .AsNoTracking()
                .Include(x => x.Product)
                .Include(x => x.Warehouse)
                .Where(x =>
                    x.TransactionType == StockTransactionType.StockTransferIn &&
                    x.TransactionDate >= startDateTime &&
                    x.TransactionDate < endDateTime)
                .OrderBy(x => x.TransactionDate)
                .ThenBy(x => x.Id)
                .ToListAsync();

            var mainCategories = await _context.MainCategories
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .ToDictionaryAsync(x => x.Id, x => x.Name);

            var categories = await _context.Categories
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .ToDictionaryAsync(x => x.Id, x => new
                {
                    x.Name,
                    x.MainCategoryId
                });

            var subCategories = await _context.SubCategories
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .ToDictionaryAsync(x => x.Id, x => new
                {
                    x.Name,
                    x.CategoryId
                });

            var reportItems = new List<StockTransferReportRowViewModel>();

            foreach (var transferOut in transferOuts)
            {
                var transferIn = transferIns
                    .Where(x =>
                        x.ProductId == transferOut.ProductId &&
                        x.ReferenceNo == transferOut.ReferenceNo &&
                        x.WarehouseId != transferOut.WarehouseId)
                    .OrderBy(x => x.TransactionDate)
                    .ThenBy(x => x.Id)
                    .FirstOrDefault();

                if (transferIn == null)
                {
                    continue;
                }

                var product = transferOut.Product;

                if (product == null)
                {
                    continue;
                }

                string mainCategoryName = "-";
                string categoryName = "-";
                string subcategoryName = "-";

                if (product.CategoryId > 0 &&
                    categories.TryGetValue(product.CategoryId, out var category))
                {
                    categoryName = category.Name;

                    if (mainCategories.TryGetValue(
                        category.MainCategoryId, out var mainCategory))
                    {
                        mainCategoryName = mainCategory;
                    }
                }

                if (product.SubCategoryId > 0 &&
                    subCategories.TryGetValue(
                        product.SubCategoryId, out var subCategory))
                {
                    subcategoryName = subCategory.Name;

                    if (categories.TryGetValue(
                        subCategory.CategoryId, out var categoryFromSubCategory))
                    {
                        categoryName = categoryFromSubCategory.Name;

                        if (mainCategories.TryGetValue(
                            categoryFromSubCategory.MainCategoryId,
                            out var mainCategoryFromSubCategory))
                        {
                            mainCategoryName = mainCategoryFromSubCategory;
                        }
                    }
                }

                var transferNo = transferOut.ReferenceNo ?? "-";

                // Updated: product title is the report display name.
                var productName = product.Title ?? "-";

                var fromWarehouseName = transferOut.Warehouse?.Name ?? "-";
                var toWarehouseName = transferIn.Warehouse?.Name ?? "-";

                var quantity = Math.Abs(Convert.ToDecimal(transferOut.Quantity));
                var unitCost = Convert.ToDecimal(transferOut.UnitCost);

                reportItems.Add(new StockTransferReportRowViewModel
                {
                    ProductId = transferOut.ProductId,
                    TransferNo = transferNo,
                    TransferType = "Outward",
                    TransferDate = transferOut.TransactionDate,
                    ProductName = productName,
                    MainCategoryName = mainCategoryName,
                    CategoryName = categoryName,
                    SubcategoryName = subcategoryName,
                    FromWarehouseId = transferOut.WarehouseId,
                    FromWarehouseName = fromWarehouseName,
                    ToWarehouseId = transferIn.WarehouseId,
                    ToWarehouseName = toWarehouseName,
                    Quantity = quantity,
                    UnitCost = unitCost
                });

                reportItems.Add(new StockTransferReportRowViewModel
                {
                    ProductId = transferIn.ProductId,
                    TransferNo = transferIn.ReferenceNo ?? transferNo,
                    TransferType = "Inward",
                    TransferDate = transferIn.TransactionDate,

                    // Updated: use the inward item's product title.
                    ProductName = transferIn.Product?.Title ?? productName,

                    MainCategoryName = mainCategoryName,
                    CategoryName = categoryName,
                    SubcategoryName = subcategoryName,
                    FromWarehouseId = transferOut.WarehouseId,
                    FromWarehouseName = fromWarehouseName,
                    ToWarehouseId = transferIn.WarehouseId,
                    ToWarehouseName = toWarehouseName,
                    Quantity = Math.Abs(Convert.ToDecimal(transferIn.Quantity)),
                    UnitCost = Convert.ToDecimal(transferIn.UnitCost)
                });
            }

            reportItems = reportItems
                .OrderByDescending(x => x.TransferDate)
                .ThenBy(x => x.TransferNo)
                .ThenBy(x => x.ProductName)
                .ThenBy(x => x.TransferType == "Outward" ? 0 : 1)
                .ToList();

            var model = new StockTransferReportViewModel
            {
                FromDate = startDate,
                ToDate = endDate,
                Transfers = reportItems,

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
        // 8. PROFIT & LOSS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> ProfitLoss(
            DateTime? fromDate,
            DateTime? toDate)
        {
            await LoadSalesOrderSettingsAsync();

            var salesOrderEnabled =
                ViewData["SalesOrderEnabled"] is bool enabled && enabled;

            var today = DateTime.Today;

            var startDate = fromDate?.Date
                ?? new DateTime(today.Year, today.Month, 1);

            var endDate = toDate?.Date ?? today;

            if (endDate < startDate)
            {
                endDate = startDate;
            }

            decimal totalSales = 0m;
            decimal costOfGoodsSold = 0m;
            decimal grossProfit = 0m;
            decimal expenses = 0m;

            if (salesOrderEnabled)
            {
                var salesOrders = await _context.SalesOrders
                    .AsNoTracking()
                    .Include(x => x.SalesOrderItems)
                    .Where(x =>
                        !x.IsDeleted &&
                        x.Status == SalesOrderStatus.Confirmed &&
                        x.OrderDate >= startDate &&
                        x.OrderDate < endDate.AddDays(1))
                    .ToListAsync();

                foreach (var order in salesOrders)
                {
                    foreach (var item in order.SalesOrderItems)
                    {
                        decimal grossSales =
                            item.Quantity * item.SalePrice;

                        decimal discountAmount =
                            grossSales * item.DiscountPercent / 100m;

                        decimal netSales =
                            Math.Max(grossSales - discountAmount, 0m);

                        decimal itemCost =
                            item.Quantity * item.UnitPrice;

                        decimal itemGrossProfit =
                            netSales - itemCost;

                        totalSales += netSales;
                        costOfGoodsSold += itemCost;
                        grossProfit += itemGrossProfit;
                    }
                }

                totalSales = Math.Round(
                    totalSales, 2, MidpointRounding.AwayFromZero);

                costOfGoodsSold = Math.Round(
                    costOfGoodsSold, 2, MidpointRounding.AwayFromZero);

                grossProfit = Math.Round(
                    grossProfit, 2, MidpointRounding.AwayFromZero);
            }

            // Expense module has not been implemented yet.
            expenses = 0m;

            decimal netProfit = Math.Round(
                grossProfit - expenses,
                2,
                MidpointRounding.AwayFromZero);

            var model = new ProfitLossViewModel
            {
                TotalSales = totalSales,
                CostOfGoodsSold = costOfGoodsSold,
                GrossProfit = grossProfit,
                Expenses = expenses,
                NetProfit = netProfit,

                FromDate = startDate,
                ToDate = endDate,

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
        // 9. GST REPORT
        // Add the GST report action when its view model is available.
        // =========================================================

        // =========================================================
        // 10. CUSTOMER REPORT
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Customers()
        {
            await LoadOrganisationSettingsAsync();

            var model = new CustomerReportViewModel
            {
                TotalCustomers =
                    await _context.Customers.CountAsync()
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

            int totalSuppliers = await _context.Suppliers
                .CountAsync(x => !x.IsDeleted && x.IsActive);

            decimal totalPurchase = await _context.PurchaseOrders
                .Where(x => !x.IsDeleted)
                .SumAsync(x => x.GrandTotal);

            // Payment module has not been implemented yet.
            decimal totalPaid = 0m;

            decimal totalOutstanding = totalPurchase - totalPaid;

            var model = new SupplierReportViewModel
            {
                TotalSuppliers = totalSuppliers,
                TotalPurchase = totalPurchase,
                TotalPaid = totalPaid,
                TotalOutstanding = totalOutstanding,

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
    }
}
