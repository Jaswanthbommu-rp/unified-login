using Moq;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Accounting;
using UnifiedLogin.SharedObjects.Saml;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using IC = UnifiedLogin.SharedObjects.IdentityConfig;

namespace UnifiedLogin.LandingAPI.Test.Logic
{
    [ExcludeFromCodeCoverage]
	public class ManageProductBaseTests : TestBase
    {
        protected int _productId;

        protected static string _ROLETYPE_NAME_USER = "User";
        protected static string _ROLETYPE_NAME_SUPERUSER = "SuperUser";
        protected static string _ROLETYPE_NAME_REALPAGE_EMPLOYEE = "RealPage Employee";
        protected static string _ROLETYPE_NAME_USER_NOEMAIL = "User (No Email)";

        protected static string _uniqueIdentifier = "1234567|userlogin";

		protected long _editorPersonaId = 4;
        protected int _editorUserId = 14;
        protected Guid _editorRealPageId = new Guid("523C6677-C20D-4E6A-A4CC-0DE5781F0D5C");
        protected int _editorOrganizationPartyId = 1234;
        private Guid _editorOrganizationRealPageId = new Guid("12345678-C20D-4E6A-A4CC-0DE5781F0D5C");
		private Guid _editorCorrelationId = new Guid("8C5F223C-169A-44BD-9844-F925B5F0C332");
		protected DefaultUserClaim _editorUserClaim;

		protected long _userPersonaId = 5;
        protected int _userUserId = 15;
        protected Guid _userRealPageId = new Guid("623C6677-D20D-5E6A-B4CC-1DE5781F0D5C");
        protected Guid _userOrganizationRealPageId = new Guid("12345678-C20D-4E6A-A4CC-0DE5781F0D5C");
        protected int _userOrganizationPartyId = 1234;
		private Guid _userCorrelationId = new Guid("078724B2-D381-4E45-9EE9-6DD6D9B9B74B");
		protected DefaultUserClaim _userUserClaim;

		protected long _newUserPersonaId = 7;
        protected int _newUserUserId = 17;
        private Guid _newUserRealPageId = new Guid("523C6677-D20D-DDDD-B4CC-1DE5781F0D5C");
	    protected Guid _newuserOrganizationRealPageId = new Guid("12345678-C20D-4E6A-A4CC-0DE5781F0D5C");
        protected int _newUserOrganizationPartyId = 1234;

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
		
		
		protected List<IC.ProductInternalSetting> _productInternalSettings = new List<IC.ProductInternalSetting>();
        protected IList<IC.ElectronicAddress> _electronicAddressList = new List<IC.ElectronicAddress>();
		
		protected List<SamlAttributes> _editorSamlAttributes;
		protected List<SamlAttributes> _userSamlAttributes;
		protected List<SamlAttributes> _emptySamlAttributes;
		
		protected List<ProductProperty> _resultPropertyList;
		protected List<ACProperty> _resultPropertyListFinSuite;
		protected List<ProductRole> _resultRoleList;
		
		protected HttpClient client;
		protected Mock<HttpMessageHandler> mockHttpMessageHandler;
		protected Mock<HttpMessageHandler> mockTokenHttpMessageHandler;
		protected BatchProcessType batchProcessTypeCreUpd = BatchProcessType.CreateUpdateProductUser;

		protected List<OrganizationStatus> _organizationStatusListEditorPersona = new List<OrganizationStatus>();
		protected List<OrganizationStatus> _organizationStatusListUserPersona = new List<OrganizationStatus>();
		protected List<OrganizationStatus> _organizationStatusListNewUserPersona = new List<OrganizationStatus>();
		protected List<OrganizationStatus> _organizationStatusListInvalidPersona = new List<OrganizationStatus>();

        protected OrganizationStatus _organizationStatusEditorPersona;
        protected OrganizationStatus _organizationStatusUserPersona;
        protected OrganizationStatus _organizationStatusNewUserPersona;
        protected OrganizationStatus _organizationStatusInvalidPersona;

        protected List<GbProductMap> _gbProductMap;

