using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;
using System.Security.Claims;
using UnifiedLogin.BusinessLogic.Base;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.LandingAPIEnterprise
{
    /// <summary>
    /// Base controller for all API projects migrated to ASP.NET Core
    /// </summary>
    [ServiceFilter(typeof(InitializeUserClaimsFilter))]
    public class BaseApiController : ControllerBase
    {
        public static readonly Guid EmployeeCompanyRealPageId = new Guid("0D018E46-C20E-477D-ADED-4E5A35FB8F99");

        /// <summary>
        /// Enterprise UserId
        /// </summary>
        protected int _EnterpriseUserId = 0;

        /// <summary>
        /// Realpage UserId
        /// </summary>
        protected Guid _realpageUserId;

        /// <summary>
        /// Organization PartyId
        /// </summary>
        protected long _orgPartyId = 0;

        /// <summary>
        /// UserName
        /// </summary>
        protected string _loginName = "";

        /// <summary>
        /// Used to filter, sort and limit the number of records being returned by the request.
        /// </summary>
        protected RequestParameter? GlobalRequestParameter;

        /// <summary>
        /// Holds default user claim related information
        /// </summary>
        protected DefaultUserClaim _userClaims = new DefaultUserClaim();

        protected string _clientCode = string.Empty;

        private Guid _correlationId;
        private long _organizationMasterId;
        private string _organizationName = string.Empty;
        private string _OrgType = string.Empty;
        private Guid _organizationRealPageGuid;

        protected long _personaId;
        protected string _greenBookAccessToken = string.Empty;

        protected bool _realPageEmployee = false;

        /// <summary>
        /// Default constructor
        /// </summary>
        public BaseApiController()
        {
        }

        /// <summary>
        /// Legacy/unit test constructor retaining original dependencies.
        /// </summary>
        protected BaseApiController(IRepository repository, HttpMessageHandler messageHandler, DefaultUserClaim userClaims)
        {
            _userClaims = userClaims;
            InitializeFromClaims(userClaims);
        }

        /// <summary>
        /// Initialize authenticated user claims and context - called by InitializeUserClaimsFilter
        /// </summary>
        internal void InitializeAuthenticatedUser(ClaimsPrincipal currentClaimPrincipal)
        {
            if (currentClaimPrincipal.HasClaim(p => p.Type.Equals("client_info", StringComparison.OrdinalIgnoreCase)))
            {
                var identity = currentClaimPrincipal.Identity as ClaimsIdentity;
                
                // Get RealPage User ID from client_info claim
                var clientInfoClaim = currentClaimPrincipal.Claims
                    .FirstOrDefault(c => c.Type.Equals("client_info", StringComparison.OrdinalIgnoreCase));
                
                if (clientInfoClaim != null && Guid.TryParse(clientInfoClaim.Value, out Guid realGuid))
                {
                    _realpageUserId = realGuid;
                }

                // Add realPageId claim if not already present
                if (identity != null && !identity.HasClaim(c => c.Type == "realPageId"))
                {
                    identity.AddClaim(new Claim("realPageId", _realpageUserId.ToString()));
                }

                // Get person information
                IManagePerson personLogic = new ManagePerson();
                Person person = personLogic.GetPerson(_realpageUserId);
                
                if (person == null)
                {
                    string? clientId = currentClaimPrincipal.Claims
                        .FirstOrDefault(c => c.Type == "client_id")?.Value;
                    throw new Exception($"Missing persona information for client_info user. client: {clientId} realPageId: {_realpageUserId}");
                }

                IManageUserLogin userLoginLogic = new ManageUserLogin();
                IManageUserRoleRight userRoleRight = new ManageUserRoleRight();
                var userLogin = userLoginLogic.GetUserLoginOnly(_realpageUserId);

                IManagePersona managePersona = new ManagePersona();
                // Active Persona is linked to one organization
                Persona persona = managePersona.GetActivePersonaWithoutRights(_realpageUserId);

                // Add additional claims if not already present
                if (identity != null)
                {
                    AddClaimIfNotExists(identity, "sub", userLogin.UserId.ToString());
                    AddClaimIfNotExists(identity, "orgPartyId", persona.Organization.PartyId.ToString());
                    AddClaimIfNotExists(identity, "orgType", persona.Organization.organizationType.Name);
                    AddClaimIfNotExists(identity, "ORGID", persona.Organization.RealPageId.ToString());
                    AddClaimIfNotExists(identity, "LOGINNAME", userLogin.LoginName);
                    AddClaimIfNotExists(identity, "ORGMASTERID", persona.Organization.BooksMasterId.ToString());
                    AddClaimIfNotExists(identity, "ORGNAME", persona.Organization.Name);
                    AddClaimIfNotExists(identity, "FIRSTNAME", person.FirstName);
                    AddClaimIfNotExists(identity, "LASTNAME", person.LastName);
                    AddClaimIfNotExists(identity, "PERSONAID", persona.PersonaId.ToString());

                    // Get the users role so the rights can be retrieved
                    IList<SharedObjects.Product.UnifiedLogin.Role> userRoles = userRoleRight
                        .GetAssignedRoleForPersona(ProductEnum.UnifiedPlatform, persona.PersonaId, persona.Organization.PartyId);
                    
                    foreach (var role in userRoles)
                    {
                        if (!identity.HasClaim(c => c.Type == "roleid" && c.Value == role.Name))
                        {
                            identity.AddClaim(new Claim("roleid", role.Name));
                        }
                    }
                }
            }

            // Initialize DefaultUserClaim from ClaimsPrincipal
            _userClaims = new DefaultUserClaim(currentClaimPrincipal);
            
            // Extract authorization token from headers
            if (HttpContext.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                var authValue = authHeader.ToString();
                if (authValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    _greenBookAccessToken = authValue.Substring("Bearer ".Length).Trim();
                }
            }

            // Get user rights
            List<string> userRights = BaseUserRights.GetUserRightsBy(currentClaimPrincipal, _userClaims);
            _userClaims.Rights = userRights;

            // Initialize protected fields from claims
            InitializeFromClaims(_userClaims);
        }

        /// <summary>
        /// Initialize for anonymous users - called by InitializeUserClaimsFilter
        /// </summary>
        internal void InitializeAnonymousUser()
        {
            // if the call is anonymous, build a default guid
            _userClaims = new DefaultUserClaim { CorrelationId = Guid.NewGuid() };
            _correlationId = _userClaims.CorrelationId;
        }

        /// <summary>
        /// Initialize protected fields from user claims
        /// </summary>
        private void InitializeFromClaims(DefaultUserClaim userClaims)
        {
            _EnterpriseUserId = userClaims.UserId;
            _orgPartyId = userClaims.OrganizationPartyId;
            _loginName = userClaims.LoginName ?? string.Empty;
            _organizationMasterId = userClaims.OrganizationMasterId;
            _organizationName = userClaims.OrganizationName ?? string.Empty;
            _OrgType = userClaims.OrganizationType ?? string.Empty;
            _realpageUserId = userClaims.UserRealPageGuid;
            _correlationId = userClaims.CorrelationId;
            _organizationRealPageGuid = userClaims.OrganizationRealPageGuid;
            _clientCode = userClaims.ClientCode ?? string.Empty;
            _personaId = userClaims.PersonaId;
            _realPageEmployee = userClaims.RealPageEmployee;
        }

        /// <summary>
        /// Add claim to identity if it doesn't already exist
        /// </summary>
        private void AddClaimIfNotExists(ClaimsIdentity identity, string type, string value)
        {
            if (!identity.HasClaim(c => c.Type == type))
            {
                identity.AddClaim(new Claim(type, value));
            }
        }

        /// <summary>
        /// Used to write to the central log
        /// </summary>
        /// <param name="logType">Log Type</param>
        /// <param name="message">Message template</param>
        /// <param name="logData">Dictionary of additional properties to log</param>
        /// <param name="exception">Exception details</param>
        /// <param name="messageProperties">Message properties</param>
        protected internal void WriteToLog(
            LogEventLevel logType, 
            string message, 
            Dictionary<string, object>? logData = null, 
            Exception? exception = null, 
            object[]? messageProperties = null)
        {
            try
            {
                string correlationId = string.Empty;
                if (_userClaims != null)
                {
                    correlationId = (_userClaims.CorrelationId != Guid.Empty) 
                        ? _userClaims.CorrelationId.ToString() 
                        : string.Empty;
                }

                var logger = Log.Logger;
                
                if (logData?.Keys != null)
                {
                    logger = logger.ForContext("AdditionalInfo", 
                        JsonConvert.SerializeObject(logData, Formatting.Indented), 
                        destructureObjects: false);
                }

                logger = logger.ForContext("ProductModule", GetType().FullName ?? GetType().Name);
                logger = logger.ForContext("CorrelationId", correlationId);

                // Write log with proper parameter handling
                if (messageProperties != null && messageProperties.Length > 0)
                {
                    logger.Write(
                        level: logType, 
                        exception: exception, 
                        messageTemplate: message, 
                        propertyValues: messageProperties);
                }
                else
                {
                    logger.Write(
                        level: logType, 
                        exception: exception, 
                        messageTemplate: message);
                }
            }
            catch
            {
                // Swallow logging exceptions to prevent application failures
            }
        }

        /// <summary>
        /// Helper method to write diagnostic logs
        /// </summary>
        protected void WriteToDiagnosticLog(string message, object[]? messageProperties = null, Dictionary<string, object>? logData = null)
        {
            WriteToLog(LogEventLevel.Debug, message, logData, null, messageProperties);
        }

        /// <summary>
        /// Helper method to write error logs
        /// </summary>
        protected void WriteToErrorLog(string message, object[]? messageProperties = null, Exception? exception = null, Dictionary<string, object>? logData = null)
        {
            WriteToLog(LogEventLevel.Error, message, logData, exception, messageProperties);
        }

        /// <summary>
        /// Helper method to write information logs
        /// </summary>
        protected void WriteToInformationLog(string message, object[]? messageProperties = null, Dictionary<string, object>? logData = null)
        {
            WriteToLog(LogEventLevel.Information, message, logData, null, messageProperties);
        }
    }

    /// <summary>
    /// Action filter to initialize user claims before each controller action executes
    /// </summary>
    public class InitializeUserClaimsFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.Controller is BaseApiController baseController)
            {
                var currentClaimPrincipal = context.HttpContext.User;
                
                if (currentClaimPrincipal?.Identity?.IsAuthenticated == true)
                {
                    try
                    {
                        baseController.InitializeAuthenticatedUser(currentClaimPrincipal);
                    }
                    catch (Exception ex)
                    {
                        // Log the error using Serilog directly since WriteToLog is protected
                        Log.Error(ex, "Error initializing authenticated user in InitializeUserClaimsFilter");
                        throw;
                    }
                }
                else
                {
                    baseController.InitializeAnonymousUser();
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // No implementation needed
        }
    }
}
