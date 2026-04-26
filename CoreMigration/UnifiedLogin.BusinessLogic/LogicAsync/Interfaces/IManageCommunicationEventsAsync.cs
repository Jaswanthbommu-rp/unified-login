using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Native-async interface for communication event business operations.
/// Replaces <see cref="Logic.Interfaces.IManageCommunicationEvents"/>.
/// All per-call validation is enforced before delegating to the repository.
/// </summary>
public interface IManageCommunicationEventsAsync
{
    /// <summary>Creates a communication event record.</summary>
    Task<RepositoryResponse> CreateCommunicationEventAsync(
        int      statusTypeId,
        long     fromPartyContactMechanismId,
        long     toPartyContactMechanismId,
        DateTime started,
        DateTime ended,
        string   note,
        CancellationToken cancellationToken = default);

    /// <summary>Associates an email template with an existing communication event.</summary>
    Task<RepositoryResponse> CreateCommunicationEventEmailAsync(
        int  communicationEmailTemplateId,
        long communicationEventId,
        CancellationToken cancellationToken = default);

    /// <summary>Associates a CES email with an existing communication event.</summary>
    Task<RepositoryResponse> CreateCESCommunicationEventEmailAsync(
        string cesId,
        long   communicationEventId,
        CancellationToken cancellationToken = default);
}
