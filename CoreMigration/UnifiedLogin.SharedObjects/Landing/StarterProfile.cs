namespace UnifiedLogin.SharedObjects.Landing
{
    /// <summary>
    /// Starter Profile Options
    /// </summary>
    public class StarterProfile
    {
        /// <summary>
        /// EnterpriseUserName
        /// </summary>
        public string EnterpriseUserName { get; set; }

        /// <summary>
        /// ActivityToken
        /// </summary>
        public string ActivityToken { get; set; }

        /// <summary>
        /// IndustryJobTitleId
        /// </summary>
        public int IndustryJobTitleId { get; set; }

        /// <summary>
        /// CompanyJobTitle
        /// </summary>
        public string CompanyJobTitle { get; set; }

        /// <summary>
        /// PhoneNumber
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// PhoneTypeId
        /// </summary>
        public int PhoneTypeId { get; set; }
    }
}
