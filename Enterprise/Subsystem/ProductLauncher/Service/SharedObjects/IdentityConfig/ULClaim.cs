namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig
{
    public class ULClaim
    {
        /// <summary>
        /// The id of the claim
        /// </summary>
        public int ClaimId { get; set; }

        /// <summary>
        /// The name of the claim
        /// </summary>
        public string ClaimName { get; set; }

        /// <summary>
        /// The data the claim should return if coming from a products SAML attributes
        /// </summary>
        public string SAMLAttributeName { get; set; }

        /// <summary>
        /// The product id to retrieve the claim for
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// The product name to retrieve the claim for
        /// </summary>
        public string ProductName { get; set; }

    }
}
