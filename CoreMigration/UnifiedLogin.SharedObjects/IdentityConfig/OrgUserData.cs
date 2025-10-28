using Newtonsoft.Json;
using UnifiedLogin.SharedObjects.Enum;
using System;
using System.Collections.Generic;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.IdentityConfig
{
    /// <summary>
    /// Organization User Data with UserLoginOnly data
    /// </summary>
    public class OrgUserData : UserLoginOnly
    {
        /// <summary>
        /// OrganizationPartyId
        /// </summary>
        [JsonProperty(PropertyName = "OrganizationPartyId")]
        public long OrganizationPartyId { get; set; }

    }
}

