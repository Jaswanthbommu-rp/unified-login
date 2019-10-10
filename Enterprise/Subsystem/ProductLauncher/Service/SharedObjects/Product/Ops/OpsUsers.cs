using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Ops;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing.Product.Ops
{
    /// <summary>
    /// 
    /// </summary>
    public class OpsUsers
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("pagination")]
        public OpsPagination Pagination { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("user_list")]
        public IList<OpsUser> UserList { get; set; }
    }
}
