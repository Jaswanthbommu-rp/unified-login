using Newtonsoft.Json;
using System.Collections.Generic;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Product.MarketingCenter
{
    /// <summary>
    /// Used to store information about a Marketing Center user
    /// </summary>
    public class MarketingCenterUserDetails
    {
        /// <summary>
        /// Used to store the id of the user
        /// </summary>
        [JsonProperty("id")]
        public long Id { get; set; }
        /// <summary>
        /// Used to store if the user is active
        /// </summary>
        [JsonProperty("active")]
        public bool Active { get; set; }
        /// <summary>
        /// Used to store the role id of the user
        /// </summary>
        [JsonProperty("contactRoleId")]
        public int ContactRoleId { get; set; }
		/// <summary>
		/// Used to store the role id of the user
		/// </summary>
		[JsonProperty("companyId")]
		public int CompanyId { get; set; }
		/// <summary>
		/// Indicates whether new properties will automatically be assigned to this contact	
		/// </summary>
		[JsonProperty("assignNewProperty")]
		public bool AssignNewProperty { get; set; }
		/// <summary>
		/// Used to store a list of property ids assigned to the user
		/// </summary>
		[JsonProperty("assignedProperties")]
        public List<Property> AssignedProperties { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Property
    {
        /// <summary>
        /// The unique id of the property
        /// </summary>
        [JsonProperty("id")]
        public long Id { get; set; }
        /// <summary>
        /// The name of the property
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
        /// <summary>
        /// The status of the property
        /// </summary>
        [JsonProperty("active")]
        public bool Active { get; set; }
        /// <summary>
        /// The address of the property
        /// </summary>
        [JsonProperty("address")]
        public Address Address { get; set; }
    }

    /// <summary>
    /// Used to store property addresses for Marketing Center
    /// </summary>
    public class Address
    {
        /// <summary>
        /// The primary address of the property
        /// </summary>
        [JsonProperty("address1")]
        public string Address1 { get; set; }
        /// <summary>
        /// The secondary street address of the property
        /// </summary>
        [JsonProperty("address2")]
        public string Address2 { get; set; }
        /// <summary>
        /// The city where the property is located
        /// </summary>
        [JsonProperty("cityName")]
        public string CityName { get; set; }
        /// <summary>
        /// The state where the property is located
        /// </summary>
        [JsonProperty("stateCode")]
        public string StateCode { get; set; }
        /// <summary>
        /// The zip code where the property is located
        /// </summary>
        [JsonProperty("postalCode")]
        public string PostalCode { get; set; }
    }
}
