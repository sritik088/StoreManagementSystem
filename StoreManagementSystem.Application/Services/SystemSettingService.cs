using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;

namespace StoreManagementSystem.Application.Services
{
    public class SystemSettingService : ISystemSettingService
    {
        private readonly ISystemSettingRepository _repository;


        // =========================================================
        // ORGANISATION SETTINGS
        // =========================================================

        private const string OrganisationNameKey =
            "OrganisationName";

        private const string BranchKey =
            "Branch";

        private const string AddressKey =
            "Address";

        private const string PhoneKey =
            "Phone";

        private const string EmailKey =
            "Email";

        private const string GSTINKey =
            "GSTIN";


        // =========================================================
        // REPORT SETTINGS
        // =========================================================

        private const string ReportHeadingKey =
            "ReportHeading";

        private const string FromDateLabelKey =
            "FromDateLabel";

        private const string ToDateLabelKey =
            "ToDateLabel";


        // =========================================================
        // AUTHORISATION / SIGNATURE SETTINGS
        // =========================================================

        private const string AuthorisedSignatoryKey =
            "AuthorisedSignatory";

        private const string AuthorisedDesignationKey =
            "AuthorisedDesignation";


        // =========================================================
        // LOCATION SETTINGS
        // =========================================================

        private const string LocationLabelKey =
            "LocationLabel";

        private const string LocationPluralLabelKey =
            "LocationPluralLabel";


        // =========================================================
        // SALES SETTINGS
        // =========================================================

        private const string SalesOrderLabelKey =
            "SalesOrderLabel";

        private const string SalesOrderPluralLabelKey =
            "SalesOrderPluralLabel";

        private const string SalesOrderEnabledKey =
            "SalesOrderEnabled";


        // =========================================================
        // CUSTOMER SETTINGS
        // =========================================================

        private const string CustomersEnabledKey =
            "CustomersEnabled";


        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public SystemSettingService(
            ISystemSettingRepository repository)
        {
            _repository = repository;
        }


        // =========================================================
        // ORGANISATION - NAME
        // =========================================================

        public async Task<string> GetOrganisationNameAsync()
        {
            return await GetSettingValueAsync(
                OrganisationNameKey,
                string.Empty);
        }


        // =========================================================
        // ORGANISATION - BRANCH
        // =========================================================

        public async Task<string> GetBranchAsync()
        {
            return await GetSettingValueAsync(
                BranchKey,
                string.Empty);
        }


        // =========================================================
        // ORGANISATION - ADDRESS
        // =========================================================

        public async Task<string> GetAddressAsync()
        {
            return await GetSettingValueAsync(
                AddressKey,
                string.Empty);
        }


        // =========================================================
        // ORGANISATION - PHONE
        // =========================================================

        public async Task<string> GetPhoneAsync()
        {
            return await GetSettingValueAsync(
                PhoneKey,
                string.Empty);
        }


        // =========================================================
        // ORGANISATION - EMAIL
        // =========================================================

        public async Task<string> GetEmailAsync()
        {
            return await GetSettingValueAsync(
                EmailKey,
                string.Empty);
        }


        // =========================================================
        // ORGANISATION - GSTIN
        // =========================================================

        public async Task<string> GetGSTINAsync()
        {
            return await GetSettingValueAsync(
                GSTINKey,
                string.Empty);
        }


        // =========================================================
        // SAVE ORGANISATION DETAILS
        // =========================================================

        public async Task SetOrganisationDetailsAsync(
            string organisationName,
            string branch,
            string address,
            string phone,
            string email,
            string gstin)
        {
            await SaveSettingAsync(
                OrganisationNameKey,
                organisationName?.Trim() ?? string.Empty);

            await SaveSettingAsync(
                BranchKey,
                branch?.Trim() ?? string.Empty);

            await SaveSettingAsync(
                AddressKey,
                address?.Trim() ?? string.Empty);

            await SaveSettingAsync(
                PhoneKey,
                phone?.Trim() ?? string.Empty);

            await SaveSettingAsync(
                EmailKey,
                email?.Trim() ?? string.Empty);

            await SaveSettingAsync(
                GSTINKey,
                gstin?.Trim() ?? string.Empty);
        }


