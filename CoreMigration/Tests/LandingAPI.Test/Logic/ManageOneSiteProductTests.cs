using Moq;
using Moq.Language.Flow;
using Newtonsoft.Json;
using UnifiedLogin.BusinessLogic;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.Product.Interfaces;
using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.Product.OneSite;
using UnifiedLogin.SharedObjects.Saml;
using UnifiedLogin.LandingAPI.Test.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using Xunit;
using IC = UnifiedLogin.SharedObjects.IdentityConfig;

namespace UnifiedLogin.LandingAPI.Test.Logic
{
    /// <summary>
    /// Product xUnit tests
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ManageOneSiteProductTests : ManageProductBaseTests
    {
        private static string _pmcID = "1234566";
        private static string _systemIdentifier = "TUser1|1234566";

        private static string _soapExceptionText = "Server was unable to process request. ---> error";

	    private Mock<IOneSiteProductService> _mockService = new Mock<IOneSiteProductService>();
	    private Mock<ISamlRepository> _mockSamlRepository = new Mock<ISamlRepository>();
	    private Mock<IManagePersona> _mockManagePersona = new Mock<IManagePersona>();
	    private Mock<IPersonaRepository> _mockPersonaRepository = new Mock<IPersonaRepository>();
	    private Mock<IPersonRepository> _mockPersonRepository = new Mock<IPersonRepository>();
	    private Mock<IUserLoginRepository> _mockUserLoginRepository = new Mock<IUserLoginRepository>();
        private Mock<IUserRepository> _mockUserRepository = new Mock<IUserRepository>();
        private Mock<IUserLoginPersonaRepository> _mockUserLoginPersonaRepository = new Mock<IUserLoginPersonaRepository>();
        private Mock<IManagePerson> _mockManagePerson = new Mock<IManagePerson>();
	    private Mock<IManageUserLogin> _mockManageUserLogin = new Mock<IManageUserLogin>();
	    private Mock<IManageBlueBook> _mockManageBlueBook = new Mock<IManageBlueBook>();
	    private Mock<IProductRepository> _mockProductRepository = new Mock<IProductRepository>();
	    private Mock<IProductInternalSettingRepository> _mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
	    private Mock<IManagePartyRelationship> _mockManagePartyRelationship = new Mock<IManagePartyRelationship>();
	    private Mock<IManageElectronicAddress> _mockManageElectronicAddress = new Mock<IManageElectronicAddress>();

		private UserLoginOnly _userlogin = new UserLoginOnly();

		// GreenBook Property result
		private static ProductProperty _property1 = new ProductProperty() { ID = "1234567", Name = "Test Property", City = "Test City", State = "Test State", Street1 = "Test Street 1", Street2 = "Test Street 2", Zip = "12345", IsAssigned = true };
        private static ProductProperty _property2 = new ProductProperty() { ID = "7654321", Name = "Test Property 2", City = "Test City 2", State = "Test State 2", Street1 = "Test Street 1 2", Street2 = "Test Street 2 2", Zip = "54321", IsAssigned = false };
        private static ProductProperty _property3 = new ProductProperty() { ID = "2345678", Name = "Test Property 3", City = "Test City 3", State = "Test State 3", Street1 = "Test Street 1 3", Street2 = "Test Street 2 3", Zip = "54321", IsAssigned = true };

        // OneSite Property result
        private static PropertyType _propertyType1 = new PropertyType() { PropertyID = "1234567", PropertyName = "Test Property", SiteCityName = "Test City", SiteState = "Test State", SiteAddress = "Test Street 1", SitePhone = "123-456-7890", SiteZip = "12345", IsAssignedToUser = true };
        private static PropertyType _propertyType2 = new PropertyType() { PropertyID = "7654321", PropertyName = "Test Property 2", SiteCityName = "Test City 2", SiteState = "Test State 2", SiteAddress = "Test Street 1 2", SitePhone = "123-456-7890", SiteZip = "54321", IsAssignedToUser = false };
        private static PropertyType _propertyType3 = new PropertyType() { PropertyID = "2345678", PropertyName = "Test Property 3", SiteCityName = "Test City 3", SiteState = "Test State 3", SiteAddress = "Test Street 1 3", SitePhone = "222-555-7890", SiteZip = "54321", IsAssignedToUser = true };

        // GreenBook Role result
        private static ProductRole _role1 = new ProductRole() { ID = "1", Name = "Role 1", Roletype = "Default", IsAssigned = true };
        private static ProductRole _role2 = new ProductRole() { ID = "2", Name = "Role 2", Roletype = "External", IsAssigned = false };

        // OneSite Role result
        private static RoleType _roleType1 = new RoleType() { RoleID = "1", RoleName = "Role 1", IsInternal = true, Roletype = "Default", IsAssigned = true };
        private static RoleType _roleType2 = new RoleType() { RoleID = "2", RoleName = "Role 2", IsInternal = false, Roletype = "External", IsAssigned = false };
        private static RoleType _roleType3 = new RoleType() { RoleID = "21", RoleName = "Role 3", IsInternal = true, Roletype = "Default", IsAssigned = true };
        private static RoleType _roleType4 = new RoleType() { RoleID = "33", RoleName = "Developer", IsInternal = true, Roletype = "Default", IsAssigned = true };
        private static RoleType _roleType5 = new RoleType() { RoleID = "44", RoleName = "Internal Administrator", IsInternal = true, Roletype = "Default", IsAssigned = true };
        private static RoleType _roleType6 = new RoleType() { RoleID = "55", RoleName = "Internal User", IsInternal = true, Roletype = "Default", IsAssigned = true };
        private static RoleType _roleType7 = new RoleType() { RoleID = "66", RoleName = "Close Criminal Dispute", IsInternal = true, Roletype = "Default", IsAssigned = true };

        private static RoleType _roleTypeDeveloper = new RoleType() { RoleID = "3", RoleName = "Developer", IsInternal = true, Roletype = "Default", IsAssigned = false };
        private static RoleType _roleTypeInternalAdmin = new RoleType() { RoleID = "4", RoleName = "Internal Administrator", IsInternal = true, Roletype = "Default", IsAssigned = true };
        private static RoleType _roleTypeInternalUser = new RoleType() { RoleID = "5", RoleName = "Internal User", IsInternal = true, Roletype = "Default", IsAssigned = false };
        private static RoleType _roleTypeCloseCriminalDispute = new RoleType() { RoleID = "6", RoleName = "Close Criminal Dispute", IsInternal = true, Roletype = "Default", IsAssigned = false };

        // OneSite Right result
        private static RightType _rightType1 = new RightType() { RightID = "444", RightDescription = "Right 444", CenterName = "Core", Assigned = false, RolesAssigned = 5 };
        private static RightType _rightType2 = new RightType() { RightID = "55", RightDescription = "Right 55", CenterName = "Leasing and Rents", Assigned = true, RolesAssigned = 15 };
        private static RightType _rightType3 = new RightType() { RightID = "6", RightDescription = "Right 6", CenterName = "Core", Assigned = false, RolesAssigned = 2 };

        // OneSite User result
        private static UserType _userType1 = new UserType() { UserId = 12345, UserLogin = "testuser", UserName = "Test User", Assigned = true };
        private static UserType _userType2 = new UserType() { UserId = 3456, UserLogin = "anotheruser", UserName = "Another User", Assigned = false };
        private static UserType _userType3 = new UserType() { UserId = 45, UserLogin = "mylogin", UserName = "My Login", Assigned = true };

        private static List<NameValuePair> _userInfo1 = new List<NameValuePair>();
        private static List<NameValuePair> _userInfoEmpty = new List<NameValuePair>();

        private string _mtApiEndPoint = "api/core/common/ulmigration";
        private string _mtTokenUrl = "api/core/authentication/login";
        private string _mtClientId = "OneSiteClient";
        private string _mtClientSecret = "OneSiteClientSecret";

        private string _pmcUrl = "someurl.onesite.realpage.com";

        List<IC.ProductInternalSetting> _productInternalSettingsOneSite = new List<IC.ProductInternalSetting>();

        public ManageOneSiteProductTests() : base((int)ProductEnum.OneSite)
        {

            _editorSamlAttributes = new List<SamlAttributes>();
            _editorSamlAttributes.Add(new SamlAttributes() { Name = "UserId", Value = "1234567|username" });

            _userSamlAttributes = new List<SamlAttributes>();
            _userSamlAttributes.Add(new SamlAttributes() { Name = "UserId", Value = "1234567|username2" });

            _productSettingType.Add(new ProductSettingType() { ProductSettingTypeId = 1, Name = "ProductStatus" });

            _userProductSettings.Add(new ProductSettingList() { ProductId = 1, Name = "IsFavorite", Value = "1", ProductSettingId = 1234 });
            _userProductSettings.Add(new ProductSettingList() { ProductId = 1, Name = "AllProperties", Value = "1", ProductSettingId = 1234 });

            _electronicAddressList = new List<IC.ElectronicAddress>();
            _electronicAddressList.Add(new IC.ElectronicAddress() { AddressType = "Email", AddressString = "test" });

            _userInfo1.Add(new NameValuePair() { Name = "UserId", Value = "444" });
            _userInfo1.Add(new NameValuePair() { Name = "SystemIdentifier", Value = "1234567|username" });
            _userInfo1.Add(new NameValuePair() { Name = "FirstName", Value = "Test" });
            _userInfo1.Add(new NameValuePair() { Name = "LastName", Value = "User" });
            _userInfo1.Add(new NameValuePair() { Name = "UserPin", Value = "4321" });
            _userInfo1.Add(new NameValuePair() { Name = "UserAllProperty", Value = "0" });

            _userInfoEmpty.Add(new NameValuePair() { Name = null, Value = null });

            _repositoryResponseProductStatus.ErrorMessage = "";

            _productInternalSettingsOneSite.Add(new IC.ProductInternalSetting() { Name = "MTAPiEndPoint", Value = _mtApiEndPoint });
            _productInternalSettingsOneSite.Add(new IC.ProductInternalSetting() { Name = "MTTokenEndPoint", Value = _mtTokenUrl });
            _productInternalSettingsOneSite.Add(new IC.ProductInternalSetting() { Name = "MTClientId", Value = _mtClientId });
            _productInternalSettingsOneSite.Add(new IC.ProductInternalSetting() { Name = "MTClientSECRET", Value = _mtClientSecret });

            HttpResponseMessage tokenResponse = new HttpResponseMessage(HttpStatusCode.OK);
            tokenResponse.Content = new StringContent(JsonConvert.SerializeObject(new { access_token = "mocked access token" }));
            mockHttpMessageHandler.Setup(HttpMethod.Post, $"https://{_pmcUrl}/{_mtTokenUrl}", tokenResponse);

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct,
                    It.IsAny<object>()))
                .Returns(_gbProductMap);

