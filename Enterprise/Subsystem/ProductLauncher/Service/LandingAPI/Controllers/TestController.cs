using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
    public class TestController : ApiController
    {
        [AllowAnonymous]
        [Route("test/testapi")]
        [HttpGet]
        public HttpResponseMessage GetSuccessResult()
        {
            return Request.CreateResponse(HttpStatusCode.OK, Guid.NewGuid());
        }
    }
}
