using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Helper;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Async-first custom fields service.
/// <para>
/// Replaces <c>ManageCustomFields</c> which:
/// <list type="bullet">
///   <item>Instantiated <c>new CustomFieldsRepository()</c> inline in a parameterless constructor.</item>
///   <item>Injected <c>DefaultUserClaim</c> to extract <c>CorrelationId</c> solely for a
///         hand-rolled <c>WriteToLog</c> helper using the static <c>Serilog.Log.Logger</c>.</item>
///   <item>Accepted <c>IDictionary&lt;object,object&gt; globals</c> in every query method —
///         a bag object used only to extract one <c>RequestParameter</c> value.</item>
///   <item>Contained a completely dead overload (<c>GetCustomField(globals, booksCustomerMasterId,
///         bookMasterTypeId)</c>) whose repository call was commented out.</item>
///   <item>Threw <c>new Exception(...)</c> for guard failures.</item>
/// </list>
/// </para>
/// <para>
/// Key improvements:
/// <list type="bullet">
///   <item>Single DI constructor — no inline instantiation.</item>
///   <item><c>DefaultUserClaim</c> replaced by <see cref="IUserClaimsAccessor"/> —
///         <c>CorrelationId</c> read from <c>_userClaims.CorrelationId</c>.</item>
///   <item><c>IDictionary&lt;object,object&gt; globals</c> removed — callers pass
///         <see cref="RequestParameter"/> directly.</item>
///   <item>Dead <c>GetCustomField(booksCustomerMasterId, bookMasterTypeId)</c> overload removed.</item>
///   <item><c>Serilog.Log.Logger</c> replaced by injected <see cref="ILogger{T}"/>.</item>
///   <item>Guard clauses use <see cref="ArgumentOutOfRangeException.ThrowIfZero"/> and
///         <see cref="ArgumentException.ThrowIfNullOrWhiteSpace"/>.</item>
///   <item>All calls are fully async with <c>ConfigureAwait(false)</c>.</item>
/// </list>
/// </para>
/// <para><b>DI registration:</b> <c>Scoped</c>.</para>
/// </summary>
public sealed class ManageCustomFieldsAsync : IManageCustomFieldsAsync
{
    private readonly ICustomFieldsRepositoryAsync   _repository;
    private readonly IUserClaimsAccessor            _userClaims;
    private readonly ILogger<ManageCustomFieldsAsync> _logger;

    public ManageCustomFieldsAsync(
        ICustomFieldsRepositoryAsync    repository,
        IUserClaimsAccessor             userClaims,
        ILogger<ManageCustomFieldsAsync> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _userClaims = userClaims ?? throw new ArgumentNullException(nameof(userClaims));
        _logger     = logger     ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> AddUpdateFieldValueAsync(
        string            customFieldsValuesJson,
        long              createdBy,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(customFieldsValuesJson);
        ArgumentOutOfRangeException.ThrowIfZero(createdBy);

        if (!ValidateJson.IsValidJson<IList<CustomFieldValue>>(customFieldsValuesJson))
            throw new ArgumentException("Invalid user Custom Fields Json.", nameof(customFieldsValuesJson));

        _logger.LogDebug("{CorrelationId} AddUpdateFieldValueAsync begin — createdBy={CreatedBy}",
            _userClaims.CorrelationId, createdBy);

        var response = await _repository
            .AddUpdateFieldValueAsync(customFieldsValuesJson, createdBy, cancellationToken)
            .ConfigureAwait(false);

        _logger.LogDebug("{CorrelationId} AddUpdateFieldValueAsync end — id={Id}",
            _userClaims.CorrelationId, response.Id);

        return response;
    }

    /// <inheritdoc/>
    public async Task<IList<CustomField>> GetCustomFieldAsync(
        long              partyId,
        RequestParameter? dataFilter        = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfZero(partyId);

        _logger.LogDebug("{CorrelationId} GetCustomFieldAsync begin — partyId={PartyId}",
            _userClaims.CorrelationId, partyId);

        var result = await _repository
            .GetCustomFieldAsync(partyId, dataFilter, cancellationToken)
            .ConfigureAwait(false);

        _logger.LogDebug("{CorrelationId} GetCustomFieldAsync end — count={Count}",
            _userClaims.CorrelationId, result.Count);

        return result;
    }

    /// <inheritdoc/>
    public async Task<IList<CustomFieldValue>> GetCustomFieldsValuesAsync(
        long              organizationPartyId,
        long?             userLoginPersonaId = null,
        bool?             enabled            = null,
        CancellationToken cancellationToken  = default)
    {
        ArgumentOutOfRangeException.ThrowIfZero(organizationPartyId);

        // Normalise: treat <= 0 as "no filter" (preserved from legacy logic)
        if (userLoginPersonaId <= 0) userLoginPersonaId = null;

        _logger.LogDebug(
            "{CorrelationId} GetCustomFieldsValuesAsync begin — orgPartyId={OrgPartyId} personaId={PersonaId}",
            _userClaims.CorrelationId, organizationPartyId, userLoginPersonaId);

        var result = await _repository
            .GetCustomFieldsValuesAsync(organizationPartyId, userLoginPersonaId, enabled, cancellationToken)
            .ConfigureAwait(false);

        _logger.LogDebug("{CorrelationId} GetCustomFieldsValuesAsync end — count={Count}",
            _userClaims.CorrelationId, result.Count);

        return result;
    }
}
