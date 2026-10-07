using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Domain.Enums;
using StoreManagementSystem.Infrastructure.Data;
using StoreManagementSystem.Web.ViewModels.GoodsReceipt;

namespace StoreManagementSystem.Web.Controllers
{
    public class GoodsReceiptController : Controller
    {
        private readonly IGoodsReceiptService _goodsReceiptService;
        private readonly IPurchaseOrderRepository _purchaseOrderRepository;
        private readonly IWarehouseService _warehouseService;
        private readonly ApplicationDbContext _context;

        public GoodsReceiptController(
            IGoodsReceiptService goodsReceiptService,
            IPurchaseOrderRepository purchaseOrderRepository,
            IWarehouseService warehouseService,
            ApplicationDbContext context)
        {
            _goodsReceiptService = goodsReceiptService;
            _purchaseOrderRepository = purchaseOrderRepository;
            _warehouseService = warehouseService;
            _context = context;
        }

        // =========================================================
        // GIFT GRN - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Gift()
        {
            var model = new GoodsReceiptViewModel
            {
                ReceiptType = GoodsReceiptType.Gift,
                ReceiptDate = DateTime.Today
            };

            await LoadGiftDropdowns(model);

            return View(model);
        }

        // =========================================================
        // GIFT GRN - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Gift(
            GoodsReceiptViewModel model)
        {
            // -----------------------------------------------------
            // Purchase-only validation is irrelevant for Gift
            // -----------------------------------------------------

            ModelState.Remove(nameof(model.PurchaseOrderId));
            ModelState.Remove(nameof(model.SupplierId));
            ModelState.Remove(nameof(model.SupplierName));
            ModelState.Remove(nameof(model.SubTotal));
            ModelState.Remove(nameof(model.TaxAmount));
            ModelState.Remove(nameof(model.Discount));
            ModelState.Remove(nameof(model.GrandTotal));

            model.ReceiptType = GoodsReceiptType.Gift;

            // =====================================================
            // DONOR VALIDATION
            // =====================================================

            if (string.IsNullOrWhiteSpace(model.DonorName))
            {
                ModelState.AddModelError(
                    nameof(model.DonorName),
                    "Donor name is required.");
            }

            // =====================================================
            // ITEMS
            // =====================================================

            var selectedItems = (model.Items ?? new List<GoodsReceiptItemViewModel>())
                .Where(x => x.ReceivedQuantity > 0)
                .ToList();

            if (selectedItems.Count == 0)
            {
                ModelState.AddModelError(
                    nameof(model.Items),
                    "Please add at least one gift item.");
            }

            // =====================================================
            // ITEM VALIDATION
            // =====================================================

            foreach (var item in selectedItems)
            {
                if (item.ProductId <= 0)
                {
                    ModelState.AddModelError(
                        nameof(model.Items),
                        "Please select a product for every gift item.");
                }

                if (item.WarehouseId <= 0)
                {
                    ModelState.AddModelError(
                        nameof(model.Items),
                        "Please select a warehouse for every gift item.");
                }

                if (item.ReceivedQuantity <= 0)
                {
                    ModelState.AddModelError(
                        nameof(model.Items),
                        "Gift quantity must be greater than zero.");
                }
            }

            // =====================================================
            // RETURN VIEW IF VALIDATION FAILED
            // =====================================================

            if (!ModelState.IsValid)
            {
                await LoadGiftDropdowns(model);
                return View(model);
            }

            // =====================================================
            // CREATE GIFT GOODS RECEIPT
            // =====================================================

            var receipt = new GoodsReceipt
            {
                ReceiptType = GoodsReceiptType.Gift,

                // Gift has no Purchase Order
                PurchaseOrderId = null,

                // Gift has no Supplier
                SupplierId = null,

                DonorName = model.DonorName?.Trim(),

                GiftReason = model.GiftReason?.Trim(),

                ReceiptDate =
                    model.ReceiptDate == default
                        ? DateTime.Today
                        : model.ReceiptDate,

                Remarks = model.Remarks?.Trim(),

                // Gift has zero financial value
                SubTotal = 0m,
                TaxAmount = 0m,
                Discount = 0m,
                GrandTotal = 0m,

                Status = GoodsReceiptStatus.Received,

                IsDeleted = false,

                CreatedDate = DateTime.Now,

                Items = selectedItems
                    .Select(item => new GoodsReceiptItem
                    {
                        PurchaseOrderItemId = null,

                        ProductId = item.ProductId,

                        WarehouseId = item.WarehouseId,

                        OrderedQuantity = 0m,

                        ReceivedQuantity = item.ReceivedQuantity,

                        UnitPrice = 0m,

                        Discount = 0m,

                        TaxAmount = 0m,

                        Total = 0m
                    })
                    .ToList()
            };

            // =====================================================
            // SAVE GIFT GRN
            // =====================================================

            try
            {
                var result =
                    await _goodsReceiptService.CreateGiftAsync(receipt);

                if (!result)
                {
                    ModelState.AddModelError(
                        "",
                        "Unable to create Gift GRN.");

                    await LoadGiftDropdowns(model);

                    return View(model);
                }

                TempData["Success"] =
                    $"Gift GRN {receipt.GRNNumber} created successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(
                    "",
                    ex.Message);

                await LoadGiftDropdowns(model);

                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    "",
                    "An unexpected error occurred while creating Gift GRN.");

