using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Web.ViewModels.GoodsReceipt;

namespace StoreManagementSystem.Web.Controllers
{
    public class GoodsReceiptController : Controller
    {
        private readonly IGoodsReceiptService _goodsReceiptService;
        private readonly IPurchaseOrderRepository _purchaseOrderRepository;
        private readonly IWarehouseService _warehouseService;

        public GoodsReceiptController(
            IGoodsReceiptService goodsReceiptService,
            IPurchaseOrderRepository purchaseOrderRepository,
            IWarehouseService warehouseService)
        {
            _goodsReceiptService = goodsReceiptService;
            _purchaseOrderRepository = purchaseOrderRepository;
            _warehouseService = warehouseService;
        }

        public async Task<IActionResult> Index()
        {
            var receipts = await _goodsReceiptService.GetAllAsync();

            return View(receipts);
        }

        public async Task<IActionResult> Details(int id)
        {
            var receipt = await _goodsReceiptService.GetByIdAsync(id);

            if (receipt == null)
                return NotFound();

            return View(receipt);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new GoodsReceiptViewModel();

            var purchaseOrders =
     await _purchaseOrderRepository.GetAllAsync();

            model.PurchaseOrders =
                purchaseOrders.Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = $"{x.PONumber} - {x.Supplier?.Name}"
                });

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetPurchaseOrder(int id)
        {
            var po = await _purchaseOrderRepository.GetByIdAsync(id);

            if (po == null)
                return NotFound();

            return Json(new
            {
                po.Id,
                po.PONumber,
                Supplier = po.Supplier?.Name,

                Items = po.Items.Select(x => new
                {
                    PurchaseOrderItemId = x.Id,

                    ProductId = x.ProductId,

                    Product = x.Product!.Name,

                    SKU = x.Product.SKU,

                    Unit = x.Product.Unit!.Name,

                    OrderedQty = x.Quantity,

                    Price = x.UnitPrice,

                    Tax = x.TaxAmount,

                    Discount = x.Discount
                })
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
    GoodsReceiptViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var receipt = new GoodsReceipt
            {
                PurchaseOrderId = model.PurchaseOrderId,

                SupplierId = model.SupplierId,

                ReceiptDate = model.ReceiptDate,

                Remarks = model.Remarks,

                Items = model.Items.Select(x =>
                    new GoodsReceiptItem
                    {
                        PurchaseOrderItemId = x.PurchaseOrderItemId,

                        ProductId = x.ProductId,

                        WarehouseId = x.WarehouseId,

                        OrderedQuantity = x.OrderedQuantity,

                        ReceivedQuantity = x.ReceivedQuantity,

                        UnitPrice = x.UnitPrice,

                        Discount = x.Discount,

                        TaxAmount = x.TaxAmount
                    }).ToList()
            };

            bool result =
                await _goodsReceiptService.CreateAsync(receipt);

            if (!result)
            {
                TempData["Error"] =
                    "Unable to receive goods.";

                return View(model);
            }

            TempData["Success"] =
                "Goods received successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            bool result =
                await _goodsReceiptService.DeleteAsync(id);

            if (result)
                TempData["Success"] =
                    "GRN deleted successfully.";
            else
                TempData["Error"] =
                    "Unable to delete GRN.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetWarehouses()
        {
            var warehouses =
                await _warehouseService.GetAllAsync();

            return Json(warehouses.Select(x => new
            {
                x.Id,

                x.Name
            }));
        }

        public async Task<IActionResult> Print(int id)
        {
            var receipt =
                await _goodsReceiptService.GetByIdAsync(id);

            if (receipt == null)
                return NotFound();

            return View(receipt);
        }

    }

}
