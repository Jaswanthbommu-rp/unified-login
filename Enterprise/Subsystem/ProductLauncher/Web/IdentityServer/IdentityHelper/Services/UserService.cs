using IdentityModel.Client;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Services.Default;
using Microsoft.Owin;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Extensions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Saml;
using RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Extensions;
using RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Repository;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Enterprise.Helpers;
using RPModel = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Services
{
    public partial class UserService : UserServiceBase
    {
        private readonly AuthenticateService _authenticateService;
        private readonly ManageUserLogin _userLoginLogic;
        private readonly ManageUserLoginIdentity _userLoginIdentityLogic;
        private readonly ManageUserRole _userRoleManager;
        private readonly ManagePersona _personaManager;
        private readonly ManagePerson _personManager;
        private readonly ManageProvider _manageProvider;
        private readonly SamlRepository _samlRepository;
        private readonly ManageContactMechanismUsageType _contactMechanismUsageType;
        private readonly ManageTelecommunicationNumber _manageTelecommunicationNumber;
        private readonly IIdentityServerRepository _identityServerRepository;
        private readonly IOwinContext _ctx;

        /// <summary>
        /// UserService
        /// </summary>
        /// <param name="membershipService"></param>
        /// <param name="owinEnv"></param>
        /// <param name="manageUserLogin"></param>
        /// <param name="manageUserLoginIdentity"></param>
        public UserService(AuthenticateService membershipService, OwinEnvironmentService owinEnv, ManageUserLogin manageUserLogin, ManageUserLoginIdentity manageUserLoginIdentity)
        {
            _authenticateService = membershipService;
            _userLoginLogic = manageUserLogin;
            _userLoginIdentityLogic = manageUserLoginIdentity;
            _userRoleManager = new ManageUserRole();
            _personaManager = new ManagePersona();
            _manageProvider = new ManageProvider();
            _personManager = new ManagePerson();
            _samlRepository = new SamlRepository();
            _contactMechanismUsageType = new ManageContactMechanismUsageType();
            _manageTelecommunicationNumber = new ManageTelecommunicationNumber();
            _identityServerRepository = new IdentityServerRepository();
            _ctx = new OwinContext(owinEnv.Environment);
        }


        public override async Task PreAuthenticateAsync(PreAuthenticationContext context)
        {
            // anything other than the Unified Platform UI does not have any special pre-auth logic
            if (!context.SignInMessage.ClientId.Equals("LANDING", StringComparison.OrdinalIgnoreCase) &&
                !context.SignInMessage.ClientId.Equals("greenbookoidc", StringComparison.OrdinalIgnoreCase))
            {
                await base.PreAuthenticateAsync(context);
                return;
            }

            //if we are requesting impersonation, I pass it in acr values as you will see later on (Impersonate:UserName)
            var newUserContext = context.SignInMessage.AcrValues.FirstOrDefault(x => x.Split(':')[0] == "ChangeUserContext");
            var revertUser = context.SignInMessage.AcrValues.FirstOrDefault(x => x.Split(':')[0] == "Impersonated");
            var prompt = context.SignInMessage.AcrValues.FirstOrDefault(x => x.Split(':')[0] == "prompt");
            if (prompt != null && !string.IsNullOrEmpty(prompt))
            {
                _ctx.Set("prompt", prompt);
            }
            var idp = context.SignInMessage.IdP;

			if (!string.IsNullOrWhiteSpace(idp) && !string.IsNullOrWhiteSpace(revertUser))
			{
                if (_ctx.Authentication.User.Identity.IsAuthenticated)
                {
				    RevertSigninToOriginalUser(context, idp);
                }
                else
                {
                    context.SignInMessage.IdP = null;
                    context.SignInMessage.AcrValues = new List<string>();
                    await base.PreAuthenticateAsync(context);
                }
                return;
			}

            if (string.IsNullOrWhiteSpace(idp) || string.IsNullOrWhiteSpace(newUserContext))
            {
                await base.PreAuthenticateAsync(context);  // must NOT be either reverting or starting impersonation
                return;
            }

            await ChangeUserContextAsync(context, newUserContext, idp);
        }

        private async Task ChangeUserContextAsync(PreAuthenticationContext context, string newUserContext, string idp)
        {
            var userContext = Convert.FromBase64String(newUserContext.Split(':')[1]);
            var newUserRealPageId = Encoding.UTF8.GetString(userContext).Split('|')[0];
            var impersonatingRealPageId = Encoding.UTF8.GetString(userContext).Split('|')[1];
            var revertUser = context.SignInMessage.AcrValues.FirstOrDefault(x => x.Split(':')[0] == "Impersonated");
            var originalPersona = Convert.ToInt64(_ctx.Authentication.User.Claims.FirstOrDefault(a => string.Equals(a.Type, "personaId", StringComparison.OrdinalIgnoreCase))?.Value);
            var newPersonaIdString = "0";
            bool employeeChangedCompany = false;
            bool fireChangeCompanyEvent = false;

            if (Encoding.UTF8.GetString(userContext).Split('|').Length == 3)
            {
                newPersonaIdString = Encoding.UTF8.GetString(userContext).Split('|')[2];
            }

            var correlationId = _ctx.Authentication.User.Claims.CorrelationId();
            var loggedInUserRPID = _ctx.Authentication.User.Claims.RealPageId();
            if (!string.IsNullOrEmpty(_ctx.Authentication.User.Claims.ImpersonatedBy()))
            {
                loggedInUserRPID = _ctx.Authentication.User.Claims.ImpersonatedBy();
            }
            WriteToLog(LogEventLevel.Debug, "PreAuthenticateAsync: Begin change user context", new Guid(correlationId),
                new Dictionary<string, object>
                {
                    { "User data", $"userContext:{userContext} newUser:{newUserRealPageId} loggedInUserRPID:{loggedInUserRPID} newPersonaId:{newPersonaIdString}" }
                });

            if (string.IsNullOrEmpty(impersonatingRealPageId) || string.IsNullOrEmpty(loggedInUserRPID))
            {
                WriteToLog(LogEventLevel.Debug, "PreAuthenticateAsync: Invalid attempt to change user context 1", new Guid(correlationId));
                context.AuthenticateResult = new AuthenticateResult("Invalid attempt to change user context");
                await base.PreAuthenticateAsync(context);
                return;
            }

            var impersonator = _userLoginLogic.GetUserLoginOnly(new Guid(loggedInUserRPID));
            var impersonatorPerson = _personManager.GetPerson(impersonator.RealPageId);
            var impersonatorPersona = _personaManager.GetActivePersona(impersonator.RealPageId); // get the last persona the impersonator was using
            var userFromParamsMatchesLoggedInUser = string.Equals(impersonator.RealPageId.ToString(), newUserRealPageId, StringComparison.CurrentCultureIgnoreCase);
            var userMatchesNewUserContext = string.Equals(impersonator.RealPageId.ToString(), impersonatingRealPageId, StringComparison.CurrentCultureIgnoreCase);

            // TODO WHO SHOULD BE ALLOWED TO CHANGE A USERS PERSONA?
            if (!(userMatchesNewUserContext || userFromParamsMatchesLoggedInUser) && !UserIsAllowedToChangeContext(impersonator, impersonatorPersona.Organization.BooksMasterId, newUserRealPageId))
            //if (!UserIsAllowedToChangeContext(impersonator, impersonatorPersona.Organization.BooksMasterId, newUserRealPageId))
            {
                WriteToLog(LogEventLevel.Debug, "PreAuthenticateAsync: Invalid attempt to change user context 2", new Guid(correlationId));
                context.AuthenticateResult = new AuthenticateResult("Invalid attempt to change user context");
            }
            
            var user = _userLoginLogic.GetUserLoginOnly(new Guid(impersonatingRealPageId));
            var userPerson = _personManager.GetPerson(user.RealPageId);
            var userPersonaId = _personaManager.GetActivePersonaId(user.RealPageId);
            user.PersonaId = userPersonaId;

            if (!newPersonaIdString.Equals("0") && newPersonaIdString != userPersonaId.ToString())
                //if (userMatchesNewUserContext && !newPersonaId.Equals("0") && newPersonaId != userPersona.PersonaId.ToString())
            {
                var activePersonaList = _personaManager.ListActivePersona(user.RealPageId, false);
                long newPersonaId = Convert.ToInt64(newPersonaIdString);
                if (activePersonaList.Any(p => p.PersonaId == newPersonaId))
                {
                    _personaManager.UpdateActivePersona(user.RealPageId, newPersonaId);
                    user.PersonaId = newPersonaId;
                    if (activePersonaList.Any(ap => ap.Organization.BooksCustomerMasterId == -1))
                    {
                        employeeChangedCompany = (activePersonaList.FirstOrDefault(ap => ap.PersonaId == newPersonaId).Organization.BooksCustomerMasterId != -1) ? true : false;
                    }
                }
            }
            var userPersona = _personaManager.GetPersona(user.PersonaId);

            WriteToLog(LogEventLevel.Debug, $"PreAuthenticateAsync: loggedInUserRPID:{loggedInUserRPID} newUser:{newUserRealPageId} impersonatorPersona.Organization.BooksMasterId:{impersonatorPersona.Organization.BooksMasterId} ConfigReader.OrgMasterId:{ConfigReader.OrgMasterId}", new Guid(correlationId));

            var claims = GetClaimsForUser(user, userPersona.OrganizationPartyId, context.SignInMessage.ClientId);
            if (revertUser != null)
            {
                claims.Add(new Claim("ImpersonatedBy", revertUser));
            }
            if (!userMatchesNewUserContext || revertUser != null)
            {
                if (revertUser == null)
                {
                    claims.Add(new Claim("ImpersonatedBy", loggedInUserRPID));
                }

                LogActivity.WriteActivity(new ActivityDetails
                {
                    LogActivityTypeName = "Support Tool",
                    LogCategoryName = LogActivityCategoryType.User.ToString(),
                    CorrelationId = correlationId,
                    BooksMasterOrganizationId = Convert.ToInt64(impersonatorPersona.Organization.BooksMasterId),
                    Message = $"{impersonatorPerson.FirstName} {impersonatorPerson.LastName} accessed Unified Platform via Support Tool for {userPersona.Organization.Name}.",

                    FromUserLoginName = impersonator.LoginName,
                    FromUserLoginId = impersonator.UserId,
                    FromUserFirstName = impersonatorPerson.FirstName,
                    FromUserLastName = impersonatorPerson.LastName,
                    FromUserRealpageId = impersonator.RealPageId.ToString(),

                    ToUserFirstName = userPerson.FirstName,
                    ToUserLastName = userPerson.LastName,
                    ToUserLoginId = Convert.ToInt64(userPersona.UserId),
                    ToUserRealpageId = userPersona.RealPageId.ToString(),
                    ToUserLoginName = user.LoginName,

                    BooksProductCode = "ST",
                });
            }
            else
            {
                if (employeeChangedCompany)
                {
                    LogActivity.WriteActivity(new ActivityDetails
                    {
                        LogActivityTypeName = "Change Company",
                        LogCategoryName = LogActivityCategoryType.User.ToString(),
                        CorrelationId = correlationId,
                        BooksMasterOrganizationId = -1,
                        Message = $"{impersonatorPerson.FirstName} {impersonatorPerson.LastName} accessed {userPersona.Organization.Name} with Change Company.",

                        FromUserLoginName = impersonator.LoginName,
                        FromUserLoginId = impersonator.UserId,
                        FromUserFirstName = impersonatorPerson.FirstName,
                        FromUserLastName = impersonatorPerson.LastName,
                        FromUserRealpageId = impersonator.RealPageId.ToString(),

                        ToUserFirstName = userPerson.FirstName,
                        ToUserLastName = userPerson.LastName,
                        ToUserLoginId = Convert.ToInt64(userPersona.UserId),
                        ToUserRealpageId = userPersona.RealPageId.ToString(),
                        ToUserLoginName = user.LoginName,

                        BooksProductCode = "ST",
                    });
                    LogEventActivity(claims, "RealPage Employee {0} {1} successfully logged-in.");
                }
                else
                {
                    LogEventActivity(claims, "User {0} {1} successfully logged-in.");
                }
                fireChangeCompanyEvent = true;
            }

            WriteToLog(LogEventLevel.Debug, "PreAuthenticateAsync: Success ", new Guid(correlationId), new Dictionary<string, object>
            {
                {"userid", user.UserId.ToString()},
                {"user.LoginName", user.LoginName},
            });
            
            // call changecompany notification event
            if (fireChangeCompanyEvent)
            {
                DoChangeCompanyEvent(originalPersona);
            }
            context.AuthenticateResult = new AuthenticateResult(user.UserId.ToString(), user.LoginName, claims, idp);
        }

        private void DoChangeCompanyEvent(long userPersonaId)
        {
            var productInternalSettingList = GetProductInternalSettings(ProductEnum.UnifiedPlatform);
            string issueUri = ConfigReader.GetIssuerUri;
            string landingApiUri = ConfigReader.GetLandingAPIUri;
            string clientId = productInternalSettingList.First(a => a.Name.Equals("UnifiedLoginServerClientName", StringComparison.OrdinalIgnoreCase)).Value;
            string apiSecret = Encoding.UTF8.GetString(Convert.FromBase64String(productInternalSettingList.First(a => a.Name.Equals("UnifiedLoginServerClientSecret", StringComparison.OrdinalIgnoreCase)).Value));
            
            var accessToken = GetToken(issueUri, clientId, apiSecret, "userinfoapi notificationsapi");
            
            var apiPathAndQuery = $"{landingApiUri}api/persona/{userPersonaId}/company";
            PostApi(apiPathAndQuery, accessToken);
        }

        private void PostApi(string apiUrl, string accessToken)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                    var response = client.PostAsync(apiUrl, null).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonContent = response.Content.ReadAsStringAsync().Result;
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private bool UserIsAllowedToChangeContext(RPModel.UserLoginOnly impersonator, long impersonatorOrgId, string impersonatedBy)
        {
            var allowedToImpersonate = false;
            
            var impersonatorPersona = _personaManager.GetActivePersona(impersonator.RealPageId);
            var roleList = _userRoleManager.GetProductRolesByPersona(impersonatorPersona.PersonaId, ProductEnum.UnifiedPlatform);
            
            foreach (var productRole in roleList)
            {
                var roleRights = _userRoleManager.ListRightsByRole(impersonatorPersona.OrganizationPartyId, impersonatorPersona.Organization.RealPageId, ProductEnum.UnifiedPlatform, Convert.ToInt32(productRole.ID));
                if (roleRights.Any(a => a.Alias.Equals("AccessToUnifiedPlatform", StringComparison.OrdinalIgnoreCase) ||
                                        a.Alias.Equals("ViewOnlySupportToolAccess", StringComparison.OrdinalIgnoreCase)))
                {
                    allowedToImpersonate = true;
                    break;
                }
            }
            var userIsFromRealPageMasterOrg = ConfigReader.OrgMasterId.Equals(impersonatorOrgId.ToString(), StringComparison.OrdinalIgnoreCase);

            return allowedToImpersonate && userIsFromRealPageMasterOrg;
        }

        private void RevertSigninToOriginalUser(PreAuthenticationContext context, string idp)
        {
            var correlationId = _ctx.Authentication.User.Claims.CorrelationId();
            string userId = "";
            RPModel.UserLoginOnly user = null;
            if (_ctx.Authentication.User.Claims.Any(p => p.Type.Equals("ImpersonatedBy", StringComparison.OrdinalIgnoreCase)))
            {
                // get the original user id from the impersonated users claim and log the user back in
                userId = _ctx.Authentication.User.Claims.ImpersonatedBy();
                user = _userLoginLogic.GetUserLoginOnly(new Guid(userId));
            }
            else
            {
                if (_ctx.Authentication.User.Identity.IsAuthenticated)
                {
                    userId = _ctx.Authentication.User.Claims.UserId();
                    user = _userLoginLogic.GetUserLoginOnly(Convert.ToInt64(userId));
                }
            }

            if (user != null && !string.IsNullOrEmpty(userId))
            {
                var userPersona = _personaManager.GetActivePersona(user.RealPageId);
                user.PersonaId = userPersona.PersonaId;

                var claims = GetClaimsForUser(user, userPersona.OrganizationPartyId, context.SignInMessage.ClientId);
                var logData = new Dictionary<string, object> {{"User data", $"revertedUserId:{userId}"}};

                WriteToLog(LogEventLevel.Debug, "PreAuthenticateAsync: Revert impersonation", new Guid(correlationId), logData);
                context.SignInMessage.AcrValues = new List<string>();
                context.AuthenticateResult = new AuthenticateResult(user.UserId.ToString(), user.LoginName, claims, idp);
            }
            else
            {
                WriteToLog(LogEventLevel.Debug, "PreAuthenticateAsync: Revert impersonation failed, no userId", new Guid(correlationId));
                //context.AuthenticateResult = new AuthenticateResult("Could not revert back to original login.");
                context.SignInMessage.IdP = null;
                context.SignInMessage.AcrValues = new List<string>();
                //await base.PreAuthenticateAsync(context);
            }
        }

        public override async Task AuthenticateExternalAsync(ExternalAuthenticationContext context)
        {
            var userLoginField = "";
            var externalIdToken = "";
            LogExternalUserClaims(context, "user idp claims");

            // get the claim type for the login name based on the external identity
            var rpcache = new RPObjectCache();
            const string cacheKey = "providerConfigurations";
            var providerName = ProviderEnum.All.ToString().ToLower();
            var providerConfiguration = rpcache.GetFromCache(cacheKey, 600, () => _manageProvider.GetProviderConfigurationByName(providerName));

            LogExternalUserClaims(context, providerName);

            if (providerConfiguration.Any(p => string.Equals(p.AuthenticationType, context.ExternalIdentity.Provider, StringComparison.CurrentCultureIgnoreCase)))
            {
                var pc = (from a in providerConfiguration
                          where string.Equals(a.AuthenticationType, context.ExternalIdentity.Provider, StringComparison.CurrentCultureIgnoreCase)
                          select a).FirstOrDefault();
                if (pc != null)
                {
                    if (!string.IsNullOrEmpty(pc.UserLoginClaim))
                    {
                        userLoginField = pc.UserLoginClaim;
                    }
                }
            }

            if (context.ExternalIdentity.Claims != null && context.ExternalIdentity.Claims.Any(x => x.Type == "external_idtoken"))
            {
                externalIdToken = context.ExternalIdentity.Claims.FirstOrDefault(x => x.Type.Equals("external_idtoken", StringComparison.OrdinalIgnoreCase))?.Value;
            }

            var username = GetUsernameFromExternalIdentity(context.ExternalIdentity, userLoginField);
            if (string.IsNullOrEmpty(username))
            {
                LogExternalUserClaims(context, "null or empty username");
                context.AuthenticateResult = new AuthenticateResult("Could not determine identity from external provider.");
                return;
            }

            // validate user on sign in page is same as from IDP
            //var loginHintFromIdp = Convert.ToBase64String(Encoding.UTF8.GetBytes(username.ToLower()));
            var loginHint = "";
            if (context.ExternalIdentity.Claims != null && context.ExternalIdentity.Claims.Any(x => x.Type == "login_username"))
            {
                loginHint = context.ExternalIdentity.Claims.FirstOrDefault(x => x.Type.Equals("login_username", StringComparison.OrdinalIgnoreCase))?.Value;
            }

            if (!string.IsNullOrEmpty(loginHint) && (!loginHint.Equals(username, StringComparison.OrdinalIgnoreCase)))
            {
                LogExternalUserClaims(context, "login entered does not match idp returned");
                context.AuthenticateResult = new AuthenticateResult("You must be logged into the external identity provider using the same username indicated on the RealPage Unified Platform Login page.  Please log out of all systems and try again.");
                return;
            }

            // clear out the acrvalues in case it has a msgId in it
            context.SignInMessage.AcrValues = new List<string>();

            try
            {
                var authUser = await _authenticateService.AuthenticateExternalUser(username);
                if (authUser?.UserLogin == null)
                {
                    LogExternalUserClaims(context, "user not found in system");
                    string errorMessage = "Your Username/Password is incorrect.";
                    if (!string.IsNullOrEmpty(authUser?.ErrorReason))
                    {
                        errorMessage = authUser.ErrorReason;
                    }
                    
                    context.AuthenticateResult = new AuthenticateResult(errorMessage);
                    return;
                }

                var userInfo = authUser.UserLogin;

                // verify the IDP of the user matches the IDP they are coming from
                //var identityProviderOutput = _authenticateService.GetProviderByEnterpriseUserName(username);
                var identityProviderType = _manageProvider.GetProviderByEnterpriseUserName(username);

                if (string.IsNullOrEmpty(identityProviderType.AuthenticationType))
                {
                    context.AuthenticateResult = new AuthenticateResult("Your Username/Password is incorrect.");
                    return; // user provider not configured in database
                }

                if (identityProviderType.IsLocal || !string.Equals(context.ExternalIdentity.Provider, identityProviderType.AuthenticationType, StringComparison.CurrentCultureIgnoreCase))
                {
                    context.AuthenticateResult = new AuthenticateResult("You have clicked a link to access the RealPage Unified Platform through an external identity provider that is not associated with your Username. Please enter your credentials on this login page.");
                    return; // user provider not configured in database
                }

                var personaList = _personaManager.ListActivePersona(userInfo.RealPageId, false);
                if (personaList.Any(p => p.Organization.BooksCustomerMasterId == -1)){
                    long activePersonaId = GetActivePersonaId(userInfo, 0);
                    
                    if (personaList.Any(p => p.Organization.BooksCustomerMasterId == -1 && p.PersonaId != activePersonaId))
                    {
                        // change the user back to the employee company if they are logging back in later
                        activePersonaId = personaList.FirstOrDefault(p => p.Organization.BooksCustomerMasterId == -1).PersonaId;    
                        _personaManager.UpdateActivePersona(userInfo.RealPageId, activePersonaId);
                    }
                }

                var claims = GetClaimsForUser(userInfo, 0, context.SignInMessage.ClientId);
                if (!string.IsNullOrEmpty(externalIdToken))
                {
                    claims.Add(new Claim("external_idtoken", externalIdToken));
                }

                context.AuthenticateResult = new AuthenticateResult(userInfo.UserId.ToString(), userInfo.LoginName, claims, context.ExternalIdentity.Provider);

                LogEventActivity(claims, "User {0} {1} successfully logged-in using external IDP.");
            }
            catch (ApplicationException) // these get thrown when more than one user found
            {
                context.AuthenticateResult = new AuthenticateResult("Unable to determine unique user. Contact RealPage support for help.");
            }
        }

        /// <summary>
        /// Authenticate Local Async
        /// </summary>
        /// <param name="context">context of the local authentication</param>
        /// <returns>Task</returns>
        public override async Task AuthenticateLocalAsync(LocalAuthenticationContext context)
        {
            // validate user
            var authUser = await _authenticateService.AuthenticateUser(context.UserName, context.Password);

            // clear out the acrvalues in case it has a msgId in it
            context.SignInMessage.AcrValues = new List<string>();
            if (authUser == null)
            {
                context.AuthenticateResult = new AuthenticateResult("Your Username/Password is incorrect.");
                return;
            }

            var validUser = authUser.UserLogin;

            if (validUser == null)
            {
                if (authUser.IsError)
                {
                    context.AuthenticateResult = new AuthenticateResult(authUser.ErrorReason);
                    return;
                }

                context.AuthenticateResult =
                    new AuthenticateResult(
                        "Your Username/Password is incorrect.");
                return;
            }

            if (authUser.IsError)
            {
                context.AuthenticateResult = new AuthenticateResult(authUser.ErrorReason);
                return;
            }

            var userInfo = authUser.UserLogin;
            var personaList = _personaManager.ListActivePersona(userInfo.RealPageId, false);
            if (personaList.Any(p => p.Organization.BooksCustomerMasterId == -1)){
                long activePersonaId = GetActivePersonaId(userInfo, 0);
                    
                if (personaList.Any(p => p.Organization.BooksCustomerMasterId == -1 && p.PersonaId != activePersonaId))
                {
                    // change the user back to the employee company if they are logging back in later
                    activePersonaId = personaList.FirstOrDefault(p => p.Organization.BooksCustomerMasterId == -1).PersonaId;    
                    _personaManager.UpdateActivePersona(userInfo.RealPageId, activePersonaId);
                }
            }

            List<Claim> claims = new List<Claim>();
            try
            {
                claims = GetClaimsForUser(authUser.UserLogin, 0, context.SignInMessage.ClientId);
                if (claims.Count == 0)
                {
                    context.AuthenticateResult = new AuthenticateResult("Your user is not active in any companies.");
                    return;
                }
            }
            catch (Exception ex)
            {
                context.AuthenticateResult = new AuthenticateResult($"Exception during getting claims for user. {ex.Message}");
                return;
            }

            // Note: Subject is set to the user Id
            context.AuthenticateResult = new AuthenticateResult(authUser.UserLogin.UserId.ToString(), authUser.UserLogin.LoginName, claims);
            LogEventActivity(claims, "User {0} {1} successfully logged-in.");
        }
        
        private void LogExternalUserClaims(ExternalAuthenticationContext context, string errorReason)
        {
            var logData = new Dictionary<string, object>();
            try
            {
                var claimList = context.ExternalIdentity.Claims
                    .ToDictionary(claim => $"CLAIM-{claim.Type} (Issuer={claim.Issuer})", claim => claim.Value);
                logData.Add("context.ExternalIdentity.Claims", claimList);

                WriteToLog(LogEventLevel.Debug, $"AuthenticateExternalAsync: {errorReason}", Guid.NewGuid(), logData);
            }
            catch (Exception)
            {
                // ignored
            }
            WriteToLog(LogEventLevel.Debug, $"AuthenticateExternalAsync: {errorReason}", Guid.NewGuid(), logData);
        }

        
        public override Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var impersonating = "";

            var userId = Convert.ToInt32(context.Subject.GetSubjectId());
            var user = _userLoginLogic.GetUserLoginOnly(userId);

            try
            {
                impersonating = context.Subject?.FindFirst("ImpersonatedBy")?.Value;
            }
            catch (Exception)
            {
                // ignored
            }

            if (user == null) return base.GetProfileDataAsync(context);

            var persona = _personaManager.GetActivePersona(user.RealPageId);
            user.PersonaId = persona.PersonaId;

            if (!(context.Client.ClientId.Equals("Landing", StringComparison.OrdinalIgnoreCase)
                || context.Client.ClientId.Equals("Migration", StringComparison.OrdinalIgnoreCase)
                || context.Client.ClientId.Equals("greenbookoidc", StringComparison.OrdinalIgnoreCase))
                || string.IsNullOrEmpty(impersonating))
            {
                var issuedClaims = GetClaimsForUser(user, persona.OrganizationPartyId, context.Client.ClientId);
                if (!string.IsNullOrEmpty(impersonating) && issuedClaims.Find(p => p.Type.Equals("ImpersonatedBy", StringComparison.OrdinalIgnoreCase)) == null)
                {
                    issuedClaims.Add(new Claim("ImpersonatedBy", impersonating));
                }

                context.IssuedClaims = issuedClaims;
            }
            else
            {
                context.IssuedClaims = context.Subject.Claims;
            }

            return Task.FromResult(0);
        }

        public override Task IsActiveAsync(IsActiveContext context)
        {
            context.IsActive = false;
            if (!context.Subject.Identity.IsAuthenticated) return Task.FromResult(0);

            RPModel.UserLoginOnly user = _userLoginLogic.GetUserLoginOnly(Convert.ToInt64(context.Subject.GetSubjectId()));
            var activePersona = _personaManager.GetActivePersona(user.RealPageId);

            OrganizationStatus primaryOrgStatus = _userLoginIdentityLogic.GetUserOrganizationStatus(user.UserId, user.LastLogin, activePersona.OrganizationPartyId, false);

            if (primaryOrgStatus?.IsActive == true && primaryOrgStatus?.IsLocked == false && primaryOrgStatus?.IsExpired == false)
            {
                context.IsActive = true;
            }

            return Task.FromResult(0);
        }

        private List<Claim> GetClaimsForUser(RPModel.UserLoginOnly userInfo, long organizationPartyId, string clientId)
        {
            var claims = new List<Claim>();
            IPerson person = _personManager.GetPerson(userInfo.RealPageId);

            long activePersonaId = GetActivePersonaId(userInfo, organizationPartyId);

            if (activePersonaId == 0)
            {
                return claims;
            }

            var persona = _personaManager.GetPersona(activePersonaId);

            claims.AddRange(GetOrganizationClaims(persona.Organization));
            claims.AddRange(GetUserClaims(person, userInfo, persona));

            IList<ProductRole> roleList;
            IList<SamlAttributes> _;

            // add any dynamic claims for the given client
            var userClaimTypesForClients = _identityServerRepository.GetUserClaimTypesForClient(clientId);
            foreach (var clientClaim in userClaimTypesForClients)
            {
                if (!string.IsNullOrEmpty(clientClaim.SamlAttributeName))
                {
                    var userClaim = GetSamlUserClaimAndAttributesForProduct(clientClaim.ClaimName, clientClaim.SamlAttributeName, persona.PersonaId, (ProductEnum) clientClaim.ProductId, out _);
                    if (userClaim != null)
                        claims.Add(userClaim);
                    continue;
                }

                if (string.IsNullOrEmpty(clientClaim.SamlAttributeName))
                {
                    string dataField = clientClaim.ClaimName.ToUpperInvariant();
                    string claimName = clientClaim.ClaimName;

                    if (clientClaim.ClaimName.Contains("~"))
                    {
                        var claimSplit = clientClaim.ClaimName.Split('~');
                        dataField = claimSplit[0].ToUpperInvariant();
                        claimName = claimSplit[1];
                    }

                    switch (dataField)
                    {
                        case "LOGINNAME":
                            claims.Add(new Claim(claimName, userInfo.LoginName));
                            break;

                        case "FIRSTNAME":
                            claims.Add(new Claim(claimName, person.FirstName));
                            break;

                        case "LASTNAME":
                            claims.Add(new Claim(claimName, person.LastName));
                            break;

                        case "USERID":
                            claims.Add(new Claim(claimName, userInfo.UserId.ToString()));
                            break;

                        case "ROLE":
                            roleList = _userRoleManager.GetProductRolesByPersona(persona.PersonaId, (ProductEnum) clientClaim.ProductId);
                            if (roleList != null && roleList.Count > 0)
                                claims.AddRange(roleList.Select(a => new Claim("role", a.Name)).ToList());
                            break;

                        case "ROLE|ROLEID":
                            roleList = _userRoleManager.GetProductRolesByPersona(persona.PersonaId, (ProductEnum) clientClaim.ProductId);
                            if (roleList != null && roleList.Count > 0)
                            {
                                claims.AddRange(roleList.Select(a => new Claim("role", a.Name)).ToList());
                                claims.AddRange(roleList.Select(a => new Claim("roleId", a.ID)).ToList());
                            }

                            break;

                        case "ROLE|ROLEALIAS":
                            roleList = _userRoleManager.GetProductRolesByPersona(persona.PersonaId, (ProductEnum) clientClaim.ProductId);
                            if (roleList != null && roleList.Count > 0)
                            {
                                claims.AddRange(roleList.Select(a => new Claim("role", a.Name)).ToList());
                                claims.AddRange(roleList.Select(a => new Claim("rolealias", a.Alias)).ToList());
                            }

                            break;

                        case "ROLE|RIGHTS":
                            roleList = _userRoleManager.GetProductRolesByPersona(persona.PersonaId, (ProductEnum) clientClaim.ProductId);
                            if (roleList != null && roleList.Count > 0)
                            {
                                claims.AddRange(roleList.Select(a => new Claim("role", a.Name)).ToList());
                                claims.AddRange(roleList.Select(a => new Claim("roleId", a.ID)).ToList());
                                claims.AddRange(roleList.Select(a => new Claim("rolealias", a.Alias)).ToList());
                            }

                            foreach (var productRole in roleList)
                            {
                                var roleRights = _userRoleManager.ListRightsByRole(persona.OrganizationPartyId, persona.Organization.RealPageId, (ProductEnum) clientClaim.ProductId, Convert.ToInt32(productRole.ID));
                                claims.AddRange(roleRights.Select(a => new Claim("right", a.Alias)).ToList());
                            }

                            break;
                        case "PHONENUMBER":
                            IList<TelecommunicationNumber> telecommunicationNumbers = _manageTelecommunicationNumber.ListTelecommunicationNumberForPerson(person.RealPageId, null);
                            string ph = "";
                            string ut = "";

                            if (telecommunicationNumbers != null && telecommunicationNumbers.Count > 0)
                            {
                                telecommunicationNumbers.OrderBy(a => a.ContactMechanismId);
                                TelecommunicationNumber tnObj = telecommunicationNumbers[0];
                                IList<ContactMechanismUsageType> usagetype = _contactMechanismUsageType.ListContactMechanismUsageType("Phone Type");

                                ph = tnObj.AreaCode + tnObj.PhoneNumber;
                                ut = usagetype.FirstOrDefault(s => s.ContactMechanismUsageTypeId == tnObj.ContactMechanismUsageTypeId).Name;
                                //ut = cm.Name;
                            }

                            claims.Add(new Claim("PhoneNumber", ph));
                            claims.Add(new Claim("PhoneType", ut));
                            break;

                    }
                }
            }

            switch (clientId.ToUpperInvariant())
            {
                case "PROPERTYPHOTOS":
                    if (!CheckForRight(ProductEnum.PropertyPhotos, persona))
                    {
                        throw new Exception("User not authorized for this product");
                    }
                    break;

                case "VENDORMARKETPLACE":
                    if (!CheckForRight(ProductEnum.VendorMarketplace, persona))
                    {
                        throw new Exception("User not authorized for this product");
                    }
                    break;
            }

            claims.AddRange(GetPortfolioProductUserClaims(persona.OrganizationPartyId, clientId, userInfo.UserId));
            
            return claims;
        }

        private IEnumerable<Claim> GetUserClaims(IPerson person, RPModel.IUserLoginOnly userInfo, IPersona persona)
        {
            var claims = new List<Claim>
            {
                new Claim("correlationId", Guid.NewGuid().ToString()),
                new Claim("firstName", person.FirstName),
                new Claim("middleName", person.MiddleName),
                new Claim("lastName", person.LastName),
                new Claim("loginName", userInfo.LoginName),
                new Claim("realPageId", userInfo.RealPageId.ToString()),
                new Claim("greenBookUrl", ConfigReader.GetReturnUri),
                new Claim("personaId", persona.PersonaId.ToString()),
                new Claim("userPartyId", person.PartyId.ToString())
            };

            if (claims.All(a => a.Type != "sub"))
                claims.Add(new Claim("sub", userInfo.UserId.ToString()));

            return claims;
        }

        private long GetActivePersonaId(RPModel.UserLoginOnly userInfo, long organizationPartyId)
        {
            long activePersonaId = _personaManager.GetActivePersonaId(userInfo.RealPageId);
            var activePersonaList = _personaManager.ListActivePersona(userInfo.RealPageId, false);
            Persona newPersona;

            if (activePersonaList.All(p => p.PersonaId != activePersonaId))
            {
                // the active persona was not found, so get the first available for the company, or the default one?
                if (organizationPartyId == 0)
                {
                    // get the users primary org because it wasn't passed by the caller, but exclude the external company -2 because it can't be logged into
                    var organizationList = _userLoginLogic.GetUserPersonaOrganization(userInfo.LoginName);
                    if (organizationList.Any(p => p.PrimaryOrganization && p.OrganizationRealPageId != DefaultUserClaim.ExternalCompanyRealPageId))
                    {
                        organizationPartyId = organizationList.FirstOrDefault(p => p.PrimaryOrganization).OrganizationPartyId;
                    }
                }

                if (activePersonaList.All(p => p.OrganizationPartyId != organizationPartyId))
                {
                    // get whatever persona is available
                    if (activePersonaList.Count > 0)
                    {
                        newPersona = activePersonaList.FirstOrDefault(p => p.Organization.RealPageId != DefaultUserClaim.ExternalCompanyRealPageId);
                        if (newPersona != null)
                        {
                            _personaManager.UpdateActivePersona(userInfo.RealPageId, newPersona.PersonaId);
                            return newPersona.PersonaId;
                        }

                        return 0;
                    }

                    return 0;
                }

                // possibly change to get the default persona if we fix the data?
                newPersona = activePersonaList.FirstOrDefault(p => p.OrganizationPartyId == organizationPartyId);
                if (newPersona != null)
                {
                    _personaManager.UpdateActivePersona(userInfo.RealPageId, newPersona.PersonaId);
                    return newPersona.PersonaId;
                }
            }

            return activePersonaId;
        }

        private IList<ProductInternalSetting> GetProductInternalSettings(ProductEnum product)
        {
            IList<ProductInternalSetting> productInternalSettingList;
            var rpcache = new RPObjectCache();
            var cacheKey = $"productInternalSetting_{(int)product}";
            productInternalSettingList = rpcache.GetFromCache<IList<ProductInternalSetting>>(cacheKey, 600, () =>
            {
                // load from database
                var productRepository = new ProductInternalSettingRepository();
                return productRepository.GetProductSettings((int)product).ToList();
            });

            return productInternalSettingList;
        }

        private bool CheckForRight(ProductEnum product, IPersona persona)
        {
            var productInternalSettingList = GetProductInternalSettings(product);
            if (productInternalSettingList.All(a => !(a.Name.Equals("REQUIRESUNIFIEDLOGINRIGHT", StringComparison.OrdinalIgnoreCase)))) return true;

            var rightToCheck = productInternalSettingList.First(a => a.Name.Equals("REQUIRESUNIFIEDLOGINRIGHT", StringComparison.OrdinalIgnoreCase)).Value;
            if (string.IsNullOrEmpty(rightToCheck)) return true;

            var roleList = _userRoleManager.GetProductRolesByPersona(persona.PersonaId, ProductEnum.UnifiedPlatform);
            foreach (ProductRole pr in roleList)
            {
                IList<ProductRight> roleRights = _userRoleManager.ListRightsByRole(persona.OrganizationPartyId, persona.Organization.RealPageId, ProductEnum.UnifiedPlatform, Convert.ToInt32(pr.ID));
                if (roleRights.Where(q => q.Alias != null).Any(a => a.Alias.Equals(rightToCheck, StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Used to get a client token
        /// </summary>
        /// <param name="issueUri"></param>
        /// <param name="clientId"></param>
        /// <param name="apiSecret"></param>
        /// <param name="scopes"></param>
        /// <returns></returns>
        private string GetToken(string issueUri, string clientId, string apiSecret, string scopes)
        {
            try
            {
                RPObjectCache rpCache = new RPObjectCache();
                var cacheKey = $"GetToken_{clientId}_{scopes}";

                string accessToken = rpCache.GetFromCache<string>(cacheKey, 180, () =>
                {
                    TokenClient tokenClient = new TokenClient($"{issueUri}/connect/token", clientId, apiSecret);

                    var tokenResponse = tokenClient.RequestClientCredentialsAsync(scopes).Result;

                    if (tokenResponse.IsError)
                    {
                        throw new Exception($"UserService.GetToken - Received null or empty token. {tokenResponse.Error}");
                    }

                    return tokenResponse.AccessToken;
                });

                return accessToken;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in UserService.GetToken- {ex.Message}");
            }
        }

        /// <summary>
        /// Log Event Activity
        /// </summary>
        /// <param name="authUser">authUser</param>
        /// <param name="claims">claims</param>
        /// <param name="message">message</param>
        private void LogEventActivity(List<Claim> claims, string message)
        {
            Guid correlationId = Guid.Empty;
            try
            {
                Guid.TryParse(
                    (from nvp in claims where nvp.Type.Equals("CORRELATIONID", StringComparison.OrdinalIgnoreCase) select nvp.Value).FirstOrDefault(),
                    out correlationId);

                var booksMasterOrganizationId =
                    Convert.ToInt64(
                        (from nvp in claims where nvp.Type.Equals("orgMasterId", StringComparison.OrdinalIgnoreCase) select nvp.Value).FirstOrDefault());

                Guid realGuid;
                Guid.TryParse((from nvp in claims where nvp.Type.Equals("realPageId", StringComparison.OrdinalIgnoreCase) select nvp.Value).FirstOrDefault(), out realGuid);

                var fromUserFirstName = (from nvp in claims where nvp.Type.Equals("FirstName", StringComparison.OrdinalIgnoreCase) select nvp.Value).FirstOrDefault().TrimEnd();
                var fromUserLastName = (from nvp in claims where nvp.Type.Equals("LastName", StringComparison.OrdinalIgnoreCase) select nvp.Value).FirstOrDefault().TrimEnd();

                var loginName = (from nvp in claims where nvp.Type.Equals("loginName", StringComparison.OrdinalIgnoreCase) select nvp.Value).FirstOrDefault().TrimEnd();
                var userId = Convert.ToInt64((from nvp in claims where nvp.Type.Equals("sub", StringComparison.OrdinalIgnoreCase) select nvp.Value).FirstOrDefault());

                LogActivity.WriteActivity(new ActivityDetails
                {
                    LogActivityTypeName = LogActivityTypeConstants.LOGIN_SUCCESS,
                    LogCategoryName = LogActivityCategoryType.Security.ToString(),
                    CorrelationId = correlationId.ToString(),
                    BooksMasterOrganizationId = booksMasterOrganizationId,
                    Message = string.Format(message, fromUserFirstName, fromUserLastName),
                    FromUserLoginName = loginName,
                    FromUserLoginId = userId,

                    FromUserFirstName = fromUserFirstName,
                    FromUserLastName = fromUserLastName,
                    FromUserRealpageId = realGuid.ToString(),

                    ToUserLoginName = null,
                    ToUserLoginId = null,
                    BooksProductCode = ProductEnum.UnifiedPlatform.ToEnumDescription()
                });
            }
            catch (Exception ex)
            {
                Log.Write(LogEventLevel.Error, ex, message);
            }
        }

        /// <summary>
        /// Used to write to the log
        /// </summary>
        /// <param name="logType">logType</param>
        /// <param name="message">message</param>
        /// <param name="logData">logData</param>
        /// <param name="exception">exception</param>
        /// <param name="correlationId">correlationId</param>
        private void WriteToLog(LogEventLevel logType, string message, Guid correlationId, Dictionary<string, object> logData = null, Exception exception = null)
        {
            var logger = Log.Logger;
			if (logData?.Keys != null)
			{
				logger = logger.ForContext("AdditionalInfo", JsonConvert.SerializeObject(logData, Formatting.Indented), false);
			}
			logger = logger.ForContext("ProductModule", this.GetType());
            logger = logger.ForContext("CorrelationId", correlationId);
            logger.Write(logType, exception, message );
        }

        /// <summary>
        /// Used to get the user login name from the third party IDP for the user
        /// </summary>
        /// <param name="externalIdentity"></param>
        /// <param name="userClaimType"></param>
        /// <returns>default location of the identity provider name</returns>
        private static string GetUsernameFromExternalIdentity(ExternalIdentity externalIdentity, string userClaimType)
        {
            // see if external identity has a claim type to look for
            if (!string.IsNullOrEmpty(userClaimType))
            {
                Claim nameClaim = externalIdentity.Claims.FirstOrDefault(a => a.Type.Equals(userClaimType, StringComparison.OrdinalIgnoreCase));
                if (nameClaim == null)
                {
                    var ex = new Exception($"No usable {userClaimType} claim found.  Review exception Data for actual claims included.");
                    foreach (var claim in externalIdentity.Claims)
                    {
                        ex.Data.Add($"CLAIM-{claim.Type} (Issuer={claim.Issuer})", claim.Value);
                    }

                    throw ex;
                }

                return nameClaim.Value;
            }

            // return the default location of the identity provider name
            return externalIdentity.ProviderId;
        }
    }

    /// <summary>
    /// ClaimResponse
    /// </summary>
    public class ClaimResponse
    {
        public List<RPClaim> Claims { get; set; }

        public bool IsValidUser { get; set; }
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// RPClaim
    /// </summary>
    public class RPClaim
    {
        public string Type { get; set; }
        public string Value { get; set; }
    }
}