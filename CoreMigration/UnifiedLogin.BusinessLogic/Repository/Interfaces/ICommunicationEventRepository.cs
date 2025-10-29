using System;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces
{
    /// <summary>
    /// CommunicationEvent Repository
    /// </summary>
    public interface ICommunicationEventRepository
    {
        /// <summary>
        /// Create a communication event purpose type
        /// </summary>
        RepositoryResponse CreateCommunicationEventPurposeType(string description);

        /// <summary>
        /// Create a communication event
        /// </summary>
        RepositoryResponse CreateCommunicationEvent(int statusTypeId, long fromPartyContactMechanismId, long toPartyContactMechanismId, DateTime started, DateTime ended, string note);

        /// <summary>
        /// Create a communication event for email
        /// </summary>
        RepositoryResponse CreateCommunicationEventEmail(int communicationEmailTemplateId, long communicationEventId);
        /// <summary>
        /// Create a CES communication event for email
        /// </summary>
        RepositoryResponse CreateCESCommunicationEventEmail(string cesId, long communicationEventId);

    }
}