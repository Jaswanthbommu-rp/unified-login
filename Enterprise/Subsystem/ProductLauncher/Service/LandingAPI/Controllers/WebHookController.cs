using Newtonsoft.Json.Linq;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.WebHook;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Routing;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
    public class WebHookController : BaseApiController
    {
        private OrganizationRepository _organizationRepository;
        private ProductInternalSettingRepository _productInternalSettingRepository;

        public WebHookController()
        {
            _organizationRepository = new OrganizationRepository();
            _productInternalSettingRepository = new ProductInternalSettingRepository();
        }

        public WebHookController(IRepository repository)
        {
            _organizationRepository = new OrganizationRepository(repository);
            _productInternalSettingRepository = new ProductInternalSettingRepository(repository);
        }
        
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.Accepted, Description = "Received book event successfully", Type = typeof(ThinEvent<string>))]
        [HttpPost]
        [AllowAnonymous]
        [Route("webhook/books")]
        public HttpResponseMessage PostBooks([FromBody] ThinEvent<JToken> thinEvent)
        {
            var response = Request.CreateResponse(HttpStatusCode.Accepted);
            string signature = Request.Headers?.FirstOrDefault(h => h.Key == "signature").Value?.FirstOrDefault();
            
            if (signature != null && Request.Properties?["TibcoPostData"] is string requestBody)
            {
                string signingSecret = GetTiboWebHookSigningSecret();
                var hashed = SHA.GenerateHMACSHA256String(signingSecret, requestBody);
                if (!string.Equals(signature, hashed, StringComparison.CurrentCultureIgnoreCase))
                {
                    response = Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid Signature.");
                    return response;
                }
            }

            if (thinEvent == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Missing Content.");
            }

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
                            if (newCustomerCompanyId != 0)
                            {
                                Organization newOrganization = new Organization() {PartyId = organization.PartyId, BooksCustomerMasterId = newCustomerCompanyId};
                                RepositoryResponse result = _organizationRepository.UpdateOrganizationBooksCompanyMasterId(organization, newOrganization);
                                if (result.ErrorMessage.Length != 0 || result.Id == 0)
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, ResultErrorMessage(result));
                                }
                            }
                        }
                        break;

                    case "books.customercompany.updated":
                        var customerCompanyIdUpdated = thinEvent.Payload["payload"]["customerCompanyId"];
                        break;

                    default:
                        return Request.CreateResponse(HttpStatusCode.Accepted);
                }
            }
            catch (Exception ex)
            {
                response = Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }

            return response;

        }

        /// <summary>
        /// Used to get the signing secret used to validate Tibco WebHook events
        /// </summary>
        /// <returns>The list of settings</returns>
        public string GetTiboWebHookSigningSecret()
        {
            IList<ProductInternalSetting> productInternalSettingList = new List<ProductInternalSetting>();
            RPObjectCache rpcache = new RPObjectCache();
            var cacheKey = "productInternalSetting_" + (int)ProductEnum.UnifiedLogin;
            productInternalSettingList = rpcache.GetFromCache<IList<ProductInternalSetting>>(cacheKey, 60, () =>
            {
                // load from database
                return _productInternalSettingRepository.GetProductInternalSettings((int)ProductEnum.UnifiedLogin);
            });
            
            string signingSecret = signingSecret = productInternalSettingList?.ToList().FirstOrDefault(s => s.Name.Equals("TiboWebHookSigningSecret", StringComparison.OrdinalIgnoreCase))?.Value;

            return signingSecret ?? "";
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
