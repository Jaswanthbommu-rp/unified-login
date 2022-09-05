using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.WebHook;
using Serilog;
using Serilog.Events;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
    public class WebHookController : BaseApiController
    {
        private IOrganizationRepository _organizationRepository;
        private IPropertyRepository _propertyRepository;
        private ProductInternalSettingRepository _productInternalSettingRepository;
        private IManageOrganization _manageOrganization;
        private IManageBlueBook _manageBlueBook;
        private IManageOrganizationProduct _manageOrganizationProduct;
        private IOrganizationProductRepository _organizationProductRepository;
        private IManageProduct _manageProduct;
        private IManageHotsCloneUsers _manageHotsCloneUsers;

        public WebHookController()
        {
            // DONT USE USERCLAIM IN BASE, IT IS NULL AT THIS POINT. MOVE TO Initialize FUNCTION
        }

        public WebHookController(IRepository repository, DefaultUserClaim userClaim, HttpMessageHandler messageHandler)
        {
            _organizationRepository = new OrganizationRepository(repository);
            _propertyRepository = new PropertyRepository(repository);
            _productInternalSettingRepository = new ProductInternalSettingRepository(repository);
            _manageOrganization = new ManageOrganization(repository, userClaim, messageHandler);
            _manageBlueBook = new ManageBlueBook(userClaim, repository, _productInternalSettingRepository, messageHandler);
            _organizationProductRepository = new OrganizationProductRepository(repository);
            _manageProduct = new ManageProduct(repository, userClaim, messageHandler);
            _manageOrganizationProduct = new ManageOrganizationProduct(userClaim, repository, _manageBlueBook, _manageProduct);
            _manageHotsCloneUsers = new ManageHotsCloneUsers(repository, userClaim, messageHandler);
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
            _manageOrganization = new ManageOrganization(_userClaims);
            
            ClaimsPrincipal currentClaimPrincipal = ClaimsPrincipal.Current;
            if (!currentClaimPrincipal.Identity.IsAuthenticated)
            {
                var org = _manageOrganization.GetOrganization(EmployeeCompanyRealPageId);
                _userClaims.OrganizationPartyId = org.PartyId;
                _userClaims.UserRealPageGuid = new Guid("00000000-0000-0000-0000-000000000001");
                _userClaims.OrganizationMasterId = -1;
                _userClaims.UserId = 0;
                _userClaims.LoginName = "autoprovisioning";
                _userClaims.FirstName = "Auto";
                _userClaims.LastName = "Provisioning";
            }

            _manageOrganization = new ManageOrganization(_userClaims);
            _organizationProductRepository = new OrganizationProductRepository();
            _manageOrganizationProduct = new ManageOrganizationProduct(_userClaims);
            _manageBlueBook = new ManageBlueBook(_userClaims);
            _manageHotsCloneUsers = new ManageHotsCloneUsers(_userClaims);
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
            WriteToLog(LogEventLevel.Debug, "PostBooks : Begin", logData);

            if (thinEvent == null)
            {
                WriteToLog(LogEventLevel.Error, "Missing Content.");
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Missing Content.");
            }

            if (signature == null)
            {
                WriteToLog(LogEventLevel.Error, "Missing Signature.");
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Missing Signature.");
            }

            if (Request.Properties?["TibcoPostData"] is string requestBody)
            {
                string signingSecret = GetTibcoWebHookSigningSecret();
                if (string.IsNullOrEmpty(signingSecret))
                {
                    response = Request.CreateResponse(HttpStatusCode.BadRequest, "Missing Signing Secret.");
                    WriteToLog(LogEventLevel.Error, "Signing secret was empty");
                    return response;
                }

                var hashed = SHA.GenerateHMACSHA256String(signingSecret, requestBody);
                logData.Add("requestBody", requestBody);

                logData.Add("hashed", hashed ?? "null");
                WriteToLog(LogEventLevel.Debug, "Hash compare begin", logData);

                if (!string.Equals(signature, hashed, StringComparison.CurrentCultureIgnoreCase))
                {
                    response = Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid Signature.");
                    WriteToLog(LogEventLevel.Error, "Hash compare failed");
                    return response;
                }

                try
                {
                    logData = new Dictionary<string, object>();
                    WriteToLog(LogEventLevel.Debug, thinEvent.Topic.ToLowerInvariant());
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

                                        WriteToLog(LogEventLevel.Error, "Error", logData);
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
                                                WriteToLog(LogEventLevel.Error, "Error", logData);
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
                                    errorResponseList.ForEach(p => { errorText += ResultErrorMessage(p); });

                                    return Request.CreateResponse(HttpStatusCode.BadRequest, errorText);
                                }
                            }

                            break;

                        case "books.customercompany.updated":
                            var customerCompanyIdUpdated = thinEvent.Payload["payload"]["customerCompanyId"];
                            break;

                        case "provisioning.upfmorder.create":
                        case "provisioning.upfmclone.create":
                            // get the company info
                            var customerCompanyId = Convert.ToInt32(thinEvent.Payload?["company"]["customerCompanyId"] == null || thinEvent.Payload["company"]["customerCompanyId"].Type == JTokenType.Null ? 0 : thinEvent.Payload?["company"]["customerCompanyId"]);
                            var customerDomain = thinEvent.Payload?["company"]["customerEnvironment"] == null || thinEvent.Payload?["company"]["customerEnvironment"].Type == JTokenType.Null ? thinEvent.Payload?["customerEnvironment"].ToString() : thinEvent.Payload?["company"]["customerEnvironment"].ToString();

                            if (thinEvent.Topic.Equals("provisioning.upfmclone.create", StringComparison.OrdinalIgnoreCase))
                            {
                                string hotsCloningEnabled = GetUnifiedPlatformSettings((int)ProductEnum.UnifiedPlatform)?.ToList().FirstOrDefault(s => s.Name.Equals("IsCloneUsersProcessEnabledForHOTS", StringComparison.OrdinalIgnoreCase))?.Value;
                                if (string.IsNullOrEmpty(hotsCloningEnabled) || !hotsCloningEnabled.Equals("1", StringComparison.OrdinalIgnoreCase))
                                {
                                    WriteToLog(LogEventLevel.Debug, $"Environment not enabled for HOTS cloning");
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, $"Environment not enabled for HOTS cloning");
                                }

                                var cloneCompanyId = thinEvent.Payload?["company"]["cloneCompanyInstanceSourceId"] == null || thinEvent.Payload?["company"]["cloneCompanyInstanceSourceId"].Type == JTokenType.Null ? null : thinEvent.Payload?["company"]["cloneCompanyInstanceSourceId"].ToString();
                                if (string.IsNullOrEmpty(cloneCompanyId))
                                {
                                    response = Request.CreateResponse(HttpStatusCode.BadRequest, "Missing cloneCompanyInstanceSourceId");
                                    WriteToLog(LogEventLevel.Error, "Missing cloneCompanyInstanceSourceId");
                                    return response;
                                }

                                if (!Guid.TryParse(cloneCompanyId, out var baselineCompanyGuid))
                                {
                                    response = Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid cloneCompanyInstanceSourceId, not Guid");
                                    WriteToLog(LogEventLevel.Error, "Invalid cloneCompanyInstanceSourceId, not Guid");
                                    return response;
                                };
                                var cloneBaselineOrg = _manageOrganization.GetOrganization(baselineCompanyGuid);
                                if (cloneBaselineOrg == null)
                                {
                                    WriteToLog(LogEventLevel.Error, $"HOTS Baseline Company {cloneCompanyId} not found");
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, $"HOTS Baseline Company {cloneCompanyId} not found");
                                }
                            }

                            var propertyList = thinEvent.Payload["properties"];
                            string existingUnifiedLoginInstanceId = thinEvent.Payload?["company"]["companyInstanceSourceId"] == null || thinEvent.Payload?["company"]["companyInstanceSourceId"].Type == JTokenType.Null ? null : thinEvent.Payload?["company"]["companyInstanceSourceId"].ToString();

                            List<int> uniqueProductIdList = new List<int>();

                            List<UPFMPropertyInstance> propertyInstanceList = new List<UPFMPropertyInstance>();
                            ProductCenterEnablement centerEnablement = new ProductCenterEnablement() {Details = new List<ProductCenterEnablementSettings>()};
                            centerEnablement.EnabledBy = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform) + " Automation";

                            var companyProductCenters = thinEvent.Payload?["company"]["productCenters"];
                            try
                            {
                                foreach (var product in companyProductCenters)
                                {
                                    int productId = Convert.ToInt32(product["productCenterSourceId"]);
                                    if (!uniqueProductIdList.Contains(productId))
                                    {
                                        uniqueProductIdList.Add(productId);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                logData = new Dictionary<string, object> {{"error", ex.Message}};
                                WriteToLog(LogEventLevel.Error, "Error parsing company product list", logData);
                            }

                            try
                            {
                                foreach (var property in propertyList)
                                {
                                    var currentProductList = new List<int>();
                                    var productList = property["productCenters"];
                                    foreach (var product in productList)
                                    {
                                        int productId = Convert.ToInt32(product["productCenterSourceId"]);
                                        currentProductList.Add(productId);
                                        if (!uniqueProductIdList.Contains(productId))
                                        {
                                            uniqueProductIdList.Add(productId);
                                        }
                                    }

                                    string existingUPFMPropertyInstanceId = property["propertyInstanceSourceId"] == null || property["propertyInstanceSourceId"].Type == JTokenType.Null ? null : property["propertyInstanceSourceId"].ToString();
                                    string clonePropertyInstanceSourceId = property["clonePropertyInstanceSourceId"] == null || property["clonePropertyInstanceSourceId"].Type == JTokenType.Null ? null : property["clonePropertyInstanceSourceId"].ToString();
                                    if (string.IsNullOrEmpty(clonePropertyInstanceSourceId) && thinEvent.Topic.Equals("provisioning.upfmclone.create", StringComparison.OrdinalIgnoreCase))
                                    {
                                        response = Request.CreateResponse(HttpStatusCode.BadRequest, "Missing clonePropertyInstanceSourceId");
                                        WriteToLog(LogEventLevel.Error, "Missing clonePropertyInstanceSourceId");
                                        return response;
                                    }

                                    var newProperty =
                                        new UPFMPropertyInstance()
                                        {
                                            Name = property["propertyName"].ToString(),
                                            City = property["city"] == null || property["city"].Type == JTokenType.Null ? null : property["city"].ToString(),
                                            State = property["state"] == null || property["state"].Type == JTokenType.Null ? null : property["state"].ToString(),
                                            County = property["county"] == null || property["county"].Type == JTokenType.Null ? null : property["county"].ToString(),
                                            Address = property["address"] == null || property["address"].Type == JTokenType.Null ? null : property["address"].ToString(),
                                            Country = property["country"] == null || property["country"].Type == JTokenType.Null ? null : property["country"].ToString(),
                                            PostalCode = property["postalCode"] == null || property["postalCode"].Type == JTokenType.Null ? null : property["postalCode"].ToString(),
                                            Longitude = Convert.ToDecimal(property?["longitude"] == null || property["longitude"].Type == JTokenType.Null ? 0 : property["longitude"]),
                                            Latitude = Convert.ToDecimal(property?["latitude"] == null || property["latitude"].Type == JTokenType.Null ? 0 : property["latitude"]),
                                            CustomerPropertyId = property["customerPropertyId"] == null || property["customerPropertyId"].Type == JTokenType.Null ? null : property["customerPropertyId"].ToString(),
                                            InstanceId = existingUPFMPropertyInstanceId == null ? Guid.Empty : new Guid(existingUPFMPropertyInstanceId),
                                            Domain = property["customerEnvironment"] == null || property["customerEnvironment"].Type == JTokenType.Null ? customerDomain : property["customerEnvironment"].ToString(),
                                            ProductList = currentProductList,
                                            ClonePropertyInstanceSourceId = clonePropertyInstanceSourceId == null ? Guid.Empty : new Guid(clonePropertyInstanceSourceId),
                                        };

                                    // 
                                    if (newProperty.Longitude == 0 || newProperty.Latitude == 0)
                                    {
                                        try
                                        {
                                            var propertyDetails = _manageBlueBook.GetCustomerPropertyDetails(newProperty.CustomerPropertyId);
                                            if (!string.IsNullOrEmpty(propertyDetails?.attributes?.address?.longitude) && !string.IsNullOrEmpty(propertyDetails?.attributes?.address?.latitude))
                                            {
                                                newProperty.Longitude = Convert.ToDecimal(propertyDetails.attributes.address.longitude);
                                                newProperty.Latitude = Convert.ToDecimal(propertyDetails.attributes.address.latitude);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            logData = new Dictionary<string, object> {{"error", ex.Message}};
                                            WriteToLog(LogEventLevel.Error, "Error parsing property address longitude", logData);
                                        }
                                    }

                                    propertyInstanceList.Add(newProperty);
                                }
                            }
                            catch (Exception ex)
                            {
                                logData = new Dictionary<string, object> {{"error", ex.Message}};
                                WriteToLog(LogEventLevel.Error, "Error parsing property list", logData);
                            }

                            if (string.IsNullOrEmpty(customerDomain))
                            {
                                response = Request.CreateResponse(HttpStatusCode.BadRequest, "Missing customerEnvironment");
                                WriteToLog(LogEventLevel.Error, "Missing customerEnvironment");
                                return response;
                            }

                            if (existingUnifiedLoginInstanceId == null)
                            {
                                //return Request.CreateResponse(HttpStatusCode.BadRequest, "stop");
                                var createResult = CreateCompanyFromBooks(thinEvent.Payload?["company"], customerCompanyId, customerDomain, uniqueProductIdList, thinEvent.Topic.ToLowerInvariant());
                                if (!string.IsNullOrEmpty(createResult.Result))
                                {
                                    propertyInstanceList = new List<UPFMPropertyInstance>();
                                    centerEnablement.Details = new List<ProductCenterEnablementSettings>();
                                    logData = new Dictionary<string, object> {{"error", createResult}};

                                    WriteToLog(LogEventLevel.Error, "Error", logData);
                                    if (createResult.Result.Equals("Company not found in books environment- test", StringComparison.OrdinalIgnoreCase))
                                    {
                                        // shortcut out, this create may be for another environment
                                        return Request.CreateResponse(HttpStatusCode.Accepted);
                                    }
                                    return Request.CreateResponse(HttpStatusCode.BadRequest, createResult);
                                }

                                if (!createResult.RealPageId.Equals(Guid.Empty.ToString()))
                                {
                                    existingUnifiedLoginInstanceId = createResult.RealPageId;
                                }
                            }

                            var org = _manageOrganization.GetOrganization(new Guid(existingUnifiedLoginInstanceId));
                            if (org == null)
                            {
                                WriteToLog(LogEventLevel.Error, $"Company {existingUnifiedLoginInstanceId} not found");
                                return Request.CreateResponse(HttpStatusCode.BadRequest, $"Company {existingUnifiedLoginInstanceId} not found");
                            }

                            if (uniqueProductIdList.Count > 0)
                            {
                                var cacheKey = $"getProductsByCompany_{org.RealPageId}";
                                RPObjectCache.RemoveFromCache(cacheKey);

                                var existingProductList = _organizationRepository.GetProductsByCompany(org.RealPageId);
                                foreach (var productId in uniqueProductIdList)
                                {
                                    if (existingProductList.All(p => p.ProductId != productId))
                                    {
                                        var addresponse = _manageOrganizationProduct.InsertUpdateOrganizationProductFromProvisioning(productId, null, null, null, org);
                                    }
                                }
                            }

                            // add ack for new products for the company
                            foreach (var productId in uniqueProductIdList)
                            {
                                var internalSettings = GetUnifiedPlatformSettings(productId);
                                var updateinUDM = internalSettings.Where(x => x.Name.ToUpper() == "UPDATEPRODUCTINUDM").FirstOrDefault();

                                if (updateinUDM != null && updateinUDM.Value == "1")
                                {
                                    centerEnablement.Details.Add(new ProductCenterEnablementSettings()
                                    {
                                        Source = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform),
                                        CustomerEnvironment = customerDomain,
                                        CustomerCompanyId = customerCompanyId,
                                        CompanyInstanceSourceId = existingUnifiedLoginInstanceId,
                                        ProductCenterSourceId = productId.ToString(),
                                        PropertyInstanceSourceId = null,
                                        CustomerPropertyId = null
                                    });
                                }
                            }

                            // add any new properties
                            string propertyResult = AddPropertiesFromBooks(customerCompanyId, existingUnifiedLoginInstanceId, customerDomain, propertyInstanceList);

                            // enable the products
                            if (centerEnablement.Details.Count > 0)
                            {
                                _manageBlueBook.AcknowledgeProvisioningEvent(centerEnablement);
                            }

                            break;
                        case "provisioning.upfmorder.cancel":
                            var productListToCancel = thinEvent.Payload?["company"]["productCenters"];
                            string companyInstanceSourceId = thinEvent.Payload?["company"]["companyInstanceSourceId"] == null || thinEvent.Payload?["company"]["companyInstanceSourceId"].Type == JTokenType.Null ? null : thinEvent.Payload?["company"]["companyInstanceSourceId"].ToString();
                            if (string.IsNullOrEmpty(companyInstanceSourceId))
                            {
                                WriteToLog(LogEventLevel.Error, $"companyInstanceSourceId should not be null or empty");
                                return Request.CreateResponse(HttpStatusCode.BadRequest, $"Invalid companyInstanceSourceId");
                            }

                            ProductCenterCancellation centerCancel = new ProductCenterCancellation() {Details = new List<ProductCenterCancellationSettings>()};
                            centerCancel.CancelledBy = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform) + " Automation";
                            var orgDetails = _manageOrganization.GetOrganization(new Guid(companyInstanceSourceId));
                            if (orgDetails != null)
                            {
                                foreach (var product in productListToCancel)
                                {
                                    if (product["productCenterSourceId"] != null && product["productCenterSourceId"].ToString() != "")
                                    {
                                        var addresponse = _manageOrganizationProduct.DeleteOrganizationProduct(partyId: orgDetails.PartyId, product: (ProductEnum) Convert.ToInt32(product["productCenterSourceId"]), org: orgDetails);
                                        _manageOrganizationProduct.DisableUsersForProduct(partyId: orgDetails.PartyId, product: (ProductEnum) Convert.ToInt32(product["productCenterSourceId"]));
                                        centerCancel.Details.Add(new ProductCenterCancellationSettings()
                                        {
                                            CompanyInstanceSourceId = companyInstanceSourceId,
                                            PropertyInstanceSourceId = null,
                                            ProductCenterSourceId = product["productCenterSourceId"].ToString(),
                                            Source = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform)
                                        });
                                    }
                                    else
                                    {
                                        WriteToLog(LogEventLevel.Error, $"Invalid ProductCenterSourceId -  {product["productCenterSourceId"]}");
                                        return Request.CreateResponse(HttpStatusCode.BadRequest, $"Invalid ProductCenterSourceId");
                                    }
                                }
                            }
                            else
                            {
                                WriteToLog(LogEventLevel.Error, $"Company {companyInstanceSourceId} not found");
                                return Request.CreateResponse(HttpStatusCode.BadRequest, $"Company {companyInstanceSourceId} not found");
                            }

                            if (centerCancel.Details.Count > 0)
                            {
                                _manageBlueBook.AcknowledgeProvisioningCancelEvent(centerCancel);
                            }

                            break;
                        default:
                            return Request.CreateResponse(HttpStatusCode.Accepted);
                    }
                }
                catch (Exception ex)
                {
                    WriteToLog(LogEventLevel.Error, $"PostBooks Error", exception: ex);
                    response = Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }

            logData.Add("response.StatusCode", response.StatusCode);
            WriteToLog(LogEventLevel.Debug, "PostBooks : Complete", logData);
            return response;
        }

        /// <summary>
        /// Used to add a new property instance to UPFM and then send the new instance id to books
        /// </summary>
        /// <param name="customerCompanyId"></param>
        /// <param name="unifiedLoginInstanceId"></param>
        /// <param name="customerCompanyDomain"></param>
        /// <param name="propertyInstanceList"></param>
        /// <returns></returns>
        private string AddPropertiesFromBooks(int customerCompanyId, string unifiedLoginInstanceId, string customerCompanyDomain, List<UPFMPropertyInstance> propertyInstanceList)
        {
            string result = "";
            foreach (var property in propertyInstanceList)
            {
                if (!unifiedLoginInstanceId.Equals(Guid.Empty.ToString()) && property.InstanceId == Guid.Empty)
                {
                    // add instance to db
                    var response = _manageOrganization.InsertUPFMPropertyInstance(property);
                    if (response.ErrorMessage.Length == 0)
                    {
                        // insert to books
                        property.InstanceId = response.RealPageId;
                        PropertyInstance pi = new PropertyInstance()
                        {
                            PropertyName = property.Name,
                            CompanyInstanceSourceId = unifiedLoginInstanceId,
                            PropertyInstanceSourceId = property.InstanceId.ToString(),
                            CustomerPropertyId = Convert.ToInt32(!string.IsNullOrEmpty(property.CustomerPropertyId) ? property.CustomerPropertyId : "0"),
                            CustomerEnvironment = property.Domain,
                            Source = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform),
                            IsActive = true,
                            Address = new InstanceAddress()
                            {
                                Address = property.Address,
                                City = property.City,
                                State = property.State,
                                PostalCode = property.PostalCode,
                                County = property.County,
                                Country = property.Country,
                            },
                            ModifiedBy = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform) + " Automation"
                        };
                        var resultBooks = _manageBlueBook.AddBooksGreenBookPropertyInstanceFromProvisioning(pi);

                        if (resultBooks && property.ClonePropertyInstanceSourceId != Guid.Empty)
                        {
                            Guid.TryParse(unifiedLoginInstanceId, out var cloneCompanyGuid);
                            var hotsResult = _manageHotsCloneUsers.InsertHotsPropertyRelationship(property.ClonePropertyInstanceSourceId, property.InstanceId, cloneCompanyGuid, 1);
                            if (hotsResult?.Id == 0)
                            {
                                WriteToLog(LogEventLevel.Error, $"Failed to add HOTS property relationship. baseline {property.ClonePropertyInstanceSourceId} clone {property.InstanceId}");
                            }
                        }
                    }
                    else
                    {
                        var logData = new Dictionary<string, object>
                        {
                                {"property", property},
                                {"response.ErrorMessage", response.ErrorMessage}
                        };
                        WriteToLog(LogEventLevel.Error, "AddPropertiesFromBooks Error", logData);
                    }
                }
            }

            return result;
        }

        private IList<ProductInternalSetting> GetUnifiedPlatformSettings(int productId)
        {
            IList<ProductInternalSetting> productInternalSettingList = new List<ProductInternalSetting>();
            RPObjectCache rpcache = new RPObjectCache();
            var cacheKey = "productInternalSetting_" + (int)productId;
            productInternalSettingList = rpcache.GetFromCache<IList<ProductInternalSetting>>(cacheKey, 60, () =>
            {
                // load from database
                return _productInternalSettingRepository.GetProductInternalSettings((int)productId);
            });
            return productInternalSettingList;
        }

        /// <summary>
        /// Used to get the signing secret used to validate Tibco WebHook events
        /// </summary>
        /// <returns>The list of settings</returns>
        private string GetTibcoWebHookSigningSecret()
        {
            string signingSecret = GetUnifiedPlatformSettings((int)ProductEnum.UnifiedPlatform)?.ToList().FirstOrDefault(s => s.Name.Equals("TiboWebHookSigningSecret", StringComparison.OrdinalIgnoreCase))?.Value;
            return signingSecret ?? "";
        }

        private CreateCompanyResult CreateCompanyFromBooks(JToken companyPayload, long booksCustomerMasterId, string domain, List<int> productIdList, string eventTopic)
        {
            bool processBlueBookMessage = false;
            CreateCompanyResult createCompanyResult = new CreateCompanyResult() {RealPageId = Guid.Empty.ToString()};

            // check to see if the company already exists
            var organizationList = _manageOrganization.GetOrganizationList();
            if (organizationList.Any(o => o.BooksCustomerMasterId == booksCustomerMasterId && o.OrganizationDomain.Name.Equals(domain, StringComparison.OrdinalIgnoreCase)))
            {
                createCompanyResult.Result = $"Company customerMasterId {booksCustomerMasterId} / domain {domain} already exists";
                return createCompanyResult;
            }

            string ignoreEnvironment = GetUnifiedPlatformSettings((int)ProductEnum.UnifiedPlatform)?.ToList().FirstOrDefault(s => s.Name.Equals("UPFMOrderIgnoreEnvironment", StringComparison.OrdinalIgnoreCase))?.Value;
            if (!string.IsNullOrEmpty(ignoreEnvironment))
            {
                createCompanyResult.Result = "Ignoring environment";
                return createCompanyResult;
            }

            var customerCompany = _manageBlueBook.GetCompanyCustomerInfo(companyRealPageId: Guid.Empty, domain: null, booksCompanyMasterId: booksCustomerMasterId);
            if (customerCompany == null)
            {
                createCompanyResult.Result = "Company not found in books environment";
                return createCompanyResult;
            }

            var companyName = companyPayload?["companyName"] == null || companyPayload?["companyName"].Type == JTokenType.Null ? "Missing name" : companyPayload["companyName"].ToString();
            var companyAddress = companyPayload?["address"] == null || companyPayload?["address"].Type == JTokenType.Null ? "" : companyPayload["address"].ToString();
            var companyCity = companyPayload?["city"] == null || companyPayload?["city"].Type == JTokenType.Null ? "" : companyPayload["city"].ToString();
            var companyState = companyPayload?["state"] == null || companyPayload?["state"].Type == JTokenType.Null ? "" : companyPayload["state"].ToString();
            var companyPostalCode = companyPayload?["postalCode"] == null || companyPayload?["postalCode"].Type == JTokenType.Null ? "" : companyPayload["postalCode"].ToString();
            var companyCounty = companyPayload?["county"] == null || companyPayload?["county"].Type == JTokenType.Null ? "" : companyPayload["county"].ToString();
            var companyCountry = companyPayload?["country"] == null || companyPayload?["country"].Type == JTokenType.Null ? "" : companyPayload["country"].ToString();

            if (eventTopic.Equals("provisioning.upfmclone.create", StringComparison.OrdinalIgnoreCase))
            {
                companyName += $"-{customerCompany.CustomerCompanyId}";
            }

            OrganizationCreate organization = new OrganizationCreate()
            {
                Name = companyName,
                BooksCompanyId = customerCompany.MasterCompanyId,
                BooksCustomerMasterId = customerCompany.CustomerCompanyId,
                AdminUser = new OrganizationAdminUser()
                {
                    FirstName = "RealPage",
                    LastName = "Access",
                    Suffix = "",
                    Title = "",
                },
                CompanyAddress = new CompanyInstanceAddress() {Address = companyAddress, City = companyCity, State = companyState, PostalCode = companyPostalCode, County = companyCounty, Country = companyCountry}
            };

            WriteToLog(LogEventLevel.Debug, $"Adding company {companyName}");
            var organizationTypeList = _manageOrganization.ListOrganizationType();
            var organizationDomainList = _manageOrganization.ListOrganizationDomain();

            if (organizationTypeList.FirstOrDefault(p => p.Name.Equals(customerCompany.CompanyType, StringComparison.OrdinalIgnoreCase)) == null)
            {
                createCompanyResult.Result = "Unknown organization type";
                return createCompanyResult;
            }

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

            organization.OrganizationDomain = domain;

            var addProductList = new List<int>(productIdList);

            var result = _manageOrganization.CreateOrganization(organization, addProductList, processBlueBookMessage);

            if (!result.Status.Success || !string.IsNullOrEmpty(result.Status.ErrorMsg))
            {
                createCompanyResult.Result = result.Status.ErrorMsg;
                return createCompanyResult;
            }

            createCompanyResult.RealPageId = result.obj.Org.RealPageId.ToString();
            if (eventTopic.Equals("provisioning.upfmclone.create", StringComparison.OrdinalIgnoreCase))
            {
                var cloneCompanyInstanceSourceId = companyPayload?["cloneCompanyInstanceSourceId"] == null || companyPayload?["cloneCompanyInstanceSourceId"].Type == JTokenType.Null ? null : companyPayload?["cloneCompanyInstanceSourceId"].ToString();
                if (cloneCompanyInstanceSourceId != null)
                {
                    Guid.TryParse(cloneCompanyInstanceSourceId, out var baselineCompanyGuid);
                    Guid.TryParse(createCompanyResult.RealPageId, out var cloneCompanyGuid);
                    
                    var hotsResult = _manageHotsCloneUsers.InsertHotsCompanyRelationship(baselineCompanyGuid, cloneCompanyGuid, 1);
                    if (hotsResult.Id == 0)
                    {
                        createCompanyResult.Result = "Error inserting HOTS company relationship";
                        return createCompanyResult;
                    }
                }
            }
            
            var companyInstance = new CompanyInstanceAdd()
            {
                CustomerCompanyId = booksCustomerMasterId,
                CompanyInstanceSourceId = result.obj.Org.RealPageId.ToString(),
                CompanyName = result.obj.Org.Name,
                Source = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform),
                IsActive = true,
                CreatedBy = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform) + " Automation",
                CustomerEnvironment = domain,
                CompanyType = customerCompany.CompanyType
            };
            
            // add the new company data back to books
            var booksResult = _manageBlueBook.AddUPFMCompanyFromProvisioningEvent(companyInstance);

            return createCompanyResult;
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
        private void WriteToLog(LogEventLevel logType, string message, Dictionary<string, object> logData = null, Exception exception = null)
        {
            try
            {
                string correlationId = "";
                if (_userClaims != null)
                {
                    correlationId = (_userClaims.CorrelationId != Guid.Empty) ? _userClaims.CorrelationId.ToString() : "";
                }

                var logger = Log.Logger;
                if (logData?.Keys != null)
                {
                    logger = logger.ForContext("AdditionalInfo", JsonConvert.SerializeObject(logData, Formatting.Indented), false);
                }

                logger = logger.ForContext("ProductModule", this.GetType());
                logger = logger.ForContext("CorrelationId", correlationId);
                logger.Write(logType, exception, message);
            }
            catch
            {
                /*ignored*/
            }
        }
    }

    public class CreateCompanyResult
    {
        public string Result { get; set; }

        public string RealPageId { get; set; }
    }
}
