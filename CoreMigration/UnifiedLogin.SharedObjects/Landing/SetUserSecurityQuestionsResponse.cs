namespace UnifiedLogin.SharedObjects.Landing
{
    /// <summary>
    /// Set User Security Questions Response
    /// </summary>
    public class SetUserSecurityQuestionsResponse : ResponseBase
    {
        /// <summary>
        /// Enterprise User Name
        /// </summary>
        public string EnterpriseUserName { get; set; }

        /// <summary>
        /// Returns true if Success
        /// </summary>
        public bool IsSuccess { get; set; }
    }
}
