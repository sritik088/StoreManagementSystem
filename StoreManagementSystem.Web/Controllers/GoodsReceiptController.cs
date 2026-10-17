
using Microsoft.AspNetCore.Authorization;
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
    [Authorize(Roles = "SuperAdmin")]
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
            ModelState.Remove(nameof(model.PurchaseOrderId));
            ModelState.Remove(nameof(model.SupplierId));
            ModelState.Remove(nameof(model.SupplierName));
            ModelState.Remove(nameof(model.SubTotal));
            ModelState.Remove(nameof(model.TaxAmount));
            ModelState.Remove(nameof(model.Discount));
            ModelState.Remove(nameof(model.GrandTotal));

            model.ReceiptType = GoodsReceiptType.Gift;

            if (string.IsNullOrWhiteSpace(model.DonorName))
            {
                ModelState.AddModelError(
                    nameof(model.DonorName),
                    "Donor name is required.");
            }

            var selectedItems = (model.Items ??
                    new List<GoodsReceiptItemViewModel>())
                .Where(x => x.ReceivedQuantity > 0)
                .ToList();

            if (selectedItems.Count == 0)
            {
                ModelState.AddModelError(
                    nameof(model.Items),
                    "Please add at least one gift item.");
            }

            // Validate gift item selections against active database records.
            var activeProductIds = await _context.Products
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .Select(x => x.Id)
                .ToListAsync();

            var activeWarehouseIds = (await _warehouseService.GetAllAsync())
                .Where(x => x.IsActive)
                .Select(x => x.Id)
                .ToHashSet();

            foreach (var item in selectedItems)
            {
                if (!activeProductIds.Contains(item.ProductId))
                {
                    ModelState.AddModelError(
                        nameof(model.Items),
                        "Please select a valid product for every gift item.");
                }

                if (!activeWarehouseIds.Contains(item.WarehouseId))
                {
                    ModelState.AddModelError(
                        nameof(model.Items),
                        "Please select a valid active warehouse for every gift item.");
                }

                if (item.ReceivedQuantity <= 0)
                {
                    ModelState.AddModelError(
                        nameof(model.Items),
                        "Gift quantity must be greater than zero.");
                }
            }

            if (!ModelState.IsValid)
            {
                await LoadGiftDropdowns(model);
                return View(model);
            }

            var receipt = new GoodsReceipt
            {
                ReceiptType = GoodsReceiptType.Gift,
                PurchaseOrderId = null,
                SupplierId = null,
                DonorName = model.DonorName!.Trim(),
                GiftReason = model.GiftReason?.Trim(),

                ReceiptDate = model.ReceiptDate == default
                    ? DateTime.Today
                    : model.ReceiptDate,

                Remarks = model.Remarks?.Trim(),

                SubTotal = 0m,
                TaxAmount = 0m,
                Discount = 0m,
                GrandTotal = 0m,

                Status = GoodsReceiptStatus.Received,
                IsDeleted = false,
                CreatedDate = DateTime.Now,

                Items = selectedItems.Select(item =>
                    new GoodsReceiptItem
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
                    }).ToList()
            };

            try
            {
                var result =
                    await _goodsReceiptService.CreateGiftAsync(receipt);

                if (!result)
                {
                    ModelState.AddModelError(
                        string.Empty,
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
                ModelState.AddModelError(string.Empty, ex.Message);
                await LoadGiftDropdowns(model);
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "An unexpected error occurred while creating Gift GRN.");

                Console.WriteLine($"GIFT GRN ERROR: {ex}");

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
            var receipts = await _goodsReceiptService.GetAllAsync();

            return View(receipts);
        }

        // =========================================================
        // CREATE PURCHASE GRN - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new GoodsReceiptViewModel
            {
                ReceiptDate = DateTime.Today,
                ReceiptType = GoodsReceiptType.Purchase
            };

            await LoadCreateDropdowns(model);

            return View(model);
        }

        // =========================================================
        // CREATE PURCHASE GRN - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            GoodsReceiptViewModel model)
        {
            ModelState.Remove(nameof(model.SubTotal));
            ModelState.Remove(nameof(model.TaxAmount));
            ModelState.Remove(nameof(model.Discount));
            ModelState.Remove(nameof(model.GrandTotal));

            if (model.PurchaseOrderId <= 0)
            {
                ModelState.AddModelError(
                    nameof(model.PurchaseOrderId),
                    "Please select a Purchase Order.");
            }

            if (model.Items == null || !model.Items.Any())
            {
                ModelState.AddModelError(
                    string.Empty,
                    "No Purchase Order items were submitted.");
            }

            if (!ModelState.IsValid)
            {
                await LoadCreateDropdowns(model);
                return View(model);
            }

            var selectedItems = model.Items!
                .Where(x => x.ReceivedQuantity > 0)
                .ToList();

            if (selectedItems.Count == 0)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Please enter at least one Receive Quantity.");

                await LoadCreateDropdowns(model);
                return View(model);
            }

            var purchaseOrder =
                await _purchaseOrderRepository.GetByIdWithDetailsAsync(
                    model.PurchaseOrderId);

            if (purchaseOrder == null || purchaseOrder.IsDeleted)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Purchase Order was not found or has been deleted.");

                await LoadCreateDropdowns(model);
                return View(model);
            }

            if (purchaseOrder.Status != PurchaseOrderStatus.Approved &&
                purchaseOrder.Status != PurchaseOrderStatus.PartiallyReceived)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "This Purchase Order is not available for receiving.");

                await LoadCreateDropdowns(model);
                return View(model);
            }

            var activeWarehouseIds = (await _warehouseService.GetAllAsync())
                .Where(x => x.IsActive)
                .Select(x => x.Id)
                .ToHashSet();

            foreach (var item in selectedItems)
            {
                if (item.ReceivedQuantity <= 0)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        $"Receive quantity for {item.SKU} must be greater than zero.");

                    continue;
                }

                var poItem = purchaseOrder.Items.FirstOrDefault(
                    x => x.Id == item.PurchaseOrderItemId);

                if (poItem == null)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        $"Purchase Order Item {item.PurchaseOrderItemId} was not found.");

                    continue;
                }

                if (poItem.ProductId != item.ProductId)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        $"Product mismatch for Purchase Order Item {poItem.Id}.");

                    continue;
                }

                if (!activeWarehouseIds.Contains(item.WarehouseId))
                {
                    ModelState.AddModelError(
                        string.Empty,
                        $"Please select a valid active warehouse for {item.SKU}.");

                    continue;
                }

                var previouslyReceived =
                    await _goodsReceiptService
                        .GetPreviouslyReceivedQuantityAsync(poItem.Id);

                var remainingQuantity =
                    poItem.Quantity - previouslyReceived;

                if (remainingQuantity <= 0)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        $"Product {poItem.Product?.Title ?? poItem.Product?.SKU ?? poItem.Id.ToString()} " +
                        "has no remaining quantity to receive.");

                    continue;
                }

                if (item.ReceivedQuantity > remainingQuantity)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        $"Received quantity {item.ReceivedQuantity} cannot exceed " +
                        $"remaining quantity {remainingQuantity} for " +
                        $"{poItem.Product?.Title ?? poItem.Product?.SKU ?? poItem.Id.ToString()}.");

                    continue;
                }

                // Use trusted purchase order values.
                item.OrderedQuantity = poItem.Quantity;
                item.UnitPrice = poItem.UnitPrice;
                item.TaxAmount = poItem.TaxAmount;
                item.Discount = poItem.Discount;

                // Populate display values from the database product.
                item.ProductName = poItem.Product?.Title ?? string.Empty;
                item.SKU = poItem.Product?.SKU ?? string.Empty;
            }

            if (!ModelState.IsValid)
            {
                await LoadCreateDropdowns(model);
                return View(model);
            }

            decimal subTotal = 0m;
            decimal taxAmount = 0m;
            decimal discount = 0m;

            foreach (var item in selectedItems)
            {
                subTotal += item.ReceivedQuantity * item.UnitPrice;
                taxAmount += item.TaxAmount;
                discount += item.Discount;
            }

            var grandTotal = subTotal - discount + taxAmount;

            var receipt = new GoodsReceipt
            {
                ReceiptType = GoodsReceiptType.Purchase,
                PurchaseOrderId = purchaseOrder.Id,
                SupplierId = purchaseOrder.SupplierId,

                ReceiptDate = model.ReceiptDate == default
                    ? DateTime.Today
                    : model.ReceiptDate,

                Remarks = model.Remarks?.Trim(),

                SubTotal = subTotal,
                TaxAmount = taxAmount,
                Discount = discount,
                GrandTotal = grandTotal,

                Status = GoodsReceiptStatus.Received,
                IsDeleted = false,
                CreatedDate = DateTime.Now,

                Items = selectedItems.Select(item =>
                    new GoodsReceiptItem
                    {
                        PurchaseOrderItemId = item.PurchaseOrderItemId,
                        ProductId = item.ProductId,
                        WarehouseId = item.WarehouseId,
                        OrderedQuantity = item.OrderedQuantity,
                        ReceivedQuantity = item.ReceivedQuantity,
                        UnitPrice = item.UnitPrice,
                        Discount = item.Discount,
                        TaxAmount = item.TaxAmount,
                        Total = (item.ReceivedQuantity * item.UnitPrice)
                                - item.Discount
                                + item.TaxAmount
                    }).ToList()
            };

            try
            {
                var result = await _goodsReceiptService.CreateAsync(receipt);

                if (!result)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Unable to receive goods.");

                    await LoadCreateDropdowns(model);
                    return View(model);
                }

                TempData["Success"] =
                    $"Goods received successfully. GRN: {receipt.GRNNumber}";

                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await LoadCreateDropdowns(model);
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "An unexpected error occurred while receiving goods.");

                Console.WriteLine($"GRN ERROR: {ex}");

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
            var purchaseOrders =
                await _purchaseOrderRepository.GetAllAsync();

            model.PurchaseOrders = purchaseOrders
                .Where(x =>
                    !x.IsDeleted &&
                    (x.Status == PurchaseOrderStatus.Approved ||
                     x.Status == PurchaseOrderStatus.PartiallyReceived))
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = $"{x.PONumber} - {x.Supplier?.Name ?? "Unknown Supplier"}",
                    Selected = x.Id == model.PurchaseOrderId
                })
                .ToList();

            var warehouses = await _warehouseService.GetAllAsync();

            model.Warehouses = warehouses
                .Where(x => x.IsActive)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name,
                    Selected = x.Id == (model.Warehouses
                        .FirstOrDefault(w => w.Selected)?.Value is string selectedId
                            && int.TryParse(selectedId, out var selectedWarehouseId)
                                ? selectedWarehouseId
                                : 0)
                })
                .ToList();
        }

        // =========================================================
        // LOAD GIFT GRN DROPDOWNS
        // =========================================================

        private async Task LoadGiftDropdowns(
            GoodsReceiptViewModel model)
        {
            model.PurchaseOrders = new List<SelectListItem>();

            var warehouses = await _warehouseService.GetAllAsync();

            model.Warehouses = warehouses
     .Where(x => x.IsActive)
     .Select(x => new SelectListItem
     {
         Value = x.Id.ToString(),
         Text = x.Name
     })
     .ToList();

            var products = await _context.Products
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Title)
                .ToListAsync();

            model.Products = products
     .Select(x => new SelectListItem
     {
         Value = x.Id.ToString(),
         Text = string.IsNullOrWhiteSpace(x.SKU)
             ? x.Title
             : $"{x.Title} ({x.SKU})"
     })
     .ToList();
        }

        // =========================================================
        // GET PURCHASE ORDER DETAILS FOR GRN
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> GetPurchaseOrder(int id)
        {
            if (id <= 0)
                return BadRequest();

            var purchaseOrder =
                await _purchaseOrderRepository.GetByIdWithDetailsAsync(id);

            if (purchaseOrder == null || purchaseOrder.IsDeleted)
                return NotFound();

            if (purchaseOrder.Status != PurchaseOrderStatus.Approved &&
                purchaseOrder.Status != PurchaseOrderStatus.PartiallyReceived)
            {
                return BadRequest(
                    "This Purchase Order is not available for receiving.");
            }

            var items = new List<object>();

            foreach (var item in purchaseOrder.Items)
            {
                var previouslyReceived =
                    await _goodsReceiptService
                        .GetPreviouslyReceivedQuantityAsync(item.Id);

                var remaining = item.Quantity - previouslyReceived;

                if (remaining <= 0)
                    continue;

                items.Add(new
                {
                    purchaseOrderItemId = item.Id,
                    productId = item.ProductId,

                    // NEW: Product Title is distinct from SKU.
                    productTitle = item.Product?.Title ?? string.Empty,
                    sku = item.Product?.SKU ?? string.Empty,

                    orderedQty = item.Quantity,
                    previouslyReceivedQty = previouslyReceived,
                    remainingQty = remaining,
                    price = item.UnitPrice,
                    tax = item.TaxAmount,
                    discount = item.Discount
                });
            }

            return Json(new
            {
                id = purchaseOrder.Id,
                poNumber = purchaseOrder.PONumber,
                supplierId = purchaseOrder.SupplierId,
                supplier = purchaseOrder.Supplier?.Name ?? string.Empty,
                items
            });
        }

        // =========================================================
        // GET WAREHOUSES
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> GetWarehouses()
        {
            var warehouses = await _warehouseService.GetAllAsync();

            return Json(warehouses
                .Where(x => x.IsActive)
                .Select(x => new
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
                var result = await _goodsReceiptService.DeleteAsync(id);

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
                TempData["Error"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    "Unable to delete the Goods Receipt. " +
                    "No stock or transaction changes were applied.";

                Console.WriteLine($"GRN DELETE ERROR: {ex}");
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
                    TempData["Error"] = "Invalid Goods Receipt ID.";
                    return RedirectToAction(nameof(Index));
                }

                var result = await _goodsReceiptService.RestoreAsync(id);

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
                TempData["Error"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    "Unable to restore the Goods Receipt.";

                Console.WriteLine($"GRN RESTORE ERROR: {ex}");
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // DETAILS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0)
                return BadRequest();

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
                return NotFound();

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

                Items = grn.Items.Select(item =>
                    new GoodsReceiptItemViewModel
                    {
                        Id = item.Id,
                        PurchaseOrderItemId = item.PurchaseOrderItemId,
                        ProductId = item.ProductId,

                        // FIX: Show the actual Product Title.
                        ProductName = item.Product?.Title ?? string.Empty,

                        // Keep SKU as its own field.
                        SKU = item.Product?.SKU ?? string.Empty,

                        OrderedQuantity = item.OrderedQuantity,
                        ReceivedQuantity = item.ReceivedQuantity,
                        WarehouseId = item.WarehouseId,
                        UnitPrice = item.UnitPrice,
                        Discount = item.Discount,
                        TaxAmount = item.TaxAmount,
                        Total = item.Total,

                        RemainingQuantity = grn.ReceiptType ==
                            GoodsReceiptType.Gift
                                ? 0m
                                : Math.Max(
                                    0m,
                                    item.OrderedQuantity - item.ReceivedQuantity)
                    }).ToList()
            };

            return View("~/Views/GoodsReceipt/Details.cshtml", model);
        }

        // =========================================================
        // PRINT
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Print(int id)
        {
            if (id <= 0)
                return BadRequest();

            var receipt = await _goodsReceiptService.GetByIdAsync(id);

            if (receipt == null)
                return NotFound();

            return View(receipt);
        }
    }
}
