using System.Collections.Specialized;
using System.Configuration;

namespace RP.Enterprise.Foundation.Activity.Service.Logging.Reader.Helper
{
    public static class ConfigReader
    {
        private static readonly NameValueCollection ConfigSection;

        #region Ctor

        static ConfigReader()
        {
            ConfigSection = ConfigurationManager.GetSection("IdentityConfig") as NameValueCollection;
        }

        #endregion

        /// <summary>
        /// Identity server URI
        /// </summary>
        public static string GetIssuerUri => ConfigSection["IssuerUri"];

        /// <summary>
        /// Get comma separated or single scope
        /// </summary>
        public static string GetRequiredScope => ConfigSection["RequiredScope"];

        /// <summary>
        /// Get API Secret
        /// </summary>
        public static string GetApiSecret => ConfigSection["ApiSecret"];

        /// <summary>
        /// Get the environment of the system
        /// </summary>
        public static string Environment { get; } = ConfigurationManager.AppSettings["logging:environment"];

        /// <summary>
        /// Get Client Id  
        /// </summary>
        public static string GetClientId => ConfigSection["ClientId"];

        /// <summary>
        /// ActivityMQName
        /// </summary>
        public static string ActivityMQName { get; } = ConfigurationManager.AppSettings["ActivityMQName"];

        /// <summary>
        /// Get the Identity server signing certificate thumbprint
        /// </summary>
        public static string GetIdentityServerSigningCertThumbprint => ConfigSection["IdentityServerSigningCertThumbprint"];
    }
}