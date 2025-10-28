using System;

namespace UnifiedLogin.SharedObjects.Landing
{
    /// <summary>
    /// Interface for ICommunicationEmail
    /// </summary>
    public interface ICommunicationEmail
    {
        /// <summary>
        /// CommunicationEmailTemplateId
        /// </summary>
        int CommunicationEmailTemplateId { get; set; }

        /// <summary>
        /// CommunicationEventAudienceTypeId
        /// </summary>
        int CommunicationEventAudienceTypeId { get; set; }

        /// <summary>
        /// Subject
        /// </summary>
        string Subject { get; set; }

        /// <summary>
        /// Body
        /// </summary>
        string Body { get; set; }
        
    }
}