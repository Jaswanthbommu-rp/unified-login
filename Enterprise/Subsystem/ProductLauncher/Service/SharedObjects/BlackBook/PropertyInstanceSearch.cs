using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook
{
    /// <summary>
    /// PropertyInstanceSearch
    /// </summary>
    public class PropertyInstanceSearch
    {
        /// <summary>
        /// The customerProperty
        /// </summary>
        public CustomerProperty CustomerProperty { get; set; }

        /// <summary>
        /// propertyInstance
        /// </summary>
        public List<PropertySetup> PropertyInstance { get; set; }

        public List<string> Domain { get; set; }
    }
}
