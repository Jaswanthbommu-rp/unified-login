using System;
using System.IO;
using System.Linq;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.WebHook;
using Swashbuckle.Swagger.Annotations;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Routing;
using Microsoft.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
        public Task<HttpResponseMessage> PostBooks([FromBody] ThinEvent<JToken> thinEvent)
        {
            string signingSecret = "7EFE59F38D17D83721E241D27638FED0";
            string signature = Request.Headers?.FirstOrDefault(h => h.Key == "signature").Value?.FirstOrDefault();
            
            //string requestBody = Request.Properties["TibcoPostData"] as string;
            //var hashed = SHA.GenerateHMACSHA256String(signingSecret, requestBody);
            //if (!string.Equals(signature, hashed, StringComparison.CurrentCultureIgnoreCase))
            //{
            //    throw new UnauthorizedAccessException("Invalid Signature.");
            //}
            using (var stream = new MemoryStream())
            {
                //var context = (HttpContextBase)Request.Properties["MS_HttpContext"];
                //var context = (Microsoft.Owin.OwinRequest)Request.Properties["MS_RequestContext"];
                //var type = Request.Properties["MS_RequestContext"].GetType();

                //context.Context.Request.Body.Seek(0, SeekOrigin.Begin);
                //string requestBody2 = Encoding.UTF8.GetString(context.Context.Request.Body..ToArray());

                //context.HttpContext.Request.InputStream.CopyTo(stream);
                string requestBody = Encoding.UTF8.GetString(stream.ToArray());
            }


            var response = Request.CreateResponse(HttpStatusCode.Accepted);
            try
            {
                switch (thinEvent.Topic.ToLowerInvariant())
                {
                    case "books.customerproperty.deleted":
                        var customerPropertyIdDeleted = thinEvent.Payload["payload"]["customerPropertyId"];
                        break;

                    case "books.customerproperty.updated":
                        var customerPropertyIdUpdates = thinEvent.Payload["payload"]["customerPropertyId"];

                        break;
                    case "books.customercompany.deleted":
                        var customerCompanyIdDeleted = thinEvent.Payload["payload"]["customerCompanyId"];

                        break;
                    case "books.customercompany.updated":
                        var customerCompanyIdUpdated = thinEvent.Payload["payload"]["customerCompanyId"];
                        break;

                    default:
                        response = Request.CreateResponse(HttpStatusCode.BadRequest);
                        break;
                }
            }
            catch (Exception ex)
            {
                response = Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }

            //response = Request.CreateResponse(HttpStatusCode.BadRequest);

            
            return Task.FromResult(response);

        }
    }
}
