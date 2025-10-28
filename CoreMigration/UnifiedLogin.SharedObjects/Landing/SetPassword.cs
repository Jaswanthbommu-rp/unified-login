namespace UnifiedLogin.SharedObjects.Landing
{
    /// <summary>
    /// Set Password
    /// </summary>
    public class SetPassword
    {
        /// <summary>
        /// Enterprise User Name
        /// </summary>
        public string EnterpriseUserName { get; set; }

        /// <summary>
        /// Activity Token
        /// </summary>
        public string ActivityToken { get; set; }

        /// <summary>
        /// New Password
        /// </summary>
        public string NewPassword { get; set; }
    }
}
