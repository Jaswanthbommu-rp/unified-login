using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Logic.Product.Interfaces
{
    public interface IManageResearchApplication
    {

        /// <summary>
        /// Returns Roles (Roles in GB)
        /// </summary>
        ListResponse GetRoles(long editorPersonaId, long userPersonaId, long partyId);
        ListResponse GetRightsByRole(long editorPersonaId, long partyId, long roleId);
    }
}
