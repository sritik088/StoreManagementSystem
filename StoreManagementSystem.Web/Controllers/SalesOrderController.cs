using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Domain.Enums;
using StoreManagementSystem.Web.ViewModels.SalesOrder;

namespace StoreManagementSystem.Web.Controllers
{
    [Authorize]
    public class SalesOrderController : Controller
    {
        private readonly ISalesOrderService _salesOrderService;
        private readonly IProductService _productService;
        private readonly IWarehouseService _warehouseService;
        private readonly ISystemSettingService _settingService;

        public SalesOrderController(
            ISalesOrderService salesOrderService,
            IProductService productService,
            IWarehouseService warehouseService,
            ISystemSettingService settingService)
        {
            _salesOrderService = salesOrderService;
            _productService = productService;
            _warehouseService = warehouseService;
            _settingService = settingService;
        }

        // =========================================================
        // MODULE ACCESS
        // =========================================================

        private async Task<IActionResult?> CheckModuleAccessAsync()
        {
            var enabled =
                await _settingService.IsSalesOrderEnabledAsync();

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

        // =========================================================
        // SETTINGS
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
        // INDEX
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var access =
                await CheckModuleAccessAsync();

            if (access != null)
                return access;

            var orders =
                await _salesOrderService.GetAllAsync();

            await LoadSalesOrderSettingsAsync();

            return View(orders);
        }

        // =========================================================
        // CREATE - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var access =
                await CheckModuleAccessAsync();

            if (access != null)
                return access;

            var model =
                new SalesOrderViewModel
                {
                    OrderNumber =
                        await GenerateOrderNumberAsync(),

                    OrderDate =
                        DateTime.Today,

                    Status =
                        SalesOrderStatus.Draft
                };

            model.Items.Add(
                new SalesOrderItemViewModel
                {
                    Quantity = 1,
                    UnitPrice = 0,
                    SalePrice = 0,
                    TaxPercent = 0,
                    DiscountPercent = 0
                });

            await LoadDropdownsAsync(model);
            await LoadSalesOrderSettingsAsync();

            return View(model);
        }

        // =========================================================
        // CREATE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            SalesOrderViewModel model)
        {
            var access =
                await CheckModuleAccessAsync();

            if (access != null)
                return access;

            model.Items =
                (model.Items ??
                    new List<SalesOrderItemViewModel>())
                .Where(x => x.ProductId > 0)
                .ToList();

            if (!model.Items.Any())
            {
                ModelState.AddModelError(
                    "",
                    "Please add at least one product.");
            }

            if (model.WarehouseId <= 0)
            {
                ModelState.AddModelError(
                    "WarehouseId",
                    "Please select a warehouse.");
            }

            ValidateItems(model.Items);

            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync(model);
                await LoadSalesOrderSettingsAsync();

                return View(model);
            }

            var salesOrder =
                new SalesOrder
                {
                    OrderNumber =
                        model.OrderNumber,

                    OrderDate =
                        model.OrderDate,

                    WarehouseId =
                        model.WarehouseId,

                    CustomerName =
                        model.CustomerName,

                    CustomerPhone =
                        model.CustomerPhone,

                    Remarks =
                        model.Remarks,

                    // New sales always start as Draft.
                    Status =
                        SalesOrderStatus.Draft,

                    IsDeleted =
                        false,

                    SalesOrderItems =
                        model.Items
                            .Select(x =>
                                new SalesOrderItem
                                {
                                    ProductId =
                                        x.ProductId,

                                    Quantity =
                                        x.Quantity,

                                    // Actual purchase / GRN cost
                                    UnitPrice =
                                        x.UnitPrice,

                                    // Customer selling price
                                    SalePrice =
                                        x.SalePrice,

                                    TaxPercent =
                                        x.TaxPercent,

                                    DiscountPercent =
                                        x.DiscountPercent
                                })
                            .ToList()
                };

