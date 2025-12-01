using Moq;
using UnifiedLogin.BusinessLogic;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Xunit;
using System.Net.Http;
using UnifiedLogin.SharedObjects.BlackBook;

namespace UnifiedLogin.LandingAPI.Test.Logic
{
	[ExcludeFromCodeCoverage]
	public class ManageProfileTest : ManageProductBaseTests
	{
        IManageProfile _profileLogic;
        Mock<IProfileRepository> _mockProfileRepository = new Mock<IProfileRepository>();
		Mock<IManagePersona> _mockPersonaLogic = new Mock<IManagePersona>();
		
		public ManageProfileTest() : base((int) ProductEnum.UnifiedPlatform)
		{
			_productSettingType.Add(new ProductSettingType() { ProductSettingTypeId = 1, Name = "ProductStatus" });

			_repositoryResponseProductStatus = new RepositoryResponse() { ErrorMessage = "", Id = 1 };
			_repositoryResponsePropertySuccess = new RepositoryResponse() { ErrorMessage = "", Id = 1 };
			_repositoryResponsePropertyFail = new RepositoryResponse() { ErrorMessage = "error", Id = -1 };
		}

		[Fact]
        public void ListProfileDetails_InvalidProfileDetailList_EmptyList()
        {
            //Arrange
            RequestParameter datafilter = new RequestParameter();
            IDictionary<object, object> globals = new Dictionary<object, object>();

            var productUIList = new List<ProductUI>()
            {
                new PersonaProductUserDetails() { ProductId = (int)ProductEnum.OneSite},
                new PersonaProductUserDetails() { ProductId = (int)ProductEnum.FinancialSuite},
                new PersonaProductUserDetails() { ProductId = (int)ProductEnum.ProspectContactCenter}
            };

            var userLoginPersonaList = new List<UserLoginPersona>() { new UserLoginPersona()
            {
                PrimaryOrganization = true,
                IsDelegateAdmin = false,
                UserLoginId = _userUserId,
                UserLoginPersonaId = 12345
            }};

            var externalUserRelationship = new ExternalUserRelationship()
            {
                UserLoginPersonaId = 12345,
                OperatorCode = null,
                OperatorValue = null,
                ThirdPartyRelationShipId = 1
            };

            mockRepository
                .Setup(m => m.GetOne<long>(StoredProcNameConstants.SP_GetActivePersona,
                    It.Is<Guid>(l => l == _userRealPageId)
                ))
                .Returns(_userPersona.PersonaId);

            mockRepository
                .Setup(m => m.GetMany<ProductUI>(StoredProcNameConstants.SP_ListProductsByOrganization,
                    It.Is<object>(
                        d => TestIs("OrganizationRealPageId".ToLower(), d, _userOrganizationRealPageId))
                ))
                .Returns(productUIList);

            mockRepository
                .Setup(m => m.GetMany<UserLoginPersona>(StoredProcNameConstants.SP_GetUserLoginPersona,
                It.Is<object>(
                        d => TestSqlParameter(d, "{ UserLoginId = " + _userUserId + ", OrganizationPartyId = 0 }"))
                ))
                .Returns(userLoginPersonaList);

            mockRepository
                .Setup(m => m.GetOne<ExternalUserRelationship>(StoredProcNameConstants.SP_GetExternalUserRelationship,
                    It.Is<object>(
                        d => TestSqlParameter(d, "{ UserLoginPersonaId = " + userLoginPersonaList[0].UserLoginPersonaId + " }"))
                ))
                .Returns(externalUserRelationship);

            globals.Add(BaseType.RequestParameter, datafilter);
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            _profileLogic = new ManageProfile(mockRepository.Object, _userUserClaim, mockHttpMessageHandler.Object);

            new RPObjectCache().BustCache();

			//Act
			var profileDetailList = _profileLogic.ListProfileDetails(globals);

			//Assert
			Assert.True(profileDetailList.Count == 0);
		}

