using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.Product.Rum
{
    public class RumPropertyGroup
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string State { get; set; }
        public bool IsAssigned { get; set; }

        /// <summary>
        /// The UPFM property instance id
        /// </summary>
        public string InstanceId { get; set; }
    }

    /// <summary>
    /// Access type enum
    /// </summary>
    public enum AccessTypeEnum
    {
        Property,
        Group,
        Region
    }
}
