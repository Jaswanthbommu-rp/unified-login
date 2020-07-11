using Newtonsoft.Json.Linq;
using RP.Enterprise.Foundation.Audit.Core.Component;
using RP.Enterprise.Foundation.Audit.Core.Component.Enums;
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
using System.Web.Http.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
    public class WebHookController : BaseApiController
    {
        private IOrganizationRepository _organizationRepository;
        private IPropertyRepository _propertyRepository;
        private ProductInternalSettingRepository _productInternalSettingRepository;
        private IManageOrganization _manageOrganization;
        private IManageBlueBook _manageBlueBook;
        
        public WebHookController()
        {
            // DONT USE USERCLAIM IN BASE, IT IS NULL AT THIS POINT. MOVE TO Initialize FUNCTION
        }

        public WebHookController(IRepository repository, DefaultUserClaim userClaim, HttpMessageHandler messageHandler)
        {
            _organizationRepository = new OrganizationRepository(repository);
            _propertyRepository = new PropertyRepository(repository);
            _productInternalSettingRepository = new ProductInternalSettingRepository(repository);
            _manageOrganization = new ManageOrganization(repository, userClaim);
            _manageBlueBook = new ManageBlueBook(userClaim, _productInternalSettingRepository, messageHandler);
            _userClaims = userClaim;
        }

        /// <summary>
        /// Used to initialize DI classes with userclaim
        /// </summary>
        /// <param name="controllerContext"></param>
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            _organizationRepository = new OrganizationRepository();
            _propertyRepository = new PropertyRepository();
            _productInternalSettingRepository = new ProductInternalSettingRepository();
            _manageOrganization = new ManageOrganization();
            _manageBlueBook = new ManageBlueBook(_userClaims);
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
            Dictionary<string, object> logData = new Dictionary<string, object>() {{"signature", signature ?? "null"}};
            WriteToLog(LogType.Diagnostic, "PostBooks : Begin", logData);

            if (thinEvent == null)
            {
                WriteToLog(LogType.Error, "Missing Content.");
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Missing Content.");
            }

            if (signature == null)
            {
                WriteToLog(LogType.Error, "Missing Signature.");
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Missing Signature.");
            }

            if (Request.Properties?["TibcoPostData"] is string requestBody)
            {
                string signingSecret = GetTiboWebHookSigningSecret();
                if (string.IsNullOrEmpty(signingSecret))
                {
                    response = Request.CreateResponse(HttpStatusCode.BadRequest, "Missing Signing Secret.");
                    WriteToLog(LogType.Error, "Signing secret was empty");
                    return response;
                }
                var hashed = SHA.GenerateHMACSHA256String(signingSecret, requestBody);
                logData.Add("requestBody", requestBody);

                logData.Add("hashed", hashed ?? "null");
                WriteToLog(LogType.Diagnostic, "Hash compare begin", logData);

                if (!string.Equals(signature, hashed, StringComparison.CurrentCultureIgnoreCase))
                {
                    response = Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid Signature.");
                    WriteToLog(LogType.Error, "Hash compare failed");
                    return response;
                }

                try
                {
                    logData = new Dictionary<string, object>();
                    WriteToLog(LogType.Diagnostic, thinEvent.Topic.ToLowerInvariant());
                    switch (thinEvent.Topic.ToLowerInvariant())
                    {
                        case "books.customerproperty.deleted":
                            var customerPropertyIdDeleted = Convert.ToInt64(thinEvent.Payload?["payload"]["customerPropertyId"] == null || thinEvent.Payload["payload"]["customerPropertyId"].Type == JTokenType.Null ? 0 : thinEvent.Payload?["payload"]["customerPropertyId"]);
                            var newCustomerPropertyId = Convert.ToInt64(thinEvent.Payload?["payload"]["replacementCustomerPropertyId"] == null || thinEvent.Payload["payload"]["replacementCustomerPropertyId"].Type == JTokenType.Null ? 0 : thinEvent.Payload?["payload"]["replacementCustomerPropertyId"]);
                            if (customerPropertyIdDeleted != 0)
                            {
                                if (newCustomerPropertyId != 0)
                                {
                                    RepositoryResponse result = _propertyRepository.UpdatePropertyMappingReMap(customerPropertyIdDeleted, newCustomerPropertyId);
                                    if (result.ErrorMessage.Length != 0)
                                    {
                                        logData = new Dictionary<string, object> {{"error", result}};

                                        WriteToLog(LogType.Error, "Error", logData);
                                        return Request.CreateResponse(HttpStatusCode.BadRequest, ResultErrorMessage(result));
                                    }
                                }
                                else
                                {
                                    // the site is being deleted with no replacement, but we don't do anything with this yet
                                }
                            }

                            break;

                        case "books.customerproperty.updated":
                            var customerPropertyIdUpdates = thinEvent.Payload?["payload"]["customerPropertyId"];

                            break;
                        case "books.customercompany.deleted":
                            var customerCompanyIdDeleted = Convert.ToInt64(thinEvent.Payload?["payload"]["customerCompanyId"] == null || thinEvent.Payload["payload"]["customerCompanyId"].Type == JTokenType.Null ? 0 : thinEvent.Payload?["payload"]["customerCompanyId"]);
                            
                            // NEED TO GET ALL COMPANIES WITH BLUE ID
                            var orgList = _organizationRepository.GetUnifiedLoginCompanyList();
                            //var organization = _organizationRepository.GetOrganization(blueBookId: customerCompanyIdDeleted);
                            if (orgList.Any(p => p.BooksCustomerMasterId == customerCompanyIdDeleted))
                            {
                                List<RepositoryResponse> errorResponseList = new List<RepositoryResponse>();

                                orgList.ForEach(p =>
                                {
                                    if (p.BooksCustomerMasterId == customerCompanyIdDeleted)
                                    {
                                        var newCustomerCompanyId = Convert.ToInt64(thinEvent.Payload?["payload"]["replacementCustomerCompanyId"] == null || thinEvent.Payload["payload"]["replacementCustomerCompanyId"].Type == JTokenType.Null ? 0 : thinEvent.Payload?["payload"]["replacementCustomerCompanyId"]);
                                        if (newCustomerCompanyId != 0)
                                        {
                                            Organization oldOrganization = new Organization() {PartyId = p.PartyId, BooksCustomerMasterId = p.BooksCustomerMasterId};
                                            Organization newOrganization = new Organization() {PartyId = p.PartyId, BooksCustomerMasterId = newCustomerCompanyId};
                                            RepositoryResponse result = _organizationRepository.UpdateOrganizationBooksCompanyMasterId(oldOrganization, newOrganization);
                                            if (result.ErrorMessage.Length != 0 || result.Id == 0)
                                            {
                                                logData = new Dictionary<string, object> {{"error", result}};
                                                WriteToLog(LogType.Error, "Error", logData);
                                                errorResponseList.Add(result);
                                            }
                                        }
                                        else
                                        {
                                            // the company is being deleted with no replacement, but we don't do anything with this yet
                                        }
                                    }
                                });
                                if (errorResponseList.Count > 0)
                                {
                                    string errorText = "";
                                    errorResponseList.ForEach(p =>
                                    {
                                        errorText += ResultErrorMessage(p);
                                    });
                                    
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, errorText);
                                }
                            }

                            break;

                        case "books.customercompany.updated":
                            var customerCompanyIdUpdated = thinEvent.Payload["payload"]["customerCompanyId"];
                            break;

                        case "provisioning.upfmorder.create":
                            // get the company info
                            var customerCompanyId = Convert.ToInt64(thinEvent.Payload?["company"]["customerCompanyId"] == null || thinEvent.Payload["company"]["customerCompanyId"].Type == JTokenType.Null ? 0 : thinEvent.Payload?["company"]["customerCompanyId"]);
                            var customerDomain = thinEvent.Payload?["customerEnvironment"].ToString();
                            if (string.IsNullOrEmpty(customerDomain))
                            {
                                response = Request.CreateResponse(HttpStatusCode.BadRequest, "Missing customerEnvironment");
                                WriteToLog(LogType.Error, "Missing customerEnvironment");
                                return response;
                            }

                            if (customerCompanyId != 0 && !string.IsNullOrEmpty(customerDomain))
                            {
                                string createResult = CreateCompanyFromBooks(customerCompanyId, customerDomain);
                                if (!string.IsNullOrEmpty(createResult) )
                                {
                                    logData = new Dictionary<string, object> {{"error", createResult}};

                                    WriteToLog(LogType.Error, "Error", logData);
                                    if (!createResult.Equals("Company not found in books environment", StringComparison.OrdinalIgnoreCase))
                                    {
                                        return Request.CreateResponse(HttpStatusCode.BadRequest, createResult);
                                    }
                                }
                            }

                            break;
                        default:
                            return Request.CreateResponse(HttpStatusCode.Accepted);
                    }
                }
                catch (Exception ex)
                {
                    response = Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
            
            logData.Add("response.StatusCode", response.StatusCode);
            WriteToLog(LogType.Diagnostic, "PostBooks : Complete", logData);
            return response;
        }

        private IList<ProductInternalSetting> GetUnifiedPlatformSettings()
        {
            IList<ProductInternalSetting> productInternalSettingList = new List<ProductInternalSetting>();
            RPObjectCache rpcache = new RPObjectCache();
            var cacheKey = "productInternalSetting_" + (int)ProductEnum.UnifiedPlatform;
            productInternalSettingList = rpcache.GetFromCache<IList<ProductInternalSetting>>(cacheKey, 60, () =>
            {
                // load from database
                return _productInternalSettingRepository.GetProductInternalSettings((int)ProductEnum.UnifiedPlatform);
            });
            return productInternalSettingList;
        }
        /// <summary>
        /// Used to get the signing secret used to validate Tibco WebHook events
        /// </summary>
        /// <returns>The list of settings</returns>
        private string GetTiboWebHookSigningSecret()
        {
            string signingSecret = GetUnifiedPlatformSettings()?.ToList().FirstOrDefault(s => s.Name.Equals("TiboWebHookSigningSecret", StringComparison.OrdinalIgnoreCase))?.Value;
            return signingSecret ?? "";
        }

        private string CreateCompanyFromBooks(long booksCustomerMasterId, string domain)
        {
            bool processBlueBookMessage = false;

            // check to see if the company already exists
            var organizationList = _manageOrganization.GetOrganizationList();
            if (organizationList.Any(o => o.BooksCustomerMasterId == booksCustomerMasterId && o.OrganizationDomain.Name.Equals(domain, StringComparison.OrdinalIgnoreCase)))
            {
                return "";
            }
            string ignoreEnvironment = GetUnifiedPlatformSettings()?.ToList().FirstOrDefault(s => s.Name.Equals("UPFMOrderIgnoreEnvironment", StringComparison.OrdinalIgnoreCase))?.Value;
            if (!string.IsNullOrEmpty(ignoreEnvironment))
            {
                return "";
            }

            var customerCompany = _manageBlueBook.GetCompanyCustomerInfo(companyRealPageId: Guid.Empty, domain:null, booksCompanyMasterId:booksCustomerMasterId );
            if (customerCompany == null){ return "Company not found in books environment"; }
            
            OrganizationCreate organization = new OrganizationCreate()
            {
                Name = customerCompany.CompanyName,
                BooksCompanyId = customerCompany.MasterCompanyId,
                BooksCustomerMasterId = customerCompany.CustomerCompanyId,
                AdminUser = new OrganizationAdminUser()
                {
                    FirstName = "RealPage",
                    LastName = "Access",
                    Suffix = "",
                    Title = "",
                    Email = $"{customerCompany.CustomerCompanyId}admin@realpage.com"
                }
            };
            var organizationTypeList = _manageOrganization.ListOrganizationType();
            var organizationDomainList = _manageOrganization.ListOrganizationDomain();

            organization.OrganizationTypeId = organizationTypeList.FirstOrDefault(p => p.Name.Equals(customerCompany.CompanyType, StringComparison.OrdinalIgnoreCase)).OrganizationTypeId;

            if (!organizationDomainList.Any(d => d.Name.Equals(domain, StringComparison.OrdinalIgnoreCase)))
            {
                RepositoryResponse response = _manageOrganization.CreateOrganizationDomain(new OrganizationDomain() {Name = domain});
                if (response.Id > 0)
                {
                    organization.OrganizationDomainId = Convert.ToInt32(response.Id);
                }
            }
            else
            {
                organization.OrganizationDomainId = organizationDomainList.FirstOrDefault(p => p.Name.Equals(domain, StringComparison.OrdinalIgnoreCase)).OrganizationDomainId;
            }
            
            organization.Products = new List<string>();

            // get a list of products from blue
            var productList = _manageBlueBook.GetCompanyMap(companyRealPageId: Guid.Empty, booksCompanyMasterId: booksCustomerMasterId, source: null, domain: domain, includeGreenBookCares: false);
            if (productList != null)
            {
                foreach (var customerCompanyMap in productList)
                {
                    organization.Products.Add(customerCompanyMap.Source);
                }
            }

            var result = _manageOrganization.CreateOrganization(organization, processBlueBookMessage);

            if (!result.Status.Success || !string.IsNullOrEmpty(result.Status.ErrorMsg))
            {
                return result.Status.ErrorMsg;
            }
            var companyInstance= new CompanyInstanceAdd()
            {
                CustomerCompanyId = booksCustomerMasterId,
                CompanyInstanceSourceId = result.obj.Org.RealPageIdUpperCaseForBooks,
                CompanyName = result.obj.Org.Name,
                Source = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform),
                IsActive = true,
                CreatedBy = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform) + " Automation",
                CustomerEnvironment = domain
            };

            // add the new company data back to books
            var booksResult = _manageBlueBook.AddBooksGreenBookCompanyInstance(companyInstance);

            return "";
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

        /// <summary>
        /// Used to write to the log
        /// </summary>
        /// <param name="logType"></param>
        /// <param name="message"></param>
        /// <param name="logData"></param>
        /// <param name="exception"></param>
        private void WriteToLog(LogType logType, string message, Dictionary<string, object> logData = null, Exception exception = null)
        {
            try
            {
                Log.Write(logType, new LogDetails
                {
                    Message = message,
                    AdditionalInfo = logData,
                    ProductModule = this.GetType().ToString(),
                    UserId = "0",
                    PmcId = "0",
                    Exception = exception,
                    CorrelationId = _userClaims?.CorrelationId.ToString(),
                });
            }
            catch
            {
                /*ignored*/
            }
        }
    }
}
