using Aspose.Cells;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;
using UnifiedLogin.BusinessLogic.Attributes;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.Product.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.BusinessLogic.ThirdParty;
using UnifiedLogin.Core;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Batch;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Landing.Export;
using UnifiedLogin.SharedObjects.Maintenance;
using UnifiedLogin.SharedObjects.Product;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// Used to insert/update the Blue Book Organization in the Green Book system
    /// </summary>
    [Route("")]
    [ApiController]
    public class OrganizationController : BaseController
    {
        #region Private Fields

        private readonly IManageOrganizationProduct _manageOrganizationProduct;
        private readonly IManageCustomFields _manageCustomFields;
        private readonly IManageUserLogin _manageUserLogin;
        private readonly IManagePartyRelationship _managePartyRelationship;
        private readonly IManageOrganization _manageOrganization;
        private readonly IManageBlueBook _manageBlueBook;
        private readonly IProductInternalSettingRepository _productInternalSettingRepository;
        private readonly IManageProduct _manageProduct;
        private readonly IManageCredential _manageCredential;
        private readonly IManagePerson _managePerson;
        private readonly IManagePersona _managePersona;
        private readonly IManageProductOneSite _manageProductOneSite;
        private readonly IMemoryCache _memoryCache;
        private readonly int _maxDOPSetting = 6;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public OrganizationController(
            IManageOrganizationProduct manageOrganizationProduct,
            IManageCustomFields manageCustomFields,
            IManageUserLogin manageUserLogin,
            IManagePartyRelationship managePartyRelationship,
            IManageOrganization manageOrganization,
            IManageBlueBook manageBlueBook,
            IProductInternalSettingRepository productInternalSettingRepository,
            IManageProduct manageProduct,
            IManageCredential manageCredential,
            IManagePerson managePerson,
            IManagePersona managePersona,
            IManageProductOneSite manageProductOneSite,
            IMemoryCache memoryCache,
            IUserClaimsAccessor userClaimsAccessor) : base(userClaimsAccessor)
        {
            _manageOrganizationProduct = manageOrganizationProduct ?? throw new ArgumentNullException(nameof(manageOrganizationProduct));
            _manageCustomFields = manageCustomFields ?? throw new ArgumentNullException(nameof(manageCustomFields));
            _manageUserLogin = manageUserLogin ?? throw new ArgumentNullException(nameof(manageUserLogin));
            _managePartyRelationship = managePartyRelationship ?? throw new ArgumentNullException(nameof(managePartyRelationship));
            _manageOrganization = manageOrganization ?? throw new ArgumentNullException(nameof(manageOrganization));
            _manageBlueBook = manageBlueBook ?? throw new ArgumentNullException(nameof(manageBlueBook));
            _productInternalSettingRepository = productInternalSettingRepository ?? throw new ArgumentNullException(nameof(productInternalSettingRepository));
            _manageProduct = manageProduct ?? throw new ArgumentNullException(nameof(manageProduct));
            _manageCredential = manageCredential ?? throw new ArgumentNullException(nameof(manageCredential));
            _managePerson = managePerson ?? throw new ArgumentNullException(nameof(managePerson));
            _managePersona = managePersona ?? throw new ArgumentNullException(nameof(managePersona));
            _manageProductOneSite = manageProductOneSite ?? throw new ArgumentNullException(nameof(manageProductOneSite));
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        }

        #endregion

        #region Organization CRUD Operations

        /// <summary>
        /// List Company Custom Fields
        /// </summary>
        /// <returns>A list of company's customfields</returns>
        [HttpGet("organization/customfields")]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> OrganizationCustomFields([FromQuery] RequestParameter datafilter)
        {
            return await Task.Run<IActionResult>(() =>
            {
                IDictionary<object, object> globals = new Dictionary<object, object>();

                if (datafilter == null)
                {
                    datafilter = new RequestParameter();
                }

                globals.Add(BaseType.RequestParameter, datafilter);

                IList<CustomField> customFieldList = _manageCustomFields.GetCustomField(globals: globals, partyId: _userClaimsAccessor.OrganizationPartyId);

                ListResponse response = new ListResponse()
                {
                    Records = customFieldList.Cast<object>().ToList(),
                    TotalRows = customFieldList.Count(),
                    RowsPerPage = customFieldList.Count(),
                    ErrorReason = string.Empty,
                    TotalPages = 1
                };
                return Ok(response);
            });
        }

        /// <summary>
        /// Insert the Organization in the GreenBook system
        /// </summary>
        /// <param name="organization">The organization information</param>
        /// <param name="processBlueBookMessage">Process a RabbitMQ BlueBook Message to Create a company, RealPage Employee admin user, and a company Admin user</param>
        /// <returns>The Organization that was created</returns>
        [HttpPost]
        [Route("organization")]
        [AuthorizeScope("companyfunctions", "rplandingapi")]
        [ProducesResponseType(typeof(Organization), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> InsertOrganization([FromBody] OrganizationCreate organization, [FromQuery] bool processBlueBookMessage = false)
        {
            return await Task.Run<IActionResult>(() =>
            {
                if (!string.IsNullOrEmpty(organization.CompanyInstancePartnerSourceId) && !string.IsNullOrEmpty(organization.CompanyInstancePartner))
                {
                    var partnerInstance = _manageBlueBook.GetCompanyInstanceBySourceAndInstanceId(organization.CompanyInstancePartnerSourceId, organization.CompanyInstancePartner);
                    if (partnerInstance == null)
                    {
                        return BadRequest("Partner instance could not be found");
                    }
                }

                var organizationDomainList = _manageOrganization.ListOrganizationDomain();

                if (!organizationDomainList.Exists(d => d.Name.Equals(organization.OrganizationDomain, StringComparison.OrdinalIgnoreCase)))
                {
                    RepositoryResponse response = _manageOrganization.CreateOrganizationDomain(new OrganizationDomain() { Name = organization.OrganizationDomain });
                    if (response.Id > 0)
                    {
                        organization.OrganizationDomainId = Convert.ToInt32(response.Id);
                    }
                }
                else
                {
                    organization.OrganizationDomainId = organizationDomainList.Find(p => p.Name.Equals(organization.OrganizationDomain, StringComparison.OrdinalIgnoreCase)).OrganizationDomainId;
                }

                var addProductList = new List<int>();
                // verify the products, if any, exist and can be added to the customer
                List<string> invalidProductList = _manageOrganization.ParseProduct(organization.Products, addProductList);

                if (invalidProductList.Count > 0)
                {
                    return BadRequest("An invalid product was given : " + String.Join(",", invalidProductList));
                }

                var result = _manageOrganization.CreateOrganization(organization, addProductList, processBlueBookMessage);

                if (!result.Status.Success)
                {
                    return BadRequest(result.Status.ErrorMsg);
                }

                IList<CustomerCompanyMap> companyMapResource = null;
                try
                {
                    companyMapResource = _manageBlueBook.GetCompanyMap(companyRealPageId: result.obj.Org.RealPageId, booksCompanyMasterId: (long)organization.BooksCustomerMasterId, source: ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform), domain: result.obj.Org.OrganizationDomain.Name, includeGreenBookCares: false);
                }
                catch (Exception)
                {
                    // ignored
                }

                var companyTypeName = _manageOrganization.ListOrganizationType()?.Find(t => t.OrganizationTypeId == organization.OrganizationTypeId)?.Name;
                // add the new company to books
                var companyInstance = new CompanyInstanceAdd()
                {
                    Id = (long)organization.BooksCustomerMasterId,
                    CustomerCompanyId = null,
                    CompanyInstanceSourceId = result.obj.Org.RealPageId.ToString().ToLower(),
                    CompanyName = result.obj.Org.Name,
                    Source = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform),
                    IsActive = organization.IsActive == 1,
                    ModifiedBy = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform) + " Automation",
                    CustomerEnvironment = result.obj.Org.OrganizationDomain.Name,
                    CompanyType = companyTypeName
                };

                if (organization.CompanyAddress != null)
                {
                    CompanyInstanceAddress address = new CompanyInstanceAddress()
                    {
                        Address = organization.CompanyAddress.Address,
                        City = organization.CompanyAddress.City,
                        State = organization.CompanyAddress.State,
                        PostalCode = organization.CompanyAddress.PostalCode,
                        County = organization.CompanyAddress.County,
                        Country = organization.CompanyAddress.Country
                    };
                    companyInstance.CompanyInstanceLocation = new List<CompanyInstanceAddress>() { address };
                }

                if (!string.IsNullOrEmpty(organization.CompanyInstancePartner) && !string.IsNullOrEmpty(organization.CompanyInstancePartnerSourceId))
                {
                    companyInstance.CompanyInstancePartners = new List<CompanyInstancePartner>() { new CompanyInstancePartner() { TargetSource = organization.CompanyInstancePartner, TargetCompanyInstanceSourceId = organization.CompanyInstancePartnerSourceId } };
                }

                bool addInstance = true;

                if (companyMapResource != null)
                {
                    // remove any existing instance and add a new one
                    foreach (var customerCompanyMap in companyMapResource)
                    {
                        customerCompanyMap.CompanyInstance?.ForEach(i =>
                        {
                            if (i.CustomerEnvironment != null && i.CustomerEnvironment.Equals(companyInstance.CustomerEnvironment, StringComparison.OrdinalIgnoreCase))
                            {
                                addInstance = false;
                            }

                            if (i.Domain != null && i.Domain.Equals(companyInstance.CustomerEnvironment, StringComparison.OrdinalIgnoreCase))
                            {
                                addInstance = false;
                            }
                        });
                    }
                }

                // add the new company data to books
                if (addInstance)
                {
                    var companyCreatedSuccessfully = _manageBlueBook.AddUPFMCompanyFromCompanySetup(companyInstance);

                    if (!companyCreatedSuccessfully)
                    {
                        return BadRequest("There was a problem adding the UPFM instance to UDM");
                    }

                    if (!_manageOrganization.AddUpdateCompanyToUnifiedSettings(companyInstance.CompanyInstanceSourceId, "Create", companyInstance.CustomerEnvironment))
                    {
                        return BadRequest($"Unified Login and MDM company was updated successfully but Settings data update failed.");
                    }

                    // add the products assigned to the new company
                    var cacheKey = $"getListProductsByOrganization_{result.obj.Org.RealPageId}";
                    _memoryCache.Remove(cacheKey);

                    IList<ProductUI> productList = _manageProduct.GetProducts(result.obj.Org.RealPageId, 0, true);
                    foreach (var product in productList)
                    {
                        var productInternalSettings = _manageProduct.GetProductInternalSettings(product.ProductId);
                        var updateInUDM = productInternalSettings.Find(x => x.Name.Equals("UpdateProductInUDM", StringComparison.OrdinalIgnoreCase));

                        if (updateInUDM?.Value == "1")
                        {
                            SystemProductCenter spc = new SystemProductCenter()
                            {
                                Id = 0,
                                CompanyInstanceSourceId = companyInstance.CompanyInstanceSourceId,
                                CreatedBy = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform) + " Automation",
                                ProductCenterSourceId = product.ProductId.ToString(),
                                Source = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform)
                            };
                            _manageBlueBook.ProductCenterEnable(spc);
                        }
                    }
                }

                return Ok(result.obj);
            });
        }

        /// <summary>
        /// Update the Organization in the GreenBook system
        /// </summary>
        /// <param name="organization">The organization information to update</param>
        /// <returns>The Organization that was updated</returns>
        [HttpPut]
        [Route("organization")]
        [ProducesResponseType(typeof(Organization), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateOrganization([FromBody] OrganizationUpdate organization)
        {
            return await Task.Run<IActionResult>(() =>
            {
                Organization org = null;
                CompanyLocation oldAddress = null;

                if (organization != null)
                {
                    // get the org by UL realpageID
                    org = _manageOrganization.GetOrganization(organization.RealPageId);
                    oldAddress = _manageOrganization.GetCompanyList(org.Name, 0, null, 0, new Dictionary<object, object>())?.FirstOrDefault()?.CompanyLocation;
                }
                else
                {
                    return NotFound("Not found");
                }

                if (org == null)
                {
                    return NotFound("Not found");
                }

                bool orgNameChanged = org.Name != organization.Name;
                bool orgStatusChanged = org.IsActive != organization.IsActive;
                bool orgTypeChanged = org.OrganizationTypeId != organization.OrganizationTypeId;
                bool orgAddressChanged = false;

                //Did the address change
                if (organization.CompanyAddress != null && oldAddress != null)
                {
                    if (!organization.CompanyAddress.Address.Equals(oldAddress.Address, StringComparison.OrdinalIgnoreCase) ||
                        !organization.CompanyAddress.City.Equals(oldAddress.City, StringComparison.OrdinalIgnoreCase) ||
                        !organization.CompanyAddress.County.Equals(oldAddress.County, StringComparison.OrdinalIgnoreCase) ||
                        !organization.CompanyAddress.Country.Equals(oldAddress.Country, StringComparison.OrdinalIgnoreCase) ||
                        !organization.CompanyAddress.State.Equals(oldAddress.State, StringComparison.OrdinalIgnoreCase) ||
                        !organization.CompanyAddress.PostalCode.Equals(oldAddress.PostalCode, StringComparison.OrdinalIgnoreCase))
                    {
                        orgAddressChanged = true;
                    }
                }
                else if ((organization.CompanyAddress != null && oldAddress == null) ||
                         (organization.CompanyAddress == null && oldAddress != null))
                {
                    // Address added or removed
                    orgAddressChanged = true;
                }

                org.Name = organization.Name;
                org.IsActive = organization.IsActive;
                org.EnableEnterpriseRoles = organization.EnableEnterpriseRoles;
                org.EnablePrimaryProperties = organization.EnablePrimaryProperties;

                var orgTypes = _manageOrganization.ListOrganizationType();
                if (organization.OrganizationTypeId != 0)
                {
                    if (!orgTypes.Exists(o => o.OrganizationTypeId == organization.OrganizationTypeId))
                    {
                        return BadRequest("Invalid organization type id");
                    }
                }
                else if (!string.IsNullOrEmpty(organization.OrganizationTypeName))
                {
                    if (orgTypes.Exists(o => o.Name.Equals(organization.OrganizationTypeName, StringComparison.OrdinalIgnoreCase)))
                    {
                        organization.OrganizationTypeId = orgTypes.Find(o => o.Name.Equals(organization.OrganizationTypeName, StringComparison.OrdinalIgnoreCase)).OrganizationTypeId;
                    }
                    else
                    {
                        return BadRequest("Invalid organization type");
                    }
                }
                else
                {
                    return BadRequest("Missing organization type");
                }

                var orgDomains = _manageOrganization.ListOrganizationDomain();
                if (organization.OrganizationDomainId != 0)
                {
                    if (orgDomains.TrueForAll(o => o.OrganizationDomainId != organization.OrganizationDomainId))
                    {
                        return BadRequest("Invalid organization domain id");
                    }
                }
                else if (!string.IsNullOrEmpty(organization.OrganizationDomainName))
                {
                    if (orgDomains.Exists(o => o.Name.Equals(organization.OrganizationDomainName, StringComparison.OrdinalIgnoreCase)))
                    {
                        organization.OrganizationDomainId = orgDomains.Find(o => o.Name.Equals(organization.OrganizationDomainName, StringComparison.OrdinalIgnoreCase)).OrganizationDomainId;
                    }
                    else
                    {
                        return BadRequest("Invalid organization domain");
                    }
                }
                else
                {
                    return BadRequest("Missing organization domain");
                }

                org.OrganizationTypeId = organization.OrganizationTypeId;
                org.organizationType = _manageOrganization.ListOrganizationType()?.Find(t => t.OrganizationTypeId == organization.OrganizationTypeId);
                org.OrganizationDomain.OrganizationDomainId = organization.OrganizationDomainId;
                org.ThirdPartyIDP = organization.ThirdPartyIDP;

                var repositoryResponse = _manageOrganization.UpdateOrganization(org);

                if (!String.IsNullOrEmpty(repositoryResponse.ErrorMessage))
                {
                    return BadRequest(repositoryResponse.ErrorMessage);
                }

                _manageOrganization.UpdateOrganizationUsePrimaryPropertySetting(org);
                _manageOrganization.UpdateOrganizationThirdPartyIDP(org);

                if (orgNameChanged || orgStatusChanged || orgAddressChanged || orgTypeChanged)
                {
                    // update the name in MDM
                    IList<CustomerCompanyMap> companyMapResource = null;
                    companyMapResource = _manageBlueBook.GetCompanyMap(companyRealPageId: org.RealPageId, booksCompanyMasterId: org.BooksCustomerMasterId, source: ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform), domain: organization.OrganizationDomainName, includeGreenBookCares: false);

                    if (companyMapResource != null && companyMapResource.Any(c => c.CompanyInstanceSourceId == org.RealPageId.ToString()))
                    {
                        var companyMap = companyMapResource.FirstOrDefault(c => c.CompanyInstanceSourceId == org.RealPageId.ToString());
                        var companyTypeName = _manageOrganization.ListOrganizationType()?.Find(t => t.OrganizationTypeId == organization.OrganizationTypeId)?.Name;

                        CompanyInstanceAdd updateCompanyInstance = new CompanyInstanceAdd()
                        {
                            CompanyInstanceId = null,
                            CompanyInstanceSourceId = companyMap.CompanyInstanceSourceId,
                            CompanyName = org.Name,
                            CustomerCompanyId = null,
                            IsActive = organization.IsActive == 1,
                            Source = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform),
                            CustomerEnvironment = null,
                            ModifiedBy = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform) + " Automation",
                            CompanyType = companyTypeName
                        };

                        if (organization.CompanyAddress != null)
                        {
                            CompanyInstanceAddress address = new CompanyInstanceAddress()
                            {
                                Address = organization.CompanyAddress.Address,
                                City = organization.CompanyAddress.City,
                                State = organization.CompanyAddress.State,
                                PostalCode = organization.CompanyAddress.PostalCode,
                                County = organization.CompanyAddress.County,
                                Country = organization.CompanyAddress.Country
                            };
                            updateCompanyInstance.CompanyInstanceLocation = new List<CompanyInstanceAddress>() { address };
                        }
                        // Checkpoint: update the company instance in UDM
                        var booksResult = _manageBlueBook.UpdateBooksGreenBookCompanyInstance(updateCompanyInstance, oldAddress);
                        if (!string.IsNullOrEmpty(booksResult))
                        {
                            return BadRequest($"Unified Login company was updated successfully but MDM data update failed. Error: " + booksResult);
                        }

                        // Activate or deactivate the company properties in UDM only if the organization status has changed
                        if (orgStatusChanged)
                        {
                            var status = _manageOrganization.AddCompanyToJob(org.RealPageId.ToString(), _userClaimsAccessor.UserId, _userClaimsAccessor.PersonaId, org.IsActive);
                        }
                        // Checkpoint: updating the company instance in UDM is successful
                        if (!_manageOrganization.AddUpdateCompanyToUnifiedSettings(org.RealPageId.ToString(), "Update", null))
                        {
                            return BadRequest($"Unified Login and MDM company was updated successfully but Settings data update failed.");
                        }
                       
                    }
                    else
                    {
                        return BadRequest($"Unified Login company was updated successfully but MDM data failed because the {ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform)} company instance could not be found");
                    }
                }

                org = _manageOrganization.GetOrganization(org.RealPageId);

                return Ok(org);
            });
        }

        /// <summary>
        /// Used to get details for the given Organization id
        /// </summary>
        /// <param name="realPageId">The unique identifier for the organization</param>
        /// <returns>The Organization information for the given id</returns>
        [HttpGet("organization/{realPageId}")]
        [AuthorizeScope("companyfunctions", "rplandingapi")]
        [ProducesResponseType(typeof(Organization), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetOrganization(Guid? realPageId = null)
        {
            return await Task.Run<IActionResult>(() =>
            {
                if (realPageId.HasValue)
                {
                    Organization org = _manageOrganization.GetOrganization(realPageId.Value);
                    if (org == null)
                    {
                        return NotFound("Not found");
                    }

                    return Ok(org);
                }
                else
                {
                    IList<Organization> orgList = _manageOrganization.GetOrganizationList();
                    return Ok(orgList);
                }
            });
        }

        /// <summary>
        /// list of Organization By Enterprise User Id
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <param name="roleTypeFrom">Person Role Type name in the Relationship (Optional)</param>
        /// <param name="roleTypeTo">Organization Role Type name in the Relationship (Optional)</param>
        /// <param name="relationshipType">Parties Relationship type name (Optional)</param>
        /// <returns>A list of Organization(s) Details for a person</returns>
        [HttpGet("organization/person/{realPageId}")]
        [ProducesResponseType(typeof(ObjectListOutput<Organization, IErrorData>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ListOrganizationByEnterpriseUserId(Guid realPageId, [FromQuery] string roleTypeFrom = null, [FromQuery] string roleTypeTo = null, [FromQuery] string relationshipType = null)
        {
            return await Task.Run<IActionResult>(() =>
            {
                if (realPageId == Guid.Empty)
                {
                    return BadRequest("Invalid parameter: realPageId");
                }

                IList<Organization> organizationList = _manageUserLogin.ListOrganizationByEnterpriseUserId(realPageId, relationshipType);

                if (organizationList != null)
                {
                    foreach (var organization in organizationList)
                    {
                        var partyRelationship = _managePartyRelationship.GetPartyRelationship(realPageId, organization.RealPageId, roleTypeFrom, roleTypeTo, relationshipType);
                        if (partyRelationship != null)
                        {
                            organization.partyRelationship = partyRelationship;
                        }
                    }

                    ObjectListOutput<Organization, IErrorData> output = new ObjectListOutput<Organization, IErrorData>() { list = organizationList };
                    return Ok(output);
                }

                //When trying to get a list of Organization(s) for a Person that doesn't exists
                return NoContent();
            });
        }

        #endregion

        #region Organization Product Operations

        /// <summary>
        /// List organization product(s)
        /// </summary>
        /// <param name="realPageId">The unique identifier for the organization</param>
        /// <param name="mergePersonaAccess">Merge persona product access</param>
        /// <param name="allProducts">Return all product types</param>
        /// <returns></returns>
        [HttpGet("organization/{realPageId}/products")]
        [Authorize]
        [ProducesResponseType(typeof(ObjectListOutput<ProductUI, IErrorData>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetProductsByOrganization([FromRoute] Guid? realPageId, [FromQuery] bool? mergePersonaAccess, [FromQuery] bool? allProducts)
        {
            return await Task.Run<IActionResult>(() =>
            {
                ObjectListOutput<ProductUI, IErrorData> output = new ObjectListOutput<ProductUI, IErrorData>();
                Status<IErrorData> errorStatus = new Status<IErrorData>();

                if (realPageId == null || realPageId == Guid.Empty)
                {
                    realPageId = _userClaimsAccessor.OrganizationRealPageGuid;
                }

                Organization org = _manageOrganization.GetOrganization(realPageId.Value);

                if (org == null)
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "400";
                    errorStatus.ErrorMsg = "Organization not found!";
                    output.Status = errorStatus;
                    return BadRequest(output);
                }

                long personaId = 0;

                if (mergePersonaAccess.HasValue && mergePersonaAccess.Value)
                {
                    var persona = _managePersona.GetPersona(_userClaimsAccessor.PersonaId);

                    if (persona == null)
                    {
                        errorStatus.Success = false;
                        errorStatus.ErrorCode = "400";
                        errorStatus.ErrorMsg = "Persona not found!";
                        output.Status = errorStatus;
                        return BadRequest(output);
                    }

                    personaId = persona.PersonaId;
                }

                CheckPasswordExpirationResponse checkPasswordExpirationResponse = _manageCredential.CheckPasswordExpiration(_userClaimsAccessor.UserId, _userClaimsAccessor.UserRealPageGuid);
                if (checkPasswordExpirationResponse != null && !checkPasswordExpirationResponse.IsPasswordExpired)
                {
                    var cacheKey = $"getListProductsByOrganization_{org.RealPageId}";
                    _memoryCache.Remove(cacheKey);

                    IList<ProductUI> productList = _manageProduct.GetProducts(realPageId: org.RealPageId, personaId: personaId, allProducts: (allProducts.HasValue ? allProducts.Value : false), replaceProductCodeWithUDMIfExists: false);
                    if (allProducts.HasValue && allProducts.Value)
                    {
                        output.list = _manageProduct.AddProductSourceAndGreenBookCareFlagToProducts(org.RealPageId, org.PartyId, productList);
                    }
                    else
                    {
                        output.list = productList;
                    }
                }

                output.Status = errorStatus;

                return Ok(output);
            });
        }

        /// <summary>
        /// Add/Updates products to an organization
        /// </summary>
        /// <param name="realPageId">The unique identifier for the organization</param>
        /// <param name="enableDisableProducts">A list of BlueBook product names. i.e. </param>
        /// <returns></returns>
        [HttpPut("organization/{realPageId}/product")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> AddProductToOrganization([FromRoute] Guid realPageId, [FromBody] EnableDisableProducts enableDisableProducts)
        {
            return await Task.Run<IActionResult>(() =>
            {
                Status<IErrorData> errorStatus = new Status<IErrorData>();

                if (realPageId == Guid.Empty)
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "200.3";
                    errorStatus.ErrorMsg = "Invalid parameter: realPageId";
                    return BadRequest(errorStatus);
                }
                if (enableDisableProducts.AddProducts == null)
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "400";
                    errorStatus.ErrorMsg = "Products not found!";
                    return BadRequest(errorStatus);
                }

                Organization org = _manageOrganization.GetOrganization(realPageId);

                if (org == null)
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "400";
                    errorStatus.ErrorMsg = "Organization not found!";
                    return BadRequest(errorStatus);
                }
                List<int> addProductList = new List<int>();
                List<int> removeProductList = new List<int>();
                // verify the products, if any, exist and can be added to the customer
                List<string> invalidProductList = _manageOrganization.ParseProduct(enableDisableProducts.AddProducts, addProductList);
                if (enableDisableProducts.Removeproducts != null)
                {
                    _manageOrganization.ParseProduct(enableDisableProducts.Removeproducts, removeProductList);
                }
                if (invalidProductList.Count > 0)
                {
                    return BadRequest("An invalid product was given : " + String.Join(",", invalidProductList));
                }

                // add the given products to the new company
                if (addProductList.Count > 0)
                {
                    IList<ProductUI> productList = _manageProduct.GetProducts(realPageId: org.RealPageId, personaId: 0, allProducts: true, replaceProductCodeWithUDMIfExists: false);
                    var repositoryResponse = _manageOrganizationProduct.CheckSharedProductsEnabled(productList, addProductList, removeProductList);

                    if (!string.IsNullOrEmpty(repositoryResponse.ErrorMessage))
                    {
                        return Ok(repositoryResponse.ErrorMessage);
                    }
                    _manageOrganization.EnableProductOnOtherProductsActivation(addProductList);
                    repositoryResponse = _manageOrganizationProduct.InsertUpdateOrganizationProduct(org, addProductList);

                    if (!string.IsNullOrEmpty(repositoryResponse.ErrorMessage))
                    {
                        return BadRequest(repositoryResponse.ErrorMessage);
                    }
                }

                return StatusCode((int)HttpStatusCode.Created);
            });
        }

        /// <summary>
        /// Update Use primary properties for products
        /// </summary>
        [HttpPut("companysetup/party/{organizationPartyId}/product/{productId}/usePrimaryProperty/{usePrimaryProperty}")]
        [AuthorizeScope("companyfunctions", "rplandingapi")]
        [ProducesResponseType(typeof(RepositoryResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateUsePrimaryPropertyForOrganizationProduct(long organizationPartyId, int productId, bool usePrimaryProperty)
        {
            return await Task.Run<IActionResult>(() =>
            {
                if (organizationPartyId == 0)
                {
                    return BadRequest("organizationPartyId not supplied");
                }

                if (productId == 0)
                {
                    return BadRequest("productId not supplied");
                }

                var repositoryResponse = _manageOrganization.UpdateUsePrimaryPropertyForOrganizationProduct(organizationPartyId, productId, usePrimaryProperty);
                if (repositoryResponse.Id == 0)
                {
                    return BadRequest(repositoryResponse.ErrorMessage);
                }

                return Ok(repositoryResponse);
            });
        }

        /// <summary>
        /// Remove products from an organization
        /// </summary>
        /// <param name="realPageId">The unique identifier for the organization</param>
        /// <param name="enableDisableProducts"></param>
        /// <returns></returns>
        [HttpDelete("organization/{realPageId}/product")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> DeleteProductFromOrganization([FromRoute] Guid realPageId, [FromBody] EnableDisableProducts enableDisableProducts)
        {
            return await Task.Run<IActionResult>(() =>
            {
                Status<IErrorData> errorStatus = new Status<IErrorData>();

                if (realPageId == Guid.Empty)
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "200.3";
                    errorStatus.ErrorMsg = "Invalid parameter: realPageId";
                    return BadRequest(errorStatus);
                }
                if (enableDisableProducts.Removeproducts == null)
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "400";
                    errorStatus.ErrorMsg = "Products not found!";
                    return BadRequest(errorStatus);
                }

                Organization org = _manageOrganization.GetOrganization(realPageId);
                if (org == null)
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "400";
                    errorStatus.ErrorMsg = "Organization not found!";
                    return BadRequest(errorStatus);
                }

                List<int> unassignProductList = new List<int>();
                List<int> assignProductList = new List<int>();
                //Validate Products
                List<string> invalidProductList = _manageOrganization.ParseProduct(enableDisableProducts.Removeproducts, unassignProductList);
                if (enableDisableProducts.AddProducts != null)
                    _manageOrganization.ParseProduct(enableDisableProducts.AddProducts, assignProductList);
                if (invalidProductList.Count > 0)
                {
                    return BadRequest("An invalid product was given : " + String.Join(",", invalidProductList));
                }

                // Delete the given products to the company
                if (unassignProductList.Count > 0)
                {
                    IList<ProductUI> productList = _manageProduct.GetProducts(realPageId: org.RealPageId, personaId: 0, allProducts: true, replaceProductCodeWithUDMIfExists: false);
                    var repositoryResponse = _manageOrganizationProduct.CheckSharedProductsEnabled(productList, assignProductList, unassignProductList);

                    if (!string.IsNullOrEmpty(repositoryResponse.ErrorMessage))
                    {
                        return Ok(repositoryResponse.ErrorMessage);
                    }
                    repositoryResponse = _manageOrganizationProduct.DeleteProductsFromOrganization(unassignProductList, org);
                    if (!string.IsNullOrEmpty(repositoryResponse.ErrorMessage))
                    {
                        return BadRequest(repositoryResponse.ErrorMessage);
                    }
                }

                return NoContent();
            });
        }

        /// <summary>
        /// Used to get the Organization Identity ProviderType by realPageId
        /// </summary>
        /// <param name="realPageId">Organization unique identifier</param>
        /// <returns>Identity Provider Type object</returns>
        [HttpGet("organization/Providertype")]
        [ProducesResponseType(typeof(ObjectOutput<IIdentityProviderType, IErrorData>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetOrganizationIdentityProviderType([FromQuery] Guid? realPageId = null)
        {
            return await Task.Run<IActionResult>(() =>
            {
                ObjectOutput<IIdentityProviderType, IErrorData> output = new ObjectOutput<IIdentityProviderType, IErrorData>();
                Status<IErrorData> errorStatus = new Status<IErrorData>();

                if ((realPageId == Guid.Empty) || (realPageId == null))
                {
                    realPageId = _userClaimsAccessor.OrganizationRealPageGuid;
                }

                IList<IdentityProviderType> identityProviderTypeList = _manageOrganization.GetOrganizationIdentityProviderType(realPageId.Value);
                IIdentityProviderType idpt = (from a in identityProviderTypeList where a.IsLocal == (identityProviderTypeList.Count > 1 ? false : true) select a).FirstOrDefault();
                if (idpt != null)
                {
                    output.obj = idpt;
                    output.Status = errorStatus;
                    return Ok(output);
                }

                errorStatus.Success = false;
                errorStatus.ErrorCode = "Organization.GetOrganizationIdentityProviderType.2";
                errorStatus.ErrorMsg = "Get Organization Identity ProviderType: No data";
                output.Status = errorStatus;
                return Ok(output);
            });
        }

        #endregion

        #region Organization Type Methods

        /// <summary>
        /// List Organization Types
        /// </summary>
        /// <returns>Profile object</returns>
        [HttpGet("organizationtype")]
        [AuthorizeScope("companyfunctions", "rplandingapi")]
        [ProducesResponseType(typeof(ObjectListOutput<OrganizationType, IErrorData>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> OrganizationType()
        {
            return await Task.Run<IActionResult>(() =>
            {
                ObjectListOutput<OrganizationType, IErrorData> output = new ObjectListOutput<OrganizationType, IErrorData>();
                Status<IErrorData> errorStatus = new Status<IErrorData>();

                IList<OrganizationType> organizationTypeList = _manageOrganization.ListOrganizationType();

                if (organizationTypeList != null)
                {
                    output.Status = errorStatus;
                    output.list = organizationTypeList;
                    return Ok(output);
                }
                else
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "Organization.OrganizationType.1";
                    errorStatus.ErrorMsg = "List OrganizationType: No data";
                    output.Status = errorStatus;
                    return Ok(output);
                }
            });
        }

        #endregion

        #region Organization Domain Methods

        /// <summary>
        /// List Organization Domains
        /// </summary>
        /// <returns>Company domains</returns>
        [HttpGet("organizationdomain")]
        [AuthorizeScope("companyfunctions", "rplandingapi")]
        [ProducesResponseType(typeof(ObjectListOutput<OrganizationDomain, IErrorData>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetOrganizationDomain()
        {
            return await Task.Run<IActionResult>(() =>
            {
                ObjectListOutput<OrganizationDomain, IErrorData> output = new ObjectListOutput<OrganizationDomain, IErrorData>();
                Status<IErrorData> errorStatus = new Status<IErrorData>();
                _memoryCache.Remove("getListOrganizationDomain");
                IList<OrganizationDomain> organizationDomainList = _manageOrganization.ListOrganizationDomain();

                if (organizationDomainList != null)
                {
                    output.Status = errorStatus;
                    output.list = organizationDomainList;
                    return Ok(output);
                }
                else
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "Organization.OrganizationDomain.1";
                    errorStatus.ErrorMsg = "List OrganizationDomain: No data";
                    output.Status = errorStatus;
                    return Ok(output);
                }
            });
        }

        #endregion

        #region Company Setup Operations

        /// <summary>
        /// Get List of Organizations
        /// </summary>
        /// <param name="organizationName">OrganizationName</param>
        /// <param name="domain">Domain</param>
        /// <param name="blueId">BlueId</param>
        /// <param name="organizationId">organizationId</param>
        /// <param name="datafilter">datafilter</param>
        /// <returns>List of Organizations</returns>
        [HttpGet("CompanySetup")]
        [AuthorizeScope("companyfunctions", "rplandingapi")]
        [ProducesResponseType(typeof(ObjectListOutput<CompanySetup, IErrorData>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetCompanyList([FromQuery] string organizationName = null, [FromQuery] int? domain = null, [FromQuery] int? blueId = null, [FromQuery] int? organizationId = null,  RequestParameter datafilter = null)
        {
            return await Task.Run<IActionResult>(() =>
            {
                if (string.IsNullOrEmpty(organizationName) && domain == null && blueId == null && organizationId == null)
                {
                    return BadRequest("organizationName/Domain/BlueId not supplied ");
                }

                IDictionary<object, object> globals = new Dictionary<object, object>();
                ObjectListOutput<CompanySetup, IErrorData> output = new ObjectListOutput<CompanySetup, IErrorData>();
                Status<IErrorData> errorStatus = new Status<IErrorData>();
                
                datafilter ??= new RequestParameter
                {
                    FilterBy = new Dictionary<string, string>(),
                    SortBy = new Dictionary<string, string>(),
                    Pages = new PageRequest { StartRow = 0, ResultsPerPage = 100 }
                };

                globals.Add(BaseType.RequestParameter, datafilter);
                
                List<CompanySetup> companyList = _manageOrganization.GetCompanyList(organizationName, domain ?? 0, blueId, organizationId ?? 0, globals);

                int totalRecords = companyList.Count > 0 ? companyList[0].TotalRecords : 0;
                decimal resultsPerPage = ((datafilter.Pages.ResultsPerPage == 100) && (totalRecords > 0)) ? totalRecords : datafilter.Pages.ResultsPerPage;
                resultsPerPage = (resultsPerPage == 0) ? totalRecords : resultsPerPage;
                PagingSummary pagingSummary = new PagingSummary()
                {
                    TotalRecords = totalRecords,
                    TotalPages = (resultsPerPage == 0) ? 0 : (int)Math.Ceiling(totalRecords / resultsPerPage)
                };
                output = new ObjectListOutput<CompanySetup, IErrorData>() { list = companyList, Status = errorStatus };
                output.pagingSummary = pagingSummary;
                return Ok(output);
            });
        }

        /// <summary>
        /// Search Company By CustomerCompanyId
        /// </summary>
        [HttpGet("companysetup/companymaster/{customerCompanyId}")]
        [AuthorizeScope("companyfunctions", "rplandingapi")]
        [ProducesResponseType(typeof(CompanySetup), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetCompanyMasterByCustomerCompanyId(long customerCompanyId)
        {
            return await Task.Run<IActionResult>(() =>
            {
                if (customerCompanyId == 0)
                {
                    return BadRequest("CompanyMasterId not supplied");
                }

                var companyMaster = _manageOrganization.SearchCompanyDetailsByCustomerCompanyId(customerCompanyId);

                return Ok(companyMaster);
            });
        }

        /// <summary>
        /// Export Companies
        /// </summary>
        /// <param name="organizationName">OrganizationName</param>
        /// <param name="domain">Domain</param>
        /// <param name="blueId">BlueId</param>
        /// <param name="organizationId">organizationId</param>
        /// <param name="datafilter">Filter, Sort, Paginate</param>
        /// <param name="dataFormat">Return data in this format (default = CSV)</param>
        /// <returns>List of Companies object</returns>
        [HttpGet("CompanySetup/export")]
        [AuthorizeScope("companyfunctions", "rplandingapi")]
        [ProducesResponseType(typeof(ObjectOutput<string, IErrorData>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ListCompanyExport([FromQuery] string organizationName = null, [FromQuery] string domain = null, [FromQuery] int? blueId = null, [FromQuery] int? organizationId = null, [FromQuery] RequestParameter datafilter = null, [FromQuery] SaveFormat dataFormat = SaveFormat.CSV)
        {
            return await Task.Run<IActionResult>(() =>
            {
                byte[] plainBytes;
                IDictionary<object, object> globals = new Dictionary<object, object>();
                ObjectOutput<string, IErrorData> output = new ObjectOutput<string, IErrorData>();

                Status<IErrorData> errorStatus = new Status<IErrorData>();

                if (datafilter == null)
                {
                    datafilter = new RequestParameter();
                }

                globals.Add(BaseType.RequestParameter, datafilter);

                int domainId = 0;
                var organizationDomainList = _manageOrganization.ListOrganizationDomain();

                if (organizationDomainList != null && organizationDomainList.Count > 0 && !string.IsNullOrEmpty(domain))
                {
                    domainId = organizationDomainList.Find(o => o.Name.Equals(domain, StringComparison.OrdinalIgnoreCase)).OrganizationDomainId;
                }

                List<CompanySetup> companyList = _manageOrganization.GetCompanyList(organizationName, domainId, blueId, organizationId ?? 0, globals);

                if (companyList != null)
                {
                    errorStatus = DataExport.SetAsposeLicense();
                    if (errorStatus.Success)
                    {
                        List<ExportDataFileConfiguration> exportConfigurations = new List<ExportDataFileConfiguration>
                        {
                            new ExportDataFileConfiguration { Header = "UPFM Company Name", MappedField = "OrganizationName", PDFColumnWidth = "2.85", Preference = 1 },
                            new ExportDataFileConfiguration { Header = "Contracted Name", MappedField = "ContractedName", PDFColumnWidth = "2.85", Preference = 2 },
                            new ExportDataFileConfiguration { Header = "Domain", MappedField = "Domain", PDFColumnWidth = "0.85", Preference = 3 },
                            new ExportDataFileConfiguration { Header = "Address", MappedField = "Address", PDFColumnWidth = "3.25", Preference = 4 },
                            new ExportDataFileConfiguration { Header = "RPUP ID", MappedField = "BooksCustomerMasterId", PDFColumnWidth = "0.70", Preference = 5 },
                            new ExportDataFileConfiguration { Header = "Type", MappedField = "OrganizationType", PDFColumnWidth = "1.00", Preference = 6 },
                            new ExportDataFileConfiguration { Header = "Products", MappedField = "Products", PDFColumnWidth = "0.50", Preference = 7 },
                            new ExportDataFileConfiguration { Header = "UPFM Company ID", MappedField = "RealPageId", PDFColumnWidth = "3.25", Preference = 8 },
                            new ExportDataFileConfiguration { Header = "Status", MappedField = "Status", PDFColumnWidth = "2.25", Preference = 9 }
                        };

                        plainBytes = DataExport.ExportDataToFile<CompanySetup>(exportConfigurations.OrderBy(p => p.Preference).ToList(), companyList, dataFormat);
                        output = new ObjectOutput<string, IErrorData>()
                        {
                            obj = Convert.ToBase64String(plainBytes),
                            Status = errorStatus
                        };
                        return Ok(output);
                    }
                    else
                    {
                        output.Status = errorStatus;
                        return Ok(output);
                    }
                }
                else
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "Company.ListCompanyExport.1";
                    errorStatus.ErrorMsg = "List Company Export: No data";
                    output.Status = errorStatus;
                    return Ok(output);
                }
            });
        }

        #endregion

        #region Property Management Operations

        /// <summary>
        /// Get Properties for a Organization
        /// </summary>
        /// <param name="companyInstanceId">companyInstanceId</param>
        /// <param name="propertyName">PropertyName</param>
        /// <param name="domain">Domain</param>
        /// <param name="blueId">blueId</param>
        /// <param name="status"></param>
        /// <param name="datafilter">datafilter</param>
        /// <param name="userPersonaId">userPersonaId</param>
        /// <param name="editorPersonaId">editorPersonaId</param>
        /// <param name="isSelectedProperties">isSelectedProperties</param>
        /// <param name="selectedProperties"></param>
        /// <returns>List of Properties for a company </returns>
        [HttpPost("CompanySetup/CompanyPropertyList")]
        [AuthorizeScope("companyfunctions", "rplandingapi")]
        [ProducesResponseType(typeof(ObjectListOutput<CompanyPropertySetup, IErrorData>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPropertiesForCompany(Guid companyInstanceId, [FromBody] List<Guid> selectedProperties, [FromQuery] string domain = null, [FromQuery] string propertyName = null, [FromQuery] int? blueId = null, [FromQuery] int? status = null, [FromQuery] RequestParameter datafilter = null, [FromQuery] long userPersonaId = 0, [FromQuery] long editorPersonaId = 0, [FromQuery] bool? isSelectedProperties = null, [FromQuery] string operatorCode = null, [FromQuery] string operatorValue = null)
        {
            return await Task.Run<IActionResult>(() =>
            {
                if (companyInstanceId == Guid.Empty)
                {
                    return BadRequest("Company Instance Id not supplied");
                }

                IDictionary<object, object> globals = new Dictionary<object, object>();
                ObjectListOutput<CompanyPropertySetup, IErrorData> output = new ObjectListOutput<CompanyPropertySetup, IErrorData>();
                Status<IErrorData> errorStatus = new Status<IErrorData>();

                if (datafilter == null)
                {
                    datafilter = new RequestParameter();
                }

                globals.Add(BaseType.RequestParameter, datafilter);
                var cacheKey = $"getPropertyInstanceForCompany_{companyInstanceId}";

                if (!FeatureFlag.GetUserCompanyAssociationFeatureFlag())
                {
                    operatorCode = null;
                    operatorValue = null;
                }
                else
                {
                    if (string.IsNullOrEmpty(operatorCode) && string.IsNullOrEmpty(operatorValue))
                    {
                        cacheKey = $"getPropertyInstanceForCompanyByOperatorId_{companyInstanceId}_{operatorCode}_{operatorValue}";
                    }
                }

                RPObjectCache.RemoveFromCache(cacheKey);
                List<CompanyPropertySetup> companyPropertySetup = _manageOrganization.GetPropertiesForCompany(companyInstanceId, propertyName, domain, blueId, status, globals, editorPersonaId, userPersonaId, isSelectedProperties, selectedProperties, operatorCode, operatorValue);

                int totalRecords = 0;
                if (companyPropertySetup.Count > 0)
                {
                    totalRecords = companyPropertySetup[0]?.Property.Count > 0 ? companyPropertySetup[0].Property[0].TotalRecords : 0;
                }

                decimal resultsPerPage = ((datafilter.Pages.ResultsPerPage == 100) && (totalRecords > 0)) ? totalRecords : datafilter.Pages.ResultsPerPage;
                resultsPerPage = (resultsPerPage == 0) ? totalRecords : resultsPerPage;
                PagingSummary pagingSummary = new PagingSummary()
                {
                    TotalRecords = totalRecords,
                    TotalPages = (resultsPerPage == 0) ? 0 : (int)Math.Ceiling(totalRecords / resultsPerPage)
                };
                output = new ObjectListOutput<CompanyPropertySetup, IErrorData>() { list = companyPropertySetup, Status = errorStatus };
                output.pagingSummary = pagingSummary;
                return Ok(output);
            });
        }

        /// <summary>
        /// Update Company Properties (Batch operation)
        /// </summary>
        [HttpPost("CompanySetup/CompanyPropertiesUpdate")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Guid[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> UpdateCompanyProperties(CompanyPropertyBatch batchRecord)
        {
            if (batchRecord == null)
                return BadRequest("Batch record not supplied");

            if (!Guid.TryParse(batchRecord.CompanyInstanceSourceId, out Guid companyInstanceGuid) || companyInstanceGuid == Guid.Empty)
                return BadRequest("Company Instance Id not supplied");

            // Obtain properties for company (editorPersonaId = creator persona, userPersonaId = 0)
            var globals = new Dictionary<object, object> { { BaseType.RequestParameter, new RequestParameter() } };
            var companyPropertySetup = _manageOrganization.GetPropertiesForCompany(companyInstanceGuid, null, null, null, null, globals, batchRecord.CreateUserPersonaId, 0, null, null, null, null);

            var propertyList = companyPropertySetup?
                .Where(c => c?.Property != null)
                .SelectMany(c => c.Property)
                .ToList() ?? new List<PropertySetup>();

            if (propertyList.Count == 0)
            {
                await _manageOrganization.UpdateCompanyInstance(batchRecord.CompanyBatchJobId, (int)ProductBatchStatusType.Error, "No properties found for company").ConfigureAwait(false);
                return NoContent();
            }

            if (propertyList.Any(p => p.InstanceId == Guid.Empty))
            {
                await _manageOrganization.UpdateCompanyInstance(batchRecord.CompanyBatchJobId, (int)ProductBatchStatusType.Error, "Invalid parameter: propertyInstanceId").ConfigureAwait(false);
                return BadRequest("Invalid parameter: propertyInstanceId");
            }

            if (propertyList.Any(p => string.IsNullOrWhiteSpace(p.Name)))
            {
                await _manageOrganization.UpdateCompanyInstance(batchRecord.CompanyBatchJobId, (int)ProductBatchStatusType.Error, "Null parameter: propertyName").ConfigureAwait(false);
                return BadRequest("Null parameter: propertyName");
            }

            // Fetch current property instances once
            var propertyInstanceIds = propertyList.Select(p => p.InstanceId).Distinct().ToList();
            var propertyListToUpdate = _manageOrganization.GetPropertiesByInstanceId(propertyInstanceIds);
            var oldPropertyList = _manageOrganization.GetPropertiesByInstanceId(propertyInstanceIds); // retained for audit diff

            if (propertyListToUpdate == null || propertyListToUpdate.Count == 0)
                return NoContent();

            bool newActiveState = batchRecord.IsActive == 1;
            foreach (var p in propertyListToUpdate)
                p.IsActive = newActiveState;

            var repositoryResponse = await _manageOrganization.UpdatePropertyList(propertyListToUpdate, companyInstanceGuid).ConfigureAwait(false);

            if (repositoryResponse.Id == 0 || !string.IsNullOrEmpty(repositoryResponse.ErrorMessage))
            {
                await _manageOrganization.UpdateCompanyInstance(batchRecord.CompanyBatchJobId, (int)ProductBatchStatusType.Error, "Unable to update property list.").ConfigureAwait(false);
                return BadRequest(repositoryResponse.ErrorMessage);
            }

            // Fire-and-forget audit/settings updates with Parallel.ForEach
            _ = Task.Run(async () =>
            {
                try
                {
                    var options = new ParallelOptions { MaxDegreeOfParallelism = _maxDOPSetting };
                    Parallel.ForEach(propertyListToUpdate, options, property =>
                    {
                        var userClaim = _userClaimsAccessor.GetUserClaim();
                        var orgManager = new ManageOrganization(userClaim);
                        orgManager.UpdatePropertyInSettingsAndActivityLogs(property, companyInstanceGuid, oldPropertyList);
                    });
                }
                catch
                {
                    await _manageOrganization.UpdateCompanyInstance(batchRecord.CompanyBatchJobId, (int)ProductBatchStatusType.Error, "Update property list process failed.").ConfigureAwait(false);
                }
            });

            await _manageOrganization.UpdateCompanyInstance(batchRecord.CompanyBatchJobId, (int)ProductBatchStatusType.Success, string.Empty).ConfigureAwait(false);

            var instancesList = propertyListToUpdate.Select(p => p.InstanceId).ToArray();
            return Ok(instancesList);
        }

        /// <summary>
        /// Update Properties for a Organization
        /// </summary>
        /// <param name="propertyList">properties Object</param>
        /// <param name="companyInstanceId">companyInstanceId</param>
        /// <param name="isFromBulkPropertyUpdate"></param>
        [HttpPut("CompanySetup/CompanyPropertyList")]
        [AuthorizeScope("companyfunctions", "rplandingapi")]
        [ProducesResponseType(typeof(IEnumerable<Guid>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdatePropertyForOrganization([FromBody] List<UPFMPropertyInstance> propertyList, [FromQuery] Guid companyInstanceId, [FromQuery] bool isFromBulkPropertyUpdate = false)
        {
            if (companyInstanceId == Guid.Empty)
            {
                return BadRequest("Invalid parameter: companyInstanceId");
            }

            if (propertyList.Any(m => m.InstanceId == Guid.Empty))
            {
                return BadRequest("Invalid parameter: propertyInstanceId");
            }

            if (propertyList.Any(m => m.Name == string.Empty))
            {
                return BadRequest("Null parameter: propertyName");
            }

            RepositoryResponse repositoryResponse = new RepositoryResponse();

            if (!isFromBulkPropertyUpdate)
            {
                var options = new ParallelOptions() { MaxDegreeOfParallelism = _maxDOPSetting };
                Parallel.ForEach(propertyList, options, async (property, cancelToken) =>
                {
                    var userClaim = _userClaimsAccessor.GetUserClaim();
                    var manageOrganization = new ManageOrganization(userClaim);
                    repositoryResponse = (RepositoryResponse)await manageOrganization.ProcessPropertyList(property, companyInstanceId);
                });
                await Task.WhenAll();
            }
            else
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                var manageOrganization = new ManageOrganization(userClaim);
                List<UPFMPropertyInstance> oldPropertyList = manageOrganization.GetPropertiesByInstanceId(propertyList.Select(m => m.InstanceId).ToList());
                repositoryResponse = await manageOrganization.UpdatePropertyList(propertyList, companyInstanceId);
                if (repositoryResponse.Id > 0)
                {
                    _ = Task.Run(() =>
                    {
                        var options = new ParallelOptions() { MaxDegreeOfParallelism = _maxDOPSetting };
                        Parallel.ForEach(propertyList, options, property =>
                        {
                            var userClaimLocal = _userClaimsAccessor.GetUserClaim();
                            var orgManager = new ManageOrganization(userClaimLocal);
                            orgManager.UpdatePropertyInSettingsAndActivityLogs(property, companyInstanceId, oldPropertyList);
                        });
                    });
                }
            }

            if (repositoryResponse.Id == 0 || !string.IsNullOrEmpty(repositoryResponse.ErrorMessage))
            {
                return BadRequest(repositoryResponse.ErrorMessage);
            }
            var instancesList = propertyList.Select(m => m.InstanceId);
            return Ok(instancesList);
        }

        /// <summary>
        /// Update Properties for a Organization (Action from UDM)
        /// </summary>
        /// <param name="property">property Object</param>
        [HttpPut("CompanySetup/CompanyPropertyList/UDM")]
        [AuthorizeScope("companyfunctions", "rplandingapi")]
        [ProducesResponseType(typeof(Guid), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateUDMPropertyForOrganization([FromBody] UPFMPropertyInstance property)
        {
            return await Task.Run<IActionResult>(() =>
            {
                if (String.IsNullOrEmpty(property.CustomerPropertyId))
                {
                    return BadRequest("Invalid parameter: CustomerPropertyId");
                }

                if (property.InstanceId == Guid.Empty)
                {
                    return BadRequest("Invalid parameter: propertyInstanceId");
                }

                if (String.IsNullOrEmpty(property.Name))
                {
                    return BadRequest("Null parameter: propertyName");
                }

                var companyInstances = _manageBlueBook.GetCompanyInstancesByCustomerCompanyId(property.CustomerCompanyId);
                string companyInstanceId = string.Empty;
                foreach (var instance in companyInstances)
                {
                    var attributes = instance?.attributes;
                    if (attributes != null && attributes.Domain == property.Domain)
                    {
                        companyInstanceId = instance.attributes.companyInstanceSourceId;
                    }
                }

                var currentProperty = _manageOrganization.GetPropertyByInstanceId(property.InstanceId).FirstOrDefault();

                if (currentProperty != null && !string.IsNullOrEmpty(companyInstanceId))
                {
                    currentProperty.IsActive = property.IsActive;
                    currentProperty.Name = property.Name;
                    var repositoryResponse = _manageOrganization.UpdateProperty(property, new Guid(companyInstanceId));
                    if (repositoryResponse.Id == 0 || !string.IsNullOrEmpty(repositoryResponse.ErrorMessage))
                    {
                        return BadRequest(repositoryResponse.ErrorMessage);
                    }
                }

                return Ok(property.InstanceId);
            });
        }

        /// <summary>
        /// Export Properties
        /// </summary>
        [HttpGet("CompanySetup/CompanyPropertyList/export")]
        [AuthorizeScope("companyfunctions", "rplandingapi")]
        [ProducesResponseType(typeof(ObjectOutput<string, IErrorData>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ListPropertyExport([FromQuery] Guid companyInstanceId, [FromQuery] string propertyName = null, [FromQuery] string domain = null, [FromQuery] int? blueId = null, [FromQuery] int? status = null, [FromQuery] RequestParameter datafilter = null, [FromQuery] SaveFormat dataFormat = SaveFormat.CSV)
        {
            return await Task.Run<IActionResult>(() =>
            {
                byte[] plainBytes;
                IDictionary<object, object> globals = new Dictionary<object, object>();
                ObjectOutput<string, IErrorData> output = new ObjectOutput<string, IErrorData>();

                Status<IErrorData> errorStatus = new Status<IErrorData>();
                if (companyInstanceId == Guid.Empty)
                {
                    return BadRequest("Company Instance Id not supplied");
                }

                if (datafilter == null)
                {
                    datafilter = new RequestParameter();
                }

                globals.Add(BaseType.RequestParameter, datafilter);
                var cacheKey = $"getPropertyInstanceForCompany_{companyInstanceId}";
                RPObjectCache.RemoveFromCache(cacheKey);
                List<CompanyPropertySetup> propertyList = _manageOrganization.GetPropertiesForCompany(companyInstanceId, propertyName, domain, blueId, status, globals);

                if (propertyList != null && propertyList.Count > 0)
                {
                    errorStatus = DataExport.SetAsposeLicense();
                    if (errorStatus.Success)
                    {
                        List<ExportDataFileConfiguration> exportConfigurations = new List<ExportDataFileConfiguration>
                        {
                            new ExportDataFileConfiguration { Header = "UPFM Property Name", MappedField = "Name", PDFColumnWidth = "2.85", Preference = 1 },
                            new ExportDataFileConfiguration { Header = "Contracted Name", MappedField = "ContractedName", PDFColumnWidth = "2.85", Preference = 2 },
                            new ExportDataFileConfiguration { Header = "RPUP ID", MappedField = "CustomerPropertyId", PDFColumnWidth = "0.70", Preference = 3 },
                            new ExportDataFileConfiguration { Header = "Domain", MappedField = "Domain", PDFColumnWidth = "0.85", Preference = 4 },
                            new ExportDataFileConfiguration { Header = "Address", MappedField = "PropertyAddress", PDFColumnWidth = "3.25", Preference = 5 },
                            new ExportDataFileConfiguration { Header = "UPFM Property ID", MappedField = "InstanceId", PDFColumnWidth = "3.25", Preference = 6 },
                            new ExportDataFileConfiguration { Header = "Status", MappedField = "IsActive", PDFColumnWidth = "2.25", Preference = 7 }
                        };

                        plainBytes = DataExport.ExportDataToFile<PropertySetup>(exportConfigurations.OrderBy(p => p.Preference).ToList(), propertyList[0]?.Property, dataFormat);
                        output = new ObjectOutput<string, IErrorData>()
                        {
                            obj = Convert.ToBase64String(plainBytes),
                            Status = errorStatus
                        };
                        return Ok(output);
                    }
                    else
                    {
                        output.Status = errorStatus;
                        return Ok(output);
                    }
                }
                else
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "CompanySetup.ListPropertyExport.1";
                    errorStatus.ErrorMsg = "List Property Export: No data";
                    output.Status = errorStatus;
                    return Ok(output);
                }
            });
        }

        /// <summary>
        /// Add Properties for a Organization
        /// </summary>
        [HttpPost("CompanySetup/CompanyProperty")]
        [AuthorizeScope("companyfunctions", "rplandingapi")]
        [ProducesResponseType(typeof(RepositoryResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> AddPropertyForOrganization([FromBody] UPFMPropertyInstance property, [FromQuery] Guid companyInstanceID)
        {
            return await Task.Run<IActionResult>(() =>
            {
                if (companyInstanceID == Guid.Empty)
                {
                    return BadRequest("Invalid parameter: companyInstanceID");
                }

                if (property == null)
                {
                    return BadRequest("Null parameter: Property Object");
                }

                if (((string.IsNullOrEmpty(property.Name)) || (property.Name.Trim().Length == 0))
                    || ((string.IsNullOrEmpty(property.Domain)) || (property.Domain.Trim().Length == 0)))
                {
                    return BadRequest("PropertyName,Domain should not be empty");
                }

                if ((string.IsNullOrEmpty(property.CustomerPropertyId)) || (property.CustomerPropertyId.Trim().Length == 0))
                {
                    return BadRequest("CustomerPropertyId should not be empty or null");
                }

                if ((Convert.ToInt64(property.CustomerPropertyId.Trim()) == 0)
                    || (Convert.ToInt64(property.CustomerPropertyId.Trim()) <= 0))
                {
                    return BadRequest("CustomerPropertyId should be greater then zero");
                }

                var repositoryResponse = _manageOrganization.AddPropertyForOrganization(property, companyInstanceID);
                return Ok(repositoryResponse);
            });
        }

        /// <summary>
        /// Delete Properties for a Organization
        /// </summary>
        [HttpDelete("CompanySetup/CompanyProperty/propertyinstance/{propertyInstanceID}/{companyInstanceID}")]
        [AuthorizeScope("companyfunctions", "rplandingapi")]
        [ProducesResponseType(typeof(RepositoryResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> DeleteProperty(Guid propertyInstanceID, Guid companyInstanceID)
        {
            return await Task.Run<IActionResult>(() =>
            {
                if (propertyInstanceID == Guid.Empty)
                {
                    return BadRequest("Invalid parameter: propertyInstanceID");
                }

                var repositoryResponse = _manageOrganization.DeletePropertyForOrganization(propertyInstanceID, companyInstanceID);
                return Ok(repositoryResponse);
            });
        }

        /// <summary>
        /// Search Property By BlueId
        /// </summary>
        [HttpGet("CompanySetup/CompanyProperty/propertyinstance/{customerPropertyId}")]
        [AuthorizeScope("companyfunctions", "rplandingapi")]
        [ProducesResponseType(typeof(PropertyInstanceSearch), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SearchPropertyByBlueId(string customerPropertyId, [FromQuery] string booksCustomerMasterId)
        {
            return await Task.Run<IActionResult>(() =>
            {
                if ((string.IsNullOrEmpty(customerPropertyId)) || (customerPropertyId == "0"))
                {
                    return BadRequest("Invalid parameter: companyInstanceID");
                }

                if ((string.IsNullOrEmpty(booksCustomerMasterId)) || (booksCustomerMasterId == "0"))
                {
                    return BadRequest("Invalid parameter: companyInstanceID");
                }

                PropertyInstanceSearch _propertySearchList = _manageOrganization.SearchPropertyDetailsByCustomerPropertyId(customerPropertyId, booksCustomerMasterId);
                return Ok(_propertySearchList);
            });
        }

        /// <summary>
        /// Get Product Status Details
        /// </summary>
        [HttpGet("CompanySetup/Audit/product/{productInstanceId}/source/{source}/productStatus")]
        [AuthorizeScope("companyfunctions", "rplandingapi")]
        [ProducesResponseType(typeof(ProductPropertyDetails), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetProductStatusDetails(string productInstanceId, string source)
        {
            return await Task.Run<IActionResult>(() =>
            {
                if ((string.IsNullOrEmpty(productInstanceId)) || (productInstanceId == "0"))
                {
                    return BadRequest("Invalid parameter: companyInstanceID");
                }

                if (string.IsNullOrEmpty(source))
                {
                    return BadRequest("Invalid parameter: source");
                }

                ProductPropertyDetails _productPropertyDetails = _manageOrganization.GetSourceProductDetails(productInstanceId, source);
                return Ok(_productPropertyDetails);
            });
        }

        #endregion

        #region Audit Operations

        /// <summary>
        /// Audit the given product properties to UPFM properties
        /// </summary>
        [HttpGet("CompanySetup/{companyInstanceId}/product/{productId}/audit")]
        [AuthorizeScope("companyfunctions", "rplandingapi")]
        [ProducesResponseType(typeof(ObjectListOutput<PropertyAudit, IErrorData>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> AuditCompanyProductPropertiesToUPFM(Guid companyInstanceId, int productId)
        {
            return await Task.Run<IActionResult>(() =>
            {
                if (companyInstanceId == Guid.Empty)
                {
                    return BadRequest("Company Instance Id not supplied");
                }

                Status<IErrorData> errorStatus = new Status<IErrorData>();
                // Use User property from ControllerBase instead of ClaimsPrincipal.Current
                var currentClaimPrincipal = User;

                var userClaim = _userClaimsAccessor.GetUserClaim();

                if (currentClaimPrincipal.HasClaim("scope", "internalapi"))
                {
                    var adminCreatorRealPageId = _manageOrganization.GetOrganizationAdminUserRealPageId(companyInstanceId);
                    //recreate clams
                    if (adminCreatorRealPageId == Guid.Empty)
                    {
                        return BadRequest("Admin Creator RealPageId is empty.");
                    }
                    RecreateClaimsForClient(adminCreatorRealPageId, ref userClaim);
                    // need to alter the user being used to match the company or the product calls will not have the correct context
                }
                else if (_userClaimsAccessor.OrganizationRealPageGuid != EmployeeCompanyRealPageId)
                {
                    return BadRequest("Invalid company context");
                }

                var orgDetails = _manageOrganization.GetOrganization(companyInstanceId);

                if (orgDetails == null)
                {
                    return BadRequest("Invalid company");
                }

                userClaim.CustomerMasterId = orgDetails.BooksCustomerMasterId;
                userClaim.OrganizationMasterId = orgDetails.BooksMasterId;
                userClaim.OrganizationName = orgDetails.Name;
                userClaim.OrganizationPartyId = orgDetails.PartyId;
                userClaim.OrganizationRealPageGuid = orgDetails.RealPageId;

                var adminUserGuid = _manageOrganization.GetOrganizationAdminUserRealPageId(orgDetails.RealPageId);
                if (adminUserGuid == Guid.Empty)
                {
                    return BadRequest("Unable to locate company user");
                }

                userClaim.UserRealPageGuid = adminUserGuid;
                var auditResult = GetAuditProductProperties(companyInstanceId, productId, adminUserGuid, orgDetails.PartyId, userClaim);
                ObjectListOutput<PropertyAudit, IErrorData> output = new ObjectListOutput<PropertyAudit, IErrorData> { list = auditResult, Status = errorStatus, pagingSummary = new PagingSummary() { TotalRecords = auditResult.Count, TotalPages = 1 } };
                return Ok(output);
            });
        }

        /// <summary>
        /// Audit the given product properties to UPFM properties to export
        /// </summary>
        [HttpGet("CompanySetup/{companyInstanceId}/product/{productId}/audit/export")]
        [AuthorizeScope("companyfunctions", "rplandingapi")]
        [ProducesResponseType(typeof(ObjectOutput<string, IErrorData>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> AuditCompanyProductPropertiesToUPFMExport(Guid companyInstanceId, int productId, [FromQuery] SaveFormat dataFormat = SaveFormat.Csv)
        {
            return await Task.Run<IActionResult>(() =>
            {
                byte[] plainBytes;
                ObjectOutput<string, IErrorData> output = new ObjectOutput<string, IErrorData>();

                Status<IErrorData> errorStatus = new Status<IErrorData>();
                if (companyInstanceId == Guid.Empty)
                {
                    return BadRequest("Company Instance Id not supplied");
                }

                var userClaim = _userClaimsAccessor.GetUserClaim();

                // need to alter the user being used to match the company or the product calls will not have the correct context
                if (_userClaimsAccessor.OrganizationRealPageGuid != EmployeeCompanyRealPageId)
                {
                    return BadRequest("Invalid company context");
                }

                var orgDetails = _manageOrganization.GetOrganization(companyInstanceId);

                if (orgDetails == null)
                {
                    return BadRequest("Invalid company");
                }

                userClaim.CustomerMasterId = orgDetails.BooksCustomerMasterId;
                userClaim.OrganizationMasterId = orgDetails.BooksMasterId;
                userClaim.OrganizationName = orgDetails.Name;
                userClaim.OrganizationPartyId = orgDetails.PartyId;
                userClaim.OrganizationRealPageGuid = orgDetails.RealPageId;

                var adminUserGuid = _manageOrganization.GetOrganizationAdminUserRealPageId(orgDetails.RealPageId);
                if (adminUserGuid == Guid.Empty)
                {
                    return BadRequest("Unable to locate company user");
                }

                userClaim.UserRealPageGuid = adminUserGuid;
                List<PropertyAudit> propertyAudit = GetAuditProductProperties(companyInstanceId, productId, adminUserGuid, orgDetails.PartyId, userClaim);

                if (propertyAudit != null)
                {
                    errorStatus = DataExport.SetAsposeLicense();
                    if (errorStatus.Success)
                    {
                        List<ExportDataFileConfiguration> exportConfigurations = new List<ExportDataFileConfiguration>
                        {
                            new ExportDataFileConfiguration { Header = "Property", MappedField = "Name", PDFColumnWidth = "3.85", Preference = 1 },
                            new ExportDataFileConfiguration { Header = "Instance ID", MappedField = "ProductInstanceId", PDFColumnWidth = "2.85", Preference = 2 },
                            new ExportDataFileConfiguration { Header = "Platform Property name", MappedField = "UPFMName", PDFColumnWidth = "3.25", Preference = 3 },
                            new ExportDataFileConfiguration { Header = "Status", MappedField = "Status", PDFColumnWidth = "1.25", Preference = 4 }
                        };
                        plainBytes = DataExport.ExportDataToFile<PropertyAudit>(exportConfigurations.OrderBy(p => p.Preference).ToList(), propertyAudit, dataFormat);
                        output = new ObjectOutput<string, IErrorData>()
                        {
                            obj = Convert.ToBase64String(plainBytes),
                            Status = errorStatus
                        };
                        return Ok(output);
                    }
                    else
                    {
                        output.Status = errorStatus;
                        return Ok(output);
                    }
                }
                else
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "Company.ListPropertyAuditExport.1";
                    errorStatus.ErrorMsg = "List Property Audit Export: No data";
                    output.Status = errorStatus;
                    return Ok(output);
                }
            });
        }

        #endregion

        #region Cleanup and Deletion Operations

        /// <summary>
        /// Run the Organization clean up
        /// </summary>
        /// <returns>List of Organizations</returns>
        [HttpDelete("companysetup/cleanup")]
        [AuthorizeScope("companyfunctions", "rplandingapi")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> RunCompanyDatabaseDeleteAndUDMCleanUp()
        {
            return await Task.Run<IActionResult>(() =>
            {
                _manageOrganization.DeleteQueuedOrganizations();
                return Ok();
            });
        }

        /// <summary>
        /// Insert the Organization into the delete queuing table
        /// </summary>
        /// <param name="orgToDelete">The organization to add to the queue to be deleted</param>
        /// <param name="removeUPFMInstanceInUDM">Should the UPFM instance in UDM be removed</param>
        /// <returns></returns>
        [HttpPost("companysetup/cleanup")]
        [AuthorizeScope("companyfunctions", "rplandingapi")]
        [ProducesResponseType(typeof(OrganizationRemovalQueue), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> InsertOrganizationToDelete([FromBody] OrganizationDelete orgToDelete, [FromQuery] bool removeUPFMInstanceInUDM = true)
        {
            return await Task.Run<IActionResult>(() =>
            {
                var errorList = ValidateObject(orgToDelete).ToList();

                if (errorList.Count > 0)
                {
                    return BadRequest(errorList);
                }

                var org = _manageOrganization.GetOrganization(realPageId: (Guid)orgToDelete.OrganizationRealPageId);
                if (org == null)
                {
                    return BadRequest("Unknown Organization Id.");
                }

                OrganizationRemovalQueue organizationRemovalQueue = new OrganizationRemovalQueue()
                {
                    OrganizationRealPageId = org.RealPageId,
                    OrganizationCustomerMasterId = org.BooksCustomerMasterId,
                    OrganizationPartyId = org.PartyId,
                    OrganizationName = org.Name,
                    OrganizationDomain = org.OrganizationDomain.Name,
                    OrganizationRemovalQueueStatusId = 0,
                    OrganizationRemovalRetryCount = 0,
                    OrganizationRemoveUDMData = removeUPFMInstanceInUDM,
                    RequestedBy = orgToDelete.RequestedBy
                };

                var result = _manageOrganization.InsertOrganizationRemovalQueue(organizationRemovalQueue);

                return StatusCode((int)HttpStatusCode.Created, result);
            });
        }

        #endregion

        #region Private Helper Methods


        private void RecreateClaimsForClient(Guid _realpageUserId, ref DefaultUserClaim userClaim)
        {
            if (!string.IsNullOrEmpty(_realpageUserId.ToString()))
            {
                var person = _managePerson.GetPerson(_realpageUserId);
                if (person == null)
                {
                    throw new Exception($"Missing persona information for client_info user while Recreation of Claims For Client.  realPageId: {_realpageUserId}");
                }
                var userLogin = _manageUserLogin.GetUserLoginOnly(_realpageUserId);

                //Active Persona is linked to one organization
                var persona = _managePersona.GetActivePersonaWithoutRights(_realpageUserId); // this user can only be under 1 company to work correctly

                userClaim = new DefaultUserClaim
                {
                    UserId = (int)userLogin.UserId,
                    OrganizationPartyId = persona.Organization.PartyId,
                    LoginName = userLogin.LoginName,
                    OrganizationMasterId = (long)persona.Organization.BooksMasterId,
                    CustomerMasterId = (long)persona.Organization.BooksMasterId,
                    OrganizationName = persona.Organization.Name.ToString(),
                    FirstName = person.FirstName,
                    LastName = person.LastName,
                    PersonaId = persona.PersonaId,
                    OrganizationRealPageGuid = persona.Organization.RealPageId,
                    UserRealPageGuid = _realpageUserId,
                    CorrelationId = Guid.NewGuid(),
                    RealPageEmployee = persona.Organization.Name.ToUpper() == "REALPAGE EMPLOYEE"
                };
            }
        }

        private List<PropertyAudit> GetAuditProductProperties(Guid companyInstanceId, int productId, Guid adminUserGuid, long partyId, DefaultUserClaim userClaim)
        {
            var userLogin = _manageUserLogin.GetUserLogin(adminUserGuid, partyId);

            if (userLogin != null)
            {
                userClaim.LoginName = userLogin.LoginName;
                userClaim.UserId = Convert.ToInt32(userLogin.UserId);

                var userPersonas = _manageUserLogin.GetUserPersonaOrganization(userLogin.LoginName);
                if (userPersonas != null && userPersonas.Any(p => p.OrganizationPartyId == partyId))
                {
                    userClaim.PersonaId = userPersonas.First(p => p.OrganizationPartyId == partyId).PersonaId;
                }

                userClaim.FirstName = "XX";
                userClaim.LastName = "XX";
            }

            var manageOrganization = new ManageOrganization(userClaim);
            return manageOrganization.AuditCompanyProductPropertiesToUPFM(companyInstanceId, productId);
        }

        private IEnumerable<ValidationResult> ValidateObject(object source)
        {
            var result = new List<ValidationResult>();

            if (source == null)
            {
                result.Add(new ValidationResult("Request object is null."));
                return result;
            }

            // Dto Validation
            var valContext = new ValidationContext(source, null, null);
            Validator.TryValidateObject(source, valContext, result, true);
            return result;
        }

        #endregion
    }
}
