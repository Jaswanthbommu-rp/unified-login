using Moq;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Extensions;
using RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI;
using RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Xunit;
using Xunit.Abstractions;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.ControllerTest
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
		}

		#region Controller Unit Tests
		[Fact]
		public void GetProductFamilies_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
            var config = new HttpConfiguration();

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
			controller.Configuration = new HttpConfiguration();

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
			HttpConfiguration Config = new HttpConfiguration();

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
				Configuration = new HttpConfiguration()
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
			controller.Configuration = new HttpConfiguration();

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
			controller.Configuration = new HttpConfiguration();

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
				Configuration = new HttpConfiguration()
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
				Configuration = new HttpConfiguration()
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
	}
}
