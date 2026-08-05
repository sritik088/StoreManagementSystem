using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Web.ViewModels.Product;

namespace StoreManagementSystem.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ISubCategoryService _subCategoryService;
        private readonly IBrandService _brandService;
        private readonly ISupplierService _supplierService;
        private readonly IUnitService _unitService;
        private readonly ITaxService _taxService;
        private readonly IWarehouseService _warehouseService;
        private readonly IWebHostEnvironment _environment;

        public ProductController(
            IProductService productService,
            ICategoryService categoryService,
            ISubCategoryService subCategoryService,
            IBrandService brandService,
            ISupplierService supplierService,
            IUnitService unitService,
            ITaxService taxService,
            IWarehouseService warehouseService,
            IWebHostEnvironment environment)
        {
            _productService = productService;
            _categoryService = categoryService;
            _subCategoryService = subCategoryService;
            _brandService = brandService;
            _supplierService = supplierService;
            _unitService = unitService;
            _taxService = taxService;
            _warehouseService = warehouseService;
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllAsync();

            return View(products);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetByIdAsync(id);

            if (product == null)
                return NotFound();

            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new ProductViewModel();

            await LoadDropdowns(model);

            return View(model);
        }

        private async Task LoadDropdowns(ProductViewModel model)
        {
            model.Categories = (await _categoryService.GetAllAsync())
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name
                });

            model.SubCategories = (await _subCategoryService.GetAllAsync())
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name
                });

            model.Brands = (await _brandService.GetAllAsync())
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name
                });

            model.Suppliers = (await _supplierService.GetAllAsync())
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name
                });

            model.Units = (await _unitService.GetAllAsync())
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name
                });

            model.Taxes = (await _taxService.GetAllAsync())
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name
                });

            model.Warehouses = (await _warehouseService.GetAllAsync())
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name
                });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdowns(model);
                return View(model);
            }

            string? imagePath = null;

            if (model.ImageFile != null)
            {
                string folder = Path.Combine(_environment.WebRootPath, "uploads/products");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string fileName = Guid.NewGuid() +
                                  Path.GetExtension(model.ImageFile.FileName);

                string filePath = Path.Combine(folder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);

                await model.ImageFile.CopyToAsync(stream);

                imagePath = "/uploads/products/" + fileName;
            }

            var product = new Product
            {
                Name = model.Name,
                SKU = model.SKU,
                Barcode = model.Barcode,
                HSNCode = model.HSNCode,

                CategoryId = model.CategoryId,
                SubCategoryId = model.SubCategoryId,
                BrandId = model.BrandId,
                SupplierId = model.SupplierId,
                UnitId = model.UnitId,
                TaxId = model.TaxId,
                WarehouseId = model.WarehouseId,

                PurchasePrice = model.PurchasePrice,
                SellingPrice = model.SellingPrice,
                DiscountPrice = model.DiscountPrice,

                OpeningStock = model.OpeningStock,
                CurrentStock = model.CurrentStock,
                ReorderLevel = model.ReorderLevel,
                MaximumStock = model.MaximumStock,

                ImageUrl = imagePath,

                Description = model.Description,

                IsActive = model.IsActive
            };

            var result = await _productService.CreateAsync(product);

            if (!result)
            {
                ModelState.AddModelError("", "Product Name or SKU already exists.");

                await LoadDropdowns(model);

                return View(model);
            }

            TempData["Success"] = "Product created successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.GetByIdAsync(id);

            if (product == null)
                return NotFound();

            var model = new ProductViewModel
            {
                Id = product.Id,

                Name = product.Name,
                SKU = product.SKU,
                Barcode = product.Barcode,
                HSNCode = product.HSNCode,

                CategoryId = product.CategoryId,
                SubCategoryId = product.SubCategoryId,
                BrandId = product.BrandId,
                SupplierId = product.SupplierId,
                UnitId = product.UnitId,
                TaxId = product.TaxId,
                WarehouseId = product.WarehouseId,

                PurchasePrice = product.PurchasePrice,
                SellingPrice = product.SellingPrice,
                DiscountPrice = product.DiscountPrice,

                OpeningStock = product.OpeningStock,
                CurrentStock = product.CurrentStock,
                ReorderLevel = product.ReorderLevel,
                MaximumStock = product.MaximumStock,

                ImageUrl = product.ImageUrl,

                Description = product.Description,

                IsActive = product.IsActive
            };

            await LoadDropdowns(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdowns(model);
                return View(model);
            }

            var existing = await _productService.GetByIdAsync(model.Id);

            if (existing == null)
                return NotFound();

            string imagePath = existing.ImageUrl ?? "";

            if (model.ImageFile != null)
            {
                // Delete old image
                if (!string.IsNullOrWhiteSpace(existing.ImageUrl))
                {
                    string oldFile = Path.Combine(
                        _environment.WebRootPath,
                        existing.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

                    if (System.IO.File.Exists(oldFile))
                        System.IO.File.Delete(oldFile);
                }

                // Upload new image
                string folder = Path.Combine(_environment.WebRootPath, "uploads", "products");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string fileName = Guid.NewGuid() + Path.GetExtension(model.ImageFile.FileName);

                string filePath = Path.Combine(folder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);

                await model.ImageFile.CopyToAsync(stream);

                imagePath = "/uploads/products/" + fileName;
            }

            var product = new Product
            {
                Id = model.Id,

                Name = model.Name,
                SKU = model.SKU,
                Barcode = model.Barcode,
                HSNCode = model.HSNCode,

                CategoryId = model.CategoryId,
                SubCategoryId = model.SubCategoryId,
                BrandId = model.BrandId,
                SupplierId = model.SupplierId,
                UnitId = model.UnitId,
                TaxId = model.TaxId,
                WarehouseId = model.WarehouseId,

                PurchasePrice = model.PurchasePrice,
                SellingPrice = model.SellingPrice,
                DiscountPrice = model.DiscountPrice,

                OpeningStock = model.OpeningStock,
                CurrentStock = model.CurrentStock,
                ReorderLevel = model.ReorderLevel,
                MaximumStock = model.MaximumStock,

                ImageUrl = imagePath,

                Description = model.Description,

                IsActive = model.IsActive
            };

            var result = await _productService.UpdateAsync(product);

            if (!result)
            {
                ModelState.AddModelError("", "Product Name or SKU already exists.");

                await LoadDropdowns(model);

                return View(model);
            }

            TempData["Success"] = "Product updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _productService.DeleteAsync(id);

            if (!result)
            {
                TempData["Error"] = "Product not found.";
            }
            else
            {
                TempData["Success"] = "Product deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<JsonResult> GetSubCategories(int categoryId)
        {
            var subCategories = await _subCategoryService.GetAllAsync();

            var data = subCategories
                .Where(x => x.CategoryId == categoryId)
                .Select(x => new
                {
                    id = x.Id,
                    name = x.Name
                });

            return Json(data);
        }

        private string GenerateSku()
        {
            return "PRD" + DateTime.Now.ToString("yyyyMMddHHmmss");
        }



    }
}
