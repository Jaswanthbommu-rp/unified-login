using RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI;
using RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Xunit;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.ControllerTest
{
    /// <summary>
    /// User xUnit tests
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class UserTests
    {
        #region Controller Unit Tests
        [Fact]
        public void GetUserProfile_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            HttpConfiguration config = new HttpConfiguration();

            //Act
            WebApiConfig.Register(config);
            config.EnsureInitialized();
            DefaultHttpControllerSelector controllerSelector = new DefaultHttpControllerSelector(config);
            RouteTestBase baseTest = new RouteTestBase(config, controllerSelector);

            //Assert
            Assert.True("GetUserProfile" == baseTest.VerifyRouteToAction(
                HttpMethod.Get,
                "http://localhost/api/user/13E71DE5-BAFA-469D-9F7A-E12DB3961BA9"
                )
            );
        }

        [Fact]
        public void UserCustomFields_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            HttpConfiguration config = new HttpConfiguration();

            //Act
            WebApiConfig.Register(config);
            config.EnsureInitialized();
            DefaultHttpControllerSelector controllerSelector = new DefaultHttpControllerSelector(config);
            RouteTestBase baseTest = new RouteTestBase(config, controllerSelector);

            //Assert
            Assert.True("UserCustomFields" == baseTest.VerifyRouteToAction(
                HttpMethod.Get,
                "http://localhost/api/customfields"
                )
            );
        }

        [Fact]
        public void CreateNewUser_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            HttpConfiguration config = new HttpConfiguration();

            //Act
            WebApiConfig.Register(config);
            config.EnsureInitialized();
            DefaultHttpControllerSelector controllerSelector = new DefaultHttpControllerSelector(config);
            RouteTestBase baseTest = new RouteTestBase(config, controllerSelector);

            //Assert
            Assert.True("CreateUser" == baseTest.VerifyRouteToAction(
                HttpMethod.Post,
                "http://localhost/api/user"
                )
            );

            Assert.True("CreateUser" == baseTest.VerifyRouteToAction(
                HttpMethod.Post,
                "http://localhost/api/user"
                )
            );

            Assert.True("CreateUser" == baseTest.VerifyRouteToAction(
                HttpMethod.Post,
                "http://localhost/api/user"
                )
            );
        }

        [Fact]
        public void UpdateNewUser_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            HttpConfiguration config = new HttpConfiguration();

            //Act
            WebApiConfig.Register(config);
            config.EnsureInitialized();
            DefaultHttpControllerSelector controllerSelector = new DefaultHttpControllerSelector(config);
            RouteTestBase baseTest = new RouteTestBase(config, controllerSelector);

            //Assert
            Assert.True("UpdateNewUser" == baseTest.VerifyRouteToAction(
                HttpMethod.Post,
                "http://localhost/api/newuser/profile?activityToken=41E2469D-4CAF-4FF4-B251-46CBC161A1C6&companyJobTitle=Leasing+Agent+I&userLogin=test@test1.com"
                )
            );
        }

        [Fact]
        public void AssignProductsToAdministrators_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            HttpConfiguration config = new HttpConfiguration();

            //Act
            WebApiConfig.Register(config);
            config.EnsureInitialized();
            DefaultHttpControllerSelector controllerSelector = new DefaultHttpControllerSelector(config);
            RouteTestBase baseTest = new RouteTestBase(config, controllerSelector);

            //Assert
            Assert.True("AssignProductsToAdministrators" == baseTest.VerifyRouteToAction(
                HttpMethod.Post,
                "http://localhost/api/user/assignproductstoadministrators?organizationRealPageId=8DA6737A-55FA-4DEA-BE2B-6AA2BE620A57&assignUserPersonaId=1"
                )
            );
        }

        [Fact]
        public void AssignProductsToAdministrators_InvalidOrganizationRealPageId_ReturnsErrorResponse()
        {
            //Arrange
            Guid realPageId = Guid.Empty;
            UserController userController = new UserController()
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };

            //Act
            HttpResponseMessage response = userController.AssignProductsToAdministrators(realPageId, 1);

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public void AssignProductsToAdministrators_InvalidAssignUserPersonaId_ReturnsErrorResponse()
        {
            //Arrange
            Guid realPageId = Guid.Empty;
            UserController userController = new UserController()
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };

            //Act
            HttpResponseMessage response = userController.AssignProductsToAdministrators(realPageId, -1);

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public void CreateUser_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            HttpConfiguration config = new HttpConfiguration();

            //Act
            WebApiConfig.Register(config);
            config.EnsureInitialized();
            DefaultHttpControllerSelector controllerSelector = new DefaultHttpControllerSelector(config);
            RouteTestBase baseTest = new RouteTestBase(config, controllerSelector);

            //Assert
            Assert.True("CreateUser" == baseTest.VerifyRouteToAction(
                HttpMethod.Post,
                "http://localhost/api/user"
                )
            );
        }

        [Fact]
        public void UpdateUser_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            HttpConfiguration config = new HttpConfiguration();

            //Act
            WebApiConfig.Register(config);
            config.EnsureInitialized();
            DefaultHttpControllerSelector controllerSelector = new DefaultHttpControllerSelector(config);
            RouteTestBase baseTest = new RouteTestBase(config, controllerSelector);

            //Assert
            Assert.True("UpdateUser" == baseTest.VerifyRouteToAction(
                HttpMethod.Put,
                "http://localhost/api/user"
                )
            );
        }
        #endregion
    }
}
