using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Lead2Lease
{
    /// <summary>
    /// Used when creating a combined OneSite and Lead2Lease user because the Lead2Lease user must be created after the OneSite user is created
    /// </summary>
    public class Lead2LeaseOneSiteProduct
    {
        /// <summary>
        /// OneSite property and role info
        /// </summary>
        public RolePropertyList OneSite { get; set; }
        /// <summary>
        /// Lead2Lease property and role info
        /// </summary>
        public RolePropertyList Lead2Lease { get; set; }

    }
}
