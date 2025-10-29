using Newtonsoft.Json;
using RealPage.UnifiedNotifications;
using UnifiedLogin.DataAccess;
using UnifiedLogin.BusinessLogic.Logic.Enterprise.Helpers;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.BusinessLogic.Logic
{
    /// <summary>
    /// Manage Persona
    /// </summary>
    public class ManagePersona : IManagePersona
    {
        #region Private Variables
        IPersonaRepository _personaRepository;
        readonly ITokenHelper _tokenHelper;
        readonly IProductInternalSettingRepository _productRepository;
        private DefaultUserClaim _userClaim;
        #endregion

        #region Constructors
        /// <summary>
        /// ManagePersona Constructor
        /// </summary>
        /// <param name="personaRepository"></param>
        public ManagePersona(IPersonaRepository personaRepository)
        {
            _personaRepository = personaRepository;
        }

        /// <summary>
        /// Create a basic instance of the ManagePerson Controller class
        /// </summary>
        public ManagePersona()
        {
            _personaRepository = new PersonaRepository();
            _tokenHelper = new TokenHelper();
            _productRepository = new ProductInternalSettingRepository();
        }

        /// <summary>
        /// Create a basic instance of the ManagePerson Controller class
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="userClaim"></param>
        /// <param name="messageHandler"></param>
        public ManagePersona(IRepository repository, DefaultUserClaim userClaim, HttpMessageHandler messageHandler)
        {
            _userClaim = userClaim;
            _personaRepository = new PersonaRepository(repository,userClaim);
            _productRepository = new ProductInternalSettingRepository(repository);
            _tokenHelper = new TokenHelper(repository);
        }

        /// <summary>
        /// Create a basic instance of the ManagePerson Controller class
        /// </summary>
        public ManagePersona(DefaultUserClaim userClaim)
        {
            _personaRepository = new PersonaRepository(userClaim);
            _productRepository = new ProductInternalSettingRepository();
            _tokenHelper = new TokenHelper();
            _userClaim = userClaim;
        }
        #endregion

        #region Public ManagePerson methods
        /// <summary>
        /// Get Persona Environment Type
        /// </summary>
        /// <returns>Persona Environment Type Object</returns>
        public IList<PersonaEnvironment> GetPersonaEnvironmentType()
        {
            return _personaRepository.GetPersonaEnvironmentType();
        }
        
        /// <summary>
        /// Create a new Persona
        /// </summary>
        /// <param name="personRealPageId">User unique identifier</param>
        /// <param name="organizationRealPageId">Organization unique identifier</param>
        /// <param name="persona">Persona object of the parameter values</param>
        /// <returns>Repository response object</returns>
        public RepositoryResponse CreatePersona(Guid personRealPageId, Guid organizationRealPageId, IPersona persona)
        {
            if (personRealPageId == Guid.Empty)
            {
                throw new Exception("Invalid parameter Person realPageId.");
            }

            if (organizationRealPageId == Guid.Empty)
            {
                throw new Exception("Invalid parameter Organization realPageId.");
            }

            if (persona == null)
            {
                throw new ArgumentNullException(nameof(persona), "Null Persona.");
            }

            return _personaRepository.CreatePersona(personRealPageId, organizationRealPageId, persona);
        }

        /// <summary>
        /// Create a new Persona
        /// </summary>
        /// <param name="userId">User unique identifier</param>
        /// <param name="organizationRealPageId">Organization unique identifier</param>
        /// <param name="createdBy">created by</param>
        /// /// <param name="personaName">Persona Name</param>
        /// <returns>Repository response object</returns>
        public RepositoryResponse CreateAdditionalPersona(Guid organizationRealPageId, long userId, long createdBy, string personaName)
        {
            if (userId == 0)
            {
                throw new Exception("Invalid parameter UserId.");
            }

            if (organizationRealPageId == Guid.Empty)
            {
                throw new Exception("Invalid parameter Organization realPageId.");
            }

            return _personaRepository.CreateAdditionalPersona(organizationRealPageId, userId,createdBy,personaName);
        }

        /// <summary>
        /// Get Persona by Persona Id
        /// </summary>
        /// <param name="personaId">Persona Id</param>
        /// <returns>Persona Object</returns>
        public Persona GetPersona(long personaId)
        {
            if (personaId == 0)
            {
                throw new Exception("Invalid parameter personaId.");
            }
            return _personaRepository.GetPersona(personaId);
        }

        /// <summary>
        /// Get Persona by Persona Id but only include rights if needed
        /// </summary>
        /// <param name="personaId">Persona Id</param>
        /// <param name="withRights">Should the rights also be included</param>
        /// <returns>Persona Object</returns>
        public Persona GetPersonaWithRightsToggle(long personaId, bool withRights = false)
        {
            if (personaId == 0)
            {
                throw new Exception("Invalid parameter personaId.");
            }

            return _personaRepository.GetPersona(personaId, withRights);
        }

        /// <summary>
        /// Lists Personas by Enterprise UserId, does NOT include rights correctly!
        /// </summary>
        /// <param name="realPageId">Person Enterprise Id</param>
        /// <returns>A List of Persona Object(s)</returns>
        public IList<Persona> ListPersona(Guid realPageId)
        {
            if (realPageId == Guid.Empty)
            {
                throw new Exception("Invalid parameter realPageId.");
            }

            return _personaRepository.ListPersona(realPageId);
        }

        /// <summary>
        /// Lists active personas by Enterprise UserId, does NOT include rights correctly!
        /// </summary>
        /// <param name="realPageId">Person Enterprise Id</param>
        /// <param name="includeOrganization">Include organization details</param>
        /// <returns>A List of Persona Object(s)</returns>
        public IList<Persona> ListActivePersona(Guid realPageId, bool includeOrganization)
        {
            if (realPageId == Guid.Empty)
            {
                throw new Exception("Invalid parameter realPageId.");
            }

            return _personaRepository.ListActivePersona(realPageId, includeOrganization);
        }

        public IList<Persona> ListEmployeePersonas(long userId, long orgPartyId)
        {
            if (userId == 0)
            {
                throw new Exception("Invalid parameter userId.");
            }
            if (orgPartyId == 0)
            {
                throw new Exception("Invalid parameter orgPartyId.");
            }

            return _personaRepository.ListEmployeePersonas(userId, orgPartyId);
        }
        /// <summary>
        /// List Persona by Enterprise Organization PartyId
        /// </summary>
        /// <param name="organizationPartyId">Organization Party Id</param>
        /// <param name="IsDefault">Optional Default persona only</param>
        /// <returns>A List of Persona Object(s)</returns>
        public IList<Persona> ListPersonaByOrganizationPartyId(long organizationPartyId, bool? IsDefault = null)
        {
            if (organizationPartyId == 0)
            {
                throw new Exception("Invalid parameter organizationPartyId.");
            }

            return _personaRepository.ListPersonaByOrganizationPartyId(organizationPartyId, IsDefault, null);
        }

        /// <summary>
        /// Get Active Persona by Enterpise UserId
        /// </summary>
        /// <param name="realPageId"></param>
        /// <returns>Persona Id</returns>
        public long GetActivePersonaId(Guid realPageId)
        {
            return _personaRepository.GetActivePersonaId(realPageId);
        }

        /// <summary>
        /// Get current active Persona by Enterprise UserId
        /// </summary>
        /// <param name="realPageId">Person Enterprise Id</param>
        /// <returns>A Persona Object</returns>
        public Persona GetActivePersona(Guid realPageId)
        {
            if (realPageId == Guid.Empty)
            {
                throw new Exception("Invalid parameter realPageId.");
            }
            Persona persona = new Persona();
            long personaId = _personaRepository.GetActivePersonaId(realPageId);

            if (personaId > 0)
            {
                persona = GetPersona(personaId);
            }
            return persona;
        }

        /// <summary>
        /// Get current active Persona by Enterprise UserId, but excluding the rights merging
        /// </summary>
        /// <param name="realPageId">Person Enterprise Id</param>
        /// <returns>A Persona Object</returns>
        public Persona GetActivePersonaWithoutRights(Guid realPageId)
        {
            if (realPageId == Guid.Empty)
            {
                throw new Exception("Invalid parameter realPageId.");
            }
            Persona persona = new Persona();
            long personaId = _personaRepository.GetActivePersonaId(realPageId);

            if (personaId > 0)
            {
                persona = GetPersonaWithRightsToggle(personaId, withRights: false);
            }
            return persona;
        }


        /// <summary>
        /// Get current active Persona by Enterprise UserId and company party id
        /// </summary>
        /// <param name="realPageId">Person Enterprise Id</param>
        /// <param name="orgPartyId">Company party id</param>
        /// <returns>A Persona Object</returns>
        public Persona GetFirstAvailablePersonaByCompany(Guid realPageId, long orgPartyId)
        {
            var personaList = _personaRepository.ListPersona(realPageId);
            Persona persona = null;
            long personaId = 0;

            if (personaList.Count > 0)
            {
                if (personaList.All(p => p.OrganizationPartyId != orgPartyId))
                {
                    if (_userClaim.RealPageEmployee)
                    {
                        orgPartyId = personaList.FirstOrDefault().OrganizationPartyId;
                    }
                }
                else
                {
                    personaId = personaList.FirstOrDefault(p => p.OrganizationPartyId == orgPartyId).PersonaId;
                }
            }

            if (personaId > 0)
            {
                persona = GetPersona(personaId);
            }
            return persona;
        }


        /// <summary>
        /// Used to update the give users active persona id
        /// </summary>
        /// <param name="realPageId"></param>
        /// <param name="personaId"></param>
        /// <returns></returns>
        public RepositoryResponse UpdateActivePersona(Guid realPageId, long personaId)
        {
            return _personaRepository.UpdateActivePersona(realPageId, personaId);
        }

        /// <summary>
        /// Used to generate notification event that the user has changed company
        /// </summary>
        /// <param name="personaId"></param>
        /// <returns></returns>
        public Guid ChangeCompanyNotification(long personaId)
        {
            var guid = Guid.Empty;

            var productInternalSettingList = GetProductInternalSettings(ProductEnum.UnifiedPlatform);
            var notificationsEventChangeCompany = productInternalSettingList.First(a => a.Name.Equals("NotificationsEventChangeCompany", StringComparison.OrdinalIgnoreCase)).Value;
            var notificationsApiEndPoint = productInternalSettingList.First(a => a.Name.Equals("NotificationsApiEndPoint", StringComparison.OrdinalIgnoreCase)).Value;
            var notificationsEventsEndPoint = productInternalSettingList.First(a => a.Name.Equals("NotificationsEventsEndPoint", StringComparison.OrdinalIgnoreCase)).Value;
            var tokenEndpoint = productInternalSettingList.First(a => a.Name.Equals("TokenEndPoint", StringComparison.OrdinalIgnoreCase)).Value;

            var clientId = productInternalSettingList.First(a => a.Name.Equals("UnifiedLoginServerClientName", StringComparison.OrdinalIgnoreCase)).Value;
            var apiSecret = Encoding.UTF8.GetString(Convert.FromBase64String(productInternalSettingList.First(a => a.Name.Equals("UnifiedLoginServerClientSecret", StringComparison.OrdinalIgnoreCase)).Value));

            NotificationEvent nEvent = new NotificationEvent()
            {
                Method = notificationsEventChangeCompany,
                ProductCode = "UL",
                Users = new List<string>() { personaId.ToString() },
                Data = new NotificationEventData() { PersonaId = personaId }
            };

            Dictionary<string, object> logData = new Dictionary<string, object>() { { "notificationsApiEndPoint", notificationsApiEndPoint }, { "nEvent", nEvent } };

            var logger = Log.Logger;
            if (logData?.Keys != null)
                logger = logger.ForContext("AdditionalInfo", JsonConvert.SerializeObject(logData, Formatting.Indented), false);

            logger = logger.ForContext("ProductModule", this.GetType());
            logger.Write(Serilog.Events.LogEventLevel.Debug, "{ActionName} - {state}", propertyValue0: "ChangeCompanyNotification", propertyValue1: "ChangeCompany Event Before...");

            Notification notification = new Notification(clientId, apiSecret, tokenEndpoint, notificationsApiEndPoint + "/v1/notifications", notificationsApiEndPoint + "/" + notificationsEventsEndPoint);
            var result = Task.Run(async () => await notification.SendEvent(nEvent.ProductCode, nEvent.Users.ToList(), nEvent.Method, nEvent.Data)).Result;

            if (!string.IsNullOrWhiteSpace(result.Id))
                guid = new Guid(result.Id);

            logger.Write(Serilog.Events.LogEventLevel.Debug, "{ActionName} - {state}", propertyValue0: "ChangeCompanyNotification", propertyValue1: $"ChangeCompany Event Complete. Guid: {guid}");

            return guid;
        }

        #endregion

        #region Private
        private List<ProductInternalSetting> GetProductInternalSettings(ProductEnum product)
        {
            var rpcache = new RPObjectCache();
            var cacheKey = $"productInternalSetting_{(int)product}";
            var productInternalSettingList = rpcache.GetFromCache(cacheKey, 120, () =>
            {
                // load from database
                
                return _productRepository.GetProductInternalSettings((int)product).ToList();
            });

            return productInternalSettingList;
        }
        
        #endregion
    }
}