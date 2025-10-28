using Newtonsoft.Json;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Product.Lead2Lease
{
    /// <summary>
    /// Used to store Property information from Lead2Lease
    /// </summary>
    public class Property
    {
        /// <summary>
        /// Used to store the account id
        /// </summary>
        [JsonProperty("AccountId", NullValueHandling = NullValueHandling.Ignore)]
        public int? AccountId { get; set; }

        /// <summary>
        /// Used to store the property id
        /// </summary>
        [JsonProperty("PropertyId", NullValueHandling = NullValueHandling.Ignore)]
        public int PropertyId { get; set; }
        
        /// <summary>
        /// Used to store the property name
        /// </summary>
        [JsonProperty("ComplexName", NullValueHandling = NullValueHandling.Ignore)]
        public string ComplexName { get; set; }

        /// <summary>
        /// Used to store the property street address
        /// </summary>
        [JsonProperty("Address", NullValueHandling = NullValueHandling.Ignore)]
        public string Address { get; set; }

        /// <summary>
        /// Used to store the property city
        /// </summary>
        [JsonProperty("City", NullValueHandling = NullValueHandling.Ignore)]
        public string City { get; set; }

        /// <summary>
        /// Used to store the property state
        /// </summary>
        [JsonProperty("State", NullValueHandling = NullValueHandling.Ignore)]
        public string State { get; set; }

        /// <summary>
        /// Used to store the property zip
        /// </summary>
        [JsonProperty("Zip", NullValueHandling = NullValueHandling.Ignore)]
        public string Zip { get; set; }

        /// <summary>
        /// Used to store the property phone
        /// </summary>
        [JsonProperty("Phone", NullValueHandling = NullValueHandling.Ignore)]
        public string Phone { get; set; }

        /// <summary>
        /// Used to store the property email
        /// </summary>
        [JsonProperty("Email", NullValueHandling = NullValueHandling.Ignore)]
        public string Email { get; set; }

        /// <summary>
        /// Used to store the property system type id
        /// </summary>
        [JsonProperty("PMSystemTypeID", NullValueHandling = NullValueHandling.Ignore)]
        public int? PMSystemTypeID { get; set; }

        /// <summary>
        /// Used to store the property system id
        /// </summary>
        [JsonProperty("PMSystemID", NullValueHandling = NullValueHandling.Ignore)]
        public string PMSystemID { get; set; }

        /// <summary>
        /// Used to store the property management user id
        /// </summary>
        [JsonProperty("PMUserId", NullValueHandling = NullValueHandling.Ignore)]
        public string PMUserId { get; set; }

        /// <summary>
        /// Used to store the property management user name
        /// </summary>
        [JsonProperty("PMUserName", NullValueHandling = NullValueHandling.Ignore)]
        public string PMUserName { get; set; }

        /// <summary>
        /// Used to store the property management user first name
        /// </summary>
        [JsonProperty("FirstName", NullValueHandling = NullValueHandling.Ignore)]
        public string FirstName { get; set; }

        /// <summary>
        /// Used to store the property management user last name
        /// </summary>
        [JsonProperty("LastName", NullValueHandling = NullValueHandling.Ignore)]
        public string LastName { get; set; }
    }
}