        // =========================================================
        // REPORT - HEADING
        // =========================================================

        public async Task<string> GetReportHeadingAsync()
        {
            return await GetSettingValueAsync(
                ReportHeadingKey,
                string.Empty);
        }


        // =========================================================
        // REPORT - FROM LABEL
        // =========================================================

        public async Task<string> GetFromDateLabelAsync()
        {
            return await GetSettingValueAsync(
                FromDateLabelKey,
                "From");
        }


        // =========================================================
        // REPORT - TO LABEL
        // =========================================================

        public async Task<string> GetToDateLabelAsync()
        {
            return await GetSettingValueAsync(
                ToDateLabelKey,
                "To");
        }


        // =========================================================
        // SAVE REPORT SETTINGS
        // =========================================================

        public async Task SetReportSettingsAsync(
            string reportHeading,
            string fromDateLabel,
            string toDateLabel)
        {
            if (string.IsNullOrWhiteSpace(reportHeading))
            {
                reportHeading = "Report";
            }

            if (string.IsNullOrWhiteSpace(fromDateLabel))
            {
                fromDateLabel = "From";
            }

            if (string.IsNullOrWhiteSpace(toDateLabel))
            {
                toDateLabel = "To";
            }

            await SaveSettingAsync(
                ReportHeadingKey,
                reportHeading.Trim());

            await SaveSettingAsync(
                FromDateLabelKey,
                fromDateLabel.Trim());

            await SaveSettingAsync(
                ToDateLabelKey,
                toDateLabel.Trim());
        }


        // =========================================================
        // AUTHORISED SIGNATORY
        // =========================================================

        public async Task<string> GetAuthorisedSignatoryAsync()
        {
            return await GetSettingValueAsync(
                AuthorisedSignatoryKey,
                string.Empty);
        }


        // =========================================================
        // AUTHORISED DESIGNATION
        // =========================================================

        public async Task<string> GetAuthorisedDesignationAsync()
        {
            return await GetSettingValueAsync(
                AuthorisedDesignationKey,
                string.Empty);
        }


        // =========================================================
        // SAVE AUTHORISATION SETTINGS
        // =========================================================

        public async Task SetAuthorisationSettingsAsync(
            string authorisedSignatory,
            string authorisedDesignation)
        {
            await SaveSettingAsync(
                AuthorisedSignatoryKey,
                authorisedSignatory?.Trim() ?? string.Empty);

            await SaveSettingAsync(
                AuthorisedDesignationKey,
                authorisedDesignation?.Trim() ?? string.Empty);
        }


        // =========================================================
        // LOCATION - SINGULAR
        // =========================================================

        public async Task<string> GetLocationLabelAsync()
        {
            var setting =
                await _repository.GetByKeyAsync(
                    LocationLabelKey);

            if (setting == null ||
                string.IsNullOrWhiteSpace(setting.Value))
            {
                return "Warehouse";
            }

            return setting.Value;
        }


        // =========================================================
        // LOCATION - PLURAL
        // =========================================================

        public async Task<string> GetLocationPluralLabelAsync()
        {
            var setting =
                await _repository.GetByKeyAsync(
                    LocationPluralLabelKey);

            if (setting == null ||
                string.IsNullOrWhiteSpace(setting.Value))
            {
                return "Warehouses";
            }

            return setting.Value;
        }


        // =========================================================
        // SAVE LOCATION
        // =========================================================

        public async Task SetLocationLabelsAsync(
            string locationLabel,
            string locationPluralLabel)
        {
            if (string.IsNullOrWhiteSpace(locationLabel))
            {
                locationLabel = "Warehouse";
            }

            if (string.IsNullOrWhiteSpace(locationPluralLabel))
            {
                locationPluralLabel = "Warehouses";
            }

            await SaveSettingAsync(
                LocationLabelKey,
                locationLabel.Trim());

            await SaveSettingAsync(
                LocationPluralLabelKey,
                locationPluralLabel.Trim());
        }


        // =========================================================
        // SALES - ENABLE / DISABLE
        // =========================================================

