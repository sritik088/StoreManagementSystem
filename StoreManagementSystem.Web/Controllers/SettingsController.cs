using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Application.ViewModels;

namespace StoreManagementSystem.Web.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly ISystemSettingService _systemSettingService;

        public SettingsController(
            ISystemSettingService systemSettingService)
        {
            _systemSettingService = systemSettingService;
        }


        // =========================================================
        // GET: /Settings
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = new SystemSettingViewModel
            {
                // =================================================
                // ORGANISATION
                // =================================================

                OrganisationName =
                    await _systemSettingService
                        .GetOrganisationNameAsync(),

                Branch =
                    await _systemSettingService
                        .GetBranchAsync(),

                Address =
                    await _systemSettingService
                        .GetAddressAsync(),

                Phone =
                    await _systemSettingService
                        .GetPhoneAsync(),

                Email =
                    await _systemSettingService
                        .GetEmailAsync(),

                GSTIN =
                    await _systemSettingService
                        .GetGSTINAsync(),


                // =================================================
                // REPORT SETTINGS
                // =================================================

                ReportHeading =
                    await _systemSettingService
                        .GetReportHeadingAsync(),

                FromDateLabel =
                    await _systemSettingService
                        .GetFromDateLabelAsync(),

                ToDateLabel =
                    await _systemSettingService
                        .GetToDateLabelAsync(),


                // =================================================
                // AUTHORISATION / SIGNATURE
                // =================================================

                AuthorisedSignatory =
                    await _systemSettingService
                        .GetAuthorisedSignatoryAsync(),

                AuthorisedDesignation =
                    await _systemSettingService
                        .GetAuthorisedDesignationAsync(),


                // =================================================
                // SALES
                // =================================================

                SalesOrderEnabled =
                    await _systemSettingService
                        .IsSalesOrderEnabledAsync(),

                SalesOrderLabel =
                    await _systemSettingService
                        .GetSalesOrderLabelAsync(),

                SalesOrderPluralLabel =
                    await _systemSettingService
                        .GetSalesOrderPluralLabelAsync(),


                // =================================================
                // LOCATION
                // =================================================

                LocationLabel =
                    await _systemSettingService
                        .GetLocationLabelAsync(),

                LocationPluralLabel =
                    await _systemSettingService
                        .GetLocationPluralLabelAsync(),


                // =================================================
                // CUSTOMERS
                // =================================================

                CustomersEnabled =
                    await _systemSettingService
                        .GetCustomersEnabledAsync()
            };

            return View(model);
        }


        // =========================================================
        // POST: /Settings
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(
            SystemSettingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            // =====================================================
            // ORGANISATION
            // =====================================================

            await _systemSettingService
                .SetOrganisationDetailsAsync(
                    model.OrganisationName,
                    model.Branch,
                    model.Address,
                    model.Phone,
                    model.Email,
                    model.GSTIN);


            // =====================================================
            // REPORT SETTINGS
            // =====================================================

            await _systemSettingService
                .SetReportSettingsAsync(
                    model.ReportHeading,
                    model.FromDateLabel,
                    model.ToDateLabel);


            // =====================================================
            // AUTHORISATION / SIGNATURE
            // =====================================================

            await _systemSettingService
                .SetAuthorisationSettingsAsync(
                    model.AuthorisedSignatory,
                    model.AuthorisedDesignation);


            // =====================================================
            // SALES + LOCATION
            // =====================================================

            await _systemSettingService
                .SetSalesTerminologyAsync(
                    model.SalesOrderLabel,
                    model.SalesOrderPluralLabel,
                    model.LocationLabel,
                    model.LocationPluralLabel,
                    model.SalesOrderEnabled);


            // =====================================================
            // CUSTOMERS
            // =====================================================

            await _systemSettingService
                .SetCustomersEnabledAsync(
                    model.CustomersEnabled);


            // =====================================================
            // SUCCESS
            // =====================================================

            TempData["SuccessMessage"] =
                "System settings updated successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}