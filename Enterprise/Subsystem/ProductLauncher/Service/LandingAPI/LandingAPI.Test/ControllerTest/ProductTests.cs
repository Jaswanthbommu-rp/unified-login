using Moq;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI;
using RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using Xunit;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.ControllerTest
{
	/// <summary>
    /// Product xUnit tests
    /// </summary>
    [ExcludeFromCodeCoverage]
	public class ProductTests
	{
		private static Guid _realPageId = new Guid("C802694D-5553-4527-8616-3C0F434AE62D");

		#region Controller Unit Tests
		[Fact]
		public void GetProductFamilies_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpConfiguration Config = new HttpConfiguration();

			//Act
            WebApiConfig.Register(Config);
			Config.EnsureInitialized();
			DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
			RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

			//Assert
			Assert.True("GetProductFamilies" == baseTest.VerifyRouteToAction(
				HttpMethod.Get,
				"http://localhost/api/productfamilies"
				)
			);
		}

		[Fact]
		public void GetProductFamilies_MockRepository_ReturnValidRepositoryResponseObject()
		{
			//Arrange
			Guid userRealPageId = Guid.Empty;
            Guid editorRealpageUserId = Guid.Empty;

            Guid organizationRealPageId = Guid.Empty;
			ObjectListOutput<ProductFamily, IErrorData> productFamilyListOutput = new ObjectListOutput<ProductFamily, IErrorData>();
			Type type = typeof(ProductFamily);

			IList<ProductFamily> expectedProductFamilyList = new List<ProductFamily>();
			IList<Solution> solutionList = new List<Solution>();

			solutionList.Add(new Solution()
			{
				FamilyId = 100,
				IsAssigned = false,
				ProductId = 1,
				ProductName = "OneSite",
				SubSolution = "OneSite Leasing & Rents, Facilities, Purchasing, Doc. Mgmt"
			});
			solutionList.Add(new Solution()
			{
				FamilyId = 100,
				IsAssigned = false,
				ProductId = 8,
				ProductName = "RealPage Accounting",
				SubSolution = "Property, Job Cost, Corporate"
			});
			solutionList.Add(new Solution()
			{
				FamilyId = 100,
				IsAssigned = false,
				ProductId = 13,
				ProductName = "Spend Management",
				SubSolution = null
			});
			solutionList.Add(new Solution()
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
				Solutions = solutionList
			});

			solutionList = new List<Solution>();
			solutionList.Add(new Solution()
			{
				FamilyId = 200,
				IsAssigned = false,
				ProductId = 15,
				ProductName = "Renters Insurance",
				SubSolution = null
			});
			solutionList.Add(new Solution()
			{
				FamilyId = 200,
				IsAssigned = false,
				ProductId = 17,
				ProductName = "Active Building",
				SubSolution = null
			});
			solutionList.Add(new Solution()
			{
				FamilyId = 200,
				IsAssigned = false,
				ProductId = 18,
				ProductName = "Utility Management",
				SubSolution = null
			});

			expectedProductFamilyList.Add(new ProductFamily()
			{
				ProductTypeId = 200,
				Name = "Resident Services",
				Description = "Resident Services",
				Solutions = solutionList
			});

			var mockRepository = new Mock<IProductRepository>();
			mockRepository
                .Setup(m => m.GetProductFamilies(organizationRealPageId, editorRealpageUserId, userRealPageId, null, null))
                .Returns(() => expectedProductFamilyList);

            DefaultUserClaim userClaim = new DefaultUserClaim() {PersonaId = 1234, OrganizationRealPageGuid = new Guid(), UserRealPageGuid = new Guid()};

			ProductController controller = new ProductController(userClaim, mockRepository.Object, null);
			controller.Request = new HttpRequestMessage();
			controller.Configuration = new HttpConfiguration();

			//Act
			int NumberOfProperties = type.GetProperties().Length;
			HttpResponseMessage response = controller.GetProductFamilies(userRealPageId,null);
			productFamilyListOutput = response.Content.ReadAsAsync<ObjectListOutput<ProductFamily, IErrorData>>().Result;

			//Assert
			Assert.True(response.IsSuccessStatusCode);
			Assert.Equal("GET", response.RequestMessage.Method.ToString());
			Assert.Equal(2, productFamilyListOutput.list.Count);
			Assert.True(productFamilyListOutput.list.Count == expectedProductFamilyList.Count);
		}

		[Fact]
		public void GetProductFamilies_With_Access_Filter_MockRepository_ReturnValidRepositoryResponseObject()
		{
			//Arrange
			Guid userRealPageId = Guid.Empty;
			Guid editorRealpageUserId = Guid.Empty;

			Guid organizationRealPageId = Guid.Empty;
			ObjectListOutput<ProductFamily, IErrorData> productFamilyListOutput = new ObjectListOutput<ProductFamily, IErrorData>();
			Type type = typeof(ProductFamily);

			IList<ProductFamily> expectedProductFamilyList = new List<ProductFamily>();
			IList<Solution> solutionList = new List<Solution>();

			solutionList.Add(new Solution()
			{
				FamilyId = 100,
				IsAssigned = false,
				ProductId = 1,
				ProductName = "OneSite",
				SubSolution = "OneSite Leasing & Rents, Facilities, Purchasing, Doc. Mgmt",
				ShowInRolesAndRights = true,
				ShowInUserDetails = true
			});
			solutionList.Add(new Solution()
			{
				FamilyId = 100,
				IsAssigned = false,
				ProductId = 8,
				ProductName = "RealPage Accounting",
				SubSolution = "Property, Job Cost, Corporate",
				ShowInRolesAndRights = true,
				ShowInUserDetails = true
			});
			solutionList.Add(new Solution()
			{
				FamilyId = 100,
				IsAssigned = false,
				ProductId = 13,
				ProductName = "Spend Management",
				SubSolution = null,
				ShowInRolesAndRights = false,
				ShowInUserDetails = false
			});
			solutionList.Add(new Solution()
			{
				FamilyId = 100,
				IsAssigned = false,
				ProductId = 16,
				ProductName = "Vendor Credentialing",
				SubSolution = "Vendor Compliance",
				ShowInRolesAndRights = false,
				ShowInUserDetails = false
			});

			expectedProductFamilyList.Add(new ProductFamily()
			{
				ProductTypeId = 100,
				Name = "Property Management",
				Description = "Property Management",
				Solutions = solutionList
			});

			solutionList = new List<Solution>();
			solutionList.Add(new Solution()
			{
				FamilyId = 200,
				IsAssigned = false,
				ProductId = 15,
				ProductName = "Renters Insurance",
				SubSolution = null,
				ShowInRolesAndRights = true,
				ShowInUserDetails = true
			});
			solutionList.Add(new Solution()
			{
				FamilyId = 200,
				IsAssigned = false,
				ProductId = 17,
				ProductName = "Active Building",
				SubSolution = null,
				ShowInRolesAndRights = false,
				ShowInUserDetails = false
			});
			solutionList.Add(new Solution()
			{
				FamilyId = 200,
				IsAssigned = false,
				ProductId = 18,
				ProductName = "Utility Management",
				SubSolution = null,
				ShowInRolesAndRights = false,
				ShowInUserDetails = false
			});

			expectedProductFamilyList.Add(new ProductFamily()
			{
				ProductTypeId = 200,
				Name = "Resident Services",
				Description = "Resident Services",
				Solutions = solutionList
			});

			var mockRepository = new Mock<IProductRepository>();
			mockRepository
                .Setup(m => m.GetProductFamilies(organizationRealPageId, editorRealpageUserId, userRealPageId, "userdetails",null))
                .Returns(() => expectedProductFamilyList);

            DefaultUserClaim userClaim = new DefaultUserClaim() { PersonaId = 1234, OrganizationRealPageGuid = new Guid(), UserRealPageGuid = new Guid() };

            ProductController controller = new ProductController(userClaim, mockRepository.Object, null);
			controller.Request = new HttpRequestMessage();
			controller.Configuration = new HttpConfiguration();

			//Act
			int NumberOfProperties = type.GetProperties().Length;
			HttpResponseMessage response = controller.GetProductFamilies(userRealPageId, "userdetails");
			productFamilyListOutput = response.Content.ReadAsAsync<ObjectListOutput<ProductFamily, IErrorData>>().Result;

			//Assert
			Assert.True(response.IsSuccessStatusCode);
			Assert.Equal("GET", response.RequestMessage.Method.ToString());
			Assert.Equal(2, productFamilyListOutput.list.Count);
			Assert.True(productFamilyListOutput.list.Count == expectedProductFamilyList.Count);			
		}

		[Fact]
		public void ListProductUsers_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpConfiguration Config = new HttpConfiguration();

			//Act
            WebApiConfig.Register(Config);
			Config.EnsureInitialized();
			DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
			RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

            string result = baseTest.VerifyRouteToAction(
                    HttpMethod.Get,
                    "http://localhost/api/products/24/organization/-1");
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
            IList<ProductInternalSetting> productInternalSettings = new List<ProductInternalSetting>()
            {
                new ProductInternalSetting() {Name = "BooksUseDomains", Value = "1"}, 
                new ProductInternalSetting() {Name = "BooksUseUPFMId", Value = "1"}
            };
			
            var mockRepository = new Mock<IProductInternalSettingRepository>();
            mockRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.Is<int>(l => l == 1)
                    ))
                .Returns(productInternalSettings);

            DefaultUserClaim userClaim = new DefaultUserClaim() {PersonaId = 1234, OrganizationRealPageGuid = new Guid(), UserRealPageGuid = new Guid()};

            new RPObjectCache().BustCache();

            ProductController controller = new ProductController(userClaim, null, mockRepository.Object);
            controller.Request = new HttpRequestMessage();
            controller.Configuration = new HttpConfiguration();
			
            //Act
            var result = controller.GetProductNonSensitiveSettings(1);

            //Assert
            Assert.True(result.Count == productInternalSettings.Count);
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
			
            var mockProductRepository = new Mock<IProductRepository>();
            mockProductRepository
                .Setup(m => m.ListProductSettingType())
                .Returns(() => productSettingTypes);

            var mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            mockProductInternalSettingRepository
                .Setup(m => m.GetProductSettingByType(
                    It.Is<string>(l => l == "ProductStatus")
                ))
                .Returns(settings.Where(p => p.Name == "ProductStatus").ToList());

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductSettingByType(
                    It.Is<string>(l => l == "ApiSecret")
                ))
                .Returns(settings.Where(p => p.Name == "ApiSecret").ToList());

            DefaultUserClaim userClaim = new DefaultUserClaim() {PersonaId = 1234, OrganizationRealPageGuid = new Guid(), UserRealPageGuid = new Guid()};
            new RPObjectCache().BustCache();

            ProductController controller = new ProductController(userClaim, mockProductRepository.Object, mockProductInternalSettingRepository.Object);
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
			
            var mockProductRepository = new Mock<IProductRepository>();
            mockProductRepository
                .Setup(m => m.ListProductSettingType())
                .Returns(() => productSettingTypes);

            DefaultUserClaim userClaim = new DefaultUserClaim() {PersonaId = 1234, OrganizationRealPageGuid = new Guid(), UserRealPageGuid = new Guid()};
            new RPObjectCache().BustCache();

            ProductController controller = new ProductController(userClaim, mockProductRepository.Object, null);
            controller.Request = new HttpRequestMessage();
            controller.Configuration = new HttpConfiguration();

            //Act
            var response = controller.ListProductSettingType();

            //Assert
            Assert.True(response.Count == productSettingTypes.Count);
        }
		#endregion
	}
}
