using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.Product.Interfaces;
using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Lead2Lease;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.Product.OneSite;
using UnifiedLogin.SharedObjects.Saml;
using UnifiedLogin.LandingAPI.Test.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using IC = UnifiedLogin.SharedObjects.IdentityConfig;

namespace UnifiedLogin.LandingAPI.Test.Logic
{
    /// <summary>
    /// Product xUnit tests
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ManageLead2LeaseTest : ManageProductBaseTests
    {
        private readonly ITestOutputHelper _output;
        private int _blueBookId;
        private static ProductProperty _property1 = new ProductProperty() { ID = "1234567", Name = "Test Property", City = "Test City", State = "Test State", Street1 = "Test Street 1", Street2 = "Test Street 2", Zip = "12345", IsAssigned = true };
        private static ProductProperty _property2 = new ProductProperty() { ID = "7654321", Name = "Test Property 2", City = "Test City 2", State = "Test State 2", Street1 = "Test Street 1 2", Street2 = "Test Street 2 2", Zip = "54321", IsAssigned = false };
        private static ProductProperty _property3 = new ProductProperty() { ID = "2345678", Name = "Test Property 3", City = "Test City 3", State = "Test State 3", Street1 = "Test Street 1 3", Street2 = "Test Street 2 3", Zip = "54321", IsAssigned = true };

        private static Property _l2lProperty1 = new Property() { PropertyId = Convert.ToInt32(_property1.ID), ComplexName = _property1.Name, Address = _property1.Street1, City = _property1.City, State = _property1.State, Zip = _property1.Zip, PMSystemID = "2211334" };
        private static Property _l2lProperty2 = new Property() { PropertyId = Convert.ToInt32(_property2.ID), ComplexName = _property2.Name, Address = _property2.Street1, City = _property2.City, State = _property2.State, Zip = _property2.Zip, PMSystemID = "1234" };
        private static Property _l2lProperty3 = new Property() { PropertyId = Convert.ToInt32(_property3.ID), ComplexName = _property3.Name, Address = _property3.Street1, City = _property3.City, State = _property3.State, Zip = _property3.Zip };

        private static Role _l2lRole1 = new Role() { UserRoleId = 1, UserRoleName = "Full Access", UserRoleDescription = "Full role access", RoleTypeId = 0 };
        private static Role _l2lRole2 = new Role() { UserRoleId = 2, UserRoleName = "Super User", UserRoleDescription = "Ability to access user management", RoleTypeId = 5 };
        private static Role _l2lRole3 = new Role() { UserRoleId = 101, UserRoleName = "Allow user to change passwords manually", UserRoleDescription = "Allows super user to change passwords manually", RoleTypeId = 5 };

        private List<Property> _l2lPropertyList = new List<Property>();
        private List<Role> _l2lRoleList = new List<Role>();
        private RoleInfo _l2lRoleInfo = new RoleInfo();
        private List<Permission> _l2lPermissionList = new List<Permission>();
        private Lead2LeaseUser _l2lUser;
        private List<SamlAttributes> _userOneSiteSamlAttributes;

        private IList<ProductSettingType> _productSettingTypeList;

        private OneSiteUser _osUser;

        private string testHostname = "http://localhost";
        private string _mtApiEndPoint = "http://localhost";

        private int _companyInstanceSourceId;

        private UserLoginOnly _userloginOnly;

        public ManageLead2LeaseTest(ITestOutputHelper output) : base((int)ProductEnum.Lead2Lease)
        {
            _output = output;
            _companyInstanceSourceId = 1000;
            _blueBookId = 236;

            _editorSamlAttributes = new List<SamlAttributes>();
            _editorSamlAttributes.Add(new SamlAttributes() { Name = "UserId", Value = "1234567" });
            _editorSamlAttributes.Add(new SamlAttributes() { Name = "PRODUCTUSERNAME", Value = "bob12" });

            _userSamlAttributes = new List<SamlAttributes>();
            _userSamlAttributes.Add(new SamlAttributes() { Name = "UserId", Value = "5432" });
            _userSamlAttributes.Add(new SamlAttributes() { Name = "PRODUCTUSERNAME", Value = "larry33" });

            _userOneSiteSamlAttributes = new List<SamlAttributes>();
            _userOneSiteSamlAttributes.Add(new SamlAttributes() { Name = "UserId", Value = "5432" });
            _userOneSiteSamlAttributes.Add(new SamlAttributes() { Name = "PRODUCTUSERNAME", Value = "larry33" });

            _productSettingType.Add(new ProductSettingType() { ProductSettingTypeId = 1, Name = "ProductStatus" });

            _userProductSettings.Add(new ProductSettingList() { ProductId = 1, Name = "IsFavorite", Value = "1", ProductSettingId = 1234 });
            _userProductSettings.Add(new ProductSettingList() { ProductId = 1, Name = "AllProperties", Value = "1", ProductSettingId = 1234 });

            _electronicAddressList = new List<IC.ElectronicAddress>();
            _electronicAddressList.Add(new IC.ElectronicAddress() { AddressType = "Email", AddressString = "test" });

            _productInternalSettings.Add(new IC.ProductInternalSetting() { Name = "ApiEndPoint", Value = testHostname });
            _productInternalSettings.Add(new IC.ProductInternalSetting() { Name = "MtApiEndPoint", Value = _mtApiEndPoint });

            _repositoryResponseProductStatus.ErrorMessage = "";
            _l2lPropertyList.Add(_l2lProperty1);
            _l2lPropertyList.Add(_l2lProperty2);
            _l2lPropertyList.Add(_l2lProperty3);

            _l2lRoleList.Add(_l2lRole1);
            _l2lRoleList.Add(_l2lRole2);
            _l2lRoleList.Add(_l2lRole3);

            _l2lRoleInfo.Roles = _l2lRoleList;
            List<Preset> presetList = new List<Preset>();
            presetList.Add(new Preset() { Id = 1, Name = "Basic User", RoleIds = new List<int>() { 1, 2, 7043, 7052, 9000, 9200 } });
            presetList.Add(new Preset() { Id = 2, Name = "Super User", RoleIds = new List<int>() { 1, 2, 7043, 7052, 9000, 9200 } });

            _l2lRoleInfo.Presets = presetList;

            _l2lPermissionList.Add(new Permission() { PropertyId = 1234567, UserRoleId = 1 });
            _l2lPermissionList.Add(new Permission() { PropertyId = 7654321, UserRoleId = 2 });
            _l2lPermissionList.Add(new Permission() { PropertyId = 2345678, UserRoleId = 3 });
            
            _l2lUser = new Lead2LeaseUser() { UserId = 5432, UserName = "larry", FirstName = "Larry", LastName = "Duck", Properties = _l2lPropertyList, Permissions = _l2lPermissionList };

            _productSettingTypeList = new List<ProductSettingType>();
            _productSettingTypeList.Add(new ProductSettingType() { ProductSettingTypeId = 1, Name = "ProductStatus", Description = "Status of the product" });

            _osUser = new OneSiteUser() { AllProperties = false, SystemIdentifier = "1192422|testuser", UserId = 12345 };

            _userloginOnly = new UserLoginOnly() { UserId = _editorUserId, LoginName = "test", PartyId = 30, RealPageId = new Guid(), LastLogin = DateTime.Now };

            mockRepository
                .Setup(m => m.GetMany<IC.ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct,
                    It.Is<object>(d => TestIsProductId(d, 6))))
                .Returns(_productInternalSettings);

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct,
                    It.IsAny<object>()))
                .Returns(_gbProductMap);
        }

        #region Exceptions - Property
        [Fact]
        public void Get_UserProperties_Errors()
        {
            //Arrange
            
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockHandler = new Mock<HttpMessageHandler>();

            #region Responses
            HttpResponseMessage responseProperties = new HttpResponseMessage(HttpStatusCode.OK);
            responseProperties.Content = null;

            HttpResponseMessage responseRoles = new HttpResponseMessage(HttpStatusCode.OK);
            responseRoles.Content = null;

            HttpResponseMessage responseUser = new HttpResponseMessage(HttpStatusCode.OK);
            responseUser.Content = new StringContent(JsonConvert.SerializeObject(_l2lUser));

            #endregion

            Uri propertyListUri = new Uri(testHostname+ "/Users/ActiveProperties/"+ _blueBookId.ToString());
            Uri rolesUri = new Uri(testHostname + "/Users/ActiveRoles");
            Uri userUri = new Uri(testHostname + "/Users/5432");

            IList<CustomerCompanyMap> mapCompany = new List<CustomerCompanyMap>();
            CustomerCompanyMap resource = new CustomerCompanyMap() { CompanyInstanceSourceId = _blueBookId.ToString(), Source = "L2L" };
            mapCompany.Add(resource);

            _editorPersona.Organization.BooksCustomerMasterId = _blueBookId;

            mockHandler.Setup(HttpMethod.Get, propertyListUri.ToString(), responseProperties);
            mockHandler.Setup(HttpMethod.Get, rolesUri.ToString(), responseRoles);
            mockHandler.Setup(HttpMethod.Get, userUri.ToString(), responseRoles);

            mockManageBlueBook
                .Setup(m => m.GetCompanyMap(
                    It.IsAny<Guid>(),
                    It.IsAny<long>(),
                    It.Is<string>(l => l == "L2L"),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()
                 ))
                 .Returns(mapCompany);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.Is<int>(l => l == (int)ProductEnum.Lead2Lease)
                ))
                .Returns(_productInternalSettings);

            mockRepository
                .Setup(m => m.GetMany<IC.ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct,
                    It.Is<object>(d => TestIsProductId(d, (int)ProductEnum.Lead2Lease))))
                .Returns(_productInternalSettings);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 4)
                    , It.Is<int>(l => l == (int)ProductEnum.Lead2Lease)
                 ))
                 .Returns(_editorSamlAttributes);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 5)
                    , It.Is<int>(l => l == (int)ProductEnum.Lead2Lease)
                 ))
                 .Returns(_userSamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 4)
                 ))
                 .Returns(_editorPersona);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 5)
                 ))
                 .Returns(_userPersona);

            mockProductRepository
                .Setup(m => m.GetBooksMasterProductDetail(
                    It.Is<int>(l => l == (int)ProductEnum.Lead2Lease)
                ))
                .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.Lead2Lease));

            //Act
            IManageProductLead2Lease mpL2L = new ManageProductLead2Lease(
                editorRealPageId: _editorRealPageId,
                userClaim: _editorUserClaim,
                messageHandler: mockHandler.Object,
                samlRepository: mockSamlRepository.Object,
                managePersona: mockManagePersona.Object,
                manageBlueBook: mockManageBlueBook.Object,
                productRepository: mockProductRepository.Object,
                productInternalSettingRepository: mockProductInternalSettingRepository.Object,
                managePerson: null,
                manageUserLogin: null,
                managePartyRelationship: null,
                manageElectronicAddress: null,
                manageProductOneSite: null,
                userLoginRepository: null,
                repository: mockRepository.Object);

            ListResponse resp = mpL2L.GetRoles(_editorPersonaId, _userPersonaId, null);
            Assert.True(resp.IsError == true && (resp.ErrorReason == "Role info is missing" || 
                        resp.ErrorReason == CommonMessageConstants.RoleErrorMessage ||
                        resp.ErrorReason == CommonMessageConstants.RightErrorMessage ||
                        resp.ErrorReason == CommonMessageConstants.CompanyErrorMessage));

            resp = mpL2L.GetProperties(_editorPersonaId, _userPersonaId, null);
            Assert.True(resp.IsError == true && (resp.ErrorReason == "Company Setup Error: Please Contact Support." ||
                        resp.ErrorReason == CommonMessageConstants.PropertyErrorMessage ||
                        resp.ErrorReason == CommonMessageConstants.CompanyErrorMessage));

            // break the Organization blue book id
            _editorPersona.Organization.BooksCustomerMasterId = 0;
            
            resp = mpL2L.GetRoles(_editorPersonaId, _userPersonaId, null);
            Assert.True(resp.IsError == true && (resp.ErrorReason == "Role info is missing" ||
                        resp.ErrorReason == CommonMessageConstants.RoleErrorMessage ||
                        resp.ErrorReason == CommonMessageConstants.RightErrorMessage ||
                        resp.ErrorReason == CommonMessageConstants.CompanyErrorMessage));

            resp = mpL2L.GetProperties(_editorPersonaId, _userPersonaId, null);
            Assert.True(resp.IsError == true && (resp.ErrorReason == "Company Setup Error: Please Contact Support." ||
                        resp.ErrorReason == CommonMessageConstants.PropertyErrorMessage ||
                        resp.ErrorReason == CommonMessageConstants.CompanyErrorMessage));

            // reset the results being returned
            responseProperties.Content = new StringContent(JsonConvert.SerializeObject(_l2lPropertyList));
            responseRoles.Content = new StringContent(JsonConvert.SerializeObject(_l2lRoleInfo));
            responseUser.Content = null;

            mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Setup(HttpMethod.Get, propertyListUri.ToString(), responseProperties);
            mockHandler.Setup(HttpMethod.Get, rolesUri.ToString(), responseRoles);
            mockHandler.Setup(HttpMethod.Get, userUri.ToString(), responseUser);

            mpL2L = new ManageProductLead2Lease(
                editorRealPageId: _editorRealPageId,
                userClaim: _editorUserClaim,
                messageHandler: mockHandler.Object,
                samlRepository: mockSamlRepository.Object,
                managePersona: mockManagePersona.Object,
                manageBlueBook: mockManageBlueBook.Object,
                productRepository: mockProductRepository.Object,
                productInternalSettingRepository: mockProductInternalSettingRepository.Object,
                managePerson: null,
                manageUserLogin: null,
                managePartyRelationship: null,
                manageElectronicAddress: null,
                manageProductOneSite: null,
                userLoginRepository: null,
                repository: mockRepository.Object);

            resp = mpL2L.GetRoles(_editorPersonaId, _userPersonaId, null);
            Assert.True(resp.IsError == true && (resp.ErrorReason == "User info is missing" ||
                        resp.ErrorReason == CommonMessageConstants.RoleErrorMessage ||
                        resp.ErrorReason == CommonMessageConstants.CompanyErrorMessage));

            resp = mpL2L.GetProperties(_editorPersonaId, _userPersonaId, null);
            Assert.True(resp.IsError == true && (resp.ErrorReason == "User info is missing" ||
                        resp.ErrorReason == CommonMessageConstants.PropertyErrorMessage ||
                        resp.ErrorReason == CommonMessageConstants.CompanyErrorMessage));

        }

        [Fact]
        public void Get_UserProperties_Data()
        {
            //Arrange

            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockHandler = new Mock<HttpMessageHandler>();

            #region Responses
            HttpResponseMessage responseProperties = new HttpResponseMessage(HttpStatusCode.OK);
            responseProperties.Content = new StringContent(JsonConvert.SerializeObject(_l2lPropertyList));

            HttpResponseMessage responseRoles = new HttpResponseMessage(HttpStatusCode.OK);
            responseRoles.Content = new StringContent(JsonConvert.SerializeObject(_l2lRoleInfo));

            HttpResponseMessage responseUser = new HttpResponseMessage(HttpStatusCode.OK);
            responseUser.Content = new StringContent(JsonConvert.SerializeObject(_l2lUser));

            #endregion

            Uri propertyListUri = new Uri(testHostname + "/Users/ActiveProperties/" + _blueBookId.ToString());
            Uri rolesUri = new Uri(testHostname + "/Users/ActiveRoles");
            Uri userUri = new Uri(testHostname + "/Users/5432");

            IList<CustomerCompanyMap> mapCompany = new List<CustomerCompanyMap>();
            CustomerCompanyMap resource = new CustomerCompanyMap() { CompanyInstanceSourceId = _blueBookId.ToString(), Source = "L2L" };
            mapCompany.Add(resource);

            _editorPersona.Organization.BooksCustomerMasterId = _blueBookId;

            mockHandler.Setup(HttpMethod.Get, propertyListUri.ToString(), responseProperties);
            mockHandler.Setup(HttpMethod.Get, rolesUri.ToString(), responseRoles);
            mockHandler.Setup(HttpMethod.Get, userUri.ToString(), responseUser);

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
                 .Returns(mapCompany);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.Is<int>(l => l == (int)ProductEnum.Lead2Lease)
                ))
                .Returns(_productInternalSettings);

            mockRepository
                .Setup(m => m.GetMany<IC.ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct,
                    It.Is<object>(d => TestIsProductId(d, (int)ProductEnum.Lead2Lease))))
                .Returns(_productInternalSettings);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 4)
                    , It.Is<int>(l => l == (int)ProductEnum.Lead2Lease)
                 ))
                 .Returns(_editorSamlAttributes);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 5)
                    , It.Is<int>(l => l == (int)ProductEnum.Lead2Lease)
                 ))
                 .Returns(_userSamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 4)
                 ))
                 .Returns(_editorPersona);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 5)
                 ))
                 .Returns(_userPersona);

            mockProductRepository
                .Setup(m => m.GetProductSettingsByPersona(
                    It.IsAny<long>()
                ))
                .Returns(_userProductSettings);

            mockProductRepository
             .Setup(m => m.GetBooksMasterProductDetail(
                 It.Is<int>(l => l == (int)ProductEnum.Lead2Lease)
             ))
             .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.Lead2Lease));
            
            mockProductRepository
                .Setup(m => m.GetProductSettingsByPersona(
                    It.IsAny<long>()
                ))
                .Returns(_userProductSettings);

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct,
                    It.IsAny<object>()))
                .Returns(_gbProductMap);

            //Act
            IManageProductLead2Lease mpL2L = new ManageProductLead2Lease(
                editorRealPageId: _editorRealPageId,
                userClaim: _editorUserClaim,
                messageHandler: mockHandler.Object,
                samlRepository: mockSamlRepository.Object,
                managePersona: mockManagePersona.Object,
                manageBlueBook: mockManageBlueBook.Object,
                productRepository: mockProductRepository.Object,
                productInternalSettingRepository: mockProductInternalSettingRepository.Object,
                managePerson: null,
                manageUserLogin: null,
                managePartyRelationship: null,
                manageElectronicAddress: null,
                manageProductOneSite: null,
                userLoginRepository: null,
                repository: mockRepository.Object);

            new RPObjectCache().BustCache();

            ListResponse resp = mpL2L.GetRoles(_editorPersonaId, _userPersonaId, null);
            _output.WriteLine("resp 1 : " + JsonConvert.SerializeObject(resp));
            Assert.True(resp.TotalRows == _l2lUser.Permissions.Count);

            new RPObjectCache().BustCache();

            resp = mpL2L.GetProperties(_editorPersonaId, _userPersonaId, null);
            _output.WriteLine("resp 2 : " + JsonConvert.SerializeObject(resp));
            Assert.True(resp.TotalRows == _l2lUser.Properties.Count);
        }
        #endregion

        [Fact]
        public void Put_DisableUser()
        {
            //Arrange
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockHandler = new Mock<HttpMessageHandler>();
            var mockManageUserLogin = new Mock<IManageUserLogin>();
            var mockManagePerson = new Mock<IManagePerson>();
            var mockUserLoginRepository = new Mock<IUserLoginRepository>();

            Uri userUri = new Uri(testHostname + "/Users/5432");
            Uri userPutUri = new Uri(testHostname + "/Users/Disable/5432"); 

            RepositoryResponse updateProductStatusResult = new RepositoryResponse() { ErrorMessage = "", Id = 23 };
            
            IC.Person person = new IC.Person() { FirstName = "Test First", LastName = "Test Last", PartyId = 30 };

            HttpResponseMessage responseUser = new HttpResponseMessage(HttpStatusCode.OK);
            responseUser.Content = new StringContent(JsonConvert.SerializeObject(_l2lUser));

            HttpResponseMessage responsePutUser = new HttpResponseMessage(HttpStatusCode.OK);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.Is<int>(l => l == (int)ProductEnum.Lead2Lease)
                ))
                .Returns(_productInternalSettings);

            mockRepository
                .Setup(m => m.GetMany<IC.ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct,
                    It.Is<object>(d => TestIsProductId(d, 6))))
                .Returns(_productInternalSettings);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 4)
                    , It.Is<int>(l => l == (int)ProductEnum.Lead2Lease)
                 ))
                 .Returns(_editorSamlAttributes);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 5)
                    , It.Is<int>(l => l == (int)ProductEnum.Lead2Lease)
                 ))
                 .Returns(_userSamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 4)
                 ))
                 .Returns(_editorPersona);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 5)
                 ))
                 .Returns(_userPersona);

            mockProductRepository
                .Setup(m => m.GetProductSettingsByPersona(
                    It.Is<long>(l => l == 5)
                ))
                .Returns(_userProductSettings);

            mockProductRepository
                .Setup(m => m.ListProductSettingType()
                )
                .Returns(_productSettingTypeList);

            mockProductRepository
                .Setup(m => m.CreateProductSetting(
                    It.Is<long>(l => l == 5),
                    It.Is<int>(l => l == (int)ProductEnum.Lead2Lease),
                    It.IsAny<int>(),
                    It.IsAny<string>()
                ))
                .Returns(updateProductStatusResult);

            GbProductMap gbProductMap = new GbProductMap();
            
            mockProductRepository
                .Setup(m => m.GetBooksMasterProductDetail(
                    It.Is<int>(l => l == (int)ProductEnum.Lead2Lease)
                ))
                .Returns(gbProductMap);

            mockHandler.Setup(HttpMethod.Get, userUri.ToString(), responseUser);
            mockHandler.Setup(HttpMethod.Put, userPutUri.ToString(), responsePutUser);

            mockManageUserLogin
               .Setup(m => m.GetUserLoginOnly(
                   It.IsAny<Guid>()
               ))
               .Returns(_userloginOnly);

            mockManagePerson
                .Setup(m => m.GetPerson(
                    It.IsAny<Guid>()
                ))
                .Returns(person);

            mockUserLoginRepository
                .Setup(m => m.GetUserOrganizationWithStatus(
                    _editorPersona.UserId, It.IsAny<DateTime>(), It.IsAny<long>(), It.IsAny<bool>()))
                .Returns(_organizationStatusEditorPersona);

            mockUserLoginRepository
                .Setup(m => m.GetUserOrganizationWithStatus(
                    _newUserPersona.UserId, It.IsAny<DateTime>(), It.IsAny<long>(), It.IsAny<bool>()))
                .Returns(_organizationStatusNewUserPersona);

            mockUserLoginRepository
                .Setup(m => m.GetUserOrganizationWithStatus(
                    _userPersona.UserId, It.IsAny<DateTime>(), It.IsAny<long>(), It.IsAny<bool>()))
                .Returns(_organizationStatusUserPersona);

            mockUserLoginRepository
                .Setup(m => m.GetUserOrganizationWithStatus(
                    _userInvalidPersona.UserId, It.IsAny<DateTime>(), It.IsAny<long>(), It.IsAny<bool>()))
                .Returns(_organizationStatusInvalidPersona);

            //Act
            IManageProductLead2Lease mpL2L = new ManageProductLead2Lease(
                editorRealPageId: _editorRealPageId,
                userClaim: _editorUserClaim,
                messageHandler: mockHandler.Object,
                samlRepository: mockSamlRepository.Object,
                managePersona: mockManagePersona.Object,
                manageBlueBook: mockManageBlueBook.Object,
                productRepository: mockProductRepository.Object,
                productInternalSettingRepository: mockProductInternalSettingRepository.Object,
                managePerson: mockManagePerson.Object,
                manageUserLogin: mockManageUserLogin.Object,
                managePartyRelationship: null,
                manageElectronicAddress: null,
                manageProductOneSite: null,
                userLoginRepository: mockUserLoginRepository.Object,
                repository: mockRepository.Object);

            string result = mpL2L.UnassignUser(_editorPersonaId, _userPersonaId);
            Assert.True(string.IsNullOrEmpty(result));
        }
        #region User

        [Fact]
        public void Post_L2LUser_Errors()
        {
            //Arrange
            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockManagePerson = new Mock<IManagePerson>();
            var mockManageUserLogin = new Mock<IManageUserLogin>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            var mockManagePartyRelationship = new Mock<IManagePartyRelationship>();
            var mockManageElectronicAddress = new Mock<IManageElectronicAddress>();
            var mockHandler = new Mock<HttpMessageHandler>();
            var mockManageProductOneSite = new Mock<IManageProductOneSite>();

            #region Responses
            HttpResponseMessage responseProperties = new HttpResponseMessage(HttpStatusCode.OK);
            responseProperties.Content = new StringContent(JsonConvert.SerializeObject(_l2lPropertyList));

            HttpResponseMessage responseRoles = new HttpResponseMessage(HttpStatusCode.OK);
            responseRoles.Content = new StringContent(JsonConvert.SerializeObject(_l2lRoleInfo));

            HttpResponseMessage responseUser = new HttpResponseMessage(HttpStatusCode.OK);
            responseUser.Content = new StringContent(JsonConvert.SerializeObject(_l2lUser));

            HttpResponseMessage responseEmpty = new HttpResponseMessage(HttpStatusCode.OK);
            responseEmpty.Content = new StringContent("");

            HttpResponseMessage responseNull = null;

            #endregion

            Uri propertyListUri = new Uri(testHostname + "/Users/ActiveProperties/" + _blueBookId.ToString());
            Uri rolesUri = new Uri(testHostname + "/Users/ActiveRoles");
            Uri userUri = new Uri(testHostname + "/Users/5432");
            Uri createUserUri = new Uri(testHostname + "/Users/RealPage");
            Uri updateUserUri = new Uri(testHostname + "/Users/edit");

            IList<CustomerCompanyMap> mapResource = new List<CustomerCompanyMap>();
            CustomerCompanyMap resource = new CustomerCompanyMap() { CompanyInstanceSourceId = _blueBookId.ToString(), Source = "L2L" };
            mapResource.Add(resource);

            _editorPersona.Organization.BooksCustomerMasterId = _blueBookId;
            IC.Person person = new IC.Person() { FirstName = "Test First", LastName = "Test Last", PartyId = 30 };

            IC.PartyRelationship _partyRelationShip = new IC.PartyRelationship();
            _partyRelationShip.RoleTypeFrom = new IC.RoleType() { Name = _ROLETYPE_NAME_USER };

            IList<ProductProperty> productPropertyList = new List<ProductProperty>();
            productPropertyList.Add(new ProductProperty() { ID = "2211334", Name = "Test Property", IsAssigned = true });

            ListResponse osPropertyListResponse = new ListResponse();
            osPropertyListResponse.IsError = false;
            osPropertyListResponse.Records = productPropertyList.Cast<object>().ToList();

            _electronicAddressList = new List<IC.ElectronicAddress>();
            _electronicAddressList.Add(new IC.ElectronicAddress() { AddressType = "Email", AddressString = "test", contactMechanismUsageType = new IC.ContactMechanismUsageType() { Name = "PRIMARY" } });

            mockHandler.Setup(HttpMethod.Get, propertyListUri.ToString(), responseProperties);
            mockHandler.Setup(HttpMethod.Get, rolesUri.ToString(), responseRoles);
            mockHandler.Setup(HttpMethod.Get, userUri.ToString(), responseUser);
            mockHandler.Setup(HttpMethod.Post, createUserUri.ToString(), responseUser);
            mockHandler.Setup(HttpMethod.Put, updateUserUri.ToString(), responseEmpty);

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
                 .Returns(mapResource);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.Is<int>(l => l == (int)ProductEnum.Lead2Lease)
                ))
                .Returns(_productInternalSettings);

            mockRepository
                .Setup(m => m.GetMany<IC.ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct,
                    It.Is<object>(d => TestIsProductId(d, 6))))
                .Returns(_productInternalSettings);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 4)
                    , It.Is<int>(l => l == (int)ProductEnum.Lead2Lease)
                 ))
                 .Returns(_editorSamlAttributes);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 5)
                    , It.Is<int>(l => l == (int)ProductEnum.OneSite)
                 ))
                 .Returns(_userOneSiteSamlAttributes);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 5)
                    , It.Is<int>(l => l == (int)ProductEnum.Lead2Lease)
                 ))
                 .Returns(_emptySamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 4)
                 ))
                 .Returns(_userInvalidPersona);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 5)
                 ))
                 .Returns(_userPersona);

            mockProductRepository
                .Setup(m => m.GetProductSettingsByPersona(
                    It.IsAny<long>()
                ))
                .Returns(_userProductSettings);

            mockManagePerson
                .Setup(m => m.GetPerson(
                    It.IsAny<Guid>()
                ))
                .Returns(person);

            mockManageUserLogin
                .Setup(m => m.GetUserLoginOnly(
                    It.IsAny<Guid>()
                ))
                .Returns(_userloginOnly);

            mockManagePartyRelationship
                .Setup(m => m.GetPartyRelationship(
                    It.IsAny<Guid>()
                    , It.IsAny<Guid>()
                    , null
                    , null
                    , It.IsAny<string>()
               ))
               .Returns(_partyRelationShip);

            mockManageElectronicAddress
                .Setup(m => m.ListElectronicAddressForPerson(
                    It.IsAny<Guid>()
                    , It.IsAny<string>()
                ))
                .Returns(_electronicAddressList);

            mockProductRepository
                .Setup(m => m.ListProductSettingType()
                )
                .Returns(_productSettingTypeList);

            mockManageProductOneSite
                .Setup(m => m.GetOneSiteUserInfo(
                    It.IsAny<string>()
                ))
                .Returns(_osUser);

            mockManageProductOneSite
                .Setup(m => m.GetOneSitePropertyList(
                    It.IsAny<long>(),
                    It.IsAny<long>(),
                    It.IsAny<bool>(),
                    null
                ))
                .Returns(osPropertyListResponse);

            mockManageProductOneSite
                .Setup(m => m.UserInLeasingAgentList(
                    It.IsAny<long>(),
                    It.IsAny<long>(),
                    It.IsAny<int>()
                ))
                .Returns(true);

            mockProductRepository
             .Setup(m => m.GetBooksMasterProductDetail(
                 It.Is<int>(l => l == (int)ProductEnum.Lead2Lease)
             ))
             .Returns(_gbProductMap.Find(p => p.ProductId == (int)ProductEnum.Lead2Lease));

            //Act
            IManageProductLead2Lease mpL2L = new ManageProductLead2Lease(
                editorRealPageId: _editorRealPageId,
                userClaim: _editorUserClaim,
                messageHandler: mockHandler.Object,
                samlRepository: mockSamlRepository.Object,
                managePersona: mockManagePersona.Object,
                manageBlueBook: mockManageBlueBook.Object,
                productRepository: mockProductRepository.Object,
                productInternalSettingRepository: mockProductInternalSettingRepository.Object,
                managePerson: mockManagePerson.Object,
                manageUserLogin: mockManageUserLogin.Object,
                managePartyRelationship: mockManagePartyRelationship.Object,
                manageElectronicAddress: mockManageElectronicAddress.Object,
                manageProductOneSite: mockManageProductOneSite.Object,
                userLoginRepository: null,
                repository: mockRepository.Object);

            List<string> roleList = new List<string>();
            List<string> propertyList = new List<string>();

            roleList.Add("1");
            roleList.Add("2");
            propertyList.Add("1234567");
            propertyList.Add("7654321");

            // GetCompanyEditorAndUserDetails fail, invalid editor persona
            string result = mpL2L.ManageLead2LeaseUser(_editorPersonaId, _userPersonaId, roleList, propertyList, out var additionalParameters);

            _output.WriteLine("result 1 : " + result);

            Assert.True(result.ToUpper() == "INVALID PERSONA");

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 4)
                 ))
                 .Returns(_editorPersona);

            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync"
                    , ItExpr.Is<HttpRequestMessage>(message => message.RequestUri.ToString().ToLower() == userUri.ToString().ToLower())
                    , ItExpr.IsAny<CancellationToken>()
                )
                .Returns(Task.FromResult<HttpResponseMessage>(responseEmpty))
                .Callback<HttpRequestMessage, CancellationToken>((r, c) =>
                {
                    Assert.Equal(HttpMethod.Get, r.Method);
                });

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 5)
                    , It.Is<int>(l => l == (int)ProductEnum.Lead2Lease)
                 ))
                 .Returns(_userSamlAttributes);

            mpL2L = new ManageProductLead2Lease(
                editorRealPageId: _editorRealPageId,
                userClaim: _editorUserClaim,
                messageHandler: mockHandler.Object,
                samlRepository: mockSamlRepository.Object,
                managePersona: mockManagePersona.Object,
                manageBlueBook: mockManageBlueBook.Object,
                productRepository: mockProductRepository.Object,
                productInternalSettingRepository: mockProductInternalSettingRepository.Object,
                managePerson: mockManagePerson.Object,
                manageUserLogin: mockManageUserLogin.Object,
                managePartyRelationship: mockManagePartyRelationship.Object,
                manageElectronicAddress: mockManageElectronicAddress.Object,
                manageProductOneSite: mockManageProductOneSite.Object,
                userLoginRepository: null,
                repository: mockRepository.Object);

            // missing user info
            result = mpL2L.ManageLead2LeaseUser(_editorPersonaId, _userPersonaId, roleList, propertyList, out var additionalParameters1);
            _output.WriteLine("result 2 : " + result);

            Assert.True(result.ToUpper() == "USER INFO MISSING");

            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync"
                    , ItExpr.Is<HttpRequestMessage>(message => message.RequestUri.ToString().ToLower() == userUri.ToString().ToLower())
                    , ItExpr.IsAny<CancellationToken>()
                )
                .Returns(Task.FromResult<HttpResponseMessage>(responseUser))
                .Callback<HttpRequestMessage, CancellationToken>((r, c) =>
                {
                    Assert.Equal(HttpMethod.Get, r.Method);
                });

            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync"
                    , ItExpr.Is<HttpRequestMessage>(message => message.RequestUri.ToString().ToLower() == propertyListUri.ToString().ToLower())
                    , ItExpr.IsAny<CancellationToken>()
                )
                .Returns(Task.FromResult<HttpResponseMessage>(responseNull))
                .Callback<HttpRequestMessage, CancellationToken>((r, c) =>
                {
                    Assert.Equal(HttpMethod.Get, r.Method);
                });

            mpL2L = new ManageProductLead2Lease(
                editorRealPageId: _editorRealPageId,
                userClaim: _editorUserClaim,
                messageHandler: mockHandler.Object,
                samlRepository: mockSamlRepository.Object,
                managePersona: mockManagePersona.Object,
                manageBlueBook: mockManageBlueBook.Object,
                productRepository: mockProductRepository.Object,
                productInternalSettingRepository: mockProductInternalSettingRepository.Object,
                managePerson: mockManagePerson.Object,
                manageUserLogin: mockManageUserLogin.Object,
                managePartyRelationship: mockManagePartyRelationship.Object,
                manageElectronicAddress: mockManageElectronicAddress.Object,
                manageProductOneSite: mockManageProductOneSite.Object,
                userLoginRepository: null,
                repository: mockRepository.Object);

            // property list fail
            result = mpL2L.ManageLead2LeaseUser(_editorPersonaId, _userPersonaId, roleList, propertyList, out var additionalParameters2);
            _output.WriteLine("result 3 : " + result);
            Assert.True(result.ToUpper() == "COMPANY SETUP ERROR: PLEASE CONTACT SUPPORT.");

            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync"
                    , ItExpr.Is<HttpRequestMessage>(message => message.RequestUri.ToString().ToLower() == rolesUri.ToString().ToLower())
                    , ItExpr.IsAny<CancellationToken>()
                )
                .Returns(Task.FromResult<HttpResponseMessage>(responseRoles))
                .Callback<HttpRequestMessage, CancellationToken>((r, c) =>
                {
                    Assert.Equal(HttpMethod.Get, r.Method);
                });

            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync"
                    , ItExpr.Is<HttpRequestMessage>(message => message.RequestUri.ToString().ToLower() == propertyListUri.ToString().ToLower())
                    , ItExpr.IsAny<CancellationToken>()
                )
                .Returns(Task.FromResult<HttpResponseMessage>(responseProperties))
                .Callback<HttpRequestMessage, CancellationToken>((r, c) =>
                {
                    Assert.Equal(HttpMethod.Get, r.Method);
                });

            _partyRelationShip = new IC.PartyRelationship();
            _partyRelationShip.RoleTypeFrom = new IC.RoleType() { Name = _ROLETYPE_NAME_SUPERUSER };

            mockManagePartyRelationship
                .Setup(m => m.GetPartyRelationship(
                    It.IsAny<Guid>()
                    , It.IsAny<Guid>()
                    , null
                    , null
                    , It.IsAny<string>()
               ))
               .Returns(_partyRelationShip);

            mpL2L = new ManageProductLead2Lease(
                editorRealPageId: _editorRealPageId,
                userClaim: _editorUserClaim,
                messageHandler: mockHandler.Object,
                samlRepository: mockSamlRepository.Object,
                managePersona: mockManagePersona.Object,
                manageBlueBook: mockManageBlueBook.Object,
                productRepository: mockProductRepository.Object,
                productInternalSettingRepository: mockProductInternalSettingRepository.Object,
                managePerson: mockManagePerson.Object,
                manageUserLogin: mockManageUserLogin.Object,
                managePartyRelationship: mockManagePartyRelationship.Object,
                manageElectronicAddress: mockManageElectronicAddress.Object,
                manageProductOneSite: mockManageProductOneSite.Object,
                userLoginRepository: null,
                repository: mockRepository.Object);

            // role list failed for superuser
            result = mpL2L.ManageLead2LeaseUser(_editorPersonaId, _userPersonaId, roleList, propertyList, out var additionalParameters3);
            _output.WriteLine("result 4 : " + result);
            Assert.True(result.ToUpper() == "" || result == CommonMessageConstants.RoleErrorMessage);
        }

        [Fact]
        public void Post_L2LUser()
        {
            //Arrange
            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockManagePerson = new Mock<IManagePerson>();
            var mockManageUserLogin = new Mock<IManageUserLogin>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            var mockManagePartyRelationship = new Mock<IManagePartyRelationship>();
            var mockManageElectronicAddress = new Mock<IManageElectronicAddress>();
            var mockHandler = new Mock<HttpMessageHandler>();
            var mockManageProductOneSite = new Mock<IManageProductOneSite>();

            #region Responses
            HttpResponseMessage responseProperties = new HttpResponseMessage(HttpStatusCode.OK);
            responseProperties.Content = new StringContent(JsonConvert.SerializeObject(_l2lPropertyList));

            HttpResponseMessage responseRoles = new HttpResponseMessage(HttpStatusCode.OK);
            responseRoles.Content = new StringContent(JsonConvert.SerializeObject(_l2lRoleInfo));

            HttpResponseMessage responseUser = new HttpResponseMessage(HttpStatusCode.OK);
            responseUser.Content = new StringContent(JsonConvert.SerializeObject(_l2lUser));

            HttpResponseMessage responseEmpty = new HttpResponseMessage(HttpStatusCode.OK);
            responseEmpty.Content = new StringContent("");

            HttpResponseMessage migrationResponse = new HttpResponseMessage(HttpStatusCode.OK);
            migrationResponse.Content = new StringContent(JsonConvert.SerializeObject(new MigrateResponse() { Message = "Success", Status = true }));

            #endregion

            Uri propertyListUri = new Uri(testHostname + "/Users/ActiveProperties/" + _blueBookId.ToString());
            Uri rolesUri = new Uri(testHostname + "/Users/ActiveRoles");
            Uri userUri = new Uri(testHostname + "/Users/5432");
            Uri createUserUri = new Uri(testHostname + "/Users/RealPage");
            Uri updateUserUri = new Uri(testHostname + "/Users/edit");
            Uri migrationUri = new Uri(testHostname + $"/{_blueBookId.ToString()}/migrate-users");

            IList<CustomerCompanyMap> mapResource = new List<CustomerCompanyMap>();
            CustomerCompanyMap resource = new CustomerCompanyMap() { CompanyInstanceSourceId = _blueBookId.ToString(), Source = "L2L" };
            mapResource.Add(resource);

            _editorPersona.Organization.BooksCustomerMasterId = _blueBookId;
            IC.Person person = new IC.Person() { FirstName = "Test First", LastName = "Test Last", PartyId = 30 };

            IC.PartyRelationship _partyRelationShip = new IC.PartyRelationship();
            _partyRelationShip.RoleTypeFrom = new IC.RoleType() { Name = _ROLETYPE_NAME_USER };

            IList<ProductProperty> productPropertyList = new List<ProductProperty>();
            productPropertyList.Add(new ProductProperty() { ID = "2211334", Name = "Test Property", IsAssigned = true });

            ListResponse osPropertyListResponse = new ListResponse();
            osPropertyListResponse.IsError = false;
            osPropertyListResponse.Records = productPropertyList.Cast<object>().ToList();

            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync"
                    , ItExpr.Is<HttpRequestMessage>(message => message.RequestUri.ToString().ToLower() == propertyListUri.ToString().ToLower())
                    , ItExpr.IsAny<CancellationToken>()
                )
                .Returns(Task.FromResult<HttpResponseMessage>(responseProperties))
                .Callback<HttpRequestMessage, CancellationToken>((r, c) =>
                {
                    Assert.Equal(HttpMethod.Get, r.Method);
                });

            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync"
                    , ItExpr.Is<HttpRequestMessage>(message => message.RequestUri.ToString().ToLower() == rolesUri.ToString().ToLower())
                    , ItExpr.IsAny<CancellationToken>()
                )
                .Returns(Task.FromResult<HttpResponseMessage>(responseRoles))
                .Callback<HttpRequestMessage, CancellationToken>((r, c) =>
                {
                    Assert.Equal(HttpMethod.Get, r.Method);
                });

            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync"
                    , ItExpr.Is<HttpRequestMessage>(message => message.RequestUri.ToString().ToLower() == userUri.ToString().ToLower())
                    , ItExpr.IsAny<CancellationToken>()
                )
                .Returns(Task.FromResult<HttpResponseMessage>(responseUser))
                .Callback<HttpRequestMessage, CancellationToken>((r, c) =>
                {
                    Assert.Equal(HttpMethod.Get, r.Method);
                });

            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync"
                    , ItExpr.Is<HttpRequestMessage>(message => message.RequestUri.ToString().ToLower() == createUserUri.ToString().ToLower())
                    , ItExpr.IsAny<CancellationToken>()
                )
                .Returns(Task.FromResult<HttpResponseMessage>(responseUser))
                .Callback<HttpRequestMessage, CancellationToken>((r, c) =>
                {
                    Assert.Equal(HttpMethod.Post, r.Method);
                });
            
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync"
                    , ItExpr.Is<HttpRequestMessage>(message => message.RequestUri.ToString().ToLower() == updateUserUri.ToString().ToLower())
                    , ItExpr.IsAny<CancellationToken>()
                )
                .Returns(Task.FromResult<HttpResponseMessage>(responseEmpty))
                .Callback<HttpRequestMessage, CancellationToken>((r, c) =>
                {
                    Assert.Equal(HttpMethod.Put, r.Method);
                });

            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync"
                    , ItExpr.Is<HttpRequestMessage>(message => message.RequestUri.ToString().ToLower() == migrationUri.ToString().ToLower())
                    , ItExpr.IsAny<CancellationToken>()
                )
                .Returns(Task.FromResult<HttpResponseMessage>(migrationResponse))
                .Callback<HttpRequestMessage, CancellationToken>((r, c) =>
                {
                    Assert.Equal(HttpMethod.Put, r.Method);
                });

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
                 .Returns(mapResource);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.Is<int>(l => l == (int)ProductEnum.Lead2Lease)
                ))
                .Returns(_productInternalSettings);

            mockRepository
                .Setup(m => m.GetMany<IC.ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct,
                    It.Is<object>(d => TestIsProductId(d, 6))))
                .Returns(_productInternalSettings);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 4)
                    , It.Is<int>(l => l == (int)ProductEnum.Lead2Lease)
                 ))
                 .Returns(_editorSamlAttributes);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 5)
                    , It.Is<int>(l => l == (int)ProductEnum.OneSite)
                 ))
                 .Returns(_userOneSiteSamlAttributes);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 5)
                    , It.Is<int>(l => l == (int)ProductEnum.Lead2Lease)
                 ))
                 .Returns(_emptySamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 4)
                 ))
                 .Returns(_editorPersona);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 5)
                 ))
                 .Returns(_userPersona);

            mockProductRepository
                .Setup(m => m.GetProductSettingsByPersona(
                    It.IsAny<long>()
                ))
                .Returns(_userProductSettings);

            mockManagePerson
                .Setup(m => m.GetPerson(
                    It.IsAny<Guid>()
                ))
                .Returns(person);

            mockManageUserLogin
                .Setup(m => m.GetUserLoginOnly(
                    It.IsAny<Guid>()
                ))
                .Returns(_userloginOnly);

            mockManagePartyRelationship
                .Setup(m => m.GetPartyRelationship(
                    It.IsAny<Guid>()
                    , It.IsAny<Guid>()
                    , null
                    , null
                    , It.IsAny<string>()
               ))
               .Returns(_partyRelationShip);

            mockManageElectronicAddress
                .Setup(m => m.ListElectronicAddressForPerson(
                    It.IsAny<Guid>()
                    , It.IsAny<string>()
                ))
                .Returns(_electronicAddressList);

            mockProductRepository
                .Setup(m => m.ListProductSettingType()
                )
                .Returns(_productSettingTypeList);

            mockManageProductOneSite
                .Setup(m => m.GetOneSiteUserInfo(
                    It.IsAny<string>()
                ))
                .Returns(_osUser);

            mockManageProductOneSite
                .Setup(m => m.GetOneSitePropertyList(
                    It.IsAny<long>(),
                    It.IsAny<long>(),
                    It.IsAny<bool>(),
                    null
                ))
                .Returns(osPropertyListResponse);

            mockManageProductOneSite
                .Setup(m => m.UserInLeasingAgentList(
                    It.IsAny<long>(),
                    It.IsAny<long>(),
                    It.IsAny<int>()
                ))
                .Returns(true);

            //Act
            IManageProductLead2Lease mpL2L = new ManageProductLead2Lease(
                editorRealPageId: _editorRealPageId,
                userClaim: _editorUserClaim,
                messageHandler: mockHandler.Object,
                samlRepository: mockSamlRepository.Object,
                managePersona: mockManagePersona.Object,
                manageBlueBook: mockManageBlueBook.Object,
                productRepository: mockProductRepository.Object,
                productInternalSettingRepository: mockProductInternalSettingRepository.Object,
                managePerson: mockManagePerson.Object,
                manageUserLogin: mockManageUserLogin.Object,
                managePartyRelationship: mockManagePartyRelationship.Object,
                manageElectronicAddress: mockManageElectronicAddress.Object,
                manageProductOneSite: mockManageProductOneSite.Object,
                userLoginRepository: null,
                repository: mockRepository.Object);

            List<string> roleList = new List<string>();
            List<string> propertyList = new List<string>();

            roleList.Add("1");
            roleList.Add("2");
            propertyList.Add("1234567");
            propertyList.Add("7654321");

            // normal user
            string result = mpL2L.ManageLead2LeaseUser(_editorPersonaId, _userPersonaId, roleList, propertyList, out var additionalParameters1);
            Assert.True(string.IsNullOrEmpty(result));

            roleList = new List<string>();
            propertyList = new List<string>();

            _partyRelationShip = new IC.PartyRelationship();
            _partyRelationShip.RoleTypeFrom = new IC.RoleType() { Name = _ROLETYPE_NAME_SUPERUSER };

            mockManagePartyRelationship
                .Setup(m => m.GetPartyRelationship(
                    It.IsAny<Guid>()
                    , It.IsAny<Guid>()
                    , null
                    , null
                    , It.IsAny<string>()
               ))
               .Returns(_partyRelationShip);

            // super user
            result = mpL2L.ManageLead2LeaseUser(_editorPersonaId, _userPersonaId, roleList, propertyList, out var additionalParameters2);
            Assert.True(string.IsNullOrEmpty(result));

            // update user
            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 5)
                    , It.Is<int>(l => l == (int)ProductEnum.Lead2Lease)
                 ))
                 .Returns(_userSamlAttributes);

            mpL2L = new ManageProductLead2Lease(
                editorRealPageId: _editorRealPageId,
                userClaim: _editorUserClaim,
                messageHandler: mockHandler.Object,
                samlRepository: mockSamlRepository.Object,
                managePersona: mockManagePersona.Object,
                manageBlueBook: mockManageBlueBook.Object,
                productRepository: mockProductRepository.Object,
                productInternalSettingRepository: mockProductInternalSettingRepository.Object,
                managePerson: mockManagePerson.Object,
                manageUserLogin: mockManageUserLogin.Object,
                managePartyRelationship: mockManagePartyRelationship.Object,
                manageElectronicAddress: mockManageElectronicAddress.Object,
                manageProductOneSite: mockManageProductOneSite.Object,
                userLoginRepository: null,
                repository: mockRepository.Object);

            roleList = new List<string>();
            propertyList = new List<string>();

            roleList.Add("1");
            roleList.Add("2");
            propertyList.Add("1234567");
            propertyList.Add("7654321");

            // update user
            result = mpL2L.ManageLead2LeaseUser(_editorPersonaId, _userPersonaId, roleList, propertyList, out var additionalParameters3);
            Assert.True(string.IsNullOrEmpty(result));
        }

        #endregion

        #region Migration tests
        [Fact]
        public void GetMigrationUsers_GivenEditorIdAndDataFilter_ShouldReturnListOfMarketingCenterUsers()
        {
            //Arrange
            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockManagePerson = new Mock<IManagePerson>();
            var mockManageUserLogin = new Mock<IManageUserLogin>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            var mockManagePartyRelationship = new Mock<IManagePartyRelationship>();
            var mockManageElectronicAddress = new Mock<IManageElectronicAddress>();
            var mockHandler = new Mock<HttpMessageHandler>();
            var mockManageProductOneSite = new Mock<IManageProductOneSite>();

            var mapResource = new List<CustomerCompanyMap>()
            {
                new CustomerCompanyMap() { CompanyInstanceSourceId = _companyInstanceSourceId.ToString(), Source = "L2L" }
            };
            
            mockManageBlueBook
                .Setup(m => m.GetCompanyMap(
                    It.IsAny<Guid>(),
                    It.IsAny<long>(),
                    It.Is<string>(l => l == "L2L"),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()
                 ))
                 .Returns(mapResource);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.Is<int>(l => l == (int)ProductEnum.Lead2Lease)
                ))
                .Returns(_productInternalSettings);

            mockRepository
                .Setup(m => m.GetMany<IC.ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct,
                    It.Is<object>(d => TestIsProductId(d, 6))))
                .Returns(_productInternalSettings);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 4)
                    , It.Is<int>(l => l == (int)ProductEnum.Lead2Lease)
                 ))
                 .Returns(_editorSamlAttributes);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 5)
                    , It.Is<int>(l => l == (int)ProductEnum.OneSite)
                 ))
                 .Returns(_userOneSiteSamlAttributes);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 5)
                    , It.Is<int>(l => l == (int)ProductEnum.Lead2Lease)
                 ))
                 .Returns(_emptySamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 4)
                 ))
                 .Returns(_editorPersona);

            mockProductRepository
             .Setup(m => m.GetBooksMasterProductDetail(
                 It.Is<int>(l => l == (int)ProductEnum.Lead2Lease)
             ))
             .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.Lead2Lease));

            var dataFilter = new RequestParameter()
            {
                Pages = new PageRequest()
                {
                    StartRow = 1,
                    ResultsPerPage = 1000
                },
                FilterBy = new Dictionary<string, string>() {
                    { "filter" , "NonMigrated" }
                }
            };
            var totalRecords = 5;
            var url = $"{_mtApiEndPoint}/{_companyInstanceSourceId}/users?filter=NonMigrated&startRow={dataFilter.Pages.StartRow}&resultsperpage={dataFilter.Pages.ResultsPerPage}";
            var editorPersonaId = _editorPersonaId;
            var actual = new List<MigrationUser>()
            {
                new MigrationUser() { FirstName = "Person", LastName = "1", Email = "person1@test.com", Username = "person1" },
                new MigrationUser() { FirstName = "Person", LastName = "2", Email = "person2@test.com", Username = "person2", Properties = new List<MigrationProperty>(){
                new MigrationProperty() { PropertyInstanceSourceId = "1" }, new MigrationProperty() { PropertyInstanceSourceId = "2" }
                } },
                new MigrationUser() { FirstName = "Person", LastName = "3", Email = "person3@test.com", Username = "person3" },
                new MigrationUser() { FirstName = "Person", LastName = "4", Email = "person4@test.com", Username = "person4" },
                new MigrationUser() { FirstName = "Person", LastName = "5", Email = "person5@test.com", Username = "person5" }
            };
            HttpResponseMessage userResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(actual))
            };

            mockHandler.Setup(HttpMethod.Get, url, userResponse);
            
            IManageProductLead2Lease mpL2L = new ManageProductLead2Lease(
                editorRealPageId: _editorRealPageId,
                userClaim: _editorUserClaim,
                messageHandler: mockHandler.Object,
                samlRepository: mockSamlRepository.Object,
                managePersona: mockManagePersona.Object,
                manageBlueBook: mockManageBlueBook.Object,
                productRepository: mockProductRepository.Object,
                productInternalSettingRepository: mockProductInternalSettingRepository.Object,
                managePerson: mockManagePerson.Object,
                manageUserLogin: mockManageUserLogin.Object,
                managePartyRelationship: mockManagePartyRelationship.Object,
                manageElectronicAddress: mockManageElectronicAddress.Object,
                manageProductOneSite: mockManageProductOneSite.Object,
                userLoginRepository: null,
                repository: mockRepository.Object);
            //Act
            var expected = mpL2L.GetMigrationUsers(editorPersonaId, dataFilter);

            _output.WriteLine("expected :" + JsonConvert.SerializeObject(expected));
            //Assert
            Assert.IsType<MigrationUser>(expected.Records[0]);
            Assert.True(expected.Records.Count == totalRecords);
            Assert.Same(expected.ErrorReason, "");
        }

        [Fact]
        public void UpdateUsersMigrationStatus_GivenEditorIdUserIdAndMigrateUser_ShouldReturnSuccessMessage()
        {
            //Arrange
            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockManagePerson = new Mock<IManagePerson>();
            var mockManageUserLogin = new Mock<IManageUserLogin>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            var mockManagePartyRelationship = new Mock<IManagePartyRelationship>();
            var mockManageElectronicAddress = new Mock<IManageElectronicAddress>();
            var mockHandler = new Mock<HttpMessageHandler>();
            var mockManageProductOneSite = new Mock<IManageProductOneSite>();

            IList<CustomerCompanyMap> mapResource = new List<CustomerCompanyMap>();
            CustomerCompanyMap resource = new CustomerCompanyMap() { CompanyInstanceSourceId = _companyInstanceSourceId.ToString(), Source = "L2L" };
            mapResource.Add(resource);
            mockManageBlueBook
                .Setup(m => m.GetCompanyMap(
                    It.IsAny<Guid>(),
                    It.IsAny<long>(),
                    It.Is<string>(l => l == "L2L"),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()
                 ))
                 .Returns(mapResource);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.Is<int>(l => l == (int)ProductEnum.Lead2Lease)
                ))
                .Returns(_productInternalSettings);

            mockRepository
                .Setup(m => m.GetMany<IC.ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct,
                    It.Is<object>(d => TestIsProductId(d, 6))))
                .Returns(_productInternalSettings);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 4)
                    , It.Is<int>(l => l == (int)ProductEnum.Lead2Lease)
                 ))
                 .Returns(_editorSamlAttributes);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 5)
                    , It.Is<int>(l => l == (int)ProductEnum.OneSite)
                 ))
                 .Returns(_userOneSiteSamlAttributes);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 5)
                    , It.Is<int>(l => l == (int)ProductEnum.Lead2Lease)
                 ))
                 .Returns(_emptySamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 4)
                 ))
                 .Returns(_editorPersona);

            mockProductRepository
             .Setup(m => m.GetBooksMasterProductDetail(
                 It.Is<int>(l => l == (int)ProductEnum.Lead2Lease)
             ))
             .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.Lead2Lease));
            
            var migrateUsers = new List<MigrateUser>()
            {
                new MigrateUser(){
                    UserId = "123456",
                    UnifiedLoginUserName = "abc@test.com",
                    UsingUnifiedLogin = true
                },
                new MigrateUser(){
                    UserId = "123457",
                    UnifiedLoginUserName = "abc@test.com",
                    UsingUnifiedLogin = true
                }
            };
            var expected = new MigrateResponse()
            {
                Message = "Success",
                Status = true
            };
            var url = $"{_mtApiEndPoint}/{_companyInstanceSourceId}/migrate-users";
            HttpResponseMessage userResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(expected))
            };

            mockHandler.Setup(HttpMethod.Put, url, userResponse);

            IManageProductLead2Lease mpL2L = new ManageProductLead2Lease(
                editorRealPageId: _editorRealPageId,
                userClaim: _editorUserClaim,
                messageHandler: mockHandler.Object,
                samlRepository: mockSamlRepository.Object,
                managePersona: mockManagePersona.Object,
                manageBlueBook: mockManageBlueBook.Object,
                productRepository: mockProductRepository.Object,
                productInternalSettingRepository: mockProductInternalSettingRepository.Object,
                managePerson: mockManagePerson.Object,
                manageUserLogin: mockManageUserLogin.Object,
                managePartyRelationship: mockManagePartyRelationship.Object,
                manageElectronicAddress: mockManageElectronicAddress.Object,
                manageProductOneSite: mockManageProductOneSite.Object,
                userLoginRepository: null,
                repository: mockRepository.Object);

            //Act
            var actual = mpL2L.UpdateUsersMigrationStatus(_editorPersonaId, migrateUsers);

            _output.WriteLine("expected :" + JsonConvert.SerializeObject(expected));
            _output.WriteLine("actual :" + JsonConvert.SerializeObject(actual));

            //Assert
            Assert.Equal(actual.Message, expected.Message);
            Assert.True(actual.Status);
        }
        #endregion

        #region User-Status

        [Fact]
        public void ChangeUserStatus_Given_SystemIdentifier_Enable_ShouldReturn_True()
        {
            //Arrange
            var pmcID = 123456;
            var editorPersonaId = _editorPersonaId;

            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockPersonaRepository = new Mock<IPersonaRepository>();
            var mockPersonRepository = new Mock<IPersonRepository>();
            var mockUserLoginRepository = new Mock<IUserLoginRepository>();
            var mockManagePerson = new Mock<IManagePerson>();
            var mockManageUserLogin = new Mock<IManageUserLogin>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            var mockManagePartyRelationship = new Mock<IManagePartyRelationship>();
            var mockManageElectronicAddress = new Mock<IManageElectronicAddress>();
            var mockHandler = new Mock<HttpMessageHandler>();
            var mockManageProductOneSite = new Mock<IManageProductOneSite>();

            IList<CustomerCompanyMap> mapResource = new List<CustomerCompanyMap>();
            CustomerCompanyMap resource = new CustomerCompanyMap() { CompanyInstanceSourceId = _companyInstanceSourceId.ToString(), Source = "L2L" };
            mapResource.Add(resource);
            mockManageBlueBook
                .Setup(m => m.GetCompanyMap(
                    It.IsAny<Guid>(),
                    It.IsAny<long>(),
                    It.Is<string>(l => l == "L2L"),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()
                 ))
                 .Returns(mapResource);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.Is<int>(l => l == (int)ProductEnum.Lead2Lease)
                ))
                .Returns(_productInternalSettings);

            mockRepository
                .Setup(m => m.GetMany<IC.ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct,
                    It.Is<object>(d => TestIsProductId(d, 6))))
                .Returns(_productInternalSettings);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 4)
                    , It.Is<int>(l => l == (int)ProductEnum.Lead2Lease)
                 ))
                 .Returns(_editorSamlAttributes);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 5)
                    , It.Is<int>(l => l == (int)ProductEnum.OneSite)
                 ))
                 .Returns(_userOneSiteSamlAttributes);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 5)
                    , It.Is<int>(l => l == (int)ProductEnum.Lead2Lease)
                 ))
                 .Returns(_emptySamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 4)
                 ))
                 .Returns(_editorPersona);

            mockProductRepository
               .Setup(m => m.GetBooksMasterProductDetail(
                   It.Is<int>(l => l == (int)ProductEnum.Lead2Lease)
               ))
               .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.Lead2Lease));

            IManageProductLead2Lease mpL2L = new ManageProductLead2Lease(
              editorRealPageId: _editorRealPageId,
              userClaim: _editorUserClaim,
              messageHandler: mockHandler.Object,
              samlRepository: mockSamlRepository.Object,
              managePersona: mockManagePersona.Object,
              manageBlueBook: mockManageBlueBook.Object,
              productRepository: mockProductRepository.Object,
              productInternalSettingRepository: mockProductInternalSettingRepository.Object,
              managePerson: mockManagePerson.Object,
              manageUserLogin: mockManageUserLogin.Object,
              managePartyRelationship: mockManagePartyRelationship.Object,
              manageElectronicAddress: mockManageElectronicAddress.Object,
              manageProductOneSite: mockManageProductOneSite.Object,
              userLoginRepository: null,
              repository: mockRepository.Object);

            var username = "testuser";
            var isActive = true;

            //Act
            var actual = true;

            //Assert
            Assert.True(actual);
        }

        [Fact]
        public void ChangeUserStatus_Given_SystemIdentifier_Disable_ShouldReturn_True()
        {
            //Arrange
            var pmcID = 123456;
            var editorPersonaId = _editorPersonaId;

            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockPersonaRepository = new Mock<IPersonaRepository>();
            var mockPersonRepository = new Mock<IPersonRepository>();
            var mockUserLoginRepository = new Mock<IUserLoginRepository>();
            var mockManagePerson = new Mock<IManagePerson>();
            var mockManageUserLogin = new Mock<IManageUserLogin>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            var mockManagePartyRelationship = new Mock<IManagePartyRelationship>();
            var mockManageElectronicAddress = new Mock<IManageElectronicAddress>();
            var mockHandler = new Mock<HttpMessageHandler>();
            var mockManageProductOneSite = new Mock<IManageProductOneSite>();

            IList<CustomerCompanyMap> mapResource = new List<CustomerCompanyMap>();
            CustomerCompanyMap resource = new CustomerCompanyMap() { CompanyInstanceSourceId = _companyInstanceSourceId.ToString(), Source = "L2L" };
            mapResource.Add(resource);
            mockManageBlueBook
                .Setup(m => m.GetCompanyMap(
                    It.IsAny<Guid>(),
                    It.IsAny<long>(),
                    It.Is<string>(l => l == "L2L"),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()
                 ))
                 .Returns(mapResource);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.Is<int>(l => l == (int)ProductEnum.Lead2Lease)
                ))
                .Returns(_productInternalSettings);

            mockRepository
                .Setup(m => m.GetMany<IC.ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct,
                    It.Is<object>(d => TestIsProductId(d, 6))))
                .Returns(_productInternalSettings);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 4)
                    , It.Is<int>(l => l == (int)ProductEnum.Lead2Lease)
                 ))
                 .Returns(_editorSamlAttributes);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 5)
                    , It.Is<int>(l => l == (int)ProductEnum.OneSite)
                 ))
                 .Returns(_userOneSiteSamlAttributes);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 5)
                    , It.Is<int>(l => l == (int)ProductEnum.Lead2Lease)
                 ))
                 .Returns(_emptySamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 4)
                 ))
                 .Returns(_editorPersona);

            mockProductRepository
              .Setup(m => m.GetBooksMasterProductDetail(
                  It.Is<int>(l => l == (int)ProductEnum.Lead2Lease)
              ))
              .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.Lead2Lease));

            IManageProductLead2Lease mpL2L = new ManageProductLead2Lease(
                editorRealPageId: _editorRealPageId,
                userClaim: _editorUserClaim,
                messageHandler: mockHandler.Object,
                samlRepository: mockSamlRepository.Object,
                managePersona: mockManagePersona.Object,
                manageBlueBook: mockManageBlueBook.Object,
                productRepository: mockProductRepository.Object,
                productInternalSettingRepository: mockProductInternalSettingRepository.Object,
                managePerson: mockManagePerson.Object,
                manageUserLogin: mockManageUserLogin.Object,
                managePartyRelationship: mockManagePartyRelationship.Object,
                manageElectronicAddress: mockManageElectronicAddress.Object,
                manageProductOneSite: mockManageProductOneSite.Object,
                userLoginRepository: null,
                repository: mockRepository.Object);

            var username = "testuser";
            var isActive = false;

            //Act
            var actual = true;

            //Assert
            Assert.True(actual);
        }

        #endregion
        private class RoleInfo
        {
            public IList<Preset> Presets { get; set; }
            public IList<Role> Roles { get; set; }
        }
    }
}
