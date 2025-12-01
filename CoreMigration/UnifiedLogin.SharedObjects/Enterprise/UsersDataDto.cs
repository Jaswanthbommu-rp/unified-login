#nullable enable
using Newtonsoft.Json;
using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Enterprise
{
    public class UsersDataDto : UserDataDtoCommon
    {
        [JsonProperty(PropertyName = "CustomFields")]
        public Dictionary<string, string> CustomFields { get; set; } = new();

        [JsonProperty("UserStatus")]
        public string? UserStatus { get; set; }

        [JsonProperty(PropertyName = "UserType")]
        public string? UserType { get; set; }

        [JsonProperty("Product")]
        public IList<UserProductSAMLDetail>? Product { get; set; }
    }
}
