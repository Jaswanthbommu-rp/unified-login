using Newtonsoft.Json;
using UnifiedLogin.SharedObjects.BlackBook;
using System;

namespace UnifiedLogin.SharedObjects.Landing
{
    /// <summary>
    /// Property Setup
    /// </summary>
    public class PropertySetup
    {
        /// <summary>
        /// PropertyInstanceId
        /// </summary>
        [JsonProperty(PropertyName = "PropertyInstanceId")]
        public int PropertyInstanceId { get; set; }

        /// <summary>
        /// Property Name
        /// </summary>
        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; }

        /// <summary>
        /// OrganizationType
        /// </summary>
        [JsonProperty(PropertyName = "ContractedName")]
        public string ContractedName { get; set; }

        /// <summary>
        /// Address
        /// </summary>
        [JsonProperty(PropertyName = "Address")]
        public string Address { get; set; }

        /// <summary>
        /// City
        /// </summary>
        [JsonProperty(PropertyName = "City")]
        public string City { get; set; }

        /// <summary>
        /// State
        /// </summary>
        [JsonProperty(PropertyName = "State")]
        public string State { get; set; }

        /// <summary>
        /// PostalCode
        /// </summary>
        [JsonProperty(PropertyName = "PostalCode")]
        public string PostalCode { get; set; }

        /// <summary>
        /// Country
        /// </summary>
        [JsonProperty(PropertyName = "Country")]
        public string Country { get; set; }

        /// <summary>
        /// County
        /// </summary>
        [JsonProperty(PropertyName = "County")]
        public string County { get; set; }


        /// <summary>
        /// Property UPFM Id
        /// </summary>
        [JsonProperty(PropertyName = "InstanceId")]
        public Guid InstanceId { get; set; }

        /// <summary>
        /// CustomerPropertyId
        /// </summary>
        [JsonProperty(PropertyName = "CustomerPropertyId")]
        public string CustomerPropertyId { get; set; }

        /// <summary>
        /// Domain
        /// </summary>
        [JsonProperty(PropertyName = "Domain")]
        public string Domain { get; set; }

        /// <summary>
        /// Latitude
        /// </summary>
        [JsonProperty(PropertyName = "Latitude")]
        public decimal? Latitude { get; set; }

        /// <summary>
        /// Longitude
        /// </summary>
        [JsonProperty(PropertyName = "Longitude")]
        public decimal? Longitude { get; set; }

        /// <summary>
        /// IsActive
        /// </summary>
        [JsonProperty(PropertyName = "IsActive")]
        public string IsActive { get; set; }

        /// <summary>
        /// PropertyAddress for export
        /// </summary>
        [JsonProperty(PropertyName = "PropertyAddress")]
        public string PropertyAddress { get; set; }

        /// <summary>
        /// IsAssigned
        /// </summary>
        [JsonProperty(PropertyName = "IsAssigned")]
        public bool IsAssigned { get; set; } = false;

		/// <summary>
		/// CustomerStatus
		/// </summary>
		[JsonProperty(PropertyName = "CustomerStatus")]
		public bool CustomerStatus { get; set; }

		/// <summary>
		/// OrderType
		/// </summary>
		[JsonProperty(PropertyName = "OrderType")]
		public string OrderType { get; set; }

		 /// <summary>
		 /// Total number of records count (without any paging if the response is limited by paging)
		 /// </summary>
		 public int TotalRecords { get; set; }
    }
}
