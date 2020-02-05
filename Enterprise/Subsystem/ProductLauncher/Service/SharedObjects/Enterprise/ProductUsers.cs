using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enterprise
{
    /// <summary>
    /// List of users by company and products
    /// </summary>
    public class ProductUsers
    {
        /// <summary>
        /// UserId
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// LoginName
        /// </summary>
        public string LoginName { get; set; }

        /// <summary>
        /// FirstName
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// LastName
        /// </summary>
        public string LastName { get; set; }

    }
}
