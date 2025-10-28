using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.IdentityConfig
{
	/// <summary>
	/// Identity Provider Type
	/// </summary>
	public class IdentityProviderType : IIdentityProviderType
	{
		/// <summary>
		/// Identity Provider type unique Id
		/// </summary>
		[JsonProperty(PropertyName = "AuthenticationType")]
		public string AuthenticationType { get; set; }

		/// <summary>
		/// Identity Provider Type value Contact Mechanism unique Id. 
		/// e.g. local = 46, oktacamden = 47, Google = 77,....
		/// </summary>
		[JsonProperty("ContactMechanismId", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int ContactMechanismId { get; set; } = 0;

        /// <summary>
		/// Identity Provider type Name
		/// </summary>
		[JsonProperty(PropertyName = "Name")]
        public string Name { get; set; } = null;

        /// <summary>
        /// Get value  indicating whether identity provider is RP System
        /// </summary>
        public bool IsLocal
        {
            get
            {
                return AuthenticationType.ToLower() == "local";
            }
        }

        #region Examples
        /// <summary>
        /// Example for New IdentityProviderType method
        /// </summary>
        /// <returns>Newly Created Identity Provider Type Id</returns>
        public static IdentityProviderTypeOutputResult GetNewIdentityProviderTypeExample()
        {
            IdentityProviderTypeOutputResult result = new IdentityProviderTypeOutputResult {IdentityProviderTypeId = 1};
            return result;
        }

        /// <summary>
        /// Output result Identity Provider Type Id
        /// </summary>
        public class IdentityProviderTypeOutputResult
        {
            /// <summary>
            /// Represents the newly created Identity Provider Type Id
            /// </summary>
            public int IdentityProviderTypeId { get; set; }
        }
        #endregion
    }
}
