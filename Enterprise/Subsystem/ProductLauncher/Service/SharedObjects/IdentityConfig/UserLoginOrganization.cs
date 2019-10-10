using Newtonsoft.Json;
using System;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects
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
