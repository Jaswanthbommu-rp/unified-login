using System;
using System.Collections.Generic;
using System.Text;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces
{
    public interface IPersonaRepositoryAsync
    {
        /// <summary>
        /// Get Persona Environment Type asynchronously.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of PersonaEnvironment objects</returns>
        Task<IList<PersonaEnvironment>> GetPersonaEnvironmentTypeAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Create a new Persona asynchronously.
        /// </summary>
        /// <param name="personRealPageId">User unique identifier</param>
        /// <param name="organizationRealPageId">Organization unique identifier</param>
        /// <param name="persona">Persona object of the parameter values</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Repository response object</returns>
        Task<RepositoryResponse> CreatePersonaAsync(
            Guid personRealPageId,
            Guid organizationRealPageId,
            IPersona persona,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get Default Persona by Persona Id asynchronously.
        /// </summary>
        /// <param name="personaId">Persona Id</param>
        /// <param name="withRights">Also merge persona with current user rights</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Persona Object</returns>
        Task<Persona> GetPersonaAsync(
            long personaId,
            bool withRights = true,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// List Persona by Enterprise UserId asynchronously.
        /// </summary>
        /// <param name="realPageId">Person Enterprise Id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A List of Persona Object(s)</returns>
        Task<IList<Persona>> ListPersonaAsync(
            Guid realPageId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists only active personas by Enterprise UserId asynchronously,
        /// does NOT include rights correctly!
        /// </summary>
        /// <param name="realPageId">Person Enterprise Id</param>
        /// <param name="includeOrganization">Include organization details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A List of Persona Object(s)</returns>
        Task<IList<Persona>> ListActivePersonaAsync(
            Guid realPageId,
            bool includeOrganization,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// List Employee Personas by Enterprise UserId asynchronously.
        /// </summary>
        /// <param name="userId">Person Enterprise Id</param>
        /// <param name="orgPartyId">Org party id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A List of Persona Object(s)</returns>
        Task<IList<Persona>> ListEmployeePersonasAsync(
            long userId,
            long orgPartyId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// List Persona by Enterprise Organization PartyId asynchronously.
        /// </summary>
        /// <param name="organizationPartyId">Organization Party Id</param>
        /// <param name="isDefault">Optional Default persona only</param>
        /// <param name="userRoleType">User Role type (e.g. refer to UserRoleType Enum)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A List of Persona Object(s)</returns>
        Task<IList<Persona>> ListPersonaByOrganizationPartyIdAsync(
            long organizationPartyId,
            bool? isDefault = null,
            int? userRoleType = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get current active PersonaId by Enterprise UserId asynchronously.
        /// </summary>
        /// <param name="realPageId">Person Enterprise Id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Active PersonaId</returns>
        Task<long> GetActivePersonaIdAsync(
            Guid realPageId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update the active persona for a user asynchronously.
        /// </summary>
        /// <param name="personRealPageId">Person real page identifier</param>
        /// <param name="personaId">Target persona identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Repository response object</returns>
        Task<RepositoryResponse> UpdateActivePersonaAsync(
            Guid personRealPageId,
            long personaId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get persona product settings asynchronously.
        /// </summary>
        /// <param name="personaId">Persona identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of product setting items</returns>
        Task<List<ProductSettingList>> GetPersonaProductSettingsAsync(
            long personaId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Create a Secondary Persona asynchronously.
        /// </summary>
        /// <param name="organizationRealPageId">Organization unique identifier</param>
        /// <param name="userId">User unique identifier</param>
        /// <param name="createdBy">Creator user identifier</param>
        /// <param name="personaName">Persona Name</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Repository response object</returns>
        Task<RepositoryResponse> CreateAdditionalPersonaAsync(
            Guid organizationRealPageId,
            long userId,
            long createdBy,
            string personaName,
            CancellationToken cancellationToken = default);
    }
}
