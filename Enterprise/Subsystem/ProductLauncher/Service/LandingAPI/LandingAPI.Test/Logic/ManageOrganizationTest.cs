using Moq;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using Xunit;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic
{
    [ExcludeFromCodeCoverage]
    /// <summary>
    /// ManageOrganization Unit tests
    /// </summary>
    public class ManageOrganizationTest
    {
        private static Guid _RealPageId = new Guid("C802694D-5553-4527-8616-3C0F434AE62D");
        private static Guid _adminRealPageId = new Guid("C802694D-1111-2222-3333-3C0F434AE62D");
        private static string _CompanyName = "Test Company";
        private static DateTime _CreateDate = DateTime.MaxValue.ToUniversalTime();
        private static int _PartyId = 54321;
        private static long _BooksMasterId = 12345;
        private static long _BooksCompanyMasterId = 12345;
		private static int _organizationTypeId = 6;
        private static int _organizationDomainId = 1;
		private static string _organizationTypeName = "Multifamily";

        private Organization _organization = null;
        private List<Organization> _organizationList = null;
        private List<OrganizationType> _organizationTypes = null;
        private List<OrganizationDomain> _organizationDomains = null;
        private List<IdentityProviderType> _identityProviderTypes = null;

        private Mock<IRepository> _mockRepository = new Mock<IRepository>();
        private Mock<IUnitOfWork> _mockUnitofWork = new Mock<IUnitOfWork>();

        private DefaultUserClaim _defaultUserClaim = new DefaultUserClaim();
        Mock<HttpMessageHandler> mockMessageHandler = new Mock<HttpMessageHandler>();

        public ManageOrganizationTest()
        {
            _defaultUserClaim.CorrelationId = new Guid();

            _organization = new Organization()
            {
                RealPageId = _RealPageId,
                CreateDate = _CreateDate,
                Name = _CompanyName,
                PartyId = _PartyId,
                BooksMasterId = _BooksMasterId,
                BooksCustomerMasterId = _BooksCompanyMasterId,
                OrganizationTypeId = _organizationTypeId,
                organizationType = new OrganizationType()
                {
                    OrganizationTypeId = _organizationTypeId
                },
                OrganizationDomain = new OrganizationDomain()
                {
                    OrganizationDomainId = _organizationDomainId
                }
            };

            _organizationList = new List<Organization>() {_organization};

            _organizationTypes = new List<OrganizationType>()
            {
                new OrganizationType()
                {
                    OrganizationTypeId = 6,
                    Name = "Multifamily",
                    CreateDate = new DateTime()
                },
                new OrganizationType()
                {
                    OrganizationTypeId = 14,
                    Name = "Vendor",
                    CreateDate = new DateTime()
                },
                new OrganizationType()
                {
                    OrganizationTypeId = 7,
                    Name = "Other",
                    CreateDate = new DateTime()
                }
            };

            _organizationDomains = new List<OrganizationDomain>()
            {
                new OrganizationDomain()
                {
                    OrganizationDomainId = 1,
                    Name = "Primary",
                    CreateDate = new DateTime()
                }
            };

            _identityProviderTypes = new List<IdentityProviderType>() {new IdentityProviderType() {ContactMechanismId = 1000, AuthenticationType = "id3"}, new IdentityProviderType() {ContactMechanismId = 1001, AuthenticationType = "aad"}};

            List<DatabaseResult> orgList = new List<DatabaseResult>();
            orgList.Add(new DatabaseResult() {PartyId = "2345", PersonRealPageId = _RealPageId.ToString()});

            #region Set up the mocks

            _mockRepository.Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization, It.IsAny<object>()))
                .Returns(_organization);

            _mockRepository.Setup(m => m.GetMany<Organization>(StoredProcNameConstants.SP_GetOrganization, It.IsAny<object>()))
                .Returns(_organizationList);

            _mockRepository.Setup(m => m.GetMany<OrganizationType>(StoredProcNameConstants.SP_ListOrganizationType, null))
                .Returns(_organizationTypes);

            _mockRepository.Setup(m => m.GetMany<OrganizationDomain>(StoredProcNameConstants.SP_ListOrganizationDomain, null))
                .Returns(_organizationDomains);

            _mockRepository.Setup(m => m.GetMany<IdentityProviderType>(StoredProcNameConstants.SP_GetOrganizationIdentityProviderType, It.IsAny<object>()))
                .Returns(_identityProviderTypes);

            _mockRepository.Setup(m => m.GetMany<dynamic>(StoredProcNameConstants.SP_ListOrganizations, It.IsAny<object>()))
                .Returns(orgList);

            _mockRepository.Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdateOrganization, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = _PartyId, ErrorMessage = "", RealPageId = _RealPageId });

            _mockRepository.Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_InsertOrganization, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = _PartyId, ErrorMessage = "", RealPageId = _RealPageId });

            _mockRepository.Setup(m => m.UnitOfWork).Returns(_mockUnitofWork.Object);
            #endregion
            
        }

        #region Unit Tests
   //     [Fact]
   //     public void CreateOrganization_InvalidRealPageId_ExceptionThrown()
   //     {

   //         //Arrange
   //        // IManageOrganization manageOrganization = new ManageOrganization(_defaultUserClaim);
   //         IManageOrganization manageOrganization = new ManageOrganization(_mockRepository.Object, _defaultUserClaim, mockMessageHandler.Object);
   //         //Act
   //         Guid emptyRealPageId = Guid.Empty;
   //         Organization organization = new Organization()
   //         {
   //             RealPageId = emptyRealPageId,
   //             CreateDate = _CreateDate,
   //             Name = _CompanyName,
   //             PartyId = _PartyId,
   //             OrganizationTypeId = _organizationTypeId,
   //             organizationType = new OrganizationType()
			//	{
			//		OrganizationTypeId = _organizationTypeId
			//	},
   //             OrganizationDomainId = _organizationDomainId,
   //             OrganizationDomain = new OrganizationDomain()
   //             {
   //                 OrganizationDomainId = _organizationDomainId
   //             }
			//};

   //         //Assert
   //         Assert.Throws<Exception>(() => manageOrganization.InsertOrganization(organization));
   //     }

        [Fact]
        public void CreateOrganization_InvalidOrganization_ExceptionThrown()
        {
            //Arrange
            // IManageOrganization manageOrganization = new ManageOrganization(_defaultUserClaim);
            IManageOrganization manageOrganization = new ManageOrganization(_mockRepository.Object, _defaultUserClaim, mockMessageHandler.Object);
            //Act

            //Assert
            Assert.Throws<ArgumentNullException>(() => manageOrganization.InsertOrganization(null));
		}

        [Fact]
        public void UpdateOrganization_InvalidRealPage_ExceptionThrown()
        {
            //Arrange
            //IManageOrganization manageOrganization = new ManageOrganization();
            IManageOrganization manageOrganization = new ManageOrganization(_mockRepository.Object, _defaultUserClaim, mockMessageHandler.Object);

            //Act
            Guid emptyRealPageId = Guid.Empty;
            Organization organization = new Organization()
            {
                RealPageId = emptyRealPageId,
                CreateDate = _CreateDate,
                Name = _CompanyName,
                PartyId = _PartyId,
                OrganizationTypeId = _organizationTypeId,
                organizationType = new OrganizationType()
				{
					OrganizationTypeId = _organizationTypeId
				},
                OrganizationDomain = new OrganizationDomain()
                {
                    OrganizationDomainId = _organizationDomainId
                }
			};

            //Assert
            Assert.Throws<Exception>(() => manageOrganization.UpdateOrganization(organization));
        }

        [Fact]
        public void GetOrganization_InvalidRealPageId_ExceptionThrown()
        {
            //Arrange
            //IManageOrganization manageOrganization = new ManageOrganization();
            IManageOrganization manageOrganization = new ManageOrganization(_mockRepository.Object, _defaultUserClaim, mockMessageHandler.Object);

            //Act
            Guid emptyRealPageId = Guid.Empty;

            //Assert
            Assert.Throws<Exception>(() => manageOrganization.GetOrganization(emptyRealPageId));
        }

        [Fact]
        public void UpdateOrganization_InvalidOrganization_ExceptionThrown()
        {
            //Arrange
            //IManageOrganization manageOrganization = new ManageOrganization();
            IManageOrganization manageOrganization = new ManageOrganization(_mockRepository.Object, _defaultUserClaim, mockMessageHandler.Object);

            //Act
            //Assert
            Assert.Throws<ArgumentNullException>(() => manageOrganization.UpdateOrganization(null));
        }

        [Fact]
        public void CreateOrganization_MockInputData_ReturnValidRepositoryResponseObject()
        {
			//Arrange
			RPObjectCache rPObjectCache = new RPObjectCache();
			rPObjectCache.BustCache();
            Mock<HttpMessageHandler> mockMessageHandler = new Mock<HttpMessageHandler>();

            //Act
            IManageOrganization manageOrganization = new ManageOrganization(_mockRepository.Object, _defaultUserClaim, mockMessageHandler.Object);
            IRepositoryResponse repositoryResponse = manageOrganization.InsertOrganization(_organization);

            //Assert
            Assert.True(
                repositoryResponse.Id == _PartyId
                && repositoryResponse.ErrorMessage == ""
                && repositoryResponse.RealPageId == _RealPageId
            );
        }

        [Fact]
        public void UpdateOrganization_MockInputData_ReturnValidRepositoryResponseObject()
        {
			//Arrange
			RPObjectCache rPObjectCache = new RPObjectCache();
			rPObjectCache.BustCache();

            //Act
            IManageOrganization manageOrganization = new ManageOrganization(_mockRepository.Object, _defaultUserClaim, mockMessageHandler.Object);
            IRepositoryResponse repositoryResponse = manageOrganization.UpdateOrganization(_organization);

            //Assert
            Assert.True(
                repositoryResponse.Id == _PartyId
                && repositoryResponse.ErrorMessage == ""
                && repositoryResponse.RealPageId == _RealPageId
            );
        }

        [Fact]
        public void GetOrganization_MockInputData_ReturnValidRepositoryResponseObject()
        {
            //Arrange

            //Act
            IManageOrganization manageOrganization = new ManageOrganization(_mockRepository.Object, _defaultUserClaim, mockMessageHandler.Object);
            var orgResult = manageOrganization.GetOrganization(realPageId: _RealPageId);

            //Assert
            Assert.True(
                orgResult.PartyId == _PartyId
                && orgResult.RealPageId == _RealPageId
                && orgResult.Name == _CompanyName
                && orgResult.CreateDate == _CreateDate
            );
        }

        [Fact]
        public void GetOrganization_MockInputData_ReturnExampleData()
        {
            //Arrange
            var result = Organization.GetOrganizationExample();

            //Act

            //Assert
            Assert.Equal("Some company", result.Name);
        }

        [Fact]
        public void ListOrganization_ReturnList()
        {
            //Arrange
            IManageOrganization manageOrganization = new ManageOrganization(_mockRepository.Object, _defaultUserClaim, mockMessageHandler.Object);

            //Act
            var result = manageOrganization.GetOrganizationList();

            //Assert
            Assert.True(result.Count == 1);
        }

		[Fact]
		public void ListOrganizationType_Mock_ReturnValidOrganizationTypeList()
		{
			//Arrange
			RPObjectCache rPObjectCache = new RPObjectCache();
			rPObjectCache.BustCache();
			Type type = typeof(OrganizationType);

			//Act
			int NumberOfProperties = type.GetProperties().Length;
            IManageOrganization manageOrganization = new ManageOrganization(_mockRepository.Object, _defaultUserClaim, mockMessageHandler.Object);
            IList<OrganizationType> organizationTypeList = manageOrganization.ListOrganizationType();

			//Assert
			Assert.True(
				organizationTypeList.Count == _organizationTypes.Count
				&& organizationTypeList.All(
					o => _organizationTypes.Any(
						w => w.OrganizationTypeId == o.OrganizationTypeId
						&&
						w.CreateDate == o.CreateDate
						&&
						w.Name == o.Name
					)
				) == true
				&& NumberOfProperties == 3
			);
		}
        #endregion

        [Fact]
        public void GetOrganizationIdentityProviderType_ReturnExampleData()
        {
            //Arrange
            IManageOrganization manageOrganization = new ManageOrganization(_mockRepository.Object, _defaultUserClaim, mockMessageHandler.Object);

            //Act
            var result = manageOrganization.GetOrganizationIdentityProviderType(_RealPageId);

            //Assert
            Assert.True(
                result.Count == 2
            );
        }

        [Fact]
        public void GetOrganizationByMultipleIds_ReturnExampleData()
        {
            //Arrange
            //OrganizationRepository organizationRepository = new OrganizationRepository(_mockRepository.Object);
            IManageOrganization manageOrganization = new ManageOrganization(_mockRepository.Object, _defaultUserClaim, mockMessageHandler.Object);

            //Act
            var result = manageOrganization.GetOrganization(_RealPageId);
            Assert.True(result.RealPageId == _organization.RealPageId);

            result = manageOrganization.GetOrganization(realPageId: Guid.Empty, organizationPartyId: _PartyId);
            Assert.True(result.RealPageId == _organization.RealPageId);

            Exception exception = Record.Exception(() => manageOrganization.GetOrganization(Guid.Empty));

            //Assert
            Assert.IsType<Exception>(exception);
            Assert.Equal("Invalid parameter: Organization realPageId, partyId is required.", exception.Message);
        }

        [Fact]
        public void GetOrganizationList_ReturnExampleData()
        {
            //Arrange
            IManageOrganization manageOrganization = new ManageOrganization(_mockRepository.Object, _defaultUserClaim, mockMessageHandler.Object);

            //Act
            var result = manageOrganization.GetOrganizationList();
            Assert.True(result[0].RealPageId == _organization.RealPageId && result.Count == 1);
        }

        [Fact]
        public void GetOrganizationAdminUserRealPageId_ReturnExampleData()
        {
            //Arrange
            IManageOrganization manageOrganization = new ManageOrganization(_mockRepository.Object, _defaultUserClaim, mockMessageHandler.Object);

            //Act
            var result = manageOrganization.GetOrganizationAdminUserRealPageId(_organization.RealPageId);
            
            Assert.True(result.Equals(_RealPageId));
        }

    }

    [ExcludeFromCodeCoverage]
    public class DatabaseResult
    {
        public string PartyId { get; set; }
        public string PersonRealPageId { get; set; }

    }
}
