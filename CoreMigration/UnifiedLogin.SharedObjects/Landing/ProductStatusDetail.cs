using Newtonsoft.Json;
using UnifiedLogin.SharedObjects.BlackBook;
using System;
using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Landing
{
    /// <summary>
    /// Product status detail
    /// </summary>
    public class ProductStatusDetail
    {
        /// <summary>
        /// PropertyInstanceId
        /// </summary>
        public string CustomerPropertyId { get; set; }

        /// <summary>
        /// ContractedName
        /// </summary>
        public string ContractedName { get; set; }

        /// <summary>
        /// ContractedName
        /// </summary>
        public string ProductInstanceId { get; set; }

        /// <summary>
        /// Domain
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// IsActive
        /// </summary>
        public string IsActive { get; set; }
    }

    public class ProductPropertyDetails
	{
        /// <summary>
        /// productStatusDetail
        /// </summary>
        public ProductStatusDetail ProductStatusDetail { get; set; }

        /// <summary>
        /// PropertyDetails
        /// </summary>
        public List<PropertySetup> PropertyDetails { get; set; }
    }
}
