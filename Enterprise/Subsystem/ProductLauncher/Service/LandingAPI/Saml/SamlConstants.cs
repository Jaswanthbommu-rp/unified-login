namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Saml
{
    /// <summary>
    /// All Constants for Saml helper
    /// </summary>
    public static class RealPageSamlConstants
    {
        /// <summary>
        /// The SAML version
        /// </summary>
        public const string Version = "2.0";

        /// <summary>
        /// The Assertion URI
        /// </summary>
        public const string AssertionUri = "urn:oasis:names:tc:SAML:2.0:assertion";

        /// <summary>
        /// The Password URI
        /// </summary>
        public const string PasswordUri = "urn:oasis:names:tc:SAML:2.0:ac:classes:Password";

        /// <summary>
        /// The SAML XML prefixes
        /// </summary>
        public static class Prefixes
        {
            /// <summary>
            /// The SAML prefix
            /// </summary>
            public const string SAML = "saml";

            /// <summary>
            /// The SAML protocol prefix
            /// </summary>
            public const string SAMLP = "samlp";

            /// <summary>
            /// The SAML metadata prefix
            /// </summary>
            public const string MD = "md";
        }

        public static class Algorithms
        {
            /// <summary>
            /// The SAML SHA1 signature method URI
            /// </summary>
            public const string SHA1_SignatureMethod = "http://www.w3.org/2000/09/xmldsig#rsa-sha1";
            /// <summary>
            /// The SAML SHA1 digest method URI
            /// </summary>
            public const string SHA1_DigestMethod = "http://www.w3.org/2000/09/xmldsig#sha1";
        }

        /// <summary>
        /// The SAML XML namespace URIs
        /// </summary>
        public static class NamespaceURIs
        {
            /// <summary>
            /// The SAML assertion namespace URI
            /// </summary>
            public const string Assertion = "urn:oasis:names:tc:SAML:2.0:assertion";

            /// <summary>
            /// The SAML protocol namespace URI
            /// </summary>
            public const string Protocol = "urn:oasis:names:tc:SAML:2.0:protocol";

            /// <summary>
            /// The SAML metadata namespace URI
            /// </summary>
            public const string Metadata = "urn:oasis:names:tc:SAML:2.0:metadata";

            /// <summary>
            /// The SAML encryption signature namespage
            /// </summary>
            public const string Signature = "http://www.w3.org/2000/09/xmldsig#";
        }
        public static class AttributeURIs
        {
            /// <summary>
            /// The SAML basic attribute URI
            /// </summary>
            public const string Basic = "urn:oasis:names:tc:SAML:2.0:attrname-format:basic";
            /// <summary>
            /// The SAML dotnet attribute URI
            /// </summary>
            public const string DotNet = "urn:oasis:names:tc:SAML:2.0:attrname-format:dotnet";

        }
        public static class StatusUris
        {
            /// <summary>
            /// The SAML status Success URI
            /// </summary>
            public const string Success = "urn:oasis:names:tc:SAML:2.0:status:Success";
        }
        public static class NameIDFormatUris
        {
            /// <summary>
            /// The SAML NameID format unspecified URI
            /// </summary>
            public const string Unspecified = "urn:oasis:names:tc:SAML:1.1:nameid-format:unspecified";
        }
    }
}