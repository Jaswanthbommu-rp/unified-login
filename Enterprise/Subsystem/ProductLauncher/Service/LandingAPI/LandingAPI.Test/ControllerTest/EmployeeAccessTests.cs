using Moq;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic.ProductIntegration;
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
    [ExcludeFromCodeCoverage]
    public class EmployeeAccessTests
    {
        protected readonly Mock<IRepository> mockRepository = new Mock<IRepository>();
        #region Controller Unit Tests
        [Fact]
        public void GetCompanies_InvalidEditorPersona_ExceptionThrown()
        {
            //Arrange   
            EmployeeAccessController employeeAccessController = new EmployeeAccessController();

            //Act
            Exception exception = Record.Exception(() => employeeAccessController.GetCompanies(12,null));

            //Assert
            Assert.IsType<NullReferenceException>(exception);
        }

        [Fact]
        public void GetCompanies_InvalidEditorPersona0_ExceptionThrown()
        {
            //Arrange   
            EmployeeAccessController employeeAccessController = new EmployeeAccessController();

            //Act
            Exception exception = Record.Exception(() => employeeAccessController.GetCompanies(0, null));

            //Assert
            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void GetEmployeePersonaId_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            HttpConfiguration Config = new HttpConfiguration();

            //Act
            WebApiConfig.Register(Config);
            Config.EnsureInitialized();
            DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
            RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

            //Assert
            Assert.True("GetEmployeePersonaId" == baseTest.VerifyRouteToAction(HttpMethod.Get, $"http://localhost/api/employeeaccess/company/{Guid.Empty}/persona"));
        }


        [Fact]
        public void CreateEmployeeProductUser_ProductIdWithZero_ReturnException()
        {
            //Arrange   
            EmployeeAccessController employeeAccessController = new EmployeeAccessController();

            //Act
            Exception exception = Record.Exception(() => employeeAccessController.CreateEmployeeProductUser(0, 12));

            //Assert
            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void CreateEmployeeProductUser_PersonaIdWithZero_ReturnException()
        {
            //Arrange   
            EmployeeAccessController employeeAccessController = new EmployeeAccessController();

            //Act
            Exception exception = Record.Exception(() => employeeAccessController.CreateEmployeeProductUser(22, 0));

            //Assert
            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void CreateEmployeeProductUser_Valid_ReturnResult()
        {
         	IList<ProductInternalSetting> settings = new List<ProductInternalSetting>()
			{
				new ProductInternalSetting
				{
					ProductConfigurationId = "1234",
					Name = "ProductStatus",
					Value = "8"
				},
				new ProductInternalSetting {
				ProductConfigurationId = "1235",
				Name = "ProductStatus",
				Value = "8"
				},
				new ProductInternalSetting
				{
					ProductConfigurationId = "5555",
					Name = "ApiSecret",
					Value = "somesecret"
				},
				new ProductInternalSetting
				{
					ProductConfigurationId = "6666",
					Name = "ApiSecret",
					Value = "some password"
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
				.Setup(m => m.GetMany<ProductSettingType>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, null))
				.Returns(() => productSettingTypes);

            var productInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            var manageEmployeeAccess = new Mock<IManageEmployeeAccess>();
            DefaultUserClaim userClaim = new DefaultUserClaim() { PersonaId = 1234, OrganizationRealPageGuid = new Guid(), UserRealPageGuid = new Guid() };

            //Arrange   
            ProductEnum productType = ProductEnum.LeadManagement;
            productInternalSettingRepository.Setup(m => m.GetProductInternalSettings((int)productType)).Returns(IlmTestData.Get_DEV_ILM_ProductSettings());
            EmployeeAccessController employeeAccessController = new EmployeeAccessController(manageEmployeeAccess.Object);
            //Act
            var response = employeeAccessController.CreateEmployeeProductUser(22, 65);

            Assert.True(response.IsSuccessStatusCode);
        }
        #endregion
    }
}
