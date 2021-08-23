using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing.Security
{
    /// <summary>
    /// Route Security
    /// </summary>
    public class RouteSecurity
    {
        #region Ctor
        /// <summary>
        /// Route Security Constructor
        /// </summary>
        public RouteSecurity()
        {
            Rights = new List<string>();
        }
        #endregion

        /// <summary>
        /// Route Id
        /// </summary>
        public string RouteId { get; set; }

        /// <summary>
        /// List of all rights associated with route
        /// </summary>
        public IList<string> Rights { get; set; }

        public RouteSecurity ImpersonatedRights { get; set; }
    }
}
