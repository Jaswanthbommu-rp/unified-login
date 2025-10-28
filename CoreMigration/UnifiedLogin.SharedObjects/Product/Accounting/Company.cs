using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.Product.Accounting
{
   public class ACCompany
    {
        /// <summary>
        /// Company Id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Company Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Is Selected
        /// </summary>
        public bool isAssigned { get; set; } = false;
    }
}
