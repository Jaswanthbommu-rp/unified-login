using Newtonsoft.Json;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing
{
    public class SecurityQuestionAnswer
    {
        [JsonProperty(PropertyName = "questionId")]
        public int SecurityQuestionId { get; set; }

        [JsonProperty(PropertyName = "answer")]
        public string Answer { get; set; }
    }
}
