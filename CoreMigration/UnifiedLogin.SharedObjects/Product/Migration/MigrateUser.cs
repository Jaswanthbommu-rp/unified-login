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
    public class MigrateUser
    {
        /// <summary>
        /// User Id
        /// </summary>
        /// 
        [JsonProperty("userId")]
        public string UserId { get; set; }
        
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

		/// <summary>
		/// Unified Login User
		/// </summary>
		/// 
		[JsonProperty("leadEmailAddress")]
		public string LeadEmailAddress { get; set; }
	}   
}
