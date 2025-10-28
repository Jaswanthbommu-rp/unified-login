using System.Collections.Generic;
using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.BlackBook
{
    /// <summary>
    /// The object to store BlackBook Company instance information
    /// </summary>
    public class CompanyInstance
    {
        /// <summary>
        /// The id of the site
        /// </summary>
        //public string Id { get; set; }
        
		/// <summary>
        /// Property instance id
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public long? CompanyInstanceId { get; set; }

        /// <summary>
        /// Customer company instance id
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public long? CustomerCompanyId { get; set; }

		/// <summary>
        /// Property instance source id
        /// </summary>
        public string CompanyInstanceSourceId { get; set; }
        
		/// <summary>
        /// Property name
        /// </summary>
        public string CompanyName { get; set; }
        
		/// <summary>
        /// Source
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Company address
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<CompanyInstanceAddress> CompanyInstanceLocation { get; set; }
        
		/// <summary>
        /// Phone number
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string PhoneNumber { get; set; }
        
		/// <summary>
        /// Type of company
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string CompanyType { get; set; }
        
		/// <summary>
        /// Is the Company active
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool IsActive { get; set; }
        
		/// <summary>
        /// Who created the original record
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string CreatedBy { get; set; }
        
		/// <summary>
        /// Who last modified the record
        /// </summary
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ModifiedBy { get; set; }

		/// <summary>
		/// List of attributes
		/// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public List<InstanceAttribute> Attributes { get; set; }

        /// <summary>
        /// used to store the customer domain
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string CustomerEnvironment { get; set; }

        /// <summary>
        /// used to store the customer domain
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Domain { get; set; }

        /// <summary>
        /// GreenBookCares
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool GreenBookCares { get; set; }
    }
}
