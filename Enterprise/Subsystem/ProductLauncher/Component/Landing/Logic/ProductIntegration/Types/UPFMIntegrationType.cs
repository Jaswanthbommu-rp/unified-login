using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Types
{
    public class UPFMIntegrationType : IIntegrationType
    {
        private readonly int _productId;

        private readonly DefaultUserClaim _userClaims;

        private ManageUPFMProductsIntegration _upfmProductIntegration => new ManageUPFMProductsIntegration(_productId, _userClaims);

        public UPFMIntegrationType(int productId, DefaultUserClaim userClaims)
        {
            _productId = productId;
            _userClaims = userClaims;
        }

        public ListResponse GetRoles(long editorPersonaId, long userPersonaId, long partyId, AccessType? accessType, RequestParameter dataFilter) =>
            _upfmProductIntegration.GetRoles(editorPersonaId, userPersonaId, _userClaims.OrganizationPartyId);

        public ListResponse GetProperties(long editorPersonaId, long userPersonaId, RequestParameter dataFilter) =>
            _upfmProductIntegration.GetUPFMProperties(editorPersonaId, userPersonaId, false, dataFilter);

        public ListResponse GetRightsForRole(long editorPersonaId, int roleId, long partyId, bool assignedToRoleOnly, RequestParameter dataFilter) =>
            _upfmProductIntegration.GetRightsByRole(editorPersonaId, partyId, roleId);
    }
}
