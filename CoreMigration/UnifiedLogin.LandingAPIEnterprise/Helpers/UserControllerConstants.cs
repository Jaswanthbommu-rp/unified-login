using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.LandingAPIEnterprise.Helpers
{
    /// <summary>
    /// Constants used across User Controller operations
    /// </summary>
    public static class UserControllerConstants
    {
        // Super User Settings
        public const int MaxSuperUsersPerOrganization = 2;
        public const string SmbSettingName = "enablesmb";
        public const string SmbEnabledValue = "1";

        // Persona Names
        public const string PrimaryPersonaName = "Primary";
        public const string SystemAdministratorPersonaName = "System Administrator";

        // Settings
        public const string SecuritySettingCategory = "Security";
        public const string SessionTimeoutSettingName = "SessionTimeout";
        public const int DefaultSessionTimeout = 480;

        // Product Resources
        public const int ReportingProductId = 67;
        public const int SettingsProductId = 56;
        public const string ReportingPageId = "reporting";
        public const string ManageSettingsPageId = "manage-settings";

        // External Company
        public static readonly Guid ExternalCompanyId = SharedObjects.Landing.DefaultUserClaim.ExternalCompanyRealPageId;

        // Phone Number Defaults
        public const string DefaultCountryCode = "+1";
        public const string DefaultISOCode = "US";
        public const int PhoneContactMechanismUsageTypeId = 203;

        // Validation Messages
        public const string ErrorTitle = "Error";
        public const string ValidationErrorTitle = "Validation Error";
        public const string ErrorSource = "/user";

        // Persona Environment
        public const string ProductionEnvironment = "Production";
    }
}
