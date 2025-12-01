using Moq;
using Newtonsoft.Json;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.RPDocumentManagement;
using UnifiedLogin.SharedObjects.Saml;
using UnifiedLogin.LandingAPI.Test.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using Xunit;
using IC = UnifiedLogin.SharedObjects.IdentityConfig;

namespace UnifiedLogin.LandingAPI.Test.Logic.Product
{
    /// <summary>
    /// Product xUnit tests
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ManageRPDocumentManagementTests : ManageProductBaseTests
    {
        private Mock<IProductRepository> _mockProductRepository = new Mock<IProductRepository>();
        private Mock<ISamlRepository> _mockSamlRepository = new Mock<ISamlRepository>();
        private Mock<IManageBlueBook> _mockManageBlueBook = new Mock<IManageBlueBook>();
        private Mock<IProductInternalSettingRepository> _mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
        private Mock<IManagePersona> _mockManagePersona = new Mock<IManagePersona>();
        private Mock<IManagePerson> _mockManagePerson = new Mock<IManagePerson>();
        private Mock<IManageUserLogin> _mockManageUserLogin = new Mock<IManageUserLogin>();
        private Mock<IManageContactMechanism> _mockManageContactMechanism = new Mock<IManageContactMechanism>();
        private Mock<IManagePartyRelationship> _mockManagePartyRelationship = new Mock<IManagePartyRelationship>();
        private Mock<IUserLoginRepository> _mockUserLoginRepository = new Mock<IUserLoginRepository>();

        private static string _domain = "demoapi";
        private static string _companyName = "TESTCOMPANY";
        private static string _companyInstanceSourceId = "demoapi";
        private static string _userId = "1234567";

        private static string _apiEndPoint = "https://{{domain}}-test.com";
        private IList<CustomerCompanyMap> _mapCompany = new List<CustomerCompanyMap>();
       
        private RPDMClassifier _classifierResult;
        private RPDMRoleDetail _role1Detail;
        private RPDMRoleDetail _role2Detail;
        private IC.Person _person = new IC.Person();
        private UserLoginOnly _userlogin = new UserLoginOnly();
        private List<CommonAddress> _commonAddressList = new List<CommonAddress>();
        private IC.PartyRelationship _partyRelationShipSuperUser = new IC.PartyRelationship();
        private IC.PartyRelationship _partyRelationShipRegularUser = new IC.PartyRelationship();

        public ManageRPDocumentManagementTests() : base((int)ProductEnum.RPDocumentManagement)
        {
            _productSettingType.Add(new ProductSettingType() { ProductSettingTypeId = 1, Name = "ProductStatus" });

            _repositoryResponseProductStatus = new RepositoryResponse() { ErrorMessage = "", Id = 1 };
            _repositoryResponsePropertySuccess = new RepositoryResponse() { ErrorMessage = "", Id = 1 };
            _repositoryResponsePropertyFail = new RepositoryResponse() { ErrorMessage = "error", Id = -1 };
        }

        /// <summary>
        /// used to initialize unit tests
        /// </summary>
        private void AssertInitial()
        {
            _productInternalSettings.Add(new ProductInternalSetting() { Name = "APIENDPOINT", Value = _apiEndPoint });

            _mapCompany = new List<CustomerCompanyMap>()
            {
                new CustomerCompanyMap()
                {
                    CompanyInstanceSourceId = _companyInstanceSourceId,
                    Source = BlueBookProductConstants.RPDocumentManagement,
                    CompanyInstanceId = 1234,
                    CompanyInstance = new List<CompanyInstance>()
                    {
                        new CompanyInstance()
                        {
                            CompanyName = _companyName,
                            Attributes = new List<InstanceAttribute>() {new InstanceAttribute() {AttributeName = "Domain Id", AttributeValue = "demoapi"}}
                        }
                    }
                },
                new CustomerCompanyMap() {CompanyInstanceSourceId = _companyName, Source = BlueBookProductConstants.Insurance, CompanyInstanceId = 4321}
            };

            IList<GbProductMap> productList = new List<GbProductMap> { new GbProductMap { ProductId = 20, BooksProductCode = "DOC", Name = "Document Management" } };
            CustomerCompany customerCompany = new CustomerCompany() { CustomerCompanyId = 123456, IsActive = true, CompanyName = "Test Company", MigrationStatus = "migrated" };//Category = "rpup",

            RPDMResult<RPDMRole> roleResult = new RPDMResult<RPDMRole>();

            roleResult.Page = new List<RPDMRole>()
            {
                new RPDMRole() {HRef = "/demoapi/roles/70700", ID = "70700", Name = "Property Manager"},
                new RPDMRole() {HRef = "/demoapi/roles/70750", ID = "70750", Name = "Domain Admin"}
            };

            _role1Detail = new RPDMRoleDetail() { Id = "70700", Name = "Property Manager", Scope = new RPDMScope() { Id = "70700", Name = "Site Name", HRef = "/classifiers/852", Rel = "classifier" }, Type = "entity", Domain = "RealPage Demo" };
            _role2Detail = new RPDMRoleDetail() { Id = "70750", Name = "Domain Admin", Type = "domain", Domain = "RealPage Demo" };

            RPDMUser rpdmUser = new RPDMUser()
            {
                Id = _userId,
                Domain = _domain,
                Name = "user12",
                FirstName = "first name",
                LastName = "last name",
                Enabled = true,
                Roles = new List<RPDMUserRoles>()
                {
                    new RPDMUserRoles()
                    {
                        Role = new RPDMScope() {Id = "70700", Name = "Site Name", HRef = "/classifiers/852", Rel = "classifier"},
                        Entity = new RPDMScope() {Id = "73852", Name = "OCRTHighland Hills", HRef = "/onesitedm/demoapi/datasets/site name/values/71653", Rel = "value"}
                    }
                }
            };

            _classifierResult = new RPDMClassifier()
            {
                Id = "852",
                Domain = "RealPage Demo",
                DisplayName = "Site Name",
                Multiplicity = "M0N",
                Predicate = "Property",
                UniqueName = "property",
                DataSet = new RPDMDataset() { HRef = "/datasets/52453", Name = "sitename" }
            };

            RPDMResult<RPDMDataset> datasetResult = new RPDMResult<RPDMDataset>()
            {
                Page = new List<RPDMDataset>()
                {
                    new RPDMDataset() {Id = "73852", HRef = "/onesitedm/demoapi/datasets/site name/values/73852", Name = "Autumn Chasenzm", Rel = "value"},
                    new RPDMDataset() {Id = "71653", HRef = "/onesitedm/demoapi/datasets/site name/values/71653", Name = "OCRTHighland Hills", Rel = "value"}
                }
            };

            _person = new IC.Person() { FirstName = "Test First", LastName = "Test Last", PartyId = 30 };
            _userlogin = new UserLoginOnly() { UserId = _editorUserId, LoginName = "test", PartyId = 30, RealPageId = new Guid(), LastLogin = DateTime.Now };

            _editorSamlAttributes = new List<SamlAttributes>();
            _editorSamlAttributes.Add(new SamlAttributes() { Name = "UserId", Value = _userId });
            _editorSamlAttributes.Add(new SamlAttributes() { Name = "PRODUCTUSERNAME", Value = "bob12" });

            _userSamlAttributes = new List<SamlAttributes>();
            _userSamlAttributes.Add(new SamlAttributes() { Name = "UserId", Value = _userId });
            _userSamlAttributes.Add(new SamlAttributes() { Name = "PRODUCTUSERNAME", Value = (_person.FirstName.Substring(0, 1) + _person.LastName.Substring(0, (_person.LastName.Length >= 19 ? 19 : _person.LastName.Length))).ToLower().Replace(" ", "") });

            ContactMechanismUsageType contactMechanismUsageType = new ContactMechanismUsageType()
            {
                ContactMechanismUsageTypeId = 201,
                ParentContactMechanismUsageTypeId = 200,
                Name = "Primary"
            };

            CommonAddress commonAddress = new CommonAddress()
            {
                AddressString = "none@nowhere.com",
                AddressType = "Email",
                ContactMechanismId = 1,
                PartyContactMechanismId = 1,
                ContactMechanismUsageTypeId = 1,
                contactMechanismUsageType = contactMechanismUsageType
            };
            _commonAddressList.Add(commonAddress);

            _partyRelationShipSuperUser.RoleTypeFrom = new IC.RoleType() { Name = _ROLETYPE_NAME_SUPERUSER };
            _partyRelationShipRegularUser.RoleTypeFrom = new IC.RoleType() { Name = _ROLETYPE_NAME_USER };

            _mockManageBlueBook
                .Setup(m => m.GetCompanyMap(
                    It.IsAny<Guid>(),
                    It.IsAny<long>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()
                ))
                .Returns(_mapCompany);

            _mockProductRepository
                .Setup(m => m.GetBooksMasterProductDetail(
                    It.Is<int>(l => l == (int)ProductEnum.RPDocumentManagement)))
                .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.RPDocumentManagement));

