using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using System;
using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing
{
    /// <summary>
    /// 
    /// </summary>
    public class ProductUserAccountDetails
    {
        /// <summary>
        /// Product Name
        /// </summary>
        public ProductEnum ProductName { get; set; }

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
        /// Editor PersonaId
        /// </summary>
        public long LoggedInUserPersonaId { get; set; }

        /// <summary>
        /// Editor RealpageId
        /// </summary>
        public Guid LoggedInUserRealPageId { get; set; }
    }
}
