using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreManagementSystem.Domain.Enums;
using StoreManagementSystem.Infrastructure.Data;
using StoreManagementSystem.Web.ViewModels.Damage;

namespace StoreManagementSystem.Web.Controllers
{
    public class DamageEntryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DamageEntryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // INDEX
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var damageEntries = await _context.DamageEntries
                .AsNoTracking()
                .Include(x => x.Damage)
                .Include(x => x.Product)
                .Include(x => x.Warehouse)
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.DamageDate)
                .ThenByDescending(x => x.Id)
                .ToListAsync();

            return View(damageEntries);
        }

        // ============================================================
        // CREATE - GET
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new DamageEntryViewModel
            {
                DamageDate = DateTime.Now,
                DamageNumber = await GenerateDamageNumberAsync()
            };

            await LoadDropdownsAsync(model);

            return View(model);
        }

        // ============================================================
        // CREATE - POST
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DamageEntryViewModel model)
        {
            // --------------------------------------------------------
            // Remove calculated fields from validation
            // --------------------------------------------------------

            ModelState.Remove(nameof(model.DamageNumber));
            ModelState.Remove(nameof(model.UnitCost));
            ModelState.Remove(nameof(model.TotalValue));
            ModelState.Remove(nameof(model.AvailableQuantity));

            // --------------------------------------------------------
            // Validate Damage Type
            // --------------------------------------------------------

            var damage = await _context.Damages
                .FirstOrDefaultAsync(x =>
                    x.Id == model.DamageId &&
                    x.IsActive &&
                    !x.IsDeleted);

            if (damage == null)
            {
                ModelState.AddModelError(
                    nameof(model.DamageId),
                    "Please select a valid damage type.");
            }

            // --------------------------------------------------------
            // Validate Product
            // --------------------------------------------------------

            var product = await _context.Products
                .FirstOrDefaultAsync(x =>
                    x.Id == model.ProductId &&
                    !x.IsDeleted);

            if (product == null)
            {
                ModelState.AddModelError(
                    nameof(model.ProductId),
                    "Please select a valid product.");
            }

            // --------------------------------------------------------
            // Validate Warehouse
            // --------------------------------------------------------

            var warehouse = await _context.Warehouses
                .FirstOrDefaultAsync(x =>
                    x.Id == model.WarehouseId &&
                    x.IsActive &&
                    !x.IsDeleted);

            if (warehouse == null)
            {
                ModelState.AddModelError(
                    nameof(model.WarehouseId),
                    "Please select a valid active warehouse.");
            }

            // --------------------------------------------------------
            // Validate quantity
            // --------------------------------------------------------

            if (model.Quantity <= 0)
            {
                ModelState.AddModelError(
                    nameof(model.Quantity),
                    "Damage quantity must be greater than zero.");
            }

            // --------------------------------------------------------
            // If basic validation failed, reload dropdowns
            // --------------------------------------------------------

            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync(model);

                return View(model);
            }

            // --------------------------------------------------------
            // Find warehouse stock
            // --------------------------------------------------------

            var warehouseStock = await _context.WarehouseStocks
                .FirstOrDefaultAsync(x =>
                    x.ProductId == model.ProductId &&
                    x.WarehouseId == model.WarehouseId);

            if (warehouseStock == null)
            {
                ModelState.AddModelError(
                    nameof(model.Quantity),
                    "No stock record exists for the selected product and warehouse.");

                await LoadDropdownsAsync(model);

                return View(model);
            }

            // --------------------------------------------------------
            // Available stock
            // --------------------------------------------------------

            var availableQuantity = warehouseStock.AvailableQuantity;

            model.AvailableQuantity = availableQuantity;

            // --------------------------------------------------------
            // Prevent damage quantity greater than available quantity
            // --------------------------------------------------------

            if (model.Quantity > availableQuantity)
            {
                ModelState.AddModelError(
                    nameof(model.Quantity),
                    $"Insufficient available stock. Available quantity is {availableQuantity:N2}.");

                await LoadDropdownsAsync(model);

                return View(model);
            }

            // --------------------------------------------------------
            // Calculate unit cost
            //
            // Use latest received purchase price for this product and
            // warehouse.
            // --------------------------------------------------------

            var latestReceiptItem = await _context.GoodsReceiptItems
                .AsNoTracking()
                .Include(x => x.GoodsReceipt)
                .Where(x =>
                    x.ProductId == model.ProductId &&
                    x.WarehouseId == model.WarehouseId &&
                    x.ReceivedQuantity > 0 &&
                    x.GoodsReceipt != null &&
                    !x.GoodsReceipt.IsDeleted &&
                    x.GoodsReceipt.Status == GoodsReceiptStatus.Received)
                .OrderByDescending(x => x.GoodsReceipt!.ReceiptDate)
                .ThenByDescending(x => x.Id)
                .FirstOrDefaultAsync();

            decimal unitCost = 0m;

            if (latestReceiptItem != null)
            {
                unitCost = latestReceiptItem.UnitPrice;
            }

            // --------------------------------------------------------
            // Calculate total damage value
            // --------------------------------------------------------

            var totalValue = model.Quantity * unitCost;

            // --------------------------------------------------------
            // Generate damage number
            // --------------------------------------------------------

            var damageNumber = await GenerateDamageNumberAsync();

            // --------------------------------------------------------
            // Calculate new stock balance
            // --------------------------------------------------------

            var balanceBefore = warehouseStock.QuantityOnHand;

            warehouseStock.QuantityOnHand -= model.Quantity;

            // --------------------------------------------------------
            // Safety check
            // --------------------------------------------------------

            if (warehouseStock.QuantityOnHand < 0)
            {
                ModelState.AddModelError(
                    nameof(model.Quantity),
                    "Damage quantity cannot reduce stock below zero.");

                // Restore in-memory value
                warehouseStock.QuantityOnHand = balanceBefore;

                await LoadDropdownsAsync(model);

                return View(model);
            }

            warehouseStock.LastUpdated = DateTime.Now;

            // --------------------------------------------------------
            // Create Damage Entry
            // --------------------------------------------------------

            var damageEntry = new Domain.Entities.DamageEntry
            {
                DamageNumber = damageNumber,
                DamageDate = model.DamageDate,

                DamageId = model.DamageId,
                ProductId = model.ProductId,
                WarehouseId = model.WarehouseId,

                Quantity = model.Quantity,
                UnitCost = unitCost,
                TotalValue = totalValue,

                Remarks = string.IsNullOrWhiteSpace(model.Remarks)
                    ? null
                    : model.Remarks.Trim(),

                IsDeleted = false,
                CreatedDate = DateTime.Now
            };

            _context.DamageEntries.Add(damageEntry);

            // --------------------------------------------------------
            // Create Stock Ledger entry
            // --------------------------------------------------------

            var stockLedger = new Domain.Entities.StockLedger
            {
                ProductId = model.ProductId,
                WarehouseId = model.WarehouseId,

                TransactionType = StockTransactionType.StockAdjustment,

                // Negative quantity because stock is going OUT
                Quantity = -model.Quantity,

                BalanceAfterTransaction =
                    warehouseStock.QuantityOnHand,

                UnitCost = unitCost,

                ReferenceNo = damageNumber,

                TransactionDate = model.DamageDate,

                Remarks = $"Damage - {damage.DamageName}" +
                          (string.IsNullOrWhiteSpace(model.Remarks)
                              ? string.Empty
                              : $" - {model.Remarks.Trim()}"),

                CreatedBy = User?.Identity?.Name ?? "System"
            };

            _context.StockLedgers.Add(stockLedger);

            // --------------------------------------------------------
            // Save everything in one transaction
            // --------------------------------------------------------

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["SuccessMessage"] =
                    $"Damage entry {damageNumber} created successfully. " +
                    $"{model.Quantity:N2} unit(s) removed from warehouse stock.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    string.Empty,
                    "An error occurred while saving the damage entry. Please try again.");

                await LoadDropdownsAsync(model);

                return View(model);
            }
        }

        // ============================================================
        // DELETE / SOFT DELETE
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var damageEntry = await _context.DamageEntries
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDeleted);

            if (damageEntry == null)
            {
                TempData["ErrorMessage"] =
                    "Damage entry was not found.";

                return RedirectToAction(nameof(Index));
            }

            /*
             * IMPORTANT:
             *
             * A damage entry should not normally be deleted after
             * stock has already been reduced.
             *
             * Therefore this action only soft-deletes the record.
             * It does NOT automatically add stock back.
             *
             * If you want reversal functionality later, create a
             * separate "Reverse Damage" process.
             */

            damageEntry.IsDeleted = true;
            damageEntry.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"Damage entry {damageEntry.DamageNumber} deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // GET AVAILABLE STOCK
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> GetAvailableStock(
            int productId,
            int warehouseId)
        {
            if (productId <= 0 || warehouseId <= 0)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid product or warehouse."
                });
            }

            var stock = await _context.WarehouseStocks
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.ProductId == productId &&
                    x.WarehouseId == warehouseId);

            if (stock == null)
            {
                return Json(new
                {
                    success = true,
                    availableQuantity = 0,
                    unitCost = 0
                });
            }

            // --------------------------------------------------------
            // Get latest purchase price
            // --------------------------------------------------------

            var latestReceiptItem = await _context.GoodsReceiptItems
                .AsNoTracking()
                .Include(x => x.GoodsReceipt)
                .Where(x =>
                    x.ProductId == productId &&
                    x.WarehouseId == warehouseId &&
                    x.ReceivedQuantity > 0 &&
                    x.GoodsReceipt != null &&
                    !x.GoodsReceipt.IsDeleted &&
                    x.GoodsReceipt.Status == GoodsReceiptStatus.Received)
                .OrderByDescending(x => x.GoodsReceipt!.ReceiptDate)
                .ThenByDescending(x => x.Id)
                .FirstOrDefaultAsync();

            decimal unitCost = 0m;

            if (latestReceiptItem != null)
            {
                unitCost = latestReceiptItem.UnitPrice;
            }

            return Json(new
            {
                success = true,
                availableQuantity = stock.AvailableQuantity,
                quantityOnHand = stock.QuantityOnHand,
                reservedQuantity = stock.ReservedQuantity,
                unitCost = unitCost
            });
        }

        // ============================================================
        // GET PRODUCT INFORMATION
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> GetProductInfo(int productId)
        {
            if (productId <= 0)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid product."
                });
            }

            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == productId &&
                    !x.IsDeleted);

            if (product == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Product not found."
                });
            }

            return Json(new
            {
                success = true,
                sku = product.SKU,
                barcode = product.Barcode
            });
        }

        // ============================================================
        // LOAD DROPDOWNS
        // ============================================================

        private async Task LoadDropdownsAsync(
            DamageEntryViewModel model)
        {
            // --------------------------------------------------------
            // Damage Types
            // --------------------------------------------------------

            model.Damages = await _context.Damages
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    !x.IsDeleted)
                .OrderBy(x => x.DamageName)
                .Select(x => new DamageDropdownItem
                {
                    Id = x.Id,
                    Name = x.DamageName
                })
                .ToListAsync();

            // --------------------------------------------------------
            // Products
            // --------------------------------------------------------

            model.Products = await _context.Products
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.SKU)
                .Select(x => new ProductDropdownItem
                {
                    Id = x.Id,
                    SKU = x.SKU
                })
                .ToListAsync();

            // --------------------------------------------------------
            // Warehouses
            // --------------------------------------------------------

            model.Warehouses = await _context.Warehouses
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    !x.IsDeleted)
                .OrderBy(x => x.Name)
                .Select(x => new WarehouseDropdownItem
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .ToListAsync();
        }

        // ============================================================
        // DETAILS
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var damageEntry = await _context.DamageEntries
                .AsNoTracking()
                .Include(x => x.Damage)
                .Include(x => x.Product)
                .Include(x => x.Warehouse)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDeleted);

            if (damageEntry == null)
            {
                TempData["ErrorMessage"] =
                    "Damage entry was not found.";

                return RedirectToAction(nameof(Index));
            }

            return View(damageEntry);
        }

        // ============================================================
        // REVERSE DAMAGE
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reverse(int id)
        {
            var damageEntry = await _context.DamageEntries
                .Include(x => x.Damage)
                .Include(x => x.Product)
                .Include(x => x.Warehouse)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDeleted);

            if (damageEntry == null)
            {
                TempData["ErrorMessage"] =
                    "Damage entry was not found.";

                return RedirectToAction(nameof(Index));
            }

            // --------------------------------------------------------
            // Prevent duplicate reversal
            // --------------------------------------------------------

            var reversalReference = $"REV-{damageEntry.DamageNumber}";

            var alreadyReversed = await _context.StockLedgers
                .AnyAsync(x =>
                    x.ReferenceNo == reversalReference &&
                    x.ProductId == damageEntry.ProductId &&
                    x.WarehouseId == damageEntry.WarehouseId);

            if (alreadyReversed)
            {
                TempData["ErrorMessage"] =
                    "This damage entry has already been reversed.";

                return RedirectToAction(nameof(Index));
            }

            // --------------------------------------------------------
            // Find warehouse stock
            // --------------------------------------------------------

            var warehouseStock = await _context.WarehouseStocks
                .FirstOrDefaultAsync(x =>
                    x.ProductId == damageEntry.ProductId &&
                    x.WarehouseId == damageEntry.WarehouseId);

            if (warehouseStock == null)
            {
                TempData["ErrorMessage"] =
                    "Warehouse stock record was not found. Damage cannot be reversed.";

                return RedirectToAction(nameof(Details), new { id });
            }

            // --------------------------------------------------------
            // Begin transaction
            // --------------------------------------------------------

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // --------------------------------------------------------
                // Restore damaged quantity
                // --------------------------------------------------------

                warehouseStock.QuantityOnHand += damageEntry.Quantity;
                warehouseStock.LastUpdated = DateTime.Now;

                // --------------------------------------------------------
                // Create reversal ledger
                // --------------------------------------------------------

                var reversalLedger = new Domain.Entities.StockLedger
                {
                    ProductId = damageEntry.ProductId,
                    WarehouseId = damageEntry.WarehouseId,

                    TransactionType =
                        StockTransactionType.StockAdjustment,

                    // Positive because stock is being restored
                    Quantity = damageEntry.Quantity,

                    BalanceAfterTransaction =
                        warehouseStock.QuantityOnHand,

                    UnitCost = damageEntry.UnitCost,

                    ReferenceNo = reversalReference,

                    TransactionDate = DateTime.Now,

                    Remarks =
                        $"Damage reversal - {damageEntry.DamageNumber}" +
                        (damageEntry.Damage != null
                            ? $" - {damageEntry.Damage.DamageName}"
                            : string.Empty),

                    CreatedBy =
                        User?.Identity?.Name ?? "System"
                };

                _context.StockLedgers.Add(reversalLedger);

                // --------------------------------------------------------
                // Mark original damage as deleted
                // --------------------------------------------------------

                damageEntry.IsDeleted = true;
                damageEntry.UpdatedDate = DateTime.Now;

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["SuccessMessage"] =
                    $"Damage {damageEntry.DamageNumber} reversed successfully. " +
                    $"{damageEntry.Quantity:N2} unit(s) restored to warehouse stock.";

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                await transaction.RollbackAsync();

                TempData["ErrorMessage"] =
                    "An error occurred while reversing the damage entry.";

                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // ============================================================
        // GENERATE DAMAGE NUMBER
        // ============================================================

        private async Task<string> GenerateDamageNumberAsync()
        {
            var today = DateTime.Now;

            var prefix = $"DMG-{today:yyyyMMdd}-";

            var lastDamageNumber = await _context.DamageEntries
                .AsNoTracking()
                .Where(x =>
                    x.DamageNumber.StartsWith(prefix))
                .OrderByDescending(x => x.Id)
                .Select(x => x.DamageNumber)
                .FirstOrDefaultAsync();

            var nextNumber = 1;

            if (!string.IsNullOrWhiteSpace(lastDamageNumber))
            {
                var lastPart = lastDamageNumber
                    .Substring(prefix.Length);

                if (int.TryParse(lastPart, out var lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{prefix}{nextNumber:0000}";
        }
    }
}