        [Fact]
        public void ListProfileDetails_MockInputData_ReturnValidUserListOneUser()
        {
            //Arrange
            RequestParameter datafilter = new RequestParameter();
            IList<ProfileDetail> profileDetailList = new List<ProfileDetail>();
            IList<ProfileDetail> expectedProfileDetailList = new List<ProfileDetail>();

            var productUIList = new List<ProductUI>()
            {
                new PersonaProductUserDetails() { ProductId = (int)ProductEnum.OneSite},
                new PersonaProductUserDetails() { ProductId = (int)ProductEnum.FinancialSuite},
                new PersonaProductUserDetails() { ProductId = (int)ProductEnum.ProspectContactCenter}
            };

            var userLoginPersonaList = new List<UserLoginPersona>() { new UserLoginPersona()
            {
                PrimaryOrganization = true,
                IsDelegateAdmin = false,
                UserLoginId = _userUserId,
                UserLoginPersonaId = 12345
            }};

            var externalUserRelationship = new ExternalUserRelationship()
            {
                UserLoginPersonaId = 12345,
                OperatorCode = null,
                OperatorValue = null,
                ThirdPartyRelationShipId = 1
            };

            mockRepository
                .Setup(m => m.GetOne<long>(StoredProcNameConstants.SP_GetActivePersona,
                    It.Is<Guid>(l => l == _userRealPageId)
                ))
                .Returns(_userPersona.PersonaId);

            mockRepository
                .Setup(m => m.GetMany<ProductUI>(StoredProcNameConstants.SP_ListProductsByOrganization,
                    It.Is<object>(
                        d => TestIs("OrganizationRealPageId".ToLower(), d, _userOrganizationRealPageId))
                ))
                .Returns(productUIList);

            mockRepository
                .Setup(m => m.GetMany<UserLoginPersona>(StoredProcNameConstants.SP_GetUserLoginPersona,
                    It.Is<object>(
                        d => TestSqlParameter(d, "{ UserLoginId = " + _userUserId + ", OrganizationPartyId = 0 }"))
                ))
                .Returns(userLoginPersonaList);

            mockRepository
                .Setup(m => m.GetOne<ExternalUserRelationship>(StoredProcNameConstants.SP_GetExternalUserRelationship,
                    It.Is<object>(
                        d => TestSqlParameter(d, "{ UserLoginPersonaId = " + userLoginPersonaList[0].UserLoginPersonaId + " }"))
                ))
                .Returns(externalUserRelationship);

            mockRepository
                .Setup(m => m.GetOne<ExternalUserRelationship>(StoredProcNameConstants.SP_GetExternalUserRelationship,
                    It.Is<object>(
                        d => TestSqlParameter(d, "{ UserLoginPersonaId = " + userLoginPersonaList[0].UserLoginPersonaId + " }"))
                ))
                .Returns(externalUserRelationship);

            mockRepository
                .Setup(m => m.GetManyWithSpliOn(
                    StoredProcNameConstants.SP_ListPersons,
                    It.IsAny<Func<ProfileDetail, UserLogin, int, string, ProfileDetail>>(),
                    It.Is<object>(d => TestSqlParameter(d, "{ RealPageId = " + _userOrganizationRealPageId + ", ParentPartyRoleTypeId = 400, UserListFilterType = 2, AssignedProducts = {\"assignedProducts\":[{\"ColumnName\":\"ProductId\",\"SearchValue\":\"1,8,10\"}]}, FilterBy = , SortBy = , RowsPerPage = 0, PageNumber = 1 }")),
                    It.IsAny<string>()
                ))
                .Returns(new List<ProfileDetail>() { new ProfileDetail() { FirstName = "John", LastName = "Doe",  } });

            var userLogin = new UserLogin()
            {
                LoginName = "john.doe@realpage.com",
                PartyId = 1,
                RealPageId = new Guid("1aafde71-cf3a-46d8-9d73-08c6deccc92b"),
                UserId = 1
            };
            var globals = new Dictionary<object, object> { { BaseType.RequestParameter, datafilter } };

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

_profileLogic = new ManageProfile(mockRepository.Object, _userUserClaim, mockHttpMessageHandler.Object);

            //Act
            profileDetailList = _profileLogic.ListProfileDetails(globals);

            //Assert
            Assert.True(
                profileDetailList != null
                && profileDetailList.Count() == 1
           );
        }

