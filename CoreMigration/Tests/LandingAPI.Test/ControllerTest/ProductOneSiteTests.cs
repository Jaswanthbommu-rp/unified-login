using UnifiedLogin.LandingAPI;
using Microsoft.AspNetCore.Mvc;

using Xunit;
using System.Net.Http;
using UnifiedLogin.LandingAPI.Test.Logic;
using System.Diagnostics.CodeAnalysis;

namespace UnifiedLogin.LandingAPI.Test.ControllerTest
{
    [ExcludeFromCodeCoverage]
    public class ProductOneSiteTests
    {
        private readonly RouteTestBase _baseTest;
        private const string _baseUrl = "http://localhost/api/products/onesite";

        #region Constructor
        public ProductOneSiteTests()
        {
            HttpClient config = new HttpClient();
            WebApiConfig.Register(config);
            config.EnsureInitialized();
            DefaultHttpControllerSelector controllerSelector = new DefaultHttpControllerSelector(config);
            _baseTest = new RouteTestBase(config, controllerSelector);
        }
        #endregion

        #region Controller Unit Tests
        [Fact]
        public void UpdateProperty_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            var expected = "UpdateOneSiteUserProperties";
            var url = $"{_baseUrl}/user/properties?editorPersonaId=12&userPersonaId=123";
            var method = HttpMethod.Put;

            //Act
            var actual = _baseTest.VerifyRouteToAction(method, url);

            //Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void UpdateOneSiteUserRoles_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            var expected = "UpdateOneSiteUserRoles";
            var url = $"{_baseUrl}/user/roles?editorPersonaId=12&userPersonaId=123";
            var method = HttpMethod.Put;

            //Act
            var actual = _baseTest.VerifyRouteToAction(method, url);

            //Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetUserProperties_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            var expected = "GetOneSitePropertyList";
            var url = $"{_baseUrl}/user/properties?editorPersonaId=12&userPersonaId=123&assignedOnly=false";
            var method = HttpMethod.Get;

            //Act
            var actual = _baseTest.VerifyRouteToAction(method, url);

            //Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetUserRoles_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            var expected = "GetOneSiteRoleList";
            var url = $"{_baseUrl}/user/roles?editorPersonaId=12&userPersonaId=123&assignedOnly=false";
            var method = HttpMethod.Get;

            //Act
            var actual = _baseTest.VerifyRouteToAction(method, url);

            //Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetAllRoles_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            var expected = "GetOneSiteRoleListAll";
            var url = $"{_baseUrl}/role?editorPersonaId=123";
            var method = HttpMethod.Get;

            //Act
            var actual = _baseTest.VerifyRouteToAction(method, url);

            //Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetOneSitePropertyUsersList_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            var expected = "GetOneSitePropertyUsersList";
            var url = $"{_baseUrl}/property/users?editorPersonaId=123&propertyId=1234567";
            var method = HttpMethod.Get;

            //Act
            var actual = _baseTest.VerifyRouteToAction(method, url);

            //Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetOneSiteRoleUsersList_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            var expected = "GetOneSiteRoleUsersList";
            var url = $"{_baseUrl}/role/users?editorPersonaId=123&propertyId=1234567&roleid=4";
            var method = HttpMethod.Get;

            //Act
            var actual = _baseTest.VerifyRouteToAction(method, url);

            //Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void UpdateRoleRights_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            var expected = "UpdateRoleRights";
            var url = $"{_baseUrl}/role/rights?editorPersonaId=123&roleid=4";
            var method = HttpMethod.Put;

            //Act
            var actual = _baseTest.VerifyRouteToAction(method, url);

            //Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CreateOneSiteUser_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            var expected = "CreateOneSiteUser";
            var url = $"{_baseUrl}/user?editorPersonaId=123&userPersonaId=4";
            var method = HttpMethod.Post;

            //Act
            var actual = _baseTest.VerifyRouteToAction(method, url);

            //Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void UpdateOneSiteUser_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            var expected = "UpdateOneSiteUser";
            var url = $"{_baseUrl}/user?editorPersonaId=123&userPersonaId=4";
            var method = HttpMethod.Put;

            //Act
            var actual = _baseTest.VerifyRouteToAction(method, url);

            //Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void DeleteOneSiteUser_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            var expected = "DeleteOneSiteUser";
            var url = $"{_baseUrl}/user?editorPersonaId=123&userPersonaId=4";
            var method = HttpMethod.Delete;

            //Act
            var actual = _baseTest.VerifyRouteToAction(method, url);

            //Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void UpdateOneSiteRolesWithRight_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            var expected = "UpdateOneSiteRolesWithRight";
            var url = $"{_baseUrl}/right/roles?editorPersonaId=123&rightid=4&roleList=4&assignStatus=true";
            var method = HttpMethod.Put;

            //Act
            var actual = _baseTest.VerifyRouteToAction(method, url);

            //Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetRolesForRight_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            var expected = "GetRolesForRight";
            var url = $"{_baseUrl}/right/roles?editorPersonaId=123&rightId=5&assignedOnly=false";
            var method = HttpMethod.Get;

            //Act
            var actual = _baseTest.VerifyRouteToAction(method, url);

            //Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetOneSiteRightCenters_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            var expected = "GetOneSiteRightCenters";
            var url = $"{_baseUrl}/right/center?editorPersonaId=123";
            var method = HttpMethod.Get;

            //Act
            var actual = _baseTest.VerifyRouteToAction(method, url);

            //Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetOneSiteRights_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            var expected = "GetOneSiteRights";
            var url = $"{_baseUrl}/rights?editorPersonaId=123&roleId=5";
            var method = HttpMethod.Get;

            //Act
            var actual = _baseTest.VerifyRouteToAction(method, url);

            //Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AddRole_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            var expected = "AddRole";
            var url = $"{_baseUrl}/role?editorPersonaId=123&rolename=newrole";
            var method = HttpMethod.Post;

            //Act
            var actual = _baseTest.VerifyRouteToAction(method, url);

            //Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void UpdateRole_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            var expected = "UpdateRole";
            var url = $"{_baseUrl}/role?editorPersonaId=123&roleid=5&rolename=newrole2";
            var method = HttpMethod.Put;

            //Act
            var actual = _baseTest.VerifyRouteToAction(method, url);

            //Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void DeleteRole_VerifyRouteToAction_ReturnAction()
        {

            //Arrange
            var expected = "DeleteRole";
            var url = $"{_baseUrl}/role?editorPersonaId=123&roleid=5";
            var method = HttpMethod.Delete;

            //Act
            var actual = _baseTest.VerifyRouteToAction(method, url);

            //Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ListOneSiteMigrationUsers_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            var expected = "ListOneSiteMigrationUsers";
            var url = $"{_baseUrl}/migration-users?editorPersonaId=0";
            var method = HttpMethod.Get;

            //Act
            var actual = _baseTest.VerifyRouteToAction(method, url);

            //Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void DisableOneSiteUser_VerifyRouteToAction_ReturnAction()
        {
            //Arrange
            var expected = "UpdateOneSiteUserStatus";
            var url = $"{_baseUrl}/user/MT/status";
            var method = HttpMethod.Put;

            //Act
            var actual = _baseTest.VerifyRouteToAction(method, url);

            //Assert
            Assert.Equal(expected, actual);
        }

#endregion
    }
}
