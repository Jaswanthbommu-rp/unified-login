using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.IdentityConfig
{
    /// <summary>
    /// Used for the result UI
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class IdentityProviderConfigurationListOutput
    {
        /// <summary>
        /// User Profile
        /// </summary>
        [JsonProperty(PropertyName = "data")]
        public IList<ProviderConfiguration> providerConfigurationList { get; set; }
    }
}