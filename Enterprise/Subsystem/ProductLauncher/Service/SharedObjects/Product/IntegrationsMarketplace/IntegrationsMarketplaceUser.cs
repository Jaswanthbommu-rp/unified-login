using Newtonsoft.Json;
using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.IntegrationMarketplace
{
    
    /// <summary>
	/// Used to store information about Integration MarketplaceUser for a user
	/// </summary>
	public class IntegrationMarketplaceUser
    {
    }

    /// <summary>
    /// Object to map with Input Json from UI
    /// </summary>
    public class UserAssignProductPropertyRole
    {
        /// <summary>
        /// A role to assign to the user
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> RoleList { get; set; }


        /// <summary>
        /// Is product assigned or removed
        /// </summary>
        public bool IsAssigned { get; set; }
    }
}
