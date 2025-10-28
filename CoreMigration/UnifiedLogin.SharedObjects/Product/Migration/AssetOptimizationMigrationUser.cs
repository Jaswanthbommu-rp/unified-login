using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.Product.Migration
{
    /// <summary>
    /// 
    /// </summary>
    public class AssetOptimizationMigrationUser
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// 
        [JsonProperty("fname")]
        public string FirstName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// 
        [JsonProperty("lname")]
        public string LastName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("username")]
        public string UserName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("status")]
        public string Status { get; set; } 
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("userId")]
        public string UserId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("companysourceinstanceid")]
        public string CompanySourceInstanceId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("lastactivity")]
        public DateTime? Activity { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("products")]
        public IList<string> Products { get; set; }

    }
}