                Console.WriteLine(
                    $"GIFT GRN ERROR: {ex}");

                await LoadGiftDropdowns(model);

                return View(model);
            }
        }

        // =========================================================
        // INDEX
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var receipts =
                await _goodsReceiptService.GetAllAsync();

            return View(receipts);
        }

        // =========================================================
        // CREATE - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new GoodsReceiptViewModel
            {
                ReceiptDate = DateTime.Today
            };

            await LoadCreateDropdowns(model);

            return View(model);
        }

        // =========================================================
        // CREATE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            GoodsReceiptViewModel model)
        {
            // -----------------------------------------------------
            // Calculated fields are generated on server
            // -----------------------------------------------------

            ModelState.Remove(nameof(model.SubTotal));
            ModelState.Remove(nameof(model.TaxAmount));
            ModelState.Remove(nameof(model.Discount));
            ModelState.Remove(nameof(model.GrandTotal));

            // -----------------------------------------------------
            // Validate Purchase Order
            // -----------------------------------------------------

            if (model.PurchaseOrderId <= 0)
            {
                ModelState.AddModelError(
                    nameof(model.PurchaseOrderId),
                    "Please select a Purchase Order.");
            }

            // -----------------------------------------------------
            // Validate items
            // -----------------------------------------------------

            if (model.Items == null || !model.Items.Any())
            {
                ModelState.AddModelError(
                    "",
                    "No Purchase Order items were submitted.");
            }

            if (!ModelState.IsValid)
            {
                await LoadCreateDropdowns(model);
                return View(model);
            }

            // -----------------------------------------------------
            // Only process items where Receive Qty > 0
            // -----------------------------------------------------

            var selectedItems = model.Items
                .Where(x => x.ReceivedQuantity > 0)
                .ToList();

            if (!selectedItems.Any())
            {
                ModelState.AddModelError(
                    "",
                    "Please enter at least one Receive Quantity.");

                await LoadCreateDropdowns(model);
                return View(model);
            }

            // =====================================================
            // LOAD PURCHASE ORDER WITH ITEMS
            // =====================================================

            var purchaseOrder =
                await _purchaseOrderRepository
                    .GetByIdWithDetailsAsync(
                        model.PurchaseOrderId);

            if (purchaseOrder == null)
            {
                ModelState.AddModelError(
                    "",
                    "Purchase Order was not found.");

                await LoadCreateDropdowns(model);
                return View(model);
            }

            // -----------------------------------------------------
            // Deleted PO cannot be received
            // -----------------------------------------------------

            if (purchaseOrder.IsDeleted)
            {
                ModelState.AddModelError(
                    "",
                    "This Purchase Order has been deleted.");

                await LoadCreateDropdowns(model);
                return View(model);
            }

            // -----------------------------------------------------
            // Validate PO status
            // -----------------------------------------------------

            if (purchaseOrder.Status != PurchaseOrderStatus.Approved &&
                purchaseOrder.Status != PurchaseOrderStatus.PartiallyReceived)
            {
                ModelState.AddModelError(
                    "",
                    "This Purchase Order is not available for receiving.");

                await LoadCreateDropdowns(model);
                return View(model);
            }

            // =====================================================
            // VALIDATE EACH RECEIVING ITEM
            // =====================================================

            foreach (var item in selectedItems)
            {
                // -------------------------------------------------
                // Basic quantity validation
                // -------------------------------------------------

                if (item.ReceivedQuantity <= 0)
                {
                    ModelState.AddModelError(
                        "",
                        $"Receive quantity for {item.SKU} must be greater than zero.");

                    continue;
                }

                // -------------------------------------------------
                // Find actual PO item from DATABASE
                // -------------------------------------------------

                var poItem = purchaseOrder.Items
                    .FirstOrDefault(x =>
                        x.Id == item.PurchaseOrderItemId);

                if (poItem == null)
                {
                    ModelState.AddModelError(
                        "",
                        $"Purchase Order Item {item.PurchaseOrderItemId} was not found.");

                    continue;
                }

                // -------------------------------------------------
                // Product must match
                // -------------------------------------------------

                if (poItem.ProductId != item.ProductId)
                {
                    ModelState.AddModelError(
                        "",
                        $"Product mismatch for Purchase Order Item {poItem.Id}.");

                    continue;
                }

                // -------------------------------------------------
                // Warehouse validation
                // -------------------------------------------------

                if (item.WarehouseId <= 0)
                {
                    ModelState.AddModelError(
                        "",
                        $"Please select a warehouse for {item.SKU}.");

                    continue;
                }

                // =================================================
                // GET ACTUAL RECEIVED QUANTITY FROM DATABASE
                // =================================================

                var previouslyReceived =
                    await _goodsReceiptService
                        .GetPreviouslyReceivedQuantityAsync(
                            poItem.Id);

                // =================================================
                // CALCULATE REAL REMAINING QUANTITY
                // =================================================

                var remainingQuantity =
                    poItem.Quantity - previouslyReceived;

                Console.WriteLine(
                    $"PO Item ID: {poItem.Id}");

                Console.WriteLine(
                    $"Ordered: {poItem.Quantity}");

                Console.WriteLine(
                    $"Previously Received: {previouslyReceived}");

                Console.WriteLine(
                    $"Remaining: {remainingQuantity}");

                Console.WriteLine(
                    $"Trying to Receive: {item.ReceivedQuantity}");

                // -------------------------------------------------
                // Nothing remaining
                // -------------------------------------------------

                if (remainingQuantity <= 0)
                {
                    ModelState.AddModelError(
                        "",
                        $"PO Item {poItem.Id} has no remaining quantity. " +
                        $"Ordered: {poItem.Quantity}, " +
                        $"Previously Received: {previouslyReceived}.");

                    continue;
                }

                // -------------------------------------------------
                // Cannot receive more than remaining
                // -------------------------------------------------

                if (item.ReceivedQuantity > remainingQuantity)
                {
                    ModelState.AddModelError(
                        "",
                        $"Received quantity {item.ReceivedQuantity} " +
                        $"cannot be greater than remaining quantity " +
                        $"{remainingQuantity} for {item.SKU}.");

                    continue;
                }

                // =================================================
                // COPY TRUSTED VALUES FROM DATABASE
                // =================================================

                item.OrderedQuantity =
                    poItem.Quantity;

                item.UnitPrice =
                    poItem.UnitPrice;

                item.TaxAmount =
                    poItem.TaxAmount;

                item.Discount =
                    poItem.Discount;
            }

            // -----------------------------------------------------
            // Stop if validation failed
            // -----------------------------------------------------

            if (!ModelState.IsValid)
            {
                await LoadCreateDropdowns(model);
                return View(model);
            }

            // =====================================================
            // CALCULATE TOTALS
            // =====================================================

            decimal subTotal = 0m;
            decimal taxAmount = 0m;
            decimal discount = 0m;

            foreach (var item in selectedItems)
            {
                var lineSubTotal =
                    item.ReceivedQuantity *
                    item.UnitPrice;

                subTotal += lineSubTotal;

                taxAmount += item.TaxAmount;

                discount += item.Discount;
            }

            var grandTotal =
                subTotal
                - discount
                + taxAmount;

            // =====================================================
            // CREATE GOODS RECEIPT
            // =====================================================

            var receipt = new GoodsReceipt
            {
                ReceiptType = GoodsReceiptType.Purchase,

                PurchaseOrderId =
                    purchaseOrder.Id,

                SupplierId =
                    purchaseOrder.SupplierId,

                ReceiptDate =
                    model.ReceiptDate == default
                        ? DateTime.Today
                        : model.ReceiptDate,

                Remarks =
                    model.Remarks,

                SubTotal =
                    subTotal,

                TaxAmount =
                    taxAmount,

                Discount =
                    discount,

                GrandTotal =
                    grandTotal,

                Items =
                    selectedItems
                        .Select(item =>
                            new GoodsReceiptItem
                            {
                                PurchaseOrderItemId =
                                    item.PurchaseOrderItemId,

                                ProductId =
                                    item.ProductId,

                                WarehouseId =
                                    item.WarehouseId,

                                OrderedQuantity =
                                    item.OrderedQuantity,

                                ReceivedQuantity =
                                    item.ReceivedQuantity,

                                UnitPrice =
                                    item.UnitPrice,

                                Discount =
                                    item.Discount,

                                TaxAmount =
                                    item.TaxAmount
                            })
                        .ToList()
            };

            // =====================================================
            // SAVE GOODS RECEIPT
            // =====================================================

            try
            {
                var result =
                    await _goodsReceiptService
                        .CreateAsync(receipt);

                if (!result)
                {
                    ModelState.AddModelError(
                        "",
                        "Unable to receive goods.");

                    await LoadCreateDropdowns(model);

                    return View(model);
                }

                TempData["Success"] =
                    $"Goods received successfully. " +
                    $"GRN: {receipt.GRNNumber}";

                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(
                    "",
                    ex.Message);

                await LoadCreateDropdowns(model);

                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    "",
                    "An unexpected error occurred while receiving goods.");

                Console.WriteLine(
                    $"GRN ERROR: {ex}");

                await LoadCreateDropdowns(model);

                return View(model);
            }
        }

        // =========================================================
        // LOAD PURCHASE GRN DROPDOWNS
        // =========================================================

        private async Task LoadCreateDropdowns(
            GoodsReceiptViewModel model)
        {
            // -----------------------------------------------------
            // Purchase Orders
            // -----------------------------------------------------

            var purchaseOrders =
                await _purchaseOrderRepository
                    .GetAllAsync();

            model.PurchaseOrders =
                purchaseOrders
                    .Where(x =>
                        !x.IsDeleted &&
                        (
                            x.Status == PurchaseOrderStatus.Approved ||
                            x.Status == PurchaseOrderStatus.PartiallyReceived
                        ))
                    .Select(x =>
                        new SelectListItem
                        {
                            Value =
                                x.Id.ToString(),

                            Text =
                                $"{x.PONumber} - " +
                                $"{x.Supplier?.Name ?? "Unknown Supplier"}"
                        })
                    .ToList();

            // -----------------------------------------------------
            // Warehouses
            // -----------------------------------------------------

            var warehouses =
                await _warehouseService
                    .GetAllAsync();

            model.Warehouses =
                warehouses
                    .Where(x => x.IsActive)
                    .Select(x =>
                        new SelectListItem
                        {
                            Value =
                                x.Id.ToString(),

                            Text =
                                x.Name
                        })
                    .ToList();
        }

        // =========================================================
        // LOAD GIFT GRN DROPDOWNS
        // =========================================================

        private async Task LoadGiftDropdowns(
            GoodsReceiptViewModel model)
        {
            // -----------------------------------------------------
            // Purchase Orders are NOT required for Gift GRN
            // -----------------------------------------------------

            model.PurchaseOrders =
                Enumerable.Empty<SelectListItem>();

            // -----------------------------------------------------
            // Warehouses
            // -----------------------------------------------------

            var warehouses =
                await _warehouseService
                    .GetAllAsync();

            model.Warehouses =
                warehouses
                    .Where(x => x.IsActive)
                    .Select(x =>
                        new SelectListItem
                        {
                            Value =
                                x.Id.ToString(),

                            Text =
                                x.Name
                        })
                    .ToList();

            // -----------------------------------------------------
            // Products
            // -----------------------------------------------------

            var products =
                await _context.Products
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .OrderBy(x => x.SKU)
                    .ToListAsync();

            model.Products =
                products
                    .Select(x =>
                        new SelectListItem
                        {
                            Value =
                                x.Id.ToString(),

                            Text =
                                string.IsNullOrWhiteSpace(x.SKU)
                                    ? x.SKU
                                    : $"{x.SKU} ({x.SKU})"
                        })
                    .ToList();
        }

        // =========================================================
        // GET PURCHASE ORDER DETAILS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> GetPurchaseOrder(
            int id)
        {
            if (id <= 0)
                return BadRequest();

            var purchaseOrder =
                await _purchaseOrderRepository
                    .GetByIdWithDetailsAsync(id);

            if (purchaseOrder == null)
                return NotFound();

            if (purchaseOrder.IsDeleted)
            {
                return BadRequest(
                    "This Purchase Order has been deleted.");
            }

            if (purchaseOrder.Status != PurchaseOrderStatus.Approved &&
                purchaseOrder.Status != PurchaseOrderStatus.PartiallyReceived)
            {
                return BadRequest(
                    "This Purchase Order is not available for receiving.");
            }

            var items = new List<object>();

            foreach (var item in purchaseOrder.Items)
            {
                // -------------------------------------------------
                // Get real received quantity
                // -------------------------------------------------

                var previouslyReceived =
                    await _goodsReceiptService
                        .GetPreviouslyReceivedQuantityAsync(
                            item.Id);

                // -------------------------------------------------
                // Calculate remaining
                // -------------------------------------------------

                var remaining =
                    item.Quantity -
                    previouslyReceived;

                if (remaining <= 0)
                    continue;

                items.Add(
                    new
                    {
                        purchaseOrderItemId =
                            item.Id,

                        productId =
                            item.ProductId,

                        sku =
                            item.Product?.SKU ?? "",

                        orderedQty =
                            item.Quantity,

                        previouslyReceivedQty =
                            previouslyReceived,

                        remainingQty =
                            remaining,

                        price =
                            item.UnitPrice,

                        tax =
                            item.TaxAmount,

                        discount =
                            item.Discount
                    });
            }

            return Json(
                new
                {
                    id =
                        purchaseOrder.Id,

                    poNumber =
                        purchaseOrder.PONumber,

                    supplierId =
                        purchaseOrder.SupplierId,

                    supplier =
                        purchaseOrder.Supplier?.Name
                        ?? "",

                    items
                });
        }

        // =========================================================
        // GET WAREHOUSES
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> GetWarehouses()
        {
            var warehouses =
                await _warehouseService
                    .GetAllAsync();

            return Json(
                warehouses
                    .Where(x => x.IsActive)
                    .Select(x =>
                        new
                        {
                            id = x.Id,
                            name = x.Name
                        }));
        }

        // =========================================================
        // DELETE
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                bool result =
                await _goodsReceiptService
                .DeleteAsync(id);


    if (result)
                {
                    TempData["Success"] =
                        "Goods Receipt reversed and deleted successfully.";
                }
                else
                {
                    TempData["Error"] =
                        "Goods Receipt was not found or has already been deleted.";
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] =
                    ex.Message;
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "Unable to delete the Goods Receipt. " +
                    "No stock or transaction changes were applied.";
            }

            return RedirectToAction(nameof(Index));


        }

        // =========================================================
        // RESTORE DELETED GOODS RECEIPT
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            try
            {
                if (id <= 0)
                {
                    TempData["Error"] =
                        "Invalid Goods Receipt ID.";

                    return RedirectToAction(nameof(Index));
                }

                var result =
                    await _goodsReceiptService
                        .RestoreAsync(id);

                if (result)
                {
                    TempData["Success"] =
                        "Goods Receipt restored successfully.";
                }
                else
                {
                    TempData["Error"] =
                        "Goods Receipt was not found or is already active.";
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] =
                    ex.Message;
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    "Unable to restore the Goods Receipt.";

                Console.WriteLine(
                    $"GRN RESTORE ERROR: {ex}");
            }

            return RedirectToAction(nameof(Index));
        }



        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var grn = await _context.GoodsReceipts
                .AsNoTracking()
                .Include(x => x.Supplier)
                .Include(x => x.PurchaseOrder)
                .Include(x => x.Items)
                    .ThenInclude(x => x.Product)
                .Include(x => x.Items)
                    .ThenInclude(x => x.Warehouse)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDeleted);

            if (grn == null)
            {
                return NotFound();
            }

            var model = new GoodsReceiptViewModel
            {
                Id = grn.Id,
                GRNNumber = grn.GRNNumber,
                ReceiptType = grn.ReceiptType,

                PurchaseOrderId = grn.PurchaseOrderId ?? 0,

                SupplierId = grn.SupplierId ?? 0,

                SupplierName = grn.Supplier?.Name,

                DonorName = grn.DonorName,

                GiftReason = grn.GiftReason,

                ReceiptDate = grn.ReceiptDate,

                SubTotal = grn.SubTotal,

                Discount = grn.Discount,

                TaxAmount = grn.TaxAmount,

                GrandTotal = grn.GrandTotal,

                Remarks = grn.Remarks,

                Items = grn.Items
                    .Select(item => new GoodsReceiptItemViewModel
                    {
                        Id = item.Id,

                        PurchaseOrderItemId = item.PurchaseOrderItemId,

                        ProductId = item.ProductId,

                        ProductName = item.Product?.SKU,

                        SKU = item.Product?.SKU,

                       

                        OrderedQuantity = item.OrderedQuantity,

                        ReceivedQuantity = item.ReceivedQuantity,

                        WarehouseId = item.WarehouseId,

                        UnitPrice = item.UnitPrice,

                        Discount = item.Discount,

                        TaxAmount = item.TaxAmount,

                        Total = item.Total,

                        RemainingQuantity =
                            Math.Max(
                                0,
                                item.OrderedQuantity - item.ReceivedQuantity
                            )
                    })
                    .ToList()
            };

            return View("~/Views/GoodsReceipt/Details.cshtml", model);
        }

        // =========================================================
        // PRINT
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Print(
            int id)
        {
            var receipt =
                await _goodsReceiptService
                    .GetByIdAsync(id);

            if (receipt == null)
                return NotFound();

            return View(receipt);
        }
    }
}