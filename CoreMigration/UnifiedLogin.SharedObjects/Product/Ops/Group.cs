using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.Product.Ops
{

 /// <summary>
        /// Used to store information about an Ops  group
        /// </summary>
        public class Group
        {
            /// <summary>
            /// The id of the  group
            /// </summary>
            [JsonProperty("group_id")]
            public string GroupID { get; set; }

            /// <summary>
            /// The name of the  group
            /// </summary>
            [JsonProperty("group_name")]
            public string GroupName { get; set; }

            /// <summary>
            /// The parent id of the  group
            /// </summary>
            [JsonProperty("parent_group_id")]
            public string ParentGroupId { get; set; }
            
        }
    

}
