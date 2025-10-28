namespace UnifiedLogin.SharedObjects.Landing
{
    public class OrganizationCreateResult
    {
        /// <summary>
        /// Organization
        /// </summary>
        public Organization Org { get; set; }

        /// <summary>
        /// admin unique PersonaId
        /// </summary>
        public long adminPersonaId { get; set; }

        /// <summary>
        /// admin login
        /// </summary>
        public string adminLogin { get; set; }

        /// <summary>
        /// Organization Book MasterId
        /// </summary>
        public long BooksCompanyId { get; set; }

        /// <summary>
        /// Organization Book Customer MasterId
        /// </summary>
        public long BooksCustomerMasterId { get; set; }
    }
}
