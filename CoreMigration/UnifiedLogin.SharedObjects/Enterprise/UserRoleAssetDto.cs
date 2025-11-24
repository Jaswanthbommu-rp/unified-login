#nullable enable
using Newtonsoft.Json;
using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Enterprise
{
    public class UserRoleAssetDto
    {
        [JsonProperty(PropertyName = "ProductRole")]
        public IList<ProductRole> ProductRole { get; set; } = new List<ProductRole>();

        [JsonProperty(PropertyName = "AssetGroup")]
        public IList<AssetGroup> AssetGroups { get; set; } = new List<AssetGroup>();
    }
}
