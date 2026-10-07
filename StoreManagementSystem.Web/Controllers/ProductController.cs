using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Infrastructure.Identity;
using StoreManagementSystem.Web.Authorization;
using StoreManagementSystem.Web.ViewModels.Product;

namespace StoreManagementSystem.Web.Controllers
{
    [Authorize]
    [PermissionAuthorize(PermissionConstants.ProductView)]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly IMainCategoryService _mainCategoryService;
        private readonly ICategoryService _categoryService;
        private readonly ISubCategoryService _subCategoryService;
        private readonly ISupplierService _supplierService;
        private readonly IWarehouseService _warehouseService;

        public ProductController(
            IProductService productService,
            IMainCategoryService mainCategoryService,
            ICategoryService categoryService,
            ISubCategoryService subCategoryService,
            ISupplierService supplierService,
            IWarehouseService warehouseService)
        {
            _productService = productService;
            _mainCategoryService = mainCategoryService;
            _categoryService = categoryService;
            _subCategoryService = subCategoryService;
            _supplierService = supplierService;
            _warehouseService = warehouseService;
        }

        // =========================
        // INDEX
        // =========================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllAsync();

            return View(products);
        }

        // =========================
        // CREATE - GET
        // =========================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new ProductViewModel();

            await LoadDropdownsAsync(model);

            return View(model);
        }

        // =========================
        // CREATE - POST
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            ProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync(model);

                return View(model);
            }

            var product = new Product
            {
                MainCategoryId = model.MainCategoryId,
                CategoryId = model.CategoryId,
                SubCategoryId = model.SubCategoryId,

                SKU = model.SKU.Trim(),

                Barcode = string.IsNullOrWhiteSpace(model.Barcode)
                    ? null
                    : model.Barcode.Trim(),

                SupplierId = model.SupplierId,
                WarehouseId = model.WarehouseId
            };

            var result =
                await _productService.CreateAsync(product);

            if (!result)
            {
                ModelState.AddModelError(
                    "",
                    "SKU or Barcode already exists.");

                await LoadDropdownsAsync(model);

                return View(model);
            }

            TempData["Success"] =
                "Product created successfully.";

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // EDIT - GET
        // =========================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product =
                await _productService.GetByIdAsync(id);

            if (product == null)
                return NotFound();

            var model = new ProductViewModel
            {
                Id = product.Id,

                MainCategoryId =
                    product.MainCategoryId,

                CategoryId =
                    product.CategoryId,

                SubCategoryId =
                    product.SubCategoryId,

                SKU =
                    product.SKU,

                Barcode =
                    product.Barcode,

                SupplierId =
                    product.SupplierId,

                WarehouseId =
                    product.WarehouseId
            };

            await LoadDropdownsAsync(model);

            return View(model);
        }

        // =========================
        // EDIT - POST
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            ProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync(model);

                return View(model);
            }

            var product = new Product
            {
                Id = model.Id,

                MainCategoryId =
                    model.MainCategoryId,

                CategoryId =
                    model.CategoryId,

                SubCategoryId =
                    model.SubCategoryId,

                SKU =
                    model.SKU.Trim(),

                Barcode =
                    string.IsNullOrWhiteSpace(model.Barcode)
                        ? null
                        : model.Barcode.Trim(),

                SupplierId =
                    model.SupplierId,

                WarehouseId =
                    model.WarehouseId
            };

            var result =
                await _productService.UpdateAsync(product);

            if (!result)
            {
                ModelState.AddModelError(
                    "",
                    "SKU or Barcode already exists, or Product was not found.");

                await LoadDropdownsAsync(model);

                return View(model);
            }

            TempData["Success"] =
                "Product updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // DELETE
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result =
                await _productService.DeleteAsync(id);

            if (result)
            {
                TempData["Success"] =
                    "Product deleted successfully.";
            }
            else
            {
                TempData["Error"] =
                    "Product not found.";
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // AJAX - GET CATEGORIES
        // =========================

        [HttpGet]
        public async Task<IActionResult> GetCategories(int mainCategoryId)
        {
            if (mainCategoryId <= 0)
                return Json(new List<object>());

            var categories =
                await _categoryService.GetAllAsync();

            var result = categories
                .Where(x => x.MainCategoryId == mainCategoryId)
                .Select(x => new
                {
                    id = x.Id,
                    name = x.Name
                })
                .ToList();

            return Json(result);
        }


        // =========================
        // AJAX - GET SUB CATEGORIES
        // =========================

        [HttpGet]
        public async Task<IActionResult> GetSubCategories(int categoryId)
        {
            if (categoryId <= 0)
                return Json(new List<object>());

            var subCategories =
                await _subCategoryService.GetAllAsync();

            var result = subCategories
                .Where(x => x.CategoryId == categoryId)
                .Select(x => new
                {
                    id = x.Id,
                    name = x.Name
                })
                .ToList();

            return Json(result);
        }

        // =========================
        // LOAD DROPDOWNS
        // =========================

        private async Task LoadDropdownsAsync(
    ProductViewModel model)
        {
            // =========================
            // MAIN CATEGORY
            // =========================

            var mainCategories =
                await _mainCategoryService.GetAllAsync();

            model.MainCategories =
                mainCategories.Select(x =>
                    new SelectListItem
                    {
                        Value = x.Id.ToString(),
                        Text = x.Name,
                        Selected = x.Id == model.MainCategoryId
                    });


            // =========================
            // CATEGORY
            // =========================

            var Categories =
               await _categoryService.GetAllAsync();

            model.Categories =
                mainCategories.Select(x =>
                    new SelectListItem
                    {
                        Value = x.Id.ToString(),
                        Text = x.Name,
                        Selected = x.Id == model.CategoryId
                    });


            // =========================
            // SUB CATEGORY
            // =========================

           
                var subCategories =
                    await _subCategoryService.GetAllAsync();

            model.Categories =
            mainCategories.Select(x =>
                new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name,
                    Selected = x.Id == model.CategoryId
                });






            // =========================
            // SUPPLIER
            // =========================

            var suppliers =
                await _supplierService.GetAllAsync();

            model.Suppliers =
                suppliers.Select(x =>
                    new SelectListItem
                    {
                        Value = x.Id.ToString(),
                        Text = x.Name,
                        Selected = x.Id == model.SupplierId
                    });


            // =========================
            // WAREHOUSE
            // =========================

            var warehouses =
                await _warehouseService.GetAllAsync();

            model.Warehouses =
                warehouses.Select(x =>
                    new SelectListItem
                    {
                        Value = x.Id.ToString(),
                        Text = x.Name,
                        Selected = x.Id == model.WarehouseId
                    });
        }
    }
}