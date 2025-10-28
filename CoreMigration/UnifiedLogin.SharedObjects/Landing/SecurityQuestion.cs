using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Landing
{
    /// <summary>
    /// 
    /// </summary>
    public class SecurityQuestion
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(PropertyName = "questionId")]
        public int SecurityQuestionId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(PropertyName = "question")]
        public string Question { get; set; } = "Select Security Question";

        /// <summary>
        /// Example for Security Question List
        /// </summary>
        /// <returns>List of Security Questions</returns>
        public static IList<SecurityQuestion> SecurityQuestionListExample()
        {
            return new List<SecurityQuestion>()
            {
                new SecurityQuestion(),
                new SecurityQuestion(),
                new SecurityQuestion()
            };
        }
    }
}
