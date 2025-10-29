using System;
using System.Collections.Generic;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.DapperMappingGuides;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces
{
	/// <summary>
	/// Interface for Persona Repository
	/// </summary>
	public interface IPersonaRepository
	{

		/// <summary>
		/// Get Persona Environment Type
		/// </summary>
		/// <returns>Repository response object</returns>
		IList<PersonaEnvironment> GetPersonaEnvironmentType();

		/// <summary>
		/// Create a new Persona
		/// </summary>
		/// <param name="personRealPageId">User unique identifier</param>
		/// <param name="organizationRealPageId">Organization unique identifier</param>
		/// <param name="persona">Persona object of the parameter values</param>
		/// <returns>Repository response object</returns>
		RepositoryResponse CreatePersona(Guid personRealPageId, Guid organizationRealPageId, IPersona persona);

        /// <summary>
        /// Get Default Persona by Persona Id
        /// </summary>
        /// <param name="personaId">Persona Id</param>
        /// <param name="withRights">Also merge persona with current user rights</param>
        /// <returns>Persona Object</returns>
        Persona GetPersona(long personaId, bool withRights = true);

        /// <summary>
        /// List Persona by Enterprise UserId
        /// </summary>
        /// <param name="realPageId">Person Enterprise Id</param>
        /// <returns>A List of Persona Object(s)</returns>
        IList<Persona> ListPersona(Guid realPageId);

        /// <summary>
        /// Lists only active personas by Enterprise UserId, does NOT include rights correctly!
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
        /// <param name="isDefault">Optional Default persona only</param>
        /// <param name="userRoleType">User Role type (e.g. refer to UserRoleType Enum)</param>
        /// <returns>A List of Persona Object(s)</returns>
        IList<Persona> ListPersonaByOrganizationPartyId(long organizationPartyId, bool? isDefault = null, int? userRoleType = null);

		/// <summary>
		/// Get current active Persona by Enterprise UserId
		/// </summary>
		/// <param name="realPageId">Person Enterprise Id</param>
		/// <returns>A Persona Object</returns>
		long GetActivePersonaId(Guid realPageId);

        /// <summary>
        /// Used to update the active persona for a user
        /// </summary>
        /// <param name="personRealPageId"></param>
        /// <param name="personaId"></param>
        /// <returns></returns>
        RepositoryResponse UpdateActivePersona(Guid personRealPageId, long personaId);
        /// <summary>
        /// Used to get persona products settings
        /// </summary>
        /// <param name="personaId"></param>
        /// <returns></returns>
        List<ProductSettingList> GetPersonaProductSettings(long personaId);
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