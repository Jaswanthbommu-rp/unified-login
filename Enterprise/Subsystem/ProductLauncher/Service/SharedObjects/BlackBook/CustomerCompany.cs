using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook
{
	public class CustomerCompany
	{
        /// <summary>
		/// The id 
		/// </summary>
		public int Id { get; set; }

        /// <summary>
        /// The id of the company
        /// </summary>
        public int CustomerCompanyId { get; set; }

		/// <summary>
		/// The name of the company
		/// </summary>
		public string CompanyName { get; set; }

        /// <summary>
        /// Type of company
        /// </summary>
        //[JsonProperty("companyType")]
        public string CompanyType { get; set; }

        /// Phone number
        /// </summary>
        //[JsonProperty("phoneNumber")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// The status of the Migration for the company
        /// </summary>
        public string MigrationStatus { get; set; }

		/// <summary>
		/// The category of the company
		/// </summary>
		//public string Category { get; set; }

		/// <summary>
		/// Is the company active
		/// </summary>
		public bool IsActive { get; set; }
		
	}
}
