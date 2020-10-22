using Newtonsoft.Json;
using System;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects
{
    /// <summary>
    /// Company Setup
    /// </summary>
    public class CompanySetup
    {
        /// <summary>
        /// OrganizationPartyId
        /// </summary>
        [JsonProperty(PropertyName = "OrganizationPartyId")]
        public int OrganizationPartyId { get; set; }

        /// <summary>
        /// Organization Name
        /// </summary>
        [JsonProperty(PropertyName = "OrganizationName")]
        public string OrganizationName { get; set; }

        /// <summary>
        /// OrganizationType
        /// </summary>
        [JsonProperty(PropertyName = "ContractedName")]
        public string ContractedName { get; set; }

        /// <summary>
        /// Unique RealPage Id
        /// </summary>
        [JsonProperty(PropertyName = "RealPageId")]
        public Guid RealPageId { get; set; }

        /// <summary>
        /// BooksMasterId
        /// </summary>
        [JsonProperty(PropertyName = "BooksMasterId")]
        public string BooksMasterId { get; set; }

        /// <summary>
        /// BooksCustomerMasterId
        /// </summary>
        [JsonProperty(PropertyName = "BooksCustomerMasterId")]
        public string BooksCustomerMasterId { get; set; }

        /// <summary>
        /// OrganizationTypeId
        /// </summary>
        [JsonProperty(PropertyName = "OrganizationTypeId")]
        public int OrganizationTypeId { get; set; }

        /// <summary>
        /// OrganizationType
        /// </summary>
        [JsonProperty(PropertyName = "OrganizationType")]
        public string OrganizationType { get; set; }

        /// <summary>
        /// OrganizationDomainId
        /// </summary>
        [JsonProperty(PropertyName = "OrganizationDomainId")]
        public int OrganizationDomainId { get; set; }

        /// <summary>
        /// Domain
        /// </summary>
        [JsonProperty(PropertyName = "Domain")]
        public string Domain { get; set; }

        /// <summary>
        /// Products
        /// </summary>
        [JsonProperty(PropertyName = "Products")]
        public int Products { get; set; }

        /// <summary>
        /// Products
        /// </summary>
        [JsonProperty(PropertyName = "Properties")]
        public int Properties { get; set; } = 30; //TODO: Remove Hardcoded value after implementing properties count

        /// <summary>
        /// Address
        /// </summary>
        [JsonProperty(PropertyName = "Address")]
        public string Address { get; set; }

        /// <summary>
		/// Total number of records count (without any paging if the response is limited by paging)
		/// </summary>
		public int TotalRecords { get; set; }
    }
}
