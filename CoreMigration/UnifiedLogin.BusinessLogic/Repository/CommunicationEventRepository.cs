using System;
using UnifiedLogin.DataAccess;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository
{
	/// <summary>
	/// Communication Event Repository
	/// </summary>
	public class CommunicationEventRepository : BaseRepository, ICommunicationEventRepository
    {
        #region Ctor

        /// <summary>
        /// Communication Event Base Constructor
        /// </summary>
        public CommunicationEventRepository() : base(DbConnectionEnum.IdpConfigurationDb)
        {
        }

        public CommunicationEventRepository(IRepository repository) : base(repository)
        {
        }
        #endregion

        /// <summary>
        /// Create a communication event purpose type
        /// </summary>
        /// <param name="description">Purpose Type Description</param>
        /// <returns>RepositoryResponse object</returns>
        public RepositoryResponse CreateCommunicationEventPurposeType(string description)
        {
            using (var repository = GetRepository())
            {
                return repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateCommunicationEventPurposeType, new { description });
            }
        }

        /// <summary>
        /// Create a communication event
        /// </summary>
        /// <param name="statusTypeId">StatusTypeId</param>
        /// <param name="fromPartyContactMechanismId">fromPartyContactMechanismId</param>
        /// <param name="toPartyContactMechanismId">toPartyContactMechanismId</param>
        /// <param name="started">Start Date</param>
        /// <param name="ended">End Date</param>
        /// <param name="note">Note</param>
        /// <returns>RepositoryResponse object</returns>
        public RepositoryResponse CreateCommunicationEvent(int statusTypeId, long fromPartyContactMechanismId, long toPartyContactMechanismId, DateTime started, DateTime ended, string note)
        {
            long? communicationEventId = null;
            dynamic paramCommunicationEvent = new
            {
                statusTypeId,
                fromPartyContactMechanismId,
                toPartyContactMechanismId,
                started,
                ended,
                note,
                communicationEventId
            };
            using (var repository = GetRepository())
            {
                return repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateCommunicationEvent, paramCommunicationEvent);
            }
        }
        
        /// <summary>
        /// Create a communication event for email
        /// </summary>
        /// <param name="communicationEmailTemplateId">Communication Email Template Id</param>
        /// <param name="communicationEventId">Communication Event Id</param>
        public RepositoryResponse CreateCommunicationEventEmail(int communicationEmailTemplateId, long communicationEventId)
        {
            long? communicationEventEmailId = null;
            dynamic paramCommunicationEventEmail = new
            {
                communicationEmailTemplateId,
                communicationEventId,
                communicationEventEmailId
            };
            using (var repository = GetRepository())
            {
                return repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateCommunicationEventEmail, paramCommunicationEventEmail);
            }
        }
        /// <summary>
        /// Create a communication event for CES email
        /// </summary>
        /// <param name="cesId">CES Id</param>
        /// <param name="communicationEventId">Communication Event Id</param>
        public RepositoryResponse CreateCESCommunicationEventEmail(string cesId, long communicationEventId)
        {
            long? cesCommunicationEventId = null;
            dynamic paramCommunicationEventEmail = new
            {
                cesId,
                communicationEventId,
                cesCommunicationEventId
            };
            using (var repository = GetRepository())
            {
                return repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateCESCommunicationEvent, paramCommunicationEventEmail);
            }
        }
    }
}