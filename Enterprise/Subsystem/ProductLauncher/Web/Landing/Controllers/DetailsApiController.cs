using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Web.Landing.Helper;
using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.Landing.Controllers
{
    [RoutePrefix("api/home")]
    public class DetailsApiController : ApiController
    {
        [Authorize]
        [HttpGet]
        [Route("getuserproductdetails")]
        public UserOutput GetUserProductDetails()
        {
            ClaimsPrincipal currentClaimPrincipal = ClaimsPrincipal.Current;
            // used to get a client token if needed
            //var tResp = ManagerApiHelper.GetAuthToken().Result;
            //var token = Request.["access_token"];
            var token = "";
            if (Request.Properties.ContainsKey("MS_OwinContext"))
            {
                var owinContext = Request.Properties["MS_OwinContext"] as Microsoft.Owin.OwinContext;
                if (owinContext.Request.Cookies["access_token"] != null)
                {
                    token = owinContext.Request.Cookies["access_token"];
                }
            }
            
            int userId = Convert.ToInt32((from nvp in currentClaimPrincipal.Claims where nvp.Type == "sub" select nvp.Value).FirstOrDefault());
            var queryInfo = string.Format("api/Dashboard/GetUserProductDetails?enterpriseUserId={0}", userId);
            var userProduct = new UserProduct();
            try
            {
				// is this still used?
                //userProduct = ManagerApiHelper.GetResultFromApiAsync<UserProduct>(token, queryInfo, true).Result;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(((HttpResponseException)ex.InnerException).Response);
            }

            UserOutput output = new UserOutput() { user = userProduct };
            return output;
        }

        public class UserOutput
        {
            [JsonProperty(PropertyName = "user")]
            public UserProduct user { get; set; }
        }
    }
}
