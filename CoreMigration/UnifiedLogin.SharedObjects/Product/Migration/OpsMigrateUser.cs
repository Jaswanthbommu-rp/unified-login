using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Product.Migration
{
    /// <summary>
    /// Migrate User
    /// </summary>
    public class OpsMigrateUser
    {
        /// <summary>
        /// Migrate User
        /// </summary>
          /// <summary>
            /// User Id
            /// </summary>
            /// 
            [JsonProperty("user_id")]
            public string UserId { get; set; }

            /// <summary>
            /// Unified Login User
            /// </summary>
            /// 
            [JsonProperty("unify_login_name")]
            public string UnifiedLoginUserName { get; set; }

            /// <summary>
            /// Is Unified Login User
            /// </summary>
            /// 
            [JsonProperty("unify_login_flag")]
            public int UsingUnifiedLogin { get; set; }
        }
    
}
