using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.WebHook;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
    public class WebHookController : BaseApiController
    {
        [HttpPost]
        [AllowAnonymous]
        public Task<HttpResponseMessage> PostBooks(ThinEvent thinEvent)
        {

            //"books.customerproperty.deleted";
            //"books.customerproperty.updated";

            //"books.customercompany.deleted"
            //"books.customercompany.updated"


            string result = "OK";

            var response = Request.CreateResponse(HttpStatusCode.OK, result);
            return Task.FromResult(response);

        }
    }
}
