using System;

namespace RP.Enterprise.Subsystem.ProductLauncher.WinService.ProductAssigner.Model
{
    /// <summary>
    /// Product details for a user 
    /// </summary>
    public class ProductUserProperitiesRoles
    {
        /// <summary>
        /// Product batch Id
        /// </summary>
        public int ProductBatchId { get; set; }

        /// <summary>
        /// Create User RealPage Id
        /// </summary>
        public Guid RealPageId { get; set; }
        /// <summary>
        /// Product Name
        /// </summary>
        public ProductEnum ProductName { get; set; }

        /// <summary>
        /// Person who is creating or editing user
        /// </summary>
        public long CreateUserPersonaId { get; set; }

        /// <summary>
        /// Person getting created or edited
        /// </summary>
        public long AssignUserPersonaId { get; set; }

        /// <summary>
        /// Input json param values
        /// </summary>
        public string InputJson { get; set; }
    }
}
