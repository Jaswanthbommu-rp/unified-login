using System.Collections.Generic;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Product.IntegrationsMarketplace
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
