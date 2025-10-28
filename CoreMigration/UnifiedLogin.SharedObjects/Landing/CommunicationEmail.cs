using Newtonsoft.Json;
using System;

namespace UnifiedLogin.SharedObjects.Landing
{
    /// <summary>
    /// CommunicationEmail object
    /// </summary>
    public class CommunicationEmail : ICommunicationEmail
    {

        /// <summary>
		/// CommunicationEmailTemplateId
		/// </summary>
		public int CommunicationEmailTemplateId { get; set; }

        /// <summary>
		/// CommunicationEventAudienceTypeId
		/// </summary>
		public int CommunicationEventAudienceTypeId { get; set; }

        /// <summary>
		/// Subject
		/// </summary>
		public string Subject { get; set; }

        /// <summary>
		/// Body
		/// </summary>
		public string Body { get; set; }
        
    }
}
