using System.Collections.Generic;
using System.Linq;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository
{
	/// <summary>
	/// Electronic Address Repository
	/// </summary>
	public class EmailRepository : BaseRepository, IEmailRepository
	{
		#region Constructor
		/// <summary>
		/// Email Content Base Constructor
		/// </summary>
		public EmailRepository() : base(DbConnectionEnum.IdpConfigurationDb)
		{
		}

        public EmailRepository(IRepository repository) : base(repository)
        {
        }

		#endregion

		#region public Email methods
		/// <summary>
		/// Get an Email Template by Audience Type and Purpose Type
		/// </summary>
		/// <param name="communicationEventAudienceTypeId">Audience Type Id</param>
		/// <param name="communicationEventPurposeTypeId">Purpose Type Id</param>
		/// <returns>Communication Email object</returns>
		public CommunicationEmail GetEmailTemplate(int communicationEventAudienceTypeId, int communicationEventPurposeTypeId)
		{
            dynamic paramTemplates = new
            {
                communicationEventAudienceTypeId,
                communicationEventPurposeTypeId
            };
            using (var repository = GetRepository())
            {
                CommunicationEmail emailTemplate = repository.GetOne<CommunicationEmail>(StoredProcNameConstants.SP_ListCommunicationEmailTemplates, paramTemplates);
                return emailTemplate;
            }
		}

        /// <summary>
		/// List Email Templates
		/// </summary>
		/// <returns>List of Communication Email object</returns>
		public IList<CommunicationEmail> ListEmailTemplates()
        {
            using (var repository = GetRepository())
            {
                IList<CommunicationEmail> emailTemplate = repository.GetMany<CommunicationEmail>(StoredProcNameConstants.SP_ListCommunicationEmailTemplates, null).ToList();
                return emailTemplate;
            }
        }
        #endregion

    }
}