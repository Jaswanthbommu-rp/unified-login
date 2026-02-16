using System;
using UnifiedLogin.SharedObjects.Helper;

namespace UnifiedLogin.LandingAPIEnterprise.Helpers
{
    /// <summary>
    /// Helper class for building and validating product URLs
    /// </summary>
    public static class ProductUrlHelper
    {
        /// <summary>
        /// Builds a product URL, handling both absolute and relative URLs
        /// </summary>
        public static string BuildProductUrl(string productUrl)
        {
            if (string.IsNullOrEmpty(productUrl))
                return productUrl;

            // If already contains HTTP/HTTPS, use as-is
            if (productUrl.ToUpperInvariant().Contains("HTTP"))
                return productUrl;

            // Append to base landing URI
            return ConfigReader.GetLandingUri + productUrl;
        }

        /// <summary>
        /// Builds a product redirect URL with persona information
        /// </summary>
        public static string BuildProductRedirectUrl(int productId, long personaId)
        {
            return ConfigReader.GetLandingUri + $"product-redirect.html?prod={productId}&persona={personaId}";
        }

        /// <summary>
        /// Builds a product URL with relative fallback to redirect
        /// </summary>
        public static string BuildProductUrlWithFallback(string productUrl, int productId, long personaId)
        {
            if (!string.IsNullOrEmpty(productUrl) && productUrl.ToUpperInvariant().Contains("HTTP"))
                return productUrl;

            return BuildProductRedirectUrl(productId, personaId);
        }

        /// <summary>
        /// Validates that a URL is not empty
        /// </summary>
        public static bool IsValidProductUrl(string productUrl)
        {
            return !string.IsNullOrEmpty(productUrl);
        }
    }
}
