using Microsoft.AspNetCore.Mvc;
using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Web.ViewModels.Supplier;

namespace StoreManagementSystem.Web.Controllers
{
    public class SupplierController : Controller
    {
        private readonly ISupplierService _supplierService;

        public SupplierController(ISupplierService supplierService)
        {
            _supplierService = supplierService;
        }

        public async Task<IActionResult> Index(string? search)
        {
            IEnumerable<Supplier> suppliers;

            if (string.IsNullOrWhiteSpace(search))
            {
                suppliers = await _supplierService.GetAllAsync();
            }
            else
            {
                suppliers = await _supplierService.SearchAsync(search);
            }

            ViewBag.Search = search;

            return View(suppliers);
        }
        public async Task<IActionResult> Details(int id)
        {
            var supplier = await _supplierService.GetByIdAsync(id);

            if (supplier == null)
                return NotFound();

            return View(supplier);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SupplierViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var supplier = new Supplier
            {
                Name = model.Name,
                SupplierCode = model.SupplierCode,
                ContactPerson = model.ContactPerson,
                Phone = model.Phone,
                Email = model.Email,
                Address = model.Address,
                City = model.City,
                State = model.State,
                PostalCode = model.PostalCode,
                Country = model.Country,
                GSTNumber = model.GSTNumber,
                Website = model.Website,
                Notes = model.Notes,
                IsActive = model.IsActive
            };

            var result = await _supplierService.CreateAsync(supplier);

            if (!result)
            {
                ModelState.AddModelError("", "Supplier already exists.");
                return View(model);
            }

            TempData["Success"] = "Supplier created successfully.";

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var supplier = await _supplierService.GetByIdAsync(id);

            if (supplier == null)
                return NotFound();

            var model = new SupplierViewModel
            {
                Id = supplier.Id,
                Name = supplier.Name,
                SupplierCode = supplier.SupplierCode,
                ContactPerson = supplier.ContactPerson,
                Phone = supplier.Phone,
                Email = supplier.Email,
                Address = supplier.Address,
                City = supplier.City,
                State = supplier.State,
                PostalCode = supplier.PostalCode,
                Country = supplier.Country,
                GSTNumber = supplier.GSTNumber,
                Website = supplier.Website,
                Notes = supplier.Notes,
                IsActive = supplier.IsActive
            };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SupplierViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var supplier = new Supplier
            {
                Id = model.Id,
                Name = model.Name,
                SupplierCode = model.SupplierCode,
                ContactPerson = model.ContactPerson,
                Phone = model.Phone,
                Email = model.Email,
                Address = model.Address,
                City = model.City,
                State = model.State,
                PostalCode = model.PostalCode,
                Country = model.Country,
                GSTNumber = model.GSTNumber,
                Website = model.Website,
                Notes = model.Notes,
                IsActive = model.IsActive
            };

            var result = await _supplierService.UpdateAsync(supplier);

            if (!result)
            {
                ModelState.AddModelError("", "Supplier already exists.");
                return View(model);
            }

            TempData["Success"] = "Supplier updated successfully.";

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _supplierService.DeleteAsync(id);

            TempData["Success"] = "Supplier deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}