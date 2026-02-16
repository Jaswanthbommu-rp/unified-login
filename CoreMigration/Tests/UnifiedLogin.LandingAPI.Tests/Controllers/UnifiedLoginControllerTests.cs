using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.LandingAPI.Controllers;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Product.UnifiedLogin;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Controllers
{
    [ExcludeFromCodeCoverage]
    public class UnifiedLoginControllerTests : ControllerTestBase
    {
        private UnifiedLoginController _controller;

        public UnifiedLoginControllerTests()
        {
            _controller = new UnifiedLoginController(MockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            var controller = new UnifiedLoginController(MockUserClaimsAccessor.Object);

            Assert.NotNull(controller);
        }

        #endregion

        #region GetUserRoles Tests

        [Fact]
        public async Task GetUserRoles_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.GetUserRoles(0, 200, 1000, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetUserRoles_WithZeroPartyId_ReturnsBadRequest()
        {
            var result = await _controller.GetUserRoles(100, 200, 0, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("partyId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetUserRoles_WithValidParameters_ReturnsOkResult()
        {
            var result = await _controller.GetUserRoles(100, 200, 1000, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetUserRoles_WithZeroUserPersonaId_ReturnsOkResult()
        {
            var result = await _controller.GetUserRoles(100, 0, 1000, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetUserRoles_WithNullDataFilter_ReturnsOkResult()
        {
            var result = await _controller.GetUserRoles(100, 200, 1000, null!);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region DeleteRole Tests

        [Fact]
        public async Task DeleteRole_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.DeleteRole(0, 5);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task DeleteRole_WithZeroRoleId_ReturnsBadRequest()
        {
            var result = await _controller.DeleteRole(100, 0);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("roleId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task DeleteRole_WithValidParameters_ReturnsResult()
        {
            var result = await _controller.DeleteRole(100, 5);

            Assert.NotNull(result);
        }

        #endregion

        #region SetDefaultRole Tests

        [Fact]
        public async Task SetDefaultRole_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.SetDefaultRole(0, 5);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task SetDefaultRole_WithZeroRoleId_ReturnsBadRequest()
        {
            var result = await _controller.SetDefaultRole(100, 0);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("roleId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task SetDefaultRole_WithValidParameters_ReturnsResult()
        {
            var result = await _controller.SetDefaultRole(100, 5);

            Assert.NotNull(result);
        }

        #endregion

        #region GetRoles Tests

        [Fact]
        public async Task GetRoles_WithZeroPartyId_ReturnsBadRequest()
        {
            var result = await _controller.GetRoles(100, 0, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("partyId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRoles_WithValidParameters_ReturnsOkResult()
        {
            var result = await _controller.GetRoles(100, 1000, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        //[Fact]
        //public async Task GetRoles_WithZeroEditorPersonaIdAndNoInternalApiScope_ReturnsOkResult()
        //{
        //    var result = await _controller.GetRoles(0, 1000, new RequestParameter());

        //    // When editorPersonaId is 0 and no internal API scope, the controller may still return Ok
        //    Assert.NotNull(result);
        //}

        [Fact]
        public async Task GetRoles_WithUpfmId_ReturnsResult()
        {
            var result = await _controller.GetRoles(100, 1000, new RequestParameter(), Guid.NewGuid());

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetRoles_WithNullUpfmId_ReturnsOkResult()
        {
            var result = await _controller.GetRoles(100, 1000, new RequestParameter(), null);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region GetRolesWithCount Tests

        [Fact]
        public async Task GetRolesWithCount_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.GetRolesWithCount(0, 1000, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRolesWithCount_WithZeroPartyId_ReturnsBadRequest()
        {
            var result = await _controller.GetRolesWithCount(100, 0, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("partyId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRolesWithCount_WithValidParameters_ReturnsOkResult()
        {
            var result = await _controller.GetRolesWithCount(100, 1000, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region GetRights Tests

        [Fact]
        public async Task GetRights_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.GetRights(0, 1000, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRights_WithZeroPartyId_ReturnsBadRequest()
        {
            var result = await _controller.GetRights(100, 0, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("partyId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRights_WithValidParameters_ReturnsOkResult()
        {
            var result = await _controller.GetRights(100, 1000, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region GetRightsWithCount Tests

        [Fact]
        public async Task GetRightsWithCount_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.GetRightsWithCount(0, 1000, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRightsWithCount_WithZeroPartyId_ReturnsBadRequest()
        {
            var result = await _controller.GetRightsWithCount(100, 0, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("partyId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRightsWithCount_WithValidParameters_ReturnsOkResult()
        {
            var result = await _controller.GetRightsWithCount(100, 1000, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region GetAllRightsByRole Tests

        [Fact]
        public async Task GetAllRightsByRole_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.GetAllRightsByRole(0, 1000, 5, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetAllRightsByRole_WithZeroPartyId_ReturnsBadRequest()
        {
            var result = await _controller.GetAllRightsByRole(100, 0, 5, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("partyId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetAllRightsByRole_WithValidParameters_ReturnsOkResult()
        {
            var result = await _controller.GetAllRightsByRole(100, 1000, 5, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetAllRightsByRole_WithZeroRoleId_ReturnsOkResult()
        {
            var result = await _controller.GetAllRightsByRole(100, 1000, 0, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region GetRightsByRole Tests

        [Fact]
        public async Task GetRightsByRole_WithZeroPartyId_ReturnsBadRequest()
        {
            var result = await _controller.GetRightsByRole(100, 0, 5, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("partyId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRightsByRole_WithValidParameters_ReturnsOkResult()
        {
            var result = await _controller.GetRightsByRole(100, 1000, 5, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetRightsByRole_WithUpfmId_ReturnsResult()
        {
            var result = await _controller.GetRightsByRole(100, 1000, 5, new RequestParameter(), Guid.NewGuid());

            Assert.NotNull(result);
        }

        //[Fact]
        //public async Task GetRightsByRole_WithZeroEditorPersonaId_ReturnsResult()
        //{
        //    var result = await _controller.GetRightsByRole(0, 1000, 5, new RequestParameter());

        //    Assert.NotNull(result);
        //}

        #endregion

        #region GetRolesByRight Tests

        [Fact]
        public async Task GetRolesByRight_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.GetRolesByRight(0, 1000, 5, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRolesByRight_WithZeroPartyId_ReturnsBadRequest()
        {
            var result = await _controller.GetRolesByRight(100, 0, 5, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("partyId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRolesByRight_WithValidParameters_ReturnsOkResult()
        {
            var result = await _controller.GetRolesByRight(100, 1000, 5, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetRolesByRight_WithZeroRightId_ReturnsOkResult()
        {
            var result = await _controller.GetRolesByRight(100, 1000, 0, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region AddRole Tests

        [Fact]
        public async Task AddRole_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.AddRole(0, 1000, "Test Role");

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task AddRole_WithZeroPartyId_ReturnsBadRequest()
        {
            var result = await _controller.AddRole(100, 0, "Test Role");

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("partyId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task AddRole_WithEmptyRoleName_ReturnsBadRequest()
        {
            var result = await _controller.AddRole(100, 1000, "   ");

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("roleName not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task AddRole_WithValidParameters_ReturnsOkResult()
        {
            var result = await _controller.AddRole(100, 1000, "Test Role");

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task AddRole_WithInheritRoleId_ReturnsOkResult()
        {
            var result = await _controller.AddRole(100, 1000, "Test Role", "5");

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task AddRole_WithNullInheritRoleId_ReturnsOkResult()
        {
            var result = await _controller.AddRole(100, 1000, "Test Role", null);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region UpdateRole Tests

        [Fact]
        public async Task UpdateRole_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.UpdateRole(0, 1000, 5, "Test Role");

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateRole_WithZeroPartyId_ReturnsBadRequest()
        {
            var result = await _controller.UpdateRole(100, 0, 5, "Test Role");

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("partyId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateRole_WithZeroRoleId_ReturnsBadRequest()
        {
            var result = await _controller.UpdateRole(100, 1000, 0, "Test Role");

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("roleid not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateRole_WithEmptyRoleName_ReturnsBadRequest()
        {
            var result = await _controller.UpdateRole(100, 1000, 5, "   ");

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("roleName not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateRole_WithValidParameters_ReturnsResult()
        {
            var result = await _controller.UpdateRole(100, 1000, 5, "Test Role");

            Assert.NotNull(result);
        }

        [Fact]
        public async Task UpdateRole_WithInheritRoleId_ReturnsResult()
        {
            var result = await _controller.UpdateRole(100, 1000, 5, "Test Role", "10");

            Assert.NotNull(result);
        }

        #endregion

        #region UpdateRoleRights Tests

        [Fact]
        public async Task UpdateRoleRights_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var rightsToAddRemove = new ULRightsAddRemoveList
            {
                RightsToAdd = new List<string> { "1", "2" }
            };

            var result = await _controller.UpdateRoleRights(0, 5, rightsToAddRemove);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateRoleRights_WithZeroRoleId_ReturnsBadRequest()
        {
            var rightsToAddRemove = new ULRightsAddRemoveList
            {
                RightsToAdd = new List<string> { "1", "2" }
            };

            var result = await _controller.UpdateRoleRights(100, 0, rightsToAddRemove);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("roleId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateRoleRights_WithNullRightsLists_ReturnsBadRequest()
        {
            var rightsToAddRemove = new ULRightsAddRemoveList();

            var result = await _controller.UpdateRoleRights(100, 5, rightsToAddRemove);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("No Data", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateRoleRights_WithValidParameters_ReturnsOkResult()
        {
            var rightsToAddRemove = new ULRightsAddRemoveList
            {
                RightsToAdd = new List<string> { "1", "2" },
                RightsToDelete = new List<string> { "3" }
            };

            var result = await _controller.UpdateRoleRights(100, 5, rightsToAddRemove);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateRoleRights_WithOnlyRightsToAdd_ReturnsOkResult()
        {
            var rightsToAddRemove = new ULRightsAddRemoveList
            {
                RightsToAdd = new List<string> { "1", "2" }
            };

            var result = await _controller.UpdateRoleRights(100, 5, rightsToAddRemove);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateRoleRights_WithOnlyRightsToDelete_ReturnsOkResult()
        {
            var rightsToAddRemove = new ULRightsAddRemoveList
            {
                RightsToDelete = new List<string> { "1", "2" }
            };

            var result = await _controller.UpdateRoleRights(100, 5, rightsToAddRemove);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region CloneRoleRights Tests

        [Fact]
        public async Task CloneRoleRights_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var rightsToAddRemove = new ULRightsAddRemoveList
            {
                RightsToAdd = new List<string> { "1", "2" }
            };

            var result = await _controller.CloneRoleRights(0, 5, rightsToAddRemove);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task CloneRoleRights_WithZeroRoleId_ReturnsBadRequest()
        {
            var rightsToAddRemove = new ULRightsAddRemoveList
            {
                RightsToAdd = new List<string> { "1", "2" }
            };

            var result = await _controller.CloneRoleRights(100, 0, rightsToAddRemove);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("roleId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task CloneRoleRights_WithNullRightsLists_ReturnsBadRequest()
        {
            var rightsToAddRemove = new ULRightsAddRemoveList();

            var result = await _controller.CloneRoleRights(100, 5, rightsToAddRemove);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("No Data", badRequestResult.Value);
        }

        [Fact]
        public async Task CloneRoleRights_WithValidParameters_ReturnsOkResult()
        {
            var rightsToAddRemove = new ULRightsAddRemoveList
            {
                RightsToAdd = new List<string> { "1", "2" }
            };

            var result = await _controller.CloneRoleRights(100, 5, rightsToAddRemove);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region UpdateRightRoles Tests

        [Fact]
        public async Task UpdateRightRoles_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var rolesToAddRemove = new RolesAddRemoveList
            {
                RolesToAdd = new List<string> { "1", "2" }
            };

            var result = await _controller.UpdateRightRoles(0, 5, rolesToAddRemove);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateRightRoles_WithZeroRightId_ReturnsBadRequest()
        {
            var rolesToAddRemove = new RolesAddRemoveList
            {
                RolesToAdd = new List<string> { "1", "2" }
            };

            var result = await _controller.UpdateRightRoles(100, 0, rolesToAddRemove);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("roleId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateRightRoles_WithNullRolesLists_ReturnsBadRequest()
        {
            var rolesToAddRemove = new RolesAddRemoveList();

            var result = await _controller.UpdateRightRoles(100, 5, rolesToAddRemove);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("No Data", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateRightRoles_WithValidParameters_ReturnsResult()
        {
            var rolesToAddRemove = new RolesAddRemoveList
            {
                RolesToAdd = new List<string> { "1", "2" }
            };

            var result = await _controller.UpdateRightRoles(100, 5, rolesToAddRemove);

            Assert.NotNull(result);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public async Task GetUserRoles_WithMaxLongValues_ReturnsOkResult()
        {
            var result = await _controller.GetUserRoles(long.MaxValue, long.MaxValue, long.MaxValue, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetRoles_WithMaxLongValues_ReturnsOkResult()
        {
            var result = await _controller.GetRoles(long.MaxValue, long.MaxValue, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task AddRole_WithLongRoleName_ReturnsOkResult()
        {
            var result = await _controller.AddRole(100, 1000, new string('A', 200));

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task AddRole_WithSpecialCharactersInRoleName_ReturnsOkResult()
        {
            var result = await _controller.AddRole(100, 1000, "Test & Role <Special>");

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateRoleRights_WithEmptyRightsToAdd_ReturnsOkResult()
        {
            var rightsToAddRemove = new ULRightsAddRemoveList
            {
                RightsToAdd = new List<string>(),
                RightsToDelete = new List<string> { "1" }
            };

            var result = await _controller.UpdateRoleRights(100, 5, rightsToAddRemove);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region Dispose

        public override void Dispose()
        {
            _controller = null!;
            base.Dispose();
        }

        #endregion
    }
}
