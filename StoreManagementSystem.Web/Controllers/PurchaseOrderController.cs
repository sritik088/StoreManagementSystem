using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Domain.Enums;
using StoreManagementSystem.Web.ViewModels.PurchaseOrder;

namespace StoreManagementSystem.Web.Controllers
{
    public class PurchaseOrderController : Controller
    {
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly ISupplierService _supplierService;
        private readonly IProductService _productService;

        public PurchaseOrderController(
            IPurchaseOrderService purchaseOrderService,
            ISupplierService supplierService,
            IProductService productService)
        {
            _purchaseOrderService = purchaseOrderService;
            _supplierService = supplierService;
            _productService = productService;
        }

        public async Task<IActionResult> Index()
        {
            var purchaseOrders = await _purchaseOrderService.GetAllAsync();

            return View(purchaseOrders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var purchaseOrder = await _purchaseOrderService.GetByIdAsync(id);

            if (purchaseOrder == null)
                return NotFound();

            return View(purchaseOrder);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new PurchaseOrderViewModel();

            await LoadDropdowns(model);

            model.Items.Add(new PurchaseOrderItemViewModel());

            return View(model);
        }

        private async Task LoadDropdowns(PurchaseOrderViewModel model)
        {
            var suppliers = await _supplierService.GetAllAsync();

            model.Suppliers = suppliers.Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Name
            });

            var products = await _productService.GetAllAsync();

            foreach (var item in model.Items)
            {
                item.Products = products.Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PurchaseOrderViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdowns(model);
                return View(model);
            }

            var purchaseOrder = new PurchaseOrder
            {
                SupplierId = model.SupplierId,
                OrderDate = model.OrderDate,
                ExpectedDate = model.ExpectedDate,
                Remarks = model.Remarks,

                Items = model.Items.Select(x => new PurchaseOrderItem
                {
                    ProductId = x.ProductId,
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice,
                    Discount = x.Discount,
                    TaxAmount = x.TaxAmount
                }).ToList()
            };

            var result = await _purchaseOrderService.CreateAsync(purchaseOrder);

            if (!result)
            {
                ModelState.AddModelError("", "Unable to create Purchase Order.");

                await LoadDropdowns(model);

                return View(model);
            }

            TempData["Success"] = "Purchase Order created successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var result = await _purchaseOrderService.ApproveAsync(id);

            if (result)
                TempData["Success"] = "Purchase Order Approved.";
            else
                TempData["Error"] = "Purchase Order cannot be approved.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var result = await _purchaseOrderService.CancelAsync(id);

            if (result)
                TempData["Success"] = "Purchase Order Cancelled.";
            else
                TempData["Error"] = "Purchase Order cannot be cancelled.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _purchaseOrderService.DeleteAsync(id);

            if (result)
                TempData["Success"] = "Purchase Order deleted successfully.";
            else
                TempData["Error"] = "Only Draft Purchase Orders can be deleted.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetProductInfo(int productId)
        {
            var product = await _productService.GetByIdAsync(productId);

            if (product == null)
                return NotFound();

            return Json(new
            {
                id = product.Id,
                name = product.Name,
                sku = product.SKU,
                barcode = product.Barcode,
                purchasePrice = product.PurchasePrice,
                sellingPrice = product.SellingPrice,
                taxId = product.TaxId,
                taxName = product.Tax?.Name,
                unitId = product.UnitId,
                unitName = product.Unit?.Name,
                hsnCode = product.HSNCode,
                currentStock = product.CurrentStock,
                reorderLevel = product.ReorderLevel,
                image = product.ImageUrl
            });
        }
    }
}
