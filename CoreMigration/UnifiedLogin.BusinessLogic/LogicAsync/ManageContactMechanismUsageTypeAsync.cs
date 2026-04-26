using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.IdentityConfig;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Async-first contact mechanism usage type service.
/// <para>
/// Replaces <c>ManageContactMechanismUsageType</c>, which had two constructors —
/// including a parameterless one that called <c>new ContactMechanismUsageTypeRepository()</c>
/// directly — and a single synchronous delegation method with no cancellation support.
/// </para>
/// <para>
/// Key improvements:
/// <list type="bullet">
///   <item>Single DI constructor — no <c>new</c> repository instantiation.</item>
///   <item>All calls are fully async with <c>ConfigureAwait(false)</c>.</item>
///   <item><c>CancellationToken</c> threaded through to the repository layer.</item>
///   <item>Return contract is <c>IList&lt;T&gt;</c> (never <c>null</c>) — the repository
///         guarantees an empty list on error.</item>
/// </list>
/// </para>
/// <para><b>DI registration:</b> <c>Scoped</c>.</para>
/// </summary>
public sealed class ManageContactMechanismUsageTypeAsync : IManageContactMechanismUsageTypeAsync
{
    private readonly IContactMechanismUsageTypeRepositoryAsync       _repository;
    private readonly ILogger<ManageContactMechanismUsageTypeAsync>   _logger;

    public ManageContactMechanismUsageTypeAsync(
        IContactMechanismUsageTypeRepositoryAsync      repository,
        ILogger<ManageContactMechanismUsageTypeAsync>  logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger     = logger     ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<IList<ContactMechanismUsageType>> ListContactMechanismUsageTypeAsync(
        string?           contactMechanismUsageTypeName,
        CancellationToken cancellationToken = default)
    {
        return await _repository
            .ListContactMechanismUsageTypeAsync(contactMechanismUsageTypeName, cancellationToken)
            .ConfigureAwait(false);
    }
}
