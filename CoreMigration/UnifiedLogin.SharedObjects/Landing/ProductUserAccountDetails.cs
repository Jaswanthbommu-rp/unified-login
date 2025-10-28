using UnifiedLogin.SharedObjects.Enum;
using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Landing
{
    /// <summary>
    /// 
    /// </summary>
    public class ProductUserAccountDetails
    {
        /// <summary>
        /// Product Name
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Persona id
        /// </summary>
        public long PersonaId { get; set; }

        /// <summary>
        /// Product status for the user
        /// </summary>
        public ProductBatchStatusType ProductStatus { get; set; }

        /// <summary>
        /// Product setting list
        /// </summary>
        public Dictionary<SamlAttributeEnum, string> ProductSettings { get; set; }

        /// <summary>
        /// Sub product name for the main product (E.g. Main Product - ProductName=AO, Sub Product - InvestmentAnalytics, Business Intelligence etc)
        /// </summary>
        public IList<string> SubProducts { get; set; }

        /// <summary>
        /// Employee id
        /// </summary>
        public string EmployeeId { get; set; }
        
        /// <summary>
        /// Origin
        /// </summary>
        public string Origin { get; set; } = string.Empty;
    }
}
