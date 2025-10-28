using System;
using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.BlackBook
{
    /// <summary>
    /// CompanyMap
    /// </summary>
    //public class CompanyMap
    public class CustomerCompanyMap
    {
        /// <summary>
        /// Id
        /// </summary>
        public string Id { get; set; }
        
		/// <summary>
        /// CompanyId
        /// </summary>
        public int CustomerCompanyId { get; set; }
        
		/// <summary>
        /// CompanyInstanceId
        /// </summary>
        public int CompanyInstanceId { get; set; }
        
		/// <summary>
        /// CompanyInstanceSourceId
        /// </summary>
        public string CompanyInstanceSourceId  { get; set; }
        
		/// <summary>
        /// Source
        /// </summary>
        public string Source { get; set; }
        
		/// <summary>
        /// CreatedAt
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
		/// <summary>
        /// CreatedBy
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// The domain for the company
        /// </summary>
        public string Domain { get; set; }

		/// <summary>
		/// List of attributes
		/// </summary>
		public List<CompanyInstance> CompanyInstance { get; set; }
    }
}
