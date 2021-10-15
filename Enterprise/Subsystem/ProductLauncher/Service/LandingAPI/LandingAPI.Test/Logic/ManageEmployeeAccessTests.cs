using Castle.Components.DictionaryAdapter;
using Moq;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.OneSite;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UserManagement;
using Xunit;
using Newtonsoft.Json;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic
{
    [ExcludeFromCodeCoverage]
    public class ManageEmployeeAccessTests
    {
        private DefaultUserClaim _defaultUserClaim = new DefaultUserClaim()
        {
            //UserId = 1,
            LoginName = "MocTest",
            CorrelationId = Guid.NewGuid(),
            OrganizationName = "MocTest",
            OrganizationPartyId = 1,
            OrganizationRealPageGuid = Guid.NewGuid(),
            OrganizationMasterId = 1,
            UserRealPageGuid = Guid.NewGuid(),
            PersonaId = 33,

        };
        private Mock<IRepository> _mockRepository = new Mock<IRepository>();
        Mock<HttpMessageHandler> _mockMessageHandler = new Mock<HttpMessageHandler>();
        private Mock<IOneSiteProductService> _mockOneSiteProductService = new Mock<IOneSiteProductService>();

        private Guid _organizationRealPageId = new Guid("9E9410AE-2C41-47D2-81D1-109C08CD151C");
        private string _loginName = "james@test.com";
        private Guid _userRealPageId = new Guid("9E9410AE-1111-2222-3333-109C08CD151C");
        private IList<UserOrganization> _userOrganizationList;

        private List<Persona> _personaList = new List<Persona>();
        private IList<Right> _rightList;
        private IList<Role> _roleList;

        private Persona _regularUserPersona;
        private Persona _superUserPersona;
        private List<OrganizationDomain> _organizationDomainList;
        private List<OrganizationType> _organizationTypeList;
        private List<OrganizationStatus> _orgStatusList;

        private static string _mtApiEndPoint = "api/core/common/ulmigration";
        private static string _mtTokenUrl = "api/core/authentication/login";
        private static string _mtClientId = "OneSiteClient";
        private static string _mtClientSecret = "OneSiteClientSecret";
        private static Guid _invalidRealPageId = new Guid("11111111-1111-2222-3333-3C0F434AE62D");
        private static string _CompanyName = "CF Real Estate Services";
        private static DateTime _CreateDate = DateTime.MaxValue.ToUniversalTime();
        private static int _PartyId = 54321;
        private static long _BooksMasterId = 2116;
        private static long _BooksCompanyMasterId = 379;
        private static int _organizationTypeId = 6;
        private static int _organizationDomainId = 1;
        private List<Organization> _organizationList;

        List<dynamic> _companyList = new List<dynamic>();

        public ManageEmployeeAccessTests()
        {
            _userOrganizationList = new List<UserOrganization>()
            {
                new UserOrganization()
                {
                    Name = "SuperUser",
                    OrganizationPartyId = 3,
                    OrganizationRealPageId = _organizationRealPageId,
                    PartyRoleTypeId = 402,
                }
            };

            _organizationDomainList = new List<OrganizationDomain>()
            {
                new OrganizationDomain()
                {
                    OrganizationDomainId = 1,
                    Name = "Primary",
                    CreateDate = new DateTime()
                }
            };
            _organizationTypeList = new List<OrganizationType>()
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

            //_orgStatusList = new EditableList<OrganizationStatus>() { organizationStatus };

            _rightList = new List<Right>()
            {
                new Right()
                {
                    RightId = 672,
                    RightName = "Access Reports",
                    RightValueTypeId = 47,
                    RightNickName = "reports.view"
                },
                new Right()
                {
                    RightId = 667,
                    RightName = "Create master properties",
                    RightValueTypeId = 36,
                    RightNickName = "property.create"
                }
            };

            _roleList = new List<Role>()
            {
                new Role()
                {
                    RoleID = 81,
                    Name = "Black-Book Director",
                    PersonaId = "505",
                    Right = _rightList
                }
            };

            _superUserPersona = new Persona()
            {
                FromDate = DateTime.UtcNow,
                Name = "Super User",
                Organization = new Organization() { RealPageId = _organizationRealPageId },
                PersonaId = 33,
                RealPageId = _userRealPageId,
                ThruDate = DateTime.UtcNow.AddDays(1),
                UserId = 11,
                Role = _roleList,
                hasResidentPortalUserAccess = true
            };

            _personaList.Add(_superUserPersona);

            _regularUserPersona = new Persona()
            {
                FromDate = DateTime.UtcNow,
                Name = "Regular User",
                Organization = new Organization() { RealPageId = _superUserPersona.Organization.RealPageId },
                PersonaId = 1234,
                RealPageId = _userRealPageId,
                ThruDate = DateTime.UtcNow.AddDays(5),
                UserId = 1234,
                Role = _roleList,
                hasResidentPortalUserAccess = false
            };

            _personaList.Add(_regularUserPersona);

            _organizationList = new List<Organization>()
            {
                new Organization()
                {
                    RealPageId = new Guid("9E9410AE-2C41-47D2-81D1-109C08CD151C"),
                    CreateDate = _CreateDate,
                    Name = "Onesite Test Company",
                    PartyId = 12345,
                    BooksMasterId = 123456,
                    BooksCustomerMasterId = 1234567,
                    organizationType = new OrganizationType()
                    {
                        OrganizationTypeId = _organizationTypeId,
                        Name = "Multifamily",
                        CreateDate = new DateTime()
                    },
                    OrganizationTypeId = _organizationTypeId,
                    OrganizationDomainId = 1,
                    OrganizationDomain = new OrganizationDomain()
                    {
                        OrganizationDomainId = 1,
                        Name = "Primary",
                        CreateDate = new DateTime()
                    },
                },
                new Organization()
                {
                    RealPageId = new Guid("22222222-2222-2222-2222-222222222222"),
                    CreateDate = _CreateDate,
                    Name = "Onesite Invalid Test Company",
                    PartyId = 54321,
                    BooksMasterId = 654321,
                    BooksCustomerMasterId = 7654321,
                    organizationType = new OrganizationType()
                    {
                        OrganizationTypeId = _organizationTypeId,
                        Name = "Multifamily",
                        CreateDate = new DateTime()
                    },
                    OrganizationTypeId = _organizationTypeId,
                    OrganizationDomainId = 1,
                    OrganizationDomain = new OrganizationDomain()
                    {
                        OrganizationDomainId = 1,
                        Name = "Primary",
                        CreateDate = new DateTime()
                    },
                }

            };
           
            
            //List<dynamic> companyList2 = new List<dynamic>();
            var userLogin = new UserLogin() { UserId = 1234, LoginName = "admin@test.com", RealPageId = new Guid("99999999-9999-9999-9999-999999999999") };

            var org = _organizationList.FirstOrDefault(p => p.PartyId == 12345);
            string _mockJsonCompanyList = "{\r\n\t\t\"PartyId\": \"" + org.PartyId + "\",\r\n\t\t\"Name\": \"" + org.Name + "\",\r\n\t\t\"OrganizationRealPageId\": \"" + org.RealPageId + "\",\r\n\t\t\"BooksMasterId\": \"" + org.BooksMasterId + "\",\r\n\t\t\"BooksCustomerMasterId\": \"" + org.BooksCustomerMasterId + "\",\r\n\t\t\"SettingName\": \"RealPageEmployeeAccessID\",\r\n\t\t\"PersonRealPageId\": \"" + userLogin.RealPageId + "\",\r\n\t\t\"LoginName\": \"" + userLogin.LoginName + "\",\r\n\t}";
            _companyList.Add(JsonConvert.DeserializeObject<dynamic>(_mockJsonCompanyList));

            var org2 = _organizationList.FirstOrDefault(p => p.PartyId == 54321);
            //_mockJsonCompanyList = "{\r\n\t\t\"PartyId\": \"" + org2.PartyId + "\",\r\n\t\t\"Name\": \"" + org2.Name + "\",\r\n\t\t\"OrganizationRealPageId\": \"" + org2.RealPageId + "\",\r\n\t\t\"BooksMasterId\": \"" + org2.BooksMasterId + "\",\r\n\t\t\"BooksCustomerMasterId\": \"" + org2.BooksCustomerMasterId + "\",\r\n\t\t\"SettingName\": \"RealPageEmployeeAccessID\",\r\n\t\t\"PersonRealPageId\": \"" + userLogin2.RealPageId + "\",\r\n\t\t\"LoginName\": \"" + userLogin2.LoginName + "\",\r\n\t}";
            //companyList2.Add(JsonConvert.DeserializeObject<dynamic>(_mockJsonCompanyList));

            OrganizationStatus organizationStatus = new OrganizationStatus()
            {
                PartyId = org.PartyId,
                IsPending = true,
                IsActive = true,
                IsExpired = false,
                StatusTypeId = (int)UserUiStatusType.Active,
                Status = UserUiStatusType.Active,
                FromDate = new DateTime(2019, 1, 1)
            };

            var orgStatusList = new EditableList<OrganizationStatus>() { organizationStatus };

            OrganizationStatus organizationStatus2 = new OrganizationStatus()
            {
                PartyId = org2.PartyId,
                IsPending = true,
                IsActive = true,
                IsExpired = false,
                StatusTypeId = (int)UserUiStatusType.Active,
                Status = UserUiStatusType.Active,
                FromDate = new DateTime(2019, 1, 1)
            };

            var org2StatusList = new EditableList<OrganizationStatus>() { organizationStatus2 };

            var userOrganizationList = new List<UserOrganization>()
            {
                new UserOrganization()
                {
                    Name = org.Name,
                    OrganizationPartyId = org.PartyId,
                    OrganizationRealPageId = org.RealPageId,
                    PartyRoleTypeId = 402,
                    PersonaId = 4444
                }
            };

            var userOrganizationList2 = new List<UserOrganization>()
            {
                new UserOrganization()
                {
                    Name = org2.Name,
                    OrganizationPartyId = org2.PartyId,
                    OrganizationRealPageId = org2.RealPageId,
                    PartyRoleTypeId = 402,
                    PersonaId = 5555
                }
            };
        }

        [Fact]
        public void AddEmployeeToProduct()
        {

            IList<ProductInternalSetting> _productInternalSettingsOneSite = new List<ProductInternalSetting>();
            _productInternalSettingsOneSite.Add(new ProductInternalSetting() { Name = "MTAPiEndPoint", Value = _mtApiEndPoint });
            _productInternalSettingsOneSite.Add(new ProductInternalSetting() { Name = "MTTokenEndPoint", Value = _mtTokenUrl });
            _productInternalSettingsOneSite.Add(new ProductInternalSetting() { Name = "MTClientId", Value = _mtClientId });
            _productInternalSettingsOneSite.Add(new ProductInternalSetting() { Name = "MTClientSECRET", Value = _mtClientSecret });

            var org = _organizationList.FirstOrDefault(p => p.PartyId == 12345);

            _mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.Is<object>(data => TestSqlParameter(data, "{ ProductId = 1 }"))))
                .Returns(_productInternalSettingsOneSite);

            _mockRepository
                .Setup(m => m.GetOne<Persona>(StoredProcNameConstants.SP_GetPersona, It.Is<object>(data => TestSqlParameter(data, "{ personaId = " + _regularUserPersona.PersonaId + " }"))))
                .Returns(_regularUserPersona);

            _mockRepository
                .Setup(m => m.GetMany<dynamic>(StoredProcNameConstants.SP_ListOrganizations,
                    It.Is<object>(
                        d => TestIsRealPageId(d, org.RealPageId))))
                .Returns(_companyList);

            var manageEmployeeAccess = new ManageEmployeeAccess(_defaultUserClaim, _mockRepository.Object, _mockMessageHandler.Object, _mockOneSiteProductService.Object);
            manageEmployeeAccess.CreateEmployeeProductUser(1, 1234);

        }

        private bool TestSqlParameter(object p, string value)
        {
            return value.Equals(p.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        private bool TestSqlParameterContains(object p, string value)
        {
            return p.ToString().ToLower().Contains(value);
        }

        private bool TestIsRealPageIdNull(object obj)
        {
            return obj.ToString().Contains($"RealPageId = {_invalidRealPageId}");
        }

        private bool TestIsRealPageId(object obj, Guid? realPageId)
        {
            if (obj == null && realPageId == null)
            {
                return true;
            }

            if (obj == null)
            {
                return false;
            }

            return obj.ToString().ToLower().Contains($"realpageid = {realPageId}");
        }
    }
}
