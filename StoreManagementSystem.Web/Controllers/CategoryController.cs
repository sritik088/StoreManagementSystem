using Microsoft.AspNetCore.Mvc;
using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Web.ViewModels.Category;

namespace StoreManagementSystem.Web.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryService _service;

        public CategoryController(ICategoryService service)
        {
            _service = service;
        }

        // =========================
        // INDEX
        // =========================

        public async Task<IActionResult> Index()
        {
            var categories = await _service.GetAllAsync();

            return View(categories);
        }

        // =========================
        // CREATE
        // =========================

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var category = new Category
            {
                Name = model.Name,
                Description = model.Description,
                DisplayOrder = model.DisplayOrder,
                IsActive = model.IsActive
            };

            var result = await _service.CreateAsync(category);

            if (!result)
            {
                ModelState.AddModelError("", "Category already exists.");
                return View(model);
            }

            TempData["Success"] = "Category created successfully.";

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // EDIT
        // =========================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _service.GetByIdAsync(id);

            if (category == null)
                return NotFound();

            var model = new CategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                DisplayOrder = category.DisplayOrder,
                IsActive = category.IsActive
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var category = await _service.GetByIdAsync(model.Id);

            if (category == null)
                return NotFound();

            category.Name = model.Name;
            category.Description = model.Description;
            category.DisplayOrder = model.DisplayOrder;
            category.IsActive = model.IsActive;
            category.UpdatedDate = DateTime.Now;

            await _service.UpdateAsync(category);

            TempData["Success"] = "Category updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // DELETE
        // =========================

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);

            TempData["Success"] = "Category deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
