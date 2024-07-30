using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces
{
    public interface IManageProductBatch
    {
        ListResponse GetProductRoles(long editorPersonaId, long userPersonaId, int productId, long partyId, DefaultUserClaim userClaim);
        List<string> GetPersonaRoleRights(long personaId, long orgPartyId);
        bool IsProductEnabledForUsePrimaryProperty(int productId);
        ListResponse GetEnterpriseRoleUserPrimaryPropertiesData(long editorPersonaId, long userPersonaId, int productId, bool usePrimaryProperties = true);
        ProductBatch GetProductBatchRecord(long editorUserPersonaId, long subjectUserPersonaId, IList<ProductRole> productRoles, ListResponse propertiesResponse, ListResponse rolesResponse, int product, bool usePrimaryProperties);
        List<int> GetExistingUserPrimaryPropertiesData(long userPersonaId, int productId);
    }
}
