using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.WebHook;
using Swashbuckle.Swagger.Annotations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
    public class WebHookController : BaseApiController
    {
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.Accepted, Description = "Received book update successfully", Type = typeof(ThinEvent<string>))]

        [HttpPost]
        [AllowAnonymous]
        [Route("webhook/books")]
        public Task<HttpResponseMessage> PostBooks(ThinEvent<string> thinEvent)
        {

            //"books.customerproperty.deleted";
            //"books.customerproperty.updated";

            //"books.customercompany.deleted"
            //"books.customercompany.updated"


            string result = "OK";

            var response = Request.CreateResponse(HttpStatusCode.Accepted, result);
            return Task.FromResult(response);

        }
    }
}
