using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Web.ViewModels.Category;

namespace StoreManagementSystem.Web.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly IMainCategoryService _mainCategoryService;

        public CategoryController(
            ICategoryService categoryService,
            IMainCategoryService mainCategoryService)
        {
            _categoryService = categoryService;
            _mainCategoryService = mainCategoryService;
        }

        // =========================
        // INDEX
        // =========================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetAllAsync();

            await LoadMainCategories();

            return View(categories);
        }

        // =========================
        // CREATE - GET
        // =========================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadMainCategories();

            return View();
        }

        // =========================
        // CREATE - POST
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadMainCategories();

                return View(model);
            }

            var category = new Category
            {
                Name = model.Name.Trim(),
                MainCategoryId = model.MainCategoryId
            };

            var result = await _categoryService
                .CreateAsync(category);

            if (!result)
            {
                ModelState.AddModelError(
                    "",
                    "Category already exists.");

                await LoadMainCategories();

                return View(model);
            }

            TempData["Success"] =
                "Category created successfully.";

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // EDIT - GET
        // =========================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var category =
                await _categoryService.GetByIdAsync(id);

            if (category == null)
                return NotFound();

            var model = new CategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                MainCategoryId = category.MainCategoryId
            };

            await LoadMainCategories();

            return View(model);
        }

        // =========================
        // EDIT - POST
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            CategoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadMainCategories();

                return View(model);
            }

            var category =
                await _categoryService
                    .GetByIdAsync(model.Id);

            if (category == null)
                return NotFound();

            category.Name = model.Name.Trim();
            category.MainCategoryId = model.MainCategoryId;

            var result = await _categoryService
                .UpdateAsync(category);

            if (!result)
            {
                ModelState.AddModelError(
                    "",
                    "Unable to update category.");

                await LoadMainCategories();

                return View(model);
            }

            TempData["Success"] =
                "Category updated successfully.";

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
                await _categoryService.DeleteAsync(id);

            if (!result)
            {
                TempData["Error"] =
                    "Category not found.";

                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] =
                "Category deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // LOAD MAIN CATEGORIES
        // =========================

        private async Task LoadMainCategories()
        {
            var mainCategories =
                await _mainCategoryService.GetAllAsync();

            ViewBag.MainCategoryList = new SelectList(
                mainCategories,
                "Id",
                "Name");
        }
    }
}