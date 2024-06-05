using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Attributes;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using Serilog.Events;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Controllers;
using ZiggyCreatures.Caching.Fusion;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using StackExchange.Redis;
using ZiggyCreatures.Caching.Fusion.Serialization.NewtonsoftJson;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI
{
    /// <summary>
    /// Base controller for all webapi projects
    /// </summary>
    [AllowCors("LandingAPICORSAllowedOrigins"), AuthorizeScope("rplandingapi")]
    public class BaseApiController : ApiController
    {
        public static readonly Guid EmployeeCompanyRealPageId = new Guid("0D018E46-C20E-477D-ADED-4E5A35FB8F99");

        public static IFusionCache FusionCache;
        private static FusionCacheOptions fusionOptions;
        private static RedisCacheOptions redisOptions;
        
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
        private string _OrgType;
        private Guid _organizationRealPageGuid;

        public long _personaId;
        public string _greenBookAccessToken = string.Empty;

        public bool _realPageEmployee = false;

        /// <summary>
        /// Default constructor
        /// </summary>
        public BaseApiController()
        {
        }

        /// <summary>
        /// Used to initialize the base controller and retrieve the needed information used by the infrastructure
        /// </summary>
        /// <param name="controllerContext"></param>
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);

            if (FusionCache == null)
            {
                fusionOptions = new FusionCacheOptions()
                {
                    CacheName = "landingapi",
                    DefaultEntryOptions = new FusionCacheEntryOptions
                    {
                        Duration = TimeSpan.FromMinutes(2),
                        IsFailSafeEnabled = true,
                        FailSafeMaxDuration = TimeSpan.FromMinutes(2),
                        FailSafeThrottleDuration = TimeSpan.FromSeconds(30),

                        //FactorySoftTimeout = TimeSpan.FromMilliseconds(100),
                        //FactoryHardTimeout = TimeSpan.FromMilliseconds(1500)
                    },
                };

                redisOptions = new RedisCacheOptions()
                {
                    Configuration = "localhost:6379",
                    InstanceName = "landingapi",
                    ConfigurationOptions = new ConfigurationOptions() { }
                };
                FusionCache = new FusionCache(fusionOptions);
                FusionCache.SetupDistributedCache(new RedisCache(redisOptions), new FusionCacheNewtonsoftJsonSerializer());
            }
            
            ClaimsPrincipal currentClaimPrincipal = ClaimsPrincipal.Current;
            if (currentClaimPrincipal.Identity.IsAuthenticated)
            {
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
                    Persona persona = managePersona.GetActivePersonaWithoutRights(_realpageUserId); // no company context so we need to use the active persona

                    identity.AddClaim(new Claim("sub", userLogin.UserId.ToString()));
                    identity.AddClaim(new Claim("orgPartyId", persona.Organization.PartyId.ToString()));
                    identity.AddClaim(new Claim("orgType", persona.Organization.organizationType.Name.ToString()));
                    identity.AddClaim(new Claim("ORGID", persona.Organization.RealPageId.ToString()));
                    identity.AddClaim(new Claim("LOGINNAME", userLogin.LoginName));
                    identity.AddClaim(new Claim("ORGMASTERID", persona.Organization.BooksMasterId.ToString()));
                    identity.AddClaim(new Claim("ORGNAME", persona.Organization.Name));
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
                _OrgType = _userClaims.OrganizationType;
                _realpageUserId = _userClaims.UserRealPageGuid;
                _correlationId = _userClaims.CorrelationId;
                _organizationRealPageGuid = _userClaims.OrganizationRealPageGuid;
                _clientCode = _userClaims.ClientCode;
                _personaId = _userClaims.PersonaId;
                _greenBookAccessToken = Request.Headers.Authorization.Parameter;
                _realPageEmployee = _userClaims.RealPageEmployee;

                List<string> userRights = BaseUserRights.GetUserRightsBy(currentClaimPrincipal, _userClaims, FusionCache);
                _userClaims.Rights = userRights;

            }
            else
            {
                // if the call is anonymous, build a default guid and later the code may override it if one has been stored somewhere else
                _userClaims = new DefaultUserClaim() { CorrelationId = Guid.NewGuid() };
                _correlationId = _userClaims.CorrelationId;
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
        public void WriteToLog(LogEventLevel logType, string message, Dictionary<string, object> logData = null, Exception exception = null, object[] messageProperties = null)
        {
            try
            {
                string correlationId = "";
                if (_userClaims != null)
                {
                    correlationId = (_userClaims.CorrelationId != Guid.Empty) ? _userClaims.CorrelationId.ToString() : "";
                }

                var logger = Log.Logger;
                if (logData?.Keys != null)
                {
                    logger = logger.ForContext("AdditionalInfo", JsonConvert.SerializeObject(logData, Formatting.Indented), false);
                }

                logger = logger.ForContext("ProductModule", this.GetType());
                logger = logger.ForContext("CorrelationId", correlationId);
                logger.Write(level: logType, exception: exception, messageTemplate: message, propertyValue0: messageProperties?[0], propertyValue1: messageProperties?[1]);
            }
            catch
            {
                /*ignored*/
            }
        }
    }
}
