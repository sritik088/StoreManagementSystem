using Microsoft.AspNetCore.Mvc;
using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Web.ViewModels.MainCategory;

namespace StoreManagementSystem.Web.Controllers
{
    public class MainCategoryController : Controller
    {
        private readonly IMainCategoryService _service;

        public MainCategoryController(
            IMainCategoryService service)
        {
            _service = service;
        }

        // =========================
        // INDEX
        // =========================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var mainCategories =
                await _service.GetAllAsync();

            return View(mainCategories);
        }

        // =========================
        // CREATE - GET
        // =========================

        [HttpGet]
        public IActionResult Create()
        {
            return View(
                new MainCategoryViewModel());
        }

        // =========================
        // CREATE - POST
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            MainCategoryViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var exists =
                await _service.ExistsByNameAsync(
                    model.Name.Trim());

            if (exists)
            {
                ModelState.AddModelError(
                    "Name",
                    "This Main Category already exists.");

                return View(model);
            }

            var mainCategory = new MainCategory
            {
                Name = model.Name.Trim()
            };

            var result =
                await _service.CreateAsync(
                    mainCategory);

            if (!result)
            {
                ModelState.AddModelError(
                    "Name",
                    "Unable to create Main Category.");

                return View(model);
            }

            TempData["Success"] =
                "Main Category created successfully.";

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // EDIT - GET
        // =========================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var mainCategory =
                await _service.GetByIdAsync(id);

            if (mainCategory == null)
                return NotFound();

            var model = new MainCategoryViewModel
            {
                Id = mainCategory.Id,
                Name = mainCategory.Name
            };

            return View(model);
        }

        // =========================
        // EDIT - POST
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            MainCategoryViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var mainCategory =
                await _service.GetByIdAsync(model.Id);

            if (mainCategory == null)
                return NotFound();

            mainCategory.Name =
                model.Name.Trim();

            var result =
                await _service.UpdateAsync(
                    mainCategory);

            if (!result)
            {
                ModelState.AddModelError(
                    "Name",
                    "Main Category already exists or was not found.");

                return View(model);
            }

            TempData["Success"] =
                "Main Category updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // DETAILS
        // =========================

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var mainCategory =
                await _service.GetByIdAsync(id);

            if (mainCategory == null)
                return NotFound();

            return View(mainCategory);
        }

        // =========================
        // DELETE
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result =
                await _service.DeleteAsync(id);

            if (result)
            {
                TempData["Success"] =
                    "Main Category deleted successfully.";
            }
            else
            {
                TempData["Error"] =
                    "Main Category cannot be deleted because it contains Categories.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}