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
    /// 
    public class AOMigrateResponse
    {
        /// <summary>
        /// User Id
        /// </summary>
        /// 
        [JsonProperty("userId")]
        public string UserId { get; set; }
        /// <summary>
        ///
        /// </summary>
        /// 
        /// <summary>
        /// Status
        /// </summary>
        [JsonProperty("success")]
        public bool Status { get; set; }
    }
}
