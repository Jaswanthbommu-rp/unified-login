using System.Collections.Generic;
using UnifiedLogin.SharedObjects.Enum;

namespace UnifiedLogin.SharedObjects.Landing
{
    /// <summary>
    /// Starter Profile Options Response
    /// </summary>
    public class StarterProfileOptionsResponse : ResponseBase
    {
        /// <summary>
        /// EnterpriseUserName
        /// </summary>
        public string EnterpriseUserName { get; set; }

        /// <summary>
		/// Firstname
		/// </summary>
		public string Firstname { get; set; }
        /// <summary>
        /// Lastname
        /// </summary>
        public string Lastname { get; set; }

        /// <summary>
        /// StandardJobTitles
        /// </summary>
        public IList<JobTitle> StandardJobTitles { get; set; }

        /// <summary>
        /// PhoneTypes
        /// </summary>
        public IList<Phone> PhoneTypes { get; set; }
    }
}
