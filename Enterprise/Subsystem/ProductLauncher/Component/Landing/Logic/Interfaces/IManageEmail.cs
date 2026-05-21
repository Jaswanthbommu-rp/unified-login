using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces
{
    /// <summary>
    /// Interface for Manage Email
    /// </summary>
    public interface IManageEmail
	{
        /// <summary>
        /// Get Email Template
        /// </summary>
        /// <param name="communicationEventAudienceTypeId">AudienceTypeId of the Communication</param>
        /// <param name="communicationEventPurposeTypeId">PurposeTypeId of the Communication</param>
        /// <returns>IEmail object</returns>
        CommunicationEmail GetEmailTemplate(int communicationEventAudienceTypeId, int communicationEventPurposeTypeId);

        /// <summary>
        /// Build Welcome Email for New Users
        /// </summary>
        /// <param name="loginName">Login name of new user</param>
        /// <param name="firstName">First name of new user</param>
        /// <param name="companyName">Name of the company</param>
        /// <param name="orgPartyId">Id of the company</param>
        /// <param name="emailTemplate">Email Template to be filled up</param>
        /// <param name="newUserToken">User token of the new user</param>
        /// <param name="senderEmailAddress">Email address of the system</param>
        /// <param name="notificationEmail">Email address where the email will be sent out</param>
        /// <returns>RepositoryResponse object</returns>
        //Email CreateWelcomeEmail(IProfileDetail newUser, CommunicationEmail emailTemplate, string newUserToken, string senderEmailAddress = "", string notificationEmail = "");
        Email CreateWelcomeEmail(string loginName, string firstName, string companyName, long orgPartyId, CommunicationEmail emailTemplate, string newUserToken, string senderEmailAddress = "", string notificationEmail = "");

        /// <summary>
		/// Send an Email through CES
		/// </summary>
		/// <param name="email">Email data object</param>
		/// <returns>Email Status</returns>
		string SendEmail(Email email);

		/// <summary>
		/// Send an Email through SendGrid
		/// </summary>
		/// <param name="sendGridEmail">SendGrid Email object</param>
		/// <returns>Email Status</returns>
		string SendGridEmail(ISendGridEmail sendGridEmail);

        /// <summary>
        /// Sending emails from unified emails API
        /// </summary>
        /// <param name="emailModel"></param>
        /// <returns></returns>
        bool SendEmailAsync(EmailModel emailModel);

        /// <summary>
        /// Send MFA one-time authentication code email to the user.
        /// Fetches the branded HTML template from the database, replaces {FIRST NAME} and {OTP CODE} placeholders,
        /// then sends via UnifiedEmail (preferred) or SendGrid depending on product settings.
        /// </summary>
        /// <param name="firstName">The recipient's first name</param>
        /// <param name="emailAddress">The recipient's email address</param>
        /// <param name="otpCode">The one-time authentication code to embed in the email</param>
        /// <returns>Status message string</returns>
        string SendMFAEmail(string firstName, string emailAddress, string otpCode);
    }
}