            try
            {
                var result =
                    await _salesOrderService
                        .CreateAsync(salesOrder);

                if (!result)
                {
                    ModelState.AddModelError(
                        "",
                        "Unable to create Sales Order. " +
                        "Please verify the order data.");

                    await LoadDropdownsAsync(model);
                    await LoadSalesOrderSettingsAsync();

                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    "",
                    ex.Message);

                await LoadDropdownsAsync(model);
                await LoadSalesOrderSettingsAsync();

                return View(model);
            }

            TempData["Success"] =
                "Sales Order created successfully as Draft.";

            return RedirectToAction(
                nameof(Details),
                new
                {
                    id = salesOrder.Id
                });
        }

        // =========================================================
        // EDIT - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Edit(
            int id)
        {
            var access =
                await CheckModuleAccessAsync();

            if (access != null)
                return access;

            var order =
                await _salesOrderService
                    .GetByIdAsync(id);

            if (order == null)
                return NotFound();

            if (order.Status !=
                SalesOrderStatus.Draft)
            {
                TempData["Error"] =
                    "Only Draft Sales Orders can be edited.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }

            var model =
                new SalesOrderViewModel
                {
                    Id =
                        order.Id,

                    OrderNumber =
                        order.OrderNumber,

                    OrderDate =
                        order.OrderDate,

                    WarehouseId =
                        order.WarehouseId,

                    CustomerName =
                        order.CustomerName ??
                        string.Empty,

                    CustomerPhone =
                        order.CustomerPhone ??
                        string.Empty,

                    Remarks =
                        order.Remarks ??
                        string.Empty,

                    Status =
                        order.Status,

                    SubTotal =
                        order.SubTotal,

                    DiscountAmount =
                        order.DiscountAmount,

                    TaxAmount =
                        order.TaxAmount,

                    GrandTotal =
                        order.GrandTotal,

                    GrossProfit =
                        order.GrossProfit
                };

            foreach (var item
                in order.SalesOrderItems)
            {
                model.Items.Add(
                    new SalesOrderItemViewModel
                    {
                        Id =
                            item.Id,

                        ProductId =
                            item.ProductId,

                        ProductSKU =
                            item.Product?.SKU ??
                            string.Empty,

                        Quantity =
                            item.Quantity,

                        UnitPrice =
                            item.UnitPrice,

                        SalePrice =
                            item.SalePrice,

                        TaxPercent =
                            item.TaxPercent,

                        DiscountPercent =
                            item.DiscountPercent
                    });
            }

            if (!model.Items.Any())
            {
                model.Items.Add(
                    new SalesOrderItemViewModel
                    {
                        Quantity = 1,
                        UnitPrice = 0,
                        SalePrice = 0,
                        TaxPercent = 0,
                        DiscountPercent = 0
                    });
            }

            await LoadDropdownsAsync(model);
            await LoadSalesOrderSettingsAsync();

            return View(model);
        }

        // =========================================================
        // EDIT - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            SalesOrderViewModel model)
        {
            var access =
                await CheckModuleAccessAsync();

            if (access != null)
                return access;

            if (model.Id <= 0)
                return NotFound();

            var existingOrder =
                await _salesOrderService
                    .GetByIdAsync(model.Id);

            if (existingOrder == null)
                return NotFound();

            if (existingOrder.Status !=
                SalesOrderStatus.Draft)
            {
                TempData["Error"] =
                    "Only Draft Sales Orders can be edited.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = model.Id });
            }

            model.Items =
                (model.Items ??
                    new List<SalesOrderItemViewModel>())
                .Where(x => x.ProductId > 0)
                .ToList();

            if (!model.Items.Any())
            {
                ModelState.AddModelError(
                    "",
                    "Please add at least one product.");
            }

            if (model.WarehouseId <= 0)
            {
                ModelState.AddModelError(
                    "WarehouseId",
                    "Please select a warehouse.");
            }

            ValidateItems(model.Items);

            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync(model);
                await LoadSalesOrderSettingsAsync();

                return View(model);
            }

            var updatedOrder =
                new SalesOrder
                {
                    Id =
                        existingOrder.Id,

                    OrderNumber =
                        existingOrder.OrderNumber,

                    OrderDate =
                        model.OrderDate,

                    WarehouseId =
                        model.WarehouseId,

                    CustomerName =
                        model.CustomerName,

                    CustomerPhone =
                        model.CustomerPhone,

                    Remarks =
                        model.Remarks,

                    Status =
                        SalesOrderStatus.Draft,

                    IsDeleted =
                        false,

                    SalesOrderItems =
                        model.Items
                            .Select(x =>
                                new SalesOrderItem
                                {
                                    ProductId =
                                        x.ProductId,

                                    Quantity =
                                        x.Quantity,

                                    UnitPrice =
                                        x.UnitPrice,

                                    SalePrice =
                                        x.SalePrice,

                                    TaxPercent =
                                        x.TaxPercent,

                                    DiscountPercent =
                                        x.DiscountPercent
                                })
                            .ToList()
                };

            try
            {
                var result =
                    await _salesOrderService
                        .UpdateAsync(updatedOrder);

                if (!result)
                {
                    ModelState.AddModelError(
                        "",
                        "Unable to update Sales Order.");

                    await LoadDropdownsAsync(model);
                    await LoadSalesOrderSettingsAsync();

                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    "",
                    ex.Message);

                await LoadDropdownsAsync(model);
                await LoadSalesOrderSettingsAsync();

                return View(model);
            }

            TempData["Success"] =
                "Sales Order updated successfully.";

            return RedirectToAction(
                nameof(Details),
                new
                {
                    id = updatedOrder.Id
                });
        }

        // =========================================================
        // CONFIRM SALE
        // =========================================================
        //
        // Draft
        //   ↓
        // Confirm
        //   ↓
        // Confirmed
        //
        // Only Confirmed orders are counted by Analytics.
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(
            int id)
        {
            var access =
                await CheckModuleAccessAsync();

            if (access != null)
                return access;

            if (id <= 0)
            {
                TempData["Error"] =
                    "Invalid Sales Order.";

                return RedirectToAction(
                    nameof(Index));
            }

            try
            {
                var result =
                    await _salesOrderService
                        .ConfirmAsync(id);

                if (!result)
                {
                    TempData["Error"] =
                        "Sales Order could not be confirmed. " +
                        "Only Draft Sales Orders can be confirmed.";

                    return RedirectToAction(
                        nameof(Details),
                        new { id });
                }

                TempData["Success"] =
                    "Sales Order confirmed successfully. " +
                    "It is now included in Analytics.";
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    ex.Message;
            }

            return RedirectToAction(
                nameof(Details),
                new { id });
        }

        // =========================================================
        // CANCEL SALE
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(
            int id)
        {
            var access =
                await CheckModuleAccessAsync();

            if (access != null)
                return access;

            if (id <= 0)
            {
                TempData["Error"] =
                    "Invalid Sales Order.";

                return RedirectToAction(
                    nameof(Index));
            }

            try
            {
                var result =
                    await _salesOrderService
                        .CancelAsync(id);

                if (!result)
                {
                    TempData["Error"] =
                        "Only Draft Sales Orders can be cancelled.";

                    return RedirectToAction(
                        nameof(Details),
                        new { id });
                }

                TempData["Success"] =
                    "Sales Order cancelled successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    ex.Message;
            }

            return RedirectToAction(
                nameof(Details),
                new { id });
        }

        // =========================================================
        // DETAILS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Details(
            int id)
        {
            var access =
                await CheckModuleAccessAsync();

            if (access != null)
                return access;

            var order =
                await _salesOrderService
                    .GetByIdAsync(id);

            if (order == null)
                return NotFound();

            await LoadSalesOrderSettingsAsync();

            return View(order);
        }

        // =========================================================
        // GET PRODUCT COST PRICE
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> GetProductPrice(
            int productId)
        {
            var access =
                await CheckModuleAccessAsync();

            if (access != null)
            {
                return Json(new
                {
                    success = false,
                    unitPrice = 0
                });
            }

            if (productId <= 0)
            {
                return Json(new
                {
                    success = false,
                    unitPrice = 0
                });
            }

            var cost =
                await _salesOrderService
                    .GetLatestCostPriceAsync(productId);

            if (!cost.HasValue)
            {
                return Json(new
                {
                    success = false,
                    unitPrice = 0
                });
            }

            return Json(new
            {
                success = true,
                unitPrice = cost.Value
            });
        }

        // =========================================================
        // DELETE
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int id)
        {
            var access =
                await CheckModuleAccessAsync();

            if (access != null)
                return access;

            var result =
                await _salesOrderService
                    .DeleteAsync(id);

            if (result)
            {
                TempData["Success"] =
                    "Sales Order deleted successfully.";
            }
            else
            {
                TempData["Error"] =
                    "Only Draft Sales Orders can be deleted.";
            }

            return RedirectToAction(
                nameof(Index));
        }

        // =========================================================
        // VALIDATE ITEMS
        // =========================================================

        private void ValidateItems(
            List<SalesOrderItemViewModel> items)
        {
            for (int i = 0;
                i < items.Count;
                i++)
            {
                var item =
                    items[i];

                if (item.Quantity <= 0)
                {
                    ModelState.AddModelError(
                        $"Items[{i}].Quantity",
                        "Quantity must be greater than 0.");
                }

                if (item.UnitPrice < 0)
                {
                    ModelState.AddModelError(
                        $"Items[{i}].UnitPrice",
                        "Cost price cannot be negative.");
                }

                if (item.SalePrice < 0)
                {
                    ModelState.AddModelError(
                        $"Items[{i}].SalePrice",
                        "Selling price cannot be negative.");
                }

                if (item.TaxPercent < 0 ||
                    item.TaxPercent > 100)
                {
                    ModelState.AddModelError(
                        $"Items[{i}].TaxPercent",
                        "Tax must be between 0 and 100.");
                }

                if (item.DiscountPercent < 0 ||
                    item.DiscountPercent > 100)
                {
                    ModelState.AddModelError(
                        $"Items[{i}].DiscountPercent",
                        "Discount must be between 0 and 100.");
                }
            }
        }

        // =========================================================
        // LOAD DROPDOWNS
        // =========================================================

        private async Task LoadDropdownsAsync(
            SalesOrderViewModel model)
        {
            var products =
                await _productService
                    .GetAllAsync();

            var productList =
                products
                    .Where(x => !x.IsDeleted)
                    .OrderBy(x => x.SKU)
                    .Select(x =>
                        new SelectListItem
                        {
                            Value =
                                x.Id.ToString(),

                            Text =
                                x.SKU
                        })
                    .ToList();

            foreach (var item in model.Items)
            {
                item.Products =
                    productList
                        .Select(x =>
                            new SelectListItem
                            {
                                Value =
                                    x.Value,

                                Text =
                                    x.Text,

                                Selected =
                                    x.Value ==
                                    item.ProductId.ToString()
                            })
                        .ToList();
            }

            var warehouses =
                await _warehouseService
                    .GetAllAsync();

            model.Warehouses =
                warehouses
                    .Where(x =>
                        x.IsActive &&
                        !x.IsDeleted)
                    .OrderBy(x => x.Name)
                    .Select(x =>
                        new SelectListItem
                        {
                            Value =
                                x.Id.ToString(),

                            Text =
                                x.Name,

                            Selected =
                                x.Id ==
                                model.WarehouseId
                        })
                    .ToList();
        }

        // =========================================================
        // GENERATE ORDER NUMBER
        // =========================================================

        private async Task<string> GenerateOrderNumberAsync()
        {
            var orders =
                await _salesOrderService
                    .GetAllAsync();

            var next =
                orders.Any()
                    ? orders.Max(x => x.Id) + 1
                    : 1;

            return $"SO-{next:00000}";
        }
    }
}