using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Types
{
    public class StandardV1IntegrationType : IIntegrationType
    {
        public ListResponse GetProperties(long editorPersonaId, long userPersonaId, RequestParameter dataFilter)
        {
            throw new System.NotImplementedException();
        }

        public ListResponse GetRightsForRole(long editorPersonaId, int roleId, long partyId, bool assignedToRoleOnly, RequestParameter dataFilter)
        {
            throw new System.NotImplementedException();
        }

        public ListResponse GetRoles(long editorPersonaId, long userPersonaId, long partyId, AccessType? accessType, RequestParameter dataFilter)
        {
            throw new System.NotImplementedException();
        }
    }
}