            _mockProductRepository
                .Setup(m => m.ListProductSettingType(
                ))
                .Returns(_productSettingType);

            _mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.IsAny<int>()
                ))
                .Returns(_productInternalSettings);

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct,
                    It.Is<object>(d => TestIsProductId(d, (int)ProductEnum.RPDocumentManagement))))
                .Returns(_productInternalSettings);

            _mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 4)
                ))
                .Returns(_editorPersona);

            _mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 5)
                ))
                .Returns(_userPersona);

            _mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 7)
                ))
                .Returns(_newUserPersona);

            _mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 10)
                ))
                .Returns(_userInvalidPersona);

            _mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 33)
                ))
                .Returns(_nullPersona);

            _mockManagePerson
                .Setup(m => m.GetPerson(
                    It.IsAny<Guid>()
                ))
                .Returns(_person);

            _mockManageUserLogin
                .Setup(m => m.GetUserLoginOnly(
                    It.IsAny<Guid>()
                ))
                .Returns(_userlogin);

            _mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 4)
                    , It.IsAny<int>()
                ))
                .Returns(_editorSamlAttributes);

            _mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 5)
                    , It.IsAny<int>()
                ))
                .Returns(_userSamlAttributes);

            _mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => (l != 4 && l != 5))
                    , It.IsAny<int>()
                ))
                .Returns(new List<SamlAttributes>());

            _mockManageContactMechanism
                .Setup(m => m.ListContactMechanismForPerson(
                    It.Is<Guid>(l => l == _newUserPersona.RealPageId)
                    , It.IsAny<string>()
                ))
                .Returns(() => _commonAddressList);

            _mockManageContactMechanism
                .Setup(m => m.ListContactMechanismForPerson(
                    It.Is<Guid>(l => l != _newUserPersona.RealPageId)
                    , It.IsAny<string>()
                ))
                .Returns(() => new List<CommonAddress>());

            _mockManagePartyRelationship
                .Setup(m => m.GetPartyRelationship(
                    It.Is<Guid>(l => l == _newUserPersona.RealPageId)
                    , It.IsAny<Guid>()
                    , null
                    , null
                    , It.IsAny<string>()
                ))
                .Returns(_partyRelationShipSuperUser);

            _mockManagePartyRelationship
                .Setup(m => m.GetPartyRelationship(
                    It.Is<Guid>(l => l == _userPersona.RealPageId)
                    , It.IsAny<Guid>()
                    , null
                    , null
                    , It.IsAny<string>()
                ))
                .Returns(_partyRelationShipRegularUser);

            _mockUserLoginRepository
                .Setup(m => m.GetUserOrganizationWithStatus(
                    _editorPersona.UserId, It.IsAny<DateTime>(), It.IsAny<long>(), It.IsAny<bool>()))
                .Returns(_organizationStatusEditorPersona);

            _mockUserLoginRepository
                .Setup(m => m.GetUserOrganizationWithStatus(
                    _newUserPersona.UserId, It.IsAny<DateTime>(), It.IsAny<long>(), It.IsAny<bool>()))
                .Returns(_organizationStatusNewUserPersona);

            _mockUserLoginRepository
                .Setup(m => m.GetUserOrganizationWithStatus(
                    _userPersona.UserId, It.IsAny<DateTime>(), It.IsAny<long>(), It.IsAny<bool>()))
                .Returns(_organizationStatusUserPersona);

            _mockUserLoginRepository
                .Setup(m => m.GetUserOrganizationWithStatus(
                    _userInvalidPersona.UserId, It.IsAny<DateTime>(), It.IsAny<long>(), It.IsAny<bool>()))
                .Returns(_organizationStatusInvalidPersona);

            _mockProductRepository
                .Setup(m => m.GetBooksMasterProductDetail(
                    It.Is<int>(l => l == (int)ProductEnum.RPDocumentManagement)))
                .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.RPDocumentManagement));

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct,
                    It.IsAny<object>()))
                .Returns(_gbProductMap);

            HttpResponseMessage roleResponse = new HttpResponseMessage(HttpStatusCode.OK);
            roleResponse.Content = new StringContent(JsonConvert.SerializeObject(roleResult));

            _apiEndPoint = _apiEndPoint.Replace("{{domain}}", _domain);

            mockHttpMessageHandler.Setup(HttpMethod.Get, $"{_apiEndPoint}/api/{_domain}/roles?isApi=true&pageSize=9999&sort=name", roleResponse);

            HttpResponseMessage roleDetail1Response = new HttpResponseMessage(HttpStatusCode.OK);
            roleDetail1Response.Content = new StringContent(JsonConvert.SerializeObject(_role1Detail));
            mockHttpMessageHandler.Setup(HttpMethod.Get, $"{_apiEndPoint}/api/{_domain}/roles/70700?pageSize=9999", roleDetail1Response);

            HttpResponseMessage roleDetai21Response = new HttpResponseMessage(HttpStatusCode.OK);
            roleDetai21Response.Content = new StringContent(JsonConvert.SerializeObject(_role2Detail));

            mockHttpMessageHandler.Setup(HttpMethod.Get, $"{_apiEndPoint}/api/{_domain}/roles/70750?pageSize=9999", roleDetai21Response);

            HttpResponseMessage userResponse = new HttpResponseMessage(HttpStatusCode.OK);
            userResponse.Content = new StringContent(JsonConvert.SerializeObject(rpdmUser));

            mockHttpMessageHandler.Setup(HttpMethod.Get, $"{_apiEndPoint}/api/{_domain}/users/{_userId}?pageSize=9999", userResponse);

            HttpResponseMessage classifierResponse = new HttpResponseMessage(HttpStatusCode.OK);
            classifierResponse.Content = new StringContent(JsonConvert.SerializeObject(_classifierResult));

            mockHttpMessageHandler.Setup(HttpMethod.Get, $"{_apiEndPoint}/api/{_domain}{_role1Detail.Scope.HRef}?pageSize=9999", classifierResponse);

            HttpResponseMessage datasetResponse = new HttpResponseMessage(HttpStatusCode.OK);
            datasetResponse.Content = new StringContent(JsonConvert.SerializeObject(datasetResult));

            mockHttpMessageHandler.Setup(HttpMethod.Get, $"{_apiEndPoint}/api/{_domain}{_classifierResult.DataSet.HRef}/values?pageSize=9999&sort=name", datasetResponse);

            HttpResponseMessage finduserEmptyResponse = new HttpResponseMessage(HttpStatusCode.OK);
            finduserEmptyResponse.Content = new StringContent(JsonConvert.SerializeObject(new RPDMResult<RPDMDataset>() { Page = new List<RPDMDataset>() }));

            string newusername = (_person.FirstName.Substring(0, 1) + _person.LastName.Substring(0, (_person.LastName.Length >= 19 ? 19 : _person.LastName.Length))).ToLower().Replace(" ", "");
            mockHttpMessageHandler.Setup(HttpMethod.Get, $"{_apiEndPoint}/api/{_domain}/users?s=username({newusername})&pageSize=9999", finduserEmptyResponse);
        }

        [Fact]
        public void Get_RPDMRoles()
        {
            //Arrange
            AssertInitial();

            ManageProductRPDocumentManagement manageProduct = new ManageProductRPDocumentManagement(
                userClaims: _editorUserClaim,
                httpMessageHandler: mockHttpMessageHandler.Object,
                client: client,
                productInternalSettingRepository: _mockProductInternalSettingRepository.Object,
                managePersona: _mockManagePersona.Object,
                samlRepository: _mockSamlRepository.Object,
                blueBook: _mockManageBlueBook.Object,
                managePerson: _mockManagePerson.Object,
                manageUserLogin: _mockManageUserLogin.Object,
                manageContactMechanism: _mockManageContactMechanism.Object,
                managePartyRelationship: _mockManagePartyRelationship.Object,
                productRepository: _mockProductRepository.Object,
                userLoginRepository: _mockUserLoginRepository.Object,
                repository: mockRepository.Object
            );

            ListResponse result = manageProduct.GetRoles(_editorPersonaId, 0, null);
            IList<ProductRole> roleList = result.Records.Cast<ProductRole>().ToList();
            Assert.True(roleList.Count == 2 && roleList.Count(m => m.IsAssigned) == 0);

            result = manageProduct.GetRoles(_editorPersonaId, _userPersonaId, null);
            roleList = result.Records.Cast<ProductRole>().ToList();
            Assert.True(roleList.Count == 2 && roleList.Count(m => m.IsAssigned) == 1);

            result = manageProduct.GetRoleClassifierDataset(_editorPersonaId, 0, "70700", null);
            List<ProductProperty> _resultPropertyList = result.Records.Cast<ProductProperty>().ToList();
            Assert.True(_resultPropertyList.Count == 2 && _resultPropertyList.Count(m => m.IsAssigned.Value) == 0);

            result = manageProduct.GetRoleClassifierDataset(_editorPersonaId, _userPersonaId, "70700", null);
            _resultPropertyList = result.Records.Cast<ProductProperty>().ToList();
            Assert.True(_resultPropertyList.Count == 2 && _resultPropertyList.Count(m => m.IsAssigned.Value) == 1);

        }

        [Fact]
        public void Get_RPDMRolesExceptions()
        {
            //Arrange
            AssertInitial();

            ManageProductRPDocumentManagement manageProduct = new ManageProductRPDocumentManagement(
                userClaims: _editorUserClaim,
                httpMessageHandler: mockHttpMessageHandler.Object,
                client: client,
                productInternalSettingRepository: _mockProductInternalSettingRepository.Object,
                managePersona: _mockManagePersona.Object,
                samlRepository: _mockSamlRepository.Object,
                blueBook: _mockManageBlueBook.Object,
                managePerson: _mockManagePerson.Object,
                manageUserLogin: _mockManageUserLogin.Object,
                manageContactMechanism: _mockManageContactMechanism.Object,
                managePartyRelationship: _mockManagePartyRelationship.Object,
                productRepository: _mockProductRepository.Object,
                userLoginRepository: _mockUserLoginRepository.Object,
                repository: mockRepository.Object
            );

            HttpResponseMessage datasetResponse = new HttpResponseMessage(HttpStatusCode.OK);
            datasetResponse.Content = null;

            mockHttpMessageHandler.Setup(HttpMethod.Get, $"{_apiEndPoint}/api/{_domain}{_classifierResult.DataSet.HRef}/values?pageSize=9999&sort=name", datasetResponse);

            ListResponse result = manageProduct.GetRoleClassifierDataset(_editorPersonaId, _userPersonaId, "70700", null);
            List<ProductProperty> _resultPropertyList = result.Records.Cast<ProductProperty>().ToList();
            Assert.True(_resultPropertyList.Count == 0);

            datasetResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            mockHttpMessageHandler.Setup(HttpMethod.Get, $"{_apiEndPoint}/api/{_domain}{_role1Detail.Scope.HRef}?pageSize=9999", datasetResponse);

            result = manageProduct.GetRoleClassifierDataset(_editorPersonaId, 0, "70700", null);
            _resultPropertyList = result.Records.Cast<ProductProperty>().ToList();
            Assert.True(_resultPropertyList.Count == 0);

            mockHttpMessageHandler.Setup(HttpMethod.Get, $"{_apiEndPoint}/api/{_domain}{_role1Detail.Scope.HRef}?pageSize=9999", null);
            result = manageProduct.GetRoleClassifierDataset(_editorPersonaId, 0, "70700", null);
            _resultPropertyList = result.Records.Cast<ProductProperty>().ToList();
            Assert.True(_resultPropertyList.Count == 0);

            _mapCompany = null;
            _mockManageBlueBook
                .Setup(m => m.GetCompanyMap(
                    It.IsAny<Guid>(),
                    It.IsAny<long>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()
                ))
                .Returns(_mapCompany);

            result = manageProduct.GetRoles(_editorPersonaId, 0, null);
            Assert.True(result.IsError);

            datasetResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            mockHttpMessageHandler.Setup(HttpMethod.Get, $"{_apiEndPoint}/api/{_domain}/roles?pageSize=9999&sort=name", datasetResponse);

            result = manageProduct.GetRoles(_editorPersonaId, 0, null);
            Assert.True(result.IsError);
        }

        [Fact]
        public void Post_CreateUpdateUser()
        {
            //Arrange
            AssertInitial();

            ManageProductRPDocumentManagement manageProduct = new ManageProductRPDocumentManagement(
                userClaims: _editorUserClaim,
                httpMessageHandler: mockHttpMessageHandler.Object,
                client: client,
                productInternalSettingRepository: _mockProductInternalSettingRepository.Object,
                managePersona: _mockManagePersona.Object,
                samlRepository: _mockSamlRepository.Object,
                blueBook: _mockManageBlueBook.Object,
                managePerson: _mockManagePerson.Object,
                manageUserLogin: _mockManageUserLogin.Object,
                manageContactMechanism: _mockManageContactMechanism.Object,
                managePartyRelationship: _mockManagePartyRelationship.Object,
                productRepository: _mockProductRepository.Object,
                userLoginRepository: _mockUserLoginRepository.Object,
                repository: mockRepository.Object

            );

            HttpResponseMessage datasetResponse = new HttpResponseMessage(HttpStatusCode.OK);
            datasetResponse.Content = null;

            RolePropertyList newUserRoleProperty = new RolePropertyList();
            datasetResponse.Content = new StringContent(@"{""id"":81180,""target"":{""href"":""/demoapi/users/75751""}}");

            mockHttpMessageHandler.Setup(HttpMethod.Post, $"{_apiEndPoint}/api/{_domain}/users/" + _userId.ToString(), datasetResponse);
            mockHttpMessageHandler.Setup(HttpMethod.Post, $"{_apiEndPoint}/api/{_domain}/users/newuser", datasetResponse);

            string result = manageProduct.ManageRPDMUser(_editorPersonaId, _newUserPersonaId, newUserRoleProperty, out var additionalParameters1);

            Assert.True(string.IsNullOrEmpty(result));

            // no roles assigned to user
            result = manageProduct.ManageRPDMUser(_editorPersonaId, _userPersonaId, newUserRoleProperty, out var additionalParameters2);
            Assert.True(result.ToUpper() == "THERE WAS A PROBLEM CREATING THE USER. MISSING REQUIRED INFORMATION.");

            newUserRoleProperty.RoleList = new List<string>() { "70700", "70750" };
            newUserRoleProperty.PropertyList = new List<string> { "73852", "71653" };
            result = manageProduct.ManageRPDMUser(_editorPersonaId, _userPersonaId, newUserRoleProperty, out var additionalParameters3);
            Assert.True(string.IsNullOrEmpty(result));

        }

        [Fact]
        public void Put_DisableUser()
        {
            //Arrange
            AssertInitial();

            ManageProductRPDocumentManagement manageProduct = new ManageProductRPDocumentManagement(
                userClaims: _editorUserClaim,
                httpMessageHandler: mockHttpMessageHandler.Object,
                client: client,
                productInternalSettingRepository: _mockProductInternalSettingRepository.Object,
                managePersona: _mockManagePersona.Object,
                samlRepository: _mockSamlRepository.Object,
                blueBook: _mockManageBlueBook.Object,
                managePerson: _mockManagePerson.Object,
                manageUserLogin: _mockManageUserLogin.Object,
                manageContactMechanism: _mockManageContactMechanism.Object,
                managePartyRelationship: _mockManagePartyRelationship.Object,
                productRepository: _mockProductRepository.Object,
                userLoginRepository: _mockUserLoginRepository.Object,
                repository: mockRepository.Object
            );

            HttpResponseMessage datasetResponse = new HttpResponseMessage(HttpStatusCode.OK);
            datasetResponse.Content = null;

            datasetResponse.Content = new StringContent(@"{""id"":81180,""target"":{""href"":""/demoapi/users/75751""}}");

            mockHttpMessageHandler.Setup(HttpMethod.Post, $"{_apiEndPoint}/api/{_domain}/users/{_userId.ToString()}/disable", datasetResponse);

            string result = manageProduct.UnassignUser(_editorPersonaId, _userPersonaId);
            Assert.True(string.IsNullOrEmpty(result));
        }
    }
}
