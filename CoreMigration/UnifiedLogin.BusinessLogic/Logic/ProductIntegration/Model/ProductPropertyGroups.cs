using Newtonsoft.Json;

namespace UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model
{
    /// <summary>
    /// Property group class - also for region
    /// </summary>
    public class ProductPropertyGroups
    {
        private string _groupId;
        private string _groupName;

        [JsonProperty(PropertyName = "id")]
        public string GetGroupId => _groupId;

        [JsonProperty(PropertyName = "groupId")]
        public string SetGroupId
        {
            set { _groupId = value; }
        }

        [JsonProperty(PropertyName = "name")]
        public string GetGroupName => _groupName;

        [JsonProperty(PropertyName = "groupName")]
        public string SetGroupName
        {
            set { _groupName = value; }
        }
	    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string GroupType { get; set; }
        public bool IsAssigned { get; set; }
    }
}