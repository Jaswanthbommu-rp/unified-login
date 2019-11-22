using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using System;
using System.Collections.Generic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects
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

