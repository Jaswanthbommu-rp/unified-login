using System.Collections.Generic;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces
{
	/// <summary>
	/// Interface for Email Repository
	/// </summary>	
	public interface IEmailRepository
	{
		/// <summary>
		/// Get Email Template from Repository
		/// </summary>
		CommunicationEmail GetEmailTemplate(int communicationEventAudienceTypeId, int communicationEventPurposeTypeId);

        /// <summary>
		/// List Email Templates from Repository
		/// </summary>
		IList<CommunicationEmail> ListEmailTemplates();


    }
}