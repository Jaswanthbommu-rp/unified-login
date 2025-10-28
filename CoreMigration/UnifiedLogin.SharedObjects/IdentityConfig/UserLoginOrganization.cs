using Newtonsoft.Json;
using System;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.IdentityConfig
{
    public class UserLoginOrganization
    {
        /// <summary>
        /// User RealPageId
        /// </summary>
        [JsonProperty(PropertyName = "UserRealPageId")]
        public Guid UserRealPageId { get; set; }

        /// <summary>
        /// Organization RealPageId
        /// </summary>
        [JsonProperty(PropertyName = "OrganizationRealPageId")]
        public Guid OrganizationRealPageId { get; set; }
    }
}
