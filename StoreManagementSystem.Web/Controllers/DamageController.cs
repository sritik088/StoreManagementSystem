using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Infrastructure.Data;

namespace StoreManagementSystem.Web.Controllers
{
    public class DamageController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DamageController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // INDEX
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var damages = await _context.Damages
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.DamageName)
                .ToListAsync();

            return View(damages);
        }

        // ============================================================
        // CREATE - GET
        // ============================================================

        [HttpGet]
        public IActionResult Create()
        {
            var model = new Damage
            {
                DamageCode = string.Empty,
                DamageName = string.Empty,
                IsActive = true
            };

            return View(model);
        }

        // ============================================================
        // CREATE - POST
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Damage model)
        {
            model.DamageCode = model.DamageCode?.Trim() ?? string.Empty;
            model.DamageName = model.DamageName?.Trim() ?? string.Empty;
            model.Description = string.IsNullOrWhiteSpace(model.Description)
                ? null
                : model.Description.Trim();

            // --------------------------------------------------------
            // Required validation
            // --------------------------------------------------------

            if (string.IsNullOrWhiteSpace(model.DamageCode))
            {
                ModelState.AddModelError(
                    nameof(model.DamageCode),
                    "Damage code is required.");
            }

            if (string.IsNullOrWhiteSpace(model.DamageName))
            {
                ModelState.AddModelError(
                    nameof(model.DamageName),
                    "Damage name is required.");
            }

            // --------------------------------------------------------
            // Duplicate code
            // --------------------------------------------------------

            if (!string.IsNullOrWhiteSpace(model.DamageCode))
            {
                var codeExists = await _context.Damages
                    .AnyAsync(x =>
                        x.DamageCode == model.DamageCode &&
                        !x.IsDeleted);

                if (codeExists)
                {
                    ModelState.AddModelError(
                        nameof(model.DamageCode),
                        "This damage code already exists.");
                }
            }

            // --------------------------------------------------------
            // Duplicate name
            // --------------------------------------------------------

            if (!string.IsNullOrWhiteSpace(model.DamageName))
            {
                var nameExists = await _context.Damages
                    .AnyAsync(x =>
                        x.DamageName == model.DamageName &&
                        !x.IsDeleted);

                if (nameExists)
                {
                    ModelState.AddModelError(
                        nameof(model.DamageName),
                        "This damage name already exists.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.IsDeleted = false;
            model.CreatedDate = DateTime.Now;
            model.UpdatedDate = null;

            _context.Damages.Add(model);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"Damage type '{model.DamageName}' created successfully.";

            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // EDIT - GET
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var damage = await _context.Damages
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDeleted);

            if (damage == null)
            {
                TempData["ErrorMessage"] =
                    "Damage type was not found.";

                return RedirectToAction(nameof(Index));
            }

            return View(damage);
        }

        // ============================================================
        // EDIT - POST
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Damage model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            model.DamageCode = model.DamageCode?.Trim() ?? string.Empty;
            model.DamageName = model.DamageName?.Trim() ?? string.Empty;
            model.Description = string.IsNullOrWhiteSpace(model.Description)
                ? null
                : model.Description.Trim();

            // --------------------------------------------------------
            // Required validation
            // --------------------------------------------------------

            if (string.IsNullOrWhiteSpace(model.DamageCode))
            {
                ModelState.AddModelError(
                    nameof(model.DamageCode),
                    "Damage code is required.");
            }

            if (string.IsNullOrWhiteSpace(model.DamageName))
            {
                ModelState.AddModelError(
                    nameof(model.DamageName),
                    "Damage name is required.");
            }

            // --------------------------------------------------------
            // Duplicate code
            // --------------------------------------------------------

            if (!string.IsNullOrWhiteSpace(model.DamageCode))
            {
                var codeExists = await _context.Damages
                    .AnyAsync(x =>
                        x.Id != id &&
                        x.DamageCode == model.DamageCode &&
                        !x.IsDeleted);

                if (codeExists)
                {
                    ModelState.AddModelError(
                        nameof(model.DamageCode),
                        "This damage code already exists.");
                }
            }

            // --------------------------------------------------------
            // Duplicate name
            // --------------------------------------------------------

            if (!string.IsNullOrWhiteSpace(model.DamageName))
            {
                var nameExists = await _context.Damages
                    .AnyAsync(x =>
                        x.Id != id &&
                        x.DamageName == model.DamageName &&
                        !x.IsDeleted);

                if (nameExists)
                {
                    ModelState.AddModelError(
                        nameof(model.DamageName),
                        "This damage name already exists.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existingDamage = await _context.Damages
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDeleted);

            if (existingDamage == null)
            {
                TempData["ErrorMessage"] =
                    "Damage type was not found.";

                return RedirectToAction(nameof(Index));
            }

            existingDamage.DamageCode = model.DamageCode;
            existingDamage.DamageName = model.DamageName;
            existingDamage.Description = model.Description;
            existingDamage.IsActive = model.IsActive;
            existingDamage.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"Damage type '{existingDamage.DamageName}' updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // TOGGLE ACTIVE / INACTIVE
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var damage = await _context.Damages
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDeleted);

            if (damage == null)
            {
                TempData["ErrorMessage"] =
                    "Damage type was not found.";

                return RedirectToAction(nameof(Index));
            }

            damage.IsActive = !damage.IsActive;
            damage.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                damage.IsActive
                    ? $"'{damage.DamageName}' activated successfully."
                    : $"'{damage.DamageName}' deactivated successfully.";

            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // DELETE - SOFT DELETE
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var damage = await _context.Damages
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDeleted);

            if (damage == null)
            {
                TempData["ErrorMessage"] =
                    "Damage type was not found.";

                return RedirectToAction(nameof(Index));
            }

            // --------------------------------------------------------
            // Check whether damage type is already used
            // --------------------------------------------------------

            var isUsed = await _context.DamageEntries
                .AnyAsync(x =>
                    x.DamageId == id &&
                    !x.IsDeleted);

            if (isUsed)
            {
                TempData["ErrorMessage"] =
                    $"'{damage.DamageName}' cannot be deleted because it is already used in Damage Entries. " +
                    "Deactivate it instead.";

                return RedirectToAction(nameof(Index));
            }

            damage.IsDeleted = true;
            damage.IsActive = false;
            damage.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"Damage type '{damage.DamageName}' deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}