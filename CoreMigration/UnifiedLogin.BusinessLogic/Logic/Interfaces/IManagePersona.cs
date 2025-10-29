using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Landing;
using System;
using System.Collections.Generic;

namespace UnifiedLogin.BusinessLogic.Logic.Interfaces
{
    /// <summary>
    /// Interface for Manage Persona repository calls
    /// </summary>
    public interface IManagePersona
    {
        /// <summary>
        /// Create a new Persona
        /// </summary>
        /// <param name="personRealPageId">User unique identifier</param>
        /// <param name="organizationRealPageId">Organization unique identifier</param>
        /// <param name="persona">Persona object of the parameter values</param>
        /// <returns>Repository response object</returns>
        RepositoryResponse CreatePersona(Guid personRealPageId, Guid organizationRealPageId, IPersona persona);

        /// <summary>
        /// Get Persona by Persona Id
        /// </summary>
        /// <param name="personaId">Persona Id</param>
        /// <returns>Persona Object</returns>
        Persona GetPersona(long personaId);

        /// <summary>
        /// Get Persona by Persona Id but only include rights if needed
        /// </summary>
        /// <param name="personaId">Persona Id</param>
        /// <param name="withRights">Should the rights also be included</param>
        /// <returns>Persona Object</returns>
        Persona GetPersonaWithRightsToggle(long personaId, bool withRights = false);

        /// <summary>
        /// List Persona by Enterprise UserId
        /// </summary>
        /// <param name="realPageId">Person Enterprise Id</param>
        /// <returns>A List of Persona Object(s)</returns>
        IList<Persona> ListPersona(Guid realPageId);

        /// <summary>
        /// Lists active personas by Enterprise UserId, does NOT include rights correctly!
        /// </summary>
        /// <param name="realPageId">Person Enterprise Id</param>
        /// <param name="includeOrganization">Include organization details</param>
        /// <returns>A List of Persona Object(s)</returns>
        IList<Persona> ListActivePersona(Guid realPageId, bool includeOrganization);

        /// <summary>
        /// List Employee Persona by Enterprise UserId
        /// </summary>
        /// <param name="userId">Person Enterprise Id</param>
        /// <param name="orgPartyId">org oarty id</param>
        /// <returns>A List of Persona Object(s)</returns>
        IList<Persona> ListEmployeePersonas(long userId, long orgPartyId);

        /// <summary>
        /// List Persona by Enterprise Organization PartyId
        /// </summary>
        /// <param name="organizationPartyId">Organization Party Id</param>
        /// <param name="IsDefault">Optional Default persona only</param>
        /// <returns>A List of Persona Object(s)</returns>
        IList<Persona> ListPersonaByOrganizationPartyId(long organizationPartyId, bool? IsDefault);

        /// <summary>
        /// Get current active Persona by Enterprise UserId
        /// </summary>
        /// <param name="realPageId">Person Enterprise Id</param>
        /// <returns>A Persona Object</returns>
        long GetActivePersonaId(Guid realPageId);

        /// <summary>
        /// Get current active Persona by Enterprise UserId
        /// </summary>
        /// <param name="realPageId">Person Enterprise Id</param>
        /// <returns>A Persona Object</returns>
        Persona GetActivePersona(Guid realPageId);

        /// <summary>
        /// Get current active Persona by Enterprise UserId, but excluding the rights merging
        /// </summary>
        /// <param name="realPageId">Person Enterprise Id</param>
        /// <returns>A Persona Object</returns>
        Persona GetActivePersonaWithoutRights(Guid realPageId);

        /// <summary>
        /// Get Persona Environment Type
        /// </summary>
        /// <returns>Persona Environment Type Object</returns>
        IList<PersonaEnvironment> GetPersonaEnvironmentType();

        /// <summary>
        /// Used to update the give users active persona id
        /// </summary>
        /// <param name="realPageId"></param>
        /// <param name="personaId"></param>
        /// <returns></returns>
        RepositoryResponse UpdateActivePersona(Guid realPageId, long personaId);

        /// <summary>
        /// Get current active Persona by Enterprise UserId and company party id
        /// </summary>
        /// <param name="realPageId">Person Enterprise Id</param>
        /// <param name="orgPartyId">Company party id</param>
        /// <returns>A Persona Object</returns>
        Persona GetFirstAvailablePersonaByCompany(Guid realPageId, long orgPartyId);

        Guid ChangeCompanyNotification(long personaId);
        /// <summary>
		/// Create a Secondary Persona
		/// </summary>
		/// <param name="userId">User unique identifier</param>
		/// <param name="organizationRealPageId">Organization unique identifier</param>
		/// <param name="createdBy">createdBy</param>
        /// <param name="personaName">Persona Name</param>
		/// <returns>Repository response object</returns>
		RepositoryResponse CreateAdditionalPersona(Guid organizationRealPageId, long userId, long createdBy, string personaName);
    }
}