        [Fact]
        public void ListProfileDetails_ReturnDelegatedUserList_NoFilter_TwoUsers()
        {
            //Arrange
            var productUIList = new List<ProductUI>()
            {
                new PersonaProductUserDetails() { ProductId = (int)ProductEnum.OneSite },
                new PersonaProductUserDetails() { ProductId = (int)ProductEnum.FinancialSuite },
                new PersonaProductUserDetails() { ProductId = (int)ProductEnum.ProspectContactCenter }
            };

            var userLoginPersonaList = new List<UserLoginPersona>()
            {
                new UserLoginPersona()
                {
                    PrimaryOrganization = true,
                    IsDelegateAdmin = false,
                    UserLoginId = _userUserId,
                    UserLoginPersonaId = 12345
                }
            };

            var externalUserRelationship = new ExternalUserRelationship()
            {
                UserLoginPersonaId = 12345,
                OperatorCode = "OperatorCode",
                OperatorValue = "OperatorName",
                ThirdPartyRelationShipId = 123
            };

            mockRepository
                .Setup(m => m.GetOne<long>(StoredProcNameConstants.SP_GetActivePersona,
                    It.Is<Guid>(l => l == _userRealPageId)
                ))
                .Returns(_userPersona.PersonaId);

            mockRepository
                .Setup(m => m.GetMany<ProductUI>(StoredProcNameConstants.SP_ListProductsByOrganization,
                    It.Is<object>(
                        d => TestIs("OrganizationRealPageId".ToLower(), d, _userOrganizationRealPageId))
                ))
                .Returns(productUIList);

            mockRepository
                .Setup(m => m.GetMany<UserLoginPersona>(StoredProcNameConstants.SP_GetUserLoginPersona,
                    It.Is<object>(
                        d => TestSqlParameter(d, "{ UserLoginId = " + _userUserId + ", OrganizationPartyId = 0 }"))
                ))
                .Returns(userLoginPersonaList);

            mockRepository
                .Setup(m => m.GetOne<ExternalUserRelationship>(StoredProcNameConstants.SP_GetExternalUserRelationship,
                    It.Is<object>(
                        d => TestSqlParameter(d, "{ UserLoginPersonaId = " + userLoginPersonaList[0].UserLoginPersonaId + " }"))
                ))
                .Returns(externalUserRelationship);

            mockRepository
                .Setup(m => m.GetOne<ExternalUserRelationship>(StoredProcNameConstants.SP_GetExternalUserRelationship,
                    It.Is<object>(
                        d => TestSqlParameter(d, "{ UserLoginPersonaId = " + userLoginPersonaList[0].UserLoginPersonaId + " }"))
                ))
                .Returns(externalUserRelationship);

            mockRepository
                .Setup(m => m.GetManyWithSpliOn(
                    StoredProcNameConstants.SP_ListPersons,
                    It.IsAny<Func<ProfileDetail, UserLogin, int, string, ProfileDetail>>(),
                    It.Is<object>(d => TestSqlParameter(d, "{ RealPageId = " + _userOrganizationRealPageId + ", ParentPartyRoleTypeId = 400, UserListFilterType = 3, AssignedProducts = {\"assignedProducts\":[{\"ColumnName\":\"ProductId\",\"SearchValue\":\"1,8,10\"}]}, FilterBy = {\"filterBy\":[{\"ColumnName\":\"Operator\",\"SearchValue\":\"OperatorCode|OperatorName\"},{\"ColumnName\":\"userType\",\"SearchValue\":\"405\"}]}, SortBy = , RowsPerPage = 0, PageNumber = 1 }")),
                    It.IsAny<string>()
                ))
                .Returns(new List<ProfileDetail>() { new ProfileDetail() { FirstName = "John", LastName = "Doe", Operator = "OperatorValue" }, new ProfileDetail() { FirstName = "Jane", LastName = "Doe", Operator = "OperatorValue" } });

            var globals = new Dictionary<object, object> { { BaseType.RequestParameter, null } };

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            _profileLogic = new ManageProfile(mockRepository.Object, _userUserClaim, mockHttpMessageHandler.Object);

            //Act
            var profileDetailList = _profileLogic.ListProfileDetails(globals);

            //Assert
            Assert.True(
                profileDetailList != null
                && profileDetailList.Count() == 2
                && profileDetailList[0].Operator == "OperatorValue"
            );
        }

