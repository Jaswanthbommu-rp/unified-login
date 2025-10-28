namespace UnifiedLogin.SharedObjects.IdentityConfig
{
    public class ClientClaimMapping
    {
        /// <summary>
        /// The id
        /// </summary>
        public int ClientUserClaimId { get; set; }

        /// <summary>
        /// The client id
        /// </summary>
        public int ClientId { get; set; }

        /// <summary>
        /// The claim id
        /// </summary>
        public int ClaimId { get;set; }
        public string ClaimName { get; set; }
        public string SAMLAttributeName { get; set; }
        public string ProductName { get; set; }
    }
}
