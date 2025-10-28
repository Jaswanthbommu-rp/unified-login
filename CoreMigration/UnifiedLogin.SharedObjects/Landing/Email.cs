using System;

namespace UnifiedLogin.SharedObjects.Landing
{
    /// <summary>
    /// Email object
    /// </summary>
    public class Email : IEmail
	{

        /// <summary>
        /// clientUniqueID
        /// </summary>
        public Guid ClientUniqueID { get; set; } = Guid.NewGuid();

        /// <summary>
        /// entityID
        /// </summary>
        public string EntityID { get; set; } = "1";

		/// <summary>
		/// siteID
		/// </summary>
		public string SiteID { get; set; } = "1";

        /// <summary>
        /// emailTo
        /// </summary>
        public string EmailTo { get; set; }

        /// <summary>
        /// emailFrom
        /// </summary>
        public string EmailFrom { get; set; } = "no-reply@realpage.com";

		/// <summary>
		/// emailSubject
		/// </summary>
		public string EmailSubject { get; set; }

		/// <summary>
		/// emailReplyTo
		/// </summary>
		public string EmailReplyTo { get; set; }

		/// <summary>
		/// emailAttachment
		/// </summary>
		public string EmailAttachment { get; set; }

		/// <summary>
		/// emailBody
		/// </summary>
		public string EmailBody { get; set; }
        
	}
}
