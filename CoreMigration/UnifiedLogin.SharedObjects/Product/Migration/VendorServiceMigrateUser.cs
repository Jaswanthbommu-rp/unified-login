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
    /// 
    /// </summary>
    public class VendorServiceMigrateUser
    {
        /// <summary>
        /// Id
        /// </summary>
        [JsonProperty("Id")]
        public string Id { get; set; }

        /// <summary>
        /// Company Id
        /// </summary>
        [JsonProperty("CompanyId")]
        public string CompanyId { get; set; }

        /// <summary>
        /// Unified Login User
        /// </summary>
        /// 
        [JsonProperty("unifiedLoginUserName")]
        public string UnifiedLoginUserName { get; set; }

        /// <summary>
        /// Is Unified Login User
        /// </summary>
        /// 
        [JsonProperty("usingUnifiedLogin")]
        public bool UsingUnifiedLogin { get; set; }

    }
}
