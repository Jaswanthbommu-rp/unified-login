using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook
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
        public int CompanyInstanceId { get; set; }

        /// <summary>
        /// Customer company instance id
        /// </summary>
        public long CustomerCompanyId { get; set; }

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
        /// Address
        /// </summary>
        //public List<CompanyInstanceAddress> CompanyInstanceLocation { get; set; }
        
		/// <summary>
        /// Phone number
        /// </summary>
        public string PhoneNumber { get; set; }
        
		/// <summary>
        /// Type of company
        /// </summary>
        public string CompanyType { get; set; }
        
		/// <summary>
        /// Is the Company active
        /// </summary>
        public bool IsActive { get; set; }
        
		/// <summary>
        /// Who created the original record
        /// </summary>
        public string CreatedBy { get; set; }
        
		/// <summary>
        /// Who last modified the record
        /// </summary>
        public string ModifiedBy { get; set; }

		/// <summary>
		/// List of attributes
		/// </summary>
		public List<InstanceAttribute> Attributes { get; set; }

        /// <summary>
        /// used to store the customer domain
        /// </summary>
        public string CustomerEnvironment { get; set; }
	}
}
