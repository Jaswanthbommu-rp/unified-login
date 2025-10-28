using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Product.VendorServices
{
    public class VendorServicesUser
    {
        public string ID { get; set; }

        public string UserCode { get; set; }

        public string Username { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Password { get; set; }

        public string CompanyId { get; set; }
        //public string CompanyCode { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? CompanyDivisionId { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string AccessLevel { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string AccessLevelCode { get; set; }

        public string FirstName { get; set; }

	    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	    public string MiddleName { get; set; }

		public string LastName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Email { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Phone { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool EMailNotifyInsurance { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool Locked { get; set; }//Not IsLocked in product

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? LastLoginDate { get; set; }

        public bool EMailNotifyRecommendation { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool EMailNotifyVendorNotLinkedToAnyProperty { get; set; }

        /// <summary>
        /// User access group codes
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IList<UserAccessGroup> UserAccessGroups { get; set; }

        /// <summary>
        /// List of property id
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IList<UserLocation> UserLocations { get; set; }
    }
}
