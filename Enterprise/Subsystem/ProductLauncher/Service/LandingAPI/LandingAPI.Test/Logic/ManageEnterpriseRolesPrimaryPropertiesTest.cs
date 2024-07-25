using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using System.Diagnostics.CodeAnalysis;
using Xunit;
using System.Net.Http;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using System.Net;
using RL = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UserManagement;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.EnterpriseRole;
using IC = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.OneSite;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Saml;
using OC = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.OneSite;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;
using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Extensions;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic
{
    [ExcludeFromCodeCoverage]
    public class ManageEnterpriseRolesPrimaryPropertiesTest : ManageProductBaseTests
    {
        private Mock<IManageBlueBook> mockManageBlueBook;
        private Mock<IRepository> _mockRepository;
        private Mock<IOneSiteProductService> _mockService;
        private Mock<IManageProductBatch> manageProductPanel = null;
        Mock <IUnitOfWork> _mockUnitofWork = new Mock<IUnitOfWork>();
        Mock<IRepositoryResponse> _mockRepositoryResponse = new Mock<IRepositoryResponse>();
        Mock<HttpMessageHandler> _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        private Persona _editotUserPersona;
        private Persona _subjectUserPersona;
        private IList<RL.Role> _roleList;
        private static readonly int _enterPriseRoleId = 356;
        private static readonly DateTime _enterpriseRoleCreatedDate = DateTime.Now.AddMinutes(-1);
        private static int _bulkAddEnterpriseRoleBatchProcessTypeId = (int)BatchProcessType.BulkAddUpdateEnterpriseRole;
        // OneSite Property result
        private static OC.PropertyType _propertyType1 = new OC.PropertyType() { PropertyID = "1234567", PropertyName = "Test Property", SiteCityName = "Test City", SiteState = "Test State", SiteAddress = "Test Street 1", SitePhone = "123-456-7890", SiteZip = "12345", IsAssignedToUser = true };
        private static OC.PropertyType _propertyType2 = new OC.PropertyType() { PropertyID = "7654321", PropertyName = "Test Property 2", SiteCityName = "Test City 2", SiteState = "Test State 2", SiteAddress = "Test Street 1 2", SitePhone = "123-456-7890", SiteZip = "54321", IsAssignedToUser = false };
        private static OC.PropertyType _propertyType3 = new OC.PropertyType() { PropertyID = "2345678", PropertyName = "Test Property 3", SiteCityName = "Test City 3", SiteState = "Test State 3", SiteAddress = "Test Street 1 3", SitePhone = "222-555-7890", SiteZip = "54321", IsAssignedToUser = true };

        // OneSite Role result
        private static OC.RoleType _roleType1 = new OC.RoleType() { RoleID = "1", RoleName = "Role 1", IsInternal = true, Roletype = "Default", IsAssigned = true };
        private static OC.RoleType _roleType2 = new OC.RoleType() { RoleID = "2", RoleName = "Role 2", IsInternal = false, Roletype = "External", IsAssigned = false };

        private void AssertInitial()
        {
            _editotUserPersona = new Persona()
            {
                FromDate = DateTime.UtcNow,
                Name = "Editor User",
                Organization = new Organization() { RealPageId = _editorRealPageId, PartyId = _userOrganizationPartyId },
                PersonaId = _editorPersonaId,
                RealPageId = _editorRealPageId,
                ThruDate = DateTime.UtcNow.AddDays(1),
                UserId = 11,
                OrganizationPartyId = _userOrganizationPartyId,
                // Role = _roleList,
                hasResidentPortalUserAccess = true
            };

            _subjectUserPersona = new Persona()
            {
                PersonaId = _userPersonaId,
                RealPageId = _newuserOrganizationRealPageId,
                Organization = new Organization() { RealPageId = _newuserOrganizationRealPageId, Name = "Test Company", PartyId = _userOrganizationPartyId },
                Name = "Title",
                OrganizationPartyId = _userOrganizationPartyId,
                UserTypeId = 402
            };
            _gbProductMap.Add(new GbProductMap() { BooksProductCode = "UPFM", Name = "Unified Platform", ProductId = 3, UDMSourceCode = "UPFM" });
            _gbProductMap.Add(new GbProductMap() { BooksProductCode = "OS", Name = "OneSite", ProductId = 1, UDMSourceCode = null });
        }

        private ListResponse _vendorMarketplaceMultifamilyRoles;
        private ListResponse _vendorMarketplaceVendorRoles;

        public ListResponse EnterpriseUserRolesTests()
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

          return  _vendorMarketplaceVendorRoles = new ListResponse()
            {
                Records = vendorVendorRoleList.Cast<object>().ToList(),
                CurrentPage = 1,
                TotalRows = vendorVendorRoleList.Count,
                RowsPerPage = 999
            };
        }


        public void FillInstanceData()
        {
            string productSource = "OS";
            mockManageBlueBook = new Mock<IManageBlueBook>();
            _mockService = new Mock<IOneSiteProductService>();
            _mockRepository = new Mock<IRepository>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            IList<ProductUI> productUIList = new List<ProductUI>() { new ProductUI() { ProductId = 3 }, new ProductUI() { ProductId = 56 } };
            IList<int> productIdList = new List<int>() { 3, 56 };
            IList<UserRoleRights> userRoleRights = new List<UserRoleRights>() { new UserRoleRights() { RoleId = 1, DefaultRole = "Manager", IsAssigned = true } };
            IList<Organization> orgList = new List<Organization>() { new Organization() { PartyId = _newUserOrganizationPartyId, Name = "Primary Org", PrimaryOrganization = true } };
            IList<OrganizationType> OrganizationTypeList = new List<OrganizationType>() { new OrganizationType() { CreateDate = DateTime.UtcNow, Name = "Org Name", OrganizationTypeId = 34 } };
            manageProductPanel = new Mock<IManageProductBatch>();

            manageProductPanel
               .Setup(m => m.GetProductRoles(
                   It.Is<long>(l => l == 4),
                   It.Is<long>(l => l == 0),
                   It.Is<int>(g => g == 57),
                   It.Is<long>(x => x == _editorOrganizationPartyId),
                   _userUserClaim))
               .Returns(() => EnterpriseUserRolesTests());


            _mockService
             .Setup(m => m.GetAllProperties(
                It.IsAny<OC.NameValuePair[]>()
             , It.IsAny<string>()
                , It.IsAny<OC.FilterSortParameters>()
            ))
            .Returns(GetOneSitePropertyList());

            _userUserClaim.UserRealPageGuid = _editotUserPersona.RealPageId;

            _mockRepository
            .Setup(m => m.GetOne<Persona>(StoredProcNameConstants.SP_GetPersona, It.Is<object>(
            d => d.ToString().Contains($"personaId = {_userPersonaId}"))))
            .Returns(_subjectUserPersona);

            _mockRepository
             .Setup(m => m.GetOne<Persona>(StoredProcNameConstants.SP_GetPersona, It.Is<object>(
             d => d.ToString().Contains($"personaId = {_editorPersonaId}"))))
             .Returns(_editotUserPersona);

            _mockRepository
            .Setup(m => m.GetMany<RL.Role>(StoredProcNameConstants.SP_ListRolesForProductsByPersonaId,
            It.Is<object>(
            d => TestSqlParameter(d, "{ productId = " + 3 + ", userPersonaId = " + _editorPersonaId + " }"))))
            .Returns(_roleList);

            _mockRepository
             .Setup(m => m.GetMany<ProductUI>(StoredProcNameConstants.SP_ListProductsByOrganization,
                 It.Is<object>(
                     d => TestSqlParameter(d, "{ PartyId = " + _editorOrganizationPartyId + " }"))))
             .Returns(productUIList);

            _mockRepository
              .Setup(m => m.GetMany<UserRoleRights>(StoredProcNameConstants.SP_ListRightsAssociatedWithRoles,
             It.Is<object>(
                d => TestSqlParameter(d, "{ PartyId = " + _editorOrganizationPartyId + ", ProductId = " + (int)ProductEnum.UnifiedPlatform + "  ,TargetProductId = " + productIdList + ") + "))))
              .Returns(userRoleRights);

            _mockRepository
              .Setup(m => m.GetMany<OrganizationDomain>(StoredProcNameConstants.SP_ListOrganizationDomain, null))
            .Returns(GetOrganizationDomainList());

            _mockRepository
            .Setup(m => m.GetMany<OrganizationType>(StoredProcNameConstants.SP_ListOrganizationType, It.IsAny<object>()))
            .Returns(OrganizationTypeList);

            _mockRepository
           .Setup(m => m.GetMany<Organization>(StoredProcNameConstants.SP_ListOrganizationByRealPageId, It.IsAny<object>()))
           .Returns(orgList);

            _mockRepository
            .Setup(m => m.GetMany<ProductInternalSettingByType>(StoredProcNameConstants.SP_ListProductGlobalSettingsBySettingType, It.IsAny<object>()))
            .Returns(GetProductInternalSettingByType());

            _mockRepository
            .Setup(m => m.GetMany<PersonaProductUserDetails>(StoredProcNameConstants.SP_ListProductsByPersonaId, It.IsAny<object>()))
            .Returns(GetPersonaProducts());

            _mockRepository
          .Setup(m => m.GetMany<ProductSettingType>(StoredProcNameConstants.SP_ListProductSettingType, It.IsAny<object>()))
           .Returns(GetProductSettingTypeList());

            _mockRepository
           .Setup(m => m.GetOne<RoleTemplate>(StoredProcNameConstants.SP_GetUserRoleTemplate, It.IsAny<object>()))
           .Returns(GetRoleTemplate());

            _mockRepository
            .Setup(m => m.GetMany<RoleTemplateProductRole>(StoredProcNameConstants.SP_GetRoleTemplateProductRoleMappings, It.IsAny<object>()))
            .Returns(GetroleTemplateProductRoleList());

            _mockRepository
            .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct,
              It.IsAny<object>()))
            .Returns(_gbProductMap);

            _mockRepository.Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct,
              It.IsAny<object>())).Returns(GetOneSiteProductSettings());

            _mockRepository.Setup(m => m.GetMany<SamlAttributes>(StoredProcNameConstants.SP_GetProductSamlDetails,
            It.IsAny<object>())).Returns(new List<SamlAttributes>());

            _mockRepository.Setup(m => m.GetMany<UPFMPropertyInstance>(StoredProcNameConstants.SP_GetPropertyInstanceByPersonaId,
             It.IsAny<object>())).Returns(GetOneSiteUPFMPropertyInstanceList());

            mockManageBlueBook
                .Setup(m => m.GetCompanyMap(
                    It.IsAny<Guid>(),
                    It.IsAny<long>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()
                ))
                .Returns(GetCustomerCompanyMapList());

            _mockRepository.Setup(m => m.GetMany<UPFMPropertyInstance>(StoredProcNameConstants.SP_GetPropertyInstanceListById,
              It.IsAny<object>())).Returns(GetOneSiteUPFMPropertyInstanceList());

            _mockService
                   .Setup(m => m.GetAllRoles(
                       It.IsAny<OC.NameValuePair[]>()
                       , It.IsAny<string>()
                       , It.IsAny<OC.FilterSortParameters>()
                    ))
                    .Returns(GetOnsiteRoles());

            _mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/translate/v2/companyinstance/{GetOrganizationList()[0].RealPageId}/{ProductEnum.UnifiedPlatform.ToEnumDescription()}/{productSource}?filter[greenbookCares]=true", GetHttpResourceMapResponseMessage());
            _mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/companypropertyinstancemap?include=propertyInstance&filter[source]=UPFM&filter[companyinstance.companyInstanceSourceId]={GetOrganizationList()[0].RealPageId}&fields[propertyInstance]=propertyInstanceSourceId,propertyName,domain,isActive&filter[propertyInstance.isActive]=true&page[size]=9999", GetHttpPropertyInstanceResponseMessage());

            _mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                .Returns(GetOneSiteProductSettings());

            _mockRepository.Setup(m => m.GetOne<dynamic>(StoredProcNameConstants.SP_CreateProductBatch, It.IsAny<object>()))
             .Returns(new RepositoryResponse() { Id = 322 });

        }



        [Fact]
        public void UserPrimaryProperties_TurnOnAllUsePrimeProperties()
        {
            FillInstanceData();

            _mockRepository
            .Setup(m => m.GetMany<ProductSettingList>(StoredProcNameConstants.SP_ListProductSettingsByPersonaId, It.IsAny<object>()))
            .Returns(GetUsePrimaryPropertiesSettingTurnOn());

            _mockRepository.Setup(m => m.GetMany<Setting>(StoredProcNameConstants.SP_GetUnifiedSetting, It.IsAny<object>()))
            .Returns(GetPrimaryPropertyEnterpriseRoleAtOrganizationLevelTurnOn());


            _mockRepository.Setup(m => m.GetMany<ProductSettingList>(StoredProcNameConstants.SP_ListProductSettingsByOrganization,
            It.IsAny<object>())).Returns(GetUsePrimaryPropertiesSettingTurnOnAtProductLevel());

            _mockHttpMessageHandler.Setup(HttpMethod.Post, $"http://localhost/translate/v3/propertyinstance/UPFM/OS", GetTranslatePropertyInstanceHttpResponse());




            ManageEnterpriseRolesPrimaryProperties manageEnterpriseRolesPrimaryProperies = new ManageEnterpriseRolesPrimaryProperties(
                                                        _mockRepository.Object
                                                      , _mockHttpMessageHandler.Object
                                                      , _userUserClaim,
                                                        _mockService.Object
                                                        , mockManageBlueBook.Object, manageProductPanel.Object);




            string response = manageEnterpriseRolesPrimaryProperies.ProcessEnterpriseRolesAndPrimaryPropertiesData(_editorPersonaId, _userPersonaId);
            bool isBatchProcessProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                  .Equals("Batch.CreateProductBatch", StringComparison.OrdinalIgnoreCase)));

            bool isPropertyInstanceProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                        .Equals("Enterprise.GetPropertyInstanceByPersonaId", StringComparison.OrdinalIgnoreCase)));

            bool isEnterpriseRoleNewProductsProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                                  .Equals("Security.GetEnterpriseRoleNewProductsByRoleTemplateId", StringComparison.OrdinalIgnoreCase)));

            bool isEnterpriseRoleUpdatedProductsProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                                  .Equals("Security.GetEnterpriseRoleUpdatedProductsByRoleTemplateId", StringComparison.OrdinalIgnoreCase)));

            bool isEnterpriseRoleDeletedProductsProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                                  .Equals("Security.GetEnterpriseRoleDeletedProductsByRoleTemplateId", StringComparison.OrdinalIgnoreCase)));

            Assert.False(isEnterpriseRoleNewProductsProcExecuted);
            Assert.False(isEnterpriseRoleUpdatedProductsProcExecuted);
            Assert.False(isEnterpriseRoleDeletedProductsProcExecuted);
            Assert.True(isPropertyInstanceProcExecuted);
            Assert.True(_mockHttpMessageHandler.Invocations.Count > 0);
            Assert.True(_mockService.Invocations.Count > 0);
            Assert.True(isBatchProcessProcExecuted);
        }

        [Fact]
        public void UserPrimaryProperties_TurnOffUsePrimePropertiesAtOrganization()
        {
            FillInstanceData();

            _mockRepository
            .Setup(m => m.GetMany<ProductSettingList>(StoredProcNameConstants.SP_ListProductSettingsByPersonaId, It.IsAny<object>()))
            .Returns(GetUsePrimaryPropertiesSettingTurnOn());



            _mockRepository.Setup(m => m.GetMany<ProductSettingList>(StoredProcNameConstants.SP_ListProductSettingsByOrganization,
            It.IsAny<object>())).Returns(GetUsePrimaryPropertiesSettingTurnOnAtProductLevel());

            _mockHttpMessageHandler.Setup(HttpMethod.Post, $"http://localhost/translate/v3/propertyinstance/UPFM/OS", GetTranslatePropertyInstanceHttpResponse());



            ManageEnterpriseRolesPrimaryProperties manageEnterpriseRolesPrimaryProperies = new ManageEnterpriseRolesPrimaryProperties(
                                                        _mockRepository.Object
                                                      , _mockHttpMessageHandler.Object
                                                      , _userUserClaim, _mockService.Object, mockManageBlueBook.Object);


            string response = manageEnterpriseRolesPrimaryProperies.ProcessEnterpriseRolesAndPrimaryPropertiesData(_editorPersonaId, _userPersonaId);
            bool isBatchProcessProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                              .Equals("Batch.CreateProductBatch", StringComparison.OrdinalIgnoreCase)));

            bool isPropertyInstanceProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                                  .Equals("Enterprise.GetPropertyInstanceByPersonaId", StringComparison.OrdinalIgnoreCase)));

            bool isEnterpriseRoleNewProductsProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                                  .Equals("Security.GetEnterpriseRoleNewProductsByRoleTemplateId", StringComparison.OrdinalIgnoreCase)));

            bool isEnterpriseRoleUpdatedProductsProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                                  .Equals("Security.GetEnterpriseRoleUpdatedProductsByRoleTemplateId", StringComparison.OrdinalIgnoreCase)));

            bool isEnterpriseRoleDeletedProductsProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                                  .Equals("Security.GetEnterpriseRoleDeletedProductsByRoleTemplateId", StringComparison.OrdinalIgnoreCase)));

            Assert.False(isEnterpriseRoleNewProductsProcExecuted);
            Assert.False(isEnterpriseRoleUpdatedProductsProcExecuted);
            Assert.False(isEnterpriseRoleDeletedProductsProcExecuted);
            Assert.False(isPropertyInstanceProcExecuted);
            Assert.False(isBatchProcessProcExecuted);
            Assert.True(_mockHttpMessageHandler.Invocations.Count == 0);
            Assert.True(_mockService.Invocations.Count == 0);
        }

        [Fact]
        public void UserPrimaryProperties_TurnOffUsePrimePropertiesAtProductLevel()
        {
            FillInstanceData();

            _mockRepository
            .Setup(m => m.GetMany<ProductSettingList>(StoredProcNameConstants.SP_ListProductSettingsByPersonaId, It.IsAny<object>()))
            .Returns(GetUsePrimaryPropertiesSettingTurnOn());


            _mockRepository.Setup(m => m.GetMany<Setting>(StoredProcNameConstants.SP_GetUnifiedSetting, It.IsAny<object>()))
            .Returns(GetPrimaryPropertyEnterpriseRoleAtOrganizationLevelTurnOff());



            _mockRepository.Setup(m => m.GetMany<ProductSettingList>(StoredProcNameConstants.SP_ListProductSettingsByOrganization,
            It.IsAny<object>())).Returns(GetUsePrimaryPropertiesSettingTurnOffAtProductLevel());
            _mockHttpMessageHandler.Setup(HttpMethod.Post, $"http://localhost/translate/v3/propertyinstance/UPFM/OS", GetTranslatePropertyInstanceHttpResponse());


            ManageEnterpriseRolesPrimaryProperties manageEnterpriseRolesPrimaryProperies = new ManageEnterpriseRolesPrimaryProperties(
                                                        _mockRepository.Object
                                                      , _mockHttpMessageHandler.Object
                                                      , _userUserClaim, _mockService.Object, mockManageBlueBook.Object);


            // Act
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();
            string response = manageEnterpriseRolesPrimaryProperies.ProcessEnterpriseRolesAndPrimaryPropertiesData(_editorPersonaId, _userPersonaId);

            bool isBatchProcessProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                              .Equals("Batch.CreateProductBatch", StringComparison.OrdinalIgnoreCase)));

            bool isPropertyInstanceProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                                  .Equals("Enterprise.GetPropertyInstanceByPersonaId", StringComparison.OrdinalIgnoreCase)));

            bool isEnterpriseRoleNewProductsProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                                  .Equals("Security.GetEnterpriseRoleNewProductsByRoleTemplateId", StringComparison.OrdinalIgnoreCase)));

            bool isEnterpriseRoleUpdatedProductsProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                                  .Equals("Security.GetEnterpriseRoleUpdatedProductsByRoleTemplateId", StringComparison.OrdinalIgnoreCase)));

            bool isEnterpriseRoleDeletedProductsProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                                  .Equals("Security.GetEnterpriseRoleDeletedProductsByRoleTemplateId", StringComparison.OrdinalIgnoreCase)));

            Assert.False(isEnterpriseRoleNewProductsProcExecuted);
            Assert.False(isEnterpriseRoleUpdatedProductsProcExecuted);
            Assert.False(isEnterpriseRoleDeletedProductsProcExecuted);
            Assert.False(isPropertyInstanceProcExecuted);
            Assert.False(isBatchProcessProcExecuted);
            Assert.True(_mockHttpMessageHandler.Invocations.Count == 0);
            Assert.True(_mockService.Invocations.Count == 0);
        }


        [Fact]
        public void UserPrimaryProperties_TurnOnAllUsePrimeProperties_NonTranslatedPrimaryProperty()
        {
            FillInstanceData();

            _mockRepository
            .Setup(m => m.GetMany<ProductSettingList>(StoredProcNameConstants.SP_ListProductSettingsByPersonaId, It.IsAny<object>()))
            .Returns(GetUsePrimaryPropertiesSettingTurnOn());



            _mockRepository.Setup(m => m.GetMany<Setting>(StoredProcNameConstants.SP_GetUnifiedSetting, It.IsAny<object>()))
            .Returns(GetPrimaryPropertyEnterpriseRoleAtOrganizationLevelTurnOn());


            _mockRepository.Setup(m => m.GetMany<ProductSettingList>(StoredProcNameConstants.SP_ListProductSettingsByOrganization,
            It.IsAny<object>())).Returns(GetUsePrimaryPropertiesSettingTurnOnAtProductLevel());
            _mockHttpMessageHandler.Setup(HttpMethod.Post, $"http://localhost/translate/v3/propertyinstance/UPFM/OS", GetNonTranslatePropertyInstanceHttpResponse());


            ManageEnterpriseRolesPrimaryProperties manageEnterpriseRolesPrimaryProperies = new ManageEnterpriseRolesPrimaryProperties(
                                                        _mockRepository.Object
                                                      , _mockHttpMessageHandler.Object
                                                      , _userUserClaim,
                                                        _mockService.Object
                                                        , mockManageBlueBook.Object);


            // Act
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();


            string response = manageEnterpriseRolesPrimaryProperies.ProcessEnterpriseRolesAndPrimaryPropertiesData(_editorPersonaId, _userPersonaId);
            bool isBatchProcessProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                  .Equals("Batch.CreateProductBatch", StringComparison.OrdinalIgnoreCase)));

            bool isPropertyInstanceProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                        .Equals("Enterprise.GetPropertyInstanceByPersonaId", StringComparison.OrdinalIgnoreCase)));

            bool isEnterpriseRoleNewProductsProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                                  .Equals("Security.GetEnterpriseRoleNewProductsByRoleTemplateId", StringComparison.OrdinalIgnoreCase)));

            bool isEnterpriseRoleUpdatedProductsProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                                  .Equals("Security.GetEnterpriseRoleUpdatedProductsByRoleTemplateId", StringComparison.OrdinalIgnoreCase)));

            bool isEnterpriseRoleDeletedProductsProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                                  .Equals("Security.GetEnterpriseRoleDeletedProductsByRoleTemplateId", StringComparison.OrdinalIgnoreCase)));

            Assert.False(isEnterpriseRoleNewProductsProcExecuted);
            Assert.False(isEnterpriseRoleUpdatedProductsProcExecuted);
            Assert.False(isEnterpriseRoleDeletedProductsProcExecuted);
            Assert.True(isPropertyInstanceProcExecuted);
            Assert.True(_mockHttpMessageHandler.Invocations.Count > 0);
            Assert.True(_mockService.Invocations.Count > 0);
            Assert.True(isBatchProcessProcExecuted);
        }

        [Fact]
        public void EnterpriseRole_TurnOnAllUsePrimeProperties()
        {
            FillInstanceData();

            _mockRepository
            .Setup(m => m.GetMany<ProductSettingList>(StoredProcNameConstants.SP_ListProductSettingsByPersonaId, It.IsAny<object>()))
            .Returns(GetUsePrimaryPropertiesSettingTurnOn());


            _mockRepository.Setup(m => m.GetMany<Setting>(StoredProcNameConstants.SP_GetUnifiedSetting, It.IsAny<object>()))
            .Returns(GetPrimaryPropertyEnterpriseRoleAtOrganizationLevelTurnOn());

            _mockRepository.Setup(m => m.GetMany<ProductSettingList>(StoredProcNameConstants.SP_ListProductSettingsByOrganization,
            It.IsAny<object>())).Returns(GetUsePrimaryPropertiesSettingTurnOnAtProductLevel());

            _mockHttpMessageHandler.Setup(HttpMethod.Post, $"http://localhost/translate/v3/propertyinstance/UPFM/OS", GetTranslatePropertyInstanceHttpResponse());



            _mockRepository.Setup(m => m.GetMany<int>(StoredProcNameConstants.SP_GetEnterpriseRoleNewProductsByRoleTemplateId, It.IsAny<object>()))
            .Returns(new List<int>() { 1 });

            ManageEnterpriseRolesPrimaryProperties manageEnterpriseRolesPrimaryProperies = new ManageEnterpriseRolesPrimaryProperties(
                                                        _mockRepository.Object
                                                      , _mockHttpMessageHandler.Object
                                                      , _userUserClaim,
                                                        _mockService.Object
                                                        , mockManageBlueBook.Object);


            // Act
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();


            string response = manageEnterpriseRolesPrimaryProperies.ProcessEnterpriseRolesAndPrimaryPropertiesData(_editorPersonaId, _userPersonaId, _enterPriseRoleId, _enterpriseRoleCreatedDate);

            bool isBatchProcessProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                  .Equals("Batch.CreateProductBatch", StringComparison.OrdinalIgnoreCase)));

            bool isPropertyInstanceProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                        .Equals("Enterprise.GetPropertyInstanceByPersonaId", StringComparison.OrdinalIgnoreCase)));

            bool isEnterpriseRoleNewProductsProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                                  .Equals("Security.GetEnterpriseRoleNewProductsByRoleTemplateId", StringComparison.OrdinalIgnoreCase)));

            bool isEnterpriseRoleUpdatedProductsProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                                  .Equals("Security.GetEnterpriseRoleUpdatedProductsByRoleTemplateId", StringComparison.OrdinalIgnoreCase)));

            bool isEnterpriseRoleDeletedProductsProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                                  .Equals("Security.GetEnterpriseRoleDeletedProductsByRoleTemplateId", StringComparison.OrdinalIgnoreCase)));

            bool isListProductsByPersonaIdProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                              .Equals("Enterprise.ListProductsByPersonaId", StringComparison.OrdinalIgnoreCase)));
            Assert.False(isListProductsByPersonaIdProcExecuted);
            Assert.True(isEnterpriseRoleNewProductsProcExecuted);
            Assert.True(isEnterpriseRoleUpdatedProductsProcExecuted);
            Assert.True(isEnterpriseRoleDeletedProductsProcExecuted);
            Assert.True(isPropertyInstanceProcExecuted);
            Assert.True(_mockHttpMessageHandler.Invocations.Count > 0);
            Assert.True(_mockService.Invocations.Count > 0);
            Assert.True(isBatchProcessProcExecuted);
        }

        [Fact]
        public void EnterpriseRoles_TurnOffUsePrimePropertiesAtOrganization()
        {
            FillInstanceData();


            _mockRepository
            .Setup(m => m.GetMany<ProductSettingList>(StoredProcNameConstants.SP_ListProductSettingsByPersonaId, It.IsAny<object>()))
            .Returns(GetUsePrimaryPropertiesSettingTurnOn());



            _mockRepository.Setup(m => m.GetMany<Setting>(StoredProcNameConstants.SP_GetUnifiedSetting, It.IsAny<object>()))
            .Returns(GetPrimaryPropertyEnterpriseRoleAtOrganizationLevelTurnOff());




            _mockRepository.Setup(m => m.GetMany<ProductSettingList>(StoredProcNameConstants.SP_ListProductSettingsByOrganization,
            It.IsAny<object>())).Returns(GetUsePrimaryPropertiesSettingTurnOnAtProductLevel());

            _mockHttpMessageHandler.Setup(HttpMethod.Post, $"http://localhost/translate/v3/propertyinstance/UPFM/OS", GetTranslatePropertyInstanceHttpResponse());


            _mockRepository.Setup(m => m.GetMany<int>(StoredProcNameConstants.SP_GetEnterpriseRoleNewProductsByRoleTemplateId, It.IsAny<object>()))
            .Returns(new List<int>() { 1 });

            ManageEnterpriseRolesPrimaryProperties manageEnterpriseRolesPrimaryProperies = new ManageEnterpriseRolesPrimaryProperties(
                                                        _mockRepository.Object
                                                      , _mockHttpMessageHandler.Object
                                                      , _userUserClaim, _mockService.Object, mockManageBlueBook.Object);


            // Act
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            string response = manageEnterpriseRolesPrimaryProperies.ProcessEnterpriseRolesAndPrimaryPropertiesData(_editorPersonaId, _userPersonaId, _enterPriseRoleId, _enterpriseRoleCreatedDate);

            bool isBatchProcessProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                              .Equals("Batch.CreateProductBatch", StringComparison.OrdinalIgnoreCase)));

            bool isPropertyInstanceProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                                  .Equals("Enterprise.GetPropertyInstanceByPersonaId", StringComparison.OrdinalIgnoreCase)));

            bool isEnterpriseRoleNewProductsProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                                  .Equals("Security.GetEnterpriseRoleNewProductsByRoleTemplateId", StringComparison.OrdinalIgnoreCase)));

            bool isEnterpriseRoleUpdatedProductsProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                                  .Equals("Security.GetEnterpriseRoleUpdatedProductsByRoleTemplateId", StringComparison.OrdinalIgnoreCase)));

            bool isEnterpriseRoleDeletedProductsProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                                  .Equals("Security.GetEnterpriseRoleDeletedProductsByRoleTemplateId", StringComparison.OrdinalIgnoreCase)));

            Assert.True(isEnterpriseRoleNewProductsProcExecuted);
            Assert.True(isEnterpriseRoleUpdatedProductsProcExecuted);
            Assert.True(isEnterpriseRoleDeletedProductsProcExecuted);
            Assert.False(isPropertyInstanceProcExecuted);
            Assert.False(isBatchProcessProcExecuted);
            Assert.True(_mockHttpMessageHandler.Invocations.Count == 0);
            Assert.True(_mockService.Invocations.Count == 0);
        }

        [Fact]
        public void EnterpriseRoles_TurnOffUsePrimePropertiesAtProductLevel()
        {
            FillInstanceData();


            _mockRepository
            .Setup(m => m.GetMany<ProductSettingList>(StoredProcNameConstants.SP_ListProductSettingsByPersonaId, It.IsAny<object>()))
            .Returns(GetUsePrimaryPropertiesSettingTurnOn());

            _mockRepository.Setup(m => m.GetMany<Setting>(StoredProcNameConstants.SP_GetUnifiedSetting, It.IsAny<object>()))
            .Returns(GetPrimaryPropertyEnterpriseRoleAtOrganizationLevelTurnOff());


            _mockRepository.Setup(m => m.GetMany<ProductSettingList>(StoredProcNameConstants.SP_ListProductSettingsByOrganization,
            It.IsAny<object>())).Returns(GetUsePrimaryPropertiesSettingTurnOffAtProductLevel());

            _mockHttpMessageHandler.Setup(HttpMethod.Post, $"http://localhost/translate/v3/propertyinstance/UPFM/OS", GetTranslatePropertyInstanceHttpResponse());

            _mockRepository.Setup(m => m.GetMany<int>(StoredProcNameConstants.SP_GetEnterpriseRoleNewProductsByRoleTemplateId, It.IsAny<object>()))
            .Returns(new List<int>() { 1 });





            ManageEnterpriseRolesPrimaryProperties manageEnterpriseRolesPrimaryProperies = new ManageEnterpriseRolesPrimaryProperties(
                                                        _mockRepository.Object
                                                      , _mockHttpMessageHandler.Object
                                                      , _userUserClaim, _mockService.Object, mockManageBlueBook.Object);


            // Act
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            string response = manageEnterpriseRolesPrimaryProperies.ProcessEnterpriseRolesAndPrimaryPropertiesData(_editorPersonaId, _userPersonaId, _enterPriseRoleId, _enterpriseRoleCreatedDate);

            bool isBatchProcessProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                              .Equals("Batch.CreateProductBatch", StringComparison.OrdinalIgnoreCase)));

            bool isPropertyInstanceProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                                  .Equals("Enterprise.GetPropertyInstanceByPersonaId", StringComparison.OrdinalIgnoreCase)));

            bool isEnterpriseRoleNewProductsProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                                  .Equals("Security.GetEnterpriseRoleNewProductsByRoleTemplateId", StringComparison.OrdinalIgnoreCase)));

            bool isEnterpriseRoleUpdatedProductsProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                                  .Equals("Security.GetEnterpriseRoleUpdatedProductsByRoleTemplateId", StringComparison.OrdinalIgnoreCase)));

            bool isEnterpriseRoleDeletedProductsProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                                  .Equals("Security.GetEnterpriseRoleDeletedProductsByRoleTemplateId", StringComparison.OrdinalIgnoreCase)));

            Assert.True(isEnterpriseRoleNewProductsProcExecuted);
            Assert.True(isEnterpriseRoleUpdatedProductsProcExecuted);
            Assert.True(isEnterpriseRoleDeletedProductsProcExecuted);
            Assert.False(isPropertyInstanceProcExecuted);
            Assert.False(isBatchProcessProcExecuted);
            Assert.True(_mockHttpMessageHandler.Invocations.Count == 0);
            Assert.True(_mockService.Invocations.Count == 0);
        }

        /// <summary>
        /// If there is no translated primary properties then product should be unassigned. For this Batch process required to run.
        /// </summary>
        [Fact]
        public void EnterpriseRoles_TurnOnAllUsePrimeProperties_NonTranslatedPrimaryProperty()
        {
            FillInstanceData();

            _mockRepository
            .Setup(m => m.GetMany<ProductSettingList>(StoredProcNameConstants.SP_ListProductSettingsByPersonaId, It.IsAny<object>()))
            .Returns(GetUsePrimaryPropertiesSettingTurnOn());

            _mockRepository.Setup(m => m.GetMany<Setting>(StoredProcNameConstants.SP_GetUnifiedSetting, It.IsAny<object>()))
            .Returns(GetPrimaryPropertyEnterpriseRoleAtOrganizationLevelTurnOn());

            _mockRepository.Setup(m => m.GetMany<ProductSettingList>(StoredProcNameConstants.SP_ListProductSettingsByOrganization,
            It.IsAny<object>())).Returns(GetUsePrimaryPropertiesSettingTurnOnAtProductLevel());

            _mockRepository.Setup(m => m.GetMany<int>(StoredProcNameConstants.SP_GetEnterpriseRoleNewProductsByRoleTemplateId, It.IsAny<object>()))
             .Returns(new List<int>() { 1 });

            _mockHttpMessageHandler.Setup(HttpMethod.Post, $"http://localhost/translate/v3/propertyinstance/UPFM/OS", GetNonTranslatePropertyInstanceHttpResponse());

            ManageEnterpriseRolesPrimaryProperties manageEnterpriseRolesPrimaryProperies = new ManageEnterpriseRolesPrimaryProperties(
                                                       _mockRepository.Object
                                                     , _mockHttpMessageHandler.Object
                                                     , _userUserClaim,
                                                       _mockService.Object
                                                       , mockManageBlueBook.Object);


            // Act
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            string response = manageEnterpriseRolesPrimaryProperies.ProcessEnterpriseRolesAndPrimaryPropertiesData(_editorPersonaId, _userPersonaId, _enterPriseRoleId, _enterpriseRoleCreatedDate);
            bool isBatchProcessProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                  .Equals("Batch.CreateProductBatch", StringComparison.OrdinalIgnoreCase)));

            bool isPropertyInstanceProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                        .Equals("Enterprise.GetPropertyInstanceByPersonaId", StringComparison.OrdinalIgnoreCase)));

            bool isEnterpriseRoleNewProductsProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                                  .Equals("Security.GetEnterpriseRoleNewProductsByRoleTemplateId", StringComparison.OrdinalIgnoreCase)));

            bool isEnterpriseRoleUpdatedProductsProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                                  .Equals("Security.GetEnterpriseRoleUpdatedProductsByRoleTemplateId", StringComparison.OrdinalIgnoreCase)));

            bool isEnterpriseRoleDeletedProductsProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                                  .Equals("Security.GetEnterpriseRoleDeletedProductsByRoleTemplateId", StringComparison.OrdinalIgnoreCase)));

            //Assert

            Assert.True(isEnterpriseRoleNewProductsProcExecuted);
            Assert.True(isEnterpriseRoleUpdatedProductsProcExecuted);
            Assert.True(isEnterpriseRoleDeletedProductsProcExecuted);
            Assert.True(isPropertyInstanceProcExecuted);
            Assert.True(_mockHttpMessageHandler.Invocations.Count > 0);
            Assert.True(_mockService.Invocations.Count > 0);
            Assert.True(isBatchProcessProcExecuted);
        }

        /// <summary>
        /// Bulk Assign Enterprise Role
        /// </summary>
        [Fact]
        public void EnterpriseRoles_TurnOffUsePrimePropertiesAtProductLevelForUnassign()
        {
            //int productBulkAddEnterpriseRoleBatchProcessId = (int)BatchProcessType.BulkAddUpdateEnterpriseRole;
            FillInstanceData();

            _mockRepository
            .Setup(m => m.GetMany<ProductSettingList>(StoredProcNameConstants.SP_ListProductSettingsByPersonaId, It.IsAny<object>()))
            .Returns(GetUsePrimaryPropertiesSettingTurnOn());

            _mockRepository.Setup(m => m.GetMany<Setting>(StoredProcNameConstants.SP_GetUnifiedSetting, It.IsAny<object>()))
            .Returns(GetPrimaryPropertyEnterpriseRoleAtOrganizationLevelTurnOff());


            _mockRepository.Setup(m => m.GetMany<ProductSettingList>(StoredProcNameConstants.SP_ListProductSettingsByOrganization,
            It.IsAny<object>())).Returns(GetUsePrimaryPropertiesSettingTurnOffAtProductLevel());

            _mockHttpMessageHandler.Setup(HttpMethod.Post, $"http://localhost/translate/v3/propertyinstance/UPFM/OS", GetTranslatePropertyInstanceHttpResponse());

            _mockRepository.Setup(m => m.GetMany<int>(StoredProcNameConstants.SP_GetEnterpriseRoleNewProductsByRoleTemplateId, It.IsAny<object>()))
            .Returns(new List<int>() { 1 });





            ManageEnterpriseRolesPrimaryProperties manageEnterpriseRolesPrimaryProperies = new ManageEnterpriseRolesPrimaryProperties(
                                                        _mockRepository.Object
                                                      , _mockHttpMessageHandler.Object
                                                      , _userUserClaim, _mockService.Object, mockManageBlueBook.Object);


            // Act
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            string response = manageEnterpriseRolesPrimaryProperies.ProcessEnterpriseRolesAndPrimaryPropertiesData(_editorPersonaId, _userPersonaId, _enterPriseRoleId, _enterpriseRoleCreatedDate, _bulkAddEnterpriseRoleBatchProcessTypeId, true);

            bool isBatchProcessProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                              .Equals("Batch.CreateProductBatch", StringComparison.OrdinalIgnoreCase)));

            bool isPropertyInstanceProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                                  .Equals("Enterprise.GetPropertyInstanceByPersonaId", StringComparison.OrdinalIgnoreCase)));

            bool isEnterpriseRoleNewProductsProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                                  .Equals("Security.GetEnterpriseRoleNewProductsByRoleTemplateId", StringComparison.OrdinalIgnoreCase)));

            bool isEnterpriseRoleUpdatedProductsProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                                  .Equals("Security.GetEnterpriseRoleUpdatedProductsByRoleTemplateId", StringComparison.OrdinalIgnoreCase)));

            bool isEnterpriseRoleDeletedProductsProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                                  .Equals("Security.GetEnterpriseRoleDeletedProductsByRoleTemplateId", StringComparison.OrdinalIgnoreCase)));

            Assert.False(isEnterpriseRoleNewProductsProcExecuted);
            Assert.False(isEnterpriseRoleUpdatedProductsProcExecuted);
            Assert.False(isEnterpriseRoleDeletedProductsProcExecuted);
            Assert.False(isPropertyInstanceProcExecuted);
            Assert.True(isBatchProcessProcExecuted);
            Assert.True(_mockHttpMessageHandler.Invocations.Count == 0);
            Assert.True(_mockService.Invocations.Count == 0);
        }

        /// <summary>
        /// Bulk Assign Enterprise Role
        /// </summary>
        [Fact]
        public void EnterpriseRoles_IncludeUFAndAOProductForUnassign()
        {

            FillInstanceData();

            _mockRepository.Setup(m => m.GetMany<PersonaProductUserDetails>(StoredProcNameConstants.SP_ListProductsByPersonaId, It.Is<object>(d => TestSqlParameterContains(d, _userPersonaId.ToString()))))
            .Returns(GetPersonaProductsForUnassign());

            _mockRepository
            .Setup(m => m.GetMany<ProductSettingList>(StoredProcNameConstants.SP_ListProductSettingsByPersonaId, It.IsAny<object>()))
            .Returns(GetUsePrimaryPropertiesSettingTurnOn());

            _mockRepository.Setup(m => m.GetMany<Setting>(StoredProcNameConstants.SP_GetUnifiedSetting, It.IsAny<object>()))
            .Returns(GetPrimaryPropertyEnterpriseRoleAtOrganizationLevelTurnOff());


            _mockRepository.Setup(m => m.GetMany<ProductSettingList>(StoredProcNameConstants.SP_ListProductSettingsByOrganization,
            It.IsAny<object>())).Returns(GetUsePrimaryPropertiesSettingTurnOffAtProductLevel());

            _mockHttpMessageHandler.Setup(HttpMethod.Post, $"http://localhost/translate/v3/propertyinstance/UPFM/OS", GetTranslatePropertyInstanceHttpResponse());

            _mockRepository.Setup(m => m.GetMany<int>(StoredProcNameConstants.SP_GetEnterpriseRoleNewProductsByRoleTemplateId, It.IsAny<object>()))
            .Returns(new List<int>() { 1 });

            ManageEnterpriseRolesPrimaryProperties manageEnterpriseRolesPrimaryProperies = new ManageEnterpriseRolesPrimaryProperties(
                                                        _mockRepository.Object
                                                      , _mockHttpMessageHandler.Object
                                                      , _userUserClaim, _mockService.Object, mockManageBlueBook.Object);

            // Act
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            string response = manageEnterpriseRolesPrimaryProperies.ProcessEnterpriseRolesAndPrimaryPropertiesData(_editorPersonaId, _userPersonaId, _enterPriseRoleId, _enterpriseRoleCreatedDate, _bulkAddEnterpriseRoleBatchProcessTypeId, true);

            bool isBatchProcessProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                              .Equals("Batch.CreateProductBatch", StringComparison.OrdinalIgnoreCase)));

            bool isPropertyInstanceProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                                  .Equals("Enterprise.GetPropertyInstanceByPersonaId", StringComparison.OrdinalIgnoreCase)));

            bool isEnterpriseRoleNewProductsProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                                  .Equals("Security.GetEnterpriseRoleNewProductsByRoleTemplateId", StringComparison.OrdinalIgnoreCase)));

            bool isEnterpriseRoleUpdatedProductsProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                                  .Equals("Security.GetEnterpriseRoleUpdatedProductsByRoleTemplateId", StringComparison.OrdinalIgnoreCase)));

            bool isEnterpriseRoleDeletedProductsProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                                  .Equals("Security.GetEnterpriseRoleDeletedProductsByRoleTemplateId", StringComparison.OrdinalIgnoreCase)));

            Assert.False(isEnterpriseRoleNewProductsProcExecuted);
            Assert.False(isEnterpriseRoleUpdatedProductsProcExecuted);
            Assert.False(isEnterpriseRoleDeletedProductsProcExecuted);
            Assert.False(isPropertyInstanceProcExecuted);
            Assert.False(isBatchProcessProcExecuted);
            Assert.True(_mockHttpMessageHandler.Invocations.Count == 0);
            Assert.True(_mockService.Invocations.Count == 0);
        }

        /// <summary>
        /// Bulk Assign Enterprise Role
        /// </summary>
        [Fact]
        public void EnterpriseRoles_TurnOnAllUsePrimeProperties_NonTranslatedPrimaryPropertyForUnassign()
        {
            FillInstanceData();

            _mockRepository
            .Setup(m => m.GetMany<ProductSettingList>(StoredProcNameConstants.SP_ListProductSettingsByPersonaId, It.IsAny<object>()))
            .Returns(GetUsePrimaryPropertiesSettingTurnOn());

            _mockRepository.Setup(m => m.GetMany<Setting>(StoredProcNameConstants.SP_GetUnifiedSetting, It.IsAny<object>()))
            .Returns(GetPrimaryPropertyEnterpriseRoleAtOrganizationLevelTurnOn());

            _mockRepository.Setup(m => m.GetMany<ProductSettingList>(StoredProcNameConstants.SP_ListProductSettingsByOrganization,
            It.IsAny<object>())).Returns(GetUsePrimaryPropertiesSettingTurnOnAtProductLevel());

            _mockRepository.Setup(m => m.GetMany<int>(StoredProcNameConstants.SP_GetEnterpriseRoleNewProductsByRoleTemplateId, It.IsAny<object>()))
             .Returns(new List<int>() { 1 });

            _mockHttpMessageHandler.Setup(HttpMethod.Post, $"http://localhost/translate/v3/propertyinstance/UPFM/OS", GetNonTranslatePropertyInstanceHttpResponse());

            ManageEnterpriseRolesPrimaryProperties manageEnterpriseRolesPrimaryProperies = new ManageEnterpriseRolesPrimaryProperties(
                                                       _mockRepository.Object
                                                     , _mockHttpMessageHandler.Object
                                                     , _userUserClaim,
                                                       _mockService.Object
                                                       , mockManageBlueBook.Object);


            // Act
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            string response = manageEnterpriseRolesPrimaryProperies.ProcessEnterpriseRolesAndPrimaryPropertiesData(_editorPersonaId, _userPersonaId, _enterPriseRoleId, _enterpriseRoleCreatedDate, _bulkAddEnterpriseRoleBatchProcessTypeId, true);
            bool isBatchProcessProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                  .Equals("Batch.CreateProductBatch", StringComparison.OrdinalIgnoreCase)));

            bool isPropertyInstanceProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                        .Equals("Enterprise.GetPropertyInstanceByPersonaId", StringComparison.OrdinalIgnoreCase)));

            bool isEnterpriseRoleNewProductsProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                                  .Equals("Security.GetEnterpriseRoleNewProductsByRoleTemplateId", StringComparison.OrdinalIgnoreCase)));

            bool isEnterpriseRoleUpdatedProductsProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                                  .Equals("Security.GetEnterpriseRoleUpdatedProductsByRoleTemplateId", StringComparison.OrdinalIgnoreCase)));

            bool isEnterpriseRoleDeletedProductsProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                                  .Equals("Security.GetEnterpriseRoleDeletedProductsByRoleTemplateId", StringComparison.OrdinalIgnoreCase)));

            //Assert

            Assert.False(isEnterpriseRoleNewProductsProcExecuted);
            Assert.False(isEnterpriseRoleUpdatedProductsProcExecuted);
            Assert.False(isEnterpriseRoleDeletedProductsProcExecuted);
            Assert.False(isPropertyInstanceProcExecuted);
            Assert.True(_mockHttpMessageHandler.Invocations.Count == 0);
            Assert.True(_mockService.Invocations.Count == 0);
            Assert.True(isBatchProcessProcExecuted);
        }


        /// <summary>
        /// Bulk Assign Enterprise Role
        /// </summary>
        [Fact]
        public void EnterpriseRoles_TurnOffPrimaryPropertiesForAssignProduct()
        {

            FillInstanceData();

            _mockRepository.Setup(m => m.GetMany<PersonaProductUserDetails>(StoredProcNameConstants.SP_ListProductsByPersonaId, It.Is<object>(d => TestSqlParameterContains(d, _userPersonaId.ToString()))))
            .Returns(GetPersonaProductsForUnassign());

            _mockRepository
            .Setup(m => m.GetMany<ProductSettingList>(StoredProcNameConstants.SP_ListProductSettingsByPersonaId, It.IsAny<object>()))
            .Returns(GetUsePrimaryPropertiesSettingTurnOn());

            _mockRepository.Setup(m => m.GetMany<Setting>(StoredProcNameConstants.SP_GetUnifiedSetting, It.IsAny<object>()))
            .Returns(GetPrimaryPropertyEnterpriseRoleAtOrganizationLevelTurnOff());


            _mockRepository.Setup(m => m.GetMany<ProductSettingList>(StoredProcNameConstants.SP_ListProductSettingsByOrganization,
            It.IsAny<object>())).Returns(GetUsePrimaryPropertiesSettingTurnOffAtProductLevel());

            _mockHttpMessageHandler.Setup(HttpMethod.Post, $"http://localhost/translate/v3/propertyinstance/UPFM/OS", GetTranslatePropertyInstanceHttpResponse());

            _mockRepository.Setup(m => m.GetMany<int>(StoredProcNameConstants.SP_GetEnterpriseRoleNewProductsByRoleTemplateId, It.IsAny<object>()))
            .Returns(new List<int>() { 1 });

            ManageEnterpriseRolesPrimaryProperties manageEnterpriseRolesPrimaryProperies = new ManageEnterpriseRolesPrimaryProperties(
                                                        _mockRepository.Object
                                                      , _mockHttpMessageHandler.Object
                                                      , _userUserClaim, _mockService.Object, mockManageBlueBook.Object);

            // Act
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            string response = manageEnterpriseRolesPrimaryProperies.ProcessEnterpriseRolesAndPrimaryPropertiesData(_editorPersonaId, _userPersonaId, _enterPriseRoleId, _enterpriseRoleCreatedDate, _bulkAddEnterpriseRoleBatchProcessTypeId);

            bool isBatchProcessProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                              .Equals("Batch.CreateProductBatch", StringComparison.OrdinalIgnoreCase)));

            bool isPropertyInstanceProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                                  .Equals("Enterprise.GetPropertyInstanceByPersonaId", StringComparison.OrdinalIgnoreCase)));

            bool isEnterpriseRoleNewProductsProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                                  .Equals("Security.GetEnterpriseRoleNewProductsByRoleTemplateId", StringComparison.OrdinalIgnoreCase)));

            bool isEnterpriseRoleUpdatedProductsProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                                  .Equals("Security.GetEnterpriseRoleUpdatedProductsByRoleTemplateId", StringComparison.OrdinalIgnoreCase)));

            bool isEnterpriseRoleDeletedProductsProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                                  .Equals("Security.GetEnterpriseRoleDeletedProductsByRoleTemplateId", StringComparison.OrdinalIgnoreCase)));

            Assert.False(isEnterpriseRoleNewProductsProcExecuted);
            Assert.False(isEnterpriseRoleUpdatedProductsProcExecuted);
            Assert.False(isEnterpriseRoleDeletedProductsProcExecuted);
            Assert.False(isPropertyInstanceProcExecuted);
            Assert.False(isBatchProcessProcExecuted);
            Assert.True(_mockHttpMessageHandler.Invocations.Count == 0);
            Assert.True(_mockService.Invocations.Count == 0);
        }

        /// <summary>
        /// Bulk Enterprise Roles
        /// </summary>
        [Fact]
        public void EnterpriseRoles_TurnOffAllUsePrimeProperties_NonTranslatedPrimaryPropertyForAssign()
        {
            FillInstanceData();

            _mockRepository
            .Setup(m => m.GetMany<ProductSettingList>(StoredProcNameConstants.SP_ListProductSettingsByPersonaId, It.IsAny<object>()))
            .Returns(GetUsePrimaryPropertiesSettingTurnOn());

            _mockRepository.Setup(m => m.GetMany<Setting>(StoredProcNameConstants.SP_GetUnifiedSetting, It.IsAny<object>()))
            .Returns(GetPrimaryPropertyEnterpriseRoleAtOrganizationLevelTurnOn());

            _mockRepository.Setup(m => m.GetMany<ProductSettingList>(StoredProcNameConstants.SP_ListProductSettingsByOrganization,
            It.IsAny<object>())).Returns(GetUsePrimaryPropertiesSettingTurnOnAtProductLevel());

            _mockRepository.Setup(m => m.GetMany<int>(StoredProcNameConstants.SP_GetEnterpriseRoleNewProductsByRoleTemplateId, It.IsAny<object>()))
             .Returns(new List<int>() { 1 });

            _mockHttpMessageHandler.Setup(HttpMethod.Post, $"http://localhost/translate/v3/propertyinstance/UPFM/OS", GetNonTranslatePropertyInstanceHttpResponse());

            ManageEnterpriseRolesPrimaryProperties manageEnterpriseRolesPrimaryProperies = new ManageEnterpriseRolesPrimaryProperties(
                                                       _mockRepository.Object
                                                     , _mockHttpMessageHandler.Object
                                                     , _userUserClaim,
                                                       _mockService.Object
                                                       , mockManageBlueBook.Object);


            // Act
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            string response = manageEnterpriseRolesPrimaryProperies.ProcessEnterpriseRolesAndPrimaryPropertiesData(_editorPersonaId, _userPersonaId, _enterPriseRoleId, _enterpriseRoleCreatedDate, _bulkAddEnterpriseRoleBatchProcessTypeId);
            bool isBatchProcessProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                  .Equals("Batch.CreateProductBatch", StringComparison.OrdinalIgnoreCase)));

            bool isPropertyInstanceProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                        .Equals("Enterprise.GetPropertyInstanceByPersonaId", StringComparison.OrdinalIgnoreCase)));

            bool isEnterpriseRoleNewProductsProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                                  .Equals("Security.GetEnterpriseRoleNewProductsByRoleTemplateId", StringComparison.OrdinalIgnoreCase)));

            bool isEnterpriseRoleUpdatedProductsProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                                  .Equals("Security.GetEnterpriseRoleUpdatedProductsByRoleTemplateId", StringComparison.OrdinalIgnoreCase)));

            bool isEnterpriseRoleDeletedProductsProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                                  .Equals("Security.GetEnterpriseRoleDeletedProductsByRoleTemplateId", StringComparison.OrdinalIgnoreCase)));

            //Assert

            Assert.False(isEnterpriseRoleNewProductsProcExecuted);
            Assert.False(isEnterpriseRoleUpdatedProductsProcExecuted);
            Assert.False(isEnterpriseRoleDeletedProductsProcExecuted);
            Assert.False(isPropertyInstanceProcExecuted);
            Assert.True(_mockHttpMessageHandler.Invocations.Count == 0);
            Assert.True(_mockService.Invocations.Count == 0);
            Assert.False(isBatchProcessProcExecuted);
        }


        [Fact]
        public void EnterpriseRole_TurnOnAllUsePrimePropertiesUnassign()
        {
            FillInstanceData();

            _mockRepository
            .Setup(m => m.GetMany<ProductSettingList>(StoredProcNameConstants.SP_ListProductSettingsByPersonaId, It.IsAny<object>()))
            .Returns(GetUsePrimaryPropertiesSettingTurnOn());

            _mockRepository
            .Setup(m => m.GetMany<RoleTemplateProductRole>(StoredProcNameConstants.SP_GetRoleTemplateProductRoleMappings, It.IsAny<object>()))
            .Returns(GetRoleTemplateProductOneSiteRoleList());

            _mockRepository.Setup(m => m.GetMany<Setting>(StoredProcNameConstants.SP_GetUnifiedSetting, It.IsAny<object>()))
            .Returns(GetPrimaryPropertyEnterpriseRoleAtOrganizationLevelTurnOn());

            _mockRepository.Setup(m => m.GetMany<ProductSettingList>(StoredProcNameConstants.SP_ListProductSettingsByOrganization,
            It.IsAny<object>())).Returns(GetUsePrimaryPropertiesSettingTurnOnAtProductLevel());

            _mockHttpMessageHandler.Setup(HttpMethod.Post, $"http://localhost/translate/v3/propertyinstance/UPFM/OS", GetTranslatePropertyInstanceHttpResponse());



            _mockRepository.Setup(m => m.GetMany<int>(StoredProcNameConstants.SP_GetEnterpriseRoleNewProductsByRoleTemplateId, It.IsAny<object>()))
            .Returns(new List<int>() { 1 });

            ManageEnterpriseRolesPrimaryProperties manageEnterpriseRolesPrimaryProperies = new ManageEnterpriseRolesPrimaryProperties(
                                                        _mockRepository.Object
                                                      , _mockHttpMessageHandler.Object
                                                      , _userUserClaim,
                                                        _mockService.Object
                                                        , mockManageBlueBook.Object);


            // Act
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();


            string response = manageEnterpriseRolesPrimaryProperies.ProcessEnterpriseRolesAndPrimaryPropertiesData(_editorPersonaId, _userPersonaId, _enterPriseRoleId, _enterpriseRoleCreatedDate , _bulkAddEnterpriseRoleBatchProcessTypeId );

            bool isBatchProcessProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                  .Equals("Batch.CreateProductBatch", StringComparison.OrdinalIgnoreCase)));

            bool isPropertyInstanceProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                        .Equals("Enterprise.GetPropertyInstanceByPersonaId", StringComparison.OrdinalIgnoreCase)));

            bool isEnterpriseRoleNewProductsProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                                  .Equals("Security.GetEnterpriseRoleNewProductsByRoleTemplateId", StringComparison.OrdinalIgnoreCase)));

            bool isEnterpriseRoleUpdatedProductsProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                                  .Equals("Security.GetEnterpriseRoleUpdatedProductsByRoleTemplateId", StringComparison.OrdinalIgnoreCase)));

            bool isEnterpriseRoleDeletedProductsProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                                  .Equals("Security.GetEnterpriseRoleDeletedProductsByRoleTemplateId", StringComparison.OrdinalIgnoreCase)));

            bool isListProductsByPersonaIdProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                              .Equals("Enterprise.ListProductsByPersonaId", StringComparison.OrdinalIgnoreCase)));
            Assert.False(isListProductsByPersonaIdProcExecuted);
            Assert.False(isEnterpriseRoleNewProductsProcExecuted);
            Assert.False(isEnterpriseRoleUpdatedProductsProcExecuted);
            Assert.False(isEnterpriseRoleDeletedProductsProcExecuted);
            Assert.True(isPropertyInstanceProcExecuted);
            Assert.True(_mockHttpMessageHandler.Invocations.Count > 0);
            Assert.True(_mockService.Invocations.Count > 0);
            Assert.True(isBatchProcessProcExecuted);
        }


        /// <summary>
        /// Bulk Enterprise Roles
        /// </summary>
        [Fact]
        public void EnterpriseRoles_TurnOnAllUsePrimeProperties_IncludeAdminSupportPortalProduct()
        {
            FillInstanceData();

            _mockRepository
            .Setup(m => m.GetMany<ProductSettingList>(StoredProcNameConstants.SP_ListProductSettingsByPersonaId, It.IsAny<object>()))
            .Returns(GetUsePrimaryPropertiesSettingTurnOn());

            _mockRepository.Setup(m => m.GetMany<PersonaProductUserDetails>(StoredProcNameConstants.SP_ListProductsByPersonaId, It.Is<object>(d => TestSqlParameterContains(d, _userPersonaId.ToString()))))
           .Returns(GetProductIncludingAdminSupportPortal());

            _mockRepository.Setup(m => m.GetMany<SamlAttributes>(StoredProcNameConstants.SP_GetProductSamlDetails,
            It.IsAny<object>())).Returns(GetAdminSupportPortalSaml());

            _mockRepository.Setup(m => m.GetMany<Setting>(StoredProcNameConstants.SP_GetUnifiedSetting, It.IsAny<object>()))
            .Returns(GetPrimaryPropertyEnterpriseRoleAtOrganizationLevelTurnOn());

            _mockRepository.Setup(m => m.GetMany<ProductSettingList>(StoredProcNameConstants.SP_ListProductSettingsByOrganization,
            It.IsAny<object>())).Returns(GetUsePrimaryPropertiesSettingTurnOnAtProductLevel());

            _mockRepository.Setup(m => m.GetMany<int>(StoredProcNameConstants.SP_GetEnterpriseRoleNewProductsByRoleTemplateId, It.IsAny<object>()))
             .Returns(new List<int>() { 1 });

            _mockHttpMessageHandler.Setup(HttpMethod.Post, $"http://localhost/translate/v3/propertyinstance/UPFM/OS", GetNonTranslatePropertyInstanceHttpResponse());

            ManageEnterpriseRolesPrimaryProperties manageEnterpriseRolesPrimaryProperies = new ManageEnterpriseRolesPrimaryProperties(
                                                       _mockRepository.Object
                                                     , _mockHttpMessageHandler.Object
                                                     , _userUserClaim,
                                                       _mockService.Object
                                                       , mockManageBlueBook.Object);


            // Act
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            string response = manageEnterpriseRolesPrimaryProperies.ProcessEnterpriseRolesAndPrimaryPropertiesData(_editorPersonaId, _userPersonaId, _enterPriseRoleId, _enterpriseRoleCreatedDate, _bulkAddEnterpriseRoleBatchProcessTypeId, true);
            bool isBatchProcessProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                  .Equals("Batch.CreateProductBatch", StringComparison.OrdinalIgnoreCase)));

            bool isPropertyInstanceProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                        .Equals("Enterprise.GetPropertyInstanceByPersonaId", StringComparison.OrdinalIgnoreCase)));

            bool isEnterpriseRoleNewProductsProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                                  .Equals("Security.GetEnterpriseRoleNewProductsByRoleTemplateId", StringComparison.OrdinalIgnoreCase)));

            bool isEnterpriseRoleUpdatedProductsProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                                  .Equals("Security.GetEnterpriseRoleUpdatedProductsByRoleTemplateId", StringComparison.OrdinalIgnoreCase)));

            bool isEnterpriseRoleDeletedProductsProcExecuted = _mockRepository.Invocations.Select(m => m.Arguments).Any(s => (s.Count > 0 && s[0].ToString().Replace("[", "").Replace("]", "")
                                                  .Equals("Security.GetEnterpriseRoleDeletedProductsByRoleTemplateId", StringComparison.OrdinalIgnoreCase)));

            //Assert

            Assert.False(isEnterpriseRoleNewProductsProcExecuted);
            Assert.False(isEnterpriseRoleUpdatedProductsProcExecuted);
            Assert.False(isEnterpriseRoleDeletedProductsProcExecuted);
            Assert.False(isPropertyInstanceProcExecuted);
            Assert.True(_mockHttpMessageHandler.Invocations.Count == 0);
            Assert.True(_mockService.Invocations.Count == 0);
            Assert.True(isBatchProcessProcExecuted);
        }
        public ManageEnterpriseRolesPrimaryPropertiesTest() : base((int)ProductEnum.UnifiedPlatform)
        {
            //   _manageEnterpriseRolesPrimaryProperies = new ManageEnterpriseRolesPrimaryProperies(_userUserClaim);
            _mockRepository = new Mock<IRepository>();
            _productSettingType.Add(new ProductSettingType() { ProductSettingTypeId = 1, Name = "ProductStatus" });

            _repositoryResponseProductStatus = new RepositoryResponse() { ErrorMessage = "", Id = 1 };
            _repositoryResponsePropertySuccess = new RepositoryResponse() { ErrorMessage = "", Id = 1 };
            _repositoryResponsePropertyFail = new RepositoryResponse() { ErrorMessage = "error", Id = -1 };
            AssertInitial();
        }

        private IList<IC.ProductInternalSetting> GetOneSiteProductSettings()
        {
            string _mtApiEndPoint = "api/core/common/ulmigration";
            string _mtTokenUrl = "api/core/authentication/login";
            string _mtClientId = "OneSiteClient";
            string _mtClientSecret = "OneSiteClientSecret";
            IList<IC.ProductInternalSetting> _productInternalSettingsOneSite = new List<IC.ProductInternalSetting>();
            _productInternalSettingsOneSite.Add(new IC.ProductInternalSetting() { Name = "MTAPiEndPoint", Value = _mtApiEndPoint });
            _productInternalSettingsOneSite.Add(new IC.ProductInternalSetting() { Name = "MTTokenEndPoint", Value = _mtTokenUrl });
            _productInternalSettingsOneSite.Add(new IC.ProductInternalSetting() { Name = "MTClientId", Value = _mtClientId });
            _productInternalSettingsOneSite.Add(new IC.ProductInternalSetting() { Name = "MTClientSECRET", Value = _mtClientSecret });
            _productInternalSettingsOneSite.Add(new IC.ProductInternalSetting() { Name = "BooksUseDomains", Value = "1" });
            _productInternalSettingsOneSite.Add(new IC.ProductInternalSetting() { Name = "BooksUseUPFMId", Value = "1" });
            _productInternalSettingsOneSite.Add(new IC.ProductInternalSetting() { Name = "BooksUseTranslatev2", Value = "1" });
            _productInternalSettingsOneSite.Add(new IC.ProductInternalSetting() { Name = "UsePrimaryProperties", Value = "1" });
            return _productInternalSettingsOneSite;
        }

        private RoleTemplate GetRoleTemplate()
        {
            return new RoleTemplate() { PersonaId = 33, RoleTemplateId = 8569, RoleTemplateName = "Role Template Test" };
        }

        private List<RoleTemplateProductRole> GetroleTemplateProductRoleList()
        {
            return new List<RoleTemplateProductRole>() {
            new RoleTemplateProductRole() { RoleTemplateName= "Role Template Test" , RoleTemplateId = 8569, ProductId = 57, PartyId = _userOrganizationPartyId , RoleTemplateProductId= 6358,
                ProductName = "Property Manager"  } ,
              new RoleTemplateProductRole() { RoleTemplateName= "Role Template Test" , RoleTemplateId = 8569, ProductId = 57, PartyId = _userOrganizationPartyId , RoleTemplateProductId= 6359,
                ProductName = "Portfolio Manager"  }
            };
        }

        private List<RoleTemplateProductRole> GetRoleTemplateProductOneSiteRoleList()
        {
            return new List<RoleTemplateProductRole>() {
            new RoleTemplateProductRole() { RoleTemplateName= "Role Template Test" , RoleTemplateId = 8569, ProductId = 1, PartyId = _userOrganizationPartyId , RoleTemplateProductId= 6358,
                ProductName = "Property Manager"  } ,
              new RoleTemplateProductRole() { RoleTemplateName= "Role Template Test" , RoleTemplateId = 8569, ProductId = 1, PartyId = _userOrganizationPartyId , RoleTemplateProductId= 6359,
                ProductName = "Portfolio Manager"  }
            };
        }

        private List<PersonaProductUserDetails> GetPersonaProducts()
        {
            List<PersonaProductUserDetails> assignedProducts = new List<PersonaProductUserDetails>();
            assignedProducts.Add(new PersonaProductUserDetails
            {
                PersonaId = _userPersonaId,
                ProductId = 1,
                ProductName = "OneSite"
            });
            return assignedProducts;
        }

        private List<PersonaProductUserDetails> GetPersonaProductsForUnassign()
        {
            List<PersonaProductUserDetails> assignedProducts = new List<PersonaProductUserDetails>();
            assignedProducts.Add(new PersonaProductUserDetails
            {
                PersonaId = _userPersonaId,
                ProductId = 3,
                ProductName = ProductEnum.UnifiedPlatform.ToString()
            });
            assignedProducts.Add(new PersonaProductUserDetails
            {
                PersonaId = _userPersonaId,
                ProductId = 4,
                ProductName = ProductEnum.AssetOptimizer.ToString()
            });
            return assignedProducts;
        }

        private List<SamlAttributes> GetAdminSupportPortalSaml()
        {
            return new List<SamlAttributes> { new SamlAttributes() { DisplayName = "Admin & Support Portal" , SamlAttributeId = 3, Name= "Admin & Support Portal" }  };

        }


        private List<PersonaProductUserDetails> GetProductIncludingAdminSupportPortal()
        {
            List<PersonaProductUserDetails> assignedProducts = new List<PersonaProductUserDetails>();
            assignedProducts.Add(new PersonaProductUserDetails
            {
                PersonaId = _userPersonaId,
                ProductId = 3,
                ProductName = ProductEnum.UnifiedPlatform.ToString()
            });
            assignedProducts.Add(new PersonaProductUserDetails
            {
                PersonaId = _userPersonaId,
                ProductId = 4,
                ProductName = ProductEnum.AssetOptimizer.ToString()
            });
            assignedProducts.Add(new PersonaProductUserDetails
            {
                PersonaId = _userPersonaId,
                ProductId = 89,
                ProductName = ProductEnum.AdminSupportPortal.ToString()
            });
            return assignedProducts;
        }


        private List<ProductSettingType> GetProductSettingTypeList()
        {
            return new List<ProductSettingType>() { new ProductSettingType() {
                ProductSettingTypeId= 1120,Name= "ProductIntegrationType",Description= "Defines the integration that a specific product is to use" }
            };
        }

        private OC.PropertyList GetOneSitePropertyList()
        {
            OC.PropertyList propertyResultList = new OC.PropertyList();
            //List<string> _propertiesToAdd = new List<string>();
            //_propertiesToAdd.Add("All");

            List<OC.PropertyType> _propList = new List<OC.PropertyType>();
            _propertyType1.PropertyID = "384092-IB";
            _propList.Add(_propertyType1); // already assigned
            _propList.Add(_propertyType2); // not assigned
            _propList.Add(_propertyType3); // already assigned
            propertyResultList.Property = _propList.ToArray();
            propertyResultList.TotalProperties = _propList.Count;
            return propertyResultList;
        }

        private OC.PropertyList GetOneSiteOneTransaltedPropertyList()
        {
            OC.PropertyList propertyResultList = new OC.PropertyList();
            //List<string> _propertiesToAdd = new List<string>();
            //_propertiesToAdd.Add("All");

            List<OC.PropertyType> _propList = new List<OC.PropertyType>();
            _propertyType1.PropertyID = "384092-IB";
            _propList.Add(_propertyType1); // already assigned
            _propList.Add(_propertyType2); // not assigned
            _propList.Add(_propertyType3); // already assigned
            propertyResultList.Property = _propList.ToArray();
            propertyResultList.TotalProperties = _propList.Count;
            return propertyResultList;
        }

        private HttpResponseMessage GetHttpResourceMapResponseMessage()
        {
            TranslateCompanyInstance translate = new TranslateCompanyInstance()
            {
                Data = new TranslateCompanyInstanceData()
                {
                    Type = "companyinstanceids",
                    Attributes = new TranslateCompanyInstanceAttributes()
                    {
                        Source = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform),
                        CompanyInstanceSourceId = GetOrganizationList()[0].RealPageId.ToString(),
                        TranslatedCompanyInstances = new List<TranslatedCompanyInstanceData>()
                        {
                            new TranslatedCompanyInstanceData()
                            {
                                Source = ProductEnumHelper.StringValueOf(ProductEnum.OneSite),
                                CompanyInstanceSourceId = "1051412",
                                CustomerEnvironment = "Primary",
                                Domain = "Primary"
                            }
                        }
                    }
                }
            };

            HttpResponseMessage responseMapResource = new HttpResponseMessage(HttpStatusCode.OK);
            var jsonToSave = JsonConvert.SerializeObject(translate);
            responseMapResource.Content = new StringContent(jsonToSave);
            return responseMapResource;
        }

        private HttpResponseMessage GetHttpPropertyInstanceResponseMessage()
        {
            List<PropertyInstance> propertyInstances = new List<PropertyInstance>()
            {
                new PropertyInstance() {PropertyInstanceSourceId = "cb1f5a51-56cc-415c-9d8e-3d5e3f0f8b68"},
                new PropertyInstance() {PropertyInstanceSourceId = "b6f475fc-7408-424b-a749-129035dcf57b"},
                new PropertyInstance() {PropertyInstanceSourceId = "a61481fc-5779-4546-8d5a-b29ecf139095"},
                new PropertyInstance() {PropertyInstanceSourceId = "d0ab0e33-4c04-4028-97f8-cda5a8423a30"},
            };

            UPFMPropertyInstanceRootObject propertyInstanceRoot = new UPFMPropertyInstanceRootObject()
            {
                data = new List<UPFMPropertyInstanceData>()
            { new UPFMPropertyInstanceData() { attributes = new UPFMPropertyInstanceAttributes() { propertyInstance = propertyInstances } } }
            };

            var jsonToSave = JsonConvert.SerializeObject(propertyInstanceRoot);
            HttpResponseMessage responsePropertyInstance = new HttpResponseMessage(HttpStatusCode.OK);
            responsePropertyInstance.Content = new StringContent(jsonToSave);
            return responsePropertyInstance;
        }

        private List<CustomerCompanyMap> GetCustomerCompanyMapList()
        {
            int _blueBookId = 123;
            string _companyInstanceSourceId = "123456";

            return new List<CustomerCompanyMap>()
            {
                new CustomerCompanyMap()
                {
                    CustomerCompanyId = _blueBookId,
                    CompanyInstanceSourceId = _companyInstanceSourceId,
                    Source = "ONST"
                }
            };
        }

        private List<Organization> GetOrganizationList()
        {
            Guid realPageId = new Guid("523c6677-c20d-4e6a-a4cc-0de5781f0d5c");
            string companyName = "CF Real Estate Services";
            DateTime createDate = DateTime.MaxValue.ToUniversalTime();
            int partyId = 54321;
            long booksMasterId = 2116;
            long booksCompanyMasterId = 379;
            int organizationTypeId = 6;

            return new List<Organization>()
            {
                new Organization()
                {
                    RealPageId = realPageId,
                    CreateDate = createDate,
                    Name = companyName,
                    PartyId = partyId,
                    BooksMasterId = booksMasterId,
                    BooksCustomerMasterId = booksCompanyMasterId,
                    organizationType = new OrganizationType()
                    {
                        OrganizationTypeId = organizationTypeId,
                        Name = "Multifamily",
                        CreateDate = new DateTime()
                    },
                    OrganizationTypeId = organizationTypeId,
                    OrganizationDomainId = 1,
                    OrganizationDomain = new OrganizationDomain()
                    {
                        OrganizationDomainId = 1,
                        Name = "Primary",
                        CreateDate = new DateTime()
                    },
                }
            };
        }

        private List<UPFMPropertyInstance> GetOneSiteUPFMPropertyInstanceList()
        {
            return new List<UPFMPropertyInstance>() {
            new UPFMPropertyInstance() {
              PropertyInstanceId = 72773,
              Name = "Adams Station T",
              Address = "2201 Lakeside Blvd",
              City= "Richardson",
              State="TX",
              County ="Dollas",
              Latitude = 32.979891M,
              Longitude = -96.712000M,
              InstanceId= new Guid("8C4176E6-C0F1-4878-9631-951F7C8A6A91"),
              PostalCode= "75082",
              Country= "United States of America"
            },
              new UPFMPropertyInstance() {
              PropertyInstanceId = 72773,
              Name = "Adams Station T",
              Address = "2201 Lakeside Blvd",
              City= "Richardson",
              State="TX",
              County ="Dollas",
              Latitude = 35.979891M,
              Longitude = -97.712000M,
              InstanceId= new Guid("96F86D50-D6EF-4B9D-AB7D-FAC5D5D06A3B"),
              PostalCode= "75088",
              Country= "United States of America"
            }
            };
        }

        private List<OrganizationDomain> GetOrganizationDomainList()
        {
            return new List<OrganizationDomain>()
            {
                new OrganizationDomain()
                {
                    OrganizationDomainId = 1,
                    Name = "Primary",
                    CreateDate = new DateTime()
                }
            };
        }

        private List<ProductInternalSettingByType> GetProductIntegrationType()
        {
            return new List<ProductInternalSettingByType>()
            {
                new ProductInternalSettingByType
                {
                    ProductConfigurationId = "233399",
                    Name = "ProductIntegrationType",
                    Value = "UPFM",
                    ProductId = 57,
                    ProductName = "Smart Waste",
                    BooksProductCode = "SMS-T"
                },
                new ProductInternalSettingByType
                {
                    ProductConfigurationId = "78",
                    Name = "ProductIntegrationType",
                    Value = "Legacy",
                    ProductId = 1,
                    ProductName = "OneSite",
                    BooksProductCode = "OS"
                },
            };
        }

        private List<ProductInternalSettingByType> GetProductInternalSettingByType()
        {
            return new List<ProductInternalSettingByType>()
            {
                 new ProductInternalSettingByType
                {
                    ProductConfigurationId = "233399",
                    Name = "ProductIntegrationType",
                    Value = "UPFM",
                    ProductId = 57,
                    ProductName = "Smart Waste",
                    BooksProductCode = "SMS-T"
                },
                new ProductInternalSettingByType
                {
                    ProductConfigurationId = "78",
                    Name = "ProductIntegrationType",
                    Value = "Legacy",
                    ProductId = 1,
                    ProductName = "OneSite",
                    BooksProductCode = "OS"
                },
                new ProductInternalSettingByType
                {
                    ProductConfigurationId = "1234",
                    Name = "UsePrimaryProperties",
                    Value = "1",
                    ProductId = 1,
                    ProductName = "OneSite",
                    BooksProductCode = "OS"
                },
                new ProductInternalSettingByType
                {
                    ProductConfigurationId = "1264",
                    Name = "UsePrimaryProperties",
                    Value = "1",
                    ProductId = 57,
                    ProductName = "Smart Waste",
                    BooksProductCode = "SMS-T"
                }
            };
        }

        private OC.RoleList GetOnsiteRoles()
        {
            OC.RoleList resultList = new OC.RoleList();
            List<OC.RoleType> _onesiteroleList = new List<OC.RoleType>();
            _onesiteroleList.Add(_roleType1);
            _onesiteroleList.Add(_roleType2);
            resultList.Role = _onesiteroleList.ToArray();
            resultList.TotalRoles = _onesiteroleList.Count;
            return resultList;
        }

        private HttpResponseMessage GetTranslatePropertyInstanceHttpResponse()
        {
            TranslatePropertyInstance translatedData = new TranslatePropertyInstance()
            {
                Data = new TranslatePropertyInstanceData()
                {
                    Type = "propertyinstancetranslations",
                    Attributes = new List<TranslatePropertyInstanceAttribute>()
                    {
                        new TranslatePropertyInstanceAttribute() {
                        PropertyInstanceSourceId = "8c4176e6-c0f1-4878-9631-951f7c8a6a91",
                         Source ="OS",
                          TranslatedPropertyInstances = new List<TranslatedPropertyInstanceData>()
                          {
                              new TranslatedPropertyInstanceData() {  Source ="OS", PropertyInstanceSourceId = "384092-IB" }

                          }
                          },
                        new TranslatePropertyInstanceAttribute() {
                        PropertyInstanceSourceId = "fcf0700c-639f-4dea-a88e-10727888054f",
                         Source ="UPFM",
                          TranslatedPropertyInstances = new List<TranslatedPropertyInstanceData>()
                          {
                              new TranslatedPropertyInstanceData() {  Source ="IB", PropertyInstanceSourceId = "384115-IB" }
                          }
                      }
                }
                }
            };

            HttpResponseMessage responseMapResourcePropertySave = new HttpResponseMessage(HttpStatusCode.OK);
            var jsonPropertySave = JsonConvert.SerializeObject(translatedData);
            responseMapResourcePropertySave.Content = new StringContent(jsonPropertySave);

            return responseMapResourcePropertySave;
        }

        private HttpResponseMessage GetNonTranslatePropertyInstanceHttpResponse()
        {
            TranslatePropertyInstance translatedData = new TranslatePropertyInstance()
            {
                Data = new TranslatePropertyInstanceData()
                {
                    Type = "propertyinstancetranslations",
                    Attributes = new List<TranslatePropertyInstanceAttribute>()
                    {
                        new TranslatePropertyInstanceAttribute() {
                        PropertyInstanceSourceId = "eab12950-afa1-47bb-92bb-9993a1de336e",
                         Source ="OS",
                          TranslatedPropertyInstances = new List<TranslatedPropertyInstanceData>()
                          {
                              new TranslatedPropertyInstanceData() {  Source ="OS", PropertyInstanceSourceId = "384092-IB" }

                          }
                          },
                        new TranslatePropertyInstanceAttribute() {
                        PropertyInstanceSourceId = "fcf0700c-639f-4dea-a88e-10727888054f",
                         Source ="UPFM",
                          TranslatedPropertyInstances = new List<TranslatedPropertyInstanceData>()
                          {
                              new TranslatedPropertyInstanceData() {  Source ="OS", PropertyInstanceSourceId = "384115-IB" }
                          }
                      }
                }
                }
            };
            HttpResponseMessage responseMapResourcePropertySave = new HttpResponseMessage(HttpStatusCode.OK);
            var jsonPropertySave = JsonConvert.SerializeObject(translatedData);
            responseMapResourcePropertySave.Content = new StringContent(jsonPropertySave);
            return responseMapResourcePropertySave;
        }

        private List<Setting> GetPrimaryPropertyEnterpriseRoleAtOrganizationLevelTurnOn()
        {
            return new List<Setting>()
            {
                 new Setting()
                {
                    Name = "PrimaryProperty",
                    Value = "1",
                    Editable = true,
                    Hidden = false
                },
            };
        }

        private List<Setting> GetPrimaryPropertyEnterpriseRoleAtOrganizationLevelTurnOff()
        {
            return new List<Setting>()
            {
                 new Setting()
                {
                    Name = "PrimaryProperty",
                    Value = "0",
                    Editable = true,
                    Hidden = false
                },
            };
        }

        private List<ProductSettingList> GetUsePrimaryPropertiesSettingTurnOn()
        {
            return new List<ProductSettingList>() {
                   new ProductSettingList() { ProductId = 1, Name = "ProductStatus", Value = "8" },
                   new ProductSettingList() { ProductId = 57, Name = "ProductStatus", Value = "8" },
                   new ProductSettingList() { ProductId = 1, Name = "ShowInUserDetails", Value = "1" },
                   new ProductSettingList() {ProductId = 1, Name = "OVERRIDEPMCID", Value = "1234567" },
                   new ProductSettingList() { ProductId = 57, Name = "UsePrimaryProperties", Value = "0" },
                   new ProductSettingList() { ProductId = 1, Name = "UsePrimaryProperties", Value = "1" }};
        }

        private List<ProductSettingList> GetUsePrimaryPropertiesSettingTurnOff()
        {
            return new List<ProductSettingList>() {
             new ProductSettingList() { ProductId = 1, Name = "ProductStatus", Value = "8" },
             new ProductSettingList() { ProductId = 1, Name = "ProductStatus", Value = "8" },
             new ProductSettingList() { ProductId = 1, Name = "ShowInUserDetails", Value = "1" },
             new ProductSettingList() {ProductId = 1, Name = "OVERRIDEPMCID", Value = "1234567" },
             new ProductSettingList() { ProductId = 1, Name = "UsePrimaryProperties", Value = "0" },
             new ProductSettingList() { ProductId = 57, Name = "UsePrimaryProperties", Value = "0" }};
        }

        private List<ProductSettingList> GetUsePrimaryPropertiesSettingTurnOnAtProductLevel()
        {
            return new List<ProductSettingList>() {
             new ProductSettingList() { ProductId = 1, Name = "ProductStatus", Value = "8" },
             new ProductSettingList() { ProductId = 1, Name = "ProductStatus", Value = "8" },
             new ProductSettingList() { ProductId = 1, Name = "ShowInUserDetails", Value = "1" },
             new ProductSettingList() { ProductId = 1, Name = "OVERRIDEPMCID", Value = "1234567" },
             new ProductSettingList() { ProductId = 57, Name = "UsePrimaryProperties", Value = "0" },
             new ProductSettingList() { ProductId = 1, Name = "UsePrimaryProperties", Value = "1" }};
        }

        private List<ProductSettingList> GetUsePrimaryPropertiesSettingTurnOffAtProductLevel()
        {
            return new List<ProductSettingList>() {
             new ProductSettingList() { ProductId = 1, Name = "ProductStatus", Value = "8" },
             new ProductSettingList() { ProductId = 1, Name = "ProductStatus", Value = "8" },
             new ProductSettingList() { ProductId = 1, Name = "ShowInUserDetails", Value = "1" },
             new ProductSettingList() {ProductId = 1, Name = "OVERRIDEPMCID", Value = "1234567" },
             new ProductSettingList() { ProductId = 1, Name = "UsePrimaryProperties", Value = "0" },
             new ProductSettingList() { ProductId = 57, Name = "UsePrimaryProperties", Value = "0" }
            };
        }

        public bool TestSqlParameter(object p, string value)
        {
            return value.Equals(p.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public bool TestSqlParameterContains(object p, string value)
        {
            return p.ToString().ToLower().Contains(value.ToLower());
        }
    }
}
