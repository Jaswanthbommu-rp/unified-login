using Newtonsoft.Json;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing
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