        public ManageProductBaseTests(int productId)
		{
			_productId = productId;
			_emptySamlAttributes = new List<SamlAttributes>();

			_editorPersona = new Persona() { PersonaId = _editorPersonaId, RealPageId = _editorRealPageId, OrganizationPartyId = _editorOrganizationPartyId, UserId = _editorUserId};
			_editorPersona.Organization = new Organization() { PartyId = _editorOrganizationPartyId, RealPageId = _editorOrganizationRealPageId, Name = "RealPage", BooksMasterId = 1234, BooksCustomerMasterId = 4321, OrganizationDomain = new OrganizationDomain(){ OrganizationDomainId = 1, Name = "Primary"} };

			_userPersona = new Persona() { PersonaId = _userPersonaId, RealPageId = _userRealPageId, OrganizationPartyId = _userOrganizationPartyId, UserId = _userUserId };

			_userPersona.Organization = new Organization() { PartyId = _userOrganizationPartyId, RealPageId = _userOrganizationRealPageId, Name = "RealPage", BooksMasterId = 1234, BooksCustomerMasterId = 4321, OrganizationDomain = new OrganizationDomain(){ OrganizationDomainId = 1, Name = "Primary"} };

			_newUserPersona = new Persona() { PersonaId = _newUserPersonaId, RealPageId = _newUserRealPageId, OrganizationPartyId = _newUserOrganizationPartyId };
			_newUserPersona.Organization = new Organization() { PartyId = _newUserOrganizationPartyId, Name = "RealPage", BooksMasterId = 1234, RealPageId = _newuserOrganizationRealPageId, BooksCustomerMasterId = 4321, OrganizationDomain = new OrganizationDomain(){ OrganizationDomainId = 1, Name = "Primary"} };

			_userInvalidPersona = new Persona() { PersonaId = _userPersonaId, RealPageId = _userRealPageId, OrganizationPartyId = _userInvalidOrganizationPartyId };
			_userInvalidPersona.Organization = new Organization() { PartyId = _userInvalidOrganizationPartyId, Name = "RealPage", BooksMasterId = 1234, BooksCustomerMasterId = 4321, OrganizationDomain = new OrganizationDomain(){ OrganizationDomainId = 1, Name = "Primary"} };
                    
            _editorUserClaim = new DefaultUserClaim() { CorrelationId = _editorCorrelationId, OrganizationRealPageGuid = _editorOrganizationRealPageId, UserRealPageGuid = _editorRealPageId };
			_userUserClaim = new DefaultUserClaim() { CorrelationId = _userCorrelationId, UserId = _userUserId, OrganizationRealPageGuid = _userOrganizationRealPageId, UserRealPageGuid = _userRealPageId };

            mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockTokenHttpMessageHandler = new Mock<HttpMessageHandler>();
            client = new HttpClient(mockHttpMessageHandler.Object, false);

            _organizationStatusEditorPersona = new OrganizationStatus() {PartyId = _editorPersona.OrganizationPartyId, StatusTypeId = (int) UserUiStatusType.Active, Status = UserUiStatusType.Active};
            _organizationStatusUserPersona = new OrganizationStatus() {PartyId = _userPersona.OrganizationPartyId, StatusTypeId = (int)UserUiStatusType.Active, Status = UserUiStatusType.Active };
            _organizationStatusNewUserPersona = new OrganizationStatus() {PartyId = _newUserPersona.OrganizationPartyId, StatusTypeId = (int)UserUiStatusType.Active, Status = UserUiStatusType.Active };
            _organizationStatusInvalidPersona = new OrganizationStatus() {PartyId = _userInvalidPersona.OrganizationPartyId, StatusTypeId = (int)UserUiStatusType.Active, Status = UserUiStatusType.Active };

            _organizationStatusListEditorPersona.Add(_organizationStatusEditorPersona);
            _organizationStatusListUserPersona.Add(_organizationStatusUserPersona);
            _organizationStatusListNewUserPersona.Add(_organizationStatusNewUserPersona);
            _organizationStatusListInvalidPersona.Add(_organizationStatusInvalidPersona);

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
            
        }
    }
}
