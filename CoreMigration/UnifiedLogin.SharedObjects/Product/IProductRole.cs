using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Product
{
    /// <summary>
    /// Used to store role information
    /// </summary>
    public interface IProductRole
    {
        /// <summary>
        /// The unique id of the role in the product
        /// </summary>
        string ID { get; }
        /// <summary>
        /// The name of the role in the product
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Is the role assigned to the user in the product
        /// </summary>
        bool IsAssigned { get; }

        /// <summary>
        /// The type of role
        /// </summary>
        string Roletype { get; set; }
        /// <summary>
        /// The number of rights assigned to the role
        /// </summary>
        string RightsAssigned { get; set; }
    }
}
