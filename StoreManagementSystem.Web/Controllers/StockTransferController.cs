using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Web.ViewModels.StockTransfer;

namespace StoreManagementSystem.Web.Controllers
{
    public class StockTransferController : Controller
    {
        private readonly IStockTransferService _transferService;
        private readonly IWarehouseService _warehouseService;
        private readonly IProductService _productService;
        private readonly IWarehouseStockService _stockService;
        private readonly IPurchaseOrderService _purchaseOrderService;

        public StockTransferController(
            IStockTransferService transferService,
            IWarehouseService warehouseService,
            IProductService productService,
            IWarehouseStockService stockService,
            IPurchaseOrderService purchaseOrderService)
        {
            _transferService = transferService;
            _warehouseService = warehouseService;
            _productService = productService;
            _stockService = stockService;
            _purchaseOrderService = purchaseOrderService;
        }

        // =========================================================
        // INDEX
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var transfers =
                await _transferService.GetAllAsync();

            return View(transfers);
        }

        // =========================================================
        // DETAILS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var transfer =
                await _transferService.GetByIdAsync(id);

            if (transfer == null)
                return NotFound();

            return View(transfer);
        }

        // =========================================================
        // CREATE - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new StockTransferViewModel
            {
                TransferDate = DateTime.Today
            };

            await LoadDropdowns(model);

            return View(model);
        }

        // =========================================================
        // CREATE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            StockTransferViewModel model)
        {
            if (model.Items == null)
            {
                model.Items =
                    new List<StockTransferItemViewModel>();
            }

            // =====================================================
            // WAREHOUSE VALIDATION
            // =====================================================

            if (model.FromWarehouseId <= 0)
            {
                ModelState.AddModelError(
                    "FromWarehouseId",
                    "Please select From Warehouse.");
            }

            if (model.ToWarehouseId <= 0)
            {
                ModelState.AddModelError(
                    "ToWarehouseId",
                    "Please select To Warehouse.");
            }

            if (model.FromWarehouseId > 0 &&
                model.ToWarehouseId > 0 &&
                model.FromWarehouseId == model.ToWarehouseId)
            {
                ModelState.AddModelError(
                    "ToWarehouseId",
                    "From Warehouse and To Warehouse cannot be the same.");
            }

            // =====================================================
            // ITEM VALIDATION
            // =====================================================

            if (model.Items.Count == 0)
            {
                ModelState.AddModelError(
                    "",
                    "Please add at least one product.");
            }

            foreach (var item in model.Items)
            {
                if (item.ProductId <= 0)
                {
                    ModelState.AddModelError(
                        "",
                        "Please select a product.");
                }

                if (item.Quantity <= 0)
                {
                    ModelState.AddModelError(
                        "",
                        "Quantity must be greater than zero.");
                }

                if (item.UnitCost < 0)
                {
                    ModelState.AddModelError(
                        "",
                        "Unit cost cannot be negative.");
                }
            }

            // =====================================================
            // CHECK AVAILABLE STOCK
            // =====================================================

            if (model.FromWarehouseId > 0 &&
                model.Items.Count > 0)
            {
                foreach (var item in model.Items)
                {
                    if (item.ProductId <= 0 ||
                        item.Quantity <= 0)
                    {
                        continue;
                    }

                    var available =
                        await _stockService.GetAvailableStockAsync(
                            model.FromWarehouseId,
                            item.ProductId);

                    if (item.Quantity > available)
                    {
                        ModelState.AddModelError(
                            "",
                            $"Insufficient stock for Product ID {item.ProductId}. " +
                            $"Available: {available}, Requested: {item.Quantity}.");
                    }
                }
            }

            // =====================================================
            // IF VALIDATION FAILED
            // =====================================================

            if (!ModelState.IsValid)
            {
                await LoadDropdowns(model);

                return View(model);
            }

            // =====================================================
            // CREATE DOMAIN ENTITY
            // =====================================================

            var transfer = new StockTransfer
            {
                FromWarehouseId =
                    model.FromWarehouseId,

                ToWarehouseId =
                    model.ToWarehouseId,

                TransferDate =
                    model.TransferDate == default
                        ? DateTime.Now
                        : model.TransferDate,

                Remarks =
                    model.Remarks,

                Items =
                    model.Items
                        .Select(x => new StockTransferItem
                        {
                            ProductId =
                                x.ProductId,

                            Quantity =
                                x.Quantity,

                            UnitCost =
                                x.UnitCost
                        })
                        .ToList()
            };

            // =====================================================
            // SAVE
            // =====================================================

            try
            {
                var result =
                    await _transferService.CreateAsync(
                        transfer);

                if (!result)
                {
                    TempData["Error"] =
                        "Unable to complete stock transfer.";

                    await LoadDropdowns(model);

                    return View(model);
                }

                TempData["Success"] =
                    "Stock transferred successfully.";

                return RedirectToAction(
                    nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    "",
                    ex.Message);

                await LoadDropdowns(model);

                return View(model);
            }
        }
        // =========================================================
        // CANCEL / REVERSE
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var result =
                    await _transferService
                        .CancelAsync(id);

                TempData[result
                    ? "Success"
                    : "Error"] =
                    result
                        ? "Stock transfer cancelled and stock reversed successfully."
                        : "Unable to cancel stock transfer.";
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    ex.Message;
            }

            return RedirectToAction(
                nameof(Index));
        }

        // =========================================================
        // DELETE / REVERSE STOCK TRANSFER
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                {
                    TempData["Error"] =
                        "Invalid Stock Transfer ID.";

                    return RedirectToAction(nameof(Index));
                }

                var transfer =
                    await _transferService.GetByIdAsync(id);

                if (transfer == null)
                {
                    TempData["Error"] =
                        "Stock Transfer not found or it has already been deleted.";

                    return RedirectToAction(nameof(Index));
                }

                var result =
                    await _transferService.DeleteAsync(id);

                if (result)
                {
                    TempData["Success"] =
                        $"Stock Transfer {transfer.TransferNumber} " +
                        "was reversed and deleted successfully.";
                }
                else
                {
                    TempData["Error"] =
                        $"Stock Transfer {transfer.TransferNumber} " +
                        "could not be deleted.";
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    "Unable to delete the Stock Transfer.";

                Console.WriteLine(
                    $"STOCK TRANSFER DELETE ERROR: {ex}");
            }

            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // RESTORE DELETED STOCK TRANSFER
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
                        "Invalid Stock Transfer ID.";

                    return RedirectToAction(nameof(Index));
                }

                var result =
                    await _transferService.RestoreAsync(id);

                if (result)
                {
                    TempData["Success"] =
                        "Stock Transfer restored successfully.";
                }
                else
                {
                    TempData["Error"] =
                        "Stock Transfer was not found or is already active.";
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    "Unable to restore the Stock Transfer.";

                Console.WriteLine(
                    $"STOCK TRANSFER RESTORE ERROR: {ex}");
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // GET PRODUCTS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products =
                await _productService.GetAllAsync();

            var result =
                products
                    .Where(x => !x.IsDeleted)
                    .Select(x => new
                    {
                        id = x.Id,

                        sku = x.SKU,

                        barcode = x.Barcode,

                        subCategoryId =
                            x.SubCategoryId,

                        subCategoryName =
                            x.SubCategory != null
                                ? x.SubCategory.Name
                                : "N/A"
                    })
                    .ToList();

            return Json(result);
        }

        // =========================================================
        // GET PRODUCT DETAILS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> GetProductDetails(
            int productId,
            int fromWarehouseId,
            int toWarehouseId)
        {
            if (productId <= 0)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid product."
                });
            }

            var product =
                await _productService.GetByIdAsync(
                    productId);

            if (product == null ||
                product.IsDeleted)
            {
                return Json(new
                {
                    success = false,
                    message = "Product not found."
                });
            }

            // =====================================================
            // FROM WAREHOUSE STOCK
            // =====================================================

            decimal fromAvailableStock = 0m;

            if (fromWarehouseId > 0)
            {
                fromAvailableStock =
                    await _stockService.GetAvailableStockAsync(
                        fromWarehouseId,
                        productId);
            }

            // =====================================================
            // TO WAREHOUSE STOCK
            // =====================================================

            decimal toAvailableStock = 0m;

            if (toWarehouseId > 0)
            {
                toAvailableStock =
                    await _stockService.GetAvailableStockAsync(
                        toWarehouseId,
                        productId);
            }

            // =====================================================
            // GET LATEST PURCHASE ORDER UNIT PRICE
            // =====================================================

            decimal unitCost =
                await _purchaseOrderService
                    .GetLatestUnitPriceAsync(
                        productId);

            return Json(new
            {
                success = true,

                id = product.Id,

                sku = product.SKU,

                barcode = product.Barcode,

                subCategoryId =
                    product.SubCategoryId,

                subCategoryName =
                    product.SubCategory != null
                        ? product.SubCategory.Name
                        : "N/A",

                fromAvailableQuantity =
                    fromAvailableStock,

                toAvailableQuantity =
                    toAvailableStock,

                unitCost =
                    unitCost
            });
        }

        // =========================================================
        // GET AVAILABLE STOCK
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> GetAvailableStock(
            int warehouseId,
            int productId)
        {
            if (warehouseId <= 0 ||
                productId <= 0)
            {
                return Json(new
                {
                    availableStock = 0
                });
            }

            var stock =
                await _stockService
                    .GetAvailableStockAsync(
                        warehouseId,
                        productId);

            return Json(new
            {
                availableStock = stock
            });
        }

        // =========================================================
        // PRINT
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Print(int id)
        {
            var transfer =
                await _transferService.GetByIdAsync(id);

            if (transfer == null)
                return NotFound();

            return View(transfer);
        }

        // =========================================================
        // LOAD WAREHOUSE DROPDOWN
        // =========================================================

        private async Task LoadDropdowns(
            StockTransferViewModel model)
        {
            var warehouses =
                await _warehouseService.GetAllAsync();

            model.Warehouses =
                warehouses
                    .Where(x => !x.IsDeleted)
                    .Select(x => new SelectListItem
                    {
                        Value =
                            x.Id.ToString(),

                        Text =
                            x.Name
                    })
                    .ToList();
        }
    }
}