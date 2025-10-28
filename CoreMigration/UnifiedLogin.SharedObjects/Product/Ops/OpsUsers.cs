using Newtonsoft.Json;
using UnifiedLogin.SharedObjects.Product.Ops;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Product.Ops
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
