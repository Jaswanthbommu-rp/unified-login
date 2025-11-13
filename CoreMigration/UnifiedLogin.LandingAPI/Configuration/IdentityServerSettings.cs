namespace UnifiedLogin.LandingAPI.Configuration
{
    /// <summary>
    /// Configuration settings for IdentityServer authentication
    /// </summary>
    public class IdentityServerSettings
    {
        /// <summary>
        /// The authority URL for the IdentityServer instance
        /// </summary>
        public string Authority { get; set; } = string.Empty;

        /// <summary>
        /// The issuer URI for token validation
        /// </summary>
        public string IssuerUri { get; set; } = string.Empty;

        /// <summary>
        /// Public origin URI for the IdentityServer
        /// </summary>
        public string PublicOriginUri { get; set; } = string.Empty;

        /// <summary>
        /// API URI for this service
        /// </summary>
        public string ApiUri { get; set; } = string.Empty;

        /// <summary>
        /// Space-separated list of required scopes
        /// </summary>
        public string RequiredScopes { get; set; } = string.Empty;

        /// <summary>
        /// Whether to validate the token issuer
        /// </summary>
        public bool ValidateIssuer { get; set; } = true;

        /// <summary>
        /// Whether to validate the token audience
        /// </summary>
        public bool ValidateAudience { get; set; } = true;

        /// <summary>
        /// Whether HTTPS metadata is required
        /// </summary>
        public bool RequireHttpsMetadata { get; set; } = true;

        /// <summary>
        /// Whether to delay loading metadata
        /// </summary>
        public bool DelayLoadMetadata { get; set; } = true;

        /// <summary>
        /// Whether to enable validation result cache
        /// </summary>
        public bool EnableValidationResultCache { get; set; } = true;

        /// <summary>
        /// Whether to save the access token
        /// </summary>
        public bool SaveToken { get; set; } = true;

        /// <summary>
        /// API secret for client credentials
        /// </summary>
        public string ApiSecret { get; set; } = string.Empty;

        /// <summary>
        /// Thumbprint of the IdentityServer signing certificate
        /// </summary>
        public string IdentityServerSigningCertThumbprint { get; set; } = string.Empty;

        /// <summary>
        /// Client ID for the Landing application
        /// </summary>
        public string LandingClientId { get; set; } = string.Empty;

        /// <summary>
        /// Scopes for the Landing client
        /// </summary>
        public string LandingScopes { get; set; } = string.Empty;

        /// <summary>
        /// Redirect URI after authentication
        /// </summary>
        public string RedirectUri { get; set; } = string.Empty;

        /// <summary>
        /// Return URI for the application
        /// </summary>
        public string ReturnUri { get; set; } = string.Empty;

        /// <summary>
        /// Identity API URI
        /// </summary>
        public string IdentityAPIUri { get; set; } = string.Empty;

        /// <summary>
        /// Landing API URI
        /// </summary>
        public string LandingAPIUri { get; set; } = string.Empty;

        /// <summary>
        /// Landing web URI
        /// </summary>
        public string LandingUri { get; set; } = string.Empty;

        /// <summary>
        /// Unified Settings API URI
        /// </summary>
        public string UnifiedSettingsApiUri { get; set; } = string.Empty;

        /// <summary>
        /// Images URI
        /// </summary>
        public string ImagesUri { get; set; } = string.Empty;

        /// <summary>
        /// Document URI
        /// </summary>
        public string DocumentUri { get; set; } = string.Empty;

        /// <summary>
        /// CES URL
        /// </summary>
        public string CESURL { get; set; } = string.Empty;

        /// <summary>
        /// Vendor Services Issue URI
        /// </summary>
        public string VendorServicesIssueUri { get; set; } = string.Empty;

        /// <summary>
        /// Organization Master ID
        /// </summary>
        public string OrgMasterId { get; set; } = string.Empty;

        /// <summary>
        /// Log file path
        /// </summary>
        public string LogFilePath { get; set; } = string.Empty;
    }
}
