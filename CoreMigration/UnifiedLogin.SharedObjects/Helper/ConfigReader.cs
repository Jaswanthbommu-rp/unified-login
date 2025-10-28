using System.Collections.Specialized;
using System.Configuration;

namespace UnifiedLogin.SharedObjects.Helper
{

    /// <summary>
    /// Helper class to get configuration from web.config
    /// </summary>
    public static class ConfigReader
    {
        private static readonly NameValueCollection ConfigSection;

        #region Ctor

        static ConfigReader()
        {
            ConfigSection = ConfigurationManager.GetSection("IdentityConfig") as NameValueCollection;
        }

        #endregion

        #region Common Configurations
        /// <summary>
        /// Get the environment of the system
        /// </summary>
        public static string Environment { get; } = ConfigurationManager.AppSettings["logging:environment"];
        
        /// <summary>
        /// Get API Secret
        /// </summary>
        public static string GetApiSecret => ConfigSection["ApiSecret"];
        
        /// <summary>
        /// Used to get the url for the Identity Server
        /// </summary>
        public static string GetPublicOriginUri => ConfigSection["PublicOriginUri"];
        
        /// <summary>
        /// Identity server URI
        /// </summary>
        public static string GetIssuerUri => ConfigSection["IssuerUri"];

        /// <summary>
        /// Get comma seperated or single scope
        /// </summary>
        public static string GetRequiredScope => ConfigSection["RequiredScope"];

        /// <summary>
        /// Get the uri for the API server
        /// </summary>
        public static string GetApiUri => ConfigSection["ApiUri"];

        /// <summary>
        /// Get the uri for the Unified Setting API
        /// </summary>
        public static string GetUnifiedSettingsApiUri => ConfigSection["UnifiedSettingsApiUri"];

        /// <summary>
        /// Get the uri for redirecting for the application
        /// </summary>
        public static string GetRedirectUri => ConfigSection["RedirectUri"];

        /// <summary>
        /// Get the Landing web client name
        /// </summary>
        public static string GetLandingClientId => ConfigSection["LandingClientId"];

        /// <summary>
        /// Get the Landing web client scopes
        /// </summary>
        public static string GetLandingScopes => ConfigSection["LandingScopes"];

        /// <summary>
        /// Get the Identity server signing certificate thumbprint
        /// </summary>
        public static string GetIdentityServerSigningCertThumbprint => ConfigSection["IdentityServerSigningCertThumbprint"];

        /// <summary>
        /// Get the return url for the Landing website
        /// </summary>
        public static string GetReturnUri => ConfigSection["ReturnUri"];

        /// <summary>
        /// Get the path for the Identity server log file
        /// </summary>
        public static string GetLogFilePath => ConfigSection["LogFilePath"];

        /// <summary>
        /// Get the url for the Identity API server
        /// </summary>
        public static string GetIdentityApiUri => ConfigSection["IdentityAPIUri"];

		/// <summary>
		/// Get the url for the Landing API server
		/// </summary>
		public static string GetLandingAPIUri => ConfigSection["LandingAPIUri"];
		
        /// <summary>
        /// Get the CSP Option for the allowed ScriptSrc values
        /// </summary>
        public static string GetCspOptions_ScriptSrcAdditional => ConfigSection["CspOptions.ScriptSrcAdditional"];

        /// <summary>
        /// Get the CSP Option for the allowed StyleSrc values
        /// </summary>
        public static string GetCspOptions_StyleSrcAdditional => ConfigSection["CspOptions.StyleSrcAdditional"];

        /// <summary>
        /// Get the CSP Option for the allowed FontSrc values
        /// </summary>
        public static string GetCspOptions_FontSrcAdditional => ConfigSection["CspOptions.FontSrcAdditional"];

        /// <summary>
        /// Get the CSP Option for the allowed FontSrc values
        /// </summary>
        public static string GetCspOptions_ImageSrcAdditional => ConfigSection["CspOptions.ImageSrcAdditional"];

       /// <summary>
        /// Get the CSP Option for the allowed ConnectSrc values
        /// </summary>
        public static string GetCspOptions_ConnectSrcAdditional => ConfigSection["CspOptions.ConnectSrcAdditional"];

        /// <summary>
        /// Get the CSP Option for the allowed FrameSrc values
        /// </summary>
        public static string GetCspOptions_FrameSrcAdditional => ConfigSection["CspOptions.FrameSrcAdditional"];

		/// <summary>
		/// Landing URI
		/// </summary>
		public static string GetLandingUri => ConfigSection["LandingUri"];

        /// <summary>
		/// Images URI
		/// </summary>
		public static string GetImagesUri => ConfigSection["ImagesUri"];

        /// <summary>
		/// Document URI
		/// </summary>
		public static string GetDocumentUri => ConfigSection["DocumentUri"];

        /// <summary>
        /// CES URL
        /// </summary>
        public static string GetCESURL => ConfigSection["CESURL"];

        /// <summary>
        /// After beta this will be in database under product settings
        /// </summary>
        public static string GetVendorServicesIssueUri => ConfigSection["VendorServicesIssueUri"];

		/// <summary>
		/// Used to store the RealPage company master company id
		/// </summary>
	    public static string OrgMasterId => ConfigSection["OrgMasterId"];

        /// <summary>
        /// Used to store the MQ Name
        /// </summary>
        public static string GetActivityMQName { get; } = ConfigurationManager.AppSettings["ActivityMQName"];
        
        public static string GetLaunchdarklySdkKey { get; } = ConfigurationManager.AppSettings["launchdarkly:SdkKey"];

        #endregion
    }
}