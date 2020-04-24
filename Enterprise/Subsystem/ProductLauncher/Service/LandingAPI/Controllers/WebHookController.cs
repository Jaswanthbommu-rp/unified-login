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
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
    public class WebHookController : BaseApiController
    {
        private OrganizationRepository _organizationRepository;
        public WebHookController()
        {
            _organizationRepository = new OrganizationRepository();
        }

        public WebHookController(IRepository repository)
        {
            _organizationRepository = new OrganizationRepository(repository);
        }


        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.Accepted, Description = "Received book update successfully", Type = typeof(ThinEvent<string>))]

        [HttpPost]
        [AllowAnonymous]
        [Route("webhook/books")]
        public HttpResponseMessage PostBooks([FromBody] ThinEvent<JToken> thinEvent)
        {
            string signingSecret = "7EFE59F38D17D83721E241D27638FED0";
            string signature = Request.Headers?.FirstOrDefault(h => h.Key == "signature").Value?.FirstOrDefault();
            
            string requestBody = Request.Properties?["TibcoPostData"] as string;

            if (signature != null && requestBody != null)
            {
                var hashed = SHA.GenerateHMACSHA256String(signingSecret, requestBody);
                //if (!string.Equals(signature, hashed, StringComparison.CurrentCultureIgnoreCase))
                //{
                //    throw new UnauthorizedAccessException("Invalid Signature.");
                //}
            }
            //using (var stream = new MemoryStream())
            //{
                //var context = (HttpContextBase)Request.Properties["MS_HttpContext"];
                //var context = (Microsoft.Owin.OwinRequest)Request.Properties["MS_RequestContext"];
                //var type = Request.Properties["MS_RequestContext"].GetType();

                //context.Context.Request.Body.Seek(0, SeekOrigin.Begin);
                //string requestBody2 = Encoding.UTF8.GetString(context.Context.Request.Body..ToArray());

                //context.HttpContext.Request.InputStream.CopyTo(stream);
                
            //}
            

            var response = Request.CreateResponse(HttpStatusCode.Accepted);
            try
            {
                switch (thinEvent.Topic.ToLowerInvariant())
                {
                    case "books.customerproperty.deleted":
                        var customerPropertyIdDeleted = thinEvent.Payload?["payload"]["customerPropertyId"];
                        break;

                    case "books.customerproperty.updated":
                        var customerPropertyIdUpdates = thinEvent.Payload?["payload"]["customerPropertyId"];

                        break;
                    case "books.customercompany.deleted":
                        var customerCompanyIdDeleted = Convert.ToInt64(thinEvent.Payload?["payload"]["customerCompanyId"]);
                        var organization = _organizationRepository.GetOrganization(blueBookId: customerCompanyIdDeleted);
                        if (organization != null)
                        {
                            var newCustomerCompanyId = Convert.ToInt64(thinEvent.Payload?["payload"]["replacementCustomerCompanyId"]);
                            Organization newOrganization = new Organization() {PartyId = organization.PartyId, BooksCustomerMasterId = newCustomerCompanyId};
                            RepositoryResponse result = _organizationRepository.UpdateOrganizationBooksCompanyMasterId(organization, newOrganization);
                            if (result.ErrorMessage.Length != 0 || result.Id == 0)
                            {
                                response = Request.CreateResponse(HttpStatusCode.BadRequest, ResultErrorMessage(result));
                            }
                        }
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
            
            return response;

        }

        private static string ResultErrorMessage(RepositoryResponse result)
        {
            var errorMessage = result.ErrorMessage;
            if (result.Id != 0) return errorMessage;
            if (errorMessage.Length > 0)
            {
                errorMessage += " ";
            }

            errorMessage += "id not updated";
            return errorMessage;
        }
    }
}