            mockRepository
                .Setup(m => m.GetMany<IC.ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct,
                    It.Is<object>(d => TestIsProductId(d, (int)ProductEnum.OneSite))))
                .Returns(_productInternalSettingsOneSite);
        }

		#region OneSite

	    private void AssertInitial()
	    {
			//Arrange

		    RequestParameter reqParameter = new RequestParameter();
		    reqParameter.Pages.ResultsPerPage = 1000;
		    Dictionary<string, string> args = new Dictionary<string, string>();
		    args.Add("SortBy", "ASC");

			Dictionary<string, string> responseList = new Dictionary<string, string>();
		    responseList.Add("ISSUCCESSFUL", "1");
		    responseList.Add("SYSTEMIDENTIFIER", "1234567|newuserlogin");
		    responseList.Add("ERRORMESSAGE", "");

		    NameValuePair[] newUserResponse = (from a in responseList.ToList() select new NameValuePair { Name = a.Key, Value = a.Value }).ToArray();

		    RoleList resultList = new RoleList();
		    List<RoleType> _roleList = new List<RoleType>();

		    _roleList.Add(_roleType1);
		    _roleList.Add(_roleType2);
		    resultList.Role = _roleList.ToArray();
		    resultList.TotalRoles = _roleList.Count;

		    AssignStatus removeStatus = new AssignStatus();
		    AssignStatus addStatus = new AssignStatus();

		    List<PropertyType> _propList = new List<PropertyType>();
		    PropertyList propertyList = new PropertyList();

		    _propList.Add(_propertyType1);
		    _propList.Add(_propertyType2);
		    propertyList.Property = _propList.ToArray();
		    propertyList.TotalProperties = _propList.Count;

		    Persona persona = new Persona();
		    IC.Person person = new IC.Person() { FirstName = "Test First", LastName = "Test Last", PartyId = 30 };

		    _userlogin = new UserLoginOnly() { LoginName = "test@test.com", PartyId = 30, RealPageId = new Guid() };

            IList<IC.UserLoginPersona> userLoginPersona = new List<IC.UserLoginPersona>();
            userLoginPersona.Add(new IC.UserLoginPersona() { UserLoginPersonaId = 20, UserLoginId = 15 });

            UserEmployee userEmployee = new UserEmployee() { EmployeeId = "Employee123456", UserLoginPersonaId = 20 };

			_mockService
				 .Setup(m => m.CreateUser(
						It.IsAny<NameValuePair[]>()
				 ))
				 .Returns(newUserResponse);

			_mockService
				 .Setup(m => m.CreateSuperuser(
						It.IsAny<NameValuePair[]>()
				 ))
				 .Returns(newUserResponse);

			_mockService
				.Setup(m => m.EnableUser(
						It.IsAny<string>()
				));

			_mockService
				.Setup(m => m.DisableUser(
					It.IsAny<string>()
				));

			_mockService
				.Setup(m => m.GetAllRoles(
					It.IsAny<NameValuePair[]>()
					, It.IsAny<string>()
					, It.IsAny<FilterSortParameters>()
				 ))
				 .Returns(resultList);

		    _mockService
				.Setup(m => m.RemoveRolesFromUser(
					It.IsAny<string>()
					, It.IsAny<string>()
				))
				.Returns(removeStatus);

		    _mockService
				.Setup(m => m.AssignRolesToUser(
					It.IsAny<string>()
					, It.IsAny<string>()
				))
				.Returns(removeStatus);

			_mockService
				 .Setup(m => m.GetAllProperties(
					It.IsAny<NameValuePair[]>()
					, It.IsAny<string>()
					, It.IsAny<FilterSortParameters>()
				))
				.Returns(propertyList);

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
					It.Is<long>(l => l == 10)
					, It.IsAny<int>()
				 ))
				 .Returns(_emptySamlAttributes);

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
					It.Is<long>(l => l == 10)
				 ))
				 .Returns(_userPersona);

			_mockPersonaRepository
				.Setup(m => m.GetPersona(
					It.IsAny<long>(), true
				))
				.Returns(persona);

			_mockPersonRepository
				.Setup(m => m.GetPerson(
					It.IsAny<Guid>()
				))
				.Returns(person);

			_mockManagePerson
				.Setup(m => m.GetPerson(
					It.IsAny<Guid>()
				))
				.Returns(person);

			_mockManageUserLogin
				.Setup(m => m.GetUserLoginOnly(
					It.IsAny<Guid>()
				))
				.Returns(_userlogin);

			_mockUserLoginRepository
				.Setup(m => m.GetUserLoginOnly(
					It.IsAny<Guid>()
				))
				.Returns(_userlogin);

            _mockUserRepository
             .Setup(m => m.GetUserEmployeeId(
                 It.IsAny<long>()
                 , It.IsAny<long>()
             ))
             .Returns(userEmployee);

            _mockUserLoginPersonaRepository
                .Setup(m => m.ListUserLoginPersona(
                    null
                    , It.Is<long>(l => l == 15)
                    , It.Is<long>(l => l == 1234)
                ))
                .Returns(userLoginPersona);

            _mockProductRepository
				.Setup(m => m.ListProductSettingType(
				))
				.Returns(_productSettingType);

		    _mockProductRepository
			    .Setup(m => m.ListProducts(
				    null, null, null, null
			    ))
			    .Returns(_gbProductMap);

		    _mockProductRepository
			    .Setup(m => m.GetBooksMasterProductDetail(
				    It.Is<int>(l => l == 1)
			    ))
			    .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.OneSite));

            _mockProductRepository
				.Setup(m => m.CreateProductSetting(
					It.IsAny<long>()
					, It.IsAny<int>()
					, It.IsAny<int>()
					, It.IsAny<string>()
				))
				.Returns(_repositoryResponseProductStatus);

			_mockProductInternalSettingRepository
				.Setup(m => m.GetProductInternalSettings(
					It.IsAny<int>()
				))
				.Returns(_productInternalSettingsOneSite);

            _mockManageElectronicAddress
				.Setup(m => m.ListElectronicAddressForPerson(
					It.IsAny<Guid>()
					, It.IsAny<string>()
				))
				.Returns(_electronicAddressList);
		}

	    #region Persona
		[Fact]
        public void Get_InvalidPersona()
        {
            //Arrange
            RequestParameter reqParameter = new RequestParameter();
            reqParameter.Pages.ResultsPerPage = 1000;
            reqParameter.SortBy = new Dictionary<string, string>();
            reqParameter.SortBy.Add("SiteName", "ASC");
            reqParameter.FilterBy = new Dictionary<string, string>();
            reqParameter.FilterBy.Add("SiteName", "Test Property");

            var mockService = new Mock<IOneSiteProductService>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();

            List<SamlAttributes> attributes = new List<SamlAttributes>();
            IList<CustomerCompanyMap> mapResource = new List<CustomerCompanyMap>();
            CustomerCompanyMap resource = new CustomerCompanyMap() { CompanyInstanceSourceId = _pmcID, Source = "OS" };
            mapResource.Add(resource);
			IList<ProductSettingList> productSettingList = new List<ProductSettingList>() { new ProductSettingList() { Name = "OVERRIDEPMCID", Value = "1" } };
			
			mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.IsAny<long>()
                    , It.IsAny<int>()
                 ))
                 .Returns(attributes);

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

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.IsAny<long>()
                ))
                .Returns(_editorPersona);

            mockProductRepository
                .Setup(m => m.GetProductSettingsByPersona(
                    It.IsAny<long>()
                ))
                .Returns(_userProductSettings);

			mockProductRepository
				.Setup(m => m.GetProductSettings(
					It.IsAny<Guid>(),
					It.IsAny<int>()))
				.Returns(productSettingList);

			mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.IsAny<int>()
                ))
                .Returns(_productInternalSettingsOneSite);

            mockProductRepository
               .Setup(m => m.GetBooksMasterProductDetail(
                   It.Is<int>(l => l == 1)
               ))
               .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.OneSite));

            //Act
            IManageProductOneSite manageProductOneSite = new ManageProductOneSite(_editorRealPageId, _editorUserClaim, mockService.Object, mockSamlRepository.Object, mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object, mockProductInternalSettingRepository.Object, mockHttpMessageHandler.Object, mockRepository.Object);

            ListResponse resp = manageProductOneSite.GetOneSitePropertyList(0, 0, true, reqParameter);
            Assert.True(resp.TotalRows == 0);

            resp = manageProductOneSite.GetOneSitePropertyList(_editorPersonaId, 0, true, reqParameter);
            Assert.True(resp.TotalRows == 0);

        }
        #endregion

        #region Exceptions - Property
        [Fact]
        public void Get_UserProperties_WithFilterAndSort_NoData()
        {
            //Arrange
            RequestParameter reqParameter = new RequestParameter();
            reqParameter.Pages.ResultsPerPage = 1000;
            reqParameter.SortBy = new Dictionary<string, string>();
            reqParameter.SortBy.Add("SiteName", "ASC");
            reqParameter.FilterBy = new Dictionary<string, string>();
            reqParameter.FilterBy.Add("SiteName", "Test Property");

            var mockService = new Mock<IOneSiteProductService>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockProductRepository = new Mock<IProductRepository>();

            SamlRepository samlRepository = new SamlRepository();

            mockProductRepository
              .Setup(m => m.GetBooksMasterProductDetail(
                  It.Is<int>(l => l == 1)
              ))
              .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.OneSite));

            ProductInternalSettingRepository productInternalSettingRepository = new ProductInternalSettingRepository(mockRepository.Object);

            //Act
            IManageProductOneSite manageProductOneSite = new ManageProductOneSite(_editorRealPageId, _editorUserClaim, mockService.Object, samlRepository, mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object, productInternalSettingRepository, mockHttpMessageHandler.Object, mockRepository.Object);

            ListResponse resp = manageProductOneSite.GetOneSitePropertyList(_editorPersonaId, _userPersonaId, true, reqParameter);
            Assert.True(resp.TotalRows == 0);
        }

        [Fact]
        public void Get_UserProperties_WithNoFilterAndNoSort_NoData()
        {
            //Arrange
            var mockService = new Mock<IOneSiteProductService>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();

            SamlRepository samlRepository = new SamlRepository();

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.IsAny<int>()
                ))
                .Returns(_productInternalSettingsOneSite);

            mockProductRepository
              .Setup(m => m.GetBooksMasterProductDetail(
                  It.Is<int>(l => l == 1)
              ))
              .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.OneSite));
            //Act
            IManageProductOneSite manageProductOneSite = new ManageProductOneSite(_editorRealPageId, _editorUserClaim, mockService.Object, samlRepository, mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object, mockProductInternalSettingRepository.Object, mockHttpMessageHandler.Object, mockRepository.Object);

            ListResponse resp = manageProductOneSite.GetOneSitePropertyList(_editorPersonaId, _userPersonaId, true, null);
            Assert.True(resp.TotalRows == 0);
        }

        [Fact]
        public void Get_AllProperties_WithNoFilterAndNoSort_ExceptionThrown()
        {
            //Arrange
            var mockService = new Mock<IOneSiteProductService>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();

            SamlRepository samlRepository = new SamlRepository();
            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.IsAny<int>()))
                .Returns(_productInternalSettingsOneSite);

            mockProductRepository
              .Setup(m => m.GetBooksMasterProductDetail(
                  It.Is<int>(l => l == 1)))
              .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.OneSite));

            new RPObjectCache().BustCache();

            //Act
            IManageProductOneSite manageProductOneSite = new ManageProductOneSite(_editorRealPageId, _editorUserClaim, mockService.Object, samlRepository, mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object, mockProductInternalSettingRepository.Object,
                mockHttpMessageHandler.Object, mockRepository.Object);

            Persona persona = new Persona() { Organization = new Organization() { BooksMasterId = 1234 } };
            var exception = Record.Exception(() => manageProductOneSite.GetOneSitePropertyListAll(persona, null));
            Assert.NotNull(exception);
            Assert.IsType<System.Exception>(exception);
        }
        #endregion

        #region Exceptions - Roles
        [Fact]
        public void Get_UserRoles_WithFilterAndSort_NoData()
        {
            //Arrange
            RequestParameter reqParameter = new RequestParameter();
            reqParameter.Pages.ResultsPerPage = 1000;
            reqParameter.SortBy = new Dictionary<string, string>();
            reqParameter.SortBy.Add("SiteName", "ASC");
            reqParameter.FilterBy = new Dictionary<string, string>();
            reqParameter.FilterBy.Add("SiteName", "Test Property");

            var mockService = new Mock<IOneSiteProductService>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            var mockSamlRepository = new Mock<ISamlRepository>();

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.IsAny<int>()
                ))
                .Returns(_productInternalSettingsOneSite);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.IsAny<long>()
                ))
                .Returns(_editorPersona);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.IsAny<long>()
                    , It.IsAny<int>()
                 ))
                 .Returns(_editorSamlAttributes);

            mockProductRepository
                .Setup(m => m.GetProductSettingsByPersona(
                    It.IsAny<long>()
                ))
                .Returns(_userProductSettings);

            mockProductRepository
              .Setup(m => m.GetBooksMasterProductDetail(
                  It.Is<int>(l => l ==1)))
              .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.OneSite));

            //Act
            IManageProductOneSite manageProductOneSite = new ManageProductOneSite(_editorRealPageId, _editorUserClaim, mockService.Object, mockSamlRepository.Object, mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object, mockProductInternalSettingRepository.Object,
                mockHttpMessageHandler.Object, mockRepository.Object);
            ListResponse resp = manageProductOneSite.GetOneSiteRoleList(_editorPersonaId, _userPersonaId, false, reqParameter);
            Assert.True(resp.TotalRows == 0);
        }

        [Fact]
        public void Get_UserRoles_WithNoFilterAndNoSort_NoData()
        {
            //Arrange
            var mockService = new Mock<IOneSiteProductService>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            var mockSamlRepository = new Mock<ISamlRepository>();

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.IsAny<int>()
                ))
                .Returns(_productInternalSettingsOneSite);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.IsAny<long>()
                ))
                .Returns(_editorPersona);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.IsAny<long>()
                    , It.IsAny<int>()
                 ))
                 .Returns(_editorSamlAttributes);

            mockProductRepository
                .Setup(m => m.GetProductSettingsByPersona(
                    It.IsAny<long>()
                ))
                .Returns(_userProductSettings);

            mockProductRepository
                .Setup(m => m.GetBooksMasterProductDetail(
                    (int)ProductEnum.OneSite))
                .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.OneSite));

            //Act
            IManageProductOneSite manageProductOneSite = new ManageProductOneSite(_editorRealPageId, _editorUserClaim, mockService.Object, mockSamlRepository.Object, mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object, mockProductInternalSettingRepository.Object,
                mockHttpMessageHandler.Object, mockRepository.Object);
            ListResponse resp = manageProductOneSite.GetOneSiteRoleList(_editorPersonaId, _userPersonaId, false, null);
            Assert.True(resp.TotalRows == 0);
        }

        [Fact]
        public void Get_AllRoles_WithNoFilterAndNoSort_ExceptionThrown()
        {
            //Arrange
            var mockService = new Mock<IOneSiteProductService>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();

            SamlRepository samlRepository = new SamlRepository();
            ManagePersona managePersona = new ManagePersona();

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.IsAny<int>()
                ))
                .Returns(_productInternalSettingsOneSite);

            mockProductRepository
                .Setup(m => m.GetBooksMasterProductDetail(
                    It.Is<int>(l => l == 1)))
                .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.OneSite));
            //Act
            IManageProductOneSite manageProductOneSite = new ManageProductOneSite(_editorRealPageId, _editorUserClaim, mockService.Object, samlRepository, managePersona, mockManageBlueBook.Object, mockProductRepository.Object, mockProductInternalSettingRepository.Object,
                mockHttpMessageHandler.Object, mockRepository.Object);
            Persona persona = new Persona();
            var error =  manageProductOneSite.GetOneSiteRoleListAll(_editorPersonaId, null);
            Assert.NotNull(error);
            Assert.True(error.IsError);
        }

        [Fact]
        public void Post_Role_Exception()
        {
            //Arrange
            RequestParameter reqParameter = new RequestParameter();
            reqParameter.Pages.ResultsPerPage = 1000;

            var mockService = new Mock<IOneSiteProductService>();
            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            var mockManagePartyRelationship = new Mock<IManagePartyRelationship>();

            RoleList roleResultList = new RoleList();

            List<RoleType> _roleList = new List<RoleType>();

            _roleList.Add(_roleType1);
            roleResultList.Role = _roleList.ToArray();
            roleResultList.TotalRoles = _roleList.Count;

            Persona editorPersona = new Persona() { PersonaId = _editorPersonaId, RealPageId = _editorRealPageId };
            editorPersona.Organization = new Organization() { PartyId = _editorOrganizationPartyId };

            IC.PartyRelationship _partyRelationShip = new IC.PartyRelationship();
            _partyRelationShip.RoleTypeFrom = new IC.RoleType() { Name = _ROLETYPE_NAME_USER };

            //Act
            mockService
                 .Setup(m => m.AddUpdateRole(
                        It.IsAny<string>()
                        , It.IsAny<string>()
                        , It.IsAny<string>()
                        , It.IsAny<string>()
                 ))
                 .Throws(new Exception(_soapExceptionText));

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.IsAny<long>()
                    , It.IsAny<int>()
                 ))
                 .Returns(_editorSamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.IsAny<long>()
                ))
                .Returns(_editorPersona);

            mockProductRepository
                .Setup(m => m.GetProductSettingsByPersona(
                    It.IsAny<long>()
                ))
                .Returns(_userProductSettings);

            mockManagePartyRelationship
                .Setup(m => m.GetPartyRelationship(
                    It.IsAny<Guid>()
                    , It.IsAny<Guid>()
                    , null
                    , null
                    , It.IsAny<string>()
               ))
               .Returns(_partyRelationShip);

            mockProductRepository
                .Setup(m => m.GetBooksMasterProductDetail(
                    It.Is<int>(l => l == 1)))
                .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.OneSite));

            IManageProductOneSite manageProductOneSite = new ManageProductOneSite(
		        editorRealPageId: _editorRealPageId,
                userClaim: _editorUserClaim, messageHandler: mockHttpMessageHandler.Object,
		        service: mockService.Object,
		        userList: null,
		        roleList: roleResultList,
		        rightList: null,
		        propertyList: null,
				samlRepository: mockSamlRepository.Object,
		        managePersona: mockManagePersona.Object,
		        managePerson: null,
				manageUserLogin: null,
				userLoginRepository: null,
				personaRepository: null,
		        manageBlueBook: mockManageBlueBook.Object,
		        productRepository: mockProductRepository.Object,
		        productInternalSettingRepository: mockProductInternalSettingRepository.Object,
		        managePartyRelationship: mockManagePartyRelationship.Object,
				manageElectronicAddress: null,
                userLoginPersonaRepository: null,
                userRepository: null, 
                repository: mockRepository.Object,
                systemIdentifier: _systemIdentifier
            );

			int roleId = 0; // new role
            string roleName = "New Role";
            string inheritRoleId = "1";

            ListResponse resp = manageProductOneSite.AddUpdateRole(_editorPersonaId, roleId, roleName, inheritRoleId);

            //Assert
            Assert.True(resp.ErrorReason == "error"); // an error occured
        }

        [Fact]
        public void Delete_Role_Exception()
        {
            //Arrange
            RequestParameter reqParameter = new RequestParameter();
            reqParameter.Pages.ResultsPerPage = 1000;

            var mockService = new Mock<IOneSiteProductService>();
            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            var mockManagePartyRelationship = new Mock<IManagePartyRelationship>();

            RoleList roleResultList = new RoleList();

            List<RoleType> _roleList = new List<RoleType>();

            _roleList.Add(_roleType1);
            roleResultList.Role = _roleList.ToArray();
            roleResultList.TotalRoles = _roleList.Count;

            Persona editorPersona = new Persona() { PersonaId = _editorPersonaId, RealPageId = _editorRealPageId };
            editorPersona.Organization = new Organization() { PartyId = _editorOrganizationPartyId };

            IC.PartyRelationship _partyRelationShip = new IC.PartyRelationship();
            _partyRelationShip.RoleTypeFrom = new IC.RoleType() { Name = _ROLETYPE_NAME_USER };

            //Act
            mockService
                 .Setup(m => m.DeleteRole(
                        It.IsAny<string>()
                        , It.IsAny<int>()
                  ))
                 .Throws(new Exception(_soapExceptionText));

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.IsAny<long>()
                    , It.IsAny<int>()
                 ))
                 .Returns(_editorSamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.IsAny<long>()
                ))
                .Returns(_editorPersona);

            mockProductRepository
                .Setup(m => m.GetProductSettingsByPersona(
                    It.IsAny<long>()
                ))
                .Returns(_userProductSettings);

            mockManagePartyRelationship
                .Setup(m => m.GetPartyRelationship(
                    It.IsAny<Guid>()
                    , It.IsAny<Guid>()
                    , null
                    , null
                    , It.IsAny<string>()
               ))
               .Returns(_partyRelationShip);

            mockProductRepository
                   .Setup(m => m.GetBooksMasterProductDetail(
                       It.Is<int>(l => l ==1)))
                   .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.OneSite));

            IManageProductOneSite manageProductOneSite = new ManageProductOneSite(
		        editorRealPageId: _editorRealPageId,
                userClaim: _editorUserClaim, messageHandler: mockHttpMessageHandler.Object,
                service: mockService.Object,
		        userList: null,
		        roleList: roleResultList,
		        rightList: null,
		        propertyList: null,
				samlRepository: mockSamlRepository.Object,
		        managePersona: mockManagePersona.Object,
		        managePerson: null,
		        manageUserLogin: null,
		        userLoginRepository: null,
		        personaRepository: null,
				manageBlueBook: mockManageBlueBook.Object,
		        productRepository: mockProductRepository.Object,
		        productInternalSettingRepository: mockProductInternalSettingRepository.Object,
		        managePartyRelationship: mockManagePartyRelationship.Object,
		        manageElectronicAddress: null,
                userLoginPersonaRepository: null,
                userRepository: null, 
                repository: mockRepository.Object,
                systemIdentifier: _systemIdentifier
            );

			Persona persona = new Persona();
            persona.PersonaId = 5;
            int roleId = 10; // new role

            string resp = manageProductOneSite.DeleteRole(_editorPersonaId, roleId);
            Assert.True(resp == "error"); // an error occured
        }

        [Fact]
        public void Put_UpdateRoleToRights_Exception()
        {
            //Arrange
            RequestParameter reqParameter = new RequestParameter();
            reqParameter.Pages.ResultsPerPage = 1000;
            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("SortBy", "ASC");

            var mockService = new Mock<IOneSiteProductService>();
            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            var mockManagePartyRelationship = new Mock<IManagePartyRelationship>();

            RoleList roleResultList = new RoleList();

            List<string> _rolesToAdd = new List<string>();
            _rolesToAdd.Add("1");
            _rolesToAdd.Add("2");
            _rolesToAdd.Add("4");

            List<RoleType> _roleList = new List<RoleType>();

            _roleList.Add(_roleType1); // already assigned
            _roleList.Add(_roleType2); // not assigned
            _roleList.Add(_roleType3); // assigned and keep
            _roleList.Add(_roleTypeInternalAdmin); // assign and keep
            roleResultList.Role = _roleList.ToArray();
            roleResultList.TotalRoles = _roleList.Count;

            Persona editorPersona = new Persona() { PersonaId = _editorPersonaId, RealPageId = _editorRealPageId };
            editorPersona.Organization = new Organization() { PartyId = _editorOrganizationPartyId };

            AssignStatus assignStatus = new AssignStatus();
            assignStatus.Result = true;
            assignStatus.ErrorMessage = "";

            IC.PartyRelationship _partyRelationShip = new IC.PartyRelationship();
            _partyRelationShip.RoleTypeFrom = new IC.RoleType() { Name = _ROLETYPE_NAME_USER };

            //Act
            mockService
                .Setup(m => m.ModifyRoleToRights(
                    It.IsAny<string>()
                    , It.IsAny<int>()
                    , It.IsAny<string>()
                    , It.IsAny<string>()
                ))
                .Throws(new Exception(_soapExceptionText));

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.IsAny<long>()
                    , It.IsAny<int>()
                 ))
                 .Returns(_editorSamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.IsAny<long>()
                ))
                .Returns(_editorPersona);

            mockManagePartyRelationship
                .Setup(m => m.GetPartyRelationship(
                    It.IsAny<Guid>()
                    , It.IsAny<Guid>()
                    , null
                    , null
                    , It.IsAny<string>()
               ))
               .Returns(_partyRelationShip);

            mockProductRepository
              .Setup(m => m.GetBooksMasterProductDetail(
                  It.Is<int>(l => l == 1)))
              .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.OneSite));

            IManageProductOneSite manageProductOneSite = new ManageProductOneSite(
		        editorRealPageId: _editorRealPageId,
                userClaim: _editorUserClaim, 
                messageHandler: mockHttpMessageHandler.Object,
		        service: mockService.Object,
		        userList: null,
		        roleList: roleResultList,
		        rightList: null,
		        propertyList: null,
				samlRepository: mockSamlRepository.Object,
		        managePersona: mockManagePersona.Object,
		        managePerson: null,
		        manageUserLogin: null,
		        userLoginRepository: null,
		        personaRepository: null,
				manageBlueBook: mockManageBlueBook.Object,
		        productRepository: mockProductRepository.Object,
		        productInternalSettingRepository: mockProductInternalSettingRepository.Object,
		        managePartyRelationship: mockManagePartyRelationship.Object,
		        manageElectronicAddress: null,
                userLoginPersonaRepository: null,
                userRepository: null, 
                repository: mockRepository.Object,
                systemIdentifier: _systemIdentifier
            );

			Persona persona = new Persona();
            int roleId = 1;
            List<string> rightToAddList = new List<string>();
            List<string> rightToRemoveList = new List<string>();
            rightToAddList.Add("1");
            rightToRemoveList.Add("2");

            string resp = manageProductOneSite.UpdateRoleToRights(_editorPersonaId, roleId, rightToAddList, rightToRemoveList);

            Assert.True(resp == "error"); // an error occured

            resp = manageProductOneSite.UpdateRoleToRights(_editorPersonaId, roleId, rightToAddList, null);
            Assert.True(resp == "error"); // an error occured

            resp = manageProductOneSite.UpdateRoleToRights(_editorPersonaId, roleId, null, rightToRemoveList);
            Assert.True(resp == "error"); // an error occured
        }

        [Fact]
        public void Put_UpdateRightToRoles_Exception()
        {
            //Arrange
            RequestParameter reqParameter = new RequestParameter();
            reqParameter.Pages.ResultsPerPage = 1000;
            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("SortBy", "ASC");

            var mockService = new Mock<IOneSiteProductService>();
            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            var mockManagePartyRelationship = new Mock<IManagePartyRelationship>();

            RoleList roleResultList = new RoleList();

            List<string> _rolesToAdd = new List<string>();
            _rolesToAdd.Add("1");
            _rolesToAdd.Add("2");
            _rolesToAdd.Add("4");

            List<RoleType> _roleList = new List<RoleType>();

            _roleList.Add(_roleType1); // already assigned
            _roleList.Add(_roleType2); // not assigned
            _roleList.Add(_roleType3); // assigned and keep
            _roleList.Add(_roleTypeInternalAdmin); // assign and keep
            roleResultList.Role = _roleList.ToArray();
            roleResultList.TotalRoles = _roleList.Count;

            //Act
            mockService
                .Setup(m => m.GetRolesForRight(
                    It.IsAny<NameValuePair[]>()
                    , It.IsAny<int>()
                    , It.IsAny<bool>()
                    , It.IsAny<FilterSortParameters>()
                ))
                .Returns(roleResultList);

            Persona editorPersona = new Persona() { PersonaId = _editorPersonaId, RealPageId = _editorRealPageId };
            editorPersona.Organization = new Organization() { PartyId = _editorOrganizationPartyId };

            AssignStatus assignStatus = new AssignStatus();
            assignStatus.Result = true;
            assignStatus.ErrorMessage = "";

            IC.PartyRelationship _partyRelationShip = new IC.PartyRelationship();
            _partyRelationShip.RoleTypeFrom = new IC.RoleType() { Name = _ROLETYPE_NAME_USER };

            //Act
            mockService
                .Setup(m => m.ModifyRightToRoles(
                    It.IsAny<string>()
                    , It.IsAny<int>()
                    , It.IsAny<string>()
                    , It.IsAny<bool>()
                ))
                .Throws(new Exception(_soapExceptionText));

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.IsAny<long>()
                    , It.IsAny<int>()
                 ))
                 .Returns(_editorSamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.IsAny<long>()
                ))
                .Returns(_editorPersona);

            mockManagePartyRelationship
                .Setup(m => m.GetPartyRelationship(
                    It.IsAny<Guid>()
                    , It.IsAny<Guid>()
                    , null
                    , null
                    , It.IsAny<string>()
               ))
               .Returns(_partyRelationShip);

            mockProductRepository
              .Setup(m => m.GetBooksMasterProductDetail(
                  It.Is<int>(l => l == 1)))
              .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.OneSite));

            IManageProductOneSite manageProductOneSite = new ManageProductOneSite(
		        editorRealPageId: _editorRealPageId,
                userClaim: _editorUserClaim, messageHandler: mockHttpMessageHandler.Object,
                service: mockService.Object,
		        userList: null,
		        roleList: roleResultList,
		        rightList: null,
		        propertyList: null,
				samlRepository: mockSamlRepository.Object,
		        managePersona: mockManagePersona.Object,
		        managePerson: null,
		        manageUserLogin: null,
		        userLoginRepository: null,
		        personaRepository: null,
				manageBlueBook: mockManageBlueBook.Object,
		        productRepository: mockProductRepository.Object,
		        productInternalSettingRepository: mockProductInternalSettingRepository.Object,
		        managePartyRelationship: mockManagePartyRelationship.Object,
		        manageElectronicAddress: null,
                userLoginPersonaRepository: null,
                userRepository: null,
                repository: mockRepository.Object,
                systemIdentifier: _systemIdentifier
            );
			Persona persona = new Persona();
            int rightId = 1;
            List<string> roleList = new List<string>();
            roleList.Add("1");
            string resp = manageProductOneSite.UpdateRightToRoles(_editorPersonaId, rightId, roleList, true);

            Assert.True(resp == "error"); // error message returned excludes Server was unable... info
        }

        [Fact]
        public void Get_RolesForRight_Exception()
        {
            //Arrange
            RequestParameter reqParameter = new RequestParameter();
            reqParameter.Pages.ResultsPerPage = 1000;
            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("SortBy", "ASC");

            var mockService = new Mock<IOneSiteProductService>();
            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            var mockManagePartyRelationship = new Mock<IManagePartyRelationship>();

            RoleList roleResultList = new RoleList();

            Persona editorPersona = new Persona() { PersonaId = _editorPersonaId, RealPageId = _editorRealPageId };
            editorPersona.Organization = new Organization() { PartyId = _editorOrganizationPartyId };

            AssignStatus removeStatus = new AssignStatus();
            AssignStatus addStatus = new AssignStatus();

            IC.PartyRelationship _partyRelationShip = new IC.PartyRelationship();
            _partyRelationShip.RoleTypeFrom = new IC.RoleType() { Name = _ROLETYPE_NAME_USER };

            //Act
            mockService
                .Setup(m => m.GetRolesForRight(
                    It.IsAny<NameValuePair[]>()
                    , It.IsAny<int>()
                    , It.IsAny<bool>()
                    , It.IsAny<FilterSortParameters>()
                ))
                .Throws(new Exception("error"));

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.IsAny<long>()
                    , It.IsAny<int>()
                 ))
                 .Returns(_editorSamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.IsAny<long>()
                ))
                .Returns(_editorPersona);

            mockManagePartyRelationship
                .Setup(m => m.GetPartyRelationship(
                    It.IsAny<Guid>()
                    , It.IsAny<Guid>()
                    , null
                    , null
                    , It.IsAny<string>()
               ))
               .Returns(_partyRelationShip);

            mockProductRepository
              .Setup(m => m.GetBooksMasterProductDetail(
                  It.Is<int>(l => l == 1)))
              .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.OneSite));

            IManageProductOneSite manageProductOneSite = new ManageProductOneSite(
		        editorRealPageId: _editorRealPageId,
                userClaim: _editorUserClaim, messageHandler: mockHttpMessageHandler.Object,
                service: mockService.Object,
		        userList: null,
		        roleList: roleResultList,
		        rightList: null,
		        propertyList: null,
				samlRepository: mockSamlRepository.Object,
		        managePersona: mockManagePersona.Object,
		        managePerson: null,
		        manageUserLogin: null,
		        userLoginRepository: null,
		        personaRepository: null,
				manageBlueBook: mockManageBlueBook.Object,
		        productRepository: mockProductRepository.Object,
		        productInternalSettingRepository: mockProductInternalSettingRepository.Object,
		        managePartyRelationship: mockManagePartyRelationship.Object,
		        manageElectronicAddress: null,
                userLoginPersonaRepository: null,
                userRepository: null, 
                repository: mockRepository.Object,
                systemIdentifier: _systemIdentifier
            );
			Persona persona = new Persona();
            int rightId = 0;
            bool assignedOnly = true;
            var exception = Record.Exception(() => manageProductOneSite.GetRolesForRight(_editorPersonaId, rightId, assignedOnly, reqParameter));
            Assert.NotNull(exception);
            Assert.IsType<System.ArgumentNullException>(exception);
        }

        #endregion

        #region Exceptions - User
        [Fact]
        public void Delete_User_Exception()
        {
            //Arrange
            RequestParameter reqParameter = new RequestParameter();
            reqParameter.Pages.ResultsPerPage = 1000;
            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("SortBy", "ASC");

            var mockService = new Mock<IOneSiteProductService>();
            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();

            Persona editorPersona = new Persona() { PersonaId = _editorPersonaId, RealPageId = _editorRealPageId };
            editorPersona.Organization = new Organization() { PartyId = _editorOrganizationPartyId };

            //Act
            mockService
                 .Setup(m => m.DeleteUser(
                        It.IsAny<string>()
                        , It.IsAny<string>()
                 ))
                 .Throws(new Exception(_soapExceptionText));

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.IsAny<long>()
                    , It.IsAny<int>()
                 ))
                 .Returns(_editorSamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.IsAny<long>()
                ))
                .Returns(_editorPersona);

            mockProductRepository
                .Setup(m => m.GetProductSettingsByPersona(
                    It.IsAny<long>()
                ))
                .Returns(_userProductSettings);

            mockProductRepository
                .Setup(m => m.ListProductSettingType(
                ))
                .Returns(_productSettingType);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.IsAny<int>()
                ))
                .Returns(_productInternalSettingsOneSite);

            mockProductRepository
              .Setup(m => m.GetBooksMasterProductDetail(
                  It.Is<int>(l => l == 1)))
              .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.OneSite));

            IManageProductOneSite manageProductOneSite = new ManageProductOneSite(_editorRealPageId, _editorUserClaim, mockService.Object, mockSamlRepository.Object, mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object, mockProductInternalSettingRepository.Object,
                mockHttpMessageHandler.Object, repository: mockRepository.Object);
            Persona persona = new Persona();
            long deletePersonaId = 10;
            string result = manageProductOneSite.DeleteOneSiteUser(_editorPersonaId, deletePersonaId);

            //Assert
            Assert.True(result.ToUpper() == "THERE WAS A PROBLEM DELETING THE USER");
        }

        [Fact]
        public void Put_UserStatus_Exception()
        {
            //Arrange
            RequestParameter reqParameter = new RequestParameter();
            reqParameter.Pages.ResultsPerPage = 1000;
            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("SortBy", "ASC");

            var mockService = new Mock<IOneSiteProductService>();
            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();

            Persona editorPersona = new Persona() { PersonaId = _editorPersonaId, RealPageId = _editorRealPageId };
            editorPersona.Organization = new Organization() { PartyId = _editorOrganizationPartyId };

            //Act
            mockService
                 .Setup(m => m.EnableUser(
                        It.IsAny<string>()
                 ))
                 .Throws(new Exception(_soapExceptionText));

            mockService
                 .Setup(m => m.DisableUser(
                        It.IsAny<string>()
                 ))
                 .Throws(new Exception(_soapExceptionText));

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.IsAny<long>()
                    , It.IsAny<int>()
                 ))
                 .Returns(_editorSamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.IsAny<long>()
                ))
                .Returns(_editorPersona);

            mockProductRepository
                .Setup(m => m.GetProductSettingsByPersona(
                    It.IsAny<long>()
                ))
                .Returns(_userProductSettings);

            mockProductRepository
                .Setup(m => m.ListProductSettingType(
                ))
                .Returns(_productSettingType);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.IsAny<int>()
                ))
                .Returns(_productInternalSettingsOneSite);

            mockProductRepository
               .Setup(m => m.GetBooksMasterProductDetail(
                   It.Is<int>(l => l == 1)))
               .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.OneSite));

            IManageProductOneSite manageProductOneSite = new ManageProductOneSite(_editorRealPageId, _editorUserClaim, mockService.Object, mockSamlRepository.Object, mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object, mockProductInternalSettingRepository.Object,
                mockHttpMessageHandler.Object, repository: mockRepository.Object);
            Persona persona = new Persona();

            string result = manageProductOneSite.EnableOneSiteUser(_editorPersonaId, _userPersonaId, true);

            //Assert
            Assert.True(string.IsNullOrEmpty(result));

            result = manageProductOneSite.EnableOneSiteUser(_editorPersonaId, _userPersonaId, false);
            Assert.True(string.IsNullOrEmpty(result));
        }
        #endregion

        #region Exceptions - Rights
        [Fact]
        public void Get_Rights_WithFilterAndSort_ExceptionThrown()
        {
            //Arrange
            RequestParameter reqParameter = new RequestParameter();
            reqParameter.Pages.ResultsPerPage = 1000;
            reqParameter.SortBy = new Dictionary<string, string>();
            reqParameter.SortBy.Add("RightDescription", "ASC");
            reqParameter.FilterBy = new Dictionary<string, string>();
            reqParameter.FilterBy.Add("RightDescription", "View");

            var mockService = new Mock<IOneSiteProductService>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();

            SamlRepository samlRepository = new SamlRepository();

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.IsAny<long>()
                ))
                .Returns(_editorPersona);

            mockProductRepository
                .Setup(m => m.GetProductSettingsByPersona(
                    It.IsAny<long>()
                ))
                .Returns(_userProductSettings);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.IsAny<int>()
                ))
                .Returns(_productInternalSettingsOneSite);

            mockProductRepository
              .Setup(m => m.GetBooksMasterProductDetail(
                  It.Is<int>(l => l == 1)))
              .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.OneSite));

            new RPObjectCache().BustCache();

            //Act
            IManageProductOneSite manageProductOneSite = new ManageProductOneSite(_editorRealPageId, _editorUserClaim, mockService.Object, samlRepository, mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object, mockProductInternalSettingRepository.Object,
                mockHttpMessageHandler.Object, repository: mockRepository.Object);
            var exception = Record.Exception(() => manageProductOneSite.GetOneSiteRights(_editorPersonaId, null, 0));
            Assert.NotNull(exception);
            Assert.IsType<System.Exception>(exception);
        }

        [Fact]
        public void Get_Rights_WithNoFilterAndNoSort_ExceptionThrown()
        {
            //Arrange
            var mockService = new Mock<IOneSiteProductService>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockProductRepository = new Mock<IProductRepository>();
            
            SamlRepository samlRepository = new SamlRepository();

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.IsAny<long>()
                ))
                .Returns(_editorPersona);

            mockProductRepository
                .Setup(m => m.GetProductSettingsByPersona(
                    It.IsAny<long>()
                ))
                .Returns(_userProductSettings);
            
            mockProductRepository
                .Setup(m => m.GetBooksMasterProductDetail(
                    It.Is<int>(l => l == 1)))
                .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.OneSite));

            ProductInternalSettingRepository productInternalSettingRepository = new ProductInternalSettingRepository(mockRepository.Object);

            //Act
            IManageProductOneSite manageProductOneSite = new ManageProductOneSite(_editorRealPageId, _editorUserClaim, mockService.Object, samlRepository, mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object, productInternalSettingRepository,
                mockHttpMessageHandler.Object, repository: mockRepository.Object);
            Persona persona = new Persona();
            var exception = Record.Exception(() => manageProductOneSite.GetOneSiteRights(_editorPersonaId, null, 1));
            Assert.NotNull(exception);
            Assert.IsType<System.Exception>(exception);
        }

        [Fact]
        public void Get_RightCenters__ExceptionThrown()
        {
            //Arrange
            var mockService = new Mock<IOneSiteProductService>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockProductRepository = new Mock<IProductRepository>();

            SamlRepository samlRepository = new SamlRepository();

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.IsAny<long>()
                ))
                .Returns(_editorPersona);

            mockProductRepository
                .Setup(m => m.GetProductSettingsByPersona(
                    It.IsAny<long>()
                ))
                .Returns(_userProductSettings);

            mockProductRepository
                .Setup(m => m.GetBooksMasterProductDetail(
                    It.Is<int>(l => l == 1)))
                .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.OneSite));

            ProductInternalSettingRepository productInternalSettingRepository = new ProductInternalSettingRepository(mockRepository.Object);

            //Act
            IManageProductOneSite manageProductOneSite = new ManageProductOneSite(_editorRealPageId, _editorUserClaim, mockService.Object, samlRepository, mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object, productInternalSettingRepository,
                mockHttpMessageHandler.Object, repository: mockRepository.Object);
            Persona persona = new Persona();
            var exception = Record.Exception(() => manageProductOneSite.GetOneSiteRightsCenters(_editorPersonaId));
            Assert.NotNull(exception);
            Assert.IsType<System.Exception>(exception);
        }
        #endregion

        #region Properties
        [Fact]
        public void Get_UserProperties_UserExists()
        {
            //Arrange
            RequestParameter reqParameter = new RequestParameter();
            reqParameter.Pages.ResultsPerPage = 1000;

            var mockService = new Mock<IOneSiteProductService>();
            PropertyList propertyResultList = new PropertyList();

            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            var mockManagePartyRelationship = new Mock<IManagePartyRelationship>();

            List<PropertyType> _propList = new List<PropertyType>();
            _propList.Add(_propertyType1);
            _propList.Add(_propertyType2);
            propertyResultList.Property = _propList.ToArray();
            propertyResultList.TotalProperties = _propList.Count;

            IC.PartyRelationship _partyRelationShipSuperUser = new IC.PartyRelationship();
            _partyRelationShipSuperUser.RoleTypeFrom = new IC.RoleType() { Name = _ROLETYPE_NAME_USER };

            //Act
            mockService
                 .Setup(m => m.GetUserProperties(
                        It.IsAny<string>()
                        , It.IsAny<bool>()
                        , It.IsAny<FilterSortParameters>()
                 ))
                 .Returns(propertyResultList);

            mockService
                .Setup(m => m.GetUser(
                        It.IsAny<NameValuePair[]>()
                ))
                .Returns(_userInfo1.ToArray());

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.IsAny<long>()
                    , It.IsAny<int>()
                 ))
                 .Returns(_editorSamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.IsAny<long>()
                ))
                .Returns(_editorPersona);

            mockProductRepository
                .Setup(m => m.GetProductSettingsByPersona(
                    It.IsAny<long>()
                ))
                .Returns(_userProductSettings);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.IsAny<int>()
                ))
                .Returns(_productInternalSettingsOneSite);

            mockManagePartyRelationship
                .Setup(m => m.GetPartyRelationship(
                    It.IsAny<Guid>()
                    , It.IsAny<Guid>()
                    , null
                    , null
                    , It.IsAny<string>()
               ))
               .Returns(_partyRelationShipSuperUser);

            mockProductRepository
                .Setup(m => m.GetBooksMasterProductDetail(
                    It.Is<int>(l => l == 1)))
                .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.OneSite));

            IManageProductOneSite manageProductOneSite = new ManageProductOneSite(
		        editorRealPageId: _editorRealPageId,
                userClaim: _editorUserClaim, messageHandler: mockHttpMessageHandler.Object,
                service: mockService.Object,
		        userList: null,
		        roleList: null,
		        rightList: null,
		        propertyList: propertyResultList,
		        samlRepository: mockSamlRepository.Object,
		        managePersona: mockManagePersona.Object,
		        managePerson: null,
		        manageUserLogin: null,
		        userLoginRepository: null,
		        personaRepository: null,
				manageBlueBook: mockManageBlueBook.Object,
		        productRepository: mockProductRepository.Object,
		        productInternalSettingRepository: mockProductInternalSettingRepository.Object,
		        managePartyRelationship: mockManagePartyRelationship.Object,
		        manageElectronicAddress: null,
                userLoginPersonaRepository: null,
                userRepository: null, 
                repository: mockRepository.Object,
                systemIdentifier: _systemIdentifier
            );

			ListResponse resp = manageProductOneSite.GetOneSitePropertyList(_editorPersonaId, _userPersonaId, true, reqParameter);

            ProductProperty _responseProperty1 = resp.Records[0] as ProductProperty;
            ProductProperty _responseProperty2 = resp.Records[1] as ProductProperty;

            //Assert
            Assert.True(_propertyType1.PropertyID == _responseProperty1.ID
                && _propertyType1.PropertyName == _responseProperty1.Name
                && _propertyType1.SiteCityName == _responseProperty1.City
                && _propertyType1.SiteState == _responseProperty1.State
                && _propertyType1.SiteAddress == _responseProperty1.Street1
                && _propertyType1.SiteZip == _responseProperty1.Zip
                && _propertyType1.IsAssignedToUser == _responseProperty1.IsAssigned
            );
            Assert.True(_propertyType2.PropertyID == _responseProperty2.ID
                && _propertyType2.PropertyName == _responseProperty2.Name
                && _propertyType2.SiteCityName == _responseProperty2.City
                && _propertyType2.SiteState == _responseProperty2.State
                && _propertyType2.SiteAddress == _responseProperty2.Street1
                && _propertyType2.SiteZip == _responseProperty2.Zip
                && _propertyType2.IsAssignedToUser == _responseProperty2.IsAssigned
            );

            mockService
                .Setup(m => m.GetUser(
                        It.IsAny<NameValuePair[]>()
                ))
                .Returns(_userInfoEmpty.ToArray());

	        manageProductOneSite = new ManageProductOneSite(
		        editorRealPageId: _editorRealPageId,
                userClaim: _editorUserClaim, messageHandler: mockHttpMessageHandler.Object,
                service: mockService.Object,
		        userList: null,
		        roleList: null,
		        rightList: null,
		        propertyList: propertyResultList,
		        samlRepository: mockSamlRepository.Object,
		        managePersona: mockManagePersona.Object,
		        managePerson: null,
		        manageUserLogin: null,
		        userLoginRepository: null,
		        personaRepository: null,
				manageBlueBook: mockManageBlueBook.Object,
		        productRepository: mockProductRepository.Object,
		        productInternalSettingRepository: mockProductInternalSettingRepository.Object,
		        managePartyRelationship: mockManagePartyRelationship.Object,
		        manageElectronicAddress: null,
                userLoginPersonaRepository: null,
                userRepository: null, 
                repository: mockRepository.Object,
                systemIdentifier: _systemIdentifier
            );

			resp = manageProductOneSite.GetOneSitePropertyList(_editorPersonaId, _userPersonaId, true, reqParameter);

			//Assert
			Assert.True(resp.IsError && (resp.ErrorReason.ToUpper() == "THERE WAS A PROBLEM GETTING THE LIST OF PROPERTIES" ||
                        resp.ErrorReason == CommonMessageConstants.PropertyErrorMessage ||
                        resp.ErrorReason == CommonMessageConstants.CompanyErrorMessage));
        }

        [Fact]
        public void Get_PropertyList_All()
        {
            //Arrange
            RequestParameter reqParameter = new RequestParameter();
            reqParameter.Pages.ResultsPerPage = 1000;
            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("SortBy", "ASC");

            var mockService = new Mock<IOneSiteProductService>();
            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            var mockManagePartyRelationship = new Mock<IManagePartyRelationship>();

            PropertyList propertyResultList = new PropertyList();
            PropertyList emptyResultList = new PropertyList();

            List<PropertyType> _propList = new List<PropertyType>();

            _propList.Add(_propertyType1);
            _propList.Add(_propertyType2);
            propertyResultList.Property = _propList.ToArray();
            propertyResultList.TotalProperties = _propList.Count;

            ListResponse resp;

            IList<CustomerCompanyMap> companyMapList = new List<CustomerCompanyMap>();

            IC.PartyRelationship _partyRelationShipSuperUser = new IC.PartyRelationship();
            _partyRelationShipSuperUser.RoleTypeFrom = new IC.RoleType() { Name = _ROLETYPE_NAME_SUPERUSER };

            //Act
            mockService
                 .Setup(m => m.GetAllProperties(
                        It.IsAny<NameValuePair[]>()
                        , It.IsAny<string>()
                        , It.IsAny<FilterSortParameters>()
                 ))
                 .Throws(new Exception());

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.IsAny<long>()
                    , It.IsAny<int>()
                 ))
                 .Returns(_editorSamlAttributes);

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
                 .Returns(companyMapList);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.IsAny<long>()
                ))
                .Returns(_editorPersona);

            mockProductRepository
                .Setup(m => m.GetProductSettingsByPersona(
                    It.IsAny<long>()
                ))
                .Returns(_userProductSettings);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.IsAny<int>()
                ))
                .Returns(_productInternalSettingsOneSite);

            mockManagePartyRelationship
                .Setup(m => m.GetPartyRelationship(
                    It.IsAny<Guid>()
                    , It.IsAny<Guid>()
                    , null
                    , null
                    , It.IsAny<string>()
               ))
               .Returns(_partyRelationShipSuperUser);

            mockProductRepository
                .Setup(m => m.GetBooksMasterProductDetail(
                    It.Is<int>(l => l == 1)))
                .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.OneSite));

            IManageProductOneSite manageProductOneSite = new ManageProductOneSite(
		        editorRealPageId: _editorRealPageId, 
                userClaim: _editorUserClaim, messageHandler: mockHttpMessageHandler.Object,
                service: mockService.Object,
		        userList: null,
		        roleList: null,
		        rightList: null,
		        propertyList: propertyResultList,
		        samlRepository: mockSamlRepository.Object,
		        managePersona: mockManagePersona.Object,
		        managePerson: null,
		        manageUserLogin: null,
		        userLoginRepository: null,
		        personaRepository: null,
				manageBlueBook: mockManageBlueBook.Object,
		        productRepository: mockProductRepository.Object,
		        productInternalSettingRepository: mockProductInternalSettingRepository.Object,
		        managePartyRelationship: mockManagePartyRelationship.Object,
		        manageElectronicAddress: null,
                userLoginPersonaRepository: null,
                userRepository: null, 
                repository: mockRepository.Object,
                systemIdentifier: _systemIdentifier
            );

			Persona persona = new Persona() { Organization = new Organization() { BooksMasterId = 1234 } };
            resp = manageProductOneSite.GetOneSitePropertyListAll(persona, reqParameter);

            //Assert
            Assert.True(resp.TotalRows == 0);

            //Act
            mockService
                 .Setup(m => m.GetAllProperties(
                        It.IsAny<NameValuePair[]>()
                        , It.IsAny<string>()
                        , It.IsAny<FilterSortParameters>()
                 ))
                 .Returns(propertyResultList);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.IsAny<long>()
                    , It.IsAny<int>()
                 ))
                 .Returns(_editorSamlAttributes);

            mockManagePartyRelationship
                .Setup(m => m.GetPartyRelationship(
                    It.IsAny<Guid>()
                    , It.IsAny<Guid>()
                    , null
                    , null
                    , It.IsAny<string>()
               ))
               .Returns(_partyRelationShipSuperUser);

	        manageProductOneSite = new ManageProductOneSite(
		        editorRealPageId: _editorRealPageId,
                userClaim: _editorUserClaim, messageHandler: mockHttpMessageHandler.Object,
                service: mockService.Object,
		        userList: null,
		        roleList: null,
		        rightList: null,
		        propertyList: propertyResultList,
		        samlRepository: mockSamlRepository.Object,
		        managePersona: mockManagePersona.Object,
		        managePerson: null,
		        manageUserLogin: null,
		        userLoginRepository: null,
		        personaRepository: null,
				manageBlueBook: mockManageBlueBook.Object,
		        productRepository: mockProductRepository.Object,
		        productInternalSettingRepository: mockProductInternalSettingRepository.Object,
		        managePartyRelationship: mockManagePartyRelationship.Object,
		        manageElectronicAddress: null,
                userLoginPersonaRepository: null,
                userRepository: null, 
                repository: mockRepository.Object,
                systemIdentifier: _systemIdentifier
            );

			persona = new Persona() { Organization = new Organization() { BooksMasterId = 1234 } };
            resp = manageProductOneSite.GetOneSitePropertyListAll(persona, reqParameter);

            ProductProperty _responseProperty1 = resp.Records[0] as ProductProperty;
            ProductProperty _responseProperty2 = resp.Records[1] as ProductProperty;

            //Assert
            Assert.True(_propertyType1.PropertyID == _responseProperty1.ID
                && _propertyType1.PropertyName == _responseProperty1.Name
                && _propertyType1.SiteCityName == _responseProperty1.City
                && _propertyType1.SiteState == _responseProperty1.State
                && _propertyType1.SiteAddress == _responseProperty1.Street1
                && _propertyType1.SiteZip == _responseProperty1.Zip
                && _propertyType1.IsAssignedToUser == _responseProperty1.IsAssigned
            );
            Assert.True(_propertyType2.PropertyID == _responseProperty2.ID
                && _propertyType2.PropertyName == _responseProperty2.Name
                && _propertyType2.SiteCityName == _responseProperty2.City
                && _propertyType2.SiteState == _responseProperty2.State
                && _propertyType2.SiteAddress == _responseProperty2.Street1
                && _propertyType2.SiteZip == _responseProperty2.Zip
                && _propertyType2.IsAssignedToUser == _responseProperty2.IsAssigned
            );
        }

        [Fact]
        public void Put_PropertyList_AddSitesToUser()
        {
            //Arrange
            RequestParameter reqParameter = new RequestParameter();
            reqParameter.Pages.ResultsPerPage = 1000;
            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("SortBy", "ASC");

            var mockService = new Mock<IOneSiteProductService>();
            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            var mockManagePartyRelationship = new Mock<IManagePartyRelationship>();

            PropertyList propertyResultList = new PropertyList();

            List<string> _propertiesToAdd = new List<string>();
            _propertiesToAdd.Add("1234567");
            _propertiesToAdd.Add("7654321");

            List<PropertyType> _propList = new List<PropertyType>();

            _propList.Add(_propertyType1); // already assigned
            _propList.Add(_propertyType2); // not assigned
            _propList.Add(_propertyType3); // already assigned
            propertyResultList.Property = _propList.ToArray();
            propertyResultList.TotalProperties = _propList.Count;

            AssignStatus removeStatus = new AssignStatus();
            AssignStatus addStatus = new AssignStatus();

            IC.PartyRelationship _partyRelationShipSuperUser = new IC.PartyRelationship();
            _partyRelationShipSuperUser.RoleTypeFrom = new IC.RoleType() { Name = _ROLETYPE_NAME_USER };

            //Act
            mockService
                 .Setup(m => m.GetAllProperties(
                    It.IsAny<NameValuePair[]>()
                    , It.IsAny<string>()
                    , It.IsAny<FilterSortParameters>()
                 ))
                 .Returns(propertyResultList);

            mockService
                 .Setup(m => m.RemovePropertiesFromUser(
                    It.IsAny<string>()
                    , It.IsAny<string>()
                 ))
                 .Returns(removeStatus);

            mockService
                .Setup(m => m.AssignPropertiesToUser(
                    It.IsAny<string>()
                    , It.IsAny<string>()
                ))
                .Returns(removeStatus);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.IsAny<long>()
                    , It.IsAny<int>()
                 ))
                 .Returns(_editorSamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.IsAny<long>()
                ))
                .Returns(_editorPersona);

            mockProductRepository
                .Setup(m => m.GetProductSettingsByPersona(
                    It.IsAny<long>()
                ))
                .Returns(_userProductSettings);

            mockProductRepository
                .Setup(m => m.CreateProductSetting(
                    It.IsAny<long>()
                    , It.IsAny<int>()
                    , It.IsAny<int>()
                    , It.IsAny<string>()
                ))
                .Returns(_repositoryResponseProductStatus);

            mockProductRepository
                .Setup(m => m.ListProductSettingType(
                ))
                .Returns(_productSettingType);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.IsAny<int>()
                ))
                .Returns(_productInternalSettingsOneSite);

            mockManagePartyRelationship
                .Setup(m => m.GetPartyRelationship(
                    It.IsAny<Guid>()
                    , It.IsAny<Guid>()
                    , null
                    , null
                    , It.IsAny<string>()
               ))
               .Returns(_partyRelationShipSuperUser);

            mockProductRepository
              .Setup(m => m.GetBooksMasterProductDetail(
                  It.Is<int>(l => l == 1)))
              .Returns(_gbProductMap.Find(p => p.ProductId == (int)ProductEnum.OneSite));

            IManageProductOneSite manageProductOneSite = new ManageProductOneSite(
				editorRealPageId: _editorRealPageId,
                userClaim: _editorUserClaim, messageHandler: mockHttpMessageHandler.Object,
                service: mockService.Object,
				userList: null,
				roleList: null,
				rightList: null,
				propertyList: propertyResultList,
				samlRepository: mockSamlRepository.Object,
				managePersona: mockManagePersona.Object,
				managePerson: null,
				manageUserLogin: null,
				userLoginRepository: null,
				personaRepository: null,
				manageBlueBook: mockManageBlueBook.Object,
				productRepository: mockProductRepository.Object,
				productInternalSettingRepository: mockProductInternalSettingRepository.Object,
				managePartyRelationship: mockManagePartyRelationship.Object,
				manageElectronicAddress: null,
                userLoginPersonaRepository: null,
                userRepository: null, 
                repository: mockRepository.Object,
                systemIdentifier: _systemIdentifier
            );

			string resp = manageProductOneSite.UpdatePropertiesForUser(_editorPersonaId, _userPersonaId, _propertiesToAdd, out List<AdditionalParameters> additionalParameters);
            Assert.True(resp == "2");
        }

        [Fact]
        public void Put_PropertyList_AddAllSitesToUser()
        {
            //Arrange
            RequestParameter reqParameter = new RequestParameter();
            reqParameter.Pages.ResultsPerPage = 1000;
            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("SortBy", "ASC");

            var mockService = new Mock<IOneSiteProductService>();
            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            var mockManagePartyRelationship = new Mock<IManagePartyRelationship>();

            PropertyList propertyResultList = new PropertyList();

            List<string> _propertiesToAdd = new List<string>();
            _propertiesToAdd.Add("All");

            List<PropertyType> _propList = new List<PropertyType>();

            _propList.Add(_propertyType1); // already assigned
            _propList.Add(_propertyType2); // not assigned
            _propList.Add(_propertyType3); // already assigned
            propertyResultList.Property = _propList.ToArray();
            propertyResultList.TotalProperties = _propList.Count;

            AssignStatus removeStatus = new AssignStatus();
            AssignStatus addStatus = new AssignStatus();

            IC.PartyRelationship _partyRelationShip = new IC.PartyRelationship();
            _partyRelationShip.RoleTypeFrom = new IC.RoleType() { Name = _ROLETYPE_NAME_USER };

            //Act
            mockService
                 .Setup(m => m.GetAllProperties(
                    It.IsAny<NameValuePair[]>()
                    , It.IsAny<string>()
                    , It.IsAny<FilterSortParameters>()
                 ))
                 .Returns(propertyResultList);

            mockService
                 .Setup(m => m.RemovePropertiesFromUser(
                    It.IsAny<string>()
                    , It.IsAny<string>()
                 ))
                 .Returns(removeStatus);

            mockService
                .Setup(m => m.AssignPropertiesToUser(
                    It.IsAny<string>()
                    , It.IsAny<string>()
                ))
                .Returns(removeStatus);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.IsAny<long>()
                    , It.IsAny<int>()
                 ))
                 .Returns(_editorSamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.IsAny<long>()
                ))
                .Returns(_editorPersona);

            mockProductRepository
                .Setup(m => m.GetProductSettingsByPersona(
                    It.IsAny<long>()
                ))
                .Returns(_userProductSettings);

            mockProductRepository
                .Setup(m => m.ListProductSettingType(
                ))
                .Returns(_productSettingType);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.IsAny<int>()
                ))
                .Returns(_productInternalSettingsOneSite);

            mockManagePartyRelationship
                .Setup(m => m.GetPartyRelationship(
                    It.IsAny<Guid>()
                    , It.IsAny<Guid>()
                    , null
                    , null
                    , It.IsAny<string>()
               ))
               .Returns(_partyRelationShip);

            mockProductRepository
                .Setup(m => m.GetBooksMasterProductDetail(
                    It.Is<int>(l => l == 1)))
                .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.OneSite));

            IManageProductOneSite manageProductOneSite = new ManageProductOneSite(
				editorRealPageId: _editorRealPageId, 
                userClaim: _editorUserClaim, messageHandler: mockHttpMessageHandler.Object,
                service: mockService.Object,
				userList: null,
				roleList: null,
				rightList: null,
				propertyList: propertyResultList,
				samlRepository: mockSamlRepository.Object,
				managePersona: mockManagePersona.Object,
				managePerson: null,
				manageUserLogin: null,
				userLoginRepository: null,
				personaRepository: null,
				manageBlueBook: mockManageBlueBook.Object,
				productRepository: mockProductRepository.Object,
				productInternalSettingRepository: mockProductInternalSettingRepository.Object,
				managePartyRelationship: mockManagePartyRelationship.Object,
				manageElectronicAddress: null,
                userLoginPersonaRepository: null,
                userRepository: null, 
                repository: mockRepository.Object,
                systemIdentifier: _systemIdentifier
            );
			string resp = manageProductOneSite.UpdatePropertiesForUser(_editorPersonaId, _userPersonaId, _propertiesToAdd, out List<AdditionalParameters> additionalParameters);
            Assert.True(resp == "All");
        }

        [Fact]
        public void Get_GetUsersForProperty_Valid()
        {
            //Arrange
            var mockService = new Mock<IOneSiteProductService>();
            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockProductRepository = new Mock<IProductRepository>();
	        var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();

			RequestParameter reqParameter = new RequestParameter();
            reqParameter.Pages.ResultsPerPage = 1000;
            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("SortBy", "ASC");

            UserList resultList = new UserList();
            UserList emptyResultList = new UserList();
            List<UserType> _userList = new List<UserType>();

            _userList.Add(_userType1);
            _userList.Add(_userType2);
            _userList.Add(_userType3);

            resultList.User = _userList.ToArray();
            resultList.TotalUsers = _userList.Count;

            int propertyId = 2222222;
            bool assignedOnly = false;
            ListResponse resp;

            //Act
            // test exception
            mockService
                .Setup(m => m.GetUsersForProperty(
                    It.IsAny<string>()
                    , It.IsAny<int>()
                    , It.IsAny<bool>()
                    , It.IsAny<FilterSortParameters>()
                ))
                .Throws(new SystemException());

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.IsAny<long>()
                    , It.IsAny<int>()
                 ))
                 .Returns(_editorSamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.IsAny<long>()
                ))
                .Returns(_editorPersona);

            mockProductRepository
                .Setup(m => m.GetProductSettingsByPersona(
                    It.IsAny<long>()
                ))
                .Returns(_userProductSettings);

	        mockProductInternalSettingRepository
		        .Setup(m => m.GetProductInternalSettings(
			        It.IsAny<int>()
		        ))
		        .Returns(_productInternalSettingsOneSite);

            mockProductRepository
                .Setup(m => m.GetBooksMasterProductDetail(
                    It.Is<int>(l => l == 1)))
                .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.OneSite));

            //Assert
            IManageProductOneSite manageProductOneSite = new ManageProductOneSite(
		        editorRealPageId: _editorRealPageId,
                userClaim: _editorUserClaim, messageHandler: mockHttpMessageHandler.Object,
                service: mockService.Object,
		        userList: emptyResultList,
		        roleList: null,
		        rightList: null,
		        propertyList: null,
				samlRepository: mockSamlRepository.Object,
		        managePersona: mockManagePersona.Object,
		        managePerson: null,
		        manageUserLogin: null,
		        userLoginRepository: null,
		        personaRepository: null,
				manageBlueBook: mockManageBlueBook.Object,
		        productRepository: mockProductRepository.Object,
		        productInternalSettingRepository: null,
		        managePartyRelationship: null,
		        manageElectronicAddress: null,
                userLoginPersonaRepository: null,
                userRepository: null, 
                repository: mockRepository.Object,
                systemIdentifier: _systemIdentifier
            );

            resp = manageProductOneSite.GetUsersForProperty(_editorPersonaId, propertyId, assignedOnly, reqParameter);
            Assert.True(resp.TotalRows == 0);

            //Act
            mockService
                .Setup(m => m.GetUsersForProperty(
                    It.IsAny<string>()
                    , It.IsAny<int>()
                    , It.IsAny<bool>()
                    , It.IsAny<FilterSortParameters>()
                ))
                .Returns(resultList);

			//Act
	        manageProductOneSite = new ManageProductOneSite(
		        editorRealPageId: _editorRealPageId,
                userClaim: _editorUserClaim, messageHandler: mockHttpMessageHandler.Object,
                service: mockService.Object,
		        userList: emptyResultList,
		        roleList: null,
		        rightList: null,
		        propertyList: null,
				samlRepository: mockSamlRepository.Object,
		        managePersona: mockManagePersona.Object,
		        managePerson: null,
		        manageUserLogin: null,
		        userLoginRepository: null,
		        personaRepository: null,
				manageBlueBook: mockManageBlueBook.Object,
		        productRepository: mockProductRepository.Object,
		        productInternalSettingRepository: null,
		        managePartyRelationship: null,
		        manageElectronicAddress: null,
                userLoginPersonaRepository: null,
                userRepository: null, 
                repository: mockRepository.Object,
                systemIdentifier: _systemIdentifier
            );

			resp = manageProductOneSite.GetUsersForProperty(_editorPersonaId, propertyId, assignedOnly, reqParameter);
            Assert.True(resp.TotalRows == 3);
            Assert.True(
                (resp.Records[0] as Component.SharedObjects.Product.ProductUser).UserId == _userType1.UserId
                && (resp.Records[0] as Component.SharedObjects.Product.ProductUser).UserLogin == _userType1.UserLogin
                && (resp.Records[0] as Component.SharedObjects.Product.ProductUser).UserName == _userType1.UserName
                && (resp.Records[0] as Component.SharedObjects.Product.ProductUser).IsAssigned == _userType1.Assigned
                );

            Assert.True(
                (resp.Records[1] as Component.SharedObjects.Product.ProductUser).UserId == _userType2.UserId
                && (resp.Records[1] as Component.SharedObjects.Product.ProductUser).UserLogin == _userType2.UserLogin
                && (resp.Records[1] as Component.SharedObjects.Product.ProductUser).UserName == _userType2.UserName
                && (resp.Records[1] as Component.SharedObjects.Product.ProductUser).IsAssigned == _userType2.Assigned
                );

            Assert.True(
                (resp.Records[2] as Component.SharedObjects.Product.ProductUser).UserId == _userType3.UserId
                && (resp.Records[2] as Component.SharedObjects.Product.ProductUser).UserLogin == _userType3.UserLogin
                && (resp.Records[2] as Component.SharedObjects.Product.ProductUser).UserName == _userType3.UserName
                && (resp.Records[2] as Component.SharedObjects.Product.ProductUser).IsAssigned == _userType3.Assigned
                );
        }

        #endregion

        #region Roles
        [Fact]
        public void Post_Role()
        {
            //Arrange
            RequestParameter reqParameter = new RequestParameter();
            reqParameter.Pages.ResultsPerPage = 1000;

            var mockService = new Mock<IOneSiteProductService>();
            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            var mockManagePartyRelationship = new Mock<IManagePartyRelationship>();

            RoleList roleResultList = new RoleList();

            List<RoleType> _roleList = new List<RoleType>() { _roleType1 };

            roleResultList.Role = _roleList.ToArray();
            roleResultList.TotalRoles = _roleList.Count;

            IList<SamlAttributes> samlAttributes = new List<SamlAttributes>();
            samlAttributes.Add(new SamlAttributes() { Name = "UserId", Value = "1234567|username" });

            IC.PartyRelationship _partyRelationShip = new IC.PartyRelationship();
            _partyRelationShip.RoleTypeFrom = new IC.RoleType() { Name = _ROLETYPE_NAME_USER };

            //Act
            mockService
                 .Setup(m => m.AddUpdateRole(
                        It.IsAny<string>()
                        , It.IsAny<string>()
                        , It.IsAny<string>()
                        , It.IsAny<string>()
                 ))
                 .Returns(roleResultList);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.IsAny<long>()
                    , It.IsAny<int>()
                 ))
                 .Returns(samlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.IsAny<long>()
                ))
                .Returns(_editorPersona);

            mockManagePartyRelationship
                .Setup(m => m.GetPartyRelationship(
                    It.IsAny<Guid>()
                    , It.IsAny<Guid>()
                    , null
                    , null
                    , It.IsAny<string>()
               ))
               .Returns(_partyRelationShip);

            mockProductRepository
                .Setup(m => m.GetBooksMasterProductDetail(
                    It.Is<int>(l => l == 1)))
                .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.OneSite));

            IManageProductOneSite manageProductOneSite = new ManageProductOneSite(
		        editorRealPageId: _editorRealPageId,
                userClaim: _editorUserClaim, messageHandler: mockHttpMessageHandler.Object,
                service: mockService.Object,
		        userList: null,
		        roleList: roleResultList,
		        rightList: null,
		        propertyList: null,
				samlRepository: mockSamlRepository.Object,
		        managePersona: mockManagePersona.Object,
		        managePerson: null,
		        manageUserLogin: null,
		        userLoginRepository: null,
		        personaRepository: null,
				manageBlueBook: mockManageBlueBook.Object,
		        productRepository: mockProductRepository.Object,
		        productInternalSettingRepository: mockProductInternalSettingRepository.Object,
		        managePartyRelationship: mockManagePartyRelationship.Object,
		        manageElectronicAddress: null,
                userLoginPersonaRepository: null,
                userRepository: null, 
                repository: mockRepository.Object,
                systemIdentifier: _systemIdentifier
            );
			Persona persona = new Persona();
            int roleId = 0; // new role
            string roleName = "New Role";
            string inheritRoleId = "1";

            ListResponse resp = manageProductOneSite.AddUpdateRole(_editorPersonaId, roleId, roleName, inheritRoleId);

            ProductRole _responseRole1 = resp.Records[0] as ProductRole;

            //Assert
            Assert.True(_roleType1.RoleID == _responseRole1.ID
                && _roleType1.RoleName == _responseRole1.Name
                && _roleType1.Roletype == _responseRole1.Roletype
                && _roleType1.IsAssigned == _responseRole1.IsAssigned
            );

            roleId = 10; // existing role
            roleName = "Existing Role";
            inheritRoleId = null;

            resp = manageProductOneSite.AddUpdateRole(_editorPersonaId, roleId, roleName, inheritRoleId);

            _responseRole1 = resp.Records[0] as ProductRole;

            //Assert
            Assert.True(_roleType1.RoleID == _responseRole1.ID
                && _roleType1.RoleName == _responseRole1.Name
                && _roleType1.Roletype == _responseRole1.Roletype
                && _roleType1.IsAssigned == _responseRole1.IsAssigned
            );
        }

        [Fact]
        public void Delete_Role()
        {
            //Arrange
            RequestParameter reqParameter = new RequestParameter();
            reqParameter.Pages.ResultsPerPage = 1000;

            var mockService = new Mock<IOneSiteProductService>();
            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePerson = new Mock<IManagePerson>();
            var mockManageUserLogin = new Mock<IManageUserLogin>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            var mockManagePartyRelationship = new Mock<IManagePartyRelationship>();
            var mockManageProductOneSite = new Mock<IManageProductOneSite>();
            var mockManageUnifiedLogin = new Mock<IManageUnifiedLogin>();

            RoleList roleResultList = new RoleList();

            List<RoleType> _roleList = new List<RoleType>();

            _roleList.Add(_roleType1);
            roleResultList.Role = _roleList.ToArray();
            roleResultList.TotalRoles = _roleList.Count;

            IC.PartyRelationship _partyRelationShip = new IC.PartyRelationship();
            _partyRelationShip.RoleTypeFrom = new IC.RoleType() { Name = _ROLETYPE_NAME_USER };

            //Act
            mockService
                 .Setup(m => m.DeleteRole(
                        It.IsAny<string>()
                        , It.IsAny<int>()
                 ));

            mockManageUnifiedLogin
                 .Setup(m => m.DeleteRoleLogMessage(
                           It.IsAny<long>()
                           , It.IsAny<long>()
                           , It.IsAny<string>()
                           , It.IsAny<string>()
                           , It.IsAny<int>()
                 )) .Verifiable();

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.IsAny<long>()
                    , It.IsAny<int>()
                 ))
                 .Returns(_editorSamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.IsAny<long>()
                ))
                .Returns(_editorPersona);

            mockProductRepository
                .Setup(m => m.GetProductSettingsByPersona(
                    It.IsAny<long>()
                ))
                .Returns(_userProductSettings);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.IsAny<int>()
                ))
                .Returns(_productInternalSettingsOneSite);

            mockManagePartyRelationship
                .Setup(m => m.GetPartyRelationship(
                    It.IsAny<Guid>()
                    , It.IsAny<Guid>()
                    , null
                    , null
                    , It.IsAny<string>()
               ))
               .Returns(_partyRelationShip);

            mockProductRepository
                .Setup(m => m.GetBooksMasterProductDetail(
                    It.Is<int>(l => l == 1)))
                .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.OneSite));

            IManageProductOneSite manageProductOneSite = new ManageProductOneSite(
		        editorRealPageId: _editorRealPageId,
                userClaim: _editorUserClaim, messageHandler: mockHttpMessageHandler.Object,
                service: mockService.Object,
		        userList: null,
		        roleList: roleResultList,
		        rightList: null,
		        propertyList: null,
				samlRepository: mockSamlRepository.Object,
		        managePersona: mockManagePersona.Object,
		        managePerson: mockManagePerson.Object,
		        manageUserLogin: mockManageUserLogin.Object,
		        userLoginRepository: null,
		        personaRepository: null,
				manageBlueBook: mockManageBlueBook.Object,
		        productRepository: mockProductRepository.Object,
		        productInternalSettingRepository: mockProductInternalSettingRepository.Object,
		        managePartyRelationship: mockManagePartyRelationship.Object,
		        manageElectronicAddress: null,
                userLoginPersonaRepository: null,
                userRepository: null, 
                systemIdentifier: _systemIdentifier,
                repository: mockRepository.Object,
                UnifiedLogin: mockManageUnifiedLogin.Object
            );
			Persona persona = new Persona();
            int roleId = 10; // new role

            string resp = manageProductOneSite.DeleteRole(_editorPersonaId, roleId);

            //Assert
            Assert.True(resp == ""); // no message is a success
        }

        [Fact]
        public void Get_UserRoles_UserExists()
        {
            //Arrange
            RequestParameter reqParameter = new RequestParameter();
            reqParameter.Pages.ResultsPerPage = 1000;

            var mockService = new Mock<IOneSiteProductService>();
            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            var mockManagePartyRelationship = new Mock<IManagePartyRelationship>();

            RoleList roleResultList = new RoleList();

            List<RoleType> _roleList = new List<RoleType>();

            _roleList.Add(_roleType1);
            _roleList.Add(_roleType2);
            roleResultList.Role = _roleList.ToArray();
            roleResultList.TotalRoles = _roleList.Count;

            Persona editorPersona = new Persona() { PersonaId = _editorPersonaId, RealPageId = _editorRealPageId };
            editorPersona.Organization = new Organization() { PartyId = _editorOrganizationPartyId };

            IC.PartyRelationship _partyRelationShip = new IC.PartyRelationship();
            _partyRelationShip.RoleTypeFrom = new IC.RoleType() { Name = _ROLETYPE_NAME_USER };

            //Act
            mockService
                 .Setup(m => m.GetUserRoles(
                        It.IsAny<string>()
                        , It.IsAny<bool>()
                        , It.IsAny<FilterSortParameters>()
                 ))
                 .Returns(roleResultList);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.IsAny<long>()
                    , It.IsAny<int>()
                 ))
                 .Returns(_editorSamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.IsAny<long>()
                ))
                .Returns(_editorPersona);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.IsAny<int>()
                ))
                .Returns(_productInternalSettingsOneSite);

            mockManagePartyRelationship
                .Setup(m => m.GetPartyRelationship(
                    It.IsAny<Guid>()
                    , It.IsAny<Guid>()
                    , null
                    , null
                    , It.IsAny<string>()
               ))
               .Returns(_partyRelationShip);

			mockService
				.Setup(m => m.GetUser(
						It.IsAny<NameValuePair[]>()
				))
				.Returns(_userInfo1.ToArray());

            mockProductRepository
                .Setup(m => m.GetBooksMasterProductDetail(
                    It.Is<int>(l => l == 1)))
                .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.OneSite));

            IManageProductOneSite manageProductOneSite = new ManageProductOneSite(
		        editorRealPageId: _editorRealPageId,
                userClaim: _editorUserClaim, messageHandler: mockHttpMessageHandler.Object,
                service: mockService.Object,
		        userList: null,
		        roleList: roleResultList,
		        rightList: null,
		        propertyList: null,
				samlRepository: mockSamlRepository.Object,
		        managePersona: mockManagePersona.Object,
		        managePerson: null,
		        manageUserLogin: null,
		        userLoginRepository: null,
		        personaRepository: null,
				manageBlueBook: mockManageBlueBook.Object,
		        productRepository: mockProductRepository.Object,
		        productInternalSettingRepository: mockProductInternalSettingRepository.Object,
		        managePartyRelationship: mockManagePartyRelationship.Object,
		        manageElectronicAddress: null,
                userLoginPersonaRepository: null,
                userRepository: null, 
                repository: mockRepository.Object,
                systemIdentifier: _systemIdentifier
            );
			ListResponse resp = manageProductOneSite.GetOneSiteRoleList(_editorPersonaId, _userPersonaId, false, reqParameter);

            ProductRole _responseRole1 = resp.Records[0] as ProductRole;
            ProductRole _responseRole2 = resp.Records[1] as ProductRole;

            //Assert
            Assert.True(_roleType1.RoleID == _responseRole1.ID
                && _roleType1.RoleName == _responseRole1.Name
                && _roleType1.Roletype == _responseRole1.Roletype
                && _roleType1.IsAssigned == _responseRole1.IsAssigned
            );
            Assert.True(_roleType2.RoleID == _responseRole2.ID
                && _roleType2.RoleName == _responseRole2.Name
                && _roleType2.Roletype == _responseRole2.Roletype
                && _roleType2.IsAssigned == _responseRole2.IsAssigned
            );
        }

        [Fact]
        public void Get_RoleList_All()
        {
            //Arrange
            RequestParameter reqParameter = new RequestParameter();
            reqParameter.Pages.ResultsPerPage = 1000;
            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("SortBy", "ASC");

            var mockService = new Mock<IOneSiteProductService>();
            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            var mockManagePartyRelationship = new Mock<IManagePartyRelationship>();

            RoleList roleResultList = new RoleList();
            RoleList emptyResultList = new RoleList();

            List<RoleType> _roleList = new List<RoleType>();

            _roleList.Add(_roleType1);
            _roleList.Add(_roleType2);
            roleResultList.Role = _roleList.ToArray();
            roleResultList.TotalRoles = _roleList.Count;

            IC.PartyRelationship _partyRelationShip = new IC.PartyRelationship();
            _partyRelationShip.RoleTypeFrom = new IC.RoleType() { Name = _ROLETYPE_NAME_USER };

            //Act
            mockService
                 .Setup(m => m.GetAllRoles(
                        It.IsAny<NameValuePair[]>()
                        , It.IsAny<string>()
                        , It.IsAny<FilterSortParameters>()
                 ))
                 .Throws(new Exception());

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.IsAny<long>()
                    , It.IsAny<int>()
                 ))
                 .Returns(_editorSamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.IsAny<long>()
                ))
                .Returns(_editorPersona);

            mockManagePartyRelationship
                .Setup(m => m.GetPartyRelationship(
                    It.IsAny<Guid>()
                    , It.IsAny<Guid>()
                    , null
                    , null
                    , It.IsAny<string>()
               ))
               .Returns(_partyRelationShip);

            mockProductRepository
                .Setup(m => m.GetBooksMasterProductDetail(
                    It.Is<int>(l => l == 1)))
                .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.OneSite));

            IManageProductOneSite manageProductOneSite = new ManageProductOneSite(
		        editorRealPageId: _editorRealPageId, 
                userClaim: _editorUserClaim, messageHandler: mockHttpMessageHandler.Object,
                service: mockService.Object,
		        userList: null,
		        roleList: emptyResultList,
		        rightList: null,
		        propertyList: null,
				samlRepository: mockSamlRepository.Object,
		        managePersona: mockManagePersona.Object,
		        managePerson: null,
		        manageUserLogin: null,
		        userLoginRepository: null,
		        personaRepository: null,
				manageBlueBook: mockManageBlueBook.Object,
		        productRepository: mockProductRepository.Object,
		        productInternalSettingRepository: mockProductInternalSettingRepository.Object,
		        managePartyRelationship: mockManagePartyRelationship.Object,
		        manageElectronicAddress: null,
                userLoginPersonaRepository: null,
                userRepository: null, 
                repository: mockRepository.Object,
                systemIdentifier: _systemIdentifier
            );
			ListResponse resp = manageProductOneSite.GetOneSiteRoleListAll(_editorPersonaId, reqParameter);

            //Assert
            Assert.True(resp.TotalRows == 0);

            //Act
            mockService
                 .Setup(m => m.GetAllRoles(
                        It.IsAny<NameValuePair[]>()
                        , It.IsAny<string>()
                        , It.IsAny<FilterSortParameters>()
                 ))
                 .Returns(roleResultList);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.IsAny<long>()
                    , It.IsAny<int>()
                 ))
                 .Returns(_editorSamlAttributes);

	        manageProductOneSite = new ManageProductOneSite(
		        editorRealPageId: _editorRealPageId, 
                userClaim: _editorUserClaim, messageHandler: mockHttpMessageHandler.Object,
                service: mockService.Object,
		        userList: null,
		        roleList: roleResultList,
		        rightList: null,
		        propertyList: null,
				samlRepository: mockSamlRepository.Object,
		        managePersona: mockManagePersona.Object,
		        managePerson: null,
		        manageUserLogin: null,
		        userLoginRepository: null,
		        personaRepository: null,
				manageBlueBook: mockManageBlueBook.Object,
		        productRepository: mockProductRepository.Object,
		        productInternalSettingRepository: mockProductInternalSettingRepository.Object,
		        managePartyRelationship: mockManagePartyRelationship.Object,
		        manageElectronicAddress: null,
                userLoginPersonaRepository: null,
                userRepository: null, 
                repository: mockRepository.Object,
                systemIdentifier: _systemIdentifier
            );
			resp = manageProductOneSite.GetOneSiteRoleListAll(_editorPersonaId, reqParameter);

            ProductRole _responseRole1 = resp.Records[0] as ProductRole;
            ProductRole _responseRole2 = resp.Records[1] as ProductRole;

            //Assert
            Assert.True(_roleType1.RoleID == _responseRole1.ID
                && _roleType1.RoleName == _responseRole1.Name
                && _roleType1.Roletype == _responseRole1.Roletype
                && false == _responseRole1.IsAssigned
            );
            Assert.True(_roleType2.RoleID == _responseRole2.ID
                && _roleType2.RoleName == _responseRole2.Name
                && _roleType2.Roletype == _responseRole2.Roletype
                && false == _responseRole2.IsAssigned
            );
        }

        [Fact]
        public void Get_RoleList_AllExcludingSpecialRoles()
        {
            //Arrange
            RequestParameter reqParameter = new RequestParameter();
            reqParameter.Pages.ResultsPerPage = 1000;
            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("SortBy", "ASC");

            var mockService = new Mock<IOneSiteProductService>();
            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            var mockManagePartyRelationship = new Mock<IManagePartyRelationship>();

            RoleList roleResultList = new RoleList();

            List<RoleType> _roleList = new List<RoleType>();

            _roleList.Add(_roleType1);
            _roleList.Add(_roleType2);
            _roleList.Add(_roleTypeDeveloper);
            _roleList.Add(_roleTypeInternalAdmin);
            _roleList.Add(_roleTypeInternalUser);
            _roleList.Add(_roleTypeCloseCriminalDispute);

            roleResultList.Role = _roleList.ToArray();
            roleResultList.TotalRoles = _roleList.Count;

            IList<SamlAttributes> samlAttributes = new List<SamlAttributes>();
            samlAttributes.Add(new SamlAttributes() { Name = "UserId", Value = "1234567|username" });

            IC.PartyRelationship _partyRelationShip = new IC.PartyRelationship();
            _partyRelationShip.RoleTypeFrom = new IC.RoleType() { Name = _ROLETYPE_NAME_USER };

            //Act
            mockService
                 .Setup(m => m.GetAllRoles(
                        It.IsAny<NameValuePair[]>()
                        , It.IsAny<string>()
                        , It.IsAny<FilterSortParameters>()
                 ))
                 .Returns(roleResultList);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.IsAny<long>()
                    , It.IsAny<int>()
                 ))
                 .Returns(samlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.IsAny<long>()
                ))
                .Returns(_editorPersona);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.IsAny<int>()
                ))
                .Returns(_productInternalSettingsOneSite);

            mockManagePartyRelationship
                .Setup(m => m.GetPartyRelationship(
                    It.IsAny<Guid>()
                    , It.IsAny<Guid>()
                    , null
                    , null
                    , It.IsAny<string>()
               ))
               .Returns(_partyRelationShip);

            mockProductRepository
                .Setup(m => m.GetBooksMasterProductDetail(
                    It.Is<int>(l => l == 1)))
                .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.OneSite));

            IManageProductOneSite manageProductOneSite = new ManageProductOneSite(
		        editorRealPageId: _editorRealPageId, 
                userClaim: _editorUserClaim, messageHandler: mockHttpMessageHandler.Object,
                service: mockService.Object,
		        userList: null,
		        roleList: roleResultList,
		        rightList: null,
		        propertyList: null,
				samlRepository: mockSamlRepository.Object,
		        managePersona: mockManagePersona.Object,
		        managePerson: null,
		        manageUserLogin: null,
		        userLoginRepository: null,
		        personaRepository: null,
				manageBlueBook: mockManageBlueBook.Object,
		        productRepository: mockProductRepository.Object,
		        productInternalSettingRepository: mockProductInternalSettingRepository.Object,
		        managePartyRelationship: mockManagePartyRelationship.Object,
		        manageElectronicAddress: null,
                userLoginPersonaRepository: null,
                userRepository: null, 
                repository: mockRepository.Object,
                systemIdentifier: _systemIdentifier
            );
			ListResponse resp = manageProductOneSite.GetOneSiteRoleListAll(_editorPersonaId, reqParameter);

            ProductRole _responseRole1 = resp.Records[0] as ProductRole;
            ProductRole _responseRole2 = resp.Records[1] as ProductRole;

            //Assert
            Assert.True(resp.TotalRows == 2);

            Assert.True(_roleType1.RoleID == _responseRole1.ID
                && _roleType1.RoleName == _responseRole1.Name
                && _roleType1.Roletype == _responseRole1.Roletype
                && false == _responseRole1.IsAssigned
            );
            Assert.True(_roleType2.RoleID == _responseRole2.ID
                && _roleType2.RoleName == _responseRole2.Name
                && _roleType2.Roletype == _responseRole2.Roletype
                && false == _responseRole2.IsAssigned
            );
        }

        [Fact]
        public void Put_RoleList_AddRolesToUser()
        {
            //Arrange
            RequestParameter reqParameter = new RequestParameter();
            reqParameter.Pages.ResultsPerPage = 1000;
            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("SortBy", "ASC");

            var mockService = new Mock<IOneSiteProductService>();
            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            var mockManagePartyRelationship = new Mock<IManagePartyRelationship>();

            RoleList roleResultList = new RoleList();

            List<string> _rolesToAdd = new List<string>();
            _rolesToAdd.Add("1");
            _rolesToAdd.Add("2");
            _rolesToAdd.Add("4");

            List<RoleType> _roleList = new List<RoleType>();

            _roleList.Add(_roleType1); // already assigned
            _roleList.Add(_roleType2); // not assigned
            _roleList.Add(_roleType3); // assigned and keep
            _roleList.Add(_roleTypeInternalAdmin); // assign and keep
            roleResultList.Role = _roleList.ToArray();
            roleResultList.TotalRoles = _roleList.Count;

            AssignStatus removeStatus = new AssignStatus();
            AssignStatus addStatus = new AssignStatus();

            IC.PartyRelationship _partyRelationShip = new IC.PartyRelationship();
            _partyRelationShip.RoleTypeFrom = new IC.RoleType() { Name = _ROLETYPE_NAME_USER };

            //Act
            mockService
                 .Setup(m => m.GetAllRoles(
                    It.IsAny<NameValuePair[]>()
                    , It.IsAny<string>()
                    , It.IsAny<FilterSortParameters>()
                 ))
                 .Returns(roleResultList);

            mockService
                 .Setup(m => m.RemoveRolesFromUser(
                    It.IsAny<string>()
                    , It.IsAny<string>()
                 ))
                 .Returns(removeStatus);

            mockService
                .Setup(m => m.AssignRolesToUser(
                    It.IsAny<string>()
                    , It.IsAny<string>()
                ))
                .Returns(removeStatus);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.IsAny<long>()
                    , It.IsAny<int>()
                 ))
                 .Returns(_editorSamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.IsAny<long>()
                ))
                .Returns(_editorPersona);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.IsAny<int>()
                ))
                .Returns(_productInternalSettingsOneSite);

            mockManagePartyRelationship
                .Setup(m => m.GetPartyRelationship(
                    It.IsAny<Guid>()
                    , It.IsAny<Guid>()
                    , null
                    , null
                    , It.IsAny<string>()
               ))
               .Returns(_partyRelationShip);

            mockProductRepository
                .Setup(m => m.GetBooksMasterProductDetail(
                    It.Is<int>(l => l == 1)))
                .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.OneSite));

            IManageProductOneSite manageProductOneSite = new ManageProductOneSite(
		        editorRealPageId: _editorRealPageId, 
                userClaim: _editorUserClaim, messageHandler: mockHttpMessageHandler.Object,
                service: mockService.Object,
		        userList: null,
		        roleList: roleResultList,
		        rightList: null,
		        propertyList: null,
				samlRepository: mockSamlRepository.Object,
		        managePersona: mockManagePersona.Object,
		        managePerson: null,
		        manageUserLogin: null,
		        userLoginRepository: null,
		        personaRepository: null,
				manageBlueBook: mockManageBlueBook.Object,
		        productRepository: mockProductRepository.Object,
		        productInternalSettingRepository: mockProductInternalSettingRepository.Object,
		        managePartyRelationship: mockManagePartyRelationship.Object,
		        manageElectronicAddress: null,
                userLoginPersonaRepository: null,
                userRepository: null, 
                repository: mockRepository.Object,
                systemIdentifier: _systemIdentifier
            );
			string resp = manageProductOneSite.UpdateRolesForUser(_editorPersonaId, _userPersonaId, _rolesToAdd, out List<AdditionalParameters> additionalParameters);
            // Role 1 should be kept, Role 2 added and Role 3 should be removed, causing an update of 2 records changed
            Assert.True(resp == "2");
        }

        [Fact]
        public void Put_UpdateRoleToRights_Valid()
        {
            //Arrange
            RequestParameter reqParameter = new RequestParameter();
            reqParameter.Pages.ResultsPerPage = 1000;
            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("SortBy", "ASC");

            var mockService = new Mock<IOneSiteProductService>();
            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            var mockManagePartyRelationship = new Mock<IManagePartyRelationship>();
            var mockManageProductOneSite = new Mock<IManageProductOneSite>();

            RoleList roleResultList = new RoleList();

            List<string> _rolesToAdd = new List<string>();
            _rolesToAdd.Add("1");
            _rolesToAdd.Add("2");
            _rolesToAdd.Add("4");

            List<RoleType> _roleList = new List<RoleType>();

            _roleList.Add(_roleType1); // already assigned
            _roleList.Add(_roleType2); // not assigned
            _roleList.Add(_roleType3); // assigned and keep
            _roleList.Add(_roleTypeInternalAdmin); // assign and keep
            roleResultList.Role = _roleList.ToArray();
            roleResultList.TotalRoles = _roleList.Count;

            AssignStatus assignStatus = new AssignStatus();
            assignStatus.Result = true;
            assignStatus.ErrorMessage = "";

            IC.PartyRelationship _partyRelationShip = new IC.PartyRelationship();
            _partyRelationShip.RoleTypeFrom = new IC.RoleType() { Name = _ROLETYPE_NAME_USER };

            //Act
            mockService
                .Setup(m => m.ModifyRoleToRights(
                    It.IsAny<string>()
                    , It.IsAny<int>()
                    , It.IsAny<string>()
                    , It.IsAny<string>()
                ))
                .Returns(assignStatus);

            mockManageProductOneSite
              .Setup(m => m.UpdateRightsToRoleLogMessage(
                  It.IsAny<long>()
                  , It.IsAny<int>()
                  , It.IsAny<List<string>>()
                  , It.IsAny<List<string>>()
              ))
              .Verifiable();

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.IsAny<long>()
                    , It.IsAny<int>()
                 ))
                 .Returns(_editorSamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.IsAny<long>()
                ))
                .Returns(_editorPersona);

            mockManagePartyRelationship
                .Setup(m => m.GetPartyRelationship(
                    It.IsAny<Guid>()
                    , It.IsAny<Guid>()
                    , null
                    , null
                    , It.IsAny<string>()
               ))
               .Returns(_partyRelationShip);

            mockProductRepository
                .Setup(m => m.GetBooksMasterProductDetail(
                    It.Is<int>(l => l == 1)))
                .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.OneSite));

            IManageProductOneSite manageProductOneSite = new ManageProductOneSite(
		        editorRealPageId: _editorRealPageId,
                userClaim: _editorUserClaim, messageHandler: mockHttpMessageHandler.Object,
                service: mockService.Object,
		        userList: null,
		        roleList: roleResultList,
		        rightList: null,
		        propertyList: null,
				samlRepository: mockSamlRepository.Object,
		        managePersona: mockManagePersona.Object,
		        managePerson: null,
		        manageUserLogin: null,
		        userLoginRepository: null,
		        personaRepository: null,
				manageBlueBook: mockManageBlueBook.Object,
		        productRepository: mockProductRepository.Object,
		        productInternalSettingRepository: mockProductInternalSettingRepository.Object,
		        managePartyRelationship: mockManagePartyRelationship.Object,
		        manageElectronicAddress: null,
                userLoginPersonaRepository: null,
                userRepository: null, 
                repository: mockRepository.Object,
                systemIdentifier: _systemIdentifier
            );
			Persona persona = new Persona();
            int roleId = 1;
            List<string> rightToAddList = new List<string>();
            List<string> rightToRemoveList = new List<string>();
            rightToAddList.Add("1");
            rightToRemoveList.Add("2");

            string resp = manageProductOneSite.UpdateRoleToRights(_editorPersonaId, roleId, rightToAddList, rightToRemoveList);

            Assert.True(resp == ""); // no message indicates success
        }

        [Fact]
        public void Put_UpdateRightToRoles_Valid()
        {
            //Arrange
            RequestParameter reqParameter = new RequestParameter();
            reqParameter.Pages.ResultsPerPage = 1000;
            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("SortBy", "ASC");

            var mockService = new Mock<IOneSiteProductService>();
            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            var mockManagePartyRelationship = new Mock<IManagePartyRelationship>();
            var mockManageProductOneSite = new Mock<IManageProductOneSite>();

            RoleList roleResultList = new RoleList();

            List<string> _rolesToAdd = new List<string>();
            _rolesToAdd.Add("1");
            _rolesToAdd.Add("2");
            _rolesToAdd.Add("4");

            List<RoleType> _roleList = new List<RoleType>();

            _roleList.Add(_roleType1); // already assigned
            _roleList.Add(_roleType2); // not assigned
            _roleList.Add(_roleType3); // assigned and keep
            _roleList.Add(_roleTypeInternalAdmin); // assign and keep
            roleResultList.Role = _roleList.ToArray();
            roleResultList.TotalRoles = _roleList.Count;

            AssignStatus assignStatus = new AssignStatus();
            assignStatus.Result = true;
            assignStatus.ErrorMessage = "";

            IC.PartyRelationship _partyRelationShip = new IC.PartyRelationship();
            _partyRelationShip.RoleTypeFrom = new IC.RoleType() { Name = _ROLETYPE_NAME_USER };

            //Act
            mockService
                .Setup(m => m.GetRolesForRight(
                    It.IsAny<NameValuePair[]>()
                    , It.IsAny<int>()
                    , It.IsAny<bool>()
                    , It.IsAny<FilterSortParameters>()
                ))
                .Returns(roleResultList);

            //Act
            mockService
                .Setup(m => m.ModifyRightToRoles(
                    It.IsAny<string>()
                    , It.IsAny<int>()
                    , It.IsAny<string>()
                    , It.IsAny<bool>()
                ))
                .Returns(assignStatus);

            mockManageProductOneSite
               .Setup(m => m.UpdateRolesByRightLogMessage(
                   It.IsAny<long>()
                   , It.IsAny<long>()
                   , It.IsAny<List<string>>()
                   , It.IsAny<List<string>>()
               ))
               .Verifiable();

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.IsAny<long>()
                    , It.IsAny<int>()
                 ))
                 .Returns(_editorSamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.IsAny<long>()
                ))
                .Returns(_editorPersona);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.IsAny<int>()
                ))
                .Returns(_productInternalSettingsOneSite);

            mockManagePartyRelationship
                .Setup(m => m.GetPartyRelationship(
                    It.IsAny<Guid>()
                    , It.IsAny<Guid>()
                    , null
                    , null
                    , It.IsAny<string>()
               ))
               .Returns(_partyRelationShip);

            mockProductRepository
                .Setup(m => m.GetBooksMasterProductDetail(
                    It.Is<int>(l => l == 1)))
                .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.OneSite));

            IManageProductOneSite manageProductOneSite = new ManageProductOneSite(
	            editorRealPageId: _editorRealPageId, 
                userClaim: _editorUserClaim, messageHandler: mockHttpMessageHandler.Object,
                service: mockService.Object,
	            userList: null,
	            roleList: roleResultList,
	            rightList: null,
	            propertyList: null,
				samlRepository: mockSamlRepository.Object,
	            managePersona: mockManagePersona.Object,
	            managePerson: null,
	            manageUserLogin: null,
	            userLoginRepository: null,
	            personaRepository: null,
				manageBlueBook: mockManageBlueBook.Object,
	            productRepository: mockProductRepository.Object,
	            productInternalSettingRepository: mockProductInternalSettingRepository.Object,
	            managePartyRelationship: mockManagePartyRelationship.Object,
	            manageElectronicAddress: null,
                userLoginPersonaRepository: null,
                userRepository: null, 
                repository: mockRepository.Object,
                systemIdentifier: _systemIdentifier
            );
			Persona persona = new Persona();
            int rightId = 1;
            List<string> roleList = new List<string>();
            roleList.Add("1");

            string resp = manageProductOneSite.UpdateRightToRoles(_editorPersonaId, rightId, roleList, true);

            Assert.True(resp == ""); // no message indicates success
        }

        [Fact]
        public void Get_RolesForRight_Valid()
        {
            //Arrange
            RequestParameter reqParameter = new RequestParameter();
            reqParameter.Pages.ResultsPerPage = 1000;
            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("SortBy", "ASC");

            var mockService = new Mock<IOneSiteProductService>();
            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            var mockManagePartyRelationship = new Mock<IManagePartyRelationship>();

            RoleList roleResultList = new RoleList();

            List<string> _rolesToAdd = new List<string>();
            _rolesToAdd.Add("1");
            _rolesToAdd.Add("2");
            _rolesToAdd.Add("4");
            _rolesToAdd.Add("33");
            _rolesToAdd.Add("44");
            _rolesToAdd.Add("55");
            _rolesToAdd.Add("66");

            List<RoleType> _roleList = new List<RoleType>();

            _roleList.Add(_roleType1); // already assigned
            _roleList.Add(_roleType2); // not assigned
            _roleList.Add(_roleType3); // assigned and keep
            _roleList.Add(_roleType4); // shouldn't be returned
            _roleList.Add(_roleType5); // shouldn't be returned
            _roleList.Add(_roleType6); // shouldn't be returned
            _roleList.Add(_roleType7); // shouldn't be returned

            _roleList.Add(_roleTypeInternalAdmin); // assign and keep
            roleResultList.Role = _roleList.ToArray();
            roleResultList.TotalRoles = _roleList.Count;

            AssignStatus removeStatus = new AssignStatus();
            AssignStatus addStatus = new AssignStatus();

            IC.PartyRelationship _partyRelationShip = new IC.PartyRelationship();
            _partyRelationShip.RoleTypeFrom = new IC.RoleType() { Name = _ROLETYPE_NAME_USER };

            //Act
            mockService
                .Setup(m => m.GetRolesForRight(
                    It.IsAny<NameValuePair[]>()
                    , It.IsAny<int>()
                    , It.IsAny<bool>()
                    , It.IsAny<FilterSortParameters>()
                ))
                .Returns(roleResultList);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.IsAny<long>()
                    , It.IsAny<int>()
                 ))
                 .Returns(_editorSamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.IsAny<long>()
                ))
                .Returns(_editorPersona);

            mockProductRepository
                .Setup(m => m.GetProductSettingsByPersona(
                    It.IsAny<long>()
                ))
                .Returns(_userProductSettings);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.IsAny<int>()
                ))
                .Returns(_productInternalSettingsOneSite);

            mockManagePartyRelationship
                .Setup(m => m.GetPartyRelationship(
                    It.IsAny<Guid>()
                    , It.IsAny<Guid>()
                    , null
                    , null
                    , It.IsAny<string>()
               ))
               .Returns(_partyRelationShip);

            mockProductRepository
                .Setup(m => m.GetBooksMasterProductDetail(
                    It.Is<int>(l => l == 1)))
                .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.OneSite));

            IManageProductOneSite manageProductOneSite = new ManageProductOneSite(
                editorRealPageId: _editorRealPageId, 
                userClaim: _editorUserClaim, messageHandler: mockHttpMessageHandler.Object,
                service: mockService.Object,
                userList: null,
                roleList: roleResultList,
                rightList: null,
                propertyList: null,
                samlRepository: mockSamlRepository.Object,
                managePersona: mockManagePersona.Object,
                managePerson: null,
                manageUserLogin: null,
                userLoginRepository: null,
                personaRepository: null,
                manageBlueBook: mockManageBlueBook.Object,
                productRepository: mockProductRepository.Object,
                productInternalSettingRepository: mockProductInternalSettingRepository.Object,
                managePartyRelationship: mockManagePartyRelationship.Object,
                manageElectronicAddress: null,
                userLoginPersonaRepository: null,
                userRepository: null, 
                repository: mockRepository.Object,
                systemIdentifier: _systemIdentifier
            );
			Persona persona = new Persona();
            int rightId = 0;
            bool assignedOnly = true;
            ListResponse resp = manageProductOneSite.GetRolesForRight(_editorPersonaId, rightId, assignedOnly, reqParameter);

            // Assert
            Assert.True(resp.TotalRows == 3
                && (resp.Records[0] as ProductRole).ID == _roleType1.RoleID
                && (resp.Records[0] as ProductRole).Name == _roleType1.RoleName
                && (resp.Records[0] as ProductRole).Roletype == _roleType1.Roletype
                );
        }

        [Fact]
        public void Get_GetUsersForRole_Valid()
        {
            //Arrange
            var mockService = new Mock<IOneSiteProductService>();
            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockProductRepository = new Mock<IProductRepository>();

            RequestParameter reqParameter = new RequestParameter();
            reqParameter.Pages.ResultsPerPage = 1000;
            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("SortBy", "ASC");

            UserList userResultList = new UserList();
            UserList emptyResultList = new UserList();
            List<UserType> _userList = new List<UserType>();

            _userList.Add(_userType1);
            _userList.Add(_userType2);
            _userList.Add(_userType3);

            userResultList.User = _userList.ToArray();
            userResultList.TotalUsers = _userList.Count;

            int roleId = 12;
            bool assignedOnly = false;
            ListResponse resp;

            //Act
            // test exception
            mockService
                .Setup(m => m.GetUsersForRole(
                    It.IsAny<string>()
                    , It.IsAny<int>()
                    , It.IsAny<bool>()
                    , It.IsAny<FilterSortParameters>()
                ))
                .Throws(new SystemException());

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.IsAny<long>()
                    , It.IsAny<int>()
                 ))
                 .Returns(_editorSamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.IsAny<long>()
                ))
                .Returns(_editorPersona);

            mockProductRepository
                .Setup(m => m.GetProductSettingsByPersona(
                    It.IsAny<long>()
                ))
                .Returns(_userProductSettings);

            mockProductRepository
                .Setup(m => m.GetBooksMasterProductDetail(
                    It.Is<int>(l => l == 1)))
                .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.OneSite));

            //Assert
            IManageProductOneSite manageProductOneSite = new ManageProductOneSite(
                editorRealPageId: _editorRealPageId, 
                userClaim: _editorUserClaim, messageHandler: mockHttpMessageHandler.Object,
                service: mockService.Object,
                userList: emptyResultList,
                roleList: null,
                rightList: null,
                propertyList: null,
                samlRepository: mockSamlRepository.Object,
                managePersona: mockManagePersona.Object,
                managePerson: null,
                manageUserLogin: null,
                userLoginRepository: null,
                personaRepository: null,
                manageBlueBook: mockManageBlueBook.Object,
                productRepository: mockProductRepository.Object,
                productInternalSettingRepository: null,
                managePartyRelationship: null,
                manageElectronicAddress: null,
                userLoginPersonaRepository: null,
                userRepository: null, 
                repository: mockRepository.Object,
                systemIdentifier: _systemIdentifier
            );
			resp = manageProductOneSite.GetUsersForRole(_editorPersonaId, roleId, assignedOnly, reqParameter);

            Assert.True(resp.TotalRows == 0);

            mockService
                .Setup(m => m.GetUsersForRole(
                    It.IsAny<string>()
                    , It.IsAny<int>()
                    , It.IsAny<bool>()
                    , It.IsAny<FilterSortParameters>()
                ))
                .Returns(userResultList);

            mockProductRepository
                .Setup(m => m.GetProductSettingsByPersona(
                    It.IsAny<long>()
                ))
                .Returns(_userProductSettings);

			//Act
			manageProductOneSite = new ManageProductOneSite(
				editorRealPageId: _editorRealPageId, 
                userClaim: _editorUserClaim, messageHandler: mockHttpMessageHandler.Object,
				service: mockService.Object,
				userList: userResultList,
				roleList: null,
				rightList: null,
				propertyList: null,
				samlRepository: mockSamlRepository.Object,
				managePersona: mockManagePersona.Object,
				managePerson: null,
				manageUserLogin: null,
				userLoginRepository: null,
				personaRepository: null,
				manageBlueBook: mockManageBlueBook.Object,
				productRepository: mockProductRepository.Object,
				productInternalSettingRepository: null,
				managePartyRelationship: null,
				manageElectronicAddress: null,
                userLoginPersonaRepository: null,
                userRepository: null, 
                repository: mockRepository.Object,
                systemIdentifier: _systemIdentifier
            );
			resp = manageProductOneSite.GetUsersForRole(_editorPersonaId, roleId, assignedOnly, reqParameter);
            Assert.True(resp.TotalRows == 3);
            Assert.True(
                (resp.Records[0] as Component.SharedObjects.Product.ProductUser).UserId == _userType1.UserId
                && (resp.Records[0] as Component.SharedObjects.Product.ProductUser).UserLogin == _userType1.UserLogin
                && (resp.Records[0] as Component.SharedObjects.Product.ProductUser).UserName == _userType1.UserName
                && (resp.Records[0] as Component.SharedObjects.Product.ProductUser).IsAssigned == _userType1.Assigned
                );

            Assert.True(
                (resp.Records[1] as Component.SharedObjects.Product.ProductUser).UserId == _userType2.UserId
                && (resp.Records[1] as Component.SharedObjects.Product.ProductUser).UserLogin == _userType2.UserLogin
                && (resp.Records[1] as Component.SharedObjects.Product.ProductUser).UserName == _userType2.UserName
                && (resp.Records[1] as Component.SharedObjects.Product.ProductUser).IsAssigned == _userType2.Assigned
                );

            Assert.True(
                (resp.Records[2] as Component.SharedObjects.Product.ProductUser).UserId == _userType3.UserId
                && (resp.Records[2] as Component.SharedObjects.Product.ProductUser).UserLogin == _userType3.UserLogin
                && (resp.Records[2] as Component.SharedObjects.Product.ProductUser).UserName == _userType3.UserName
                && (resp.Records[2] as Component.SharedObjects.Product.ProductUser).IsAssigned == _userType3.Assigned
                );
        }
        #endregion

        #region Rights

        [Fact]
        public void Get_RightCenterList_All()
        {
            //Arrange
            RequestParameter reqParameter = new RequestParameter();
            reqParameter.Pages.ResultsPerPage = 1000;
            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("SortBy", "ASC");

            var mockService = new Mock<IOneSiteProductService>();
            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockProductRepository = new Mock<IProductRepository>();
            //var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();

            RightCenter resultList = new RightCenter();

            List<string> _rightCenterList = new List<string>();

            _rightCenterList.Add("Center1");
            _rightCenterList.Add("Center2");
            resultList.RightCenters = _rightCenterList.ToArray();

            //Act
            mockService
                 .Setup(m => m.GetRightsCentersList(
                        It.IsAny<NameValuePair[]>()
                        , It.IsAny<string>()
                 ))
                 .Returns(resultList);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.IsAny<long>()
                    , It.IsAny<int>()
                 ))
                 .Returns(_editorSamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.IsAny<long>()
                ))
                .Returns(_editorPersona);

            mockProductRepository
                .Setup(m => m.GetProductSettingsByPersona(
                    It.IsAny<long>()
                ))
                .Returns(_userProductSettings);

            mockProductRepository
                .Setup(m => m.GetBooksMasterProductDetail(
                    It.Is<int>(l => l == 1)))
                .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.OneSite));

            ProductInternalSettingRepository productInternalSettingRepository = new ProductInternalSettingRepository(mockRepository.Object);
            new RPObjectCache().BustCache();
            
            IManageProductOneSite manageProductOneSite = new ManageProductOneSite(_editorRealPageId, _editorUserClaim, mockService.Object, mockSamlRepository.Object, mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object, productInternalSettingRepository, mockHttpMessageHandler.Object, repository: mockRepository.Object);
            Persona persona = new Persona();
            ListResponse resp = manageProductOneSite.GetOneSiteRightsCenters(_editorPersonaId);

            //Assert
            Assert.True(_rightCenterList[0] == resp.Records[0] as string
                && _rightCenterList[1] == resp.Records[1] as string
            );
        }

        [Fact]
        public void Get_RightsList_All()
        {
            //Arrange
            RequestParameter reqParameter = new RequestParameter();
            reqParameter.Pages.ResultsPerPage = 1000;
            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("SortBy", "ASC");

            var mockService = new Mock<IOneSiteProductService>();
            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();

            RightList rightResultList = new RightList();

            List<RightType> _rightList = new List<RightType>();

            _rightList.Add(_rightType1);
            _rightList.Add(_rightType2);
            _rightList.Add(_rightType3);
            rightResultList.Right = _rightList.ToArray();
            rightResultList.TotalRights = _rightList.Count;

            //Act
            mockService
                 .Setup(m => m.GetRightsList(
                        It.IsAny<NameValuePair[]>()
                        , It.IsAny<string>()
                        , It.IsAny<FilterSortParameters>()
                 ))
                 .Returns(rightResultList);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.IsAny<long>()
                    , It.IsAny<int>()
                 ))
                 .Returns(_editorSamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.IsAny<long>()
                ))
                .Returns(_editorPersona);

            mockProductRepository
                .Setup(m => m.GetProductSettingsByPersona(
                    It.IsAny<long>()
                ))
                .Returns(_userProductSettings);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.IsAny<int>()
                ))
                .Returns(_productInternalSettingsOneSite);

            mockProductRepository
              .Setup(m => m.GetBooksMasterProductDetail(
                  It.Is<int>(l => l == 1)))
              .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.OneSite));

            IManageProductOneSite manageProductOneSite = new ManageProductOneSite(
		        editorRealPageId: _editorRealPageId, 
                userClaim: _editorUserClaim, messageHandler: mockHttpMessageHandler.Object,
		        service: mockService.Object,
		        userList: null,
		        roleList: null,
		        rightList: rightResultList,
		        propertyList: null,
				samlRepository: mockSamlRepository.Object,
		        managePersona: mockManagePersona.Object,
		        managePerson: null,
		        manageUserLogin: null,
		        userLoginRepository: null,
		        personaRepository: null,
				manageBlueBook: mockManageBlueBook.Object,
		        productRepository: mockProductRepository.Object,
		        productInternalSettingRepository: mockProductInternalSettingRepository.Object,
		        managePartyRelationship: null,
		        manageElectronicAddress: null,
                userLoginPersonaRepository: null,
                userRepository: null, 
                repository: mockRepository.Object,
                systemIdentifier: _systemIdentifier
            );

            Persona persona = new Persona();
            ListResponse resp = manageProductOneSite.GetOneSiteRights(_editorPersonaId, reqParameter);

            ProductRight _responseRight1 = resp.Records[0] as ProductRight;
            ProductRight _responseRight2 = resp.Records[1] as ProductRight;
            ProductRight _responseRight3 = resp.Records[2] as ProductRight;

            //Assert
            Assert.True(_rightType1.RightID == _responseRight1.ID.ToString()
                && _rightType1.RightDescription == _responseRight1.Description
                && _rightType1.CenterName == _responseRight1.CenterName
                && _rightType1.Assigned == _responseRight1.Assigned
                && _rightType1.RolesAssigned == _responseRight1.RolesAssigned
            );
            Assert.True(_rightType2.RightID == _responseRight2.ID.ToString()
                && _rightType2.RightDescription == _responseRight2.Description
                && _rightType2.CenterName == _responseRight2.CenterName
                && _rightType2.Assigned == _responseRight2.Assigned
                && _rightType2.RolesAssigned == _responseRight2.RolesAssigned
            );
            Assert.True(_rightType3.RightID == _responseRight3.ID.ToString()
                && _rightType3.RightDescription == _responseRight3.Description
                && _rightType3.CenterName == _responseRight3.CenterName
                && _rightType3.Assigned == _responseRight3.Assigned
                && _rightType3.RolesAssigned == _responseRight3.RolesAssigned
            );
        }

        #endregion

        #region Users

        [Fact]
        public void Post_RegularUser()
        {
            //Arrange
	        AssertInitial();

			IManageProductOneSite manageProductOneSite = new ManageProductOneSite(
		        editorRealPageId: _editorRealPageId, 
                userClaim: _editorUserClaim, messageHandler: mockHttpMessageHandler.Object,
		        service: _mockService.Object,
		        userList: null,
		        roleList: null,
		        rightList: null,
		        propertyList: null,
		        samlRepository: _mockSamlRepository.Object,
		        managePersona: _mockManagePersona.Object,
				managePerson: _mockManagePerson.Object,
		        manageUserLogin: _mockManageUserLogin.Object,
		        userLoginRepository: _mockUserLoginRepository.Object,
		        personaRepository: _mockPersonaRepository.Object,
				manageBlueBook: _mockManageBlueBook.Object,
		        productRepository: _mockProductRepository.Object,
		        productInternalSettingRepository: _mockProductInternalSettingRepository.Object,
		        managePartyRelationship: _mockManagePartyRelationship.Object,
		        manageElectronicAddress: _mockManageElectronicAddress.Object,
                userLoginPersonaRepository: _mockUserLoginPersonaRepository.Object,
                userRepository: _mockUserRepository.Object, repository: mockRepository.Object,
                systemIdentifier: _systemIdentifier
            );

			long oneSitePersonaId = 10;
            List<string> roleToAddList = new List<string>();
            roleToAddList.Add("12");
            List<string> propertyToAddList = new List<string>();
            propertyToAddList.Add("1234567");

            string result = manageProductOneSite.ManageOneSiteUser(_editorPersonaId, oneSitePersonaId, roleToAddList, propertyToAddList, out List<AdditionalParameters> additionalParameters);

            //Assert
            Assert.True(result == "");
        }

        [Fact]
        public void Post_SuperUser()
        {
            //Arrange
			AssertInitial();

			IManageProductOneSite manageProductOneSite = new ManageProductOneSite(
				editorRealPageId: _editorRealPageId, 
                userClaim: _editorUserClaim, messageHandler: mockHttpMessageHandler.Object,
				service: _mockService.Object,
				userList: null,
				roleList: null,
				rightList: null,
				propertyList: null,
				samlRepository: _mockSamlRepository.Object,
				managePersona: _mockManagePersona.Object,
				managePerson: _mockManagePerson.Object,
				manageUserLogin: _mockManageUserLogin.Object,
				userLoginRepository: _mockUserLoginRepository.Object,
				personaRepository: _mockPersonaRepository.Object,
				manageBlueBook: _mockManageBlueBook.Object,
				productRepository: _mockProductRepository.Object,
				productInternalSettingRepository: _mockProductInternalSettingRepository.Object,
				managePartyRelationship: _mockManagePartyRelationship.Object,
				manageElectronicAddress: _mockManageElectronicAddress.Object,
                userLoginPersonaRepository: _mockUserLoginPersonaRepository.Object,
                userRepository: _mockUserRepository.Object, repository: mockRepository.Object,
                systemIdentifier: _systemIdentifier
            );

			long oneSitePersonaId = 10;
            List<string> roleToAddList = new List<string>();
            roleToAddList.Add("12");
            List<string> propertyToAddList = new List<string>();
            propertyToAddList.Add("1234567");

            string result = manageProductOneSite.ManageOneSiteUser(_editorPersonaId, oneSitePersonaId, roleToAddList, propertyToAddList, out List<AdditionalParameters> additionalParameters);

            //Assert
            Assert.True(result == "");

            //_mockSamlRepository
            //    .Setup(m => m.GetProductSamlDetails(
            //        It.IsAny<long>()
            //        , It.IsAny<int>()
            //        ))
            //        .Returns(samlAttributes);

            manageProductOneSite = new ManageProductOneSite(
		        editorRealPageId: _editorRealPageId, 
                userClaim: _editorUserClaim, messageHandler: mockHttpMessageHandler.Object,
		        service: _mockService.Object,
		        userList: null,
		        roleList: null,
		        rightList: null,
		        propertyList: null,
		        samlRepository: _mockSamlRepository.Object,
		        managePersona: _mockManagePersona.Object,
		        managePerson: _mockManagePerson.Object,
		        manageUserLogin: _mockManageUserLogin.Object,
		        userLoginRepository: _mockUserLoginRepository.Object,
		        personaRepository: _mockPersonaRepository.Object,
		        manageBlueBook: _mockManageBlueBook.Object,
		        productRepository: _mockProductRepository.Object,
		        productInternalSettingRepository: _mockProductInternalSettingRepository.Object,
		        managePartyRelationship: _mockManagePartyRelationship.Object,
		        manageElectronicAddress: _mockManageElectronicAddress.Object,
                userLoginPersonaRepository: _mockUserLoginPersonaRepository.Object,
                userRepository: _mockUserRepository.Object, repository: mockRepository.Object,
                systemIdentifier: _systemIdentifier
            );

			// Update
			result = manageProductOneSite.ManageOneSiteUser(_editorPersonaId, oneSitePersonaId, roleToAddList, propertyToAddList, out additionalParameters);
            Assert.True(result == "");
        }

        [Fact]
        public void Put_User()
        {
            //Arrange
			AssertInitial();

	        IManageProductOneSite manageProductOneSite = new ManageProductOneSite(
		        editorRealPageId: _editorRealPageId, 
                userClaim: _editorUserClaim, messageHandler: mockHttpMessageHandler.Object,
		        service: _mockService.Object,
		        userList: null,
		        roleList: null,
		        rightList: null,
		        propertyList: null,
		        samlRepository: _mockSamlRepository.Object,
		        managePersona: _mockManagePersona.Object,
		        managePerson: _mockManagePerson.Object,
		        manageUserLogin: _mockManageUserLogin.Object,
		        userLoginRepository: _mockUserLoginRepository.Object,
		        personaRepository: _mockPersonaRepository.Object,
				manageBlueBook: _mockManageBlueBook.Object,
		        productRepository: _mockProductRepository.Object,
		        productInternalSettingRepository: _mockProductInternalSettingRepository.Object,
		        managePartyRelationship: _mockManagePartyRelationship.Object,
		        manageElectronicAddress: _mockManageElectronicAddress.Object,
                userLoginPersonaRepository: _mockUserLoginPersonaRepository.Object,
                userRepository: _mockUserRepository.Object, repository: mockRepository.Object,
                systemIdentifier: _systemIdentifier
            );

			long oneSitePersonaId = 10;
            List<string> roleToAddList = new List<string>();
            roleToAddList.Add("12");
            List<string> propertyToAddList = new List<string>();
            propertyToAddList.Add("1234567");

            string result = manageProductOneSite.ManageOneSiteUser(_editorPersonaId, _userPersonaId, roleToAddList, propertyToAddList, out List<AdditionalParameters> additionalParameters1);
            //Assert
            Assert.True(result == "");

            result = manageProductOneSite.ManageOneSiteUser(_editorPersonaId, _userPersonaId, null, propertyToAddList, out List<AdditionalParameters> additionalParameters2);
            //Assert
            Assert.True(result == "");

            result = manageProductOneSite.ManageOneSiteUser(_editorPersonaId, _userPersonaId, roleToAddList, null, out List<AdditionalParameters> additionalParameters3);
            //Assert
            Assert.True(result == "");

            //_userlogin.IsSuperUser = true;

            _mockManageUserLogin
                .Setup(m => m.GetUserLoginOnly(
                        It.IsAny<Guid>()
                    ))
                    .Returns(_userlogin);

            _mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 10)
                    , It.IsAny<int>()
                 ))
                 .Returns(_editorSamlAttributes);

	        manageProductOneSite = new ManageProductOneSite(
		        editorRealPageId: _editorRealPageId, 
                userClaim: _editorUserClaim, messageHandler: mockHttpMessageHandler.Object,
		        service: _mockService.Object,
		        userList: null,
		        roleList: null,
		        rightList: null,
		        propertyList: null,
		        samlRepository: _mockSamlRepository.Object,
		        managePersona: _mockManagePersona.Object,
		        managePerson: _mockManagePerson.Object,
		        manageUserLogin: _mockManageUserLogin.Object,
		        userLoginRepository: _mockUserLoginRepository.Object,
		        personaRepository: _mockPersonaRepository.Object,
		        manageBlueBook: _mockManageBlueBook.Object,
		        productRepository: _mockProductRepository.Object,
		        productInternalSettingRepository: _mockProductInternalSettingRepository.Object,
		        managePartyRelationship: _mockManagePartyRelationship.Object,
		        manageElectronicAddress: _mockManageElectronicAddress.Object,
                userLoginPersonaRepository: _mockUserLoginPersonaRepository.Object,
                userRepository: _mockUserRepository.Object, repository: mockRepository.Object,
                systemIdentifier: _systemIdentifier
            );

            result = manageProductOneSite.ManageOneSiteUser(_editorPersonaId, oneSitePersonaId, roleToAddList, propertyToAddList, out List<AdditionalParameters> additionalParameters);
            //Assert
            Assert.True(result == "");

            List<SamlAttributes> noAttributes = new List<SamlAttributes>();

            _mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 10)
                    , It.IsAny<int>()
                 ))
                 .Returns(noAttributes);

            manageProductOneSite = new ManageProductOneSite(
		        editorRealPageId: _editorRealPageId, 
                userClaim: _editorUserClaim, messageHandler: mockHttpMessageHandler.Object,
		        service: _mockService.Object,
		        userList: null,
		        roleList: null,
		        rightList: null,
		        propertyList: null,
		        samlRepository: _mockSamlRepository.Object,
		        managePersona: _mockManagePersona.Object,
		        managePerson: _mockManagePerson.Object,
		        manageUserLogin: _mockManageUserLogin.Object,
		        userLoginRepository: _mockUserLoginRepository.Object,
		        personaRepository: _mockPersonaRepository.Object,
		        manageBlueBook: _mockManageBlueBook.Object,
		        productRepository: _mockProductRepository.Object,
		        productInternalSettingRepository: _mockProductInternalSettingRepository.Object,
		        managePartyRelationship: _mockManagePartyRelationship.Object,
		        manageElectronicAddress: _mockManageElectronicAddress.Object,
                userLoginPersonaRepository: _mockUserLoginPersonaRepository.Object,
                userRepository: _mockUserRepository.Object, repository: mockRepository.Object,
                systemIdentifier: _systemIdentifier
            );
			result = manageProductOneSite.ManageOneSiteUser(_editorPersonaId, oneSitePersonaId, null, null, out List<AdditionalParameters> additionalParameters4);
            //Assert
            Assert.True(result == "");
        }

