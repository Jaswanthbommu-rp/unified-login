using System;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces
{
    /// <summary>
    /// CommunicationEvent Repository
    /// </summary>
    public interface ICommunicationEventRepositoryAsync
    {
        /// <summary>
        /// Create a communication event purpose type
        /// </summary>
        Task<RepositoryResponse> CreateCommunicationEventPurposeTypeAsync(string description, CancellationToken cancellationToken = default);

        /// <summary>
        /// Create a communication event
        /// </summary>
        Task<RepositoryResponse> CreateCommunicationEventAsync(int statusTypeId, long fromPartyContactMechanismId, long toPartyContactMechanismId, DateTime started, DateTime ended, string note, CancellationToken cancellationToken = default);

        /// <summary>
        /// Create a communication event for email
        /// </summary>
        Task<RepositoryResponse> CreateCommunicationEventEmailAsync(int communicationEmailTemplateId, long communicationEventId, CancellationToken cancellationToken = default);
        /// <summary>
        /// Create a CES communication event for email
        /// </summary>
        Task<RepositoryResponse> CreateCESCommunicationEventEmailAsync(string cesId, long communicationEventId, CancellationToken cancellationToken = default);
    }
}