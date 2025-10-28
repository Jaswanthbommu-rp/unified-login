namespace UnifiedLogin.SharedObjects.Saml
{
    /// <summary>
    /// The various SAML attributes sent to the product for the given user
    /// </summary>
    public class SamlAttributes : ISamlAttributes
    {
        /*
        /// <summary>
        /// The default constructor for creating a SAML attribute
        /// </summary>
        /// <param name="SamlAttributeId"></param>
        /// <param name="Name"></param>
        /// <param name="Type"></param>
        /// <param name="Value"></param>
        public SAMLAttributes( string Name, string Type, string Value)
        {
            //this.SamlAttributeId = SamlAttributeId;
            this.Name = Name;
            this.Value = Value;
            this.Type = Type;
        }
        */

        /// <summary>
        /// The unique id for the SAML attribute
        /// </summary>
        public int SamlAttributeId { get; set; }

        /// <summary>
        /// The name of the SAML attribute
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The value of the SAML attribute
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// The data type of the SAML attribute
        /// </summary>
        public string Type { get; set; }

		/// <summary>
		/// Unique Saml User Attribute Id
		/// </summary>
		public int SamlUserAttributeId { get; set; }

        /// <summary>
		/// Display Name
		/// </summary>
		public string DisplayName { get; set; }
    }
}