using Microsoft.AspNetCore.Mvc;
using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Web.ViewModels.Tax;

namespace StoreManagementSystem.Web.Controllers
{
    public class TaxController : Controller
    {
        private readonly ITaxService _taxService;

        public TaxController(ITaxService taxService)
        {
            _taxService = taxService;
        }

        // ===========================
        // Index
        // ===========================

        public async Task<IActionResult> Index()
        {
            var taxes = await _taxService.GetAllAsync();

            return View(taxes);
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
        public async Task<IActionResult> Create(TaxViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var tax = new Tax
            {
                Name = model.Name,
                TaxPercentage = model.TaxPercentage,
                TaxType = model.TaxType,
                Description = model.Description,
                IsActive = model.IsActive
            };

            var result = await _taxService.CreateAsync(tax);

            if (!result)
            {
                ModelState.AddModelError("", "Tax name already exists.");
                return View(model);
            }

            TempData["Success"] = "Tax created successfully.";

            return RedirectToAction(nameof(Index));
        }

        // ===========================
        // Edit (GET)
        // ===========================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var tax = await _taxService.GetByIdAsync(id);

            if (tax == null)
                return NotFound();

            var model = new TaxViewModel
            {
                Id = tax.Id,
                Name = tax.Name,
                TaxPercentage = tax.TaxPercentage,
                TaxType = tax.TaxType,
                Description = tax.Description,
                IsActive = tax.IsActive
            };

            return View(model);
        }

        // ===========================
        // Edit (POST)
        // ===========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TaxViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var tax = new Tax
            {
                Id = model.Id,
                Name = model.Name,
                TaxPercentage = model.TaxPercentage,
                TaxType = model.TaxType,
                Description = model.Description,
                IsActive = model.IsActive
            };

            var result = await _taxService.UpdateAsync(tax);

            if (!result)
            {
                ModelState.AddModelError("", "Tax name already exists.");
                return View(model);
            }

            TempData["Success"] = "Tax updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        // ===========================
        // Delete
        // ===========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _taxService.DeleteAsync(id);

            if (!result)
            {
                TempData["Error"] = "Tax not found.";
            }
            else
            {
                TempData["Success"] = "Tax deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
