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
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Xunit;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.ControllerTest
{
	/// <summary>
	/// Product xUnit tests
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class ProductTests
	{
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
                .Setup(m => m.GetProductFamilies(organizationRealPageId, editorRealpageUserId, userRealPageId, null))
                .Returns(() => expectedProductFamilyList);

            DefaultUserClaim userClaim = new DefaultUserClaim() {PersonaId = 1234, OrganizationRealPageGuid = new Guid(), UserRealPageGuid = new Guid()};

			ProductController controller = new ProductController(userClaim, mockRepository.Object);
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
                .Setup(m => m.GetProductFamilies(organizationRealPageId, editorRealpageUserId, userRealPageId, "userdetails"))
                .Returns(() => expectedProductFamilyList);

            DefaultUserClaim userClaim = new DefaultUserClaim() { PersonaId = 1234, OrganizationRealPageGuid = new Guid(), UserRealPageGuid = new Guid() };

            ProductController controller = new ProductController(userClaim, mockRepository.Object);
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

			//Assert
			Assert.True("ListProductUsers" == baseTest.VerifyRouteToAction(
				HttpMethod.Get,
				"http://localhost/api/products/24/organization/-1"
				)
			);
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
		#endregion
	}
}
