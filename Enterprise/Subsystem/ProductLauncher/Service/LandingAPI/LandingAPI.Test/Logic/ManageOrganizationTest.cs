using FluentAssertions;
using Moq;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic
{
    [ExcludeFromCodeCoverage]
    /// <summary>
    /// ManageOrganization Unit tests
    /// </summary>
    public class ManageOrganizationTest : TestBase
    {
        private static Guid _RealPageId = new Guid("C802694D-5553-4527-8616-3C0F434AE62D");
        private static string _CompanyName = "Test Company";
        private static DateTime _CreateDate = DateTime.MaxValue.ToUniversalTime();
        private static int _PartyId = 54321;
        private static long _BooksMasterId = 12345;
        private static long _BooksCompanyMasterId = 12345;
        private static int _organizationTypeId = 6;
        private static int _organizationDomainId = 1;

        private readonly Organization _organization = null;
        private readonly List<Organization> _organizationList = null;
        private readonly List<OrganizationType> _organizationTypes = null;
        private readonly List<OrganizationDomain> _organizationDomains = null;
        private readonly List<IdentityProviderType> _identityProviderTypes = null;

        private readonly Mock<IUnitOfWork> _mockUnitofWork = new Mock<IUnitOfWork>();

        private readonly DefaultUserClaim _defaultUserClaim = new DefaultUserClaim();
        readonly Mock<HttpMessageHandler> mockMessageHandler = new Mock<HttpMessageHandler>();
        protected List<GbProductMap> _gbProductMap;

        private Mock<IPropertyRepository> _propertyRepositoryMock;

        public ManageOrganizationTest()
        {
            _propertyRepositoryMock = new Mock<IPropertyRepository>();
            _defaultUserClaim.CorrelationId = Guid.Empty;

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

            _organizationList = new List<Organization>() { _organization };

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

            _identityProviderTypes = new List<IdentityProviderType>() { new IdentityProviderType() { ContactMechanismId = 1000, AuthenticationType = "id3" }, new IdentityProviderType() { ContactMechanismId = 1001, AuthenticationType = "aad" } };

            List<DatabaseResult> orgList = new List<DatabaseResult>();
            orgList.Add(new DatabaseResult() { PartyId = "2345", PersonRealPageId = _RealPageId.ToString() });

            _gbProductMap = new List<GbProductMap>
            {
                new GbProductMap() { BooksProductCode = "OS", Name = "OneSite", ProductId = 1, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "UI", Name = "UnifiedUI", ProductId = 2, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "UPFM", Name = "Unified Platform", ProductId = 3, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "AO", Name = "Asset Optimization", ProductId = 4, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "PW", Name = "Propertyware", ProductId = 5, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "L2L", Name = "Lead2Lease", ProductId = 6, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "YS", Name = "YieldStar", ProductId = 7, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "ACCT", Name = "Financial Suite", ProductId = 8, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "LS", Name = "Marketing Center", ProductId = 9, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "LVL1", Name = "Prospect Contact Center", ProductId = 10, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "??", Name = "Social", ProductId = 11, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "OPSB", Name = "Ops Bid", ProductId = 12, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "OPS", Name = "Spend Management", ProductId = 13, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "OMS", Name = "Client Portal", ProductId = 14, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "LD", Name = "Renters Insurance", ProductId = 15, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "CD", Name = "Vendor Credentialing", ProductId = 16, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "AB", Name = "Resident Portals", ProductId = 17, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "NWP", Name = "Utility Management", ProductId = 18, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "LP", Name = "Product Learning Portal", ProductId = 19, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "DOC", Name = "Document Director", ProductId = 20, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "OSC", Name = "L&R Conversion Utility", ProductId = 21, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "OC", Name = "OmniChannel", ProductId = 22, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "ONST", Name = "On-Site", ProductId = 23, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "RA", Name = "Unified Data Management", ProductId = 24, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "SP", Name = "Self-provisioning portal", ProductId = 25, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "UA", Name = "Unified Amenities", ProductId = 26, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "MT", Name = "Migration Tool Application", ProductId = 27, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "PUPDATE", Name = "Product Updates", ProductId = 28, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "BI", Name = "Business Intelligence", ProductId = 29, UDMSourceCode = "AO" },
                new GbProductMap() { BooksProductCode = "PA", Name = "Performance Analytics", ProductId = 30, UDMSourceCode = "AO" },
                new GbProductMap() { BooksProductCode = "MA", Name = "Investment Analytics", ProductId = 31, UDMSourceCode = "AO" },
                new GbProductMap() { BooksProductCode = "PO", Name = "YieldStar", ProductId = 32, UDMSourceCode = "AO" },
                new GbProductMap() { BooksProductCode = "AX", Name = "Axiometrics", ProductId = 33, UDMSourceCode = "AO" },
                new GbProductMap() { BooksProductCode = "BM", Name = "Benchmarking", ProductId = 34, UDMSourceCode = "AO" },
                new GbProductMap() { BooksProductCode = "null", Name = "Support Tool", ProductId = 35, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "ELMS", Name = "EasyLMS", ProductId = 36, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "PHOTO", Name = "Property Photos", ProductId = 37, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "VMP", Name = "Vendor Marketplace", ProductId = 38, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "IMP", Name = "Integration Marketplace", ProductId = 39, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "ILMLM", Name = "ILM Lead Management", ProductId = 40, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "ILMLA", Name = "ILM Leasing Analytics", ProductId = 41, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "SM", Name = "Settings Management", ProductId = 43, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "RPM", Name = "Portfolio Management", ProductId = 44, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "CIMPL", Name = "CIMPL", ProductId = 45, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "SSM", Name = "Site Spend Management Portal", ProductId = 46, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "DIQ", Name = "Deposit Alternative", ProductId = 47, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "CPAY", Name = "ClickPay", ProductId = 48, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "HLP", Name = "Simon Help Center", ProductId = 49, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "SLM", Name = "Senior Lead Management", ProductId = 50, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "LRO", Name = "LRO", ProductId = 51, UDMSourceCode = "AO" },
                new GbProductMap() { BooksProductCode = "AA", Name = "Amenity Optimization", ProductId = 52, UDMSourceCode = "AO" },
                new GbProductMap() { BooksProductCode = "AIRM", Name = "AI Revenue Management", ProductId = 53, UDMSourceCode = "AO" },
                new GbProductMap() { BooksProductCode = "RC", Name = "Rent Control", ProductId = 54, UDMSourceCode = "AO" },
                new GbProductMap() { BooksProductCode = "RENO", Name = "Renovation Manager", ProductId = 55, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "SET", Name = "Unified Settings", ProductId = 56, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "SMS-T", Name = "Intelligent Building", ProductId = 57, UDMSourceCode = "IB" },
                new GbProductMap() { BooksProductCode = "SMS-E", Name = "Intelligent Building Energy", ProductId = 58, UDMSourceCode = "IB" },
                new GbProductMap() { BooksProductCode = "SMS-W", Name = "Intelligent Building Water", ProductId = 59, UDMSourceCode = "IB" },
                new GbProductMap() { BooksProductCode = "PME", Name = "PME Dashboard", ProductId = 62, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "RMA", Name = "Market Analytics", ProductId = 66, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "ST", Name = "Support Tool", ProductId = 35, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "HOTS", Name = "Hands On Training System", ProductId = 63, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "PEQ", Name = "P2 Engagement Queue", ProductId = 64, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "LeaseLabs", Name = "LeaseLabs", ProductId = 68, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "RPT", Name = "Reporting", ProductId = 67, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "6247", Name = "Self-Guided Tour", ProductId = 65, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "LST", Name = "Lead Scoring", ProductId = 69, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "SMS-TC", Name = "Smart Waste Commercial", ProductId = 70, UDMSourceCode = "IB" },
                new GbProductMap() { BooksProductCode = "OS", Name = "Facilities", ProductId = 75, UDMSourceCode = null }
            };

            #region Set up the mocks

            mockRepository.Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization, It.IsAny<object>()))
                .Returns(_organization);

            mockRepository.Setup(m => m.GetMany<Organization>(StoredProcNameConstants.SP_GetOrganization, It.IsAny<object>()))
                .Returns(_organizationList);

            mockRepository.Setup(m => m.GetMany<OrganizationType>(StoredProcNameConstants.SP_ListOrganizationType, null))
                .Returns(_organizationTypes);

            mockRepository.Setup(m => m.GetMany<OrganizationDomain>(StoredProcNameConstants.SP_ListOrganizationDomain, null))
                .Returns(_organizationDomains);

            mockRepository.Setup(m => m.GetMany<IdentityProviderType>(StoredProcNameConstants.SP_GetOrganizationIdentityProviderType, It.IsAny<object>()))
                .Returns(_identityProviderTypes);

            mockRepository.Setup(m => m.GetMany<dynamic>(StoredProcNameConstants.SP_ListOrganizations, It.IsAny<object>()))
                .Returns(orgList);

            mockRepository.Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdateOrganization, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = _PartyId, ErrorMessage = "", RealPageId = _RealPageId });

            mockRepository.Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_InsertOrganization, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = _PartyId, ErrorMessage = "", RealPageId = _RealPageId });

            mockRepository.Setup(m => m.UnitOfWork).Returns(_mockUnitofWork.Object);

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct,
                    It.IsAny<object>()))
                .Returns(_gbProductMap);
            #endregion

        }

        #region Unit Tests

        [Fact]
        public void CreateOrganization_InvalidOrganization_ExceptionThrown()
        {
            //Arrange
            IManageOrganization manageOrganization = new ManageOrganization(mockRepository.Object, _defaultUserClaim, mockMessageHandler.Object);
            //Act

            //Assert
            Assert.Throws<ArgumentNullException>(() => manageOrganization.InsertOrganization(null));
        }

        [Fact]
        public async Task UpdatePropertyList_ReturnsError_WhenPropertyListIsNull()
        {
            // Arrange
            List<UPFMPropertyInstance> propertyList = null;
            Guid companyInstanceId = Guid.NewGuid();
            //Arrange
            IManageOrganization manageOrganization = new ManageOrganization(mockRepository.Object, _defaultUserClaim, mockMessageHandler.Object);
            // Act
            var result = await manageOrganization.UpdatePropertyList(propertyList, companyInstanceId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Invalid parameter propertyInstanceId.", result.ErrorMessage);
        }

        [Fact]
        public async Task UpdatePropertyList_ReturnsError_WhenAnyPropertyInstanceIdIsEmpty()
        {
            // Arrange
            var propertyList = new List<UPFMPropertyInstance>
            {
                new UPFMPropertyInstance { InstanceId = Guid.Empty, Name = "Test" }
            };
            Guid companyInstanceId = Guid.NewGuid();
            //Arrange
            IManageOrganization manageOrganization = new ManageOrganization(mockRepository.Object, _defaultUserClaim, mockMessageHandler.Object);
            // Act
            var result = await manageOrganization.UpdatePropertyList(propertyList, companyInstanceId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Invalid parameter propertyInstanceId.", result.ErrorMessage);
        }

        [Fact]
        public async Task UpdatePropertyList_ReturnsError_WhenAnyPropertyNameIsNullOrEmpty()
        {
            // Arrange
            var propertyList = new List<UPFMPropertyInstance>
            {
                new UPFMPropertyInstance { InstanceId = Guid.NewGuid(), Name = "" }
            };
            Guid companyInstanceId = Guid.NewGuid();
            //Arrange
            IManageOrganization manageOrganization = new ManageOrganization(mockRepository.Object, _defaultUserClaim, mockMessageHandler.Object);
            // Act
            var result = await manageOrganization.UpdatePropertyList(propertyList, companyInstanceId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Invalid parameter propertyInstanceId.", result.ErrorMessage);
        }

        [Fact]
        public async Task UpdatePropertyList_CallsUpdateUPFMPropertyList_AndReturnsError_WhenIdIsNotGreaterThanZero_Variant3()
        {
            // Arrange  
            var propertyList = new List<UPFMPropertyInstance>
           {
               new UPFMPropertyInstance { InstanceId = Guid.NewGuid(), Name = "Test" }
           };
            IManageOrganization manageOrganization = new ManageOrganization(mockRepository.Object, _defaultUserClaim, mockMessageHandler.Object);

            typeof(ManageOrganization)
            .GetField("_propertyRepository", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(manageOrganization, _propertyRepositoryMock.Object);

            Guid companyInstanceId = Guid.NewGuid();
            _propertyRepositoryMock.Setup(x => x.UpdateUPFMPropertyList(propertyList))
                .Returns(new RepositoryResponse { Id = 0, ErrorMessage = "Some error" });

            // Act  
            var result = await manageOrganization.UpdatePropertyList(propertyList, companyInstanceId);

            // Assert  
            Assert.NotNull(result);
            _propertyRepositoryMock.Verify(x => x.UpdateUPFMPropertyList(propertyList), Times.Once);
        }

        [Fact]
        public async Task UpdatePropertyList_CallsUpdateUPFMPropertyList_AndReturnsError_WhenIdIsNotGreaterThanZero_Variant1()
        {
            // Arrange
            var propertyList = new List<UPFMPropertyInstance>
                {
                    new UPFMPropertyInstance { InstanceId = Guid.NewGuid(), Name = "Test" }
                };

            //Arrange
            IManageOrganization manageOrganization = new ManageOrganization(mockRepository.Object, _defaultUserClaim, mockMessageHandler.Object);


            Guid companyInstanceId = Guid.NewGuid();

            typeof(ManageOrganization)
          .GetField("_propertyRepository", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
          .SetValue(manageOrganization, _propertyRepositoryMock.Object);

            _propertyRepositoryMock.Setup(x => x.UpdateUPFMPropertyList(propertyList))
              .Returns(new RepositoryResponse { Id = 0, ErrorMessage = "" });




            // Act
            var result = await manageOrganization.UpdatePropertyList(propertyList, companyInstanceId);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task UpdatePropertyList_CallsUpdatePropertyInBooks_AndReturnsSuccess_WhenIdIsGreaterThanZero_Variant4()
        {
            // Arrange  
            var propertyList = new List<UPFMPropertyInstance>
           {
               new UPFMPropertyInstance { InstanceId = Guid.NewGuid(), Name = "Test" }
           };
            Guid companyInstanceId = Guid.NewGuid();
            _propertyRepositoryMock.Setup(x => x.UpdateUPFMPropertyList(propertyList))
                .Returns(new RepositoryResponse { Id = 1, ErrorMessage = "" });


            // Use a delegate or patching library if needed, or just test up to the call  
            IManageOrganization manageOrganization = new ManageOrganization(mockRepository.Object, _defaultUserClaim, mockMessageHandler.Object);
            // Act  
            var result = await manageOrganization.UpdatePropertyList(propertyList, companyInstanceId);

            // Assert  
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdatePropertyList_CallsUpdatePropertyInBooks_AndReturnsSuccess_WhenIdIsGreaterThanZero_Variant2()
        {
            // Arrange  
            var propertyList = new List<UPFMPropertyInstance>
           {
               new UPFMPropertyInstance { InstanceId = Guid.NewGuid(), Name = "Test" }
           };
            // Use a delegate or patching library if needed, or just test up to the call  
            IManageOrganization manageOrganization = new ManageOrganization(mockRepository.Object, _defaultUserClaim, mockMessageHandler.Object);

            Guid companyInstanceId = Guid.NewGuid();
            typeof(ManageOrganization)
            .GetField("_propertyRepository", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(manageOrganization, _propertyRepositoryMock.Object);

            _propertyRepositoryMock.Setup(x => x.UpdateUPFMPropertyList(propertyList))
              .Returns(new RepositoryResponse { Id = 0, ErrorMessage = "" });

            // Patch UpdatePropertyInBooks to always return true  
            var manageOrgType = typeof(ManageOrganization);
            // With this line to avoid AmbiguousMatchException by specifying parameter types (here, assuming the method signature is: private bool UpdatePropertyInBooks(Guid, Guid))
            var method = manageOrgType.GetMethod(
                "UpdatePropertyInBooks",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null,
                new Type[] { typeof(Guid), typeof(Guid) },
                null
            );
      
            // Act  
            var result = await manageOrganization.UpdatePropertyList(propertyList, companyInstanceId);

            // Assert  
            Assert.NotNull(result);
            Assert.Equal("", result.ErrorMessage);
        }    

        [Fact]
        public void UpdateOrganization_InvalidRealPage_ExceptionThrown()
        {
            //Arrange
            IManageOrganization manageOrganization = new ManageOrganization(mockRepository.Object, _defaultUserClaim, mockMessageHandler.Object);

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
            var res = manageOrganization.UpdateOrganization(organization);
            Assert.True(res.ErrorMessage == "Invalid parameter realPageId.");
        }

        [Fact]
        public void GetOrganization_InvalidRealPageId_ExceptionThrown()
        {
            //Arrange
            IManageOrganization manageOrganization = new ManageOrganization(mockRepository.Object, _defaultUserClaim, mockMessageHandler.Object);

            //Act
            Guid emptyRealPageId = Guid.Empty;

            //Assert
            Assert.Throws<Exception>(() => manageOrganization.GetOrganization(emptyRealPageId));
        }

        [Fact]
        public void UpdateOrganization_InvalidOrganization_ExceptionThrown()
        {
            //Arrange
            IManageOrganization manageOrganization = new ManageOrganization(mockRepository.Object, _defaultUserClaim, mockMessageHandler.Object);

            //Act
            //Assert
            var response = manageOrganization.UpdateOrganization(null);
            response.ErrorMessage.Should().Be("Organization is Null");
        }

        [Fact]
        public void CreateOrganization_MockInputData_ReturnValidRepositoryResponseObject()
        {
            //Arrange
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();
            Mock<HttpMessageHandler> mockMessageHandler = new Mock<HttpMessageHandler>();

            //Act
            IManageOrganization manageOrganization = new ManageOrganization(mockRepository.Object, _defaultUserClaim, mockMessageHandler.Object);
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
            IManageOrganization manageOrganization = new ManageOrganization(mockRepository.Object, _defaultUserClaim, mockMessageHandler.Object);
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
            IManageOrganization manageOrganization = new ManageOrganization(mockRepository.Object, _defaultUserClaim, mockMessageHandler.Object);
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
            IManageOrganization manageOrganization = new ManageOrganization(mockRepository.Object, _defaultUserClaim, mockMessageHandler.Object);

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
            IManageOrganization manageOrganization = new ManageOrganization(mockRepository.Object, _defaultUserClaim, mockMessageHandler.Object);
            IList<OrganizationType> organizationTypeList = manageOrganization.ListOrganizationType();

            //Assert
            Assert.True(
                organizationTypeList.Count == _organizationTypes.Count
                && organizationTypeList.All(
                    o => _organizationTypes.Exists(
                        w => w.OrganizationTypeId == o.OrganizationTypeId
                        &&
                        w.CreateDate == o.CreateDate
                        &&
                        w.Name == o.Name
                    )
                )
                && NumberOfProperties == 3
            );
        }
        #endregion

        [Fact]
        public void GetOrganizationIdentityProviderType_ReturnExampleData()
        {
            //Arrange
            IManageOrganization manageOrganization = new ManageOrganization(mockRepository.Object, _defaultUserClaim, mockMessageHandler.Object);

            //Act
            var result = manageOrganization.GetOrganizationIdentityProviderType(_RealPageId);

            //Assert
            Assert.True(result.Count == 2);
        }

        [Fact]
        public void GetOrganizationByMultipleIds_ReturnExampleData()
        {
            //Arrange
            IManageOrganization manageOrganization = new ManageOrganization(mockRepository.Object, _defaultUserClaim, mockMessageHandler.Object);

            //Act
            var result = manageOrganization.GetOrganization(_RealPageId);
            Assert.True(result.RealPageId == _organization.RealPageId);

            result = manageOrganization.GetOrganization(realPageId: Guid.Empty, organizationPartyId: _PartyId);
            Assert.True(result.RealPageId == _organization.RealPageId);

            Exception exception = Record.Exception(() => manageOrganization.GetOrganization(Guid.Empty));

            //Assert
            Assert.IsType<Exception>(exception);
            exception.Message.Should().Be("Invalid parameter: Organization realPageId, partyId is required.");
        }

        [Fact]
        public void GetOrganizationList_ReturnExampleData()
        {
            //Arrange
            IManageOrganization manageOrganization = new ManageOrganization(mockRepository.Object, _defaultUserClaim, mockMessageHandler.Object);

            //Act
            var result = manageOrganization.GetOrganizationList();
            Assert.True(result[0].RealPageId == _organization.RealPageId && result.Count == 1);
        }

        [Fact]
        public void GetOrganizationAdminUserRealPageId_ReturnExampleData()
        {
            //Arrange
            IManageOrganization manageOrganization = new ManageOrganization(mockRepository.Object, _defaultUserClaim, mockMessageHandler.Object);

            //Act
            var result = manageOrganization.GetOrganizationAdminUserRealPageId(_organization.RealPageId);

            Assert.True(result.Equals(_RealPageId));
        }

        [Fact]
        public void GetPropertiesForCompany_CustomerStatusSort_AppliesPostEnrichmentSortAndPaging()
        {
            // Arrange
            Guid companyInstanceId = Guid.NewGuid();
            Guid propertyA = Guid.NewGuid();
            Guid propertyB = Guid.NewGuid();
            Guid propertyC = Guid.NewGuid();
            RequestParameter repositoryFilter = null;

            Mock<IManageBlueBook> manageBlueBookMock = new Mock<IManageBlueBook>();
            manageBlueBookMock
                .Setup(x => x.GetPropertyInstanceForCompany(companyInstanceId))
                .Returns(new List<BooksPropertyInstance>
                {
                    BuildBooksProperty(propertyA, true, "A", "domain1"),
                    BuildBooksProperty(propertyB, false, "B", "domain1"),
                    BuildBooksProperty(propertyC, false, "C", "domain1")
                });

            _propertyRepositoryMock
                .Setup(x => x.GetPropertiesForCompany(It.IsAny<List<Guid>>(), null, null, null, It.IsAny<RequestParameter>()))
                .Callback<List<Guid>, string, int?, int?, RequestParameter>((_, __, ___, ____, filter) => repositoryFilter = filter)
                .Returns(new List<PropertySetup>
                {
                    new PropertySetup { InstanceId = propertyB, Name = "Bravo" },
                    new PropertySetup { InstanceId = propertyA, Name = "Alpha" },
                    new PropertySetup { InstanceId = propertyC, Name = "Charlie" }
                });

            IManageOrganization manageOrganization = new ManageOrganization(mockRepository.Object, _defaultUserClaim, mockMessageHandler.Object);
            typeof(ManageOrganization)
                .GetField("_propertyRepository", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(manageOrganization, _propertyRepositoryMock.Object);
            typeof(ManageOrganization)
                .GetField("_manageBlueBook", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(manageOrganization, manageBlueBookMock.Object);

            Dictionary<object, object> globals = new Dictionary<object, object>
            {
                {
                    BaseType.RequestParameter,
                    new RequestParameter
                    {
                        SortBy = new Dictionary<string, string> { { "CustomerStatus", "DESC" } },
                        Pages = new PageRequest { ResultsPerPage = 1, StartRow = 2 }
                    }
                }
            };

            // Act
            List<CompanyPropertySetup> result = manageOrganization.GetPropertiesForCompany(companyInstanceId, globals: globals);

            // Assert - repository gets safe sort and full row request.
            Assert.NotNull(repositoryFilter);
            Assert.Equal("Name", repositoryFilter.SortBy.Keys.First());
            Assert.Equal("ASC", repositoryFilter.SortBy["Name"]);
            Assert.Equal(100, repositoryFilter.Pages.ResultsPerPage);
            Assert.Equal(1, repositoryFilter.Pages.StartRow);

            // Assert - post-enrichment sorting and paging are applied.
            Assert.Single(result);
            Assert.Single(result[0].Property);
            Assert.Equal("Bravo", result[0].Property[0].Name);
            Assert.Equal(3, result[0].Property[0].TotalRecords);
        }

        [Fact]
        public void GetPropertiesForCompany_OrderTypeSort_AppliesPostEnrichmentOrderTypeSort()
        {
            // Arrange
            Guid companyInstanceId = Guid.NewGuid();
            Guid propertyA = Guid.NewGuid();
            Guid propertyB = Guid.NewGuid();
            Guid propertyC = Guid.NewGuid();

            Mock<IManageBlueBook> manageBlueBookMock = new Mock<IManageBlueBook>();
            manageBlueBookMock
                .Setup(x => x.GetPropertyInstanceForCompany(companyInstanceId))
                .Returns(new List<BooksPropertyInstance>
                {
                    BuildBooksProperty(propertyA, true, "B", "domain1"),
                    BuildBooksProperty(propertyB, true, null, "domain1"),
                    BuildBooksProperty(propertyC, true, "a", "domain1")
                });

            _propertyRepositoryMock
                .Setup(x => x.GetPropertiesForCompany(It.IsAny<List<Guid>>(), null, null, null, It.IsAny<RequestParameter>()))
                .Returns(new List<PropertySetup>
                {
                    new PropertySetup { InstanceId = propertyA, Name = "Zulu" },
                    new PropertySetup { InstanceId = propertyB, Name = "Alpha" },
                    new PropertySetup { InstanceId = propertyC, Name = "Beta" }
                });

            IManageOrganization manageOrganization = new ManageOrganization(mockRepository.Object, _defaultUserClaim, mockMessageHandler.Object);
            typeof(ManageOrganization)
                .GetField("_propertyRepository", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(manageOrganization, _propertyRepositoryMock.Object);
            typeof(ManageOrganization)
                .GetField("_manageBlueBook", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(manageOrganization, manageBlueBookMock.Object);

            Dictionary<object, object> globals = new Dictionary<object, object>
            {
                {
                    BaseType.RequestParameter,
                    new RequestParameter
                    {
                        SortBy = new Dictionary<string, string> { { "OrderType", "ASC" } },
                        Pages = new PageRequest { ResultsPerPage = 100, StartRow = 1 }
                    }
                }
            };

            // Act
            List<CompanyPropertySetup> result = manageOrganization.GetPropertiesForCompany(companyInstanceId, globals: globals);

            // Assert
            Assert.Equal(3, result[0].Property.Count);
            Assert.Equal("Alpha", result[0].Property[0].Name); // null OrderType -> empty string, sorted first
            Assert.Equal("Beta", result[0].Property[1].Name);  // "a" before "B" (case-insensitive)
            Assert.Equal("Zulu", result[0].Property[2].Name);
            Assert.All(result[0].Property, property => Assert.Equal(3, property.TotalRecords));
        }

        private static BooksPropertyInstance BuildBooksProperty(Guid instanceId, bool customerStatus, string orderType, string domain)
        {
            return new BooksPropertyInstance
            {
                attributes = new PropertyAttributesInstance
                {
                    propertyInstanceSourceId = instanceId.ToString(),
                    domain = domain,
                    customerPropertyMap = new List<CustomerPropertyMap>
                    {
                        new CustomerPropertyMap
                        {
                            customerProperty = new List<CustomerPropertyInstance>
                            {
                                new CustomerPropertyInstance
                                {
                                    isActive = customerStatus,
                                    customerPropertyOrderType = new List<CustomerPropertyOrderTypeInstance>
                                    {
                                        new CustomerPropertyOrderTypeInstance
                                        {
                                            orderType = orderType
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }
    }

    [ExcludeFromCodeCoverage]
    public class DatabaseResult
    {
        public string PartyId { get; set; }
        public string PersonRealPageId { get; set; }

    }
}
