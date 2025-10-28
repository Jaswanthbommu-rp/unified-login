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
    public class MigrationProperty
    {
        /// <summary>
        /// The user property
        /// </summary>
        [JsonProperty("PropertyInstanceSourceId")]
        public string PropertyInstanceSourceId { get; set; }

        /// <summary>
        /// The user property
        /// </summary>
        [JsonProperty("propertyId")]
        private string PropertyId
        {
            set
            {
                PropertyInstanceSourceId = value;
            }
        }

        /// <summary>
        /// The user property
        /// </summary>
        [JsonProperty("PropertySourceInstanceId")]
        private string PropertySourceInstanceId
        {
            set
            {
                PropertyInstanceSourceId = value;
            }
        }
    }
}
