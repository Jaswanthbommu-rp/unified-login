#nullable enable
using Newtonsoft.Json;
using UnifiedLogin.SharedObjects.Product.Ops;

namespace UnifiedLogin.SharedObjects.Enterprise
{
    public class OpsAssetGroupDto : AssetGroup
    {
        [JsonIgnore]
        public bool IsAssigned { get; set; }
    }
}
