using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Security.Claims;
using System.Threading.Tasks;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.WebHook;

namespace UnifiedLogin.LandingAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("")]
    public class WebHookController : ControllerBase
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
        private DefaultUserClaim _userClaims;

        public WebHookController(IUserClaimsAccessor userClaimsAccessor)
        {
            _userClaims = userClaimsAccessor.GetUserClaim();
            _organizationRepository = new OrganizationRepository();
            _propertyRepository = new PropertyRepository();
            _productInternalSettingRepository = new ProductInternalSettingRepository();
            _manageOrganization = new ManageOrganization(_userClaims);

            ClaimsPrincipal currentClaimPrincipal = ClaimsPrincipal.Current;
            if (currentClaimPrincipal != null && !currentClaimPrincipal.Identity.IsAuthenticated)
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
            _manageProduct = new ManageProduct(_userClaims);
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

        private static readonly Guid EmployeeCompanyRealPageId = new Guid("8e31a5ec-c884-4f65-89c7-98e7f55a1a7e");

        /// <summary>
        /// Webhook endpoint for Books events
        /// </summary>
        /// <param name="thinEvent">The thin event payload</param>
        /// <returns>Action result</returns>
        [HttpPost]
        [AllowAnonymous]
        [Route("webhook/books")]
        public async Task<IActionResult> PostBooks([FromBody] ThinEvent<JToken> thinEvent)
        {
            return await Task.Run<IActionResult>(() =>
            {
                string signature = Request.Headers?.FirstOrDefault(h => h.Key == "signature").Value.FirstOrDefault();
                Dictionary<string, object> logData = new Dictionary<string, object>() { { "signature", signature ?? "null" } };
                WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", logData, messageProperties: new object[] { "PostBooks", "Begin" });

                if (thinEvent == null)
                {
                    WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", messageProperties: new object[] { "PostBooks", "Missing Content" });
                    return BadRequest("Missing Content.");
                }

                if (signature == null)
                {
                    WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", messageProperties: new object[] { "PostBooks", "Missing Signature" });
                    return BadRequest("Missing Signature.");
                }

                if (Request.HttpContext.Items?["TibcoPostData"] is string requestBody)
                {
                    string signingSecret = GetTibcoWebHookSigningSecret();
                    if (string.IsNullOrEmpty(signingSecret))
                    {
                        WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", messageProperties: new object[] { "PostBooks", "Signing secret was empty" });
                        return BadRequest("Missing Signing Secret.");
                    }

                    var hashed = SHA.GenerateHMACSHA256String(signingSecret, requestBody);
                    logData.Add("requestBody", requestBody);

                    logData.Add("hashed", hashed ?? "null");
                    WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", logData, messageProperties: new object[] { "PostBooks", "Hash compare begin" });

                    if (!string.Equals(signature, hashed, StringComparison.CurrentCultureIgnoreCase))
                    {
                        WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", messageProperties: new object[] { "PostBooks", "Hash compare failed" });
                        return BadRequest("Invalid Signature.");
                    }

                    try
                    {
                        logData = new Dictionary<string, object>();
                        logData.Add("thinEventPayload", thinEvent);
                        WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", logData, messageProperties: new object[] { "PostBooks", thinEvent.Topic.ToLowerInvariant() });
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
                                            logData = new Dictionary<string, object> { { "error", result } };

                                            WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", logData, messageProperties: new object[] { "PostBooks", "Error" });
                                            return BadRequest(ResultErrorMessage(result));
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
                                                Organization oldOrganization = new Organization() { PartyId = p.PartyId, BooksCustomerMasterId = p.BooksCustomerMasterId };
                                                Organization newOrganization = new Organization() { PartyId = p.PartyId, BooksCustomerMasterId = newCustomerCompanyId };
                                                RepositoryResponse result = _organizationRepository.UpdateOrganizationBooksCompanyMasterId(oldOrganization, newOrganization);
                                                if (result.ErrorMessage.Length != 0 || result.Id == 0)
                                                {
                                                    logData = new Dictionary<string, object> { { "error", result } };
                                                    WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", logData, messageProperties: new object[] { "PostBooks", "Error" });

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

                                        return BadRequest(errorText);
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
                                WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", messageProperties: new object[] { "PostBooks", $"In provisioning.upfmclone.create {thinEvent.Topic}" });

                                if (thinEvent.Topic.Equals("provisioning.upfmclone.create", StringComparison.OrdinalIgnoreCase))
                                {
                                    string hotsCloningEnabled = GetUnifiedPlatformSettings((int)ProductEnum.UnifiedPlatform)?.ToList().FirstOrDefault(s => s.Name.Equals("IsCloneUsersProcessEnabledForHOTS", StringComparison.OrdinalIgnoreCase))?.Value;
                                    if (string.IsNullOrEmpty(hotsCloningEnabled) || !hotsCloningEnabled.Equals("1", StringComparison.OrdinalIgnoreCase))
                                    {
                                        WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", messageProperties: new object[] { "PostBooks", "Environment not enabled for HOTS cloning" });
                                        return BadRequest($"Environment not enabled for HOTS cloning");
                                    }

                                    var cloneCompanyId = thinEvent.Payload?["company"]["cloneCompanyInstanceSourceId"] == null || thinEvent.Payload?["company"]["cloneCompanyInstanceSourceId"].Type == JTokenType.Null ? null : thinEvent.Payload?["company"]["cloneCompanyInstanceSourceId"].ToString();
                                    if (string.IsNullOrEmpty(cloneCompanyId))
                                    {
                                        WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", messageProperties: new object[] { "PostBooks", "Missing cloneCompanyInstanceSourceId" });
                                        return BadRequest("Missing cloneCompanyInstanceSourceId");
                                    }

                                    if (!Guid.TryParse(cloneCompanyId, out var baselineCompanyGuid))
                                    {
                                        WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", messageProperties: new object[] { "PostBooks", "Invalid cloneCompanyInstanceSourceId, not Guid" });
                                        return BadRequest("Invalid cloneCompanyInstanceSourceId, not Guid");
                                    }

                                    var cloneBaselineOrg = _manageOrganization.GetOrganization(baselineCompanyGuid);
                                    if (cloneBaselineOrg == null)
                                    {
                                        WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", null, null, new object[] { "PostBooks", $"HOTS Baseline Company {cloneCompanyId} not found" });
                                        return BadRequest($"HOTS Baseline Company {cloneCompanyId} not found");
                                    }
                                }

                                var propertyList = thinEvent.Payload["properties"];
                                string existingUnifiedLoginInstanceId = thinEvent.Payload?["company"]["companyInstanceSourceId"] == null || thinEvent.Payload?["company"]["companyInstanceSourceId"].Type == JTokenType.Null ? null : thinEvent.Payload?["company"]["companyInstanceSourceId"].ToString();

                                List<int> uniqueProductIdList = new List<int>();

                                List<UPFMPropertyInstance> propertyInstanceList = new List<UPFMPropertyInstance>();
                                ProductCenterEnablement centerEnablement = new ProductCenterEnablement() { Details = new List<ProductCenterEnablementSettings>() };
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
                                    logData = new Dictionary<string, object> { { "error", ex.Message } };
                                    WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", logData, exception: ex, messageProperties: new object[] { "PostBooks", "Error parsing company product list" });
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
                                            WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", logData, messageProperties: new object[] { "PostBooks", "Missing clonePropertyInstanceSourceId" });

                                            return BadRequest("Missing clonePropertyInstanceSourceId");
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
                                                logData = new Dictionary<string, object> { { "error", ex.Message } };
                                                WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", logData, exception: ex, messageProperties: new object[] { "PostBooks", "Error parsing property address longitude" });

                                            }
                                        }

                                        propertyInstanceList.Add(newProperty);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logData = new Dictionary<string, object> { { "error", ex.Message } };
                                    WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", logData, exception: ex, messageProperties: new object[] { "PostBooks", "Error parsing property list" });

                                }

                                if (string.IsNullOrEmpty(customerDomain))
                                {
                                    WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", messageProperties: new object[] { "PostBooks", "Missing customerEnvironment" });
                                    return BadRequest("Missing customerEnvironment");
                                }

                                if (existingUnifiedLoginInstanceId == null)
                                {
                                    WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", messageProperties: new object[] { "PostBooks", "Calling CreateCompanyFromBooks" });
                                    //return BadRequest("stop");
                                    var createResult = CreateCompanyFromBooks(thinEvent.Payload?["company"], customerCompanyId, customerDomain, uniqueProductIdList, thinEvent.Topic.ToLowerInvariant());
                                    if (!string.IsNullOrEmpty(createResult.Result))
                                    {
                                        propertyInstanceList = new List<UPFMPropertyInstance>();
                                        centerEnablement.Details = new List<ProductCenterEnablementSettings>();
                                        logData = new Dictionary<string, object> { { "error", createResult } };

                                        WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", logData, messageProperties: new object[] { "PostBooks", "Error in CreateCompanyFromBooks" });
                                        if (createResult.Result.Equals("Company not found in books environment", StringComparison.OrdinalIgnoreCase))
                                        {
                                            // shortcut out, this create may be for another environment
                                            return StatusCode(202);
                                        }

                                        return BadRequest(createResult);
                                    }

                                    if (!createResult.RealPageId.Equals(Guid.Empty.ToString()))
                                    {
                                        existingUnifiedLoginInstanceId = createResult.RealPageId;
                                    }
                                }

                                var org = _manageOrganization.GetOrganization(new Guid(existingUnifiedLoginInstanceId));
                                if (org == null)
                                {
                                    WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", null, null, new object[] { "PostBooks", $"Company {existingUnifiedLoginInstanceId} not found" });
                                    return BadRequest($"Company {existingUnifiedLoginInstanceId} not found");
                                }

                                List<int> deleteProductIds = new List<int>();
                                var sharedProductList = _productInternalSettingRepository.GetProductSettingByType(SettingConstants.SharedProductSettingName).ToList();
                                if (uniqueProductIdList.Count > 0)
                                {
                                    var cacheKey = $"getProductsByCompany_{org.RealPageId}";
                                    RPObjectCache.RemoveFromCache(cacheKey);

                                    var existingProductList = _organizationRepository.GetProductsByCompany(org.RealPageId);
                                    foreach (var product in sharedProductList)
                                    {
                                        if (uniqueProductIdList.Any(m => m == product.ProductId) && uniqueProductIdList.Any(m => m == Convert.ToInt32(product.Value)))
                                        {
                                            uniqueProductIdList.Remove(product.ProductId);
                                            uniqueProductIdList.Remove(Convert.ToInt32(product.Value));
                                        }
                                    }

                                    foreach (var productId in uniqueProductIdList)
                                    {
                                        var productinternalsettings = GetUnifiedPlatformSettings(productId);
                                        if (existingProductList.All(p => p.ProductId != productId))
                                        {
                                            var sharedProduct = sharedProductList.FirstOrDefault(p => p.ProductId == productId);
                                            if (sharedProduct != null)
                                            {
                                                deleteProductIds.Add(Convert.ToInt32(sharedProduct.Value));
                                            }
                                            sharedProduct = sharedProductList.FirstOrDefault(p => Convert.ToInt32(p.Value) == productId);
                                            if (sharedProduct != null)
                                            {
                                                deleteProductIds.Add(Convert.ToInt32(sharedProduct.ProductId));
                                            }
                                            var addresponse = _manageOrganizationProduct.InsertUpdateOrganizationProductFromProvisioning(productId, null, null, null, org);
                                        }
                                    }

                                    ProductCenterCancellation productCenterCancellation = new ProductCenterCancellation() { Details = new List<ProductCenterCancellationSettings>() };
                                    productCenterCancellation.CancelledBy = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform) + " Automation";
                                    if (org != null)
                                    {
                                        foreach (var productId in deleteProductIds)
                                        {
                                            _manageOrganizationProduct.DeleteOrganizationProduct(partyId: org.PartyId, product: productId, org: org);
                                            productCenterCancellation.Details.Add(new ProductCenterCancellationSettings()
                                            {
                                                CompanyInstanceSourceId = existingUnifiedLoginInstanceId,
                                                PropertyInstanceSourceId = null,
                                                ProductCenterSourceId = productId.ToString(),
                                                Source = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform)
                                            });
                                        }
                                    }
                                    else
                                    {
                                        WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", null, null, new object[] { "PostBooks", $"Company {existingUnifiedLoginInstanceId} not found" });
                                        return BadRequest($"Company {existingUnifiedLoginInstanceId} not found");
                                    }

                                    if (productCenterCancellation.Details.Count > 0)
                                    {
                                        _manageBlueBook.AcknowledgeProvisioningCancelEvent(productCenterCancellation);
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
                            case "books.upfmvendor.create":
                                logData.Add("VendorData", thinEvent.Payload ?? "null");
                                WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", logData, messageProperties: new object[] { "PostBooks", "Vendor Company creation started" });
                                var createVendorResult = CreateVendorCompanyFromWebhook(thinEvent.Payload);

                                if (string.IsNullOrEmpty(createVendorResult.Result))
                                {
                                    WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", logData, messageProperties: new object[] { "PostBooks", "Complete" });
                                    return StatusCode(202);
                                }

                                logData.Add("error", createVendorResult.Result);
                                WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", logData, messageProperties: new object[] { "PostBooks", "Error" });
                                return BadRequest(createVendorResult.Result);

                            case "provisioning.upfmorder.cancel":
                                var productListToCancel = thinEvent.Payload?["company"]["productCenters"];
                                string companyInstanceSourceId = thinEvent.Payload?["company"]["companyInstanceSourceId"] == null || thinEvent.Payload?["company"]["companyInstanceSourceId"].Type == JTokenType.Null ? null : thinEvent.Payload?["company"]["companyInstanceSourceId"].ToString();
                                if (string.IsNullOrEmpty(companyInstanceSourceId))
                                {
                                    WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", messageProperties: new object[] { "PostBooks", "companyInstanceSourceId should not be null or empty" });
                                    return BadRequest($"Invalid companyInstanceSourceId");
                                }

                                ProductCenterCancellation centerCancel = new ProductCenterCancellation() { Details = new List<ProductCenterCancellationSettings>() };
                                centerCancel.CancelledBy = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform) + " Automation";
                                var orgDetails = _manageOrganization.GetOrganization(new Guid(companyInstanceSourceId));
                                if (orgDetails != null)
                                {
                                    foreach (var product in productListToCancel)
                                    {
                                        if (product["productCenterSourceId"] != null && product["productCenterSourceId"].ToString() != "")
                                        {
                                            var addresponse = _manageOrganizationProduct.DeleteOrganizationProduct(partyId: orgDetails.PartyId, product: Convert.ToInt32(product["productCenterSourceId"]), org: orgDetails);
                                            _manageOrganizationProduct.DisableUsersForProduct(partyId: orgDetails.PartyId, product: (ProductEnum)Convert.ToInt32(product["productCenterSourceId"]));
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
                                            WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", messageProperties: new object[] { "PostBooks", $"Invalid ProductCenterSourceId - {product["productCenterSourceId"]}" });

                                            return BadRequest("Invalid ProductCenterSourceId");
                                        }
                                    }
                                }
                                else
                                {
                                    WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", null, null, new object[] { "PostBooks", $"Company {companyInstanceSourceId} not found" });
                                    return BadRequest($"Company {companyInstanceSourceId} not found");
                                }

                                if (centerCancel.Details.Count > 0)
                                {
                                    _manageBlueBook.AcknowledgeProvisioningCancelEvent(centerCancel);
                                }

                                break;
                            default:
                                return StatusCode(202);
                        }
                    }

                    catch (Exception ex)
                    {
                        WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", exception: ex, messageProperties: new object[] { "PostBooks", "Error" });
                        return BadRequest(ex.Message);
                    }
                }

                logData.Add("response.StatusCode", 202);
                WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", logData, messageProperties: new object[] { "PostBooks", "Complete" });
                return StatusCode(202);
            });
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
                                WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", null, null, new object[] { "AddPropertiesFromBooks", $"Failed to add HOTS property relationship. baseline {property.ClonePropertyInstanceSourceId} clone {property.InstanceId}" });
                            }
                        }
                    }
                    else
                    {
                        var logData = new Dictionary<string, object>
                        {
                            { "property", property },
                            { "response.ErrorMessage", response.ErrorMessage }
                        };
                        WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", logData, messageProperties: new object[] { "AddPropertiesFromBooks", "Error" });
                    }
                }
            }

            return result;
        }

        private List<ProductInternalSetting> GetUnifiedPlatformSettings(int productId)
        {
            var productInternalSettingList = new List<ProductInternalSetting>();
            RPObjectCache rpcache = new RPObjectCache();
            var cacheKey = "productInternalSetting_" + (int)productId;
            productInternalSettingList = rpcache.GetFromCache(cacheKey, 120, () =>
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
            CreateCompanyResult createCompanyResult = new CreateCompanyResult() { RealPageId = Guid.Empty.ToString() };

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
                CompanyAddress = new CompanyInstanceAddress() { Address = companyAddress, City = companyCity, State = companyState, PostalCode = companyPostalCode, County = companyCounty, Country = companyCountry }
            };

            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", messageProperties: new object[] { "CreateCompanyFromBooks", $"Adding company {companyName}" });
            var organizationTypeList = _manageOrganization.ListOrganizationType();
            var organizationDomainList = _manageOrganization.ListOrganizationDomain();

            var orgType = organizationTypeList.FirstOrDefault(p => p.Name.Equals(customerCompany.CompanyType, StringComparison.OrdinalIgnoreCase));
            if (orgType == null)
            {
                orgType = organizationTypeList.FirstOrDefault(p => p.Name.Equals("Other", StringComparison.OrdinalIgnoreCase));
            }

            organization.OrganizationTypeId = orgType.OrganizationTypeId;

            if (!organizationDomainList.Any(d => d.Name.Equals(domain, StringComparison.OrdinalIgnoreCase)))
            {
                RepositoryResponse response = _manageOrganization.CreateOrganizationDomain(new OrganizationDomain() { Name = domain });
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

            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", messageProperties: new object[] { "CreateCompanyFromBooks", $"Before creating Organization {companyName}" });
            var result = _manageOrganization.CreateOrganization(organization, addProductList, processBlueBookMessage);

            if (!result.Status.Success || !string.IsNullOrEmpty(result.Status.ErrorMsg))
            {
                WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", null, null, new object[] { "CreateCompanyFromBooks", $"Error Message while creating organization {result.Status.ErrorMsg}" });
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
                        WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", null, null, new object[] { "CreateCompanyFromBooks", "Error inserting HOTS company relationship" });

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

        private CreateCompanyResult CreateVendorCompanyFromWebhook(JToken payLoad)
        {
            var createCompanyResult = new CreateCompanyResult();
            var logdata = new Dictionary<string, object>();
            logdata.Add("Additional", payLoad);

            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", logdata, null, new object[] { "CreateVendorCompanyFromWebhook", $"Payload for Vendor company in Additional key" });
            var customerCompanyId = Convert.ToInt64(payLoad?["customerCompanyId"] == null || payLoad["customerCompanyId"].Type == JTokenType.Null ? 0 : payLoad?["customerCompanyId"]);
            var productSource = payLoad?["source"].ToString();
            var productSourceId = payLoad?["companyInstanceSourceId"].ToString();
            var adminEmail = payLoad?["user"]["email"] == null || payLoad?["user"]["email"].Type == JTokenType.Null ? "" : payLoad["user"]["email"].ToString();
            var adminFirstName = payLoad?["user"]["firstName"] == null || payLoad?["user"]["firstName"].Type == JTokenType.Null ? "" : payLoad["user"]["firstName"].ToString();
            var adminLastName = payLoad?["user"]["lastName"] == null || payLoad?["user"]["lastName"].Type == JTokenType.Null ? "" : payLoad["user"]["lastName"].ToString();
            var roles = payLoad?["user"]["roles"] == null || payLoad?["user"]["roles"].Type == JTokenType.Null ? "" : payLoad["user"]["roles"];
            List<string> rolesList = roles == null || roles.Type == JTokenType.Null ? new List<string>() : roles.Select(r => Convert.ToString(r)).ToList();

            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", logdata, null, new object[] { "CreateVendorCompanyFromWebhook", $"Extracted variables from payload for customerCompanyId-{customerCompanyId}" });
            var customerCompany = _manageBlueBook.GetCompanyCustomerInfo(companyRealPageId: Guid.Empty, domain: null, booksCompanyMasterId: customerCompanyId);
            if (customerCompany == null)
            {
                createCompanyResult.Result = "Company not found in books environment";
                return createCompanyResult;
            }

            var company = _manageBlueBook.GetBooksCompanyDetailsByCompanyMasterId(customerCompany.CustomerCompanyId);
            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", logdata, null, new object[] { "CreateVendorCompanyFromWebhook", $"Got company details from books for customerCompanyId-{customerCompanyId}" });
            var existingInstances = _manageBlueBook.GetCompanyInstancesByCustomerCompanyId(customerCompany.CustomerCompanyId);
            var vendorInstance = _manageBlueBook.GetCompanyInstanceBySourceAndInstanceId(productSourceId, productSource);
            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", logdata, null, new object[] { "CreateVendorCompanyFromWebhook", $"got existing instances and vendor instance from books for customerCompanyId-{customerCompanyId}" });
            if (vendorInstance == null)
            {
                WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", null, null, new object[] { "CreateVendorCompanyFromWebhook", $"ProductSource {productSource} company not found. productSourceId {productSourceId}" });
                createCompanyResult.Result = "Vendor instance not found in books environment";
                return createCompanyResult;
            }

            if (existingInstances != null && existingInstances.Any(p => p.attributes.Domain.Equals(vendorInstance.Domain, StringComparison.OrdinalIgnoreCase) && p.attributes.Source.Equals("UPFM")))
            {
                WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", messageProperties: new object[] { "CreateVendorCompanyFromWebhook", $"UPFM vendor company {customerCompany.CompanyName} already exists. CustomerCompanyId {customerCompany.CustomerCompanyId}" });
                createCompanyResult.Result = "UPFM instance already exists";
                return createCompanyResult;
            }

            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", logdata, null, new object[] { "CreateVendorCompanyFromWebhook", $"Building Organization payload for customerCompanyId-{customerCompanyId}" });

            var organization = new OrganizationCreate()
            {
                Name = customerCompany.CompanyName,
                BooksCompanyId = customerCompany.MasterCompanyId,
                BooksCustomerMasterId = customerCompany.CustomerCompanyId,
                AdminUser = new OrganizationAdminUser()
                {
                    FirstName = "RealPage",
                    LastName = "Access",
                    Suffix = string.Empty,
                    Title = string.Empty,
                },
                //CompanyAddress = new CompanyInstanceAddress() { Address = companyAddress, City = companyCity, State = companyState, PostalCode = companyPostalCode, County = companyCounty, Country = companyCountry },
                CompanyAdminUser = new OrganizationAdminUser()
                {
                    Email = adminEmail,
                    FirstName = adminFirstName,
                    LastName = adminLastName,
                    RoleIds = rolesList
                },
                Products = new List<string>() { productSource },
                CompanyInstancePartner = productSource,
                CompanyInstancePartnerSourceId = productSourceId,
            };
            logdata.Add("organization", organization);
            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", logdata, null, new object[] { "CreateVendorCompanyFromWebhook", $"Organization payload in Organization key for customerCompanyId-{customerCompanyId}" });

            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", messageProperties: new object[] { "CreateVendorCompanyFromWebhook", $"Adding vendor company {customerCompany.CompanyName}" });
            var organizationTypeList = _manageOrganization.ListOrganizationType();
            var organizationDomainList = _manageOrganization.ListOrganizationDomain();
            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", logdata, null, new object[] { "CreateVendorCompanyFromWebhook", $"Got org types and org domains for customerCompanyId-{customerCompanyId}" });
            var orgType = organizationTypeList.FirstOrDefault(p => p.Name.Equals(customerCompany.CompanyType, StringComparison.OrdinalIgnoreCase));
            if (orgType == null)
            {
                orgType = organizationTypeList.FirstOrDefault(p => p.Name.Equals("Other", StringComparison.OrdinalIgnoreCase));
            }

            organization.OrganizationTypeId = orgType.OrganizationTypeId;

            if (!organizationDomainList.Any(d => d.Name.Equals(vendorInstance.Domain, StringComparison.OrdinalIgnoreCase)))
            {
                var response = _manageOrganization.CreateOrganizationDomain(new OrganizationDomain() { Name = vendorInstance.Domain });
                if (response.Id > 0)
                {
                    organization.OrganizationDomainId = Convert.ToInt32(response.Id);
                }
            }
            else
            {
                organization.OrganizationDomainId = organizationDomainList.FirstOrDefault(p => p.Name.Equals(vendorInstance.Domain, StringComparison.OrdinalIgnoreCase)).OrganizationDomainId;
            }

            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", logdata, null, new object[] { "CreateVendorCompanyFromWebhook", $"Organization domain logic completed for customerCompanyId-{customerCompanyId}" });
            var productList = _manageProduct.ListProducts();

            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", logdata, null, new object[] { "CreateVendorCompanyFromWebhook", $"Got list of products for customerCompanyId-{customerCompanyId}" });
            var addProductList = new List<int>();
            var productDetails = productList.FirstOrDefault(p => p.BooksProductCode.Equals(productSource, StringComparison.OrdinalIgnoreCase));
            if (productDetails != null)
            {
                addProductList.Add(productDetails.ProductId);
            }
            logdata.Add("orgdata", organization);
            logdata.Add("orgdataproducts", addProductList);
            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", logdata, null, new object[] { "CreateVendorCompanyFromWebhook", $"Before Organization creation for customerCompanyId-{customerCompanyId}" });

            var createOrgResult = _manageOrganization.CreateOrganization(organization, addProductList, true);

            logdata.Add("createOrgResult", createOrgResult);
            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", logdata, null, new object[] { "CreateVendorCompanyFromWebhook", $"After Organization creation for customerCompanyId-{customerCompanyId}" });

            if (!createOrgResult.Status.Success || !string.IsNullOrEmpty(createOrgResult.Status.ErrorMsg))
            {
                WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", null, null, new object[] { "CreateVendorCompanyFromWebhook", $"Error Message while creating organization {createOrgResult.Status.ErrorMsg}" });
                createCompanyResult.Result = createOrgResult.Status.ErrorMsg;
                return createCompanyResult;
            }

            var companyInstance = new CompanyInstanceAdd
            {
                Id = (long)organization.BooksCustomerMasterId,
                CustomerCompanyId = null,
                CompanyInstanceSourceId = createOrgResult.obj.Org.RealPageId.ToString().ToLower(),
                CompanyName = createOrgResult.obj.Org.Name,
                Source = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform),
                IsActive = true,
                ModifiedBy = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform) + " Automation",
                CustomerEnvironment = createOrgResult.obj.Org.OrganizationDomain.Name,
                CompanyType = customerCompany.CompanyType,
                CompanyInstancePartners = new List<CompanyInstancePartner>() { new CompanyInstancePartner() { TargetSource = productSource, TargetCompanyInstanceSourceId = productSourceId } }
            };

            // do we ever have an address?
            //if (organization.CompanyAddress != null)
            //{
            //    CompanyInstanceAddress address = new CompanyInstanceAddress()
            //    {
            //        Address = organization.CompanyAddress.Address,
            //        City = organization.CompanyAddress.City,
            //        State = organization.CompanyAddress.State,
            //        PostalCode = organization.CompanyAddress.PostalCode,
            //        County = organization.CompanyAddress.County,
            //        Country = organization.CompanyAddress.Country
            //    };
            //    companyInstance.CompanyInstanceLocation = new List<CompanyInstanceAddress>() { address };
            //}
            logdata.Add("UDMPayload", companyInstance);
            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", logdata, null, new object[] { "CreateVendorCompanyFromWebhook", $"Calling UDM to update company instance for customerCompanyId-{customerCompanyId}" });
            // add the new company data to books
            var companyCreatedSuccessfully = _manageBlueBook.AddUPFMCompanyFromCompanySetup(companyInstance);
            logdata.Add("UDMPayloadresponse", companyCreatedSuccessfully);
            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", logdata, null, new object[] { "CreateVendorCompanyFromWebhook", $"Company instance updated in UDM for customerCompanyId-{customerCompanyId}" });

            if (!companyCreatedSuccessfully)
            {
                createCompanyResult.Result = "There was a problem adding the UPFM instance to UDM";
                return createCompanyResult;
            }

            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", logdata, null, new object[] { "CreateVendorCompanyFromWebhook", $"Calling settings to create instance for customerCompanyId-{customerCompanyId}" });
            if (!_manageOrganization.AddUpdateCompanyToUnifiedSettings(companyInstance.CompanyInstanceSourceId, "Create", companyInstance.CustomerEnvironment))
            {
                createCompanyResult.Result = "Unified Login and MDM company was updated successfully but Settings data update failed.";
                return createCompanyResult;
            }
            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", logdata, null, new object[] { "CreateVendorCompanyFromWebhook", $"Settings instance created for customerCompanyId-{customerCompanyId}" });

            // add the products assigned to the new company
            var cacheKey = $"getListProductsByOrganization_{createOrgResult.obj.Org.RealPageId}";
            MemoryCache.Default.Remove(cacheKey);

            var productListOrg = _manageProduct.GetProducts(createOrgResult.obj.Org.RealPageId, 0, true);
            logdata.Add("productListOrg", productListOrg);
            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", logdata, null, new object[] { "CreateVendorCompanyFromWebhook", $"Got product list for customerCompanyId-{customerCompanyId}" });
            foreach (var product in productListOrg)
            {
                var productInternalSettings = _manageProduct.GetProductInternalSettings(product.ProductId);
                var updateInUDM = productInternalSettings.FirstOrDefault(x => x.Name.Equals("UpdateProductInUDM", StringComparison.OrdinalIgnoreCase));

                if (updateInUDM?.Value != "1") continue;

                var spc = new SystemProductCenter()
                {
                    Id = 0,
                    CompanyInstanceSourceId = companyInstance.CompanyInstanceSourceId,
                    CreatedBy = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform) + " Automation",
                    ProductCenterSourceId = product.ProductId.ToString(),
                    Source = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform)
                };
                _manageBlueBook.ProductCenterEnable(spc);
                WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", null, null, new object[] { "CreateVendorCompanyFromWebhook", $"Product center enable call for customerCompanyId-{customerCompanyId}" });
            }
            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", logdata, null, new object[] { "CreateVendorCompanyFromWebhook", $"Vendor company created in UPFM for customerCompanyId-{customerCompanyId}" });
            return new CreateCompanyResult() { RealPageId = createCompanyResult.RealPageId };
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
        /// Used to write to the central log
        /// </summary>
        /// <param name="logType">Log Type</param>
        /// <param name="message">Message template</param>
        /// <param name="logData">Dictionary of additional properties to log</param>
        /// <param name="exception">Exception details</param>
        /// <param name="messageProperties">Message properties</param>
        private void WriteToLog(LogEventLevel logType, string message, Dictionary<string, object> logData = null, Exception exception = null, object[] messageProperties = null)
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
                logger.Write(level: logType, exception: exception, messageTemplate: message, propertyValue0: messageProperties?[0], propertyValue1: messageProperties?[1]);
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
