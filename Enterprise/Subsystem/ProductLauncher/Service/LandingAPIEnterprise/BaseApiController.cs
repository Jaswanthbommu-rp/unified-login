using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Attributes;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Controllers;
using RP.Enterprise.Foundation.DataAccess.Component;
using System.Net.Http;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPIEnterprise
{
    /// <summary>
    /// Base controller for all webapi projects
    /// </summary>
    [AllowCors("LandingAPICORSAllowedOrigins"), AuthorizeScope("enterpriseapi")]
	public class BaseApiController : ApiController
    {
        private IManagePerson _managePerson;
        private IManageUserLogin _manageUserLogin;
		private IManagePersona _managePersona;
        
        /// <summary>
        /// Enterprise UserId
        /// </summary>
        public int _EnterpriseUserId = 0;

		/// <summary>
		/// Realpage UserId
		/// </summary>
		public Guid _realpageUserId;

		/// <summary>
		/// Organization PartyId
		/// </summary>
		public long _orgPartyId = 0;

		/// <summary>
		/// UserName
		/// </summary>
		public string _loginName = "";

		/// <summary>
		/// Used to filter, sort and limit the number of records being returned by the request.
		/// </summary>
		public RequestParameter GlobalRequestParameter;

		/// <summary>
		/// Holds default user claim related information
		/// </summary>
		public DefaultUserClaim _userClaims;

		public string _clientCode = string.Empty;

		private Guid _correlationId;
		private long _organizationMasterId;
		private string _organizationName;
		private Guid _organizationRealPageGuid;

		public long _personaId;
		public string _greenBookAccessToken = string.Empty;

		public bool _realPageEmployee = false;

		/// <summary>
		/// Base Type
		/// </summary>
		public new class BaseType
		{
			/// <summary>
			/// Request Parameter
			/// </summary>
			public const String RequestParameter = "RequestParameter";
		}

		/// <summary>
		/// Default constructor
		/// </summary>
		public BaseApiController()
		{

		}

		/// <summary>
		/// Unit test constructor v2
		/// </summary>
		/// <param name="repository"></param>
		/// <param name="messageHandler"></param>
		/// <param name="userClaims"></param>
        public BaseApiController(IRepository repository, HttpMessageHandler messageHandler, DefaultUserClaim userClaims)
        {
            _userClaims = userClaims;
            _managePerson = new ManagePerson(repository);
            _managePersona = new ManagePersona(repository, userClaims, messageHandler);
            _manageUserLogin = new ManageUserLogin(repository, userClaims, messageHandler);
        }

        /// <summary>
        /// Used to initialize the base controller and retrieve the needed information used by the infrastructure
        /// </summary>
        /// <param name="controllerContext"></param>
        protected override void Initialize(HttpControllerContext controllerContext)
		{
            base.Initialize(controllerContext);

			ClaimsPrincipal currentClaimPrincipal = ClaimsPrincipal.Current;
			if (currentClaimPrincipal.Identity.IsAuthenticated)
            {
                _managePerson = new ManagePerson();
                _managePersona = new ManagePersona();
				_manageUserLogin = new ManageUserLogin();

				if (currentClaimPrincipal.Claims.Any(p => p.Type.Equals("client_info", StringComparison.OrdinalIgnoreCase)))
				{
					var identity = (ClaimsIdentity)currentClaimPrincipal.Identity;
					Guid realGuid;
					if (Guid.TryParse((from nvp in currentClaimPrincipal.Claims where nvp.Type == "client_info" select nvp.Value).FirstOrDefault(), out realGuid))
						_realpageUserId = realGuid;

					identity.AddClaim(new Claim("realPageId", _realpageUserId.ToString()));
					IManagePerson personLogic = new ManagePerson();
					Person person = personLogic.GetPerson(_realpageUserId);
					if (person == null)
					{
						string clientid = (from nvp in currentClaimPrincipal.Claims where nvp.Type == "client_id" select nvp.Value).FirstOrDefault();
						throw new Exception($"Missing persona information for client_info user. client: {clientid} realPageId: {_realpageUserId}");
					}
					IManageUserLogin userLoginLogic = new ManageUserLogin();
					IManageUserRoleRight userRoleRight = new ManageUserRoleRight();
					var userLogin = userLoginLogic.GetUserLoginOnly(_realpageUserId);
					
					IManagePersona managePersona = new ManagePersona();
					//Active Persona is linked to one organization
					Persona persona = managePersona.GetActivePersonaWithoutRights(_realpageUserId); // this user can only be under 1 company to work correctly

					identity.AddClaim(new Claim("sub", userLogin.UserId.ToString()));
					identity.AddClaim(new Claim("orgPartyId", persona.Organization.PartyId.ToString()));
					identity.AddClaim(new Claim("ORGID", persona.Organization.RealPageId.ToString()));
					identity.AddClaim(new Claim("LOGINNAME", userLogin.LoginName));
					identity.AddClaim(new Claim("ORGMASTERID", persona.Organization.BooksMasterId.ToString()));
					identity.AddClaim(new Claim("ORGNAME", persona.Organization.Name.ToString()));
					identity.AddClaim(new Claim("FIRSTNAME", person.FirstName));
					identity.AddClaim(new Claim("LASTNAME", person.LastName));
					identity.AddClaim(new Claim("PERSONAID", persona.PersonaId.ToString()));

					// get the users role so the rights can be retrieved
					IList<Component.SharedObjects.Product.UserManagement.Role> userRoles = userRoleRight.GetAssignedRoleForPersona(ProductEnum.UnifiedPlatform, persona.PersonaId, persona.Organization.PartyId);
					identity.AddClaims((userRoles.Select(a => new Claim("roleid", a.Name)).ToList()));
				}

				_userClaims = new DefaultUserClaim(currentClaimPrincipal);

				_EnterpriseUserId = _userClaims.UserId;
				_orgPartyId = _userClaims.OrganizationPartyId;
				_loginName = _userClaims.LoginName;
				_organizationMasterId = _userClaims.OrganizationMasterId;
				_organizationName = _userClaims.OrganizationName;
				_realpageUserId = _userClaims.UserRealPageGuid;
				_correlationId = _userClaims.CorrelationId;
				_organizationRealPageGuid = _userClaims.OrganizationRealPageGuid;
				_clientCode = _userClaims.ClientCode;
				_personaId = _userClaims.PersonaId;
				_greenBookAccessToken = Request.Headers.Authorization.Parameter;
				_realPageEmployee = _userClaims.RealPageEmployee;

				List<string> userRights = BaseUserRights.GetUserRightsBy(currentClaimPrincipal, _userClaims);
                _userClaims.Rights = userRights;
			}
			else
			{
				// if the call is anonymous, build a default guid and later the code may override it if one has been stored somewhere else
				_userClaims = new DefaultUserClaim() { CorrelationId = Guid.NewGuid() };
				_correlationId = _userClaims.CorrelationId;
			}
		}

        public void RecreateClaimsForClient(Guid _realpageUserId)
        {
            if (!string.IsNullOrEmpty(_realpageUserId.ToString()))
            {
                var person = _managePerson.GetPerson(_realpageUserId);
                if (person == null)
                {
                    throw new Exception($"Missing persona information for client_info user while Recreation of Claims For Client.  realPageId: {_realpageUserId}");
                }
                var userLogin = _manageUserLogin.GetUserLoginOnly(_realpageUserId);

                //Active Persona is linked to one organization
                var persona = _managePersona.GetActivePersonaWithoutRights(_realpageUserId); // this user can only be under 1 company to work correctly

                _userClaims = new DefaultUserClaim
                {
                    UserId = (int)userLogin.UserId,
                    OrganizationPartyId = persona.Organization.PartyId,
                    LoginName = userLogin.LoginName,
                    OrganizationMasterId = (long)persona.Organization.BooksMasterId,
                    CustomerMasterId = (long)persona.Organization.BooksMasterId,
                    OrganizationName = persona.Organization.Name.ToString(),
                    FirstName = person.FirstName,
                    LastName = person.LastName,
                    PersonaId = persona.PersonaId,
                    OrganizationRealPageGuid = persona.Organization.RealPageId,
                    UserRealPageGuid = _realpageUserId,
                    CorrelationId = Guid.NewGuid(),
                    RealPageEmployee = persona.Organization.Name.ToUpper() == "REALPAGE EMPLOYEE"
                };
            }
        }

        public void RecreateClaimsForClient(Guid _realpageUserId, Guid upfmId)
        {
            if (!string.IsNullOrEmpty(_realpageUserId.ToString()))
            {
                var person = _managePerson.GetPerson(_realpageUserId);
                if (person == null)
                {
                    throw new Exception($"Missing persona information for client_info user while Recreation of Claims For Client.  realPageId: {_realpageUserId}");
                }

                IList<Persona> personas = _managePersona.ListPersona(_realpageUserId);
                var userPersona = personas.FirstOrDefault(p => p.Organization.RealPageId == upfmId);
                var userLogin = _manageUserLogin.GetUserLoginOnly(_realpageUserId);

				_userClaims = new DefaultUserClaim
				{
					UserId = (int)userPersona.UserId,
					OrganizationPartyId = userPersona.Organization.PartyId,
					LoginName = userLogin.LoginName,
					OrganizationMasterId = (long)userPersona.Organization.BooksMasterId,
					CustomerMasterId = (long)userPersona.Organization.BooksMasterId,
					OrganizationName = userPersona.Organization.Name.ToString(),
					FirstName = person.FirstName,
					LastName = person.LastName,
					PersonaId = userPersona.PersonaId,
					OrganizationRealPageGuid = userPersona.Organization.RealPageId,
					UserRealPageGuid = _realpageUserId,
					CorrelationId = Guid.NewGuid(),
					RealPageEmployee = personas.Any(p => string.Equals(p.Organization.Name, "REALPAGE EMPLOYEE", StringComparison.OrdinalIgnoreCase))
                };
            }
        }
    }
}