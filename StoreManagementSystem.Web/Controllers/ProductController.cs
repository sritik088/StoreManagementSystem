
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
    [Authorize(Roles = "SuperAdmin")]
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

        // =========================================================
        // INDEX
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllAsync();
            return View(products);
        }

        // =========================================================
        // CREATE - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new ProductViewModel();

            await LoadDropdownsAsync(model);

            return View(model);
        }

        // =========================================================
        // CREATE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel model)
        {
            NormalizeModel(model);

            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync(model);
                return View(model);
            }

            if (!await ValidateProductReferencesAsync(model))
            {
                await LoadDropdownsAsync(model);
                return View(model);
            }

            var product = new Product
            {
                Title = model.Title.Trim(),
                SKU = model.SKU.Trim(),

                Barcode = string.IsNullOrWhiteSpace(model.Barcode)
                    ? null
                    : model.Barcode.Trim(),

                MainCategoryId = model.MainCategoryId,
                CategoryId = model.CategoryId,
                SubCategoryId = model.SubCategoryId,

                SupplierId = model.SupplierId,
                WarehouseId = model.WarehouseId
            };

            var result = await _productService.CreateAsync(product);

            if (!result)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The product could not be created. Check whether the SKU or Barcode already exists.");

                await LoadDropdownsAsync(model);
                return View(model);
            }

            TempData["Success"] = "Product created successfully.";

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // EDIT - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
                return BadRequest();

            var product = await _productService.GetByIdAsync(id);

            if (product == null)
                return NotFound();

            var model = new ProductViewModel
            {
                Id = product.Id,
                Title = product.Title,
                SKU = product.SKU,
                Barcode = product.Barcode,

                MainCategoryId = product.MainCategoryId,
                CategoryId = product.CategoryId,
                SubCategoryId = product.SubCategoryId,

                SupplierId = product.SupplierId,
                WarehouseId = product.WarehouseId
            };

            await LoadDropdownsAsync(model);

            return View(model);
        }

        // =========================================================
        // EDIT - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductViewModel model)
        {
            NormalizeModel(model);

            if (model.Id <= 0)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync(model);
                return View(model);
            }

            var existingProduct =
                await _productService.GetByIdAsync(model.Id);

            if (existingProduct == null)
            {
                return NotFound();
            }

            if (!await ValidateProductReferencesAsync(model))
            {
                await LoadDropdownsAsync(model);
                return View(model);
            }

            var product = new Product
            {
                Id = model.Id,
                Title = model.Title.Trim(),
                SKU = model.SKU.Trim(),

                Barcode = string.IsNullOrWhiteSpace(model.Barcode)
                    ? null
                    : model.Barcode.Trim(),

                MainCategoryId = model.MainCategoryId,
                CategoryId = model.CategoryId,
                SubCategoryId = model.SubCategoryId,

                SupplierId = model.SupplierId,
                WarehouseId = model.WarehouseId
            };

            var result = await _productService.UpdateAsync(product);

            if (!result)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The product could not be updated. Check whether the SKU or Barcode already exists.");

                await LoadDropdownsAsync(model);
                return View(model);
            }

            TempData["Success"] = "Product updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // DELETE
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                TempData["Error"] = "Invalid product ID.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _productService.DeleteAsync(id);

            if (result)
            {
                TempData["Success"] = "Product deleted successfully.";
            }
            else
            {
                TempData["Error"] =
                    "Product not found or could not be deleted.";
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // AJAX: GET CATEGORIES BY MAIN CATEGORY
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> GetCategories(int mainCategoryId)
        {
            if (mainCategoryId <= 0)
                return Json(Array.Empty<object>());

            var categories = await _categoryService.GetAllAsync();

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

        // =========================================================
        // AJAX: GET SUBCATEGORIES BY CATEGORY
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> GetSubCategories(int categoryId)
        {
            if (categoryId <= 0)
                return Json(Array.Empty<object>());

            var subCategories = await _subCategoryService.GetAllAsync();

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

        // =========================================================
        // NORMALIZE INPUT
        // =========================================================

        private static void NormalizeModel(ProductViewModel model)
        {
            model.Title = model.Title?.Trim() ?? string.Empty;
            model.SKU = model.SKU?.Trim() ?? string.Empty;

            model.Barcode = string.IsNullOrWhiteSpace(model.Barcode)
                ? null
                : model.Barcode.Trim();
        }

        // =========================================================
        // VALIDATE CATEGORY, SUPPLIER AND WAREHOUSE REFERENCES
        // =========================================================

        private async Task<bool> ValidateProductReferencesAsync(
            ProductViewModel model)
        {
            var mainCategories =
                await _mainCategoryService.GetAllAsync();

            var mainCategoryExists = mainCategories.Any(
                x => x.Id == model.MainCategoryId);

            if (!mainCategoryExists)
            {
                ModelState.AddModelError(
                    nameof(model.MainCategoryId),
                    "Please select a valid Main Category.");
            }

            var categories = await _categoryService.GetAllAsync();

            var categoryExists = categories.Any(
                x => x.Id == model.CategoryId &&
                     x.MainCategoryId == model.MainCategoryId);

            if (!categoryExists)
            {
                ModelState.AddModelError(
                    nameof(model.CategoryId),
                    "Please select a Category belonging to the selected Main Category.");
            }

            var subCategories =
                await _subCategoryService.GetAllAsync();

            var subCategoryExists = subCategories.Any(
                x => x.Id == model.SubCategoryId &&
                     x.CategoryId == model.CategoryId);

            if (!subCategoryExists)
            {
                ModelState.AddModelError(
                    nameof(model.SubCategoryId),
                    "Please select a Sub Category belonging to the selected Category.");
            }

            var suppliers = await _supplierService.GetAllAsync();

            if (!suppliers.Any(x => x.Id == model.SupplierId))
            {
                ModelState.AddModelError(
                    nameof(model.SupplierId),
                    "Please select a valid Supplier.");
            }

            var warehouses = await _warehouseService.GetAllAsync();

            if (!warehouses.Any(x => x.Id == model.WarehouseId))
            {
                ModelState.AddModelError(
                    nameof(model.WarehouseId),
                    "Please select a valid Warehouse.");
            }

            return ModelState.IsValid;
        }

        // =========================================================
        // LOAD DROPDOWNS
        // =========================================================

        private async Task LoadDropdownsAsync(ProductViewModel model)
        {
            // Main Categories
            var mainCategories =
                await _mainCategoryService.GetAllAsync();

            model.MainCategories = mainCategories
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name,
                    Selected = x.Id == model.MainCategoryId
                })
                .ToList();

            // Categories belonging to selected Main Category
            var categories =
                await _categoryService.GetAllAsync();

            model.Categories = categories
                .Where(x =>
                    model.MainCategoryId > 0 &&
                    x.MainCategoryId == model.MainCategoryId)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name,
                    Selected = x.Id == model.CategoryId
                })
                .ToList();

            // Subcategories belonging to selected Category
            var subCategories =
                await _subCategoryService.GetAllAsync();

            model.SubCategories = subCategories
                .Where(x =>
                    model.CategoryId > 0 &&
                    x.CategoryId == model.CategoryId)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name,
                    Selected = x.Id == model.SubCategoryId
                })
                .ToList();

            // Suppliers
            var suppliers =
                await _supplierService.GetAllAsync();

            model.Suppliers = suppliers
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name,
                    Selected = x.Id == model.SupplierId
                })
                .ToList();

            // Warehouses
            var warehouses =
                await _warehouseService.GetAllAsync();

            model.Warehouses = warehouses
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name,
                    Selected = x.Id == model.WarehouseId
                })
                .ToList();
        }
    }
}
