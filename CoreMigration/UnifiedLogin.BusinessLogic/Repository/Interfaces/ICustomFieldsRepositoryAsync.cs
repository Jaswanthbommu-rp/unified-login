using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces;

/// <summary>
/// Async repository interface for custom field operations.
/// </summary>
public interface ICustomFieldsRepositoryAsync
{
    /// <summary>Adds or updates custom field values for a user within a transaction.</summary>
    Task<RepositoryResponse> AddUpdateFieldValueAsync(
        string            customFieldsValuesJson,
        long              createdBy,
        CancellationToken cancellationToken = default);

    /// <summary>Returns custom fields wrapped as a JSON-valued <see cref="Setting"/> list.</summary>
    Task<IList<Setting>> GetCustomFieldsAsync(
        long              partyId,
        RequestParameter? dataFilterSort    = null,
        CancellationToken cancellationToken = default);

    /// <summary>Returns raw custom field records for an organisation party.</summary>
    Task<IList<CustomField>> GetCustomFieldAsync(
        long              partyId,
        RequestParameter? dataFilterSort    = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns custom field values for a user.
    /// Returns an empty list when no records are found.
    /// </summary>
    Task<IList<CustomFieldValue>> GetCustomFieldsValuesAsync(
        long              organizationPartyId,
        long?             userLoginPersonaId = null,
        bool?             enabled            = null,
        CancellationToken cancellationToken  = default);
}
