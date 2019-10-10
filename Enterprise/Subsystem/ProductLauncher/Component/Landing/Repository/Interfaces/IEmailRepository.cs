using System.Collections.Generic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces
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