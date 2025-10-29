using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Logic.Interfaces
{
    public interface IManageCommunicationEvents
    {
        /// <summary>
        /// Create Communication Event
        /// </summary>
        /// <param name="statusTypeId">Status Type Id</param>
        /// <param name="fromPartyContactMechanismId">From PartyContactMechanism Id</param>
        /// <param name="toPartyContactMechanismId">To PartyContactMechanism Id</param>
        /// <param name="started">Started</param>
        /// <param name="ended">Ended</param>
        /// <param name="note">Note</param>
        /// <returns></returns>
        RepositoryResponse CreateCommunicationEvent(int statusTypeId, long fromPartyContactMechanismId, long toPartyContactMechanismId, DateTime started, DateTime ended, string note);
        
        /// <summary>
        /// Create Communication Email Event
        /// </summary>
        /// <param name="communicationEmailTemplateId"></param>
        /// <param name="communicationEventId"></param>
        /// <returns></returns>
        RepositoryResponse CreateCommunicationEventEmail(int communicationEmailTemplateId, long communicationEventId);

        /// <summary>
        /// Create CES Communication Email Event
        /// </summary>
        /// <param name="cesId"></param>
        /// <param name="communicationEventId"></param>
        /// <returns></returns>
        RepositoryResponse CreateCESCommunicationEventEmail(string cesId, long communicationEventId);
    }
}