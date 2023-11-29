using Newtonsoft.Json;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.ResponseObject
{
    public class Error
    {
        public string StatusCode { get; set; }
        public string Source { get; set; }
        public string Title { get; set; }
        public string Detail { get; set; }
    }
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