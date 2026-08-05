using Microsoft.AspNetCore.Mvc;
using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Web.ViewModels.Warehouse;

namespace StoreManagementSystem.Web.Controllers
{
    public class WarehouseController : Controller
    {
        private readonly IWarehouseService _warehouseService;

        public WarehouseController(IWarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        // ===========================
        // Index
        // ===========================

        public async Task<IActionResult> Index()
        {
            var warehouses = await _warehouseService.GetAllAsync();

            return View(warehouses);
        }

        // ===========================
        // Create (GET)
        // ===========================

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // ===========================
        // Create (POST)
        // ===========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WarehouseViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var warehouse = new Warehouse
            {
                Name = model.Name,
                Code = model.Code,
                ManagerName = model.ManagerName,
                ContactNumber = model.ContactNumber,
                Email = model.Email,
                Address = model.Address,
                City = model.City,
                State = model.State,
                Country = model.Country,
                PostalCode = model.PostalCode,
                Description = model.Description,
                IsActive = model.IsActive
            };

            var result = await _warehouseService.CreateAsync(warehouse);

            if (!result)
            {
                ModelState.AddModelError("", "Warehouse name already exists.");
                return View(model);
            }

            TempData["Success"] = "Warehouse created successfully.";

            return RedirectToAction(nameof(Index));
        }

        // ===========================
        // Edit (GET)
        // ===========================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var warehouse = await _warehouseService.GetByIdAsync(id);

            if (warehouse == null)
                return NotFound();

            var model = new WarehouseViewModel
            {
                Id = warehouse.Id,
                Name = warehouse.Name,
                Code = warehouse.Code,
                ManagerName = warehouse.ManagerName,
                ContactNumber = warehouse.ContactNumber,
                Email = warehouse.Email,
                Address = warehouse.Address,
                City = warehouse.City,
                State = warehouse.State,
                Country = warehouse.Country,
                PostalCode = warehouse.PostalCode,
                Description = warehouse.Description,
                IsActive = warehouse.IsActive
            };

            return View(model);
        }

        // ===========================
        // Edit (POST)
        // ===========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(WarehouseViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var warehouse = new Warehouse
            {
                Id = model.Id,
                Name = model.Name,
                Code = model.Code,
                ManagerName = model.ManagerName,
                ContactNumber = model.ContactNumber,
                Email = model.Email,
                Address = model.Address,
                City = model.City,
                State = model.State,
                Country = model.Country,
                PostalCode = model.PostalCode,
                Description = model.Description,
                IsActive = model.IsActive
            };

            var result = await _warehouseService.UpdateAsync(warehouse);

            if (!result)
            {
                ModelState.AddModelError("", "Warehouse name already exists.");
                return View(model);
            }

            TempData["Success"] = "Warehouse updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        // ===========================
        // Delete
        // ===========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _warehouseService.DeleteAsync(id);

            if (!result)
            {
                TempData["Error"] = "Warehouse not found.";
            }
            else
            {
                TempData["Success"] = "Warehouse deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}