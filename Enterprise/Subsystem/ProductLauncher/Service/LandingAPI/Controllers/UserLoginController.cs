using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Attributes;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Attribute;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using RP.Enterprise.Foundation.DataAccess.Component;
using RepositoryResponse = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.RepositoryResponse;
using UserLogin = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.UserLogin;
using System.Security.Claims;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
    /// <summary>
    /// UserLogin Controller to hold all user management related APIs
    /// </summary>
    public class UserLoginController : BaseApiController
    {
        #region Private variables
        IRepositoryResponse repositoryResponse = new RepositoryResponse();
        private IManageUserLogin _manageUserLogin;
        private IRepository _repository;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public UserLoginController()
        {
            // DONT USE USERCLAIM IN BASE, IT IS NULL AT THIS POINT. MOVE TO Initialize FUNCTION
        }

        public UserLoginController(DefaultUserClaim defaultUserClaim, IRepository repository)
        {
            _userClaims = defaultUserClaim;
            _repository = repository;
            _manageUserLogin = new ManageUserLogin(repository, _userClaims, null);
        }

        /// <summary>
        /// Used to initialize DI classes with userclaim
        /// </summary>
        /// <param name="controllerContext"></param>
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            _manageUserLogin = new ManageUserLogin(_userClaims);
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// Create a new UserLogin
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <param name="userLogin">UserLogin object of the parameter values</param>
        /// <returns>Response with Success Message</returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when UserLogin object have invalid entries)")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Newly created User Id", Type = typeof(UserLogin.UserLoginOutputResult))]
        [SwaggerResponseExamples(typeof(UserLogin.UserLoginOutputResult), typeof(NewUserLoginOutputResultExample))]
        [HttpPost]
        [Route("userlogins/{realPageId}")]
        public HttpResponseMessage CreateUserLogin(Guid realPageId, [FromBody] UserLogin userLogin)
        {
            realPageId = ((realPageId == Guid.Empty) || (realPageId == null)) ? _realpageUserId : realPageId;
            if ((realPageId == Guid.Empty) || (realPageId == null))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameter: realPageId");
            }

            if (userLogin == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Null parameter: UserLogin");
            }

            //CreateUserLogin
            IManageUserLogin userLoginLogic = new ManageUserLogin();
            repositoryResponse = userLoginLogic.CreateUserLogin(realPageId, userLogin);
            if (repositoryResponse.Id == 0 || !string.IsNullOrWhiteSpace(repositoryResponse.ErrorMessage))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, repositoryResponse.ErrorMessage);
            }

            UserLogin.UserLoginOutputResult result = new UserLogin.UserLoginOutputResult
            {
                NewUserId = repositoryResponse.Id
            };

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Get UserLogin detail
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <returns>UserLogin object</returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get information about the person", Type = typeof(IUserLogin))]
        [SwaggerResponseExamples(typeof(IUserLogin), typeof(UserLoginExample))]
        [Route("userlogins/{realPageId}")]
        [HttpGet]
        public HttpResponseMessage GetUserLogin(Guid realPageId)
        {
            realPageId = (realPageId == Guid.Empty) ? _realpageUserId : realPageId;
            if (realPageId == Guid.Empty)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameter: realPageId");
            }

            IManageUserLogin userLoginLogic = new ManageUserLogin();
            var userLogin = userLoginLogic.GetUserLogin(realPageId, _orgPartyId); // keep for now, used by ui, need to investigate how

            if (userLogin != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, userLogin);
            }

            return Request.CreateResponse(HttpStatusCode.NoContent, "Invalid realPageId");
        }

        /// <summary>
        /// Get UserLogin detail by organization
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <param name="orgRealPageId">Organization unique identifier</param>
        /// <returns>UserLogin object</returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get information about the person", Type = typeof(IUserLoginOnly))]
        [SwaggerResponseExamples(typeof(IUserLoginOnly), typeof(UserLoginOnlyExample))]
        [Route("userlogins/{realPageId}/organization/{orgRealPageId}")]
        [HttpGet]
        public HttpResponseMessage GetUserLoginByCompany(Guid realPageId, Guid orgRealPageId)
        {
            realPageId = (realPageId == Guid.Empty) ? _realpageUserId : realPageId;
            if (realPageId == Guid.Empty)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameter: realPageId");
            }
            if (orgRealPageId == Guid.Empty)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameter: orgRealPageId");
            }

            IManageUserLogin userLoginLogic = new ManageUserLogin(_userClaims);
            IManageOrganization manageOrganization = new ManageOrganization(_userClaims);
            var organization = manageOrganization.GetOrganization(orgRealPageId);
            var userLogin = userLoginLogic.GetUserLogin(realPageId, organization.PartyId); // keep for now, used by ui, need to investigate how

            if (userLogin != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, userLogin);
            }

            return Request.CreateResponse(HttpStatusCode.NoContent, "Invalid realPageId");
        }

        /// <summary>
        /// Update UserLogin
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <param name="userLogin">UserLogin object of the parameter values</param>
        /// <returns>Response with Success Message</returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when UserLogin object have invalid entries / when Information is out of sync with the server)")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "UserLogin Updated")]
        [Route("userlogins/{realPageId}")]
        [Authorize]
        [HttpPut]
        public HttpResponseMessage UpdateUserLogin(Guid realPageId, [FromBody] UserLogin userLogin)
        {
            realPageId = (realPageId == Guid.Empty) ? _realpageUserId : realPageId;

            if ((realPageId == Guid.Empty))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameter: realPageId");
            }

            if (userLogin == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Null parameter: UserLogin");
            }

            if (userLogin.ThruDate != null && userLogin.ThruDate < DateTime.UtcNow)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "ThruDate should be greater than current date.");
            }

            if (userLogin.ThruDate != null && userLogin.ThruDate < userLogin.FromDate)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "ThruDate should be greater than FromDate.");
            }

            IManageUserLogin userLoginLogic = new ManageUserLogin();
            repositoryResponse = userLoginLogic.UpdateUserLogin(realPageId, userLogin);

            if (repositoryResponse.Id == 0)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, repositoryResponse.ErrorMessage);
            }

            return Request.CreateResponse(HttpStatusCode.OK, userLogin);
        }

		/// <summary>
		/// Patch UserLogins' Active|Inactive|Lock|Unlock
		/// </summary>        
		/// <param name="updateType" >Patches by Batch or AllRecords</param>  
		/// <param name="userLoginStatusType">Active|Inactive|Lock|Unlock</param>  
		/// <param name="userLogins">Array of userlogins. Pass an empty array if updating by AllRecords i.e []</param>
		/// <returns>List of patched UserLogins</returns>		
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[Route("userlogins")]
		[Authorize]
		[HttpPatch]
		public HttpResponseMessage UpdateUserLogins(
			[FromUri]UserUiStatusType? userLoginStatusType,
			[FromUri]UserLoginUpdateType? updateType,
			[FromBody]IList<UserLogin> userLogins
			)
		{
			ObjectListOutput<UserLogin, IErrorData> output = new ObjectListOutput<UserLogin, IErrorData>();
			Status<IErrorData> errorStatus = new Status<IErrorData>();

			if (userLoginStatusType == null)
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "200.3";
				errorStatus.ErrorMsg = "Null parameter: userLoginStatusType";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.BadRequest, output);
			}

			if (userLogins == null && updateType == UserLoginUpdateType.Batch)
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "200.3";
				errorStatus.ErrorMsg = "Null parameter on updating batch: userLogins";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.BadRequest, output);
			}

			IManageUserLogin manageUserLogin = new ManageUserLogin(_userClaims);

            if (updateType == UserLoginUpdateType.AllRecords)
            {
                //var organizationRealPageId = Guid.Empty;
                //IManagePersona personaLogic = new ManagePersona();
                //Persona persona = personaLogic.GetActivePersona(_realpageUserId);
                //organizationRealPageId = persona.Organization.RealPageId;

                //if (organizationRealPageId != Guid.Empty)
                //{
                IManageProfile profileLogic = new ManageProfile(_userClaims);
                profileLogic.ListProfileDetails(new Dictionary<object, object>())
                    .ToList()
                    .ForEach(p => { userLogins.Add((UserLogin) p.userLogin); }
                    );
                //}
                //else
                //{
                //	errorStatus.Success = false;
                //	errorStatus.ErrorCode = "200.4";
                //	errorStatus.ErrorMsg = "Organization RealPage Id is missing.";
                //	output.list = userLogins;
                //	output.Status = errorStatus;
                //	return Request.CreateResponse(HttpStatusCode.BadRequest, output);
                //}
            }

            //Prevents the currently logged in user from being patched.
			userLogins = userLogins.Where(u => u.RealPageId != _userClaims.UserRealPageGuid).ToList();

			List<RepositoryResponse> repositoryResponses = new List<RepositoryResponse>();
			if (userLogins.Count > 0)
			{
				//filter out unnecessary records based on action
				if (userLoginStatusType.Value == UserUiStatusType.Locked)
				{
				userLogins = userLogins.ToList().Where(u => u.Status == UserUiStatusType.Active).ToList();
				}
				else if (userLoginStatusType.Value == UserUiStatusType.Unlocked)
				{
					userLogins = userLogins.ToList().Where(u => u.Status == UserUiStatusType.Locked).ToList();
				}
				else if (userLoginStatusType.Value == UserUiStatusType.Active)
				{
					userLogins = userLogins.ToList().Where(u => u.Status == UserUiStatusType.Locked || u.Status == UserUiStatusType.Deactivated).ToList();
				}
				else if (userLoginStatusType.Value == UserUiStatusType.Deactivated)
				{
					userLogins = userLogins.ToList().Where(
						u => u.Status == UserUiStatusType.Active ||
						u.Status == UserUiStatusType.Pending ||
						u.Status == UserUiStatusType.Locked ||
						u.Status == UserUiStatusType.Expired).ToList();
				}

				if (userLogins.Count > 0)
				{
					// TODO: check status - should be either Active, Disable, Lock or Unlock only
					if (!ValidateBulkUpdateStatus(userLoginStatusType.Value))
					{
						errorStatus.Success = false;
						errorStatus.ErrorCode = "200.3";
						errorStatus.ErrorMsg = $"Bulk update is not supported for {userLoginStatusType.Value.ToString()} status.";
					}
					else
					{
                        List<UserLoginOnly> userLoginsOnly = new List<UserLoginOnly>();
                        foreach ( UserLogin ul in userLogins)
                        {
                            userLoginsOnly.Add(new UserLoginOnly() { RealPageId = ul.RealPageId });
                        }
                        repositoryResponse = manageUserLogin.UpdateBulkUserLogins(userLoginsOnly, userLoginStatusType.Value);

						if (!string.IsNullOrEmpty(repositoryResponse.ErrorMessage))
						{
							errorStatus.Success = false;
							errorStatus.ErrorCode = "200.3";
							errorStatus.ErrorMsg = "Error(s) occured during bulk update!";
						}
						else
						{
							repositoryResponse = UpdateUserProductStatus(userLoginsOnly, userLoginStatusType);
						}
					}
				}
			}

			switch (userLoginStatusType.Value)
			{
				case UserUiStatusType.Active:
					userLogins.ToList().ForEach(u => u.IsActive = true);
					break;
				case UserUiStatusType.Disabled:
					userLogins.ToList().ForEach(u => u.IsActive = false);
					break;
				case UserUiStatusType.Locked:
					userLogins.ToList().ForEach(u => u.IsLocked = true);
					break;
				case UserUiStatusType.Unlocked:
					userLogins.ToList().ForEach(u => u.IsLocked = false);
					break;
			}

			output.list = userLogins;
			output.Status = errorStatus;
			return Request.CreateResponse(HttpStatusCode.OK, output);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="userLogins"></param>
		/// <param name="userLoginStatusType"></param>
		/// <returns></returns>
		private IRepositoryResponse UpdateUserProductStatus(IList<UserLoginOnly> userLogins, UserUiStatusType? userLoginStatusType)
	    {
		    IManageUser manageUser = new ManageUser(_userClaims);
            
		    return manageUser.UpdateUserStatus(_userClaims.UserRealPageGuid, _userClaims.PersonaId, userLogins, userLoginStatusType);
        }

	    /// <summary>
		/// Resend Email Invitations
		/// </summary>        
		/// <param name="userLogins">Array of user realpage Ids</param>
		/// <returns>List of patched UserLogins</returns>		
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [Route("userlogins/resendinvitation")]
        [AuthorizeRight("resendinvitation")]
        [HttpPost]
        public HttpResponseMessage ResendInvitation([FromBody]IList<UserLogin> userLogins)
        {
            ObjectListOutput<UserLogin, IErrorData> output = new ObjectListOutput<UserLogin, IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();
            bool response = true;

            if (userLogins.Count > 0)
            {
                response = _manageUserLogin.ResendInvitation(userLogins,false);

                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }

                return Request.CreateResponse(HttpStatusCode.ExpectationFailed, response);


            }
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>
        /// Resend Email Invitations For Extermal
        /// </summary>
        /// <param name="realpageId"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [Route("userlogins/resendinvitationexternal/{realpageId}")]
        [AuthorizeScope("enterpriseapi")]
        [HttpPost]
        public HttpResponseMessage ResendInvitationExternal(Guid realpageId)
        {
            ObjectListOutput<UserLogin, IErrorData> output = new ObjectListOutput<UserLogin, IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();
            bool response = true;
            var userList = new List<UserLogin>() { new UserLogin() { RealPageId = realpageId } };

            ClaimsPrincipal currentClaimPrincipal = ClaimsPrincipal.Current;
            if (!currentClaimPrincipal.Claims.Any(a => a.Type == "sub"))
            {
                var userClaim = _manageUserLogin.GetUserClaimsFromNonUser(realpageId);

                if (userClaim == null)
                {
                    return Request.CreateResponse(HttpStatusCode.ExpectationFailed, false);
                }


                var identity = (ClaimsIdentity)currentClaimPrincipal.Identity;
                identity.AddClaim(new Claim("orgPartyId", userClaim.OrganizationPartyId.ToString()));
                identity.AddClaim(new Claim("ORGID", userClaim.OrganizationRealPageGuid.ToString()));
                identity.AddClaim(new Claim("sub", userClaim.UserId.ToString()));
                identity.AddClaim(new Claim("LOGINNAME", userClaim.LoginName));
                identity.AddClaim(new Claim("ORGMASTERID", userClaim.OrganizationMasterId.ToString()));
                identity.AddClaim(new Claim("ORGNAME", userClaim.OrganizationName));
                identity.AddClaim(new Claim("FIRSTNAME", userClaim.FirstName));
                identity.AddClaim(new Claim("LASTNAME", userClaim.LastName));
                identity.AddClaim(new Claim("PERSONAID", userClaim.PersonaId.ToString()));
                identity.AddClaim(new Claim("ImpersonatedBy", userClaim.ImpersonatedBy.ToString()));
                identity.AddClaim(new Claim("ImpersonatedByName", userClaim.ImpersonatedByName));

                _manageUserLogin = new ManageUserLogin(userClaim);
            }

            _manageUserLogin.LogUserRequestedEmailLinkResent(realpageId);

            response = _manageUserLogin.ResendInvitation(userList, false);

            if (response)
            {
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }

            return Request.CreateResponse(HttpStatusCode.ExpectationFailed, false);
        }


        /// <summary>
        /// Clear user password and security questions
        /// </summary>        
        /// <param name="realPageId">The guid of the user to reset</param>
        /// <returns>List of patched UserLogins</returns>		
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [Route("userlogins/clearpasswordandquestions")]
        [AuthorizeRight("resendinvitation")]
        [HttpPut]
        public HttpResponseMessage ClearPasswordAndQuestions(Guid realPageId)
        {
            var userLogin = _manageUserLogin.GetUserLogin(realPageId, _orgPartyId);
            if (userLogin == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid company");
            }

            var response = _manageUserLogin.ClearPasswordAndQuestions(realPageId);

            if (response)
            {
                return Request.CreateResponse(HttpStatusCode.NoContent);
            }

            return Request.CreateResponse(HttpStatusCode.ExpectationFailed);
        }


        /// <summary>
        /// Process Future User Login Status
        /// </summary>        
        /// <param name="userLogins">Array of user realpage Ids</param>
        /// <returns>List of patched UserLogins</returns>		
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[Route("userlogins/processfutureuserlogins")]
		[System.Web.Http.AllowAnonymous]
		[HttpPost]
		public HttpResponseMessage ProcessFutureUserLogins([FromBody]IList<ProcessUserLogin> userLogins)
		{
			ObjectListOutput<ProcessUserLogin, IErrorData> output = new ObjectListOutput<ProcessUserLogin, IErrorData>();
			Status<IErrorData> errorStatus = new Status<IErrorData>();
			bool response = true;
			IManageUserLogin manageUserLogin = new ManageUserLogin();

			if (userLogins.Count > 0)
			{
				ManageUserLogin userLoginLogic = new ManageUserLogin(_userClaims);
                response = userLoginLogic.ProcessFutureUserLogins(userLogins);

				if (response)
				{
					return Request.CreateResponse(HttpStatusCode.OK, response);
				}

				return Request.CreateResponse(HttpStatusCode.ExpectationFailed, response);
			}
			return Request.CreateResponse(HttpStatusCode.OK, response);
		}

		/// <summary>
		/// Get user auto logout interval when expiration (thru) date is set
		/// </summary>        
		/// <returns></returns>
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request (when realPageId is null)")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [Route("userlogin/autologoutinterval")]
        [Authorize]
        [HttpGet]
        public LogOutIntervalResponse GetLogOutInterval(Guid realPageId)
        {
            if (realPageId == null)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            IManageUserLogin userLoginLogic = new ManageUserLogin(_userClaims);
            LogOutIntervalResponse response = userLoginLogic.GetLogOutInterval(realPageId, _userClaims.OrganizationPartyId);
            return response;
        }

        /// <summary>
        /// Create or Update User status based on statusTypeName
        /// </summary>
        /// <param name="statusTypeName">Status Type Name</param>
        /// <param name="realPageId">User unique identifier</param>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Create or Update User status based on statusTypeName")]
        [Route("userlogin/status")]
        [AuthorizeRight("lockunlockusers", "activatedeactivateusers")]
        [HttpPost]
        public HttpResponseMessage CreateUpdateUserStatus(UserUiStatusType statusTypeName, Guid? realPageId = null)
        {
            if (realPageId == _realpageUserId)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameter: Cannot update currently logged-in user's status");
            }

            //Since current logged-in user cannot change status of his own, this auto assignment should not be here.
            //realPageId = ((realPageId == Guid.Empty) || (realPageId == null)) ? _realpageUserId : realPageId;

            if (realPageId == null || realPageId == Guid.Empty)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameter: realPageId");
            }

            IManageUserLogin userLoginLogic = new ManageUserLogin(_userClaims);
            bool response = userLoginLogic.CreateUpdateUserStatus(realPageId.Value, statusTypeName);//, fromUtcDateTime, thruUtcDateTime);

            if (response)
            {
	            IList<UserLoginOnly> userLogins = new List<UserLoginOnly>();
	            UserLoginOnly ul = new UserLoginOnly() {RealPageId = realPageId.Value};
	            userLogins.Add(ul);
				repositoryResponse = UpdateUserProductStatus(userLogins, statusTypeName);
				return Request.CreateResponse(HttpStatusCode.OK, response);
            }

            return Request.CreateResponse(HttpStatusCode.ExpectationFailed, response);
        }

		/// <summary>
		/// disable users which is called from service
		/// </summary>
		/// <param name="userLogins">Array of user realpage Ids</param>
		/// <returns>List of patched UserLogins</returns>
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Create or Update User status based on statusTypeName")]
		[Route("disableexpiredusers")]
		[System.Web.Http.AllowAnonymous]
		[HttpPost]
		public HttpResponseMessage DisableUsersFromProducts([FromBody]IList<ProcessUserLogin> userLogins)
		{
			ObjectListOutput<UserLogin, IErrorData> output = new ObjectListOutput<UserLogin, IErrorData>();
			Status<IErrorData> errorStatus = new Status<IErrorData>();
			IRepositoryResponse response = new RepositoryResponse();

			if (userLogins.Count > 0)
			{
				IManageUser manageUser = new ManageUser(_userClaims);
				response = manageUser.DisableUsersFromProducts(userLogins);

				if (string.IsNullOrEmpty(response.ErrorMessage))
				{
					return Request.CreateResponse(HttpStatusCode.OK, true);
				}
				return Request.CreateResponse(HttpStatusCode.ExpectationFailed, false);
			}
			return Request.CreateResponse(HttpStatusCode.OK, response);
		}

        /// <summary>
        /// User Exists? User Exists in this Organization?
        /// </summary>
        /// <param name="loginName">User LoginName</param>
        /// <param name="OrganizationRealPageId">Unique Identifier - OrganizationRealPageId</param>
        /// <param name="userRealPageId">The id of the user if editing</param>
        /// <param name="isFromExport"></param>
        /// <param name="userType"></param>
        /// <returns>UserOrganizationExists object</returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Get information about the password policy", Type = typeof(UserOrganizationExists))]
		[SwaggerResponseExamples(typeof(UserOrganizationExists), typeof(UserOrganizationExistsExample))]
		[Route("userlogins/loginnameexists")]
		[HttpGet]
        public HttpResponseMessage IsLoginNameExists(string loginName, Guid OrganizationRealPageId, Guid? userRealPageId = null,string firstName = null, string lastName = null, int userType = 0, bool isFromExport = false)
		{
			ObjectOutput<UserOrganizationExists, IErrorData> output = new ObjectOutput<UserOrganizationExists, IErrorData>();
			Status<IErrorData> errorStatus = new Status<IErrorData>();
			UserOrganizationExists userOrganizationExists = new UserOrganizationExists();

            if (!userRealPageId.HasValue)
            {
                userRealPageId = Guid.Empty;
            }

			if (OrganizationRealPageId == Guid.Empty)
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "UserLogin.IsLoginNameExists.1";
				errorStatus.ErrorMsg = "IsLoginNameExists: Invalid parameter enterprise organization Id";
				output.obj = userOrganizationExists;
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}

			if (string.IsNullOrWhiteSpace(loginName))
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "UserLogin.IsLoginNameExists.2";
				errorStatus.ErrorMsg = "IsLoginNameExists: Invalid parameter loginName";
				output.obj = userOrganizationExists;
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}

			IManageUserLogin userLoginLogic = _manageUserLogin ?? new ManageUserLogin(_userClaims);
			userOrganizationExists = userLoginLogic.IsLoginNameExists(loginName, OrganizationRealPageId, userRealPageId.Value, firstName, lastName, userType, isFromExport);

            output.Status = errorStatus;
			output.obj = userOrganizationExists;
			return Request.CreateResponse(HttpStatusCode.OK, output);
		}
		#endregion

		#region Private Methods
		private bool ValidateBulkUpdateStatus(UserUiStatusType userLoginStatusType)
        {
            bool result = false;
            if (userLoginStatusType == UserUiStatusType.Active || userLoginStatusType == UserUiStatusType.Disabled ||
                userLoginStatusType == UserUiStatusType.Locked || userLoginStatusType == UserUiStatusType.Unlocked)
            {
                result = true;
            }

            return result;
        }

        #endregion

        #region Get Examples
        /// <summary>
        /// Used to document examples of the UserLogin Model webapi result
        /// </summary>
        [ExcludeFromCodeCoverage]
        public class UserLoginExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>UserLogin example</returns>
            public object GetExamples()
            {
                IUserLogin example = new UserLogin();
                IList<UserLogin> userLoginList = new List<UserLogin>();
                userLoginList = UserLogin.GetUserOutputResultExample();
                example = userLoginList[0];

                ObjectOutput<IUserLogin, IErrorData> output = new ObjectOutput<IUserLogin, IErrorData>() { obj = example };

                return output;
            }
        }

        /// <summary>
        /// Used to document examples of the UserLogin Model webapi result
        /// </summary>
        [ExcludeFromCodeCoverage]
        public class UserLoginOnlyExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>UserLogin example</returns>
            public object GetExamples()
            {
                IUserLoginOnly example = new UserLoginOnly();
                IList<UserLoginOnly> userLoginList = new List<UserLoginOnly>();
                userLoginList = UserLoginOnly.GetUserOutputResultExample();
                example = userLoginList[0];

                ObjectOutput<IUserLoginOnly, IErrorData> output = new ObjectOutput<IUserLoginOnly, IErrorData>() { obj = example };

                return output;
            }
        }
        #endregion

        #region Output results for documentation
        /// <summary>
        /// Used to document examples of the New Person webapi result
        /// </summary>
        [ExcludeFromCodeCoverage]
        public class NewUserLoginOutputResultExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>Newly created user id</returns>
            public object GetExamples()
            {
                return UserLogin.GetNewUserLoginExample();
            }
        }

		/// <summary>
		/// Used to document examples of the User LoginName Exists? webapi result
		/// </summary>
		[ExcludeFromCodeCoverage]
		public class UserOrganizationExistsExample : IProvideExamples
		{
			/// <summary>
			/// Example object data used by Swagger to document the output of the webapi method
			/// </summary>
			/// <returns>UserOrganizationExists object</returns>
			public object GetExamples()
			{
				return UserOrganizationExists.GetUserOrganizationExistsExample();
			}
		}
		#endregion
	}
}