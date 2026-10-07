
namespace StoreManagementSystem.Application.Interfaces
{
    public interface ISystemSettingService
    {
        // =========================================================
        // ORGANISATION
        // =========================================================

        Task<string> GetOrganisationNameAsync();

        Task<string> GetBranchAsync();

        Task<string> GetAddressAsync();

        Task<string> GetPhoneAsync();

        Task<string> GetEmailAsync();

        Task<string> GetGSTINAsync();

        Task SetOrganisationDetailsAsync(
            string organisationName,
            string branch,
            string address,
            string phone,
            string email,
            string gstin);


        // =========================================================
        // REPORT SETTINGS
        // =========================================================

        Task<string> GetReportHeadingAsync();

        Task<string> GetFromDateLabelAsync();

        Task<string> GetToDateLabelAsync();

        Task SetReportSettingsAsync(
            string reportHeading,
            string fromDateLabel,
            string toDateLabel);


        // =========================================================
        // AUTHORISATION / SIGNATURE
        // =========================================================

        Task<string> GetAuthorisedSignatoryAsync();

        Task<string> GetAuthorisedDesignationAsync();

        Task SetAuthorisationSettingsAsync(
            string authorisedSignatory,
            string authorisedDesignation);


        // =========================================================
        // LOCATION
        // =========================================================

        Task<string> GetLocationLabelAsync();

        Task<string> GetLocationPluralLabelAsync();

        Task SetLocationLabelsAsync(
            string locationLabel,
            string locationPluralLabel);


        // =========================================================
        // SALES
        // =========================================================

        Task<bool> IsSalesOrderEnabledAsync();

        Task<string> GetSalesOrderLabelAsync();

        Task<string> GetSalesOrderPluralLabelAsync();

        Task SetSalesTerminologyAsync(
            string salesOrderLabel,
            string salesOrderPluralLabel,
            string locationLabel,
            string locationPluralLabel,
            bool enabled);

        Task<bool> GetCustomersEnabledAsync();

        Task SetCustomersEnabledAsync(bool enabled);
    }
}

