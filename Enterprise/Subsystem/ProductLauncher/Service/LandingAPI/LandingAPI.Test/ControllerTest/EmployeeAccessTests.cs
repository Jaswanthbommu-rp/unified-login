using Moq;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.OneSite;
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
using IC = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.ControllerTest
{
    [ExcludeFromCodeCoverage]
    public class EmployeeAccessTests
    {
        protected readonly Mock<IRepository> mockRepository = new Mock<IRepository>();
        private IList<ProductInternalSetting> _settings;
        Mock<HttpMessageHandler> _mockHttpMessageHandler;
        Mock<IOneSiteProductService> _mockService;
        DefaultUserClaim userClaim;
        protected Mock<IManageEmployeeAccess>_manageEmployeeAccess;

        private void FillInstance()
        {
            _manageEmployeeAccess = new Mock<IManageEmployeeAccess>();
            userClaim = new DefaultUserClaim() { PersonaId = 1234, OrganizationRealPageGuid = new Guid(), UserRealPageGuid = new Guid() };
            _settings = new List<ProductInternalSetting>()
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

            IList<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.ProductInternalSetting> productInternalSettings = new List<ProductInternalSetting>()
            {
                new ProductInternalSetting()
                {
                    ConfigurationId = "12", Value ="345"
                },
                new ProductInternalSetting()
                {
                          ConfigurationId = "1322", Value ="33445"
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
            List<GbProductMap> gbProductMap = new List<GbProductMap>
            {
                new GbProductMap() { BooksProductCode = "OS", Name = "OneSite", ProductId = 1, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "UI", Name = "UnifiedUI", ProductId = 2, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "UPFM", Name = "Unified Platform", ProductId = 3, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "AO", Name = "Asset Optimization", ProductId = 4, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "GFF", Name = "Support Tool", ProductId = 35, UDMSourceCode = null }
            };

            _mockService = new Mock<IOneSiteProductService>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            mockRepository.Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct, It.IsAny<object>()))
            .Returns(gbProductMap);

            List<IC.ProductInternalSetting> _productInternalSettings = new List<IC.ProductInternalSetting>() {
                new ProductInternalSetting() { Name = "ApiEndPoint", Value = "1" },
               new ProductInternalSetting() { Name = "BooksUseDomains", Value = "1" },
                new ProductInternalSetting() { Name = "BooksUseUPFMId", Value = "1" },
                new ProductInternalSetting() { Name = "ShowInUserDetails", Value = "1" },
                new ProductInternalSetting() { Name = "MTAPiEndPoint", Value = "api/core/common/ulmigration" },
                new ProductInternalSetting() { Name = "MTTokenEndPoint", Value = "api/core/authentication/login" },
                new ProductInternalSetting() { Name = "MTClientId", Value = "OneSiteClient" },
                new ProductInternalSetting() { Name = "MTClientSECRET", Value = "OneSiteClientSecret" }
            };

            mockRepository.Setup(m => m.GetMany<IC.ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct,
               It.IsAny<object>()))
           .Returns(_productInternalSettings);
        }

        #region Controller Unit Tests
        [Fact]
        public void GetCompanies_InvalidEditorPersona_ExceptionThrown()
        {
            //Arrange   
            EmployeeAccessController employeeAccessController = new EmployeeAccessController();

            //Act
            Exception exception = Record.Exception(() => employeeAccessController.GetCompanies(12, null));

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
        public void CreateEmployeeProductUser_InValid_ReturnResult()
        {
            FillInstance();

            List<IC.ProductInternalSetting> _productInternalSettings = new List<IC.ProductInternalSetting>() {
                new ProductInternalSetting() { Name = "ApiEndPoint", Value = "1" },
               new ProductInternalSetting() { Name = "BooksUseDomains", Value = "1" },
                new ProductInternalSetting() { Name = "BooksUseUPFMId", Value = "1" },
                new ProductInternalSetting() { Name = "ShowInUserDetails", Value = "1" },
                new ProductInternalSetting() { Name = "MTAPiEndPoint", Value = "api/core/common/ulmigration" },
                new ProductInternalSetting() { Name = "MTTokenEndPoint", Value = "api/core/authentication/login" },
                new ProductInternalSetting() { Name = "MTClientId", Value = "OneSiteClient" },
                new ProductInternalSetting() { Name = "MTClientSECRET", Value = "OneSiteClientSecret" }
            };

            mockRepository.Setup(m => m.GetMany<IC.ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct,
               It.IsAny<object>()))
           .Returns(_productInternalSettings);

            //Arrange   
            EmployeeAccessController employeeAccessController = new EmployeeAccessController(_manageEmployeeAccess.Object, mockRepository.Object, userClaim, _mockHttpMessageHandler.Object, _mockService.Object);
            //Act
            var response = employeeAccessController.CreateEmployeeProductUser(3, 1234);
            Assert.True(response.IsSuccessStatusCode);
            var errorMessage =  response.Content.ReadAsStringAsync().Result;
            if (!string.IsNullOrEmpty(errorMessage) && errorMessage.Contains("Product does not support employee creation")) 
            {
                Assert.Contains("Product does not support employee creation", errorMessage);
            }
        }

        [Fact]
        public void CreateEmployeeProductUser_Valid_ReturnResult()
        {
            FillInstance();

            List<IC.ProductInternalSetting> _productInternalSettings = new List<IC.ProductInternalSetting>() {
                new ProductInternalSetting() { Name = "ApiEndPoint", Value = "1" },
               new ProductInternalSetting() { Name = "BooksUseDomains", Value = "1" },
                new ProductInternalSetting() { Name = "BooksUseUPFMId", Value = "1" },
                new ProductInternalSetting() { Name = "ShowInUserDetails", Value = "1" },
                new ProductInternalSetting() { Name = "MTAPiEndPoint", Value = "api/core/common/ulmigration" },
                new ProductInternalSetting() { Name = "MTTokenEndPoint", Value = "api/core/authentication/login" },
                new ProductInternalSetting() { Name = "MTClientId", Value = "OneSiteClient" },
                new ProductInternalSetting() { Name = "MTClientSECRET", Value = "OneSiteClientSecret" },
                new ProductInternalSetting() { Name = "SI_SupportsEmployeeCreation", Value = "1" },
                new ProductInternalSetting() { Name = "ProductAssignedViaADGroupWithoutUserCreation", Value = "1" }
                
            };

            mockRepository.Setup(m => m.GetMany<IC.ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct,
               It.IsAny<object>()))
           .Returns(_productInternalSettings);
            //Arrange   
            EmployeeAccessController employeeAccessController = new EmployeeAccessController(_manageEmployeeAccess.Object, mockRepository.Object, userClaim, _mockHttpMessageHandler.Object, _mockService.Object);
            //Act
            var response = employeeAccessController.CreateEmployeeProductUser(3, 1234);

            Assert.True(response.IsSuccessStatusCode);
            var errorMessage = response.Content.ReadAsStringAsync().Result;
         
        }

        public bool TestIsProductId(object obj, int productId)
        {
            return obj.ToString().ToLower().Contains($"productid = {productId}");
        }
        #endregion
    }
}
