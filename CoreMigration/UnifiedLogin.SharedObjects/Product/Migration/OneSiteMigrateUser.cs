using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Product.Migration
{
    public class OneSiteMigrateUser: MigrationUser
    {
        /// <summary>
        /// The Reference Number of the user
        /// </summary>
        [JsonProperty("ReferenceNumber")]
        public string ReferenceNumber { get; set; }
    }
}
