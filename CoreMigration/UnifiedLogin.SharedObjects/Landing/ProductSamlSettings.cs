namespace UnifiedLogin.SharedObjects.Landing
{
    /// <summary>
    /// Used to store the SAML related settings for a product for login
    /// </summary>
    public class ProductSamlSettings
    {
        /// <summary>
        /// The unique id for the setting
        /// </summary>
        public int ProductSamlSettingsId { get; set; }

        /// <summary>
        /// The id of the product
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// The url used to log into the product using SAML
        /// </summary>
        public string LoginUri { get; set; }

        /// <summary>
        /// The certificate thumbprint to use for signing the SAML assertion
        /// </summary>
        public string SigningCertificateThumbprint { get; set; }

        /// <summary>
        /// The SAML attribute name to use for the SAML Subject identifier when generating the assertion
        /// </summary>
        public string SubjectIdSamlAttribute { get; set; }
    }
}
