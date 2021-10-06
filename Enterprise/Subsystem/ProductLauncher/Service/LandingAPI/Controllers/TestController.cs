using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Services;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
    public class TestController : ApiController
    {
        [AllowAnonymous]
        [Route("test/testapi")]
        [HttpGet]
        public HttpResponseMessage GetSuccessResult()
        {
            ManageMicrosoftAzure az = new ManageMicrosoftAzure(new DefaultUserClaim());
            var user = az.GetADUserInfo("james.reames@realpage.com");
            
            return Request.CreateResponse(HttpStatusCode.OK, Guid.NewGuid());
        }
    }
}