        public async Task<bool> IsSalesOrderEnabledAsync()
        {
            var setting =
                await _repository.GetByKeyAsync(
                    SalesOrderEnabledKey);

            if (setting == null ||
                string.IsNullOrWhiteSpace(setting.Value))
            {
                return true;
            }

            return bool.TryParse(
                setting.Value,
                out bool enabled)
                ? enabled
                : true;
        }


        // =========================================================
        // SALES - SINGULAR
        // =========================================================

        public async Task<string> GetSalesOrderLabelAsync()
        {
            var setting =
                await _repository.GetByKeyAsync(
                    SalesOrderLabelKey);

            if (setting == null ||
                string.IsNullOrWhiteSpace(setting.Value))
            {
                return "Sale";
            }

            return setting.Value;
        }


        // =========================================================
        // SALES - PLURAL
        // =========================================================

        public async Task<string> GetSalesOrderPluralLabelAsync()
        {
            var setting =
                await _repository.GetByKeyAsync(
                    SalesOrderPluralLabelKey);

            if (setting == null ||
                string.IsNullOrWhiteSpace(setting.Value))
            {
                return "Sales";
            }

            return setting.Value;
        }


        // =========================================================
        // SAVE ALL SALES + LOCATION SETTINGS
        // =========================================================

        public async Task SetSalesTerminologyAsync(
            string salesOrderLabel,
            string salesOrderPluralLabel,
            string locationLabel,
            string locationPluralLabel,
            bool enabled)
        {
            if (string.IsNullOrWhiteSpace(salesOrderLabel))
            {
                salesOrderLabel = "Sale";
            }

            if (string.IsNullOrWhiteSpace(salesOrderPluralLabel))
            {
                salesOrderPluralLabel = "Sales";
            }

            if (string.IsNullOrWhiteSpace(locationLabel))
            {
                locationLabel = "Warehouse";
            }

            if (string.IsNullOrWhiteSpace(locationPluralLabel))
            {
                locationPluralLabel = "Warehouses";
            }

            await SaveSettingAsync(
                SalesOrderLabelKey,
                salesOrderLabel.Trim());

            await SaveSettingAsync(
                SalesOrderPluralLabelKey,
                salesOrderPluralLabel.Trim());

            await SaveSettingAsync(
                SalesOrderEnabledKey,
                enabled.ToString());

            await SaveSettingAsync(
                LocationLabelKey,
                locationLabel.Trim());

            await SaveSettingAsync(
                LocationPluralLabelKey,
                locationPluralLabel.Trim());
        }


        // =========================================================
        // CUSTOMERS - ENABLE / DISABLE
        // =========================================================

        public async Task<bool> GetCustomersEnabledAsync()
        {
            var setting =
                await _repository.GetByKeyAsync(
                    CustomersEnabledKey);

            // Default = ON
            if (setting == null ||
                string.IsNullOrWhiteSpace(setting.Value))
            {
                return true;
            }

            return bool.TryParse(
                setting.Value,
                out bool enabled)
                ? enabled
                : true;
        }


        // =========================================================
        // SAVE CUSTOMERS ENABLE / DISABLE
        // =========================================================

        public async Task SetCustomersEnabledAsync(
            bool enabled)
        {
            await SaveSettingAsync(
                CustomersEnabledKey,
                enabled.ToString());
        }


        // =========================================================
        // GENERIC GET SETTING
        // =========================================================

        private async Task<string> GetSettingValueAsync(
            string key,
            string defaultValue)
        {
            var setting =
                await _repository.GetByKeyAsync(key);

            if (setting == null ||
                string.IsNullOrWhiteSpace(setting.Value))
            {
                return defaultValue;
            }

            return setting.Value;
        }


        // =========================================================
        // GENERIC SAVE SETTING
        // =========================================================

        private async Task SaveSettingAsync(
            string key,
            string value)
        {
            var setting =
                await _repository.GetByKeyAsync(key);

            if (setting == null)
            {
                await _repository.AddAsync(
                    new SystemSetting
                    {
                        Key = key,
                        Value = value
                    });
            }
            else
            {
                setting.Value = value;

                await _repository.UpdateAsync(setting);
            }
        }
    }
}