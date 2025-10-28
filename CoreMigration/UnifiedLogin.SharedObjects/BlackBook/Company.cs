using Newtonsoft.Json;
using System;
using System.Collections.Generic;



namespace UnifiedLogin.SharedObjects.BlackBook
{
    /// <summary>
    /// The object to store BlackBook Company information
    /// </summary>
    
    public class Company
    {
        /// <summary>
        /// 
        /// </summary>
        public string Id { get; set; }


        /// <summary>
        /// The id of the company
        /// </summary>
        //[JsonProperty("companyId")]
        public long CustomerCompanyId { get; set; }

		/// <summary>
		/// Property name
		/// </summary>
		//[JsonProperty("companyName")]
		public string CompanyName { get; set; }
       
        /// <summary>
        /// Address
        /// </summary>        
        public CompanyLocation[] CustomerCompanyLocation { get; set; }

		/// <summary>
		/// Company info
		/// </summary>        
		public List<CustomerCompany> CustomerCompany { get; set; }

		/// <summary>
		/// Phone number
		/// </summary>
		//[JsonProperty("phoneNumber")]
		public string PhoneNumber { get; set; }
        /// <summary>
        /// Type of company
        /// </summary>
        //[JsonProperty("companyType")]
        public string CompanyType { get; set; }
        /// <summary>
        /// Is the Company active
        /// </summary>
        //[JsonProperty("isActive")]
        public bool? IsActive { get; set; }
        /// <summary>
        /// Who created the original record
        /// </summary>
        //[JsonProperty("createdBy")]
        public string CreatedBy { get; set; }
        /// <summary>
        /// Who last modified the record
        /// </summary>
        //[JsonProperty("modifiedBy")]
        public string ModifiedBy { get; set; }
        public long? MasterCompanyId { get; set; }
    }

}

