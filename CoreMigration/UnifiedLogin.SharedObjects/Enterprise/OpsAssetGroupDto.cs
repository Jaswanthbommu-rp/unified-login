#nullable enable
using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.Enterprise
{
    public class OpsAssetGroupDto : AssetGroup
    {
        [JsonIgnore]
        public bool IsAssigned { get; set; }
    }
}
