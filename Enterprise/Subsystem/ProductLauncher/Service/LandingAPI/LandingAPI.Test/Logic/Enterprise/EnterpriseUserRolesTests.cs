using System;
using FluentAssertions;
using Moq;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.OneSite;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.ResponseObject;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using Xunit;
using RoleController = RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPIEnterprise.Controllers.RoleController;
using Newtonsoft.Json;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic.Enterprise
{
    [ExcludeFromCodeCoverage]
    public class EnterpriseUserRolesTests : EnterpriseBase
    {
        private ListResponse _vendorMarketplaceMultifamilyRoles;
        private ListResponse _vendorMarketplaceVendorRoles;

        public EnterpriseUserRolesTests()
        {
            var vendorMFRoleList = new List<ProductRole>()
            {
                new ProductRole()
                {
                    ID = "101",
                    Name = "Standard User",
                    Alias = "StandardUser",
                    Roletype = "Custom",
                },
                new ProductRole()
                {
                    ID = "102",
                    Name = "Read-Only User",
                    Alias = "ReadOnlyUser",
                    Roletype = "Custom",
                },
                new ProductRole()
                {
                    ID = "201",
                    Name = "Administrator",
                    Alias = "Administrator",
                    Roletype = "Custom",
                },
            };

            var vendorVendorRoleList = new List<ProductRole>()
            {
                new ProductRole()
                {
                    ID = "5674",
                    Name = "Credentialing Administrator",
                    Alias = "CredentialingAdministrator",
                    Roletype = "System",
                    IsAssigned = true
                },
                new ProductRole()
                {
                    ID = "5675",
                    Name = "Credentialing Read Only",
                    Alias = "CredentialingReadOnly",
                    Roletype = "System",
                },
                new ProductRole()
                {
                    ID = "5684",
                    Name = "Merchant Company Profile/Payment Account",
                    Alias = "MerchantCompanyProfilePaymentAccount",
                    Roletype = "System",
                    IsAssigned = true
                },
                new ProductRole()
                {
                    ID = "5688",
                    Name = "VMP Administrator",
                    Alias = "VMPAdministrator",
                    Roletype = "System",
                },
            };
            _vendorMarketplaceMultifamilyRoles = new ListResponse()
            {
                Records = vendorMFRoleList.Cast<object>().ToList(),
                CurrentPage = 1,
                TotalRows = vendorMFRoleList.Count,
                RowsPerPage = 999
            };

            _vendorMarketplaceVendorRoles = new ListResponse()
            {
                Records = vendorVendorRoleList.Cast<object>().ToList(),
                CurrentPage = 1,
                TotalRows = vendorVendorRoleList.Count,
                RowsPerPage = 999
            };
        }

        private RoleController MakeInstance(
            DefaultUserClaim userClaim = null,
            Mock<IRepository> repository = null,
            Mock<HttpMessageHandler> httpMessageHandler = null,
            Mock<IOneSiteProductService> onesiteProductService = null,
            Mock<IManageProductPanel> manageProductPanel = null,
            ClaimsIdentity claimsIdentity = null)
        {
            var _defaultUserClaim = new DefaultUserClaim
            {
                CorrelationId = new Guid(),
                CustomerMasterId = _BooksCompanyMasterId,
                OrganizationPartyId = 1234,
                PersonaId = _vendorUserPersonaId,
                UserRealPageGuid = _vendorUserRealpageId
            };

            repository = repository ?? new Mock<IRepository>();
            httpMessageHandler = httpMessageHandler ?? new Mock<HttpMessageHandler>();
            onesiteProductService = onesiteProductService ?? new Mock<IOneSiteProductService>();
            manageProductPanel = manageProductPanel ?? new Mock<IManageProductPanel>();
            claimsIdentity = claimsIdentity ?? new ClaimsIdentity();
            userClaim = userClaim ?? _defaultUserClaim;

            var mockUnitofWork = new Mock<IUnitOfWork>();

            repository
                .Setup(m => m.UnitOfWork)
                .Returns(mockUnitofWork.Object);

            repository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct,
                    It.IsAny<object>()))
                .Returns(_gbProductMap);

            repository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                .Returns(_productInternalSettings);

            repository.Setup(m => m.GetMany<OrganizationType>(StoredProcNameConstants.SP_ListOrganizationType, null))
                .Returns(_organizationTypes);

            repository.Setup(m => m.GetMany<OrganizationDomain>(StoredProcNameConstants.SP_ListOrganizationDomain, null))
                .Returns(_organizationDomains);

            repository
                .Setup(m => m.GetMany<ProductSettingType>(StoredProcNameConstants.SP_ListProductSettingType, null))
                .Returns(() => _productSettingTypes);

            repository
                .Setup(m => m.GetMany<ProductInternalSettingByType>(StoredProcNameConstants.SP_ListProductGlobalSettingsBySettingType,
                    It.Is<object>(d => TestSqlParameterContains(d, "ProductIntegrationType"))
                ))
                .Returns(() => _productInternalSettingByType);

            repository
                .Setup(m => m.GetMany<dynamic>(StoredProcNameConstants.SP_ListOrganizations,
                    It.Is<object>(
                        d => TestSqlParameterContains(d, _multifamilyCompanyRealPageId.ToString()))))
                .Returns(_companyListApiResult);

            repository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization,
                    It.Is<object>(
                        d => TestSqlParameter(d, "{ RealPageId = , PartyId = " + _vendorCompanyPartyId + " }"))))
                .Returns(_organizationList.First(x => x.PartyId == _vendorCompanyPartyId));

            repository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization,
                    It.Is<object>(
                        d => TestSqlParameter(d, "{ RealPageId = , PartyId = " + _multifamilyCompanyPartyId + " }"))))
                .Returns(_organizationList.First(x => x.PartyId == _multifamilyCompanyPartyId));

            repository
                .Setup(m => m.GetMany<Organization>(StoredProcNameConstants.SP_ListOrganizationByRealPageId,
                    It.Is<object>(
                        d => TestSqlParameter(d, "{ RealPageId = " + _vendorUserRealpageId + ", RelationshipTypeName =  }"))))
                .Returns(_organizationList);

            repository
                .Setup(m => m.GetMany<Organization>(StoredProcNameConstants.SP_ListOrganizationByRealPageId,
                    It.Is<object>(
                        d => TestSqlParameter(d, "{ RealPageId = " + _multifamilyUserRealpageId + ", RelationshipTypeName =  }"))))
                .Returns(_organizationList);

            repository.Setup(m => m.GetOne<Person>(StoredProcNameConstants.SP_GetPerson, It.Is<object>(d => TestSqlParameterContains(d, _companySupportToolAdminUserLogin.RealPageId.ToString()))))
                .Returns(_companySupportToolAdminPerson);

            repository.Setup(m => m.GetOne<Person>(StoredProcNameConstants.SP_GetPerson, It.Is<object>(d => TestSqlParameterContains(d, _vendorUserRealpageId.ToString()))))
                .Returns(_vendorUserPerson);

            repository.Setup(m => m.GetOne<Person>(StoredProcNameConstants.SP_GetPerson, It.Is<object>(d => TestSqlParameterContains(d, _multifamilyUserRealpageId.ToString()))))
                .Returns(_multifamilyUserPerson);

            manageProductPanel
                .Setup(m => m.GetProductRoles(
                    It.Is<long>(l => l == _defaultUserClaim.PersonaId),
                    It.Is<long>(l => l == _defaultUserClaim.PersonaId),
                    It.Is<long>(g => g == _defaultUserClaim.OrganizationPartyId),
                    It.Is<int>(x => x == 38),
                    null, null))
                .Returns(() => _vendorMarketplaceMultifamilyRoles);

            manageProductPanel
                .Setup(m => m.GetProductRoles(
                    It.Is<long>(l => l == _defaultUserClaim.PersonaId),
                    It.Is<long>(l => l == _defaultUserClaim.PersonaId),
                    It.Is<long>(g => g == _defaultUserClaim.OrganizationPartyId),
                    It.Is<int>(x => x == 1),
                    null, null))
                .Returns(() => new ListResponse() { IsError = true, ErrorReason = "Product error" });

            manageProductPanel
                .Setup(m => m.GetProductRoles(
                    It.Is<long>(l => l == _companySupportToolAdminPersonaId),
                    It.Is<long>(l => l == _companySupportToolAdminPersonaId),
                    It.Is<long>(g => g == _vendorCompanyPartyId),
                    It.Is<int>(x => x == 38),
                    null, null))
                .Returns(() => _vendorMarketplaceVendorRoles);

            manageProductPanel
                .Setup(m => m.GetProductRoles(
                    It.Is<long>(l => l == _vendorUserPersonaId),
                    It.Is<long>(l => l == _vendorUserPersonaId),
                    It.Is<long>(g => g == _vendorCompanyPartyId),
                    It.Is<int>(x => x == 38),
                    null, null))
                .Returns(() => _vendorMarketplaceVendorRoles);

            manageProductPanel
                .Setup(m => m.GetProductRoles(
                    It.Is<long>(l => l == _companySupportToolAdminPersonaId),
                    It.Is<long>(l => l == _vendorUserPersonaId),
                    It.Is<long>(g => g == _vendorCompanyPartyId),
                    It.Is<int>(x => x == 38),
                    null, null))
                .Returns(() => _vendorMarketplaceVendorRoles);

            repository.Setup(m => m.GetOne<UserLoginOnly>(StoredProcNameConstants.SP_GetUserLoginOnly, It.Is<object>(d => TestSqlParameterContains(d, _companySupportToolAdminUserRealPageId.ToString()))))
                .Returns(_companySupportToolAdminuserLoginOnly);

            repository
                .Setup(m => m.GetOne<long>(StoredProcNameConstants.SP_GetActivePersona,
                    It.Is<object>(d => TestSqlParameter(d, "{ RealPageId = " + _companySupportToolAdminUserRealPageId + " }"))
                ))
                .Returns(_companySupportToolAdminPersonaId);

            repository
                .Setup(m => m.GetOne<long>(StoredProcNameConstants.SP_GetActivePersona,
                    It.Is<object>(d => TestSqlParameter(d, "{ RealPageId = " + _vendorUserRealpageId + " }"))
                ))
                .Returns(_vendorUserPersonaId);

            repository.Setup(m => m.GetOne<Persona>(StoredProcNameConstants.SP_GetPersona, It.Is<object>(d => TestSqlParameter(d, "{ personaid = " + _companySupportToolAdminPersonaId + " }"))))
                .Returns(_companySupportToolAdminPersona);

            repository.Setup(m => m.GetOne<Persona>(StoredProcNameConstants.SP_GetPersona, It.Is<object>(d => TestSqlParameter(d, "{ personaid = " + _vendorUserPersonaId + " }"))))
                .Returns(_vendorUserPersona);

            repository.Setup(m => m.GetOne<Persona>(StoredProcNameConstants.SP_GetPersona, It.Is<object>(d => TestSqlParameter(d, "{ personaid = " + _multifamilyUserPersonaId + " }"))))
                .Returns(_multifamilyUserPersona);

            repository.Setup(m => m.GetMany<Persona>(StoredProcNameConstants.SP_ListPersona, It.Is<object>(
                    d => TestSqlParameter(d, "{ RealPageId = " + _vendorUserRealpageId + " }"))))
                .Returns(new List<Persona>() { _vendorUserPersona});

            repository.Setup(m => m.GetMany<Persona>(StoredProcNameConstants.SP_ListPersona, It.Is<object>(
                    d => TestSqlParameter(d, "{ RealPageId = " + _multifamilyUserRealpageId + " }"))))
                .Returns(new List<Persona>() { _multifamilyUserPersona });

            repository
                .Setup(m => m.GetMany<ProductUI>(StoredProcNameConstants.SP_ListProductsByOrganization, It.Is<object>(
                    d => TestSqlParameter(d, "{ PartyId = , OrganizationRealPageId = " + _multifamilyCompanyRealPageId + " }"))))
                .Returns(_productUiList);

            repository
                .Setup(m => m.GetMany<ProductUI>(StoredProcNameConstants.SP_ListProductsByOrganization, It.Is<object>(
                    d => TestSqlParameter(d, "{ PartyId = " + _multifamilyCompanyPartyId + ", OrganizationRealPageId =  }"))))
                .Returns(_productUiList);

            var upfmRoleListText = JsonConvert.SerializeObject(_productExampleRoleList);
            var upfmRoleListDynamic = JsonConvert.DeserializeObject<List<dynamic>>(upfmRoleListText);

            repository
                .Setup(m => m.GetMany<dynamic>(StoredProcNameConstants.SP_ListRolesForProductsByPartyId, It.Is<object>(
                    d => TestSqlParameter(d, "{ PartyId = " + _multifamilyCompanyPartyId + ", ProductId = 3, TargetProductId = Dapper.TableValuedParameter }"))))
                .Returns(upfmRoleListDynamic);

            var upfmUserRoleListText = JsonConvert.SerializeObject(_productExampleUserRoleList);
            var upfmUserRoleListDynamic = JsonConvert.DeserializeObject<List<dynamic>>(upfmUserRoleListText);

            repository
                .Setup(m => m.GetMany<dynamic>(StoredProcNameConstants.SP_ListRolesForProductsByPersonaId, It.Is<object>(
                    d => TestSqlParameter(d, "{ ProductId = 3, PersonaID = " + _multifamilyUserPersonaId + ", PartyId = " + _multifamilyCompanyPartyId + " }"))))
                .Returns(upfmUserRoleListDynamic);

            var vmpRoleRightListText = JsonConvert.SerializeObject(_productExampleRoleRights);
            var vmpRoleRightListDynamic = JsonConvert.DeserializeObject<List<dynamic>>(vmpRoleRightListText);

            repository
                .Setup(m => m.GetMany<dynamic>(StoredProcNameConstants.SP_ListRolesAssociatedWithRights, It.Is<object>(
                    d => TestSqlParameter(d, "{ PartyId = " + _multifamilyUserPersonaId + ", ProductId = 38, RoleId = 1, TargetProductId = Dapper.TableValuedParameter }"))))
                .Returns(vmpRoleRightListDynamic);

            var vmpRoleRightImpersonationText = JsonConvert.SerializeObject(_productExampleImpersonationRoleRights);
            var vmpRoleRightImpersonationDynamic = JsonConvert.DeserializeObject<List<dynamic>>(vmpRoleRightImpersonationText);
            
            repository
                .Setup(m => m.GetMany<dynamic>(StoredProcNameConstants.SP_ListRolesAssociatedWithRights, It.Is<object>(
                    d => TestSqlParameter(d, "{ PartyId = " + _vendorCompanyPartyId + ", ProductId = 38, RoleId = 1, TargetProductId = Dapper.TableValuedParameter }"))))
                .Returns(vmpRoleRightImpersonationDynamic);
            
            return new RoleController(
                    repository.Object,
                    httpMessageHandler.Object,
                    userClaim,
                    onesiteProductService.Object,
                    manageProductPanel.Object,
                    claimsIdentity
                )
                { Request = new HttpRequestMessage(), Configuration = new HttpConfiguration() };
        }


        #region GetProductRoles

        [Fact]
        public void GetProductRoles_VendorMarketplace_Success()
        {
            // Arrange
            var roleController = MakeInstance();

            // Act
            var result = roleController.GetProductRoles("VMP");
            
            // Assert
            var content = result.Content.ReadAsAsync<PagedResponse>().Result;

            result.StatusCode.ToString().Should().Be(HttpStatusCode.OK.ToString());
            content.Data.Count.Should().Be(3);
        }

        [Fact]
        public void GetProductRoles_UPFM_Success()
        {
            // Arrange
            var userClaim = new DefaultUserClaim
            {
                CorrelationId = new Guid(),
                CustomerMasterId = _BooksCompanyMasterId,
                OrganizationPartyId = _multifamilyCompanyPartyId,
                PersonaId = _multifamilyUserPersonaId,
                UserRealPageGuid = _multifamilyUserRealpageId,
                OrganizationRealPageGuid = _multifamilyCompanyRealPageId,
            };
            var roleController = MakeInstance(userClaim: userClaim);

            // Act
            var result = roleController.GetProductRoles("UPFM");

            // Assert
            var content = result.Content.ReadAsAsync<PagedResponse>().Result;

            result.StatusCode.ToString().Should().Be(HttpStatusCode.OK.ToString());
            content.Data.Count.Should().Be(3);
            var role3 = (ProductRole)content.Data[2];
            role3.Name.Should().Be("Platform Administrator");
        }

        [Fact]
        public void GetProductRoles_VendorMarketplace_ProductError()
        {
            // Arrange
            var roleController = MakeInstance();

            // Act
            var result = roleController.GetProductRoles("OS");
            
            // Assert
            var content = result.Content.ReadAsAsync<ErrorResponse>().Result;

            result.StatusCode.ToString().Should().Be(HttpStatusCode.BadRequest.ToString());
            content.Errors.First().Detail.Should().Be("Product error");
            content.Errors.First().Source.Should().Be("/role");
        }

        [Fact]
        public void GetProductRoles_VendorMarketplace_Impersonation_Success()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim("scope", "internalapi")
            };

            var claimsIdentity = new ClaimsIdentity(claims, "pwd");
            var userClaims = new DefaultUserClaim();

            var roleController = MakeInstance(userClaim: userClaims, claimsIdentity: claimsIdentity);

            // Act
            var result = roleController.GetProductRoles("VMP", _multifamilyCompanyRealPageId);
            
            // Assert
            var content = result.Content.ReadAsAsync<PagedResponse>().Result;

            result.StatusCode.ToString().Should().Be(HttpStatusCode.OK.ToString());
            content.Data.Count.Should().Be(4);
        }

        [Fact]
        public void GetProductRoles_VendorMarketplace_Impersonation_Invalid_UPFMCompanyId()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim("scope", "internalapi")
            };

            var claimsIdentity = new ClaimsIdentity(claims, "pwd");
            var userClaims = new DefaultUserClaim();

            var roleController = MakeInstance(userClaim: userClaims, claimsIdentity: claimsIdentity);

            // Act
            var result = roleController.GetProductRoles("VMP", Guid.NewGuid());
            
            // Assert
            var content = result.Content.ReadAsAsync<ErrorResponse>().Result;

            result.StatusCode.ToString().Should().Be(HttpStatusCode.BadRequest.ToString());
            content.Errors.First().Detail.Should().Be("Invalid UPFMId.");
            content.Errors.First().Source.Should().Be("/role");
        }

        #endregion

        #region GetUserProductRoles

        [Fact]
        public void GetUserProductRoles_VendorMarketplace_Success()
        {
            // Arrange
            var userClaims = new DefaultUserClaim
            {
                CorrelationId = new Guid(),
                CustomerMasterId = _BooksCompanyMasterId,
                OrganizationPartyId = _vendorCompanyPartyId,
                PersonaId = _vendorUserPersonaId
            };

            var roleController = MakeInstance(userClaim: userClaims);

            // Act
            var result = roleController.GetUserProductRoles(_vendorUserRealpageId, "VMP");
            
            // Assert
            var content = result.Content.ReadAsAsync<PagedResponse>().Result;
            var roleList = content.Data as IList<ProductRole>;

            result.StatusCode.ToString().Should().Be(HttpStatusCode.OK.ToString());
            content.Data.Count.Should().Be(2);
            roleList?.Any(x => x.Name.Equals("Credentialing Administrator", StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
        }

        [Fact]
        public void GetUserProductRoles_VendorMarketplace_Impersonation_Success()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim("scope", "internalapi")
            };

            var claimsIdentity = new ClaimsIdentity(claims, "pwd");
            var userClaims = new DefaultUserClaim();

            var roleController = MakeInstance(userClaim: userClaims, claimsIdentity: claimsIdentity);

            // Act
            var result = roleController.GetUserProductRoles(_vendorUserRealpageId, "VMP", _multifamilyCompanyRealPageId);
            
            // Assert
            var content = result.Content.ReadAsAsync<PagedResponse>().Result;
            var roleList = content.Data as IList<ProductRole>;

            result.StatusCode.ToString().Should().Be(HttpStatusCode.OK.ToString());
            content.Data.Count.Should().Be(2);
            roleList?.Any(x => x.Name.Equals("Credentialing Administrator", StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
        }

        [Fact]
        public void GetUserProductRoles_UPFM_Success()
        {
            // Arrange
            var userClaim = new DefaultUserClaim
            {
                CorrelationId = new Guid(),
                CustomerMasterId = _BooksCompanyMasterId,
                OrganizationPartyId = _multifamilyCompanyPartyId,
                PersonaId = _multifamilyUserPersonaId,
                UserRealPageGuid = _multifamilyUserRealpageId,
                OrganizationRealPageGuid = _multifamilyCompanyRealPageId,
            };
            var roleController = MakeInstance(userClaim: userClaim);

            // Act
            var result = roleController.GetUserProductRoles(_multifamilyUserRealpageId, "UPFM");

            // Assert
            var content = result.Content.ReadAsAsync<PagedResponse>().Result;

            result.StatusCode.ToString().Should().Be(HttpStatusCode.OK.ToString());
            content.Data.Count.Should().Be(1);
            var role3 = (ProductRole)content.Data[0];
            role3.Name.Should().Be("Platform Administrator");
        }

        [Fact]
        public void GetUserProductRoles_VendorMarketplace_InvalidCompany_Fail()
        {
            // Arrange
            var userClaims = new DefaultUserClaim() { OrganizationPartyId = 9999 };

            var roleController = MakeInstance(userClaim: userClaims);

            // Act
            var result = roleController.GetUserProductRoles(_vendorUserRealpageId, "VMP");

            // Assert
            result.StatusCode.ToString().Should().Be(HttpStatusCode.NotFound.ToString());
        }

        [Fact]
        public void GetUserProductRoles_VendorMarketplace_Impersonation_Invalid_UPFMCompanyId()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim("scope", "internalapi")
            };

            var claimsIdentity = new ClaimsIdentity(claims, "pwd");
            var userClaims = new DefaultUserClaim();

            var roleController = MakeInstance(userClaim: userClaims, claimsIdentity: claimsIdentity);

            // Act
            var result = roleController.GetUserProductRoles(_vendorUserRealpageId, "VMP", Guid.NewGuid());

            // Assert
            var content = result.Content.ReadAsAsync<ErrorResponse>().Result;

            result.StatusCode.ToString().Should().Be(HttpStatusCode.BadRequest.ToString());
            content.Errors.First().Detail.Should().Be("Invalid UPFMId.");
            content.Errors.First().Source.Should().Be("/role");
        }

        [Fact]
        public void GetUserProductRoles_VendorMarketplace_NoProductResults_Fail()
        {
            // Arrange
            var userClaims = new DefaultUserClaim
            {
                CorrelationId = new Guid(),
                CustomerMasterId = _BooksCompanyMasterId,
                OrganizationPartyId = _vendorCompanyPartyId,
                PersonaId = _vendorUserPersonaId
            };

            var roleController = MakeInstance(userClaim: userClaims);

            // Act
            var result = roleController.GetUserProductRoles(_vendorUserRealpageId, "OS");

            // Assert
            var content = result.Content.ReadAsAsync<ErrorResponse>().Result;

            result.StatusCode.ToString().Should().Be(HttpStatusCode.BadRequest.ToString());
            content.Errors.First().Title.Should().Be("Error");
            content.Errors.First().Source.Should().Be("/role");
        }
        #endregion

        #region

        [Fact]
        public void GetRightsforRole_VendorMarketplace_Success()
        {
            // Arrange
            var roleController = MakeInstance();

            // Act
            var result = roleController.GetRightsforRole("VMP", 1);

            // Assert
            var content = result.Content.ReadAsAsync<ListResponse>().Result;
            var rightsList = content.Records as IList<ProductRight>;
            
            result.StatusCode.ToString().Should().Be(HttpStatusCode.OK.ToString());
            content.Records.Count.Should().Be(4);
            rightsList?.Any(x => x.Description.Equals("Award Bids", StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
        }

        [Fact]
        public void GetRightsforRole_VendorMarketplace_Impersonation_Success()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim("scope", "internalapi")
            };

            var claimsIdentity = new ClaimsIdentity(claims, "pwd");
            var userClaims = new DefaultUserClaim();
            var roleController = MakeInstance(userClaim: userClaims, claimsIdentity: claimsIdentity);

            // Act
            var result = roleController.GetRightsforRole("VMP", 1, _multifamilyCompanyRealPageId);

            // Assert
            var content = result.Content.ReadAsAsync<ListResponse>().Result;
            var rightsList = content.Records as IList<ProductRight>;

            result.StatusCode.ToString().Should().Be(HttpStatusCode.OK.ToString());
            content.Records.Count.Should().Be(1);
            rightsList?.Any(x => x.Description.Equals("Access to Bids & Contracts in Vendor Marketplace", StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
        }


        [Fact]
        public void GetRightsforRole_VendorMarketplace_Fail()
        {
            // Arrange
            var roleController = MakeInstance();

            // Act
            var result = roleController.GetRightsforRole(null, 1);

            // Assert
            result.StatusCode.ToString().Should().Be(HttpStatusCode.BadRequest.ToString());
            var content = result.Content.ReadAsAsync<string>().Result;
            content.Should().Be("ProductCode not supplied.");

            // Act
            result = roleController.GetRightsforRole("VMP", 0);
            // Assert
            result.StatusCode.ToString().Should().Be(HttpStatusCode.BadRequest.ToString());
            content = result.Content.ReadAsAsync<string>().Result;
            content.Should().Be("roleId not supplied.");
        }

        #endregion
    }
}
