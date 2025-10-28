using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.Product.Accounting
{    
    public class ACProperty
    {
        /// <summary>
        /// Company Id
        /// </summary>
        public string CompanyId { get; set; }
        /// <summary>
        /// Company Name
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// Id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Property Id
        /// </summary>
        public string PropertyId { get; set; }
        /// <summary>
        /// Company Name
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// Is Selected
        /// </summary>
        public bool IsAssigned { get; set; }  = false;

        /// <summary>
        /// Is Selected
        /// </summary>
        public bool DisableSelection { get; set; } = false;

        /// <summary>
        /// Property Id
        /// </summary>
        public string MConsoleId { get; set; } = string.Empty;

        /// <summary>
        /// Is Selected
        /// </summary>
        public bool IsCompanyAssigned { get; set; } = false;

        /// <summary>
        /// The UPFM property instance id
        /// </summary>
        public string InstanceId { get; set; }

        /// <summary>
        /// BookID
        /// </summary>
        public string BookID { get; set; }
    }
}
