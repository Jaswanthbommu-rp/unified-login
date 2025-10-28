using System;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using UnifiedLogin.SharedObjects.BlackBook;
using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// Used when updating organization information
	/// </summary>
	public class OrganizationUpdate
	{
		/*
		/// <summary>
		/// Used to store the books id for the company
		/// </summary>
		[Required(ErrorMessage = "The books company id is required.", AllowEmptyStrings = false)]
		public long BooksCompanyId { get; set; }
		*/

		/// <summary>
		/// The name of the Organization
		/// </summary>
		[Required(ErrorMessage = "The company name is required.", AllowEmptyStrings = false)]
		[JsonProperty(PropertyName = "name")]
		[StringLength(150)]
		public string Name { get; set; }

		/// <summary>
		/// Organization Type Id
		/// </summary>
		[Range(1, int.MaxValue, ErrorMessage = "Please enter a Organization Type Id greater than {0}")]
		[JsonProperty(PropertyName = "organizationTypeId")]
		public int OrganizationTypeId { get; set; }

        /// <summary>
        /// Organization Domain Id
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Please enter a Organization Domain Id greater than {0}")]
        [JsonProperty(PropertyName = "organizationDomainId")]
        public int OrganizationDomainId { get; set; }

		/// <summary>
		/// Organization Type Name
		/// </summary>
		[StringLength(25)]
		[JsonProperty(PropertyName = "organizationTypeName")]
		public string OrganizationTypeName { get; set; }

        /// <summary>
        /// Organization Domain Name
        /// </summary>
        [StringLength(25)]
        [JsonProperty(PropertyName = "organizationDomainName")]
        public string OrganizationDomainName { get; set; }

		/// <summary>
		/// The UL guid for the company
		/// </summary>
		[JsonProperty(PropertyName = "realPageId")]
		public Guid RealPageId { get; set; }

		/// <summary>
		/// IsActive
		/// </summary>		
		public int IsActive { get; set; } = 1;

        /// <summary>
        /// Enable Primary Properties
        /// </summary>		
        public int EnablePrimaryProperties { get; set; } = 0;

        /// <summary>
        /// Enable Enterprise Role
        /// </summary>		
        public int EnableEnterpriseRoles { get; set; } = 0;

        /// <summary>
        /// ThirdPartyIDP
        /// </summary>		
        public String ThirdPartyIDP { get; set; }

        /// <summary>
        /// Company Address
        /// </summary>
        public CompanyInstanceAddress CompanyAddress { get; set; }
	}

	public class EnableDisableProducts
	{
      public List<string> AddProducts { get; set; }
			
	 public List<string> Removeproducts { get; set; }


    }
}
