using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Domain.Enums;
using StoreManagementSystem.Infrastructure.Data;
using StoreManagementSystem.Web.ViewModels.Sales;

namespace StoreManagementSystem.Web.Controllers
{
    [Authorize]
    public class SalesInvoiceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ISystemSettingService _settingService;

        public SalesInvoiceController(
            ApplicationDbContext context,
            ISystemSettingService settingService)
        {
            _context = context;
            _settingService = settingService;
        }

        // ============================================================
        // MODULE CHECK
        // ============================================================

        private async Task<IActionResult?> CheckModuleAccessAsync()
        {
            var enabled =
                await _settingService
                    .IsSalesOrderEnabledAsync();

            if (!enabled)
            {
                TempData["Error"] =
                    "The Sales Order module is currently disabled.";

                return RedirectToAction(
                    "Index",
                    "Dashboard");
            }

            return null;
        }

        // ============================================================
        // INDEX
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var access =
                await CheckModuleAccessAsync();

            if (access != null)
                return access;

            var invoices =
                await _context.SalesInvoices
                    .AsNoTracking()
                    .Include(x => x.SalesOrder)
                    .Include(x => x.Warehouse)
                    .OrderByDescending(x => x.InvoiceDate)
                    .ThenByDescending(x => x.Id)
                    .ToListAsync();

            var model =
                invoices
                    .Select(x =>
                        new SalesInvoiceIndexViewModel
                        {
                            Id =
                                x.Id,

                            InvoiceNumber =
                                x.InvoiceNumber,

                            InvoiceDate =
                                x.InvoiceDate,

                            SalesOrderNumber =
                                x.SalesOrder?.OrderNumber
                                ?? "-",

                            CustomerName =
                                x.CustomerName
                                ?? "Walk-in Customer",

                            WarehouseName =
                                x.Warehouse?.Name
                                ?? "-",

                            GrandTotal =
                                x.GrandTotal,

                            PaidAmount =
                                x.PaidAmount,

                            BalanceDue =
                                x.BalanceDue,

                            PaymentMode =
                                x.PaymentMode,

                            Status =
                                x.Status
                        })
                    .ToList();

