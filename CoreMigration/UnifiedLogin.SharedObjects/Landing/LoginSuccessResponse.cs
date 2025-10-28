using Newtonsoft.Json;
using System.Net;
using System.Net.Http;

namespace UnifiedLogin.SharedObjects.Landing
{
    public class LoginSuccessResponse : ResponseBase
	{

        [JsonProperty(PropertyName = "enterpriseUserName")]
        public string EnterpriseUserName { get; set; }


        [JsonProperty(PropertyName = "isSuccess")]
        public bool IsSuccess { get; set; }

		HttpStatusCode HttpStatusCode { get; set; }		
	}
}
