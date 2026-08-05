using Microsoft.AspNetCore.Mvc;
using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Web.ViewModels.Unit;

namespace StoreManagementSystem.Web.Controllers
{
    public class UnitController : Controller
    {
        private readonly IUnitService _unitService;

        public UnitController(IUnitService unitService)
        {
            _unitService = unitService;
        }

        public async Task<IActionResult> Index()
        {
            var units = await _unitService.GetAllAsync();

            return View(units);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UnitViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var unit = new Unit
            {
                Name = model.Name,
                ShortName = model.ShortName,
                Description = model.Description,
                IsActive = model.IsActive
            };

            var result = await _unitService.CreateAsync(unit);

            if (!result)
            {
                ModelState.AddModelError("", "Unit name already exists.");
                return View(model);
            }

            TempData["Success"] = "Unit created successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var unit = await _unitService.GetByIdAsync(id);

            if (unit == null)
                return NotFound();

            var model = new UnitViewModel
            {
                Id = unit.Id,
                Name = unit.Name,
                ShortName = unit.ShortName,
                Description = unit.Description,
                IsActive = unit.IsActive
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UnitViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var unit = new Unit
            {
                Id = model.Id,
                Name = model.Name,
                ShortName = model.ShortName,
                Description = model.Description,
                IsActive = model.IsActive
            };

            var result = await _unitService.UpdateAsync(unit);

            if (!result)
            {
                ModelState.AddModelError("", "Unit name already exists.");
                return View(model);
            }

            TempData["Success"] = "Unit updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _unitService.DeleteAsync(id);

            if (!result)
            {
                TempData["Error"] = "Unit not found.";
            }
            else
            {
                TempData["Success"] = "Unit deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
