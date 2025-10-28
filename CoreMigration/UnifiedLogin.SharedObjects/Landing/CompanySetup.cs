using Newtonsoft.Json;
using UnifiedLogin.SharedObjects.BlackBook;
using System;
using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Landing
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
        /// Status
        /// </summary>
        [JsonProperty(PropertyName = "Status")]
        public string Status { get; set; }

        public int IsActive { get; set; }

        /// <summary>
        /// Products
        /// </summary>
        [JsonProperty(PropertyName = "Products")]
        public int Products { get; set; }

        /// <summary>
        /// Address
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// RealPageAccessUser
        /// </summary>
        public string RealPageAccessUser { get; set; }

        /// <summary>
        /// RealPageAccessUserId
        /// </summary>
        public Guid RealPageAccessUserId { get; set; }


        /// <summary>  
        /// CompanyLocation
        /// </summary>
        [JsonProperty(PropertyName = "CompanyLocation")]
        public CompanyLocation CompanyLocation { get; set; }

        /// <summary>
		/// Total number of records count (without any paging if the response is limited by paging)
		/// </summary>
		public int TotalRecords { get; set; }

        /// <summary>  
        /// Use Primary Properties
        /// </summary>
        [JsonProperty(PropertyName = "EnablePrimaryProperties")]
        public bool EnablePrimaryProperties { get; set; } = false;

        /// <summary>  
        /// Enterprise Roles
        /// </summary>
        [JsonProperty(PropertyName = "EnableEnterpriseRoles")]
        public bool EnableEnterpriseRoles { get; set; } = false;

        /// <summary>  
        /// ThirdPartyIdps
        /// </summary>
        [JsonProperty(PropertyName = "ThirdPartyIdps")]
        public List<ThirdPartyIDPs> ThirdPartyIdps { get; set; }
    }
}
