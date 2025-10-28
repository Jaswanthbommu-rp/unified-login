using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Hots
{
    public class HotsUser
    {
        public long BaselineUserId { get; set; }
        public string BaselineUserName { get; set; }
        public long CloneUserId { get; set; }
        public string CloneUserName { get; set; }
        public string ClonePassword { get; set; }
        
        [JsonIgnore]
        public long ClonePersonaId { get; set; }

        [JsonIgnore] public List<int> CloneProducts { get; set; } = new List<int>();
    }

    public class ClonedUsers
    {
        public string Status { get; set; }
        public Guid CloneCustomerCompanyId { get; set; }
        public string CloneCustomerEnvironment { get; set; }
        public List<HotsUser> Users { get; set; }
    }
}
