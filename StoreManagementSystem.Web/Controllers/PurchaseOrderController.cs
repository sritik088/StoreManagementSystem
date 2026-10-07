using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Domain.Enums;
using StoreManagementSystem.Infrastructure.Data;
using StoreManagementSystem.Web.ViewModels.PurchaseOrder;

namespace StoreManagementSystem.Web.Controllers
{
    public class PurchaseOrderController : Controller
    {
        // ============================================================
        // DEPENDENCIES
        // ============================================================

        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly ISupplierService _supplierService;
        private readonly IProductService _productService;
        private readonly ApplicationDbContext _context;


        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public PurchaseOrderController(
            IPurchaseOrderService purchaseOrderService,
            ISupplierService supplierService,
            IProductService productService,
            ApplicationDbContext context)
        {
            _purchaseOrderService = purchaseOrderService;
            _supplierService = supplierService;
            _productService = productService;
            _context = context;
        }


        // ============================================================
        // INDEX
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var purchaseOrders =
                await _purchaseOrderService.GetAllAsync();

            return View(purchaseOrders);
        }


        // ============================================================
        // DETAILS
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var purchaseOrder =
                await _context.PurchaseOrders
                    .Include(x => x.Supplier)
                    .Include(x => x.Items)
                        .ThenInclude(x => x.Product)
                    .FirstOrDefaultAsync(x =>
                        x.Id == id &&
                        !x.IsDeleted);

            if (purchaseOrder == null)
            {
                return NotFound();
            }

