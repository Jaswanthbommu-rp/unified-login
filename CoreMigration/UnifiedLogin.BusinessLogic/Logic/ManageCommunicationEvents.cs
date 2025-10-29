using System;
using UnifiedLogin.DataAccess;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Logic
{
	/// <summary>
	/// 
	/// </summary>
	public class ManageCommunicationEvents : IManageCommunicationEvents
    {

        ICommunicationEventRepository _communicationEventRepository;
        
        #region Ctor

        /// <summary>
        /// 
        /// </summary> 
        public ManageCommunicationEvents(ICommunicationEventRepository communicationEventRepository)
        {
            _communicationEventRepository = communicationEventRepository;
        }

        /// <summary>
        /// Unit test constructor
        /// </summary>
        /// <param name="repository"></param>
        public ManageCommunicationEvents(IRepository repository)
        {
            _communicationEventRepository = new CommunicationEventRepository(repository);
        }
        
        /// <summary>
        /// 
        /// </summary>
        public ManageCommunicationEvents()
        {
            _communicationEventRepository = new CommunicationEventRepository();
        }
        #endregion

        #region Communication Events
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
        public RepositoryResponse CreateCommunicationEvent(int statusTypeId, long fromPartyContactMechanismId, long toPartyContactMechanismId, DateTime started, DateTime ended, string note)
        {
            if (statusTypeId == 0)
                throw new ArgumentNullException(nameof(statusTypeId));
            if (fromPartyContactMechanismId == 0)
                throw new ArgumentNullException(nameof(fromPartyContactMechanismId));
            if (toPartyContactMechanismId == 0)
                throw new ArgumentNullException(nameof(toPartyContactMechanismId));

            return _communicationEventRepository.CreateCommunicationEvent(statusTypeId, fromPartyContactMechanismId, toPartyContactMechanismId, started, ended, note);
        }
        /// <summary>
        /// Create Communication Email Event
        /// </summary>
        /// <param name="communicationEmailTemplateId"></param>
        /// <param name="communicationEventId"></param>
        /// <returns></returns>
        public RepositoryResponse CreateCommunicationEventEmail(int communicationEmailTemplateId, long communicationEventId)
        {
            if (communicationEmailTemplateId == 0)
            {
                throw new ArgumentNullException(nameof(communicationEmailTemplateId));
            }
            if (communicationEventId == 0)
            {
                throw new ArgumentNullException(nameof(communicationEmailTemplateId));
            }
            return _communicationEventRepository.CreateCommunicationEventEmail(communicationEmailTemplateId, communicationEventId);
        }

        /// <summary>
        /// Create CES Communication Email Event
        /// </summary>
        /// <param name="cesId"></param>
        /// <param name="communicationEventId"></param>
        /// <returns></returns>
        public RepositoryResponse CreateCESCommunicationEventEmail(string cesId, long communicationEventId)
        {
            if (string.IsNullOrEmpty(cesId))
            {
                throw new ArgumentNullException(nameof(cesId));
            }
            if (communicationEventId == 0)
            {
                throw new ArgumentNullException(nameof(communicationEventId));
            }
            return _communicationEventRepository.CreateCESCommunicationEventEmail(cesId, communicationEventId);
        }
        #endregion
    }
}

