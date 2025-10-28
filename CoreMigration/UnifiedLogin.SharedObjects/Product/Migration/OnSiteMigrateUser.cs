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
    /// OnSite Migrate Users
    /// </summary>
    public class OnSiteMigrateUsers
    {
        /// <summary>
        /// Users
        /// </summary>
        [JsonProperty("users")]
        public IList<OnSiteMigrateUser> Users { get; set; }
    }

    /// <summary>
    /// OnSite Migrate User
    /// </summary>
    public class OnSiteMigrateUser
    {
        /// <summary>
        /// Id
        /// </summary>
        [JsonProperty("user_id")]
        public string UserId { get; set; }
    }
}
