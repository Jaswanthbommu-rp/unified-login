using System;

namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// Interface for IEmail
	/// </summary>
	public interface IEmail
	{
		/// <summary>
		/// clientUniqueID
		/// </summary>
		Guid ClientUniqueID { get; set; }

		/// <summary>
		/// entityID
		/// </summary>
		string EntityID { get; set; }

		/// <summary>
		/// siteID
		/// </summary>
		string SiteID { get; set; }

		/// <summary>
		/// emailTo
		/// </summary>
		string EmailTo { get; set; }

		/// <summary>
		/// emailFrom
		/// </summary>
		string EmailFrom { get; set; }

		/// <summary>
		/// emailSubject
		/// </summary>
		string EmailSubject { get; set; }

		/// <summary>
		/// emailReplyTo
		/// </summary>
		string EmailReplyTo { get; set; }

		/// <summary>
		/// emailAttachment
		/// </summary>
		string EmailAttachment { get; set; }

		/// <summary>
		/// emailBody
		/// </summary>
		string EmailBody { get; set; }
        
	}
}