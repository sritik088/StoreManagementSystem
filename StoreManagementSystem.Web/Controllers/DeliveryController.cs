using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Web.ViewModels.Delivery;

namespace StoreManagementSystem.Web.Controllers
{
    public class DeliveryController : Controller
    {
        private readonly IDeliveryService _deliveryService;
        private readonly ISalesOrderService _salesOrderService;
        private readonly IWarehouseService _warehouseService;
        private readonly IWarehouseStockService _stockService;

        public DeliveryController(
            IDeliveryService deliveryService,
            ISalesOrderService salesOrderService,
            IWarehouseService warehouseService,
            IWarehouseStockService stockService)
        {
            _deliveryService = deliveryService;
            _salesOrderService = salesOrderService;
            _warehouseService = warehouseService;
            _stockService = stockService;
        }

        // =====================================================
        // INDEX
        // =====================================================

        public async Task<IActionResult> Index()
        {
            var deliveries =
                await _deliveryService.GetAllAsync();

            return View(deliveries);
        }

        // =====================================================
        // DETAILS
        // =====================================================

        public async Task<IActionResult> Details(int id)
        {
            var delivery =
                await _deliveryService.GetByIdAsync(id);

            if (delivery == null)
                return NotFound();

            return View(delivery);
        }

        // =====================================================
        // CREATE GET
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Create(
            int? salesOrderId)
        {
            var model =
                new DeliveryViewModel();

            await LoadDropdowns(model);

            if (salesOrderId.HasValue)
            {
                model.SalesOrderId =
                    salesOrderId.Value;

                await LoadSalesOrderItems(model);
            }

            return View(model);
        }

        // =====================================================
        // CREATE POST
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            DeliveryViewModel model)
        {
            if (model.Items == null ||
                !model.Items.Any())
            {
                ModelState.AddModelError(
                    "",
                    "Please add at least one delivery item.");
            }

            if (!ModelState.IsValid)
            {
                await LoadDropdowns(model);

                return View(model);
            }

            var delivery =
                new Delivery
                {
                    SalesOrderId =
                        model.SalesOrderId,

                    WarehouseId =
                        model.WarehouseId,

                    DeliveryDate =
                        model.DeliveryDate,

                    Remarks =
                        model.Remarks,

                    Items =
                        model.Items
                            .Where(x =>
                                x.DeliveryQuantity > 0)
                            .Select(x =>
                                new DeliveryItem
                                {
                                    SalesOrderItemId =
                                        x.SalesOrderItemId,

                                    ProductId =
                                        x.ProductId,

                                    Quantity =
                                        x.DeliveryQuantity,

                                    UnitPrice =
                                        x.UnitPrice
                                })
                            .ToList()
                };

            if (!delivery.Items.Any())
            {
                ModelState.AddModelError(
                    "",
                    "Please enter delivery quantity.");

                await LoadDropdowns(model);

                return View(model);
            }

            try
            {
                var result =
                    await _deliveryService
                        .CreateAsync(delivery);

                if (!result)
                {
                    TempData["Error"] =
                        "Unable to create delivery.";

                    await LoadDropdowns(model);

                    return View(model);
                }

                TempData["Success"] =
                    "Delivery created successfully.";

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

        // =====================================================
        // LOAD SALES ORDER ITEMS
        // =====================================================

        [HttpGet]
        public async Task<IActionResult>
            GetSalesOrderItems(int salesOrderId)
        {
            var salesOrder =
                await _salesOrderService
                    .GetByIdAsync(salesOrderId);

            if (salesOrder == null)
                return NotFound();

            var result = new List<object>();

            foreach (var item
                in salesOrder.SalesOrderItems)
            {
                var delivered =
                    await _deliveryService
                        .GetDeliveredQuantityAsync(
                            item.Id);

                var remaining =
                    item.Quantity - delivered;

                if (remaining <= 0)
                    continue;

                var stock =
                    await _stockService
                        .GetAvailableStockAsync(
                            salesOrder.WarehouseId,
                            item.ProductId);

                result.Add(new
                {
                    salesOrderItemId = item.Id,
                    productId = item.ProductId,
                   
                    sku =
                        item.Product?.SKU,
                    orderedQuantity =
                        item.Quantity,
                    alreadyDeliveredQuantity =
                        delivered,
                    remainingQuantity =
                        remaining,
                    availableStock =
                        stock,
                    unitPrice =
                        item.UnitPrice
                });
            }

            return Json(result);
        }

        // =====================================================
        // LOAD DROPDOWNS
        // =====================================================

        private async Task LoadDropdowns(
            DeliveryViewModel model)
        {
            var salesOrders =
                await _salesOrderService
                    .GetAllAsync();

            model.SalesOrders =
                salesOrders
                    .Select(x =>
                        new SelectListItem
                        {
                            Value =
                                x.Id.ToString(),

                            Text =
                                x.OrderNumber
                        })
                    .ToList();

            var warehouses =
                await _warehouseService
                    .GetAllAsync();

            model.Warehouses =
                warehouses
                    .Where(x => !x.IsDeleted)
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

        // =====================================================
        // LOAD ITEMS
        // =====================================================

        private async Task LoadSalesOrderItems(
            DeliveryViewModel model)
        {
            var salesOrder =
                await _salesOrderService
                    .GetByIdAsync(
                        model.SalesOrderId);

            if (salesOrder == null)
                return;

            model.WarehouseId =
                salesOrder.WarehouseId;

            model.SalesOrderNumber =
                salesOrder.OrderNumber;

            foreach (var item
                in salesOrder.SalesOrderItems)
            {
                var delivered =
                    await _deliveryService
                        .GetDeliveredQuantityAsync(
                            item.Id);

                var remaining =
                    item.Quantity - delivered;

                if (remaining <= 0)
                    continue;

                var available =
                    await _stockService
                        .GetAvailableStockAsync(
                            model.WarehouseId,
                            item.ProductId);

                model.Items.Add(
                    new DeliveryItemViewModel
                    {
                        SalesOrderItemId =
                            item.Id,

                        ProductId =
                            item.ProductId,

                       

                        SKU =
                            item.Product?.SKU
                            ?? "",

                        OrderedQuantity =
                            item.Quantity,

                        AlreadyDeliveredQuantity =
                            delivered,

                        RemainingQuantity =
                            remaining,

                        AvailableStock =
                            available,

                        UnitPrice =
                            item.UnitPrice
                    });
            }
        }

        // =====================================================
        // PRINT
        // =====================================================

        public async Task<IActionResult> Print(int id)
        {
            var delivery =
                await _deliveryService
                    .GetByIdAsync(id);

            if (delivery == null)
                return NotFound();

            return View(delivery);
        }
    }
}