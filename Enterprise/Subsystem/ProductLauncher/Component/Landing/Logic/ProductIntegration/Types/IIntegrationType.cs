using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Types
{
    public interface IIntegrationType
    {
        ListResponse GetProperties(long editorPersonaId, long userPersonaId, RequestParameter dataFilter);

        ListResponse GetRoles(long editorPersonaId, long userPersonaId, long partyId, AccessType? accessType, RequestParameter dataFilter);

        ListResponse GetRightsForRole(long editorPersonaId, int roleId, long partyId, bool assignedToRoleOnly, RequestParameter dataFilter);

        string CreateUser(ProductUserProperitiesRoles productUser);

        string ChangeUserType(ProductUserProperitiesRoles batchRecord);
    }
}