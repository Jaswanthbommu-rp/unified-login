using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.Product.Accounting
{
    public class RightsAddRemove
    {
        /// <summary>
        /// A list of rights to add to the role
        /// </summary>
        public List<ProductRightAcct> RightsToAdd;
        /// <summary>
        /// A list of rights to remove from the role
        /// </summary>
        public List<ProductRightAcct> RightsToRemove;
    }
}
