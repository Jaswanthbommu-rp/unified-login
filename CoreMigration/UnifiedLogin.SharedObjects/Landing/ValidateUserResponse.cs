namespace UnifiedLogin.SharedObjects.Landing
{
    /// <summary>
    /// Validate User Response
    /// </summary>
    public class ValidateUserResponse : ResponseBase
    {
        /// <summary>
        /// EnterpriseUserName
        /// </summary>
        public string EnterpriseUserName { get; set; }

        /// <summary>
        /// Activity Token received in email
        /// </summary>
        public string ValidateUserToken { get; set; }
    }
}
