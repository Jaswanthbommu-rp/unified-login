using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.ResponseObject
{
    public class FieldErrors
    {
        public RoleError Error { get; set; }
    }
    public class RoleErrors
    {
        [JsonProperty(PropertyName = "fieldErrors")]
        public FieldErrors FieldErrors { get; set; }
    }
    public class RoleError
    {
        [JsonProperty(PropertyName = "msgCode")]
        public string MessageCode { get; set; }
        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
    }
}
