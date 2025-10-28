using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Product.Ops
{
    public class RightGroup
    {
        /// <summary>
        /// The id of the asset group
        /// </summary>
        [JsonProperty("group_list")]
        public IList<Group> GroupList { get; set; }

        /// <summary>
        /// The name of the asset group
        /// </summary>
        [JsonProperty("responsibility_list")]
        public IList<OpsRight> ResponsibilityList { get; set; }
    }

   
}
