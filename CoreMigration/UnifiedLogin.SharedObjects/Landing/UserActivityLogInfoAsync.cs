using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UnifiedLogin.SharedObjects.Landing
{
    public class UserActivityLogInfoAsync
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Guid RealPageId { get; set; }
        public string LoginName { get; set; }
        public long BooksOrganizationMasterId { get; set; }
        public long OrganizationPartyId { get; set; }
        public string OrganizationName { get; set; }
        public long UserId { get; set; }
        public string ClientCode { get; set; } = null;
        public Guid OrganizationRealpageId { get; set; }
        [JsonProperty("CreateUserSourceType", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string CreateUserSourceType { get; set; }
    }
}
