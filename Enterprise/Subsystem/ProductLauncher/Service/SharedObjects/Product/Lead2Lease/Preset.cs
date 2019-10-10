using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Lead2Lease
{
    /// <summary>
    /// Used to store preset information for Lead2Lease
    /// </summary>
    public class Preset
    {
        /// <summary>
        /// The id of the preset
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The name of the preset
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The role ids that are associated to the preset
        /// </summary>
        public List<int> RoleIds { get; set; }
    }
}
