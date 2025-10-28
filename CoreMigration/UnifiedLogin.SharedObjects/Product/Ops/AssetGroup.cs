using Newtonsoft.Json;
using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Product.Ops
{
	/// <summary>
	/// Used to store information about an Ops asset group
	/// </summary>
	public class AssetGroup : AssetGroupCommon
    {
        /// <summary>
        /// The id of the asset group
        /// </summary>
        [JsonProperty("id")]
        public string ID { get; set; }
        
        /// <summary>
        /// The Code of the asset group
        /// </summary>
        [JsonProperty("code", NullValueHandling = NullValueHandling.Ignore)]
        public string Code { get; set; }

        /// <summary>
        /// The group type of the asset group
        /// </summary>
        [JsonProperty("group_type", NullValueHandling = NullValueHandling.Ignore)]
        public string GroupType { get; set; }

        /// <summary>
        /// The id of the asset
        /// </summary>
        [JsonProperty("asset_id", NullValueHandling = NullValueHandling.Ignore)]
        public string AssetID { get; set; }

        /// <summary>
        /// The group type of the asset group
        /// </summary>
        [JsonProperty("isAssigned", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsAssigned { get; set; }

        /// <summary>
        /// Asset group property list
        /// </summary>
        [JsonProperty("property_list", NullValueHandling = NullValueHandling.Ignore)]
        public IList<Portfolio> property_list { get; set; }       

        /// <summary>
        /// The UPFM property instance id
        /// </summary>
        public string InstanceId { get; set; }
    }
}
