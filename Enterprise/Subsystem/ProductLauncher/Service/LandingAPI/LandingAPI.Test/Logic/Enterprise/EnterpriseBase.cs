using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic.Enterprise
{
    public class EnterpriseBase
    {
        //protected static DefaultUserClaim _defaultUserClaim = new DefaultUserClaim();
        //protected static int _PartyId = 54321;
        protected static long _BooksMasterId = 2116;
        protected static long _multifamilyUserPersonaId = 1234;
        protected static Persona _multifamilyUserPersona;
        protected static int _multifamilyUserId = 9090;
        protected static Guid _multifamilyUserRealpageId = new Guid("C1FACB08-0715-42B0-AD11-AF0527990064");
        protected static Guid _multifamilyCompanyRealPageId = new Guid("C802694D-5553-4527-8616-3C0F434AE62D");
        protected static Person _multifamilyUserPerson;

        protected static long _BooksCompanyMasterId = 379;
        protected static long _vendorCompanyPartyId = 54321;

        protected static long _vendorUserPersonaId = 4321;
        protected static Guid _vendorUserRealpageId = new Guid("E8E5B2B0-7ECC-4ADE-AE65-7CA82BE1B7D9");
        protected static Persona _vendorUserPersona;
        protected static int _vendorUserId = 8778;

        protected static long _multifamilyCompanyPartyId = 12345;

        protected List<GbProductMap> _gbProductMap;
        protected List<ProductInternalSetting> _productInternalSettings;
        protected List<OrganizationType> _organizationTypes;
        protected List<OrganizationDomain> _organizationDomains;
        protected List<ProductSettingType> _productSettingTypes;
        protected List<ProductInternalSettingByType> _productInternalSettingByType;
        protected List<Organization> _organizationList;
        protected List<dynamic> _companyListApiResult = new List<dynamic>();

        protected List<ProductUI> _productUiList;
        protected List<ProductExampleRole> _productExampleRoleList;
        protected List<ProductExampleUserRole> _productExampleUserRoleList;
        protected List<ProductExampleRoleRight> _productExampleRoleRights;
        protected List<ProductExampleRoleRight> _productExampleImpersonationRoleRights;

        protected Person _companySupportToolAdminPerson;
        protected Person _vendorUserPerson;

        protected UserLogin _companySupportToolAdminUserLogin;
        protected UserLoginOnly _companySupportToolAdminuserLoginOnly;
        protected Persona _companySupportToolAdminPersona;
        
        protected static long _companySupportToolAdminPersonaId = 1235;
        protected static long _companySupportToolAdminUserId = 3322;
        protected static string _loginName = "adminuser@test.com";
        protected static Guid _userRealPageId = new Guid("9E9410AE-1111-2222-3333-109C08CD151C");
        protected static Guid _companySupportToolAdminUserRealPageId = new Guid("9E9410AE-1111-2222-3333-109C08CD151C");

        public EnterpriseBase()
        {
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
                new GbProductMap() { BooksProductCode = "HAAS", Name = "Home Sharing", ProductId = 60, UDMSourceCode = null },
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

            _productUiList = new List<ProductUI>();
            foreach (var gbProductMap in _gbProductMap)
            {
                _productUiList.Add(new ProductUI() { ProductCode = gbProductMap.BooksProductCode, ProductId = gbProductMap.ProductId });
            }

            _productInternalSettings = new List<ProductInternalSetting>()
            {
                new ProductInternalSetting() { Name = "BooksUseDomains", Value = "1" },
                new ProductInternalSetting() { Name = "BooksUseUPFMId", Value = "1" },
                new ProductInternalSetting() { Name = "ShowInUserDetails", Value = "1" },
                new ProductInternalSetting() { Name = "MTAPiEndPoint", Value = "api/core/common/ulmigration" },
                new ProductInternalSetting() { Name = "MTTokenEndPoint", Value = "api/core/authentication/login" },
                new ProductInternalSetting() { Name = "MTClientId", Value = "OneSiteClient" },
                new ProductInternalSetting() { Name = "MTClientSECRET", Value = "OneSiteClientSecret" },
            };

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

            _organizationList = new List<Organization>()
            {
                new Organization()
                {
                    RealPageId = _multifamilyCompanyRealPageId,
                    CreateDate = new DateTime(),
                    Name = "Test Company",
                    PartyId = _multifamilyCompanyPartyId,
                    BooksMasterId = 123456,
                    BooksCustomerMasterId = 1234567,
                    organizationType = new OrganizationType()
                    {
                        OrganizationTypeId = 6,
                        Name = "Multifamily",
                        CreateDate = new DateTime()
                    },
                    OrganizationTypeId = 6,
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
                    CreateDate = new DateTime(),
                    Name = "Vendor Company",
                    PartyId = _vendorCompanyPartyId,
                    BooksMasterId = 654321,
                    BooksCustomerMasterId = 7654321,
                    organizationType = new OrganizationType()
                    {
                        OrganizationTypeId = 14,
                        Name = "Vendor",
                        CreateDate = new DateTime()
                    },
                    OrganizationTypeId = 14,
                    OrganizationDomainId = 1,
                    OrganizationDomain = new OrganizationDomain()
                    {
                        OrganizationDomainId = 1,
                        Name = "Primary",
                        CreateDate = new DateTime()
                    },
                }
            };

            _vendorUserPerson = new Person() { FirstName = "Larry", LastName = "Wilson", RealPageId = _vendorUserRealpageId };
            _vendorUserPersona = new Persona() { PersonaId = _vendorUserPersonaId, RealPageId = _vendorUserRealpageId, OrganizationPartyId = _vendorCompanyPartyId, UserId = _vendorUserId };

            _multifamilyUserPerson = new Person() { FirstName = "Multi", LastName = "User", RealPageId = _multifamilyUserRealpageId };
            _multifamilyUserPersona = new Persona() { PersonaId = _multifamilyUserPersonaId, RealPageId = _multifamilyUserRealpageId, OrganizationPartyId = _multifamilyCompanyPartyId, UserId = _multifamilyUserId };
            
            _companySupportToolAdminPerson = new Person() { FirstName = "Bob", LastName = "Jones", RealPageId = _companySupportToolAdminUserRealPageId };
            _companySupportToolAdminuserLoginOnly = new UserLoginOnly() { UserId = 1234, LoginName = _loginName, RealPageId = _companySupportToolAdminUserRealPageId };
            _companySupportToolAdminPersona = new Persona() { PersonaId = _companySupportToolAdminPersonaId, RealPageId = _companySupportToolAdminUserRealPageId, OrganizationPartyId = _vendorCompanyPartyId, UserId = _companySupportToolAdminUserId };
            _companySupportToolAdminUserLogin = new UserLogin() { UserId = 1234, LoginName = "admin@test.com", RealPageId = _companySupportToolAdminUserRealPageId };

            var org = _organizationList.FirstOrDefault(p => p.PartyId == 12345);
            var _mockJsonCompanyList = "{\r\n\t\t\"PartyId\": \"" + org.PartyId + "\",\r\n\t\t\"Name\": \"" + org.Name + "\",\r\n\t\t\"OrganizationRealPageId\": \"" + org.RealPageId + "\",\r\n\t\t\"BooksMasterId\": \"" + org.BooksMasterId + "\",\r\n\t\t\"BooksCustomerMasterId\": \"" + org.BooksCustomerMasterId + "\",\r\n\t\t\"SettingName\": \"RealPageEmployeeAccessID\",\r\n\t\t\"PersonRealPageId\": \"" + _companySupportToolAdminUserRealPageId + "\",\r\n\t\t\"LoginName\": \"" + _companySupportToolAdminUserLogin.LoginName + "\",\r\n\t}";
            _companyListApiResult.Add(JsonConvert.DeserializeObject<dynamic>(_mockJsonCompanyList));

            _productExampleRoleList = new List<ProductExampleRole>()
            {
                new ProductExampleRole()
                {
                    Value = "Basic End User",
                    RoleNickName = "User",
                    RoleId = 2,
                    RoleType = "System",
                    DefaultRole = 1,
                    RoleAttribute = ""
                },
                new ProductExampleRole()
                {
                    Value = "Basic End User & CIMPL",
                    RoleNickName = null,
                    RoleId = 528,
                    RoleType = "Custom",
                    DefaultRole = 0,
                    RoleAttribute = ""
                },
                new ProductExampleRole()
                {
                    Value = "Platform Administrator",
                    RoleNickName = "SuperUser",
                    RoleId = 1,
                    RoleType = "System",
                    DefaultRole = 0,
                    RoleAttribute = ""
                }
            };

            _productExampleUserRoleList = new List<ProductExampleUserRole>()
            {
                new ProductExampleUserRole()
                {
                    PersonaId = _multifamilyUserPersonaId,
                    OrganizationPartyId = _multifamilyCompanyPartyId,
                    RoleId = 1,
                    RoleType = "System",
                    Role = "Platform Administrator",
                    Product = 3,
                    RoleNickName = "SuperUser"
                }
            };

            _productExampleRoleRights = new List<ProductExampleRoleRight>()
            {
                new ProductExampleRoleRight()
                {
                    Right = "Execute and Close Contracts",
                    RightNickName = "ExecuteandCloseContracts",
                    RightValueTypeId = 520
                },
                new ProductExampleRoleRight()
                {
                    Right = "Access to Bids & Contracts in Vendor Marketplace",
                    RightNickName = "AccesstoBids&ContractsinVendorMarketplace",
                    RightValueTypeId = 519
                },
                new ProductExampleRoleRight()
                {
                    Right = "Approve or Reject Contracts",
                    RightNickName = "ApproveorRejectContracts",
                    RightValueTypeId = 524
                },
                new ProductExampleRoleRight()
                {
                    Right = "Award Bids",
                    RightNickName = "AwardBids",
                    RightValueTypeId = 521
                },
                
            };

            _productExampleImpersonationRoleRights = new List<ProductExampleRoleRight>()
            {
                new ProductExampleRoleRight()
                {
                    Right = "Access to Bids & Contracts in Vendor Marketplace",
                    RightNickName = "AccesstoBids&ContractsinVendorMarketplace",
                    RightValueTypeId = 519
                },
            };

            _productSettingTypes = new List<ProductSettingType>()
            {
                new ProductSettingType()
                {
                    ProductSettingTypeId = 1, Name = "ProductStatus", SensitiveData = false
                },
                new ProductSettingType()
                {
                    ProductSettingTypeId = 2, Name = "ApiSecret", SensitiveData = true
                },
                new ProductSettingType()
                {
                    ProductSettingTypeId = 3, Name = "ProductIntegrationType", SensitiveData = true
                }
            };

            _productInternalSettingByType = new List<ProductInternalSettingByType>()
            {
                new ProductInternalSettingByType
                {
                    ProductConfigurationId = "1234",
                    Name = "ProductIntegrationType",
                    Value = "UPFM",
                    ProductId = 38,
                    ProductName = "Vendor Marketplace",
                    BooksProductCode = "VMP"
                },
            };
        }

        public bool TestSqlParameter(object p, string value)
        {
            return value.Equals(p.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public bool TestSqlParameterContains(object p, string value)
        {
            return p.ToString().ToLower().Contains(value.ToLower());
        }
    }

    public class ProductExampleRole
    {
        [JsonProperty("value")]
        [JsonPropertyName("value")]
        public string Value { get; set; }
        public string RoleNickName { get; set; }
        public int RoleId { get; set; }
        public string RoleType { get; set; }
        public int DefaultRole { get; set; }
        public string RoleAttribute { get; set; }
    }

    public class ProductExampleUserRole
    {
        public string Role { get; set; }
        public string RoleNickName { get; set; }
        public int RoleId { get; set; }
        public int Product { get; set; }
        public string RoleType { get; set; }
        public long PersonaId { get; set; }
        public long OrganizationPartyId { get; set; }
    }

    public class ProductExampleRoleRight
    {
        public int RightValueTypeId { get; set; }
        public string Right { get; set; }
        public string RightNickName { get; set; }
        public bool Assigned { get; set; }
    }

}
