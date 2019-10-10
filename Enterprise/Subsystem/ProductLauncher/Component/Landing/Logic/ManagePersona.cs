using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Linq;
using RP.Enterprise.Foundation.DataAccess.Component;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic
{
    /// <summary>
    /// Manage Persona
    /// </summary>
    public class ManagePersona : IManagePersona
    {
        #region Private Variables
        IPersonaRepository _personaRepository;
        IManageOrganization _manageOrganization;
        private DefaultUserClaim _userClaim;
        #endregion

        #region Constructors
        /// <summary>
        /// ManagePersona Constructor
        /// </summary>
        /// <param name="personaRepository"></param>
        /// <param name="manageOrganization"></param>
        public ManagePersona(IPersonaRepository personaRepository, IManageOrganization manageOrganization)
        {
            _personaRepository = personaRepository;
            _manageOrganization = manageOrganization;
        }

        /// <summary>
        /// ManagePersona Constructor
        /// </summary>
        /// <param name="userClaim"></param>
        /// <param name="personaRepository"></param>
        /// <param name="manageOrganization"></param>
        public ManagePersona(DefaultUserClaim userClaim, IPersonaRepository personaRepository, IManageOrganization manageOrganization)
        {
            _personaRepository = personaRepository;
            _manageOrganization = manageOrganization;
            _userClaim = userClaim;
        }

        /// <summary>
        /// Create a basic instance of the ManagePerson Controller class
        /// </summary>
        public ManagePersona()
        {
            _personaRepository = new PersonaRepository();
            _manageOrganization = new ManageOrganization();
        }

        /// <summary>
        /// Create a basic instance of the ManagePerson Controller class
        /// </summary>
        public ManagePersona(IRepository repository)
        {
            _personaRepository = new PersonaRepository(repository);
            _manageOrganization = new ManageOrganization(repository);
        }

        /// <summary>
        /// Create a basic instance of the ManagePerson Controller class
        /// </summary>
        public ManagePersona(DefaultUserClaim userClaim)
        {
            _personaRepository = new PersonaRepository();
            _manageOrganization = new ManageOrganization();
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

                personaId = personaList.FirstOrDefault(p => p.OrganizationPartyId == orgPartyId).PersonaId;
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
        #endregion
    }
}