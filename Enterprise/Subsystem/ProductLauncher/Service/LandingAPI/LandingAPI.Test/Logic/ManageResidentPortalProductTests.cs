using Moq;
using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Migration;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.ResidentPortal;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Saml;
using RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using Xunit;
using IC = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using JsonApiSerializer;
using Xunit.Abstractions;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Accounting;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic
{
    /// <summary>
    /// Product xUnit tests
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ManageResidentPortalProductTests //: ManageProductBaseTests
    {
        #region Private Variables
        private readonly ITestOutputHelper _output;
        private int _blueBookId;
        private GbProductMap _gbProductMap = new GbProductMap();
        private List<Community> _PropertyList = new List<Community>();
        private List<ILevel> _RoleList = new List<ILevel>();
        private List<IMessagingGroups> _messageGroupsList = new List<IMessagingGroups>();
        private Notifications _notifications = new Notifications();
        private ListResponse _listResponse = new ListResponse();
        private ResidentPortalUser _residentPortalUser = new ResidentPortalUser();
        private ResidentPortalUser _residentPortalEditorUser = new ResidentPortalUser();
        private DefaultUserClaim _userClaims;

        private string testHostname = "http://producturl.com";
        private string _mtApiEndPoint = "http://producturl.com";
        private string _appId = "d8f43b85";
        private string _appKey = "50aa7342baf824716f87e6999cf4b472";

        protected int _productId = (int)ProductEnum.ResidentPortal;

        protected static string _ROLETYPE_NAME_USER = "User";
        protected static string _ROLETYPE_NAME_SUPERUSER = "SuperUser";
        protected static string _ROLETYPE_NAME_REALPAGE_EMPLOYEE = "RealPage Employee";
        protected static string _ROLETYPE_NAME_USER_NOEMAIL = "User (No Email)";

        protected static string _uniqueIdentifier = "1234567|userlogin";

        protected long _editorPersonaId = 4;
        protected long _editorUserId = 14;
        protected Guid _editorRealPageId = new Guid("523C6677-C20D-4E6A-A4CC-0DE5781F0D5C");
        protected int _editorOrganizationPartyId = 1234;
        private Guid _editorOrganizationRealPageId = new Guid("12345678-C20D-4E6A-A4CC-0DE5781F0D5C");
        private Guid _editorCorrelationId = new Guid("8C5F223C-169A-44BD-9844-F925B5F0C332");
        protected DefaultUserClaim _editorUserClaim;

        protected long _userPersonaId = 5;
        protected long _userUserId = 15;
        protected Guid _userRealPageId = new Guid("623C6677-D20D-5E6A-B4CC-1DE5781F0D5C");
        protected Guid _userOrganizationRealPageId = new Guid("12345678-C20D-4E6A-A4CC-0DE5781F0D5C");
        private int _userOrganizationPartyId = 1234;
        private Guid _userCorrelationId = new Guid("078724B2-D381-4E45-9EE9-6DD6D9B9B74B");
        protected DefaultUserClaim _userUserClaim;

        protected long _newUserPersonaId = 7;
        protected long _newUserUserId = 17;
        private Guid _newUserRealPageId = new Guid("523C6677-D20D-DDDD-B4CC-1DE5781F0D5C");
        protected Guid _newuserOrganizationRealPageId = new Guid("12345678-C20D-4E6A-A4CC-0DE5781F0D5C");
        private int _newUserOrganizationPartyId = 1234;

        private int _userInvalidOrganizationPartyId = 5544;

        protected Persona _editorPersona;
        protected Persona _userPersona;
        protected Persona _nullPersona;
        protected Persona _newUserPersona;
        protected Persona _userInvalidPersona;

        protected IList<ProductSettingType> _productSettingType = new List<ProductSettingType>();
        protected IList<ProductSettingList> _userProductSettings = new List<ProductSettingList>();

        protected RepositoryResponse _repositoryResponseProductStatus = new RepositoryResponse();
        protected RepositoryResponse _repositoryResponsePropertySuccess = new RepositoryResponse();
        protected RepositoryResponse _repositoryResponsePropertyFail = new RepositoryResponse();


        protected IList<IC.ProductInternalSetting> _productInternalSettings = new List<IC.ProductInternalSetting>();
        protected IList<IC.ElectronicAddress> _electronicAddressList = new List<IC.ElectronicAddress>();

        protected List<SamlAttributes> _editorSamlAttributes;
        protected List<SamlAttributes> _userSamlAttributes;
        protected List<SamlAttributes> _emptySamlAttributes;

        protected List<ProductProperty> _resultPropertyList;
        protected List<ACProperty> _resultPropertyListFinSuite;
        protected List<ProductRole> _resultRoleList;

        //protected HttpClient client;
        //protected Mock<HttpMessageHandler> mockHttpMessageHandler;
        //protected Mock<HttpMessageHandler> mockTokenHttpMessageHandler;
        protected BatchProcessType batchProcessTypeCreUpd = BatchProcessType.CreateUpdateProductUser;

        protected Mock<IRepository> mockRepository;

        protected List<OrganizationStatus> _organizationStatusListEditorPersona = new List<OrganizationStatus>();
        protected List<OrganizationStatus> _organizationStatusListUserPersona = new List<OrganizationStatus>();
        protected List<OrganizationStatus> _organizationStatusListNewUserPersona = new List<OrganizationStatus>();
        protected List<OrganizationStatus> _organizationStatusListInvalidPersona = new List<OrganizationStatus>();

        protected OrganizationStatus _organizationStatusEditorPersona;
        protected OrganizationStatus _organizationStatusUserPersona;
        protected OrganizationStatus _organizationStatusNewUserPersona;
        protected OrganizationStatus _organizationStatusInvalidPersona;
        #endregion

        #region Constructor

        public ManageResidentPortalProductTests(ITestOutputHelper output) //: base((int)ProductEnum.ResidentPortal)
        {
            _output = output;
            _blueBookId = 236;

            _emptySamlAttributes = new List<SamlAttributes>();

            _editorPersona = new Persona() { PersonaId = _editorPersonaId, RealPageId = _editorRealPageId, OrganizationPartyId = _editorOrganizationPartyId, UserId = _editorUserId };
            _editorPersona.Organization = new Organization() { PartyId = _editorOrganizationPartyId, RealPageId = _editorOrganizationRealPageId, Name = "RealPage", BooksMasterId = 1234, BooksCustomerMasterId = 4321, OrganizationDomain = new OrganizationDomain() { OrganizationDomainId = 1, Name = "Primary" } };

            _userPersona = new Persona() { PersonaId = _userPersonaId, RealPageId = _userRealPageId, OrganizationPartyId = _userOrganizationPartyId, UserId = _userUserId };

            _userPersona.Organization = new Organization() { PartyId = _userOrganizationPartyId, RealPageId = _userOrganizationRealPageId, Name = "RealPage", BooksMasterId = 1234, BooksCustomerMasterId = 4321, OrganizationDomain = new OrganizationDomain() { OrganizationDomainId = 1, Name = "Primary" } };

            _newUserPersona = new Persona() { PersonaId = _newUserPersonaId, RealPageId = _newUserRealPageId, OrganizationPartyId = _newUserOrganizationPartyId };
            _newUserPersona.Organization = new Organization() { PartyId = _newUserOrganizationPartyId, Name = "RealPage", BooksMasterId = 1234, RealPageId = _newuserOrganizationRealPageId, BooksCustomerMasterId = 4321, OrganizationDomain = new OrganizationDomain() { OrganizationDomainId = 1, Name = "Primary" } };

            _userInvalidPersona = new Persona() { PersonaId = _userPersonaId, RealPageId = _userRealPageId, OrganizationPartyId = _userInvalidOrganizationPartyId };
            _userInvalidPersona.Organization = new Organization() { PartyId = _userInvalidOrganizationPartyId, Name = "RealPage", BooksMasterId = 1234, BooksCustomerMasterId = 4321, OrganizationDomain = new OrganizationDomain() { OrganizationDomainId = 1, Name = "Primary" } };

            _editorUserClaim = new DefaultUserClaim() { CorrelationId = _editorCorrelationId, OrganizationRealPageGuid = _editorOrganizationRealPageId, UserRealPageGuid = _editorRealPageId };
            _userUserClaim = new DefaultUserClaim() { CorrelationId = _userCorrelationId, OrganizationRealPageGuid = _userOrganizationRealPageId, UserRealPageGuid = _userRealPageId };

            //mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            //mockTokenHttpMessageHandler = new Mock<HttpMessageHandler>();
            //client = new HttpClient(mockHttpMessageHandler.Object, false);

            mockRepository = new Mock<IRepository>();

            _organizationStatusEditorPersona = new OrganizationStatus() { PartyId = _editorPersona.OrganizationPartyId, StatusTypeId = (int)UserUiStatusType.Active, Status = UserUiStatusType.Active };
            _organizationStatusUserPersona = new OrganizationStatus() { PartyId = _userPersona.OrganizationPartyId, StatusTypeId = (int)UserUiStatusType.Active, Status = UserUiStatusType.Active };
            _organizationStatusNewUserPersona = new OrganizationStatus() { PartyId = _newUserPersona.OrganizationPartyId, StatusTypeId = (int)UserUiStatusType.Active, Status = UserUiStatusType.Active };
            _organizationStatusInvalidPersona = new OrganizationStatus() { PartyId = _userInvalidPersona.OrganizationPartyId, StatusTypeId = (int)UserUiStatusType.Active, Status = UserUiStatusType.Active };

            _organizationStatusListEditorPersona.Add(_organizationStatusEditorPersona);
            _organizationStatusListUserPersona.Add(_organizationStatusUserPersona);
            _organizationStatusListNewUserPersona.Add(_organizationStatusNewUserPersona);
            _organizationStatusListInvalidPersona.Add(_organizationStatusInvalidPersona);

            _editorSamlAttributes = new List<SamlAttributes>();
            _editorSamlAttributes.Add(new SamlAttributes() { Name = "UserId", Value = "1234567" });
            _editorSamlAttributes.Add(new SamlAttributes() { Name = "PRODUCTUSERNAME", Value = "bob12" });

            _userSamlAttributes = new List<SamlAttributes>();
            _userSamlAttributes.Add(new SamlAttributes() { Name = "UserId", Value = "5432" });
            _userSamlAttributes.Add(new SamlAttributes() { Name = "PRODUCTUSERNAME", Value = "larry33" });

            _productSettingType.Add(new ProductSettingType() { ProductSettingTypeId = 1, Name = "ProductStatus" });

            _userProductSettings.Add(new ProductSettingList() { ProductId = 1, Name = "IsFavorite", Value = "1", ProductSettingId = 1234 });
            _userProductSettings.Add(new ProductSettingList() { ProductId = 1, Name = "AllProperties", Value = "1", ProductSettingId = 1234 });

            _electronicAddressList = new List<IC.ElectronicAddress>();
            _electronicAddressList.Add(new IC.ElectronicAddress() { AddressType = "Email", AddressString = "test" });

            _productInternalSettings.Add(new IC.ProductInternalSetting() { Name = "ApiEndPoint", Value = testHostname });
            _productInternalSettings.Add(new IC.ProductInternalSetting() { Name = "MTAPIENDPOINT", Value = _mtApiEndPoint });
            _productInternalSettings.Add(new IC.ProductInternalSetting() { Name = "APPID", Value = _appId });
            _productInternalSettings.Add(new IC.ProductInternalSetting() { Name = "APPKEY", Value = _appKey });
            _gbProductMap = new GbProductMap() { BooksProductCode = "AB", Name = "Resident Portals", ProductId = 17, UDMSourceCode = "AB" };
            _repositoryResponseProductStatus.ErrorMessage = "";

            _userClaims = new DefaultUserClaim()
            {
                //UserId = 1,
                LoginName = "MocTest",
                CorrelationId = System.Guid.NewGuid(),
                OrganizationName = "MocTest",
                OrganizationPartyId = 1,
                OrganizationRealPageGuid = System.Guid.NewGuid(),
                OrganizationMasterId = 1,
                UserRealPageGuid = Guid.NewGuid(),
                Rights = new List<string>()
            };

            
            mockRepository
                .Setup(m => m.GetMany<IC.ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct,
                    It.Is<object>(
                        d => TestIsProductId(d, (int)ProductEnum.ResidentPortal))))
                .Returns(_productInternalSettings);

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct,
                    It.IsAny<object>()))
                .Returns(new List<GbProductMap>() { _gbProductMap });
        }

        #endregion

        #region XUnit tests

        [Fact]
        public void GetNotificationSettings_MockInputData_ReturnValidRepositoryResponseObject()
        {
            //Arrange
            Mock<IManageBlueBook> mockManageBlueBook = new Mock<IManageBlueBook>();
            Mock<IManagePersona> mockManagePersona = new Mock<IManagePersona>();
            Mock<IProductRepository> mockProductRepository = new Mock<IProductRepository>();
            Mock<IProductInternalSettingRepository> mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            Mock<ISamlRepository> mockSamlRepository = new Mock<ISamlRepository>();
            Mock<IManageProductResidentPortal> mockManageProductResidentPortal = new Mock<IManageProductResidentPortal>();

            Mock<HttpMessageHandler> mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            Notifications expectedNotifications = new Notifications();

            IList<CustomerCompanyMap> companyMapList = new List<CustomerCompanyMap>()
            {
                new CustomerCompanyMap()
                {
                    CompanyInstanceSourceId = _blueBookId.ToString(),
                    Source = BlueBookProductConstants.ResidentPortal
                }
            };

            _editorPersona.Organization.BooksCustomerMasterId = _blueBookId;

            mockManageBlueBook
                .Setup(m => m.GetCompanyMap(
                    It.IsAny<Guid>(),
                    It.IsAny<long>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()
                ))
                .Returns(companyMapList);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.Is<int>(l => l == (int)ProductEnum.ResidentPortal)
                ))
                .Returns(_productInternalSettings);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 4)
                    , It.Is<int>(l => l == (int)ProductEnum.ResidentPortal)
                ))
                .Returns(_editorSamlAttributes);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 5)
                    , It.Is<int>(l => l == (int)ProductEnum.ResidentPortal)
                ))
                .Returns(_userSamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 4)
                ))
                .Returns(_editorPersona);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 5)
                ))
                .Returns(_userPersona);

            mockProductRepository
                .Setup(m => m.GetProductSettingsByPersona(
                    It.IsAny<long>()
                ))
                .Returns(_userProductSettings);

            mockProductRepository
                .Setup(m => m.GetBooksMasterProductDetail(
                    It.IsAny<int>()
                ))
                .Returns(_gbProductMap);

            _residentPortalUser.Notifications = expectedNotifications;

            //Act
            IManageProductResidentPortal manageProductResidentPortal = new ManageProductResidentPortal(
                editorRealPageId: _editorRealPageId,
                residentPortalEditorUser: _residentPortalEditorUser,
                residentPortalUser: _residentPortalUser,
                samlRepository: mockSamlRepository.Object,
                managePersona: mockManagePersona.Object,
                manageBlueBook: mockManageBlueBook.Object,
                productRepository: mockProductRepository.Object,
                productInternalSettingRepository: mockProductInternalSettingRepository.Object,
                managePerson: null,
                manageUserLogin: null,
                managePartyRelationship: null,
                manageElectronicAddress: null,
                userClaim: _userClaims,
                messageHandler: mockHttpMessageHandler.Object,
                repository: mockRepository.Object);

            //Assert
            _notifications = manageProductResidentPortal.GetNotificationSettings(_editorPersonaId, _userPersonaId);
            Assert.True(_notifications.amenitiesViaEmail == expectedNotifications.amenitiesViaEmail);
            Assert.True(_notifications.managerFdiViaEmail == expectedNotifications.managerFdiViaEmail);
            Assert.True(_notifications.managerMrViaEmail == expectedNotifications.managerMrViaEmail);
        }

        [Fact]
        public void ListLevels_MockInputData_ReturnValidRepositoryResponseObject()
        {
            //Arrange
            Mock<IManageBlueBook> mockManageBlueBook = new Mock<IManageBlueBook>();
            Mock<IManagePersona> mockManagePersona = new Mock<IManagePersona>();
            Mock<IProductRepository> mockProductRepository = new Mock<IProductRepository>();
            Mock<IProductInternalSettingRepository> mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            Mock<ISamlRepository> mockSamlRepository = new Mock<ISamlRepository>();
            Mock<IManageProductResidentPortal> mockManageProductResidentPortal = new Mock<IManageProductResidentPortal>();
            Mock<RolePropertyList> mockRolePropertyList = new Mock<RolePropertyList>();
            Mock<HttpMessageHandler> mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            List<ILevel> expectedLevelList = new List<ILevel>()
            {
                new Level()
                {
                    Id = "STAFFSTANDARD",
                    Name = "Staff Standard",
                    IsAssigned = false,
                    IsDisabled = false
                },
                new Level()
                {
                    Id = "STAFFADMIN",
                    Name = "Staff Admin",
                    IsAssigned = false,
                    IsDisabled = false
                },
                new Level()
                {
                    Id = "STAFFLIMITED",
                    Name = "Staff Limited",
                    IsAssigned = false,
                    IsDisabled = false
                },
                new Level()
                {
                    Id = "ENTERPRISESTANDARD",
                    Name = "Enterprise Standard",
                    IsAssigned = false,
                    IsDisabled = false
                },
                new Level()
                {
                    Id = "ENTERPRISEADMIN",
                    Name = "Enterprise Admin",
                    IsAssigned = false,
                    IsDisabled = false
                }
            };

            IList<CustomerCompanyMap> companyMapList = new List<CustomerCompanyMap>()
            {
                new CustomerCompanyMap()
                {
                    CompanyInstanceSourceId = _blueBookId.ToString(),
                    Source = BlueBookProductConstants.ResidentPortal
                }
            };

            _editorPersona.Organization.BooksCustomerMasterId = _blueBookId;

            mockManageBlueBook
                .Setup(m => m.GetCompanyMap(
                    It.IsAny<Guid>(),
                    It.IsAny<long>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()
                ))
                .Returns(companyMapList);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.Is<int>(l => l == (int)ProductEnum.ResidentPortal)
                ))
                .Returns(_productInternalSettings);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 4)
                    , It.Is<int>(l => l == (int)ProductEnum.ResidentPortal)
                ))
                .Returns(_editorSamlAttributes);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 5)
                    , It.Is<int>(l => l == (int)ProductEnum.ResidentPortal)
                ))
                .Returns(_userSamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 4)
                ))
                .Returns(_editorPersona);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 5)
                ))
                .Returns(_userPersona);

            mockProductRepository
                .Setup(m => m.GetProductSettingsByPersona(
                    It.IsAny<long>()
                ))
                .Returns(_userProductSettings);

            mockProductRepository
                .Setup(m => m.GetBooksMasterProductDetail(
                    It.IsAny<int>()
                ))
                .Returns(_gbProductMap);

            Dictionary<string, string> rolesDictionary = new Dictionary<string, string>();
            rolesDictionary.Add("ENTERPRISE_ADMIN", "Enterprise Admin");
            rolesDictionary.Add("ENTERPRISE_STANDARD", "Enterprise Standard");
            rolesDictionary.Add("STAFF_ADMIN", "Staff Admin");
            rolesDictionary.Add("STAFF_STANDARD", "Staff Standard");
            rolesDictionary.Add("STAFF_LIMITED", "Staff Limited");

            _residentPortalEditorUser.canCreateRoles = rolesDictionary;

            //Act
            IManageProductResidentPortal manageProductResidentPortal = new ManageProductResidentPortal(
                editorRealPageId: _editorRealPageId,
                residentPortalEditorUser: _residentPortalEditorUser,
                residentPortalUser: _residentPortalUser,
                samlRepository: mockSamlRepository.Object,
                managePersona: mockManagePersona.Object,
                manageBlueBook: mockManageBlueBook.Object,
                productRepository: mockProductRepository.Object,
                productInternalSettingRepository: mockProductInternalSettingRepository.Object,
                managePerson: null,
                manageUserLogin: null,
                managePartyRelationship: null,
                manageElectronicAddress: null,
                userClaim: _userClaims,
                messageHandler: mockHttpMessageHandler.Object,
                repository: mockRepository.Object);


            //Assert
            _RoleList = manageProductResidentPortal.ListLevels(0, 0);
            List<ILevel> compareResult = _RoleList.Where(item => expectedLevelList.Select(eItem => eItem.Id).Contains(item.Id)).ToList();
            Assert.True(_RoleList.Count == expectedLevelList.Count);
            Assert.True(compareResult.Count == expectedLevelList.Count);
        }

        [Fact]
        public void ListMessageGroups_MockInputData_ReturnValidRepositoryResponseObject()
        {
            //Arrange
            Mock<IManageBlueBook> mockManageBlueBook = new Mock<IManageBlueBook>();
            Mock<IManagePersona> mockManagePersona = new Mock<IManagePersona>();
            Mock<IProductRepository> mockProductRepository = new Mock<IProductRepository>();
            Mock<IProductInternalSettingRepository> mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            Mock<ISamlRepository> mockSamlRepository = new Mock<ISamlRepository>();
            Mock<IManageProductResidentPortal> mockManageProductResidentPortal = new Mock<IManageProductResidentPortal>();
            Mock<HttpMessageHandler> mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            List<IMessagingGroups> expectedMessageGroupsList = new List<IMessagingGroups>()
            {
                new MessagingGroups()
                {
                    Id = "MANAGEMENT",
                    Name = "Management",
                    IsAssigned = false
                },
                new MessagingGroups()
                {
                    Id = "RESIDENT_SERVICES",
                    Name = "Resident Services",
                    IsAssigned = false
                },
                new MessagingGroups()
                {
                    Id = "FRONT_DESK",
                    Name = "Front Desk",
                    IsAssigned = false
                },
                new MessagingGroups()
                {
                    Id = "MAINTENANCE",
                    Name = "Maintenance",
                    IsAssigned = false
                },
                new MessagingGroups()
                {
                    Id = "LEASING",
                    Name = "Leasing",
                    IsAssigned = false
                }
            };

            IList<CustomerCompanyMap> companyMapList = new List<CustomerCompanyMap>()
            {
                new CustomerCompanyMap()
                {
                    CompanyInstanceSourceId = _blueBookId.ToString(),
                    Source = BlueBookProductConstants.ResidentPortal
                }
            };

            _editorPersona.Organization.BooksCustomerMasterId = _blueBookId;

            mockManageBlueBook
                .Setup(m => m.GetCompanyMap(
                    It.IsAny<Guid>(),
                    It.IsAny<long>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()
                ))
                .Returns(companyMapList);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.Is<int>(l => l == (int)ProductEnum.ResidentPortal)
                ))
                .Returns(_productInternalSettings);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 4)
                    , It.Is<int>(l => l == (int)ProductEnum.ResidentPortal)
                ))
                .Returns(_editorSamlAttributes);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 5)
                    , It.Is<int>(l => l == (int)ProductEnum.ResidentPortal)
                ))
                .Returns(_userSamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 4)
                ))
                .Returns(_editorPersona);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 5)
                ))
                .Returns(_userPersona);

            mockProductRepository
                .Setup(m => m.GetProductSettingsByPersona(
                    It.IsAny<long>()
                ))
                .Returns(_userProductSettings);

            mockProductRepository
                .Setup(m => m.GetBooksMasterProductDetail(
                    It.IsAny<int>()
                ))
                .Returns(_gbProductMap);

            _residentPortalUser.MessagingGroups = expectedMessageGroupsList;

            //Act
            IManageProductResidentPortal manageProductResidentPortal = new ManageProductResidentPortal(
                editorRealPageId: _editorRealPageId,
                residentPortalEditorUser: _residentPortalEditorUser,
                residentPortalUser: _residentPortalUser,
                samlRepository: mockSamlRepository.Object,
                managePersona: mockManagePersona.Object,
                manageBlueBook: mockManageBlueBook.Object,
                productRepository: mockProductRepository.Object,
                productInternalSettingRepository: mockProductInternalSettingRepository.Object,
                managePerson: null,
                manageUserLogin: null,
                managePartyRelationship: null,
                manageElectronicAddress: null,
                userClaim: _userClaims,
                messageHandler: mockHttpMessageHandler.Object,
                repository: mockRepository.Object);

            //Assert
            _messageGroupsList = manageProductResidentPortal.ListMessageGroups(_editorPersonaId, _userPersonaId);
            List<IMessagingGroups> compareResult = _messageGroupsList.Where(item => expectedMessageGroupsList.Select(eItem => eItem.Id).Contains(item.Id)).ToList();
            Assert.True(_messageGroupsList.Count == expectedMessageGroupsList.Count);
            Assert.True(compareResult.Count == expectedMessageGroupsList.Count);
        }

        //[Fact]
        public void ListProperties_MockInputData_ReturnValidRepositoryResponseObject()
        {
            //Arrange
            Mock<IManageBlueBook> mockManageBlueBook = new Mock<IManageBlueBook>();
            Mock<IManagePersona> mockManagePersona = new Mock<IManagePersona>();
            Mock<IProductRepository> mockProductRepository = new Mock<IProductRepository>();
            Mock<IProductInternalSettingRepository> mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            Mock<ISamlRepository> mockSamlRepository = new Mock<ISamlRepository>();
            Mock<HttpMessageHandler> mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            List<ProductProperty> expectedProductPropertyList = new List<ProductProperty>()
            {
                new ProductProperty()
                {
                    ID = "4305",
                    Name = "Lakeline Crossing",
                    IsAssigned = false,
                    disableSelection = false
                },
                new ProductProperty()
                {
                    ID = "305",
                    Name = "The Fountains at Memorial City",
                    IsAssigned = false,
                    disableSelection = false
                },
                new ProductProperty()
                {
                    ID = "379",
                    Name = "7 Riverway",
                    IsAssigned = false,
                    disableSelection = false
                },
                new ProductProperty()
                {
                    ID = "779",
                    Name = "Instrata at The Ashton Uptown",
                    IsAssigned = false,
                    disableSelection = false
                },
                new ProductProperty()
                {
                    ID = "1146",
                    Name = "The Berkeley Apartment Homes",
                    IsAssigned = false,
                    disableSelection = false
                },
                new ProductProperty()
                {
                    ID = "3744",
                    Name = "The Carter",
                    IsAssigned = false,
                    disableSelection = false
                },
                new ProductProperty()
                {
                    ID = "4264",
                    Name = "2555 North Clark",
                    IsAssigned = false,
                    disableSelection = false
                },
                new ProductProperty()
                {
                    ID = "4288",
                    Name = "Canyon Springs at Bull Creek",
                    IsAssigned = false,
                    disableSelection = false
                }
            };

            IList<CustomerCompanyMap> companyMapList = new List<CustomerCompanyMap>()
            {
                new CustomerCompanyMap()
                {
                    CompanyInstanceSourceId = _blueBookId.ToString(),
                    Source = BlueBookProductConstants.ResidentPortal
                }
            };

            _editorPersona.Organization.BooksCustomerMasterId = _blueBookId;

            mockManageBlueBook
                .Setup(m => m.GetCompanyMap(
                    It.IsAny<Guid>(),
                    It.IsAny<long>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()
                ))
                .Returns(companyMapList);

            IList<PropertyInstance> propertyInstanceList = new List<PropertyInstance>()
            {
                new PropertyInstance()
                {
                    PropertyInstanceSourceId = "4305",
                    PropertyName = "Lakeline Crossing",
                    Address = new InstanceAddress()
                    {
                        State = "TX"
                    }
                },
                new PropertyInstance()
                {
                    PropertyInstanceSourceId = "305",
                    PropertyName = "The Fountains at Memorial City",
                    Address = new InstanceAddress()
                    {
                        State = "TX"
                    }
                },
                new PropertyInstance()
                {
                    PropertyInstanceSourceId = "379",
                    PropertyName = "7 Riverway",
                    Address = new InstanceAddress()
                    {
                        State = "TX"
                    }
                },
                new PropertyInstance()
                {
                    PropertyInstanceSourceId = "779",
                    PropertyName = "Instrata at The Ashton Uptown",
                    Address = new InstanceAddress()
                    {
                        State = "TX"
                    }
                },
                new PropertyInstance()
                {
                    PropertyInstanceSourceId = "1146",
                    PropertyName = "The Berkeley Apartment Homes",
                    Address = new InstanceAddress()
                    {
                        State = "TX"
                    }
                },
                new PropertyInstance()
                {
                    PropertyInstanceSourceId = "3744",
                    PropertyName = "The Carter",
                    Address = new InstanceAddress()
                    {
                        State = "TX"
                    }
                },
                new PropertyInstance()
                {
                    PropertyInstanceSourceId = "4264",
                    PropertyName = "2555 North Clark",
                    Address = new InstanceAddress()
                    {
                        State = "IL"
                    }
                },
                new PropertyInstance()
                {
                    PropertyInstanceSourceId = "4288",
                    PropertyName = "Canyon Springs at Bull Creek",
                    Address = new InstanceAddress()
                    {
                        State = "TX"
                    }
                }
            };

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.Is<int>(l => l == (int)ProductEnum.ResidentPortal)
                ))
                .Returns(_productInternalSettings);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 4)
                    , It.Is<int>(l => l == (int)ProductEnum.ResidentPortal)
                ))
                .Returns(_editorSamlAttributes);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 5)
                    , It.Is<int>(l => l == (int)ProductEnum.ResidentPortal)
                ))
                .Returns(_userSamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 4)
                ))
                .Returns(_editorPersona);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 5)
                ))
                .Returns(_userPersona);

            mockProductRepository
                .Setup(m => m.GetProductSettingsByPersona(
                    It.IsAny<long>()
                ))
                .Returns(_userProductSettings);

            //Act
            IManageProductResidentPortal manageProductResidentPortal = new ManageProductResidentPortal(
                editorRealPageId: _editorRealPageId,
                companyInstanceId: 853322,
                samlRepository: mockSamlRepository.Object,
                managePersona: mockManagePersona.Object,
                manageBlueBook: mockManageBlueBook.Object,
                productRepository: mockProductRepository.Object,
                productInternalSettingRepository: mockProductInternalSettingRepository.Object,
                managePerson: null,
                manageUserLogin: null,
                managePartyRelationship: null,
                userClaim: _userClaims,
                messageHandler: mockHttpMessageHandler.Object,
                repository: mockRepository.Object);

            //Assert
            _listResponse = manageProductResidentPortal.ListProperties(_editorPersonaId, _userPersonaId, null);
            IList<ProductProperty> productPropertyList = _listResponse.Records.Cast<ProductProperty>().ToList();
            List<ProductProperty> compareResult = productPropertyList.Where(item => expectedProductPropertyList.Select(eItem => eItem.ID).Contains(item.ID)).ToList();
            Assert.True(_listResponse.Records.Count == expectedProductPropertyList.Count);
            Assert.True(compareResult.Count == expectedProductPropertyList.Count);
        }

        #endregion

        #region Migration tests

        [Fact]
        public void GetMigrationUsers_GivenEditorIdAndDataFilter_ShouldReturnListOfResidentPortalUsers()
        {
            //Arrange
            Mock<IManageBlueBook> mockManageBlueBook = new Mock<IManageBlueBook>();
            Mock<IManagePersona> mockManagePersona = new Mock<IManagePersona>();
            Mock<IProductRepository> mockProductRepository = new Mock<IProductRepository>();
            Mock<IProductInternalSettingRepository> mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            Mock<ISamlRepository> mockSamlRepository = new Mock<ISamlRepository>();
            Mock<HttpMessageHandler> mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            IList<CustomerCompanyMap> companyMapList = new List<CustomerCompanyMap>()
            {
                new CustomerCompanyMap()
                {
                    CompanyInstanceSourceId = _blueBookId.ToString(),
                    Source = BlueBookProductConstants.ResidentPortal
                }
            };

            _editorPersona.Organization.BooksCustomerMasterId = _blueBookId;

            //mockManageBlueBook
            //    .Setup(m => m.GetCompanyMap(
            //        It.IsAny<Guid>(),
            //        It.IsAny<long>(),
            //        It.IsAny<string>(),
            //        It.IsAny<string>(),
            //        It.IsAny<string>(),
            //        It.IsAny<bool>(),
            //        It.IsAny<bool>()
            //    ))
            //    .Returns(companyMapList);

            mockProductInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(
                    It.Is<int>(l => l == (int)ProductEnum.ResidentPortal)
                ))
                .Returns(_productInternalSettings);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 4)
                    , It.Is<int>(l => l == (int)ProductEnum.ResidentPortal)
                ))
                .Returns(_editorSamlAttributes);

            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 5)
                    , It.Is<int>(l => l == (int)ProductEnum.ResidentPortal)
                ))
                .Returns(_userSamlAttributes);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 4)
                ))
                .Returns(_editorPersona);

            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 5)
                ))
                .Returns(_userPersona);

            mockProductRepository
                .Setup(m => m.GetProductSettingsByPersona(
                    It.IsAny<long>()
                ))
                .Returns(_userProductSettings);

            mockProductRepository
                .Setup(m => m.GetBooksMasterProductDetail(
                    It.IsAny<int>()
                ))
                .Returns(_gbProductMap);

            //Act

            var filter = "NonMigrated";
            var dataFilter = new RequestParameter()
            {
                Pages = new PageRequest()
                {
                    StartRow = 1,
                    ResultsPerPage = 1000
                },
                FilterBy = new Dictionary<string, string>()
                {
                    { "filter", "NonMigrated" }
                }
            };
            var totalRecords = 5;
            var url = $"{_mtApiEndPoint}/{_blueBookId}/users?filter={filter}&app_id={_appId}&app_key={_appKey}";
            var editorPersonaId = _editorPersonaId;
            var actual = new List<ResidentPortalMigrationUser>()
            {
                new ResidentPortalMigrationUser() { FirstName = "Person", LastName = "1", Email = "person1@test.com", Username = "person1" },
                new ResidentPortalMigrationUser()
                {
                    FirstName = "Person", LastName = "2", Email = "person2@test.com", Username = "person2", Properties = new List<MigrationProperty>()
                    {
                        new MigrationProperty() { PropertyInstanceSourceId = "1" }, new MigrationProperty() { PropertyInstanceSourceId = "2" }
                    }
                },
                new ResidentPortalMigrationUser() { FirstName = "Person", LastName = "3", Email = "person3@test.com", Username = "person3" },
                new ResidentPortalMigrationUser() { FirstName = "Person", LastName = "4", Email = "person4@test.com", Username = "person4" },
                new ResidentPortalMigrationUser() { FirstName = "Person", LastName = "5", Email = "person5@test.com", Username = "person5" }
            };
            HttpResponseMessage userResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(actual))
            };

            mockHttpMessageHandler.Setup(HttpMethod.Get, url, userResponse);

            var responseMapResource = new HttpResponseMessage(HttpStatusCode.OK);
            var jsonToSave = JsonConvert.SerializeObject(companyMapList, new JsonApiSerializerSettings());
            responseMapResource.Content = new StringContent(jsonToSave);

            mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customercompanymap?filter[companyInstance.greenBookCares]=true&filter[customerCompanyId]={_blueBookId}&include=companyInstance&include=companyInstance.attributes&filter[source]=AB", responseMapResource);

            var manageProductResidentPortal = new ManageProductResidentPortal(
                editorRealPageId: _editorRealPageId,
                residentPortalEditorUser: _residentPortalEditorUser,
                residentPortalUser: _residentPortalUser,
                samlRepository: mockSamlRepository.Object,
                managePersona: mockManagePersona.Object,
                manageBlueBook: mockManageBlueBook.Object,
                productRepository: mockProductRepository.Object,
                productInternalSettingRepository: mockProductInternalSettingRepository.Object,
                managePerson: null,
                manageUserLogin: null,
                managePartyRelationship: null,
                manageElectronicAddress: null,
                userClaim: _userClaims,
                messageHandler: mockHttpMessageHandler.Object,
                repository: mockRepository.Object);

            //Act
            var expected = manageProductResidentPortal.GetMigrationUsers(editorPersonaId, dataFilter);

            //Assert
            Assert.IsType<MigrationUser>(expected.Records[0]);
            Assert.True(expected.Records.Count == totalRecords);
            Assert.Same(expected.ErrorReason, "");
        }

        [Fact]
        public void UpdateUsersMigrationStatus_GivenEditorIdUserIdAndMigrateUser_ShouldReturnSuccessMessage()
        {
            //Arrange
            //Mock<IManageBlueBook> mockManageBlueBook = new Mock<IManageBlueBook>();
            Mock<IManagePersona> mockManagePersona = new Mock<IManagePersona>();
            Mock<IProductRepository> mockProductRepository = new Mock<IProductRepository>();
            Mock<IProductInternalSettingRepository> mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            Mock<ISamlRepository> mockSamlRepository = new Mock<ISamlRepository>();
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            IList<CustomerCompanyMap> companyMapList = new List<CustomerCompanyMap>()
            {
                new CustomerCompanyMap()
                {
                    CompanyInstanceSourceId = _blueBookId.ToString(),
                    Source = BlueBookProductConstants.ResidentPortal
                }
            };

            _editorPersona.Organization.BooksCustomerMasterId = _blueBookId;

            //mockManageBlueBook
            //    .Setup(m => m.GetCompanyMap(
            //        It.IsAny<Guid>(),
            //        It.IsAny<long>(),
            //        It.IsAny<string>(),
            //        It.IsAny<string>(),
            //        It.IsAny<string>(),
            //        It.IsAny<bool>(),
            //        It.IsAny<bool>()
            //    ))
            //    .Returns(companyMapList);
            
            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 4)
                    , It.Is<int>(l => l == (int)ProductEnum.ResidentPortal)
                ))
                .Returns(_editorSamlAttributes);
            
            mockSamlRepository
                .Setup(m => m.GetProductSamlDetails(
                    It.Is<long>(l => l == 5)
                    , It.Is<int>(l => l == (int)ProductEnum.ResidentPortal)
                ))
                .Returns(_userSamlAttributes);
            
            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 4)
                ))
                .Returns(_editorPersona);
            
            mockManagePersona
                .Setup(m => m.GetPersona(
                    It.Is<long>(l => l == 5)
                ))
                .Returns(_userPersona);
            
            var migrateUsers = new List<MigrateUser>()
            {
                new MigrateUser()
                {
                    UserId = "123456",
                    UnifiedLoginUserName = "abc@test.com",
                    UsingUnifiedLogin = true
                },
                new MigrateUser()
                {
                    UserId = "123457",
                    UnifiedLoginUserName = "abc@test.com",
                    UsingUnifiedLogin = true
                }
            };
            var expected = new MigrateResponse()
            {
                Message = "Success",
                Status = true
            };
            var url = $"{_mtApiEndPoint}/{_blueBookId}/migrate-users?app_id={_appId}&app_key={_appKey}";
            var userResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(expected))
            };

            mockHttpMessageHandler.Setup(HttpMethod.Put, url, userResponse);

            var responseMapResource = new HttpResponseMessage(HttpStatusCode.OK);
            var jsonToSave = JsonConvert.SerializeObject(companyMapList, new JsonApiSerializerSettings());
            responseMapResource.Content = new StringContent(jsonToSave);

            mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customercompanymap?filter[companyInstance.greenBookCares]=true&filter[customerCompanyId]={_blueBookId}&include=companyInstance&include=companyInstance.attributes&filter[source]=AB", responseMapResource);

            var manageProductResidentPortal = new ManageProductResidentPortal(
                editorRealPageId: _editorRealPageId,
                residentPortalEditorUser: _residentPortalEditorUser,
                residentPortalUser: _residentPortalUser,
                samlRepository: mockSamlRepository.Object,
                managePersona: mockManagePersona.Object,
                manageBlueBook: null,//new ManageBlueBook(_editorUserClaim, mockRepository.Object, mockHttpMessageHandler2.Object),
                productRepository: mockProductRepository.Object,
                productInternalSettingRepository: mockProductInternalSettingRepository.Object,
                managePerson: null,
                manageUserLogin: null,
                managePartyRelationship: null,
                manageElectronicAddress: null,
                userClaim: _userClaims,
                messageHandler: mockHttpMessageHandler.Object,
                repository: mockRepository.Object);

            //Act
            var actual = manageProductResidentPortal.UpdateUsersMigrationStatus(_editorPersonaId, migrateUsers);

            _output.WriteLine("actual :" + JsonConvert.SerializeObject(actual));
            //Assert
            Assert.Equal(actual.Message, expected.Message);
            Assert.True(actual.Status);
        }

        #endregion

        private bool TestIsProductId(object obj, int value)
        {
            return obj.ToString().ToLower().Contains($"productid = {value}");
        }
    }
}
