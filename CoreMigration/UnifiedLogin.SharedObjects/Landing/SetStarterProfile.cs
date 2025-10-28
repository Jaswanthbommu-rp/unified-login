namespace UnifiedLogin.SharedObjects.Landing
{
    /// <summary>
    /// Set Starter Profile Options API Response
    /// </summary>
    public class SetStarterProfile : ResponseBase
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
