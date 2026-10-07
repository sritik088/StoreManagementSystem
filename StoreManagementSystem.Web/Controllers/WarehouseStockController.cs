using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Application.ViewModels;

using StoreManagementSystem.Infrastructure.Identity;
using StoreManagementSystem.Web.Authorization;

namespace StoreManagementSystem.Web.Controllers
{
    [Authorize]
    [PermissionAuthorize(PermissionConstants.WarehouseStockView)]
    

    public class WarehouseStockController : Controller
    {
        private readonly IWarehouseStockService _stockService;
        private readonly IWarehouseService _warehouseService;
        private readonly UserManager<ApplicationUser> _userManager;

        public WarehouseStockController(
            IWarehouseStockService stockService,
            IWarehouseService warehouseService,
            UserManager<ApplicationUser> userManager)
        {
            _stockService = stockService;
            _warehouseService = warehouseService;
            _userManager = userManager;
        }

        // ==========================================
        // INDEX
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // ==========================================
            // GET CURRENT LOGGED-IN USER
            // ==========================================

            var currentUser =
                await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            // ==========================================
            // CHECK SUPER ADMIN
            // ==========================================

            bool isSuperAdmin =
                User.IsInRole(RoleConstants.SuperAdmin);


            // ==========================================
            // GET ALL ACTIVE WAREHOUSES
            // ==========================================

            var warehouses =
                await _warehouseService.GetAllAsync();


            var activeWarehouses = warehouses
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Name)
                .ToList();


            // ==========================================
            // WAREHOUSE MANAGER
            // ==========================================
            // WarehouseManager can only see the
            // warehouse assigned to the logged-in user.
            //
            // Example:
            //
            // Pranav
            // WarehouseId = Warehouse A
            //
            // Therefore Pranav sees only Warehouse A.
            // ==========================================

            if (!isSuperAdmin)
            {
                // ------------------------------------------
                // USER HAS NO WAREHOUSE ASSIGNED
                // ------------------------------------------

                if (!currentUser.WarehouseId.HasValue)
                {
                    ViewBag.Warehouses =
                        new List<StoreManagementSystem.Domain.Entities.Warehouse>();

                    ViewBag.TotalWarehouseCount = 0;

                    return View(
                        new List<WarehouseStockViewModel>());
                }


                // ------------------------------------------
                // KEEP ONLY ASSIGNED WAREHOUSE
                // ------------------------------------------

                activeWarehouses = activeWarehouses
                    .Where(x =>
                        x.Id == currentUser.WarehouseId.Value)
                    .ToList();
            }


            // ==========================================
            // GET ALL STOCK
            // ==========================================

            var stocks =
                await _stockService.GetAllAsync();


            // ==========================================
            // FILTER STOCK BY ASSIGNED WAREHOUSE
            // ==========================================

            if (!isSuperAdmin)
            {
                stocks = stocks
                    .Where(x =>
                        x.WarehouseId ==
                        currentUser.WarehouseId!.Value)
                    .ToList();
            }


            // ==========================================
            // CREATE VIEW MODEL
            // ==========================================

            var model = stocks
                .Select(stock => new WarehouseStockViewModel
                {
                    Id = stock.Id,

                    // ==========================================
                    // WAREHOUSE
                    // ==========================================

                    WarehouseId =
                        stock.WarehouseId,

                    WarehouseName =
                        stock.Warehouse?.Name
                        ?? "Unknown Warehouse",


                    // ==========================================
                    // PRODUCT
                    // ==========================================

                    SKU =
                        stock.Product?.SKU
                        ?? string.Empty,

                    Barcode =
                        stock.Product?.Barcode
                        ?? string.Empty,


                    // ==========================================
                    // CLOSING BALANCE
                    // ==========================================

                    ClosingBalance =
                        stock.QuantityOnHand,


                    // ==========================================
                    // STOCK LIMITS
                    // ==========================================

                    MinimumStock =
                        stock.MinimumStock,

                    MaximumStock =
                        stock.MaximumStock

                })
                .ToList();


            // ==========================================
            // SEND WAREHOUSE LIST TO VIEW
            // ==========================================

            ViewBag.Warehouses =
                activeWarehouses;


            // ==========================================
            // TOTAL WAREHOUSES
            // ==========================================

            ViewBag.TotalWarehouseCount =
                activeWarehouses.Count;


            // ==========================================
            // RETURN VIEW
            // ==========================================

            return View(model);
        }
    }
}