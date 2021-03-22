using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UPFMProduct;
using System;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Types
{
    public class UPFMIntegrationType : IIntegrationType
    {
        private readonly int _productId;

        private readonly DefaultUserClaim _userClaims;

        private ManageUPFMProductsIntegration _manageUPFMProductIntegration => new ManageUPFMProductsIntegration(_productId, _userClaims);

        private IUPFMProduct _upfmProductIntegration => new UPFMProductIntegration(_productId, _userClaims);

        public UPFMIntegrationType(int productId, DefaultUserClaim userClaims)
        {
            _productId = productId;
            _userClaims = userClaims;
        }

        public ListResponse GetRoles(long editorPersonaId, long userPersonaId, long partyId, AccessType? accessType, RequestParameter dataFilter) =>
            _manageUPFMProductIntegration.GetRoles(editorPersonaId, userPersonaId, _userClaims.OrganizationPartyId);

        public ListResponse GetProperties(long editorPersonaId, long userPersonaId, RequestParameter dataFilter) =>
            _manageUPFMProductIntegration.GetUPFMProperties(editorPersonaId, userPersonaId, false, dataFilter);

        public ListResponse GetRightsForRole(long editorPersonaId, int roleId, long partyId, bool assignedToRoleOnly, RequestParameter dataFilter) =>
            _manageUPFMProductIntegration.GetRightsByRole(editorPersonaId, partyId, roleId);

        public string CreateUser(ProductUserProperitiesRoles productUser)
        {
            var productPropertiesRoles = GetProductPropertiesRoles<UPFMProductPropertyRole>(productUser.InputJson);
            return _upfmProductIntegration.CreateUser(productUser.RealPageId, productUser.CreateUserPersonaId,
               productUser.AssignUserPersonaId, productPropertiesRoles);
        }

        private T GetProductPropertiesRoles<T>(string productUserInputJson)
        {
            if (string.IsNullOrEmpty(productUserInputJson))
                return default(T); //throw new Exception("productUserInputJson is null or empty");

            try
            {
                return JsonConvert.DeserializeObject<T>(productUserInputJson.Trim());
            }
            catch (Exception ex)
            {
                // if the parser fails return an empty object so the product call can catch the error
                return default(T);
            }
        }
    }
}