public void Delete_User()
        {
            //Arrange
            RequestParameter reqParameter = new RequestParameter();
            reqParameter.Pages.ResultsPerPage = 1000;
            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("SortBy", "ASC");

            var mockService = new Mock<IOneSiteProductService>();
            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
			var mockManageUserLogin = new Mock<IUserLoginRepository>();

            IList<SamlAttributes> samlAttributes = new List<SamlAttributes>();
            samlAttributes.Add(new SamlAttributes() { Name = "UserId", Value = "1234567|username" });

            //Act
            mockService
                 .Setup(m => m.DeleteUser(
                        It.IsAny<string>()
                        , It.IsAny<string>()
                 ));

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.IsAny<long>()
                    , It.IsAny<int>()
                 ))
                 .Returns(samlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.IsAny<long>()
                ))
                .Returns(_editorPersona);

            mockProductRepository
                .Setup(m => m.GetProductSettingsByPersona(
                    It.IsAny<long>()
                ))
                .Returns(_userProductSettings);

            mockProductRepository
                .Setup(m => m.ListProductSettingType(
                ))
                .Returns(_productSettingType);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.IsAny<int>()
                ))
                .Returns(_productInternalSettingsOneSite);

			mockManageUserLogin
				.Setup(m => m.GetUserLoginOnly(
					It.IsAny<Guid>()
				))
				.Returns(_userlogin);

            IManageProductOneSite manageProductOneSite = new ManageProductOneSite(_editorRealPageId, _editorUserClaim, mockService.Object, mockSamlRepository.Object, mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object, mockProductInternalSettingRepository.Object,
                mockHttpMessageHandler.Object, repository: mockRepository.Object);
            Persona persona = new Persona();
            long deletePersonaId = 10;
            string result = manageProductOneSite.DeleteOneSiteUser(_editorPersonaId, deletePersonaId);

            //Assert
            Assert.True(string.IsNullOrEmpty(result));
        }

        [Fact]
        public void Put_UserStatus()
        {
            //Arrange
            RequestParameter reqParameter = new RequestParameter();
            reqParameter.Pages.ResultsPerPage = 1000;
            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("SortBy", "ASC");

            var mockService = new Mock<IOneSiteProductService>();
            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();

            //Act
            mockService
                 .Setup(m => m.EnableUser(
                        It.IsAny<string>()
                 ));

            mockService
                 .Setup(m => m.DisableUser(
                        It.IsAny<string>()
                 ));

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.IsAny<long>()
                    , It.IsAny<int>()
                 ))
                 .Returns(_editorSamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.IsAny<long>()
                ))
                .Returns(_editorPersona);

            mockProductRepository
                .Setup(m => m.GetProductSettingsByPersona(
                    It.IsAny<long>()
                ))
                .Returns(_userProductSettings);

            mockProductRepository
                .Setup(m => m.ListProductSettingType(
                ))
                .Returns(_productSettingType);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.IsAny<int>()
                ))
                .Returns(_productInternalSettingsOneSite);

            mockProductRepository
              .Setup(m => m.GetBooksMasterProductDetail(
                  It.Is<int>(l => l == 1)))
              .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.OneSite));

            IManageProductOneSite manageProductOneSite = new ManageProductOneSite(_editorRealPageId, _editorUserClaim, mockService.Object, mockSamlRepository.Object, mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object, mockProductInternalSettingRepository.Object,
                mockHttpMessageHandler.Object, repository: mockRepository.Object);

            string result = manageProductOneSite.EnableOneSiteUser(_editorPersonaId, _userPersonaId, true);

            //Assert
            Assert.True(string.IsNullOrEmpty(result));

            result = manageProductOneSite.EnableOneSiteUser(_editorPersonaId, _userPersonaId, false);
            Assert.True(string.IsNullOrEmpty(result));
        }

        [Fact]
        public void Get_UserInLeasingAgentList()
        {
            //Arrange
            var mockService = new Mock<IOneSiteProductService>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockManagePartyRelationship = new Mock<IManagePartyRelationship>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();

            UserList emptyResultList = new UserList();

            mockService
                .Setup(m => m.GetUserInLeasingAgentList(
                    It.IsAny<string>()
                    , It.IsAny<int>()
                 ))
                 .Returns(true);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.IsAny<long>()
                    , It.IsAny<int>()
                 ))
                 .Returns(_editorSamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.IsAny<long>()
                ))
                .Returns(_editorPersona);

            mockProductRepository
                .Setup(m => m.GetProductSettingsByPersona(
                    It.IsAny<long>()
                ))
                .Returns(_userProductSettings);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.IsAny<int>()
                ))
                .Returns(_productInternalSettingsOneSite);

            mockProductRepository
                .Setup(m => m.GetBooksMasterProductDetail(
                    It.Is<int>(l => l == 1)))
                .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.OneSite));

            IManageProductOneSite manageProductOneSite = new ManageProductOneSite(_editorRealPageId, _editorUserClaim, mockService.Object, mockSamlRepository.Object, mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object, mockProductInternalSettingRepository.Object,
                mockHttpMessageHandler.Object, repository: mockRepository.Object);

            bool result = manageProductOneSite.UserInLeasingAgentList(_editorPersonaId, _userPersonaId, 1234567);
            Assert.True(result == true);

            mockService
                .Setup(m => m.GetUserInLeasingAgentList(
                    It.IsAny<string>()
                    , It.IsAny<int>()
                 ))
                 .Throws(new Exception("Invalid user"));

            manageProductOneSite = new ManageProductOneSite(_editorRealPageId, _editorUserClaim, mockService.Object, mockSamlRepository.Object, mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object, mockProductInternalSettingRepository.Object,
                mockHttpMessageHandler.Object, repository: mockRepository.Object);

            result = manageProductOneSite.UserInLeasingAgentList(_editorPersonaId, _userPersonaId, 1234567);
            Assert.True(result == false);

        }

        [Fact]
        public void Get_PMCURL()
        {
            //Arrange
            var mockService = new Mock<IOneSiteProductService>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockManagePartyRelationship = new Mock<IManagePartyRelationship>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();

            PMCInfo pmcinfo = new PMCInfo() { ID = 1234567, PMCURL = _pmcUrl };
			IList<ProductSettingList> productSettingList = new List<ProductSettingList>() { new ProductSettingList() { Name = "OVERRIDEPMCID", Value = "1234567" } };

			mockService
                .Setup(m => m.GetPMCUrl(
                    It.IsAny<int>()
                 ))
                 .Throws(new Exception("Invalid PMCID"));

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.IsAny<long>()
                    , It.IsAny<int>()
                 ))
                 .Returns(_editorSamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.IsAny<long>()
                ))
                .Returns(_editorPersona);

            mockProductRepository
                .Setup(m => m.GetProductSettingsByPersona(
                    It.IsAny<long>()
                ))
                .Returns(_userProductSettings);

			mockProductRepository
				.Setup(m => m.GetProductSettings(
					It.IsAny<Guid>(),
					It.IsAny<int>()))
				.Returns(productSettingList);

			mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.IsAny<int>()
                ))
                .Returns(_productInternalSettingsOneSite);

            mockProductRepository
              .Setup(m => m.GetBooksMasterProductDetail(
                  It.Is<int>(l => l == 1)))
              .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.OneSite));

            IManageProductOneSite manageProductOneSite = new ManageProductOneSite(_editorRealPageId, _editorUserClaim, mockService.Object, mockSamlRepository.Object, mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object, mockProductInternalSettingRepository.Object,
                mockHttpMessageHandler.Object, repository: mockRepository.Object);
            
            // get an error initially
			
			// need to figure out how to handle exceptions in caching
			PMCInfo result = manageProductOneSite.GetPMCURL(_userPersonaId);
			Assert.Null(result);

			// need to figure out how to handle exceptions in caching

			mockService
				.Setup(m => m.GetPMCUrl(
                    It.IsAny<int>()
                 ))
                 .Returns(pmcinfo);

            manageProductOneSite = new ManageProductOneSite(_editorRealPageId, _editorUserClaim, mockService.Object, mockSamlRepository.Object, mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object, mockProductInternalSettingRepository.Object,
                mockHttpMessageHandler.Object, repository: mockRepository.Object);

            var pmcResult = manageProductOneSite.GetPMCURL(_userPersonaId);
			Assert.True(pmcResult.ID == pmcinfo.ID && pmcResult.PMCURL == pmcinfo.PMCURL);

		}
        #endregion

        #region Migration
        [Fact]
        public void GetMigrationUsers_GivenEditorIdAndFilters_ShouldReturnListOfOneSiteUsers()
        {
            //Arrange
            var filter = "NonMigrated";
            var startRow = 0;
            var resultsPerPage = 1000;
            var pmcID = 12345;
            RequestParameter dataFilter = new RequestParameter()
            {
                FilterBy = new Dictionary<string, string>() {
                     { "filter", filter }
                 },
                Pages = new PageRequest() {
                    ResultsPerPage = resultsPerPage,
                    StartRow = startRow
                }
            };

            var mockService = new Mock<IOneSiteProductService>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockSamlRepository = new Mock<ISamlRepository>();
            //var mockManagePartyRelationship = new Mock<IManagePartyRelationship>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();

            PMCInfo pmcInfo = new PMCInfo() { ID = pmcID, PMCURL = _pmcUrl };

            IList<CustomerCompanyMap> mapResource = new List<CustomerCompanyMap>();
            CustomerCompanyMap resource = new CustomerCompanyMap() { CompanyInstanceSourceId = pmcID.ToString(), Source = "OS" };
            mapResource.Add(resource);

            _editorSamlAttributes = new List<SamlAttributes>()
            {
                new SamlAttributes() { Name = "UserId", Value = "12345" },
                new SamlAttributes() { Name = "PRODUCTUSERNAME", Value = "bob12" }
            };

            mockService
                .Setup(m => m.GetPMCUrl(pmcID))
                 .Returns(pmcInfo);

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
            
            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.IsAny<long>()
                 ))
                 .Returns(_editorPersona);            

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.IsAny<int>()
                ))
                .Returns(_productInternalSettingsOneSite);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == _editorPersonaId)
                    , It.Is<int>(l => l == (int)ProductEnum.OneSite)
                 ))
                 .Returns(_editorSamlAttributes);

            mockProductRepository
                .Setup(m => m.GetBooksMasterProductDetail(
                    It.Is<int>(l => l == 1)))
                .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.OneSite));

            var url = $"https://{_pmcUrl}/{_mtApiEndPoint}/{pmcID}/users?filter={filter}&startRow={startRow}&resultsPerPage={resultsPerPage}";
            var editorPersonaId = _editorPersonaId;
            var expected = new List<MigrationUser>()
            {
                new MigrationUser() { FirstName = "Person", LastName="1", Email = "person1@test.com", Username="person1" },
                new MigrationUser() { FirstName = "Person", LastName="2", Email = "person2@test.com", Username="person2" },
                new MigrationUser() { FirstName = "Person", LastName="3", Email = "person3@test.com", Username="person3" },
                new MigrationUser() { FirstName = "Person", LastName="4", Email = "person4@test.com", Username="person4" },
                new MigrationUser() { FirstName = "Person", LastName="5", Email = "person5@test.com", Username="person5" }
            };
            HttpResponseMessage userResponse = new HttpResponseMessage(HttpStatusCode.OK);
            userResponse.Content = new StringContent(JsonConvert.SerializeObject(expected));

            mockHttpMessageHandler.Setup(HttpMethod.Get, url, userResponse);

            IManageProductOneSite manageProductOneSite = new ManageProductOneSite(_editorRealPageId, _editorUserClaim, mockService.Object, mockSamlRepository.Object, 
                mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object, mockProductInternalSettingRepository.Object, 
                mockHttpMessageHandler.Object, repository: mockRepository.Object);
            
            //Act
            var actual = manageProductOneSite.GetMigrationUsers(editorPersonaId, dataFilter);

            //Assert
            Assert.True(expected.Count == actual.Records.Count);
            Assert.Same(actual.ErrorReason, "");
        }

        [Fact]
        public void GetMigrationUsers_GivenEditorIdAndFiltersWhenPMCInfoNotFound_ShouldReturnErrorResponse()
        {
            //Arrange
            var filter = "NonMigrated";
            var startRow = 0;
            var resultsPerPage = 1000;
            var pmcID = 67890;
            RequestParameter dataFilter = new RequestParameter()
            {
                FilterBy = new Dictionary<string, string>() {
                     { "filter", filter }
                 },
                Pages = new PageRequest()
                {
                    ResultsPerPage = resultsPerPage,
                    StartRow = startRow
                }
            };

            var mockService = new Mock<IOneSiteProductService>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockSamlRepository = new Mock<ISamlRepository>();
            //var mockManagePartyRelationship = new Mock<IManagePartyRelationship>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();

            PMCInfo pmcinfo = new PMCInfo() { ID = pmcID, PMCURL = _pmcUrl };

            IList<CustomerCompanyMap> mapResource = new List<CustomerCompanyMap>();
            CustomerCompanyMap resource = new CustomerCompanyMap() { CompanyInstanceSourceId = pmcID.ToString(), Source = "OS" };
            mapResource.Add(resource);

            _editorSamlAttributes = new List<SamlAttributes>()
            {
                new SamlAttributes() { Name = "UserId", Value = "67890" },
                new SamlAttributes() { Name = "PRODUCTUSERNAME", Value = "bob12" }
            };

            mockService
                .Setup(m => m.GetPMCUrl(pmcID))
                 .Throws(new Exception());

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

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.IsAny<long>()
                 ))
                 .Returns(_editorPersona);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.IsAny<int>()
                ))
                .Returns(_productInternalSettingsOneSite);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == _editorPersonaId)
                    , It.Is<int>(l => l == (int)ProductEnum.OneSite)
                 ))
                 .Returns(_editorSamlAttributes);

            mockProductRepository
              .Setup(m => m.GetBooksMasterProductDetail(
                  It.Is<int>(l => l == 1)))
              .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.OneSite));

            var url = $"https://{_pmcUrl}/{_mtApiEndPoint}/{pmcID}/users?filter={filter}&startRow={startRow}&resultsPerPage={resultsPerPage}";
            var editorPersonaId = _editorPersonaId;
            
            HttpResponseMessage userResponse = new HttpResponseMessage(HttpStatusCode.OK);
            userResponse.Content = null;
            mockHttpMessageHandler.Setup(HttpMethod.Get, url, userResponse);

            IManageProductOneSite manageProductOneSite = new ManageProductOneSite(_editorRealPageId, _editorUserClaim, mockService.Object, mockSamlRepository.Object,
                mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object, mockProductInternalSettingRepository.Object,
                mockHttpMessageHandler.Object, repository: mockRepository.Object);

            //Act
            var actual = manageProductOneSite.GetMigrationUsers(editorPersonaId, dataFilter);

            //Assert
            Assert.True(actual.IsError);
            Assert.Equal($"Could not get PMC Info for company Instance Source id - {pmcID}.", actual.ErrorReason);
        }
        #endregion

        #region User-Status

        [Fact]
        public void ChangeUserStatus_Given_SystemIdentifier_Enable_ShouldReturn_True()
        {
            //Arrange
            var pmcID = 123456;
            var editorPersonaId = _editorPersonaId;

            var mockService = new Mock<IOneSiteProductService>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();

            IList<CustomerCompanyMap> mapResource = new List<CustomerCompanyMap>();
            CustomerCompanyMap resource = new CustomerCompanyMap() { CompanyInstanceSourceId = pmcID.ToString(), Source = "OS" };
            mapResource.Add(resource);

            _editorSamlAttributes = new List<SamlAttributes>()
            {
                new SamlAttributes() { Name = "UserId", Value = pmcID.ToString() },
                new SamlAttributes() { Name = "PRODUCTUSERNAME", Value = "bob12" }
            };

            mockService
                .Setup(m => m.EnableUser(It.IsAny<string>()));

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

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.IsAny<long>()
                 ))
                 .Returns(_editorPersona);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.IsAny<int>()
                ))
                .Returns(_productInternalSettingsOneSite);

            mockSamlRepository
               .Setup(m => m.GetProductSamlDetails(
                   It.Is<long>(l => l == _editorPersonaId)
                   , It.Is<int>(l => l == (int)ProductEnum.OneSite)
                ))
                .Returns(_editorSamlAttributes);

            mockProductRepository
               .Setup(m => m.GetBooksMasterProductDetail(
                   It.Is<int>(l => l == 1)))
               .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.OneSite));

            IManageProductOneSite manageProductOneSite = new ManageProductOneSite(_editorRealPageId, _editorUserClaim, mockService.Object, mockSamlRepository.Object,
                mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object, mockProductInternalSettingRepository.Object,
                mockHttpMessageHandler.Object, repository: mockRepository.Object);

            var username = "testuser";
            var isActive = true;

            //Act
            var actual = manageProductOneSite.ChangeUserStatus(editorPersonaId, username, isActive);

            //Assert
            Assert.True(actual);
        }

        [Fact]
        public void ChangeUserStatus_Given_SystemIdentifier_Disable_ShouldReturn_True()
        {
            //Arrange
            var pmcID = 123456;
            var editorPersonaId = _editorPersonaId;

            var mockService = new Mock<IOneSiteProductService>();
            var mockManageBlueBook = new Mock<IManageBlueBook>();
            var mockManagePersona = new Mock<IManagePersona>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockSamlRepository = new Mock<ISamlRepository>();
            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();

            IList<CustomerCompanyMap> mapResource = new List<CustomerCompanyMap>();
            CustomerCompanyMap resource = new CustomerCompanyMap() { CompanyInstanceSourceId = pmcID.ToString(), Source = "OS" };
            mapResource.Add(resource);

            _editorSamlAttributes = new List<SamlAttributes>()
            {
                new SamlAttributes() { Name = "UserId", Value = pmcID.ToString() },
                new SamlAttributes() { Name = "PRODUCTUSERNAME", Value = "bob12" }
            };

            mockService
                .Setup(m => m.EnableUser(It.IsAny<string>()));

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

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.IsAny<long>()
                 ))
                 .Returns(_editorPersona);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.IsAny<int>()
                ))
                .Returns(_productInternalSettingsOneSite);

            mockSamlRepository
               .Setup(m => m.GetProductSamlDetails(
                   It.Is<long>(l => l == _editorPersonaId)
                   , It.Is<int>(l => l == (int)ProductEnum.OneSite)
                ))
                .Returns(_editorSamlAttributes);

            mockProductRepository
              .Setup(m => m.GetBooksMasterProductDetail(
                  It.Is<int>(l => l == 1)))
              .Returns(_gbProductMap.FirstOrDefault(p => p.ProductId == (int)ProductEnum.OneSite));

            IManageProductOneSite manageProductOneSite = new ManageProductOneSite(_editorRealPageId, _editorUserClaim, mockService.Object, mockSamlRepository.Object,
                mockManagePersona.Object, mockManageBlueBook.Object, mockProductRepository.Object, mockProductInternalSettingRepository.Object,
                mockHttpMessageHandler.Object, repository: mockRepository.Object);

            var username = "testuser";
            var isActive = false;

            new RPObjectCache().BustCache();

            //Act
            var actual = manageProductOneSite.ChangeUserStatus(editorPersonaId, username, isActive);

            //Assert
            Assert.True(actual);
        }

        #endregion

        #endregion
    }
	[ExcludeFromCodeCoverage]
	public static class MoqExtensions
    {
        public static void ReturnsInOrder<T, TResult>(this ISetup<T, TResult> setup, params object[] results) where T : class
        {
            var queue = new Queue(results);
            setup.Returns(() =>
            {
                var result = queue.Dequeue();
                if (result is Exception)
                {
                    throw result as Exception;
                }
                return (TResult)result;
            });
        }
    }
}
