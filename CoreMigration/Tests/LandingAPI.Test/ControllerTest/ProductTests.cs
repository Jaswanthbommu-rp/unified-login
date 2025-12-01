using Moq;
using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.LandingAPI.Test.Extensions;
using UnifiedLogin.LandingAPI.Test.Logic;
using UnifiedLogin.LandingAPI;
using UnifiedLogin.LandingAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;

using Xunit;
using Xunit.Abstractions;

namespace UnifiedLogin.LandingAPI.Test.ControllerTest
{
	/// <summary>
	/// Product xUnit tests
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class ProductTests : TestBase
	{
        private readonly ITestOutputHelper _output;
        private static Guid _realPageId = new Guid("C802694D-5553-4527-8616-3C0F434AE62D");
		private readonly List<ProductInternalSetting> _product5InternalSettings;
		private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
		private readonly Mock<HttpMessageHandler> _mockHttpMessageHandlerError;

        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<IRepository> _mock2Repository;
        private static DefaultUserClaim _userClaim;
        private static List<AdGroupProduct> AdGroupsProduct;
        private static List<OrganizationType> organizationTypeList;
        private static List<Organization> organizationList;
        private static List<OrganizationDomain> organizationDomainList;
        private static List<Persona> personaList;
        private static List<AdGroup> ADGroupsList;

        public ProductTests(ITestOutputHelper output)
        {
            _output = output;
			_product5InternalSettings = new List<ProductInternalSetting>()
			{
				new ProductInternalSetting() {Name = "BooksUseDomains", Value = "1"},
				new ProductInternalSetting() {Name = "BooksUseUPFMId", Value = "1"}
			};

			var booksSourceListJson = "{\"data\":[{\"type\":\"source\",\"id\":\"SB\",\"attributes\":{\"source\":\"SB\",\"description\":\"SimpleBills\",\"AKA\":null,\"isActive\":true,\"priority\":99,\"isMigratable\":true,\"propertyInstanceKnownId\":true,\"companyInstanceKnownId\":true,\"propertyInstanceKnownIdDomain\":true,\"companyInstanceKnownIdDomain\":true,\"allowEnablement\":false,\"allowCancellation\":false,\"cancellationCompanyLevel\":false,\"dashboardCares\":false},\"links\":{\"self\":\"/source/SB\"}},{\"type\":\"source\",\"id\":\"INDATUS\",\"attributes\":{\"source\":\"INDATUS\",\"description\":\"Indatus\",\"AKA\":null,\"isActive\":true,\"priority\":29,\"isMigratable\":false,\"propertyInstanceKnownId\":true,\"companyInstanceKnownId\":true,\"propertyInstanceKnownIdDomain\":true,\"companyInstanceKnownIdDomain\":true,\"allowEnablement\":false,\"allowCancellation\":false,\"cancellationCompanyLevel\":false,\"dashboardCares\":false},\"links\":{\"self\":\"/source/INDATUS\"}},{\"type\":\"source\",\"id\":\"PW\",\"attributes\":{\"source\":\"PW\",\"description\":\"PropertyWare\",\"AKA\":null,\"isActive\":true,\"priority\":40,\"isMigratable\":true,\"propertyInstanceKnownId\":true,\"companyInstanceKnownId\":true,\"propertyInstanceKnownIdDomain\":true,\"companyInstanceKnownIdDomain\":true,\"allowEnablement\":false,\"allowCancellation\":false,\"cancellationCompanyLevel\":false,\"dashboardCares\":false},\"links\":{\"self\":\"/source/PW\"}}]}";
			HttpResponseMessage booksSourceListResponse = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new StringContent(booksSourceListJson)
			};
			HttpResponseMessage booksSourceListErrorResponse = new HttpResponseMessage(HttpStatusCode.NotFound);

			_mockHttpMessageHandler = new Mock<HttpMessageHandler>();
			_mockHttpMessageHandlerError = new Mock<HttpMessageHandler>();

			// use product id not normally used
            mockRepository
				.Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, 
                    It.Is<object>(d => TestProductIdTrue(d, 5))))
				.Returns(_product5InternalSettings);

			_mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/source", booksSourceListResponse);
			_mockHttpMessageHandlerError.Setup(HttpMethod.Get, $"http://localhost/source", booksSourceListErrorResponse);

            _mock2Repository = new Mock<IRepository>();
            _mockProductRepository = new Mock<IProductRepository>();

        }

        #region Controller Unit Tests
        [Fact]
		public void GetProductFamilies_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
            var config = new HttpClient();

			//Act
			WebApiConfig.Register(config);
			config.EnsureInitialized();
			var controllerSelector = new DefaultHttpControllerSelector(config);
			var baseTest = new RouteTestBase(config, controllerSelector);

            var result = baseTest.VerifyRouteToAction(
                HttpMethod.Get,
                "http://localhost/productfamilies");

            _output.WriteLine($"result : {result}");
            //Assert
            Assert.True("GetProductFamilies" == result);
		}

		[Fact]
		public void GetProductFamiliesmockRepository_ReturnValidRepositoryResponseObject()
		{
			//Arrange
			Guid userRealPageId = Guid.Empty;
			Guid editorRealpageUserId = Guid.Empty;

			Guid organizationRealPageId = Guid.Empty;
			ObjectListOutput<ProductFamily, IErrorData> productFamilyListOutput = new ObjectListOutput<ProductFamily, IErrorData>();
			Type type = typeof(ProductFamily);

			IList<ProductFamily> expectedProductFamilyList = new List<ProductFamily>();
			IList<Solution> solutionList1 = new List<Solution>();

			solutionList1.Add(new Solution()
			{
				FamilyId = 100,
				IsAssigned = false,
				ProductId = 1,
				ProductName = "OneSite",
				SubSolution = "OneSite Leasing & Rents, Facilities, Purchasing, Doc. Mgmt",
				ShowInUserDetails = true
			});
			solutionList1.Add(new Solution()
			{
				FamilyId = 100,
				IsAssigned = false,
				ProductId = 8,
				ProductName = "RealPage Accounting",
				SubSolution = "Property, Job Cost, Corporate"
			});
			solutionList1.Add(new Solution()
			{
				FamilyId = 100,
				IsAssigned = false,
				ProductId = 13,
				ProductName = "Spend Management",
				SubSolution = null,
				ShowInUserDetails = true
			});
			solutionList1.Add(new Solution()
			{
				FamilyId = 100,
				IsAssigned = false,
				ProductId = 16,
				ProductName = "Vendor Credentialing",
				SubSolution = "Vendor Compliance"
			});

			expectedProductFamilyList.Add(new ProductFamily()
			{
				ProductTypeId = 100,
				Name = "Property Management",
				Description = "Property Management",
				Solutions = solutionList1.ToList()
			});

			var productSettingList = new List<ProductSettingList>() {
				new ProductSettingList() { ProductId = 13, Name = "ProductStatus", Value = "8" },
				new ProductSettingList() { ProductId = 1, Name = "ProductStatus", Value = "8" },
				new ProductSettingList() { ProductId = 1, Name = "ShowInUserDetails", Value = "1" }
			};

			List<ProductInternalSetting> productInternalSettings = new List<ProductInternalSetting>()
			{
				new ProductInternalSetting() {Name = "BooksUseDomains", Value = "1"},
				new ProductInternalSetting() {Name = "BooksUseUPFMId", Value = "1"},
				new ProductInternalSetting() {Name = "ShowInUserDetails", Value = "1"},
			};

			mockRepository
				.Setup(m => m.GetMany<ProductFamily>(StoredProcNameConstants.SP_ListProductFamilies, null))
				.Returns(expectedProductFamilyList);

			mockRepository
				.Setup(m => m.GetMany<Solution>(StoredProcNameConstants.SP_ListProductsByOrganization, It.IsAny<object>()))
				.Returns(solutionList1);

			mockRepository
				.Setup(m => m.GetMany<ProductSettingList>(StoredProcNameConstants.SP_ListProductSettingsByPersonaId, It.IsAny<object>()))
				.Returns(productSettingList);

			mockRepository
				.Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
				.Returns(productInternalSettings);

			DefaultUserClaim userClaim = new DefaultUserClaim()
			{
				PersonaId = 1234,
				OrganizationRealPageGuid = new Guid(),
				UserRealPageGuid = new Guid(),
				Rights = new List<string>()
			};

			var productRepository = new ProductRepository(mockRepository.Object, userClaim);

			ProductController controller = new ProductController(userClaim, mockRepository.Object, productRepository, _mockHttpMessageHandler.Object);
			controller.Request = new HttpRequestMessage();
			controller.Configuration = new HttpClient();

			//Act
			int NumberOfProperties = type.GetProperties().Length;

			new RPObjectCache().BustCache();

			HttpResponseMessage response = controller.GetProductFamilies(userRealPageId, null);
			productFamilyListOutput = response.Content.ReadAsAsync<ObjectListOutput<ProductFamily, IErrorData>>().Result;

			//Assert
			Assert.True(response.IsSuccessStatusCode);
			Assert.Equal("GET", response.RequestMessage.Method.ToString());
			Assert.Equal(2, productFamilyListOutput.list[0].Solutions.Count);

			//Act
			new RPObjectCache().BustCache();
			response = controller.GetProductFamilies(userRealPageId, "userdetails");
			productFamilyListOutput = response.Content.ReadAsAsync<ObjectListOutput<ProductFamily, IErrorData>>().Result;

			//Assert
			Assert.True(response.IsSuccessStatusCode);
			Assert.Equal("GET", response.RequestMessage.Method.ToString());
			Assert.Equal(2, productFamilyListOutput.list[0].Solutions.Count);

		}

		[Fact]
		public void ListProductUsers_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpClient Config = new HttpClient();

			//Act
			WebApiConfig.Register(Config);
			Config.EnsureInitialized();
			var ControllerSelector = new DefaultHttpControllerSelector(Config);
			var baseTest = new RouteTestBase(Config, ControllerSelector);

			var result = baseTest.VerifyRouteToAction(
					HttpMethod.Get,
					"http://localhost/products/24/organization/-1");

            _output.WriteLine($"result : {result}");
			//Assert
			Assert.True("ListProductUsers" == result);
		}

		[Fact]
		public void ListProductUsers_InvalidProductId_ExceptionThrown()
		{
			//Arrange
			int productId = 0;
			long blueBookCompanyInstanceId = -1;
			long personaId = 0;
			ProductController productController = new ProductController();

			//Act
			Exception exception = Record.Exception(() => productController.ListProductUsers(productId, blueBookCompanyInstanceId, personaId));

			//Assert
			Assert.IsType<ArgumentNullException>(exception);
		}

		[Fact]
		public void ListProductUsers_InvalidBlueBookCompanyInstanceId_ExceptionThrown()
		{
			//Arrange
			int productId = (int)ProductEnum.ResearchApplication;
			long blueBookCompanyInstanceId = 0;
			long personaId = 0;
			ProductController productController = new ProductController();

			//Act
			Exception exception = Record.Exception(() => productController.ListProductUsers(productId, blueBookCompanyInstanceId, personaId));

			//Assert
			Assert.IsType<ArgumentNullException>(exception);
		}

		[Fact]
		public void ListProductUsers_InvalidPersonaId_ExceptionThrown()
		{
			//Arrange
			int productId = (int)ProductEnum.ResearchApplication;
			long blueBookCompanyInstanceId = -1;
			long personaId = -1;
			ProductController productController = new ProductController();

			//Act
			Exception exception = Record.Exception(() => productController.ListProductUsers(productId, blueBookCompanyInstanceId, personaId));

			//Assert
			Assert.IsType<ArgumentNullException>(exception);
		}

		[Fact]
		public void GetProductNonSensitiveSettings_OK()
		{
			DefaultUserClaim userClaim = new DefaultUserClaim() { PersonaId = 1234, OrganizationRealPageGuid = new Guid(), UserRealPageGuid = new Guid() };

			new RPObjectCache().BustCache();

			ProductController controller = new ProductController(userClaim, mockRepository.Object, null, _mockHttpMessageHandler.Object)
			{
				Request = new HttpRequestMessage(),
				Configuration = new HttpClient()
			};

			//Act
			var result = controller.GetProductNonSensitiveSettings(5);

			//Assert
			Assert.True(result.Count == _product5InternalSettings.Count);
		}

		[Fact]
		public void GetProductSettingByType_OK()
		{
			IList<ProductInternalSettingByType> settings = new List<ProductInternalSettingByType>()
			{
				new ProductInternalSettingByType
				{
					ProductConfigurationId = "1234",
					Name = "ProductStatus",
					Value = "8",
					ProductId = 1,
					ProductName = "OneSite",
					BooksProductCode = "OS"
				},
				new ProductInternalSettingByType
				{
				ProductConfigurationId = "1235",
				Name = "ProductStatus",
				Value = "8",
				ProductId = 37,
				ProductName = "Property Photos",
				BooksProductCode = "PHOTO"
				},
				new ProductInternalSettingByType
				{
					ProductConfigurationId = "5555",
					Name = "ApiSecret",
					Value = "somesecret",
					ProductId = 1,
					ProductName = "OneSite",
					BooksProductCode = "OS",
					SensitiveData = true
				},
				new ProductInternalSettingByType
				{
					ProductConfigurationId = "6666",
					Name = "ApiSecret",
					Value = "some password",
					ProductId = 37,
					ProductName = "Property Photos",
					BooksProductCode = "PHOTO",
					SensitiveData = true
				}
			};

			IList<ProductSettingType> productSettingTypes = new List<ProductSettingType>()
			{
				new ProductSettingType()
				{
					ProductSettingTypeId = 1, Name = "ProductStatus", SensitiveData = false
				},
				new ProductSettingType()
				{
					ProductSettingTypeId = 2, Name = "ApiSecret", SensitiveData = true
				}
			};

			mockRepository
				.Setup(m => m.GetMany<ProductSettingType>(StoredProcNameConstants.SP_ListProductSettingType, null))
				.Returns(() => productSettingTypes);

			mockRepository
				.Setup(m => m.GetMany<ProductInternalSettingByType>(StoredProcNameConstants.SP_ListProductGlobalSettingsBySettingType, It.Is<object>(l => TestProductSettingType(l, "ProductStatus"))))
				.Returns(settings.Where(p => p.Name == "ProductStatus").ToList());

			mockRepository
				.Setup(m => m.GetMany<ProductInternalSettingByType>(StoredProcNameConstants.SP_ListProductGlobalSettingsBySettingType, It.Is<object>(l => TestProductSettingType(l, "ApiSecret"))))
				.Returns(settings.Where(p => p.Name == "ApiSecret").ToList());

			DefaultUserClaim userClaim = new DefaultUserClaim() { PersonaId = 1234, OrganizationRealPageGuid = new Guid(), UserRealPageGuid = new Guid() };
			ProductRepository productRepository = new ProductRepository(mockRepository.Object, userClaim);

			new RPObjectCache().BustCache();

			ProductController controller = new ProductController(userClaim, mockRepository.Object, productRepository, _mockHttpMessageHandler.Object);
			controller.Request = new HttpRequestMessage();
			controller.Configuration = new HttpClient();

			//Act
			var response = controller.GetAllProductNonSensitiveSettingsByType("ProductStatus");
			var responseResult = response.Content.ReadAsAsync<ObjectListOutput<ProductInternalSettingByType, IErrorData>>().Result;

			//Assert
			Assert.True(responseResult.list.Count == settings.Where(l => l.Name.Equals("ProductStatus", StringComparison.OrdinalIgnoreCase)).ToList().Count);

			//Act
			response = controller.GetAllProductNonSensitiveSettingsByType("ApiSecret");
			responseResult = response.Content.ReadAsAsync<ObjectListOutput<ProductInternalSettingByType, IErrorData>>().Result;

			//Assert
			Assert.True(responseResult.list.Count == 0);

			//Act
			response = controller.GetAllProductNonSensitiveSettingsByType("UnknownSetting");
			responseResult = response.Content.ReadAsAsync<ObjectListOutput<ProductInternalSettingByType, IErrorData>>().Result;

			//Assert
			Assert.True(responseResult.list.Count == 0);
		}

		[Fact]
		public void ListProductSettingType_OK()
		{
			IList<ProductSettingType> productSettingTypes = new List<ProductSettingType>()
			{
				new ProductSettingType()
				{
					ProductSettingTypeId = 1, Name = "ProductStatus"
				}
			};

			mockRepository
				.Setup(m => m.GetMany<ProductSettingType>(StoredProcNameConstants.SP_ListProductSettingType, null))
				.Returns(() => productSettingTypes);

			DefaultUserClaim userClaim = new DefaultUserClaim() { PersonaId = 1234, OrganizationRealPageGuid = new Guid(), UserRealPageGuid = new Guid() };
			var productRepository = new ProductRepository(mockRepository.Object, userClaim);

			new RPObjectCache().BustCache();

			ProductController controller = new ProductController(userClaim, mockRepository.Object, productRepository, _mockHttpMessageHandler.Object);
			controller.Request = new HttpRequestMessage();
			controller.Configuration = new HttpClient();

			//Act
			var response = controller.ListProductSettingType();

			//Assert
			Assert.True(response.Count == productSettingTypes.Count);
		}

		[Fact]
		public void GetUDMSourceList_OK()
		{
			var mockProductRepository = new Mock<IProductRepository>();
			DefaultUserClaim userClaim = new DefaultUserClaim() { PersonaId = 1234, OrganizationRealPageGuid = new Guid(), UserRealPageGuid = new Guid() };

			new RPObjectCache().BustCache();

			ProductController controller = new ProductController(userClaim, mockRepository.Object, mockProductRepository.Object, _mockHttpMessageHandler.Object)
			{
				Request = new HttpRequestMessage(),
				Configuration = new HttpClient()
			};

			//Act
			var response = controller.GetUDMSourceList();

			//Assert
			Assert.True(response.Count() == 3);

			// check sorting
			Assert.True(response.ToArray()[1].Id == "PW");
		}

		[Fact]
		public void GetUDMSourceList_Error()
		{
			var mockProductRepository = new Mock<IProductRepository>();
			DefaultUserClaim userClaim = new DefaultUserClaim() { PersonaId = 1234, OrganizationRealPageGuid = new Guid(), UserRealPageGuid = new Guid() };
			new RPObjectCache().BustCache();

			ProductController controller = new ProductController(userClaim, mockRepository.Object, mockProductRepository.Object, _mockHttpMessageHandlerError.Object)
			{
				Request = new HttpRequestMessage(),
				Configuration = new HttpClient()
			};

			//Act
			var response = controller.GetUDMSourceList();

			//Assert
			Assert.True(!response.Any());

		}
		#endregion

		private bool TestProductIdTrue(object obj, int value)
		{
			return obj.ToString().Equals($"{{ ProductId = {value} }}");
		}

		private bool TestProductSettingType(object obj, string value)
		{
			return obj.ToString().Equals($"{{ ProductSettingType = {value} }}");
		}

        private bool TestRealPageIdTrue(object obj, string value)
        {
            return obj.ToString().Equals($"{{ RealPageId = {value} }}");
        }
        private bool TestUserIdTrue(object obj, long value)
        {
            return obj.ToString().Equals($"{{ UserId = {value} }}");
        }

        private bool TestSamlProductIdTrue(object obj, int value)
        {
            return obj.ToString().Equals($"{{ productId = {value} }}");
        }

        private bool TestOrganizationTrue(object obj, Guid RealPageId, string RelationshipTypeName)
        {
            return obj.ToString().Equals($"{{ RealPageId = {RealPageId}, RelationshipTypeName = {RelationshipTypeName} }}");
        }

        private bool TestPersonaIdTrue(object obj, long value)
        {
            return obj.ToString().Equals($"{{ PersonaId = {value} }}");
        }

        internal void FillInstanceData(int ProductId, long PersonaId, long UserId, string TestCaseName)
        {
            _userClaim = new DefaultUserClaim()
            {
                UserId = 27564,
                CorrelationId = new Guid("8fb8808d-1064-408d-a016-ea07bd7e44e4"),
                UserRealPageGuid = new Guid("452caa23-7e24-4d48-b042-ac050b05aa2c"),
                LoginName = "379admin@realpage.com",
                OrganizationRealPageGuid = new Guid("f5c090fa-78ab-452f-b504-98aafee09121"),
                OrganizationPartyId = 350,
                OrganizationName = "CF Real Estate Services",
                OrganizationType = "Multifamily",
                OrganizationMasterId = 2116,
                CustomerMasterId = 379,
                Roles = null,
                Rights = new List<string>() { "AbilitytoanswercompanylevelquestionnairesinCIMPL", "Abilitytoemailreportsandreportgroups", "AccessToUnifiedPlatform", "AccessUnifiedReporting", "CIMPLESubmitQuestionnaires", "EditOwnProfile", "EmployeeImplementRecordsCIMPL", "EmployeeViewCIMPLQuestions", "InternalAdminaccessToUnifiedSettings", "LogInAsMyself", "ManageCIMPLTemplates", "ManageCompanyLevelReporting", "Managecompanylevelsettings", "ManagePropertyLevelReporting", "Managepropertylevelsettings", "ManageRoleRight", "ManageSettingsTemplates", "PrimaryProperty","EnterpriseRole", "ViewCIMPLQuestions", "ViewRoleRight", "ViewUnifiedSettings", "ViewUsers" },
                FirstName = "RealPage",
                LastName = "Access",
                ClientCode = "rplandingapi",
                PersonaId = PersonaId,
                RealPageEmployee = false,
                ImpersonatedBy = new Guid("7afe9118-e074-ee11-a9d9-005056b070c0"),
                ImpersonatedByName = "Venkata Koteswara Rao Govindu",
                IsRPEmployee = false,
            };

            IList<ProductInternalSetting> productInternalSettingsList = new List<ProductInternalSetting>();
            productInternalSettingsList = new List<ProductInternalSetting>()
            {
                new ProductInternalSetting {ProductConfigurationId="1187382",ConfigurationId="81",Name="AOSpecialEditorUser",Value="ulserviceuser@realpage.com",SensitiveData=false},
                new ProductInternalSetting {ProductConfigurationId="1187382",ConfigurationId="81",Name="AOSpecialEditorUser",Value="ulserviceuser@realpage.com",SensitiveData=false},
                new ProductInternalSetting {ProductConfigurationId="630507",ConfigurationId="81",Name="ApiEndPoint",Value="https://aoqa.realpage.com/ysconfig/ws/",SensitiveData=false},
                new ProductInternalSetting {ProductConfigurationId="685326",ConfigurationId="268202",Name="AuthenticationType",Value="Redirect",SensitiveData=false}
            };

            if (TestCaseName == "AccessDenied")
            {
                productInternalSettingsList.Add(new ProductInternalSetting { ProductConfigurationId = "1426083", ConfigurationId = "81", Name = "CheckADGroupProductAccess", Value = "1", SensitiveData = false });
                productInternalSettingsList.Add(new ProductInternalSetting { ProductConfigurationId = "936350", ConfigurationId = "81", Name = "CheckADGroupProductAccessGroupNames", Value = "UP_Asset_Optimization_Product_Access", SensitiveData = false });
            }

            string RealPageId = "452caa23-7e24-4d48-b042-ac050b05aa2c";
            UserLoginOnly userLoginOnly = new UserLoginOnly()
            {
                PartyId = 28833,
                RealPageId = new Guid("452caa23-7e24-4d48-b042-ac050b05aa2c"),
                PersonaId = 0,
                LoginNameType = null,
                PasswordHash = "",
                PasswordSalt = "",
                PasswordModifiedDate = Convert.ToDateTime("2065-12-30 00:00:00"),
                LastLogin = Convert.ToDateTime("2022-04-04 10:31:26"),
                Password = null,
                Is3rdPartyIDP = true,
                UserId = UserId,
                LoginName = "379admin@realpage.com"
            };

            List<OrganizationStatus> orgStatus = new List<OrganizationStatus>() {
                new OrganizationStatus {
                    PartyId = 350,
                    RealPageId = new Guid("f5c090fa-78ab-452f-b504-98aafee09121"),
                    Name = "CF Real Estate Services",
                    PrimaryOrganization = true,
                    StatusTypeId = 1,
                    IsPending = false,
                    IsExpired = false,
                    IsActive = false,
                    IsLocked = false,
                    IsForceReSetPassword = false,
                    IsTainted = false,
                    Status = 0,
                    FromDate = Convert.ToDateTime("2019-03-15 13:40:38"),
                    ThruDate = null,
                    StatusThruDate = null } };

            var productSamlSettings = new ProductSamlSettings()
            {
                LoginUri = "https://qa-mc.realpage.com/mcauth/sso/oauth?productCode=PP",
                ProductId = ProductId,
                ProductSamlSettingsId = 0,
                SigningCertificateThumbprint = "3805355236F00E690E130A5DC14863B71D655DF4",
                SubjectIdSamlAttribute = "productUsername",
            };

            var _gbProductMap = new List<GbProductMap>
            {
                new GbProductMap() { BooksProductCode = "OS", Name = "OneSite", ProductId = 1, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "UI", Name = "UnifiedUI", ProductId = 2, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "UPFM", Name = "Unified Platform", ProductId = 3, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "AO", Name = "Asset Optimization", ProductId = 4, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "PHOTO", Name = "PropertyPhotos", ProductId = 37, UDMSourceCode = null }
            };

            organizationTypeList = new List<OrganizationType>()
            {
                new OrganizationType() { CreateDate = Convert.ToDateTime("0001-01-01 00:00:00"), Name="AppPartner", OrganizationTypeId = 1 },
                new OrganizationType() { CreateDate = Convert.ToDateTime("0001-01-01 00:00:00"), Name="Multifamily", OrganizationTypeId = 6 },
                new OrganizationType() { CreateDate = Convert.ToDateTime("0001-01-01 00:00:00"), Name="Other", OrganizationTypeId = 7 },
                new OrganizationType() { CreateDate = Convert.ToDateTime("0001-01-01 00:00:00"), Name="Supplier", OrganizationTypeId = 11 },
                new OrganizationType() { CreateDate = Convert.ToDateTime("0001-01-01 00:00:00"), Name="Test Data", OrganizationTypeId = 12 },
                new OrganizationType() { CreateDate = Convert.ToDateTime("0001-01-01 00:00:00"), Name="Vendor", OrganizationTypeId = 14 },
            };

            organizationList = new List<Organization>() {
                new Organization()
                {
                      BooksCustomerMasterId =  -1,
                      BooksMasterId =  -1,
                      CreateDate = Convert.ToDateTime("2/9/2018 9:07:21 PM"),
                      EnablePrimaryProperties = 0 ,
					  EnableEnterpriseRoles = 0 ,
                      IsActive = 1,
                      Name = "RealPage Employee",
                      OrganizationDomain = null,
                      OrganizationDomainId = 1,
                      OrganizationTypeId = 7,
                      PartyId = 6529,
                      PrimaryOrganization = true,
                      RealPageId = new Guid("0d018e46-c20e-477d-aded-4e5a35fb8f99"),
                      RelationshipType = "User Type",
                      RoleNameFrom = "RealPage Employee",
                      RoleNameTo = "User Type",
                      organizationType = null,
                      partyRelationship = null,
                }
            };

            AdGroupsProduct = new List<AdGroupProduct>()
            {
               new AdGroupProduct() { AssignmentOrder = 1, ADGroupId = 7, CreatedDate = Convert.ToDateTime("0001-01-01 00:00:00"), ADGroupName="AGAa-UP_Asset_Optimization_Product_Access", ActiveDirectoryId=new Guid("c403db5e-39d6-4018-adf5-431c531b10ad")  },
               new AdGroupProduct() { AssignmentOrder = 2, ADGroupId = 35, CreatedDate = Convert.ToDateTime("0001-01-01 00:00:00"), ADGroupName="AGAa-UP_Non-Prod_All_Products_Access", ActiveDirectoryId=new Guid("de94d94c-4bda-49c3-8a36-c2563b440b2d")  }
            };

            organizationDomainList = new List<OrganizationDomain>() {
                new OrganizationDomain() { CreateDate = Convert.ToDateTime("1/1/0001 12:00:00 AM"), Name = "AutoCE100267530", OrganizationDomainId = 8 },
                new OrganizationDomain() { CreateDate = Convert.ToDateTime("1/1/0001 12:00:00 AM"), Name = "Historical", OrganizationDomainId = 4 },
                new OrganizationDomain() { CreateDate = Convert.ToDateTime("1/1/0001 12:00:00 AM"), Name = "Primary", OrganizationDomainId = 1 },
                new OrganizationDomain() { CreateDate = Convert.ToDateTime("1/1/0001 12:00:00 AM"), Name = "Primary 1", OrganizationDomainId = 6 },
                new OrganizationDomain() { CreateDate = Convert.ToDateTime("1/1/0001 12:00:00 AM"), Name = "Primary 2", OrganizationDomainId = 7 },
                new OrganizationDomain() { CreateDate = Convert.ToDateTime("1/1/0001 12:00:00 AM"), Name = "Primary 3", OrganizationDomainId = 14 },
                new OrganizationDomain() { CreateDate = Convert.ToDateTime("1/1/0001 12:00:00 AM"), Name = "Primary 4", OrganizationDomainId = 13 },
                new OrganizationDomain() { CreateDate = Convert.ToDateTime("1/1/0001 12:00:00 AM"), Name = "Primary A", OrganizationDomainId = 9 },
                new OrganizationDomain() { CreateDate = Convert.ToDateTime("1/1/0001 12:00:00 AM"), Name = "Primary1", OrganizationDomainId = 5 },
                new OrganizationDomain() { CreateDate = Convert.ToDateTime("1/1/0001 12:00:00 AM"), Name = "UAT", OrganizationDomainId = 2 },
                new OrganizationDomain() { CreateDate = Convert.ToDateTime("1/1/0001 12:00:00 AM"), Name = "UAT 1", OrganizationDomainId = 10 },
                new OrganizationDomain() { CreateDate = Convert.ToDateTime("1/1/0001 12:00:00 AM"), Name = "UAT 2", OrganizationDomainId = 11 },
                new OrganizationDomain() { CreateDate = Convert.ToDateTime("1/1/0001 12:00:00 AM"), Name = "UAT 3", OrganizationDomainId = 12 },
            };

            personaList = new List<Persona>()
            {
              new Persona()
              {
                    FromDate = Convert.ToDateTime("10/10/2023 3:47:13 PM"),
                    IsDefault = false,
                    Name = "Primary",
                    Organization = null,
                    OrganizationPartyId = 6529,
                    PersonPartyId = 161654,
                    PersonaEnvironmentTypeId = 1,
                    PersonaId = 159742,
                    PersonaName = "Primary",
                    PersonaTypeId = 3,
                    RealPageId = new Guid("7afe9118-e074-ee11-a9d9-005056b070c0"),
                    ThruDate = null,
                    UserId = 153325
              }
            };

            ADGroupsList = new List<AdGroup>()
            {
              new AdGroup() { ADGroupId = 1, ADGroupName = null, ActiveDirectoryId = new Guid("00000000-0000-0000-0000-000000000000"), CreatedDate = Convert.ToDateTime("11/11/2023 10:00:20 AM") }
            };

            var UnifiedPlatform = (int)ProductEnum.UnifiedPlatform;
            var ImpersonatedBy = "7afe9118-e074-ee11-a9d9-005056b070c0";

            _mock2Repository.Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.Is<object>(d => TestProductIdTrue(d, ProductId))))
                .Returns(productInternalSettingsList);

            _mock2Repository
                .Setup(m => m.GetOne<UserLoginOnly>(StoredProcNameConstants.SP_GetUserLoginOnly, It.Is<object>(d => TestRealPageIdTrue(d, RealPageId))))
                .Returns(userLoginOnly);

            _mock2Repository
                .Setup(m => m.GetMany<OrganizationStatus>(StoredProcNameConstants.SP_ListOrganizationStatusByUserId, It.Is<object>(d => TestUserIdTrue(d, UserId))))
                .Returns(orgStatus);

            _mock2Repository
                .Setup(m => m.GetOne<ProductSamlSettings>(StoredProcNameConstants.SP_GetProductSamlSettings, It.Is<object>(d => TestSamlProductIdTrue(d, ProductId))))
                .Returns(productSamlSettings);

            _mock2Repository
            .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.Is<object>(d => TestProductIdTrue(d, UnifiedPlatform))))
                .Returns(_product5InternalSettings);

            _mock2Repository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct, It.IsAny<object>()))
                .Returns(_gbProductMap);

            _mock2Repository
               .Setup(m => m.GetOne<UserLoginOnly>(StoredProcNameConstants.SP_GetUserLoginOnly, It.Is<object>(d => TestRealPageIdTrue(d, ImpersonatedBy))))
               .Returns(userLoginOnly);

            _mock2Repository
            .Setup(m => m.ExecuteNonQuery(StoredProcNameConstants.SP_InsertProductLoginActivitybyUser, It.IsAny<object>()))
                .Returns(1);

        }

        [Fact]
        public void GetProductLoginDetails_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            var config = new HttpClient();

            //Act
            WebApiConfig.Register(config);
            config.EnsureInitialized();
            var controllerSelector = new DefaultHttpControllerSelector(config);
            var baseTest = new RouteTestBase(config, controllerSelector);

            var result = baseTest.VerifyRouteToAction(HttpMethod.Get, "http://localhost/product/37/persona/28793");

            _output.WriteLine($"result : {result}");
            //Assert
            Assert.True("GetProductLoginDetails" == result);
        }

        [Fact]
        public void GetProductLoginDetails_Success()
        {
            int ProductId = 37; long PersonaId = 28793; long UserId = 27564;

            FillInstanceData(ProductId, PersonaId, UserId, "");

            new RPObjectCache().BustCache();

            ProductController controller = new ProductController(_userClaim, _mock2Repository.Object, _mockProductRepository.Object, _mockHttpMessageHandler.Object);
            controller.Request = new HttpRequestMessage();
            controller.Configuration = new HttpClient();

            //Act
            var response = controller.GetProductLoginDetails(ProductId, PersonaId);

            //Assert
            var Expected_RedirectUrl = "https://qa-mc.realpage.com/mcauth/sso/oauth?productCode=PP";
            bool Expected_IsRedirect = true;
            Assert.True((response.IsRedirect == Expected_IsRedirect) && response.RedirectUrl == Expected_RedirectUrl);

        }

        [Fact]
        public void GetProductLoginDetails_AccessDenied()
        {
            int ProductId = 4; long PersonaId = 28793; long UserId = 27564;
            FillInstanceData(ProductId, PersonaId, UserId, "AccessDenied");

            _mock2Repository
                .Setup(m => m.GetMany<AdGroupProduct>(StoredProcNameConstants.SP_GetADGroupsForProduct, It.Is<object>(d => TestProductIdTrue(d, ProductId))))
                .Returns(AdGroupsProduct);

            _mock2Repository
                .Setup(m => m.GetMany<OrganizationType>(StoredProcNameConstants.SP_ListOrganizationType, It.IsAny<object>()))
                .Returns(organizationTypeList);

            _mock2Repository
                .Setup(m => m.GetMany<Organization>(StoredProcNameConstants.SP_ListOrganizationByRealPageId, It.Is<object>(d => TestOrganizationTrue(d, _userClaim.ImpersonatedBy, ""))))
                .Returns(organizationList);

            _mock2Repository
                .Setup(m => m.GetMany<OrganizationDomain>(StoredProcNameConstants.SP_ListOrganizationDomain, It.IsAny<object>()))
                .Returns(organizationDomainList);

            _mock2Repository
               .Setup(m => m.GetMany<Persona>(StoredProcNameConstants.SP_ListPersona, It.Is<object>(d => TestRealPageIdTrue(d, _userClaim.ImpersonatedBy.ToString()))))
               .Returns(personaList);

            _mock2Repository
              .Setup(m => m.GetMany<AdGroup>(StoredProcNameConstants.SP_GetADGroupsForUser, It.Is<object>(d => TestPersonaIdTrue(d, personaList[0].PersonaId))))
              .Returns(ADGroupsList);

            new RPObjectCache().BustCache();

            ProductController controller = new ProductController(_userClaim, _mock2Repository.Object, _mockProductRepository.Object, _mockHttpMessageHandler.Object);
            controller.Request = new HttpRequestMessage();
            controller.Configuration = new HttpClient();

            //Act
            var response = controller.GetProductLoginDetails(ProductId, PersonaId);

            //Assert
            var Expected_errorMessage = "AccessDenied";
            bool Expected_IsRedirect = false;
            Assert.True((response.IsRedirect == Expected_IsRedirect) && response.ErrorMessage == Expected_errorMessage);

        }

    }
}