        [Fact]
        public void ListProfileDetails_ReturnDelegatedUserList_OverrideExistingFilter()
        {
            //Arrange
            var datafilter = new RequestParameter
            {
                SortBy = new Dictionary<string, string> { { "Name", "ASC" } },
                FilterBy = new Dictionary<string, string> { { "UserType", "401" }, { "Operator", "Wrong Operator" } }
            };

            IList<ProfileDetail> profileDetailList = new List<ProfileDetail>();
            IList<ProfileDetail> expectedProfileDetailList = new List<ProfileDetail>();

            var productUIList = new List<ProductUI>()
            {
                new PersonaProductUserDetails() { ProductId = (int)ProductEnum.OneSite },
                new PersonaProductUserDetails() { ProductId = (int)ProductEnum.FinancialSuite },
                new PersonaProductUserDetails() { ProductId = (int)ProductEnum.ProspectContactCenter }
            };

            var userLoginPersonaList = new List<UserLoginPersona>()
            {
                new UserLoginPersona()
                {
                    PrimaryOrganization = true,
                    IsDelegateAdmin = false,
                    UserLoginId = _userUserId,
                    UserLoginPersonaId = 12345
                }
            };

            var externalUserRelationship = new ExternalUserRelationship()
            {
                UserLoginPersonaId = 12345,
                OperatorCode = "OperatorCode",
                OperatorValue = "OperatorName",
                ThirdPartyRelationShipId = 123
            };

            mockRepository
                .Setup(m => m.GetOne<long>(StoredProcNameConstants.SP_GetActivePersona,
                    It.Is<Guid>(l => l == _userRealPageId)
                ))
                .Returns(_userPersona.PersonaId);

            mockRepository
                .Setup(m => m.GetMany<ProductUI>(StoredProcNameConstants.SP_ListProductsByOrganization,
                    It.Is<object>(
                        d => TestIs("OrganizationRealPageId".ToLower(), d, _userOrganizationRealPageId))
                ))
                .Returns(productUIList);

            mockRepository
                .Setup(m => m.GetMany<UserLoginPersona>(StoredProcNameConstants.SP_GetUserLoginPersona,
                    It.Is<object>(
                        d => TestSqlParameter(d, "{ UserLoginId = " + _userUserId + ", OrganizationPartyId = 0 }"))
                ))
                .Returns(userLoginPersonaList);

            mockRepository
                .Setup(m => m.GetOne<ExternalUserRelationship>(StoredProcNameConstants.SP_GetExternalUserRelationship,
                    It.Is<object>(
                        d => TestSqlParameter(d, "{ UserLoginPersonaId = " + userLoginPersonaList[0].UserLoginPersonaId + " }"))
                ))
                .Returns(externalUserRelationship);

            mockRepository
                .Setup(m => m.GetOne<ExternalUserRelationship>(StoredProcNameConstants.SP_GetExternalUserRelationship,
                    It.Is<object>(
                        d => TestSqlParameter(d, "{ UserLoginPersonaId = " + userLoginPersonaList[0].UserLoginPersonaId + " }"))
                ))
                .Returns(externalUserRelationship);

            mockRepository
                .Setup(m => m.GetManyWithSpliOn(
                    StoredProcNameConstants.SP_ListPersons,
                    It.IsAny<Func<ProfileDetail, UserLogin, int, string, ProfileDetail>>(),
                    It.Is<object>(d => TestSqlParameter(d, "{ RealPageId = " + _userOrganizationRealPageId + ", ParentPartyRoleTypeId = 400, UserListFilterType = 3, AssignedProducts = {\"assignedProducts\":[{\"ColumnName\":\"ProductId\",\"SearchValue\":\"1,8,10\"}]}, FilterBy = {\"filterBy\":[{\"ColumnName\":\"userType\",\"SearchValue\":\"405\"},{\"ColumnName\":\"Operator\",\"SearchValue\":\"OperatorCode|OperatorName\"}]}, SortBy = {\"sortBy\":[{\"ColumnName\":\"Name\",\"SortDirection\":\"ASC\"}]}, RowsPerPage = 0, PageNumber = 1 }")),
                    It.IsAny<string>()
                ))
                .Returns(new List<ProfileDetail>() { new ProfileDetail() { FirstName = "John", LastName = "Doe", Operator = "OperatorValue" }, new ProfileDetail() { FirstName = "Jane", LastName = "Doe", Operator = "OperatorValue" } });

            var userLogin = new UserLogin()
            {
                LoginName = "john.doe@realpage.com",
                PartyId = 1,
                RealPageId = new Guid("1aafde71-cf3a-46d8-9d73-08c6deccc92b"),
                UserId = 1
            };
            var globals = new Dictionary<object, object> { { BaseType.RequestParameter, datafilter } };

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

_profileLogic = new ManageProfile(mockRepository.Object, _userUserClaim, mockHttpMessageHandler.Object);

            //Act
            profileDetailList = _profileLogic.ListProfileDetails(globals);

            //Assert
            Assert.True(
                profileDetailList != null
                && profileDetailList.Count() == 2
                && profileDetailList[0].Operator == "OperatorValue"
            );
        }

