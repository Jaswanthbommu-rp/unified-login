using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig
{
    /// <summary>
    /// Used for the result UI
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class IdentityProviderTypeOutput
    {
        /// <summary>
        /// User Profile
        /// </summary>
        [JsonProperty(PropertyName = "data")]
        public IdentityProviderType identityProviderType { get; set; } 
    }
}


