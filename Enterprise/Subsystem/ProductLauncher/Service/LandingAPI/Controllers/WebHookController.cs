using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.WebHook;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Linq;
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

            string signingSecret = "7EFE59F38D17D83721E241D27638FED0";
            string signature = Request.Headers.FirstOrDefault(h => h.Key == "signature").Value.FirstOrDefault();

            string requestBody = Request.Properties["TibcoPostData"] as string;
            var hashed = SHA.GenerateHMACSHA256String(signingSecret, requestBody);
            if (!string.Equals(signature, hashed, StringComparison.CurrentCultureIgnoreCase))
            {
                throw new UnauthorizedAccessException("Invalid Signature.");
            }
            var response = Request.CreateResponse(HttpStatusCode.Accepted, result);
            return Task.FromResult(response);

        }
    }
}