            return View(purchaseOrder);
        }


        // ============================================================
        // CREATE - GET
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new PurchaseOrderViewModel
            {
                OrderDate = DateTime.Today,

                ExpectedDate =
                    DateTime.Today.AddDays(7),

                Status =
                    PurchaseOrderStatus.Draft.ToString()
            };


            await LoadDropdowns(model);


            model.Items.Add(
                new PurchaseOrderItemViewModel
                {
                    Quantity = 1,
                    UnitPrice = 0,
                    Discount = 0,
                    TaxAmount = 0,
                    Total = 0
                });


            return View(model);
        }


        // ============================================================
        // LOAD DROPDOWNS
        // ============================================================

        private async Task LoadDropdowns(
            PurchaseOrderViewModel model)
        {
            // --------------------------------------------------------
            // SUPPLIERS
            // --------------------------------------------------------

            var suppliers =
                await _supplierService.GetAllAsync();


            model.Suppliers =
                suppliers
                    .Where(x => !x.IsDeleted)
                    .OrderBy(x => x.Name)
                    .Select(x => new SelectListItem
                    {
                        Value =
                            x.Id.ToString(),

                        Text =
                            x.Name
                    })
                    .ToList();


            // --------------------------------------------------------
            // PRODUCTS
            // --------------------------------------------------------

            var products =
                await _productService.GetAllAsync();


            model.Products =
                products
                    .Where(x => !x.IsDeleted)
                    .OrderBy(x => x.SKU)
                    .Select(x => new SelectListItem
                    {
                        Value =
                            x.Id.ToString(),

                        Text =
                            string.IsNullOrWhiteSpace(x.Barcode)
                                ? x.SKU
                                : $"{x.SKU} - {x.Barcode}"
                    })
                    .ToList();
        }


        // ============================================================
        // CREATE - POST
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            PurchaseOrderViewModel model)
        {
            // --------------------------------------------------------
            // REMOVE EMPTY PRODUCT ROWS
            // --------------------------------------------------------

            model.Items =
                (model.Items ??
                    new List<PurchaseOrderItemViewModel>())
                .Where(x => x.ProductId > 0)
                .ToList();


            // --------------------------------------------------------
            // CHECK ITEMS
            // --------------------------------------------------------

            if (!model.Items.Any())
            {
                ModelState.AddModelError(
                    "Items",
                    "Please add at least one product.");
            }


            // --------------------------------------------------------
            // VALIDATION
            // --------------------------------------------------------

            if (!ModelState.IsValid)
            {
                await LoadDropdowns(model);

                if (!model.Items.Any())
                {
                    model.Items.Add(
                        new PurchaseOrderItemViewModel
                        {
                            Quantity = 1
                        });
                }

                return View(model);
            }


            // --------------------------------------------------------
            // SERVER-SIDE TOTAL CALCULATION
            //
            // UI:
            // Discount = %
            // TaxAmount = %
            //
            // Database:
            // Discount = ₹ amount
            // TaxAmount = ₹ amount
            // --------------------------------------------------------

            decimal subTotal = 0;
            decimal discount = 0;
            decimal taxAmount = 0;
            decimal grandTotal = 0;


            foreach (var item in model.Items)
            {
                // ----------------------------------------------------
                // LINE SUBTOTAL
                // ----------------------------------------------------

                var lineSubTotal =
                    item.Quantity *
                    item.UnitPrice;


                // ----------------------------------------------------
                // DISCOUNT %
                // ----------------------------------------------------

                var discountPercent =
                    Math.Clamp(
                        item.Discount,
                        0,
                        100);


                // ----------------------------------------------------
                // DISCOUNT ₹
                // ----------------------------------------------------

                var discountAmount =
                    lineSubTotal *
                    discountPercent /
                    100m;


                // ----------------------------------------------------
                // TAXABLE AMOUNT
                // ----------------------------------------------------

                var taxableAmount =
                    Math.Max(
                        lineSubTotal -
                        discountAmount,
                        0);


                // ----------------------------------------------------
                // TAX %
                // ----------------------------------------------------

                var taxPercent =
                    Math.Clamp(
                        item.TaxAmount,
                        0,
                        100);


                // ----------------------------------------------------
                // TAX ₹
                // ----------------------------------------------------

                var calculatedTaxAmount =
                    taxableAmount *
                    taxPercent /
                    100m;


                // ----------------------------------------------------
                // LINE TOTAL
                // ----------------------------------------------------

                var lineTotal =
                    taxableAmount +
                    calculatedTaxAmount;


                // ----------------------------------------------------
                // STORE CALCULATED ₹ VALUES
                // ----------------------------------------------------

                item.Discount =
                    discountAmount;

                item.TaxAmount =
                    calculatedTaxAmount;

                item.Total =
                    lineTotal;


                // ----------------------------------------------------
                // ORDER TOTALS
                // ----------------------------------------------------

                subTotal +=
                    lineSubTotal;

                discount +=
                    discountAmount;

                taxAmount +=
                    calculatedTaxAmount;

                grandTotal +=
                    lineTotal;
            }


            // --------------------------------------------------------
            // SET ORDER TOTALS
            // --------------------------------------------------------

            model.SubTotal =
                subTotal;

            model.Discount =
                discount;

            model.TaxAmount =
                taxAmount;

            model.GrandTotal =
                grandTotal;


            // --------------------------------------------------------
            // CREATE ENTITY
            // --------------------------------------------------------

            var purchaseOrder =
                new PurchaseOrder
                {
                    SupplierId =
                        model.SupplierId,

                    OrderDate =
                        model.OrderDate,

                    ExpectedDate =
                        model.ExpectedDate,

                    Remarks =
                        model.Remarks,

                    Status =
                        PurchaseOrderStatus.Draft,

                    SubTotal =
                        model.SubTotal,

                    Discount =
                        model.Discount,

                    TaxAmount =
                        model.TaxAmount,

                    GrandTotal =
                        model.GrandTotal,

                    Items =
                        model.Items
                            .Select(x =>
                                new PurchaseOrderItem
                                {
                                    ProductId =
                                        x.ProductId,

                                    Quantity =
                                        x.Quantity,

                                    UnitPrice =
                                        x.UnitPrice,

                                    Discount =
                                        x.Discount,

                                    TaxAmount =
                                        x.TaxAmount,

                                    Total =
                                        x.Total
                                })
                            .ToList()
                };


            // --------------------------------------------------------
            // SAVE
            // --------------------------------------------------------

            var result =
                await _purchaseOrderService
                    .CreateAsync(purchaseOrder);


            if (!result)
            {
                ModelState.AddModelError(
                    "",
                    "Unable to create Purchase Order.");

                await LoadDropdowns(model);

                return View(model);
            }


            TempData["Success"] =
                "Purchase Order created successfully.";


            return RedirectToAction(
                nameof(Index));
        }


        // ============================================================
        // APPROVE
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var result =
                await _purchaseOrderService
                    .ApproveAsync(id);


            if (result)
            {
                TempData["Success"] =
                    "Purchase Order approved successfully.";
            }
            else
            {
                TempData["Error"] =
                    "Purchase Order cannot be approved.";
            }


            return RedirectToAction(
                nameof(Index));
        }


        // ============================================================
        // CANCEL
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var result =
                await _purchaseOrderService
                    .CancelAsync(id);


            if (result)
            {
                TempData["Success"] =
                    "Purchase Order cancelled successfully.";
            }
            else
            {
                TempData["Error"] =
                    "Purchase Order cannot be cancelled.";
            }


            return RedirectToAction(
                nameof(Index));
        }


        // ============================================================
        // DELETE
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            // --------------------------------------------------------
            // CHECK PURCHASE ORDER
            // --------------------------------------------------------

            var purchaseOrder =
                await _purchaseOrderService
                    .GetByIdAsync(id);

            if (purchaseOrder == null)
            {
                TempData["Error"] =
                    "Purchase Order was not found.";

                return RedirectToAction(
                    nameof(Index));
            }


            // --------------------------------------------------------
            // CHECK ACTIVE GRN FIRST
            // --------------------------------------------------------

            var hasActiveGRN =
                await _purchaseOrderService
                    .HasActiveGoodsReceiptsAsync(id);

            if (hasActiveGRN)
            {
                TempData["Error"] =
                    $"Purchase Order {purchaseOrder.PONumber} " +
                    "cannot be deleted because an active Goods Receipt " +
                    "is linked to it. Reverse/Delete the GRN first.";

                return RedirectToAction(
                    nameof(Index));
            }


            // --------------------------------------------------------
            // DELETE
            // --------------------------------------------------------

            var result =
                await _purchaseOrderService
                    .DeleteAsync(id);


            if (result)
            {
                TempData["Success"] =
                    $"Purchase Order {purchaseOrder.PONumber} " +
                    "deleted successfully.";
            }
            else
            {
                TempData["Error"] =
                    $"Purchase Order {purchaseOrder.PONumber} " +
                    "cannot be deleted. Only Draft Purchase Orders " +
                    "without an active GRN can be deleted.";
            }


            return RedirectToAction(
                nameof(Index));
        }

        // ============================================================
        // RESTORE
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var purchaseOrder =
                await _purchaseOrderService
                    .GetByIdAsync(id);

            if (purchaseOrder == null)
            {
                TempData["Error"] =
                    "Purchase Order was not found.";

                return RedirectToAction(
                    nameof(Index));
            }

            if (!purchaseOrder.IsDeleted)
            {
                TempData["Error"] =
                    $"Purchase Order {purchaseOrder.PONumber} is already active.";

                return RedirectToAction(
                    nameof(Index));
            }

            var result =
                await _purchaseOrderService
                    .RestoreAsync(id);

            if (result)
            {
                TempData["Success"] =
                    $"Purchase Order {purchaseOrder.PONumber} restored successfully.";
            }
            else
            {
                TempData["Error"] =
                    $"Purchase Order {purchaseOrder.PONumber} could not be restored.";
            }

            return RedirectToAction(
                nameof(Index));
        }


        // ============================================================
        // GET PRODUCT INFORMATION
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> GetProductInfo(
            int productId)
        {
            if (productId <= 0)
            {
                return BadRequest();
            }


            var product =
                await _productService
                    .GetByIdAsync(productId);


            if (product == null ||
                product.IsDeleted)
            {
                return NotFound();
            }


            return Json(new
            {
                id = product.Id,

                sku = product.SKU,

                barcode = product.Barcode,

                purchasePrice = 0
            });
        }


        // ============================================================
        // EDIT - GET
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var purchaseOrder =
                await _context.PurchaseOrders
                    .Include(x => x.Supplier)
                    .Include(x => x.Items)
                        .ThenInclude(x => x.Product)
                    .FirstOrDefaultAsync(x =>
                        x.Id == id &&
                        !x.IsDeleted);


            if (purchaseOrder == null)
            {
                return NotFound();
            }


            // --------------------------------------------------------
            // ONLY DRAFT CAN BE EDITED
            // --------------------------------------------------------

            if (purchaseOrder.Status !=
                PurchaseOrderStatus.Draft)
            {
                TempData["Error"] =
                    "Only Draft Purchase Orders can be edited.";

                return RedirectToAction(
                    nameof(Index));
            }


            // --------------------------------------------------------
            // CREATE VIEW MODEL
            // --------------------------------------------------------

            var model =
                new PurchaseOrderViewModel
                {
                    Id =
                        purchaseOrder.Id,

                    PONumber =
                        purchaseOrder.PONumber,

                    SupplierId =
                        purchaseOrder.SupplierId,

                    OrderDate =
                        purchaseOrder.OrderDate,

                    ExpectedDate =
                        purchaseOrder.ExpectedDate,

                    SubTotal =
                        purchaseOrder.SubTotal,

                    Discount =
                        purchaseOrder.Discount,

                    TaxAmount =
                        purchaseOrder.TaxAmount,

                    GrandTotal =
                        purchaseOrder.GrandTotal,

                    Remarks =
                        purchaseOrder.Remarks,

                    Status =
                        purchaseOrder.Status.ToString()
                };


            // --------------------------------------------------------
            // LOAD DROPDOWNS
            // --------------------------------------------------------

            await LoadPurchaseOrderDropdowns(model);


            // --------------------------------------------------------
            // LOAD ITEMS
            //
            // Database stores ₹ Discount and ₹ Tax.
            // UI needs Discount % and Tax %.
            // --------------------------------------------------------

            model.Items =
                purchaseOrder.Items
                    .Select(x =>
                    {
                        var lineSubTotal =
                            x.Quantity *
                            x.UnitPrice;


                        var discountPercent =
                            lineSubTotal > 0
                                ? (x.Discount /
                                   lineSubTotal) *
                                  100m
                                : 0;


                        var taxableAmount =
                            Math.Max(
                                lineSubTotal -
                                x.Discount,
                                0);


                        var taxPercent =
                            taxableAmount > 0
                                ? (x.TaxAmount /
                                   taxableAmount) *
                                  100m
                                : 0;


                        return new PurchaseOrderItemViewModel
                        {
                            Id =
                                x.Id,

                            ProductId =
                                x.ProductId,

                            ProductName =
                                x.Product?.SKU
                                ?? string.Empty,

                            Quantity =
                                x.Quantity,

                            UnitPrice =
                                x.UnitPrice,

                            // UI percentage
                            Discount =
                                Math.Round(
                                    discountPercent,
                                    2),

                            // UI percentage
                            TaxAmount =
                                Math.Round(
                                    taxPercent,
                                    2),

                            Total =
                                x.Total
                        };
                    })
                    .ToList();


            // --------------------------------------------------------
            // AT LEAST ONE ROW
            // --------------------------------------------------------

            if (!model.Items.Any())
            {
                model.Items.Add(
                    new PurchaseOrderItemViewModel
                    {
                        Quantity = 1
                    });
            }


            return View(model);
        }


        // ============================================================
        // EDIT - POST
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            PurchaseOrderViewModel model)
        {
            // --------------------------------------------------------
            // LOAD EXISTING PURCHASE ORDER
            // --------------------------------------------------------

            var purchaseOrder =
                await _context.PurchaseOrders
                    .Include(x => x.Items)
                    .FirstOrDefaultAsync(x =>
                        x.Id == id &&
                        !x.IsDeleted);


            if (purchaseOrder == null)
            {
                return NotFound();
            }


            // --------------------------------------------------------
            // ONLY DRAFT CAN BE EDITED
            // --------------------------------------------------------

            if (purchaseOrder.Status !=
                PurchaseOrderStatus.Draft)
            {
                TempData["Error"] =
                    "Only Draft Purchase Orders can be edited.";

                return RedirectToAction(
                    nameof(Index));
            }


            // --------------------------------------------------------
            // REMOVE EMPTY ROWS
            // --------------------------------------------------------

            model.Items =
                (model.Items ??
                    new List<PurchaseOrderItemViewModel>())
                .Where(x => x.ProductId > 0)
                .ToList();


            // --------------------------------------------------------
            // CHECK ITEMS
            // --------------------------------------------------------

            if (!model.Items.Any())
            {
                ModelState.AddModelError(
                    "Items",
                    "At least one product is required.");
            }


            // --------------------------------------------------------
            // VALIDATION
            // --------------------------------------------------------

            if (!ModelState.IsValid)
            {
                await LoadPurchaseOrderDropdowns(model);

                if (!model.Items.Any())
                {
                    model.Items.Add(
                        new PurchaseOrderItemViewModel
                        {
                            Quantity = 1
                        });
                }

                return View(model);
            }


            // --------------------------------------------------------
            // CALCULATE TOTALS
            // --------------------------------------------------------

            decimal subTotal = 0;
            decimal discount = 0;
            decimal taxAmount = 0;
            decimal grandTotal = 0;


            foreach (var item in model.Items)
            {
                // ----------------------------------------------------
                // LINE SUBTOTAL
                // ----------------------------------------------------

                var lineSubTotal =
                    item.Quantity *
                    item.UnitPrice;


                // ----------------------------------------------------
                // DISCOUNT %
                // ----------------------------------------------------

                var discountPercent =
                    Math.Clamp(
                        item.Discount,
                        0,
                        100);


                // ----------------------------------------------------
                // DISCOUNT ₹
                // ----------------------------------------------------

                var discountAmount =
                    lineSubTotal *
                    discountPercent /
                    100m;


                // ----------------------------------------------------
                // TAXABLE AMOUNT
                // ----------------------------------------------------

                var taxableAmount =
                    Math.Max(
                        lineSubTotal -
                        discountAmount,
                        0);


                // ----------------------------------------------------
                // TAX %
                // ----------------------------------------------------

                var taxPercent =
                    Math.Clamp(
                        item.TaxAmount,
                        0,
                        100);


                // ----------------------------------------------------
                // TAX ₹
                // ----------------------------------------------------

                var calculatedTaxAmount =
                    taxableAmount *
                    taxPercent /
                    100m;


                // ----------------------------------------------------
                // LINE TOTAL
                // ----------------------------------------------------

                var lineTotal =
                    taxableAmount +
                    calculatedTaxAmount;


                // ----------------------------------------------------
                // STORE ₹ VALUES
                // ----------------------------------------------------

                item.Discount =
                    discountAmount;

                item.TaxAmount =
                    calculatedTaxAmount;

                item.Total =
                    lineTotal;


                // ----------------------------------------------------
                // TOTALS
                // ----------------------------------------------------

                subTotal +=
                    lineSubTotal;

                discount +=
                    discountAmount;

                taxAmount +=
                    calculatedTaxAmount;

                grandTotal +=
                    lineTotal;
            }


            // --------------------------------------------------------
            // UPDATE HEADER
            // --------------------------------------------------------

            purchaseOrder.SupplierId =
                model.SupplierId;

            purchaseOrder.OrderDate =
                model.OrderDate;

            purchaseOrder.ExpectedDate =
                model.ExpectedDate;

            purchaseOrder.SubTotal =
                subTotal;

            purchaseOrder.Discount =
                discount;

            purchaseOrder.TaxAmount =
                taxAmount;

            purchaseOrder.GrandTotal =
                grandTotal;

            purchaseOrder.Remarks =
                model.Remarks;

            purchaseOrder.UpdatedDate =
                DateTime.Now;


            // --------------------------------------------------------
            // REMOVE OLD ITEMS
            // --------------------------------------------------------

            _context.PurchaseOrderItems
                .RemoveRange(
                    purchaseOrder.Items);


            // --------------------------------------------------------
            // ADD NEW ITEMS
            // --------------------------------------------------------

            foreach (var item in model.Items)
            {
                purchaseOrder.Items.Add(
                    new PurchaseOrderItem
                    {
                        PurchaseOrderId =
                            purchaseOrder.Id,

                        ProductId =
                            item.ProductId,

                        Quantity =
                            item.Quantity,

                        UnitPrice =
                            item.UnitPrice,

                        Discount =
                            item.Discount,

                        TaxAmount =
                            item.TaxAmount,

                        Total =
                            item.Total
                    });
            }


            // --------------------------------------------------------
            // SAVE
            // --------------------------------------------------------

            await _context.SaveChangesAsync();


            TempData["Success"] =
                $"Purchase Order {purchaseOrder.PONumber} updated successfully.";


            return RedirectToAction(
                nameof(Details),
                new
                {
                    id = purchaseOrder.Id
                });
        }


        // ============================================================
        // LOAD EDIT DROPDOWNS
        // ============================================================

        private async Task LoadPurchaseOrderDropdowns(
            PurchaseOrderViewModel model)
        {
            // --------------------------------------------------------
            // SUPPLIERS
            // --------------------------------------------------------

            var suppliers =
                await _supplierService
                    .GetAllAsync();


            model.Suppliers =
                suppliers
                    .Where(x => !x.IsDeleted)
                    .OrderBy(x => x.Name)
                    .Select(x => new SelectListItem
                    {
                        Value =
                            x.Id.ToString(),

                        Text =
                            x.Name
                    })
                    .ToList();


            // --------------------------------------------------------
            // PRODUCTS
            // --------------------------------------------------------

            var products =
                await _productService
                    .GetAllAsync();


            model.Products =
                products
                    .Where(x => !x.IsDeleted)
                    .OrderBy(x => x.SKU)
                    .Select(x => new SelectListItem
                    {
                        Value =
                            x.Id.ToString(),

                        Text =
                            string.IsNullOrWhiteSpace(x.Barcode)
                                ? x.SKU
                                : $"{x.SKU} - {x.Barcode}"
                    })
                    .ToList();
        }
    }
}