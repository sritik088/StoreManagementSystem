
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Web.ViewModels.StockLedger;

namespace StoreManagementSystem.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class StockLedgerController : Controller
    {
        private readonly IStockLedgerRepository _ledgerRepository;
        private readonly IProductService _productService;
        private readonly IWarehouseService _warehouseService;
        private readonly IMainCategoryService _mainCategoryService;
        private readonly ICategoryService _categoryService;
        private readonly ISubCategoryService _subCategoryService;

        public StockLedgerController(
            IStockLedgerRepository ledgerRepository,
            IProductService productService,
            IWarehouseService warehouseService,
            IMainCategoryService mainCategoryService,
            ICategoryService categoryService,
            ISubCategoryService subCategoryService)
        {
            _ledgerRepository = ledgerRepository;
            _productService = productService;
            _warehouseService = warehouseService;
            _mainCategoryService = mainCategoryService;
            _categoryService = categoryService;
            _subCategoryService = subCategoryService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(DateTime? date)
        {
            DateTime selectedDate = date?.Date ?? DateTime.Today;
            DateTime endOfSelectedDate =
                selectedDate.AddDays(1).AddTicks(-1);

            var allLedgers =
                (await _ledgerRepository.GetAllAsync()).ToList();

            var products =
                (await _productService.GetAllAsync()).ToList();

            var warehouses =
                (await _warehouseService.GetAllAsync()).ToList();

            var mainCategories =
                (await _mainCategoryService.GetAllAsync()).ToList();

            var categories =
                (await _categoryService.GetAllAsync()).ToList();

            var subCategories =
                (await _subCategoryService.GetAllAsync()).ToList();

            var mainCategoryDictionary =
                mainCategories.ToDictionary(x => x.Id, x => x.Name);

            var categoryDictionary =
                categories.ToDictionary(x => x.Id, x => x.Name);

            var subCategoryDictionary =
                subCategories.ToDictionary(x => x.Id, x => x.Name);

            var productDictionary =
                products.ToDictionary(x => x.Id);

            var warehouseDictionary =
                warehouses.ToDictionary(x => x.Id);

            // Get ledger transactions up to the selected date.
            var ledgersUntilDate = allLedgers
                .Where(x => x.TransactionDate <= endOfSelectedDate)
                .ToList();

            // Get the latest ledger entry for each product and warehouse.
            var closingLedgers = ledgersUntilDate
                .GroupBy(x => new
                {
                    x.ProductId,
                    x.WarehouseId
                })
                .Select(group => group
                    .OrderByDescending(x => x.TransactionDate)
                    .ThenByDescending(x => x.Id)
                    .First())
                .ToList();

            var ledgerRows = closingLedgers
                .Where(x => productDictionary.ContainsKey(x.ProductId))
                .Select(x =>
                {
                    var product = productDictionary[x.ProductId];

                    string mainCategoryName = string.Empty;
                    string categoryName = string.Empty;
                    string subCategoryName = string.Empty;

                    // Resolve category and main category.
                    if (product.CategoryId > 0 &&
                        categoryDictionary.TryGetValue(
                            product.CategoryId,
                            out var categoryNameValue))
                    {
                        categoryName = categoryNameValue;

                        var category = categories.FirstOrDefault(
                            c => c.Id == product.CategoryId);

                        if (category != null &&
                            mainCategoryDictionary.TryGetValue(
                                category.MainCategoryId,
                                out var mainCategoryNameValue))
                        {
                            mainCategoryName = mainCategoryNameValue;
                        }
                    }

                    // Resolve subcategory and its parent category.
                    if (product.SubCategoryId > 0 &&
                        subCategoryDictionary.TryGetValue(
                            product.SubCategoryId,
                            out var subCategoryNameValue))
                    {
                        subCategoryName = subCategoryNameValue;

                        var subCategory = subCategories.FirstOrDefault(
                            s => s.Id == product.SubCategoryId);

                        if (subCategory != null)
                        {
                            var category = categories.FirstOrDefault(
                                c => c.Id == subCategory.CategoryId);

                            if (category != null)
                            {
                                if (string.IsNullOrWhiteSpace(categoryName))
                                {
                                    categoryName = category.Name;
                                }

                                if (mainCategoryDictionary.TryGetValue(
                                    category.MainCategoryId,
                                    out var mainCategoryNameValue))
                                {
                                    mainCategoryName = mainCategoryNameValue;
                                }
                            }
                        }
                    }

                    // Resolve warehouse name.
                    string warehouseName;

                    if (warehouseDictionary.TryGetValue(
                        x.WarehouseId,
                        out var warehouse))
                    {
                        warehouseName = warehouse.Name;
                    }
                    else
                    {
                        warehouseName =
                            x.Warehouse?.Name ?? "Unknown Warehouse";
                    }

                    return new StockLedgerViewModel
                    {
                        Id = x.Id,
                        ProductId = x.ProductId,

                        // Display Product Title instead of SKU.
                        ProductName = product.Title ?? string.Empty,

                        MainCategoryName = mainCategoryName,
                        CategoryName = categoryName,
                        SubCategoryName = subCategoryName,

                        WarehouseId = x.WarehouseId,
                        WarehouseName = warehouseName,

                        TransactionType = x.TransactionType,
                        Quantity = x.Quantity,
                        BalanceAfterTransaction =
                            x.BalanceAfterTransaction,
                        UnitCost = x.UnitCost,
                        ReferenceNo = x.ReferenceNo ?? string.Empty,
                        TransactionDate = x.TransactionDate
                    };
                })
                .OrderBy(x => x.WarehouseName)
                .ThenBy(x => x.ProductName)
                .ToList();

            // Group ledger rows by warehouse.
            var warehouseGroups = ledgerRows
                .GroupBy(x => new
                {
                    x.WarehouseId,
                    x.WarehouseName
                })
                .Select(group => new StockLedgerWarehouseGroupViewModel
                {
                    WarehouseId = group.Key.WarehouseId,
                    WarehouseName = group.Key.WarehouseName,

                    Products = group
                        .OrderBy(x => x.ProductName)
                        .ToList()
                })
                .OrderBy(x => x.WarehouseName)
                .ToList();

            var model = new StockLedgerIndexViewModel
            {
                SelectedDate = selectedDate,
                Warehouses = warehouseGroups
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var ledgers = await _ledgerRepository.GetAllAsync();

            var ledger = ledgers.FirstOrDefault(x => x.Id == id);

            if (ledger == null)
            {
                return NotFound();
            }

            return View(ledger);
        }

        [HttpGet]
        public IActionResult Product(int productId)
        {
            if (productId <= 0)
            {
                return BadRequest();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Warehouse(int warehouseId)
        {
            if (warehouseId <= 0)
            {
                return BadRequest();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult ProductWarehouse(
            int productId,
            int warehouseId)
        {
            if (productId <= 0 || warehouseId <= 0)
            {
                return BadRequest();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
