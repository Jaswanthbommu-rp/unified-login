using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async-first interface for all persona orchestration.
/// Replaces: sync <see cref="IManagePersona"/> + blocking <c>.Result</c> calls.
/// </summary>
public interface IManagePersonaAsync
{
    /// <summary>Get all persona environment types.</summary>
    Task<IList<PersonaEnvironment>> GetPersonaEnvironmentTypeAsync(CancellationToken cancellationToken = default);

    /// <summary>Create a new persona for an existing person + org pair.</summary>
    Task<RepositoryResponse> CreatePersonaAsync(
        Guid personRealPageId, Guid organizationRealPageId, IPersona persona,
        CancellationToken cancellationToken = default);

    /// <summary>Create a named secondary persona for a user within an organisation.</summary>
    Task<RepositoryResponse> CreateAdditionalPersonaAsync(
        Guid organizationRealPageId, long userId, long createdBy, string personaName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a persona by its ID.
    /// <paramref name="withRights"/> controls whether role/right merging is performed.
    /// </summary>
    Task<Persona> GetPersonaAsync(long personaId, bool withRights = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// List all personas for a user.
    /// Does <b>not</b> include rights correctly — use <see cref="GetPersonaAsync"/> when rights are needed.
    /// </summary>
    Task<IList<Persona>> ListPersonaAsync(Guid realPageId, CancellationToken cancellationToken = default);

    /// <summary>List only active personas; optionally include organisation details.</summary>
    Task<IList<Persona>> ListActivePersonaAsync(
        Guid realPageId, bool includeOrganization, CancellationToken cancellationToken = default);

    /// <summary>List all personas for an employee within a specific organisation.</summary>
    Task<IList<Persona>> ListEmployeePersonasAsync(
        long userId, long orgPartyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// List personas scoped to an organisation party.
    /// <paramref name="isDefault"/> filters to the default persona only when set.
    /// <paramref name="userRoleType"/> filters by role type when set.
    /// </summary>
    Task<IList<Persona>> ListPersonaByOrganizationPartyIdAsync(
        long organizationPartyId, bool? isDefault = null, int? userRoleType = null,
        CancellationToken cancellationToken = default);

    /// <summary>Return only the active persona ID for a user (no rights merging).</summary>
    Task<long> GetActivePersonaIdAsync(Guid realPageId, CancellationToken cancellationToken = default);

    /// <summary>Return the full active persona including merged rights.</summary>
    Task<Persona> GetActivePersonaAsync(Guid realPageId, CancellationToken cancellationToken = default);

    /// <summary>Return the active persona without rights merging (lighter read).</summary>
    Task<Persona> GetActivePersonaWithoutRightsAsync(Guid realPageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Return the first available persona for a user within a specific company.
    /// Falls back to the first persona in the list when the requesting user is a
    /// RealPage Employee and no persona is found for <paramref name="orgPartyId"/>.
    /// </summary>
    Task<Persona?> GetFirstAvailablePersonaByCompanyAsync(
        Guid realPageId, long orgPartyId, CancellationToken cancellationToken = default);

    /// <summary>Switch the user's active persona to the specified <paramref name="personaId"/>.</summary>
    Task<RepositoryResponse> UpdateActivePersonaAsync(
        Guid realPageId, long personaId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fire a <c>ChangeCompany</c> notification event for the given persona.
    /// Returns the notification GUID, or <see cref="Guid.Empty"/> on failure.
    /// </summary>
    Task<Guid> ChangeCompanyNotificationAsync(long personaId, CancellationToken cancellationToken = default);
}