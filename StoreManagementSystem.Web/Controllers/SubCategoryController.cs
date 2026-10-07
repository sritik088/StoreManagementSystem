using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Web.ViewModels.SubCategory;

namespace StoreManagementSystem.Web.Controllers
{
    public class SubCategoryController : Controller
    {
        private readonly ISubCategoryService _subCategoryService;
        private readonly ICategoryService _categoryService;

        public SubCategoryController(
            ISubCategoryService subCategoryService,
            ICategoryService categoryService)
        {
            _subCategoryService = subCategoryService;
            _categoryService = categoryService;
        }

        // =========================
        // INDEX
        // =========================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var data = await _subCategoryService.GetAllAsync();

            return View(data);
        }

        // =========================
        // CREATE - GET
        // =========================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadCategories();

            return View();
        }

        // =========================
        // CREATE - POST
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubCategoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadCategories();

                return View(model);
            }

            var subCategory = new SubCategory
            {
                CategoryId = model.CategoryId,
                Name = model.Name.Trim()
            };

            var result = await _subCategoryService
                .CreateAsync(subCategory);

            if (!result)
            {
                ModelState.AddModelError(
                    "",
                    "Unable to create Sub Category.");

                await LoadCategories();

                return View(model);
            }

            TempData["Success"] =
                "Sub Category created successfully.";

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // EDIT - GET
        // =========================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var subCategory =
                await _subCategoryService.GetByIdAsync(id);

            if (subCategory == null)
                return NotFound();

            var model = new SubCategoryViewModel
            {
                Id = subCategory.Id,
                CategoryId = subCategory.CategoryId,
                Name = subCategory.Name
            };

            await LoadCategories();

            return View(model);
        }

        // =========================
        // EDIT - POST
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            SubCategoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadCategories();

                return View(model);
            }

            var subCategory =
                await _subCategoryService
                    .GetByIdAsync(model.Id);

            if (subCategory == null)
                return NotFound();

            subCategory.CategoryId = model.CategoryId;
            subCategory.Name = model.Name.Trim();

            var result = await _subCategoryService
                .UpdateAsync(subCategory);

            if (!result)
            {
                ModelState.AddModelError(
                    "",
                    "Unable to update Sub Category.");

                await LoadCategories();

                return View(model);
            }

            TempData["Success"] =
                "Sub Category updated successfully.";

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
                await _subCategoryService.DeleteAsync(id);

            if (!result)
            {
                TempData["Error"] =
                    "Sub Category not found.";

                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] =
                "Sub Category deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // LOAD CATEGORY DROPDOWN
        // =========================

        private async Task LoadCategories()
        {
            var categories =
                await _categoryService.GetAllAsync();

            ViewBag.CategoryList = new SelectList(
                categories,
                "Id",
                "Name");
        }
    }
}