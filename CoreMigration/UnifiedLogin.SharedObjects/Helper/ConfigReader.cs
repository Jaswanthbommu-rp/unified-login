using Microsoft.Extensions.Configuration;
using System;

namespace UnifiedLogin.SharedObjects.Helper
{
    /// <summary>
    /// Helper class to get configuration from appsettings.json
    /// Must be initialized with IConfiguration before use by calling Initialize() method
    /// </summary>
    public static class ConfigReader
    {
        private static IConfiguration _configuration;

        /// <summary>
        /// Initialize the ConfigReader with IConfiguration
        /// This should be called once during application startup (e.g., in Program.cs)
        /// </summary>
        /// <param name="configuration">The IConfiguration instance from dependency injection</param>
        public static void Initialize(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Helper method to safely read configuration values
        /// </summary>
        private static string GetConfigValue(string key)
        {
            if (_configuration == null)
            {
                throw new InvalidOperationException(
                    "ConfigReader has not been initialized. Call ConfigReader.Initialize(IConfiguration) in Program.cs before use.");
            }
            return _configuration[key];
        }

        /// <summary>
        /// Get the environment of the system
        /// </summary>
        public static string Environment => GetConfigValue("logging:environment");

        /// <summary>
        /// Identity server URI
        /// </summary>
        public static string GetIssuerUri => GetConfigValue("UnifiedPlatform:Authority");

        /// <summary>
        /// Landing URI
        /// </summary>
        public static string GetLandingUri => GetConfigValue("UnifiedPlatform:LandingUri");

        /// <summary>
        /// Landing URI
        /// </summary>
        public static string RedirectUri => GetConfigValue("UnifiedPlatform:RedirectUri");

        /// <summary>
        /// Get the Images URL for the Landing website
        /// </summary>
        public static string GetImagesUri => GetConfigValue("UnifiedPlatform:ImagesUri");

        /// <summary>
        /// Document URI
        /// </summary>
        public static string GetDocumentUri => GetConfigValue("UnifiedPlatform:DocumentUri");

        /// <summary>
        /// CES URL
        /// </summary>
        public static string GetCESURL => GetConfigValue("UnifiedPlatform:CESURL");

        /// <summary>
        /// Used to store the RealPage company master company id
        /// </summary>
        public static string OrgMasterId => GetConfigValue("UnifiedPlatform:OrgMasterId");

        /// <summary>
        /// LaunchDarkly SDK Key
        /// </summary>
        public static string GetLaunchdarklySdkKey => GetConfigValue("launchdarkly:SdkKey");

        /// <summary>
        /// Get comma seperated or single scope
        /// </summary>
        public static string GetRequiredScope => GetConfigValue("UnifiedPlatform:ApiName");
    }
}