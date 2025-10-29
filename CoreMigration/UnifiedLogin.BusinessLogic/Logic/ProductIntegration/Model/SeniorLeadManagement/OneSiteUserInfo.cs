using Newtonsoft.Json;
using System.Collections.Generic;

namespace UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model.SeniorLeadManagement
{
    /// <summary>
    /// Model for One site user info
    /// </summary>
    public sealed class OneSiteUserInfo
    {
        #region "Properties"
        [JsonProperty(PropertyName = "userId", NullValueHandling = NullValueHandling.Ignore)]
        public int UserId { get; set; }

        [JsonProperty(PropertyName = "firstName", NullValueHandling = NullValueHandling.Ignore)]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "lastName", NullValueHandling = NullValueHandling.Ignore)]
        public string LastName { get; set; }

        public List<string> Properties { get; set; }

        #endregion
    }
}
