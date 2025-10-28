using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.Product
{
	public class ProductPropertyGroup
	{
        /// <summary>
        /// The id of the property in the product
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// The name of the property in the product
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }
        /// <summary>
        /// Is the property assigned to the users
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsAssigned { get; set; } = false;

        /// <summary>
        /// property assigned to the group
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> AssignedProperties { get; set; } 
    }
}
