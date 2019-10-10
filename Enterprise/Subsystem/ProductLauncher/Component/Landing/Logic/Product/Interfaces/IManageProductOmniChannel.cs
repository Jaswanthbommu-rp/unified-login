using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces
{
    /// <summary>
    /// OmniChannel Interface
    /// </summary>
    public interface IManageProductOmniChannel
    {        
        /// <summary>
        /// Used to get properties  
        /// </summary> 
        ListResponse GetProperties(long editorPersonaId, long userPersonaId, RequestParameter datafilter);

        /// <summary>
        /// Returns Roles (Roles in GB)
        /// </summary>
        ListResponse GetRoles(long editorPersonaId, long userPersonaId, long partyId);

        /// <summary>
        /// Used to get properties  
        /// </summary> 
        ListResponse GetPropertiesByOrganization(long organizationId, long userPersonaId);
        
    }
}