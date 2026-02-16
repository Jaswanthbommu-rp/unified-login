using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.ResponseObject;

namespace UnifiedLogin.LandingAPIEnterprise.Services.Role
{
    /// <summary>
    /// Service for querying product roles and rights.
    /// Encapsulates business logic for role/right retrieval across products.
    /// </summary>
    public sealed class RoleQueryService : IRoleQueryService
    {
        private readonly DefaultUserClaim _userClaims;
        private readonly IManagePerson _managePerson;
        private readonly IManagePersona _managePersona;
        private readonly IManageUnifiedLogin _manageUnifiedLogin;
        private readonly IProductRepository _productRepository;
        private readonly IManageProductPanel _manageProductPanel;

        public RoleQueryService(
            DefaultUserClaim userClaims,
            IManagePerson managePerson,
            IManagePersona managePersona,
            IManageUnifiedLogin manageUnifiedLogin,
            IProductRepository productRepository,
            IManageProductPanel manageProductPanel)
        {
            _userClaims = userClaims ?? throw new ArgumentNullException(nameof(userClaims));
            _managePerson = managePerson ?? throw new ArgumentNullException(nameof(managePerson));
            _managePersona = managePersona ?? throw new ArgumentNullException(nameof(managePersona));
            _manageUnifiedLogin = manageUnifiedLogin ?? throw new ArgumentNullException(nameof(manageUnifiedLogin));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _manageProductPanel = manageProductPanel ?? throw new ArgumentNullException(nameof(manageProductPanel));
        }

        public ActionResultEnvelope GetUserProductRoles(Guid realPageId, string productCode)
        {
            var errorResponse = new ErrorResponse { Errors = new List<Error>() };
            var response = new PagedResponse { Meta = new Meta() };

            var person = _managePerson.GetPerson(realPageId);
            if (person == null)
            {
                return ActionResultEnvelope.NotFound();
            }

            var persona = _managePersona.GetFirstAvailablePersonaByCompany(realPageId, _userClaims.OrganizationPartyId);
            if (persona == null || persona.OrganizationPartyId != _userClaims.OrganizationPartyId)
            {
                return ActionResultEnvelope.NotFound();
            }

            ListResponse productResponse;
            var filteredList = new List<ProductRole>();

            if (productCode.Equals("UPFM", StringComparison.OrdinalIgnoreCase))
            {
                productResponse = _manageUnifiedLogin.GetUserRoles(_userClaims.PersonaId, persona.PersonaId, _userClaims.OrganizationPartyId);
                if (productResponse != null && !productResponse.IsError)
                {
                    filteredList = productResponse.Records
                        .Cast<ProductRole>()
                        .Where(p => p.IsAssigned)
                        .ToList();
                }
            }
            else
            {
                var productList = _productRepository.GetAllProducts();
                var productId = (int)ProductEnumHelper.GetProductIdByProductCode(productCode, productList);
                var productInternalSettings = _manageUnifiedLogin.GetProductInternalSettingByProductId(productId);

                // Check for sharedProductId setting and update productId if present
                var sharedProductSetting = productInternalSettings
                    .FirstOrDefault(a => a.Name.Equals(SettingConstants.SharedProductSettingName, StringComparison.OrdinalIgnoreCase))
                    ?.Value;

                if (sharedProductSetting != null && int.TryParse(sharedProductSetting, out var sharedProductId))
                {
                    productId = sharedProductId;
                }

                productResponse = _manageProductPanel.GetProductRoles(_userClaims.PersonaId, persona.PersonaId, _userClaims.OrganizationPartyId, productId, null, null);
                if (productResponse != null)
                {
                    filteredList = productResponse.Records
                        .Cast<ProductRole>()
                        .Where(p => p.IsAssigned)
                        .ToList();
                }
            }

            if (productResponse != null && !productResponse.IsError)
            {
                response.Data = filteredList.Cast<object>().ToList();
                response.Meta.CurrentPage = 1;
                response.Meta.TotalRows = filteredList.Count;
                response.Meta.RowsPerPage = filteredList.Count;
                return ActionResultEnvelope.Ok(response);
            }

            errorResponse.Errors.Add(new Error
            {
                Title = "Error",
                Detail = productResponse?.ErrorReason,
                Source = "/role",
                StatusCode = ""
            });
            return ActionResultEnvelope.BadRequest(errorResponse);
        }

        public ActionResultEnvelope GetProductRoles(string productCode)
        {
            var errorResponse = new ErrorResponse { Errors = new List<Error>() };

            ListResponse productResponse;

            if (productCode.Equals("UPFM", StringComparison.OrdinalIgnoreCase))
            {
                productResponse = _manageUnifiedLogin.GetRoles(_userClaims.PersonaId, _userClaims.OrganizationPartyId);
            }
            else
            {
                var productList = _productRepository.GetAllProducts();
                var productId = (int)ProductEnumHelper.GetProductIdByProductCode(productCode, productList);
                productResponse = _manageProductPanel.GetProductRoles(_userClaims.PersonaId, _userClaims.PersonaId, _userClaims.OrganizationPartyId, productId, null, null);
            }

            var response = new PagedResponse { Meta = new Meta() };

            if (!productResponse.IsError)
            {
                response.Data = productResponse.Records;
                response.Meta.CurrentPage = 1;
                response.Meta.TotalRows = productResponse.TotalRows;
                response.Meta.RowsPerPage = productResponse.TotalRows;
                return ActionResultEnvelope.Ok(response);
            }

            errorResponse.Errors.Add(new Error
            {
                Title = "Error",
                Detail = productResponse.ErrorReason,
                Source = "/role",
                StatusCode = ""
            });
            return ActionResultEnvelope.BadRequest(errorResponse);
        }

        public ActionResultEnvelope GetRightsForRole(string productCode, int roleId)
        {
            if (string.IsNullOrEmpty(productCode))
            {
                return ActionResultEnvelope.BadRequest("ProductCode not supplied.");
            }

            if (roleId == 0)
            {
                return ActionResultEnvelope.BadRequest("roleId not supplied.");
            }

            var result = _manageUnifiedLogin.GetListRightbyRole(productCode, roleId);
            return ActionResultEnvelope.Ok(result);
        }
    }
}
