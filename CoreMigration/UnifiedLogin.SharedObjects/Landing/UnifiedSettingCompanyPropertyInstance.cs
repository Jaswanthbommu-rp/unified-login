using Newtonsoft.Json;
using UnifiedLogin.SharedObjects.BlackBook;
using System;
using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Landing
{
    /// <summary>
    /// UnifiedSetting CompanyProperty
    /// </summary>
    public class UnifiedSettingCompanyProperty
	{
		/// <summary>
		/// Source Name
		/// </summary>
		public string Source { get; set; }

		/// <summary>
		/// Company Value
		/// </summary>
		public UnifiedSettingCompanyInstance Company { get; set; }

		/// <summary>
		/// UnifiedSettingPropertyInstance
		/// </summary>
		public List<UnifiedSettingCompanyPropertyInstance> Properties { get; set; }

		/// <summary>
		/// CustomerEnvironment
		/// </summary>
		public string CustomerEnvironment { get; set; }
	}

    public class UnifiedSettingCompanyPropertyPayload
	{
        public UnifiedSettingCompanyProperty Payload { get; set; }
	}

	public class UnifiedSettingCompanyInstance
	{
		/// <summary>
		/// CompanyInstanceSourceId
		/// </summary>
		public string CompanyInstanceSourceId { get; set; }
	}

	public class UnifiedSettingCompanyPropertyInstance
	{       

        /// <summary>
        /// Property Name
        /// </summary>
        [JsonProperty(PropertyName = "PropertyName")]
        public string PropertyName { get; set; }

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
        [JsonProperty(PropertyName = "PropertyInstanceSourceId")]
        public Guid PropertyInstanceSourceId { get; set; }

        /// <summary>
        /// CustomerPropertyId
        /// </summary>
        [JsonProperty(PropertyName = "CustomerPropertyId")]
        public string CustomerPropertyId { get; set; }

        /// <summary>
        /// IsActive
        /// </summary>
        [JsonProperty(PropertyName = "IsActive")]
        public bool IsActive { get; set; }
    }
}
