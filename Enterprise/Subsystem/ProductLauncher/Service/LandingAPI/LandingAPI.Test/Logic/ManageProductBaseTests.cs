using Moq;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Accounting;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Saml;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Text;
using IC = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic
{
	[ExcludeFromCodeCoverage]
	public class ManageProductBaseTests
    {
        protected int _productId;

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
			_userUserClaim = new DefaultUserClaim() { CorrelationId = _userCorrelationId, OrganizationRealPageGuid = _userOrganizationRealPageId, UserRealPageGuid = _userRealPageId };

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
        }

        public string Encode(string value)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
        }
		public bool TestIs(string propertyName, object obj, Guid? realPageId)
        {
            if (obj == null && realPageId == null)
            {
                return true;
            }

            if (obj == null)
            {
                return false;
            }

            return obj.ToString().ToLower().Contains($"{propertyName} = {realPageId}");
        }
    }
}