            return View(model);
        }

        // ============================================================
        // CREATE GET
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Create(
            int? salesOrderId)
        {
            var access =
                await CheckModuleAccessAsync();

            if (access != null)
                return access;

            var model =
                new SalesInvoiceCreateViewModel
                {
                    InvoiceDate =
                        DateTime.Today,

                    PaymentMode =
                        "Cash"
                };

            await LoadSalesOrdersAsync(
                model,
                salesOrderId);

            return View(model);
        }

        // ============================================================
        // CREATE POST
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            SalesInvoiceCreateViewModel model)
        {
            var access =
                await CheckModuleAccessAsync();

            if (access != null)
                return access;

            // ========================================================
            // VALIDATE MODEL
            // ========================================================

            if (!ModelState.IsValid)
            {
                await LoadSalesOrdersAsync(
                    model,
                    model.SalesOrderId);

                return View(model);
            }

            // ========================================================
            // CHECK DUPLICATE INVOICE
            // ========================================================

            var existingInvoice =
                await _context.SalesInvoices
                    .AnyAsync(x =>
                        x.SalesOrderId ==
                            model.SalesOrderId &&

                        x.Status !=
                            "Cancelled");

            if (existingInvoice)
            {
                TempData["Error"] =
                    "An invoice already exists for this Sales Order.";

                return RedirectToAction(
                    nameof(Index));
            }

            // ========================================================
            // LOAD SALES ORDER
            // ========================================================

            var salesOrder =
                await _context.SalesOrders
                    .Include(x =>
                        x.SalesOrderItems)
                    .ThenInclude(x =>
                        x.Product)
                    .Include(x =>
                        x.Warehouse)
                    .FirstOrDefaultAsync(x =>
                        x.Id ==
                        model.SalesOrderId);

            if (salesOrder == null)
            {
                ModelState.AddModelError(
                    "",
                    "Sales Order not found.");

                await LoadSalesOrdersAsync(
                    model,
                    model.SalesOrderId);

                return View(model);
            }

            // ========================================================
            // CHECK ITEMS
            // ========================================================

            if (!salesOrder.SalesOrderItems.Any())
            {
                ModelState.AddModelError(
                    "",
                    "The selected Sales Order has no items.");

                await LoadSalesOrdersAsync(
                    model,
                    model.SalesOrderId);

                return View(model);
            }

            // ========================================================
            // CHECK WAREHOUSE
            // ========================================================

            if (salesOrder.WarehouseId <= 0)
            {
                ModelState.AddModelError(
                    "",
                    "Sales Order does not have a valid warehouse.");

                await LoadSalesOrdersAsync(
                    model,
                    model.SalesOrderId);

                return View(model);
            }

            // ========================================================
            // CHECK PAID AMOUNT
            // ========================================================

            if (model.PaidAmount < 0)
            {
                ModelState.AddModelError(
                    nameof(model.PaidAmount),
                    "Paid amount cannot be negative.");

                await LoadSalesOrdersAsync(
                    model,
                    model.SalesOrderId);

                return View(model);
            }

            if (model.PaidAmount >
                salesOrder.GrandTotal)
            {
                ModelState.AddModelError(
                    nameof(model.PaidAmount),
                    "Paid amount cannot be greater than invoice total.");

                await LoadSalesOrdersAsync(
                    model,
                    model.SalesOrderId);

                return View(model);
            }

            // ========================================================
            // TRANSACTION
            // ========================================================

            await using var transaction =
                await _context.Database
                    .BeginTransactionAsync();

            try
            {
                // ====================================================
                // CREATE SALES INVOICE
                // ====================================================

                var invoice =
                    new SalesInvoice
                    {
                        InvoiceNumber =
                            await GenerateInvoiceNumberAsync(),

                        InvoiceDate =
                            model.InvoiceDate,

                        SalesOrderId =
                            salesOrder.Id,

                        WarehouseId =
                            salesOrder.WarehouseId,

                        CustomerName =
                            salesOrder.CustomerName,

                        CustomerPhone =
                            salesOrder.CustomerPhone,

                        SubTotal =
                            salesOrder.SubTotal,

                        TaxAmount =
                            salesOrder.TaxAmount,

                        DiscountAmount =
                            salesOrder.DiscountAmount,

                        GrandTotal =
                            salesOrder.GrandTotal,

                        PaymentMode =
                            model.PaymentMode,

                        PaidAmount =
                            model.PaidAmount,

                        BalanceDue =
                            salesOrder.GrandTotal -
                            model.PaidAmount,

                        PaymentReference =
                            model.PaymentReference,

                        Remarks =
                            model.Remarks,

                        Status =
                            GetPaymentStatus(
                                salesOrder.GrandTotal,
                                model.PaidAmount),

                        CreatedDate =
                            DateTime.Now,

                        CreatedBy =
                            User.Identity?.Name
                    };

                // ====================================================
                // PROCESS EACH SALES ORDER ITEM
                // ====================================================

                foreach (var orderItem
                    in salesOrder.SalesOrderItems)
                {
                    if (orderItem.Quantity <= 0)
                    {
                        throw new InvalidOperationException(
                            $"Invalid quantity for product " +
                            $"{orderItem.Product?.SKU ?? orderItem.ProductId.ToString()}.");
                    }

                    // =================================================
                    // GET WAREHOUSE STOCK
                    // =================================================

                    var stock =
                        await _context.WarehouseStocks
                            .FirstOrDefaultAsync(x =>
                                x.WarehouseId ==
                                    salesOrder.WarehouseId &&

                                x.ProductId ==
                                    orderItem.ProductId);

                    if (stock == null)
                    {
                        throw new InvalidOperationException(
                            $"No stock record exists for " +
                            $"{orderItem.Product?.SKU ?? orderItem.ProductId.ToString()}.");
                    }

                    // =================================================
                    // CHECK AVAILABLE STOCK
                    // =================================================

                    if (stock.AvailableQuantity <
                        orderItem.Quantity)
                    {
                        throw new InvalidOperationException(
                            $"Insufficient stock for " +
                            $"{orderItem.Product?.SKU ?? orderItem.ProductId.ToString()}. " +
                            $"Available: {stock.AvailableQuantity:0.##}, " +
                            $"Required: {orderItem.Quantity:0.##}.");
                    }

                    // =================================================
                    // DEDUCT STOCK
                    // =================================================

                    stock.QuantityOnHand -=
                        orderItem.Quantity;

                    stock.LastUpdated =
                        DateTime.Now;

                    // =================================================
                    // GET ACTUAL PURCHASE COST
                    // =================================================
                    //
                    // UnitPrice on SalesOrderItem is already the
                    // purchase/cost price captured when the sale
                    // was created.
                    //
                    // This is used for gross profit calculation.
                    //
                    // =================================================

                    var unitCost =
                        orderItem.UnitPrice;

                    // =================================================
                    // CREATE SALES INVOICE ITEM
                    // =================================================

                    var invoiceItem =
                        new SalesInvoiceItem
                        {
                            SalesOrderItemId =
                                orderItem.Id,

                            ProductId =
                                orderItem.ProductId,

                            Quantity =
                                orderItem.Quantity,

                            // Purchase / Cost Price
                            UnitPrice =
                                orderItem.UnitPrice,

                            TaxPercent =
                                orderItem.TaxPercent,

                            DiscountPercent =
                                orderItem.DiscountPercent,

                            LineTotal =
                                orderItem.LineTotal
                        };

                    invoice.Items.Add(
                        invoiceItem);

                    // =================================================
                    // STOCK LEDGER
                    // =================================================

                    var ledger =
                        new StockLedger
                        {
                            ProductId =
                                orderItem.ProductId,

                            WarehouseId =
                                salesOrder.WarehouseId,

                            TransactionType =
                                StockTransactionType.SalesIssue,

                            Quantity =
                                -orderItem.Quantity,

                            BalanceAfterTransaction =
                                stock.QuantityOnHand,

                            UnitCost =
                                unitCost,

                            ReferenceNo =
                                invoice.InvoiceNumber,

                            TransactionDate =
                                DateTime.Now,

                            Remarks =
                                $"Sales Invoice {invoice.InvoiceNumber}",

                            CreatedBy =
                                User.Identity?.Name
                        };

                    _context.StockLedgers.Add(
                        ledger);
                }

                // ====================================================
                // SAVE INVOICE
                // ====================================================

                _context.SalesInvoices.Add(
                    invoice);

                // ====================================================
                // UPDATE SALES ORDER STATUS
                // ====================================================
                //
                // IMPORTANT:
                // SalesOrder.Status is SalesOrderStatus enum.
                //
                // There is NO "Invoiced" enum value because your
                // system does not have a separate delivery module.
                //
                // Therefore the order remains Confirmed.
                //
                // ====================================================

                salesOrder.Status =
                    SalesOrderStatus.Confirmed;

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                // ====================================================
                // SUCCESS
                // ====================================================

                TempData["Success"] =
                    $"Sales Invoice {invoice.InvoiceNumber} created successfully.";

                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id = invoice.Id
                    });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    "",
                    ex.Message);

                await LoadSalesOrdersAsync(
                    model,
                    model.SalesOrderId);

                return View(model);
            }
        }

        // ============================================================
        // DETAILS
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Details(
            int id)
        {
            var access =
                await CheckModuleAccessAsync();

            if (access != null)
                return access;

            var invoice =
                await _context.SalesInvoices
                    .AsNoTracking()
                    .Include(x =>
                        x.SalesOrder)
                    .Include(x =>
                        x.Warehouse)
                    .Include(x =>
                        x.Items)
                    .ThenInclude(x =>
                        x.Product)
                    .FirstOrDefaultAsync(x =>
                        x.Id == id);

            if (invoice == null)
                return NotFound();

            var model =
                new SalesInvoiceDetailsViewModel
                {
                    Id =
                        invoice.Id,

                    InvoiceNumber =
                        invoice.InvoiceNumber,

                    InvoiceDate =
                        invoice.InvoiceDate,

                    SalesOrderNumber =
                        invoice.SalesOrder?.OrderNumber
                        ?? "-",

                    CustomerName =
                        invoice.CustomerName
                        ?? "Walk-in Customer",

                    CustomerPhone =
                        invoice.CustomerPhone
                        ?? "-",

                    WarehouseName =
                        invoice.Warehouse?.Name
                        ?? "-",

                    SubTotal =
                        invoice.SubTotal,

                    TaxAmount =
                        invoice.TaxAmount,

                    DiscountAmount =
                        invoice.DiscountAmount,

                    GrandTotal =
                        invoice.GrandTotal,

                    PaymentMode =
                        invoice.PaymentMode,

                    PaidAmount =
                        invoice.PaidAmount,

                    BalanceDue =
                        invoice.BalanceDue,

                    PaymentReference =
                        invoice.PaymentReference
                        ?? "-",

                    Status =
                        invoice.Status,

                    Remarks =
                        invoice.Remarks
                        ?? "",

                    Items =
                        invoice.Items
                            .Select(x =>
                                new SalesInvoiceItemViewModel
                                {
                                    SKU =
                                        x.Product?.SKU
                                        ?? "-",

                                    Quantity =
                                        x.Quantity,

                                    UnitPrice =
                                        x.UnitPrice,

                                    TaxPercent =
                                        x.TaxPercent,

                                    DiscountPercent =
                                        x.DiscountPercent,

                                    LineTotal =
                                        x.LineTotal
                                })
                            .ToList()
                };

            return View(model);
        }

        // ============================================================
        // PRINT
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Print(
            int id)
        {
            var access =
                await CheckModuleAccessAsync();

            if (access != null)
                return access;

            var invoice =
                await _context.SalesInvoices
                    .AsNoTracking()
                    .Include(x =>
                        x.SalesOrder)
                    .Include(x =>
                        x.Warehouse)
                    .Include(x =>
                        x.Items)
                    .ThenInclude(x =>
                        x.Product)
                    .FirstOrDefaultAsync(x =>
                        x.Id == id);

            if (invoice == null)
                return NotFound();

            var model =
                new SalesInvoiceDetailsViewModel
                {
                    Id =
                        invoice.Id,

                    InvoiceNumber =
                        invoice.InvoiceNumber,

                    InvoiceDate =
                        invoice.InvoiceDate,

                    SalesOrderNumber =
                        invoice.SalesOrder?.OrderNumber
                        ?? "-",

                    CustomerName =
                        invoice.CustomerName
                        ?? "Walk-in Customer",

                    CustomerPhone =
                        invoice.CustomerPhone
                        ?? "-",

                    WarehouseName =
                        invoice.Warehouse?.Name
                        ?? "-",

                    SubTotal =
                        invoice.SubTotal,

                    TaxAmount =
                        invoice.TaxAmount,

                    DiscountAmount =
                        invoice.DiscountAmount,

                    GrandTotal =
                        invoice.GrandTotal,

                    PaymentMode =
                        invoice.PaymentMode,

                    PaidAmount =
                        invoice.PaidAmount,

                    BalanceDue =
                        invoice.BalanceDue,

                    PaymentReference =
                        invoice.PaymentReference
                        ?? "-",

                    Status =
                        invoice.Status,

                    Remarks =
                        invoice.Remarks
                        ?? "",

                    Items =
                        invoice.Items
                            .Select(x =>
                                new SalesInvoiceItemViewModel
                                {
                                    SKU =
                                        x.Product?.SKU
                                        ?? "-",

                                    Quantity =
                                        x.Quantity,

                                    UnitPrice =
                                        x.UnitPrice,

                                    TaxPercent =
                                        x.TaxPercent,

                                    DiscountPercent =
                                        x.DiscountPercent,

                                    LineTotal =
                                        x.LineTotal
                                })
                            .ToList()
                };

            return View(model);
        }

        // ============================================================
        // CANCEL INVOICE
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(
            int id)
        {
            var access =
                await CheckModuleAccessAsync();

            if (access != null)
                return access;

            var invoice =
                await _context.SalesInvoices
                    .Include(x =>
                        x.Items)
                    .FirstOrDefaultAsync(x =>
                        x.Id == id);

            if (invoice == null)
                return NotFound();

            if (invoice.Status ==
                "Cancelled")
            {
                TempData["Error"] =
                    "This invoice is already cancelled.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }

            await using var transaction =
                await _context.Database
                    .BeginTransactionAsync();

            try
            {
                foreach (var invoiceItem
                    in invoice.Items)
                {
                    var stock =
                        await _context.WarehouseStocks
                            .FirstOrDefaultAsync(x =>
                                x.WarehouseId ==
                                    invoice.WarehouseId &&

                                x.ProductId ==
                                    invoiceItem.ProductId);

                    if (stock == null)
                    {
                        throw new InvalidOperationException(
                            $"Stock record not found for product " +
                            $"{invoiceItem.ProductId}.");
                    }

                    // =================================================
                    // RESTORE STOCK
                    // =================================================

                    stock.QuantityOnHand +=
                        invoiceItem.Quantity;

                    stock.LastUpdated =
                        DateTime.Now;

                    // =================================================
                    // COST FOR RETURN LEDGER
                    // =================================================

                    var unitCost =
                        await GetLatestPurchaseCostAsync(
                            invoiceItem.ProductId);

                    // =================================================
                    // STOCK LEDGER
                    // =================================================

                    var ledger =
                        new StockLedger
                        {
                            ProductId =
                                invoiceItem.ProductId,

                            WarehouseId =
                                invoice.WarehouseId,

                            TransactionType =
                                StockTransactionType.SalesReturn,

                            Quantity =
                                invoiceItem.Quantity,

                            BalanceAfterTransaction =
                                stock.QuantityOnHand,

                            UnitCost =
                                unitCost,

                            ReferenceNo =
                                invoice.InvoiceNumber,

                            TransactionDate =
                                DateTime.Now,

                            Remarks =
                                $"Cancellation of Sales Invoice {invoice.InvoiceNumber}",

                            CreatedBy =
                                User.Identity?.Name
                        };

                    _context.StockLedgers.Add(
                        ledger);
                }

                // =====================================================
                // CANCEL INVOICE
                // =====================================================

                invoice.Status =
                    "Cancelled";

                invoice.CancelledDate =
                    DateTime.Now;

                invoice.CancelledBy =
                    User.Identity?.Name;

                // =====================================================
                // UPDATE SALES ORDER
                // =====================================================
                //
                // Since the invoice has been cancelled, return the
                // Sales Order to Confirmed status.
                //
                // =====================================================

                var salesOrder =
                    await _context.SalesOrders
                        .FirstOrDefaultAsync(x =>
                            x.Id ==
                            invoice.SalesOrderId);

                if (salesOrder != null)
                {
                    salesOrder.Status =
                        SalesOrderStatus.Confirmed;
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["Success"] =
                    $"Invoice {invoice.InvoiceNumber} cancelled and stock restored.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                TempData["Error"] =
                    ex.Message;

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }
        }

        // ============================================================
        // LOAD SALES ORDERS
        // ============================================================

        private async Task LoadSalesOrdersAsync(
            SalesInvoiceCreateViewModel model,
            int? selectedId)
        {
            var invoicedOrderIds =
                await _context.SalesInvoices
                    .Where(x =>
                        x.Status !=
                        "Cancelled")
                    .Select(x =>
                        x.SalesOrderId)
                    .ToListAsync();

            var orders =
                await _context.SalesOrders
                    .AsNoTracking()
                    .Where(x =>
                        !invoicedOrderIds.Contains(x.Id))
                    .OrderByDescending(x =>
                        x.OrderDate)
                    .ToListAsync();

            model.SalesOrders =
                orders
                    .Select(x =>
                        new SelectListItem
                        {
                            Value =
                                x.Id.ToString(),

                            Text =
                                $"{x.OrderNumber} - " +
                                $"{x.CustomerName ?? "Walk-in Customer"} - " +
                                $"₹{x.GrandTotal:N2}",

                            Selected =
                                selectedId.HasValue &&
                                selectedId.Value == x.Id
                        })
                    .ToList();
        }

        // ============================================================
        // GENERATE INVOICE NUMBER
        // ============================================================

        private async Task<string>
            GenerateInvoiceNumberAsync()
        {
            string invoiceNumber;

            do
            {
                invoiceNumber =
                    $"SI-{DateTime.Now:yyyyMMddHHmmssfff}";

                var exists =
                    await _context.SalesInvoices
                        .AnyAsync(x =>
                            x.InvoiceNumber ==
                            invoiceNumber);

                if (!exists)
                    break;

                await Task.Delay(1);

            } while (true);

            return invoiceNumber;
        }

        // ============================================================
        // GET LATEST PURCHASE COST
        // ============================================================

        private async Task<decimal>
            GetLatestPurchaseCostAsync(
                int productId)
        {
            var latest =
                await _context.GoodsReceiptItems
                    .Include(x =>
                        x.GoodsReceipt)
                    .Where(x =>
                        x.ProductId ==
                        productId)
                    .OrderByDescending(x =>
                        x.GoodsReceipt!.ReceiptDate)
                    .FirstOrDefaultAsync();

            return latest?.UnitPrice ?? 0m;
        }

        // ============================================================
        // PAYMENT STATUS
        // ============================================================

        private static string GetPaymentStatus(
            decimal grandTotal,
            decimal paidAmount)
        {
            if (paidAmount <= 0)
                return "Issued";

            if (paidAmount >= grandTotal)
                return "Paid";

            return "PartiallyPaid";
        }
    }
}