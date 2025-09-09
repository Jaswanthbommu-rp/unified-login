using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Moq;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.TwoFactor;
using RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers;
using Xunit;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.ControllerTest
{
    [ExcludeFromCodeCoverage]
    public class MultiFactorAuthTest
    {
        private Mock<IRepository> _mockRepository = new Mock<IRepository>();
        private static Guid _userRealPageId = new Guid("FF943C4D-D99E-4A68-82C6-C8DFF7128F1E");

        private bool GetUser(object obj)
        {
            return obj?.ToString().ToLower().Contains(_userRealPageId.ToString().ToLower()) ?? false;
        }

        [Fact]
        public void UpdateUserTwoFactorStatus_Tests()
        {
            UserLoginOnly user = new UserLoginOnly() {RealPageId = _userRealPageId, LoginName = "test@test.com"};
            AppAuthUser appAuthUser = new AppAuthUser() {Status = 1};

            _mockRepository
                .Setup(m => m.GetOne<UserLoginOnly>(StoredProcNameConstants.SP_GetUserLoginOnly, 
                    It.Is<object>(
                        p => GetUser(p))))
                .Returns(user);

            _mockRepository
                .Setup(m => m.ExecuteNonQuery(StoredProcNameConstants.SP_UpdateUserLoginTwoFactor, 
                    It.IsAny<object>()))
                .Returns(1);

            //Arrange
            MultiFactorAuthController controller = new MultiFactorAuthController(
                _mockRepository.Object
            )
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };

            var httpRequest = new HttpRequest("", "http://localhost/", "");
            var stringWriter = new StringWriter();
            var httpResponse = new HttpResponse(stringWriter);
            HttpContext.Current = new HttpContext(httpRequest, httpResponse);

            //Act           
            HttpResponseMessage response = controller.UpdateUserAppAuth(user.RealPageId, appAuthUser);

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.NoContent));

            // Bad user id

            response = controller.UpdateUserAppAuth(Guid.Empty, appAuthUser);
            Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));
        }

        [Fact]
        public void DeleteUserAppAuthToken_Tests()
        {
            UserLoginOnly user = new UserLoginOnly() {RealPageId = _userRealPageId, LoginName = "test@test.com"};

            _mockRepository
                .Setup(m => m.GetOne<UserLoginOnly>(StoredProcNameConstants.SP_GetUserLoginOnly, 
                    It.Is<object>(
                        p => GetUser(p))))
                .Returns(user);

            _mockRepository
                .Setup(m => m.ExecuteNonQuery(StoredProcNameConstants.SP_CreateUpdateUserTokenDetail, 
                    It.IsAny<object>()))
                .Returns(1);

            _mockRepository
                .Setup(m => m.ExecuteNonQuery(StoredProcNameConstants.SP_UpdateUserLoginTwoFactor,
                    It.IsAny<object>()))
                .Returns(1);

            //Arrange
            MultiFactorAuthController controller = new MultiFactorAuthController(
                _mockRepository.Object
            )
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };

            var httpRequest = new HttpRequest("", "http://localhost/", "");
            var stringWriter = new StringWriter();
            var httpResponse = new HttpResponse(stringWriter);
            HttpContext.Current = new HttpContext(httpRequest, httpResponse);

            //Act           
            HttpResponseMessage response = controller.DeleteUserAppAuthToken(user.RealPageId);

            //Assert
            Assert.True(response.StatusCode.Equals(HttpStatusCode.NoContent));

            // Bad user id

            response = controller.DeleteUserAppAuthToken(Guid.Empty);
            Assert.True(response.StatusCode.Equals(HttpStatusCode.BadRequest));
        }

    }
}
