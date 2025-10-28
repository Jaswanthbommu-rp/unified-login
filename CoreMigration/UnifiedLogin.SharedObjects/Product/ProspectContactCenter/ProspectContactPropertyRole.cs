using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Product.ProspectContactCenter
{
    public class ProspectContactPropertyRole
    {

        public IList<string> RoleList { get; set; }
        public IList<string> PropertyList { get; set; }

        /// <summary>
        /// Is product assigned or removed
        /// </summary>
        public bool IsAssigned { get; set; }
    }
}