        [Fact]
        public void ListProfileDetails_ReturnDelegatedUserList_AppendToExistingFilter()
        {
            //Arrange
            var datafilter = new RequestParameter
            {
                SortBy = new Dictionary<string, string> { { "Name", "ASC" } },
            };

            IList<ProfileDetail> profileDetailList = new List<ProfileDetail>();

            var productUIList = new List<ProductUI>()
            {
                new PersonaProductUserDetails() { ProductId = (int)ProductEnum.OneSite },
                new PersonaProductUserDetails() { ProductId = (int)ProductEnum.FinancialSuite },
                new PersonaProductUserDetails() { ProductId = (int)ProductEnum.ProspectContactCenter }
            };

            var userLoginPersonaList = new List<UserLoginPersona>()
            {
                new UserLoginPersona()
                {
                    PrimaryOrganization = true,
                    IsDelegateAdmin = false,
                    UserLoginId = _userUserId,
                    UserLoginPersonaId = 12345
                }
            };

            var externalUserRelationship = new ExternalUserRelationship()
            {
                UserLoginPersonaId = 12345,
                OperatorCode = "OperatorCode",
                OperatorValue = "OperatorName",
                ThirdPartyRelationShipId = 123
            };

            mockRepository
                .Setup(m => m.GetOne<long>(StoredProcNameConstants.SP_GetActivePersona,
                    It.Is<Guid>(l => l == _userRealPageId)
                ))
                .Returns(_userPersona.PersonaId);

            mockRepository
                .Setup(m => m.GetMany<ProductUI>(StoredProcNameConstants.SP_ListProductsByOrganization,
                    It.Is<object>(
                        d => TestIs("OrganizationRealPageId".ToLower(), d, _userOrganizationRealPageId))
                ))
                .Returns(productUIList);

            mockRepository
                .Setup(m => m.GetMany<UserLoginPersona>(StoredProcNameConstants.SP_GetUserLoginPersona,
                    It.Is<object>(
                        d => TestSqlParameter(d, "{ UserLoginId = " + _userUserId + ", OrganizationPartyId = 0 }"))
                ))
                .Returns(userLoginPersonaList);

            mockRepository
                .Setup(m => m.GetOne<ExternalUserRelationship>(StoredProcNameConstants.SP_GetExternalUserRelationship,
                    It.Is<object>(
                        d => TestSqlParameter(d, "{ UserLoginPersonaId = " + userLoginPersonaList[0].UserLoginPersonaId + " }"))
                ))
                .Returns(externalUserRelationship);

            mockRepository
                .Setup(m => m.GetOne<ExternalUserRelationship>(StoredProcNameConstants.SP_GetExternalUserRelationship,
                    It.Is<object>(
                        d => TestSqlParameter(d, "{ UserLoginPersonaId = " + userLoginPersonaList[0].UserLoginPersonaId + " }"))
                ))
                .Returns(externalUserRelationship);

            mockRepository
                .Setup(m => m.GetManyWithSpliOn(
                    StoredProcNameConstants.SP_ListPersons,
                    It.IsAny<Func<ProfileDetail, UserLogin, int, string, ProfileDetail>>(),
                    It.Is<object>(d => TestSqlParameter(d, "{ RealPageId = " + _userOrganizationRealPageId + ", ParentPartyRoleTypeId = 400, UserListFilterType = 3, AssignedProducts = {\"assignedProducts\":[{\"ColumnName\":\"ProductId\",\"SearchValue\":\"1,8,10\"}]}, FilterBy = {\"filterBy\":[{\"ColumnName\":\"Operator\",\"SearchValue\":\"OperatorCode|OperatorName\"},{\"ColumnName\":\"userType\",\"SearchValue\":\"405\"}]}, SortBy = {\"sortBy\":[{\"ColumnName\":\"Name\",\"SortDirection\":\"ASC\"}]}, RowsPerPage = 0, PageNumber = 1 }")),
                    It.IsAny<string>()
                ))
                .Returns(new List<ProfileDetail>() { new ProfileDetail() { FirstName = "John", LastName = "Doe", Operator = "OperatorValue" } });

            var userLogin = new UserLogin()
            {
                LoginName = "john.doe@realpage.com",
                PartyId = 1,
                RealPageId = new Guid("1aafde71-cf3a-46d8-9d73-08c6deccc92b"),
                UserId = 1
            };
            var globals = new Dictionary<object, object> { { BaseType.RequestParameter, datafilter } };

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            _profileLogic = new ManageProfile(mockRepository.Object, _userUserClaim, mockHttpMessageHandler.Object);

            //Act
            profileDetailList = _profileLogic.ListProfileDetails(globals);

            //Assert
            Assert.True(
                profileDetailList != null
                && profileDetailList.Count() == 1
                && profileDetailList[0].Operator == "OperatorValue"
            );
        }
    }
}