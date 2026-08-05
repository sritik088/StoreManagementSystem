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

        //======================
        // INDEX
        //======================

        public async Task<IActionResult> Index()
        {
            var data = await _subCategoryService.GetAllAsync();

            return View(data);
        }

        //======================
        // CREATE
        //======================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadCategories();

            return View();
        }

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
                Name = model.Name,
                Description = model.Description,
                DisplayOrder = model.DisplayOrder,
                IsActive = model.IsActive
            };

            await _subCategoryService.CreateAsync(subCategory);

            TempData["Success"] = "Sub Category created successfully.";

            return RedirectToAction(nameof(Index));
        }

        //======================
        // EDIT
        //======================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var sub = await _subCategoryService.GetByIdAsync(id);

            if (sub == null)
                return NotFound();

            var vm = new SubCategoryViewModel
            {
                Id = sub.Id,
                CategoryId = sub.CategoryId,
                Name = sub.Name,
                Description = sub.Description,
                DisplayOrder = sub.DisplayOrder,
                IsActive = sub.IsActive
            };

            await LoadCategories();

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SubCategoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadCategories();
                return View(model);
            }

            var sub = await _subCategoryService.GetByIdAsync(model.Id);

            if (sub == null)
                return NotFound();

            sub.CategoryId = model.CategoryId;
            sub.Name = model.Name;
            sub.Description = model.Description;
            sub.DisplayOrder = model.DisplayOrder;
            sub.IsActive = model.IsActive;

            await _subCategoryService.UpdateAsync(sub);

            TempData["Success"] = "Sub Category updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        //======================
        // DELETE
        //======================

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _subCategoryService.DeleteAsync(id);

            TempData["Success"] = "Sub Category deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        //======================
        // LOAD CATEGORY DROPDOWN
        //======================

        private async Task LoadCategories()
        {
            var categories = await _categoryService.GetAllAsync();

            ViewBag.CategoryList = new SelectList(
                categories,
                "Id",
                "Name");
        }
    }
}
