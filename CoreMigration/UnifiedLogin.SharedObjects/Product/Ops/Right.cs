using Newtonsoft.Json;
using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Product.Ops
{
    /// <summary>
    /// An Ops right/permission
    /// </summary>
    public class OpsRight
    {
        /// <summary>
        /// The id of the  group
        /// </summary>
        [JsonProperty("group_id")]
        public string GroupID { get; set; }

        /// <summary>
        /// Used to store the name of the right
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The title of the  right
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// The description of the right
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// The value of the  right
        /// </summary>
        [JsonProperty("value")]
        public string Value { get; set; }


        /// <summary>
        /// The default value of the  right
        /// </summary>
        [JsonProperty("default_value")]
        public string DefaultValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("possible_values")]
        public List<object> PossibleValues { get; set; }

        /// <summary>
        /// isAssigned
        /// </summary>

        public bool isAssigned { get; set; }
    }
}
