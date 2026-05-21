namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing
{
    /// <summary>
    /// Request object for sending an MFA one-time authentication code email.
    /// </summary>
    public class MFAEmailRequest
    {
        /// <summary>
        /// The recipient's first name (used to personalise the email body).
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// The recipient's email address.
        /// </summary>
        public string EmailAddress { get; set; }

        /// <summary>
        /// The one-time authentication code to embed in the email.
        /// </summary>
        public string OtpCode { get; set; }
    }
}
