using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.LandingAPI.Controllers;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Controllers.Enterprise
{
    [ExcludeFromCodeCoverage]
    public class UserControllerV2Should : ControllerTestBase
    {
        private UserController _controller;

        public UserControllerV2Should()
        {
            _controller = new UserController(MockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            var controller = new UserController(MockUserClaimsAccessor.Object);

            Assert.NotNull(controller);
        }

        [Fact]
        public void Constructor_WithNullUserClaimsAccessor_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new UserController(null!));

            Assert.Equal("userClaimsAccessor", exception.ParamName);
        }

        #endregion

        #region AssignProductsToAdministrators Tests

        [Fact]
        public async Task AssignProductsToAdministrators_WithEmptyOrganizationRealPageId_ReturnsOkWithError()
        {
            var result = await _controller.AssignProductsToAdministrators(Guid.Empty, 100);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<Guid, IErrorData>>(okResult.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("User.AssignProductsToAdministrators.1", output.Status.ErrorCode);
        }

        [Fact]
        public async Task AssignProductsToAdministrators_WithNegativeAssignUserPersonaId_ReturnsOkWithError()
        {
            var organizationRealPageId = Guid.NewGuid();

            var result = await _controller.AssignProductsToAdministrators(organizationRealPageId, -1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<Guid, IErrorData>>(okResult.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("User.AssignProductsToAdministrators.2", output.Status.ErrorCode);
        }

        [Fact]
        public async Task AssignProductsToAdministrators_WithValidParameters_ReturnsOkResult()
        {
            var organizationRealPageId = Guid.NewGuid();

            var result = await _controller.AssignProductsToAdministrators(organizationRealPageId, 100);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task AssignProductsToAdministrators_WithZeroAssignUserPersonaId_ReturnsOkResult()
        {
            var organizationRealPageId = Guid.NewGuid();

            var result = await _controller.AssignProductsToAdministrators(organizationRealPageId, 0);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region GetUserProfile Tests

        [Fact]
        public async Task GetUserProfile_WithValidRealPageId_ReturnsOkResult()
        {
            var realPageId = Guid.NewGuid();

            var result = await _controller.GetUserProfile(realPageId);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetUserProfile_WithEmptyRealPageId_ReturnsOkResult()
        {
            var result = await _controller.GetUserProfile(Guid.Empty);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region GetUserProducts Tests

        [Fact]
        public async Task GetUserProducts_WithEmptyRealPageIdAndEmptyUserClaim_ReturnsBadRequest()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(new DefaultUserClaim
            {
                PersonaId = 100,
                LoginName = "test@test.com",
                UserRealPageGuid = Guid.Empty
            });

            var controller = new UserController(mockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.GetUserProducts(Guid.Empty);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<IProfileDetail, IErrorData>>(badRequestResult.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("User.GetUserProducts.1", output.Status.ErrorCode);
        }

        [Fact]
        public async Task GetUserProducts_WithValidRealPageId_ReturnsResult()
        {
            var realPageId = Guid.NewGuid();

            var result = await _controller.GetUserProducts(realPageId);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetUserProducts_WithEmptyRealPageIdButValidUserClaim_ReturnsResult()
        {
            var result = await _controller.GetUserProducts(Guid.Empty);

            Assert.NotNull(result);
        }

        #endregion

        #region GetUserProfileForClone Tests

        [Fact]
        public async Task GetUserProfileForClone_WithEmptyRealPageIdAndEmptyUserClaim_ReturnsOkWithError()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(new DefaultUserClaim
            {
                PersonaId = 100,
                LoginName = "test@test.com",
                UserRealPageGuid = Guid.Empty
            });

            var controller = new UserController(mockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.GetUserProfileForClone(Guid.Empty);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<IProfileDetail, IErrorData>>(okResult.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("User.GetProfile.1", output.Status.ErrorCode);
        }

        [Fact]
        public async Task GetUserProfileForClone_WithValidRealPageId_ReturnsOkResult()
        {
            var realPageId = Guid.NewGuid();

            var result = await _controller.GetUserProfileForClone(realPageId);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetUserProfileForClone_WithEmptyRealPageIdButValidUserClaim_ReturnsOkResult()
        {
            var result = await _controller.GetUserProfileForClone(Guid.Empty);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region UpdateUser Tests

        //[Fact]
        //public async Task UpdateUser_WithNullProfile_ReturnsOkWithError()
        //{
        //    var result = await _controller.UpdateUser(null!);

        //    var okResult = Assert.IsType<OkObjectResult>(result);
        //    var output = Assert.IsType<ObjectOutput<IProfileDetail, IErrorData>>(okResult.Value);
        //    Assert.False(output.Status.Success);
        //    Assert.Equal("User.UpdateUser.1", output.Status.ErrorCode);
        //}

        //[Fact]
        //public async Task UpdateUser_WithValidProfile_ReturnsResult()
        //{
        //    var profile = CreateValidProfileDetail();

        //    var result = await _controller.UpdateUser(profile);

        //    Assert.NotNull(result);
        //}

        //[Fact]
        //public async Task UpdateUser_WithEmptyFirstName_ReturnsOkWithError()
        //{
        //    var profile = CreateValidProfileDetail();
        //    profile.FirstName = "   ";

        //    var result = await _controller.UpdateUser(profile);

        //    var okResult = Assert.IsType<OkObjectResult>(result);
        //    var output = Assert.IsType<ObjectOutput<IProfileDetail, IErrorData>>(okResult.Value);
        //    Assert.False(output.Status.Success);
        //    Assert.Equal("User.UpdateUser.10", output.Status.ErrorCode);
        //}

        //[Fact]
        //public async Task UpdateUser_WithEmptyLastName_ReturnsOkWithError()
        //{
        //    var profile = CreateValidProfileDetail();
        //    profile.FirstName = "John";
        //    profile.LastName = "   ";

        //    var result = await _controller.UpdateUser(profile);

        //    var okResult = Assert.IsType<OkObjectResult>(result);
        //    var output = Assert.IsType<ObjectOutput<IProfileDetail, IErrorData>>(okResult.Value);
        //    Assert.False(output.Status.Success);
        //    Assert.Equal("User.UpdateUser.11", output.Status.ErrorCode);
        //}

        //[Fact]
        //public async Task UpdateUser_WithEmptyLoginName_ReturnsOkWithError()
        //{
        //    var profile = CreateValidProfileDetail();
        //    profile.FirstName = "John";
        //    profile.LastName = "Doe";
        //    profile.userLogin.LoginName = "   ";

        //    var result = await _controller.UpdateUser(profile);

        //    var okResult = Assert.IsType<OkObjectResult>(result);
        //    var output = Assert.IsType<ObjectOutput<IProfileDetail, IErrorData>>(okResult.Value);
        //    Assert.False(output.Status.Success);
        //    Assert.Equal("User.UpdateUser.4", output.Status.ErrorCode);
        //}

        //[Fact]
        //public async Task UpdateUser_WithNoFromDate_ReturnsOkWithError()
        //{
        //    var profile = CreateValidProfileDetail();
        //    profile.FirstName = "John";
        //    profile.LastName = "Doe";
        //    profile.userLogin.LoginName = "test@test.com";
        //    profile.userLogin.FromDate = null;

        //    var result = await _controller.UpdateUser(profile);

        //    var okResult = Assert.IsType<OkObjectResult>(result);
        //    var output = Assert.IsType<ObjectOutput<IProfileDetail, IErrorData>>(okResult.Value);
        //    Assert.False(output.Status.Success);
        //    Assert.Equal("User.UpdateUser.8", output.Status.ErrorCode);
        //}

        #endregion

        #region Validate Tests

        [Fact]
        public async Task Validate_WithEmptyEnterpriseUserName_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _controller.Validate("   ", "validToken"));
        }

        [Fact]
        public async Task Validate_WithEmptyNewUserRegistrationToken_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _controller.Validate("user@test.com", "   "));
        }

        [Fact]
        public async Task Validate_WithValidParameters_ReturnsResult()
        {
            var result = await _controller.Validate("user@test.com", "validToken");

            Assert.NotNull(result);
        }

        #endregion

        #region ValidateToken Tests

        [Fact]
        public async Task ValidateToken_WithEmptyEnterpriseUserName_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _controller.ValidateToken("   ", "validToken"));
        }

        [Fact]
        public async Task ValidateToken_WithEmptyVerificationToken_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _controller.ValidateToken("user@test.com", "   "));
        }

        [Fact]
        public async Task ValidateToken_WithValidParameters_ReturnsResult()
        {
            var result = await _controller.ValidateToken("user@test.com", "validToken");

            Assert.NotNull(result);
        }

        #endregion

        #region SetStarterProfile Tests

        [Fact]
        public async Task SetStarterProfile_WithNullStarterProfile_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _controller.SetStarterProfile(null!));
        }

        [Fact]
        public async Task SetStarterProfile_WithValidStarterProfile_ReturnsResult()
        {
            var starterProfile = new StarterProfile
            {
                ActivityToken = "token123",
                EnterpriseUserName = "user@test.com",
                CompanyJobTitle = "Developer",
                PhoneNumber = "1234567890"
            };

            var result = await _controller.SetStarterProfile(starterProfile);

            Assert.NotNull(result);
        }

        #endregion

        #region CreateUser Tests

        [Fact]
        public async Task CreateUser_WithNullProfile_ReturnsError()
        {
            var result = await _controller.CreateUser(null!);

            Assert.False(result.Status.Success);
            Assert.Equal("User.CreateUser.24", result.Status.ErrorCode);
        }

        [Fact]
        public async Task CreateUser_WithNullPersona_ReturnsError()
        {
            var profile = new ProfileDetail
            {
                FirstName = "John",
                LastName = "Doe",
                Persona = null!
            };

            var result = await _controller.CreateUser(profile);

            Assert.False(result.Status.Success);
            Assert.Equal("User.CreateUser.25", result.Status.ErrorCode);
        }

        [Fact]
        public async Task CreateUser_WithEmptyFirstName_ReturnsError()
        {
            var profile = CreateValidProfileDetailForCreate();
            profile.FirstName = "   ";

            var result = await _controller.CreateUser(profile);

            Assert.False(result.Status.Success);
            Assert.Equal("User.CreateUser.26", result.Status.ErrorCode);
        }

        [Fact]
        public async Task CreateUser_WithEmptyLastName_ReturnsError()
        {
            var profile = CreateValidProfileDetailForCreate();
            profile.LastName = "   ";

            var result = await _controller.CreateUser(profile);

            Assert.False(result.Status.Success);
            Assert.Equal("User.CreateUser.27", result.Status.ErrorCode);
        }

        [Fact]
        public async Task CreateUser_WithEmptyLoginName_ReturnsError()
        {
            var profile = CreateValidProfileDetailForCreate();
            profile.userLogin.LoginName = "   ";

            var result = await _controller.CreateUser(profile);

            Assert.False(result.Status.Success);
            Assert.Equal("User.CreateUser.28", result.Status.ErrorCode);
        }

        [Fact]
        public async Task CreateUser_WithNoFromDate_ReturnsError()
        {
            var profile = CreateValidProfileDetailForCreate();
            profile.userLogin.FromDate = null;

            var result = await _controller.CreateUser(profile);

            Assert.False(result.Status.Success);
            Assert.Equal("User.CreateUser.33", result.Status.ErrorCode);
        }

        #endregion

        #region UpdateNewUser Tests

        //[Fact]
        //public async Task UpdateNewUser_WithEmptyUserLogin_ReturnsError()
        //{
        //    var profile = new Profile();

        //    var result = await _controller.UpdateNewUser(profile, "", "title", "token");

        //    Assert.NotNull(result.ErrorMessage);
        //}

        //[Fact]
        //public async Task UpdateNewUser_WithEmptyCompanyJobTitle_ReturnsError()
        //{
        //    var profile = new Profile();

        //    var result = await _controller.UpdateNewUser(profile, "user@test.com", "", "token");

        //    Assert.NotNull(result.ErrorMessage);
        //}

        //[Fact]
        //public async Task UpdateNewUser_WithNullProfile_ReturnsError()
        //{
        //    var result = await _controller.UpdateNewUser(null!, "user@test.com", "title", "token");

        //    Assert.NotNull(result.ErrorMessage);
        //}

        [Fact]
        public async Task UpdateNewUser_WithEmptyActivityToken_ReturnsError()
        {
            var profile = new Profile
            {
                PartyRole = new PartyRole { RoleTypeId = 401 },
                TelecommunicationNumber = new List<TelecommunicationNumber>
                {
                    new TelecommunicationNumber { PhoneNumber = "1234567890" }
                }
            };

            var result = await _controller.UpdateNewUser(profile, "user@test.com", "title", "   ");

            Assert.NotNull(result.ErrorMessage);
        }

        [Fact]
        public async Task UpdateNewUser_WithInvalidPartyRoleTypeId_ReturnsError()
        {
            var profile = new Profile
            {
                PartyRole = new PartyRole { RoleTypeId = 0 },
                TelecommunicationNumber = new List<TelecommunicationNumber>
                {
                    new TelecommunicationNumber { PhoneNumber = "1234567890" }
                }
            };

            var result = await _controller.UpdateNewUser(profile, "user@test.com", "title", "token");

            Assert.NotNull(result.ErrorMessage);
        }

        [Fact]
        public async Task UpdateNewUser_WithNoTelecommunicationNumber_ReturnsError()
        {
            var profile = new Profile
            {
                PartyRole = new PartyRole { RoleTypeId = 401 },
                TelecommunicationNumber = new List<TelecommunicationNumber>()
            };

            var result = await _controller.UpdateNewUser(profile, "user@test.com", "title", "token");

            Assert.NotNull(result.ErrorMessage);
        }

        #endregion

        #region UserCustomFields Tests

        [Fact]
        public async Task UserCustomFields_WithNullUserLoginPersonaId_ReturnsOkResult()
        {
            var result = await _controller.UserCustomFields(null);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UserCustomFields_WithValidUserLoginPersonaId_ReturnsOkResult()
        {
            var result = await _controller.UserCustomFields(100);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region GetCurrentUserRights Tests

        [Fact]
        public async Task GetCurrentUserRights_ReturnsOkResult()
        {
            var result = await _controller.GetCurrentUserRights();

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public async Task GetUserProfile_WithMaxGuidRealPageId_ReturnsOkResult()
        {
            var realPageId = new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff");

            var result = await _controller.GetUserProfile(realPageId);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task AssignProductsToAdministrators_WithMaxLongPersonaId_ReturnsOkResult()
        {
            var organizationRealPageId = Guid.NewGuid();

            var result = await _controller.AssignProductsToAdministrators(organizationRealPageId, long.MaxValue);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region Helper Methods

        private static ProfileDetail CreateValidProfileDetail()
        {
            return new ProfileDetail
            {
                RealPageId = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                userLogin = new UserLogin
                {
                    LoginName = "john.doe@test.com",
                    FromDate = DateTime.UtcNow
                },
                UserTypeId = 401,
                Persona = new List<Persona>
                {
                    new Persona
                    {
                        PersonaId = 100,
                        Organization = new Organization
                        {
                            PartyId = 1000,
                            RealPageId = Guid.NewGuid(),
                            BooksCustomerMasterId = 12345
                        }
                    }
                },
                organization = new List<Organization>
                {
                    new Organization
                    {
                        PartyId = 1000,
                        RealPageId = Guid.NewGuid(),
                        BooksCustomerMasterId = 12345
                    }
                },
                productBatch = new List<ProductBatch>()
            };
        }

        private static ProfileDetail CreateValidProfileDetailForCreate()
        {
            return new ProfileDetail
            {
                FirstName = "John",
                LastName = "Doe",
                userLogin = new UserLogin
                {
                    LoginName = "john.doe@test.com",
                    FromDate = DateTime.UtcNow
                },
                UserTypeId = 401,
                Persona = new List<Persona>
                {
                    new Persona
                    {
                        PersonaId = 100,
                        Organization = new Organization
                        {
                            PartyId = 1000,
                            RealPageId = Guid.NewGuid()
                        }
                    }
                },
                organization = new List<Organization>
                {
                    new Organization
                    {
                        PartyId = 1000,
                        RealPageId = Guid.NewGuid()
                    }
                },
                productBatch = new List<ProductBatch>()
            };
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
