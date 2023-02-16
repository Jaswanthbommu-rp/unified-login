using Moq;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using RP.Enterprise.Foundation.DataAccess.Component;
using Xunit;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic
{
	[ExcludeFromCodeCoverage]
	public class ManageProfileTest : ManageProductBaseTests
	{
        IManageProfile _profileLogic;
        Mock<IProfileRepository> _mockProfileRepository = new Mock<IProfileRepository>();
		Mock<IManagePersona> _mockPersonaLogic = new Mock<IManagePersona>();
        private Mock<IRepository> _mockRepository = new Mock<IRepository>();
		
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

            _mockRepository
                .Setup(m => m.GetOne<long>(StoredProcNameConstants.SP_GetActivePersona,
                    It.Is<Guid>(l => l == _userRealPageId)
                ))
                .Returns(_userPersona.PersonaId);

            _mockRepository
                .Setup(m => m.GetMany<ProductUI>(StoredProcNameConstants.SP_ListProductsByOrganization,
                    It.Is<object>(
                        d => TestIs("OrganizationRealPageId".ToLower(), d, _userOrganizationRealPageId))
                ))
                .Returns(productUIList);

			globals.Add(BaseType.RequestParameter, datafilter);

            _profileLogic = new ManageProfile(_mockRepository.Object, _userUserClaim);

            new RPObjectCache().BustCache();

			//Act
			var profileDetailList = _profileLogic.ListProfileDetails(globals);

			//Assert
			Assert.True(profileDetailList.Count == 0);
		}

        //[Fact]
        public void ListProfileDetails_MockInputData_ReturnValidRepositoryResponseObject()
        {
            //Arrange
            RequestParameter datafilter = new RequestParameter();
            IList<ProfileDetail> profileDetailList = new List<ProfileDetail>();
            IList<ProfileDetail> expectedProfileDetailList = new List<ProfileDetail>();

			_mockPersonaLogic
				.Setup(m => m.GetActivePersona(
					It.Is<Guid>(l => l == _userRealPageId)
				))
				.Returns(_userPersona);

			UserLogin userLogin = new UserLogin()
            {
                LoginName = "john.doe@realpage.com",
                PartyId = 1,
                RealPageId = new Guid("1aafde71-cf3a-46d8-9d73-08c6deccc92b"),
                UserId = 1
            };
            IDictionary<object, object> globals = new Dictionary<object, object>();
            globals.Add(BaseType.RequestParameter, datafilter);

            expectedProfileDetailList.Add(new ProfileDetail()
            {
                FirstName = "John",
                LastName = "Doe",
                PartyId = 1,
                RealPageId = new Guid("1aafde71-cf3a-46d8-9d73-08c6deccc92b"),
				CreateUserSourceType = Component.SharedObjects.Enum.CreateUserSourceType.UnifiedPlatform,
                userLogin = userLogin
            });

            _mockProfileRepository
                .Setup(m => m.ListPersons(null, null, null, null,false))
                .Returns(expectedProfileDetailList);

            //_profileLogic = new ManageProfile(_mockProfileRepository.Object, _mockProductRepository.Object, _mockPersonaLogic.Object, _mockPersonLogic.Object, _mockUserLoginLogic.Object, _mockOrganizationLogic.Object, _mockPartyRelationshipLogic.Object, _mockContactMechanismLogic.Object, _mockPartyRoleLogic.Object, _userUserClaim);
            _profileLogic = new ManageProfile(_mockRepository.Object, _userUserClaim);

            //Act
            profileDetailList = _profileLogic.ListProfileDetails(globals);

            //Assert
            Assert.True(
                profileDetailList != null
                && profileDetailList.Count() == 1
           );
        }
    }
}