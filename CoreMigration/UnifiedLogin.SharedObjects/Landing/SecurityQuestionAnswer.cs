using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.Landing
{
    public class SecurityQuestionAnswer
    {
        [JsonProperty(PropertyName = "questionId")]
        public int SecurityQuestionId { get; set; }

        [JsonProperty(PropertyName = "answer")]
        public string Answer { get; set; }
    }
}
