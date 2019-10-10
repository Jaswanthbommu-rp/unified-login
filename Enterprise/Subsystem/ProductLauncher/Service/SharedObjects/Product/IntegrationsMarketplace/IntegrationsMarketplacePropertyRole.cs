using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.IntegrationMarketplace
{
    public class IntegrationMarketplacePropertyRole
    {
        /// <summary>
        /// Is product assigned or removed
        /// </summary>
        public bool IsAssigned { get; set; } = true;

        /// <summary>
        /// Role assigned to the user
        /// </summary>
        public List<string> RoleList { get; set; }
    }
}
