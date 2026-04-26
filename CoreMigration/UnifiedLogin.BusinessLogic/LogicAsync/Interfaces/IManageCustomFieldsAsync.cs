using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Native-async interface for custom field business operations.
/// Replaces <see cref="Logic.Interfaces.IManageCustomFields"/>.
/// <para>
/// Breaking change from the sync interface: <c>IDictionary&lt;object,object&gt; globals</c>
/// is removed — callers pass <see cref="RequestParameter"/> directly.
/// </para>
/// </summary>
public interface IManageCustomFieldsAsync
{
    /// <summary>
    /// Adds or updates custom field values for a user.
    /// Validates JSON shape before calling the repository.
    /// </summary>
    Task<RepositoryResponse> AddUpdateFieldValueAsync(
        string            customFieldsValuesJson,
        long              createdBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns custom fields for the given organisation party.
    /// </summary>
    Task<IList<CustomField>> GetCustomFieldAsync(
        long              partyId,
        RequestParameter? dataFilter        = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns custom field values for a user, optionally filtered by persona and enabled state.
    /// </summary>
    Task<IList<CustomFieldValue>> GetCustomFieldsValuesAsync(
        long              organizationPartyId,
        long?             userLoginPersonaId = null,
        bool?             enabled            = null,
        CancellationToken cancellationToken  = default);
}
