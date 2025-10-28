using Newtonsoft.Json;
using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Product.Migration
{
    /// <summary>
    /// 
    /// </summary>
    public class ClientPortalMigrationResponse
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("totalSize")]
        public int TotalSize { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("records")]
        public IList<ClientPortalMigrationUser> Records { get; set; }
    }
    
   
}
