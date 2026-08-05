using Microsoft.AspNetCore.Mvc;
using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Web.ViewModels.Brand;

namespace StoreManagementSystem.Web.Controllers
{
    public class BrandController : Controller
    {
        private readonly IBrandService _brandService;

        public BrandController(IBrandService brandService)
        {
            _brandService = brandService;
        }

        //==========================
        // INDEX
        //==========================

        public async Task<IActionResult> Index(string search)
        {
            IEnumerable<Brand> brands;

            if (string.IsNullOrWhiteSpace(search))
                brands = await _brandService.GetAllAsync();
            else
                brands = await _brandService.SearchAsync(search);

            ViewBag.Search = search;

            ViewBag.TotalBrands = await _brandService.CountAsync();
            ViewBag.ActiveBrands = await _brandService.ActiveCountAsync();
            ViewBag.InactiveBrands = await _brandService.InactiveCountAsync();

            return View(brands);
        }

        //==========================
        // DETAILS
        //==========================

        public async Task<IActionResult> Details(int id)
        {
            var brand = await _brandService.GetByIdAsync(id);

            if (brand == null)
                return NotFound();

            return View(brand);
        }

        //==========================
        // CREATE GET
        //==========================

        [HttpGet]
        public IActionResult Create()
        {
            return View(new BrandViewModel());
        }

        //==========================
        // CREATE POST
        //==========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BrandViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var brand = new Brand
            {
                Name = model.Name,
                Code = model.Code,
                Country = model.Country,
                Website = model.Website,
                Description = model.Description,
                IsActive = model.IsActive
            };

            var created = await _brandService.CreateAsync(brand);

            if (!created)
            {
                ModelState.AddModelError("", "Brand name or code already exists.");
                return View(model);
            }

            TempData["Success"] = "Brand created successfully.";

            return RedirectToAction(nameof(Index));
        }

        //==========================
        // EDIT GET
        //==========================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var brand = await _brandService.GetByIdAsync(id);

            if (brand == null)
                return NotFound();

            var model = new BrandViewModel
            {
                Id = brand.Id,
                Name = brand.Name,
                Code = brand.Code,
                Country = brand.Country,
                Website = brand.Website,
                Description = brand.Description,
                IsActive = brand.IsActive
            };

            return View(model);
        }

        //==========================
        // EDIT POST
        //==========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BrandViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var brand = new Brand
            {
                Id = model.Id,
                Name = model.Name,
                Code = model.Code,
                Country = model.Country,
                Website = model.Website,
                Description = model.Description,
                IsActive = model.IsActive
            };

            var updated = await _brandService.UpdateAsync(brand);

            if (!updated)
            {
                ModelState.AddModelError("", "Unable to update brand.");
                return View(model);
            }

            TempData["Success"] = "Brand updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        //==========================
        // DELETE
        //==========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _brandService.DeleteAsync(id);

            TempData["Success"] = "Brand deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
