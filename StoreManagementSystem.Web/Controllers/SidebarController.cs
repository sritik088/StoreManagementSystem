using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Web.ViewModels.Sidebar;

namespace StoreManagementSystem.Web.Controllers
{
    [Authorize]
    public class SidebarController : Controller
    {
        private readonly ISidebarMenuService _sidebarMenuService;

        public SidebarController(
            ISidebarMenuService sidebarMenuService)
        {
            _sidebarMenuService = sidebarMenuService;
        }


        // =====================================================
        // ARRANGEMENT PAGE
        // =====================================================

        public async Task<IActionResult> Index()
        {
            var sections =
                await _sidebarMenuService.GetSectionsAsync();

            var model = new SidebarArrangementViewModel
            {
                Sections = sections
            };

            return View(model);
        }


        // =====================================================
        // SAVE ARRANGEMENT
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(
            [FromBody] SidebarArrangementRequest request)
        {
            if (request == null ||
                request.Sections == null ||
                request.Sections.Count == 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "No arrangement data was submitted."
                });
            }


            var sections =
                request.Sections
                    .Select(section => new SidebarMenuSection
                    {
                        Id = section.Id,

                        DisplayOrder =
                            section.DisplayOrder,

                        Items =
                            section.Items
                                .Select(item =>
                                    new SidebarMenuItem
                                    {
                                        Id = item.Id,

                                        SectionId =
                                            section.Id,

                                        DisplayOrder =
                                            item.DisplayOrder
                                    })
                                .ToList()
                    })
                    .ToList();


            await _sidebarMenuService
                .SaveArrangementAsync(sections);


            return Json(new
            {
                success = true,
                message = "Sidebar arrangement saved successfully."
            });
        }
    }
}