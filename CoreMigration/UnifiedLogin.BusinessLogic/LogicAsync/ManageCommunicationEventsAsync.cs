using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Async-first communication event service.
/// <para>
/// Replaces <c>ManageCommunicationEvents</c> which used three overloaded constructors
/// (including a bare <c>new()</c> that instantiated <c>CommunicationEventRepository</c>
/// directly) and synchronous repository calls.
/// </para>
/// <para>
/// Key improvements:
/// <list type="bullet">
///   <item>Single DI constructor — no <c>new</c> repository instantiation.</item>
///   <item>Guard clauses use <see cref="ArgumentOutOfRangeException.ThrowIfZero"/> and
///         <see cref="ArgumentException.ThrowIfNullOrWhiteSpace"/> — semantically correct
///         replacements for the misuse of <c>ArgumentNullException</c> on value types.</item>
///   <item>All calls are fully async with <c>ConfigureAwait(false)</c>.</item>
///   <item>Bug fix: <c>CreateCommunicationEventEmail</c> threw using
///         <c>nameof(communicationEmailTemplateId)</c> for the second guard — now uses
///         <c>nameof(communicationEventId)</c>.</item>
/// </list>
/// </para>
/// <para><b>DI registration:</b> <c>Scoped</c>.</para>
/// </summary>
public sealed class ManageCommunicationEventsAsync : IManageCommunicationEventsAsync
{
    private readonly ICommunicationEventRepositoryAsync      _repository;
    private readonly ILogger<ManageCommunicationEventsAsync> _logger;

    public ManageCommunicationEventsAsync(
        ICommunicationEventRepositoryAsync      repository,
        ILogger<ManageCommunicationEventsAsync> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger     = logger     ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> CreateCommunicationEventAsync(
        int      statusTypeId,
        long     fromPartyContactMechanismId,
        long     toPartyContactMechanismId,
        DateTime started,
        DateTime ended,
        string   note,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfZero(statusTypeId);
        ArgumentOutOfRangeException.ThrowIfZero(fromPartyContactMechanismId);
        ArgumentOutOfRangeException.ThrowIfZero(toPartyContactMechanismId);

        return await _repository.CreateCommunicationEventAsync(
            statusTypeId, fromPartyContactMechanismId, toPartyContactMechanismId,
            started, ended, note, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> CreateCommunicationEventEmailAsync(
        int  communicationEmailTemplateId,
        long communicationEventId,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfZero(communicationEmailTemplateId);
        ArgumentOutOfRangeException.ThrowIfZero(communicationEventId);

        return await _repository.CreateCommunicationEventEmailAsync(
            communicationEmailTemplateId, communicationEventId, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> CreateCESCommunicationEventEmailAsync(
        string cesId,
        long   communicationEventId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cesId);
        ArgumentOutOfRangeException.ThrowIfZero(communicationEventId);

        return await _repository.CreateCESCommunicationEventEmailAsync(
            cesId, communicationEventId, cancellationToken).ConfigureAwait(false);
    }
}
