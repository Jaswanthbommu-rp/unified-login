using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.Landing
{
    public class UserEmployee : IUserEmployeeId
    {
        [JsonProperty("UserEmployeeId")]
        public int UserEmployeeId { get; set; }

        [JsonProperty("UserLoginPersonaId")]
        public long UserLoginPersonaId { get; set; }

        [JsonProperty("EmployeeId")]
        public string EmployeeId { get; set; }
    }
}
