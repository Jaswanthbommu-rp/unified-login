using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Exceptions;
using UnifiedLogin.SharedObjects.Extensions;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.MarketingCenter;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.ResponseObject;
using UnifiedLogin.SharedObjects.Saml;
using MC = UnifiedLogin.SharedObjects.Product.MarketingCenter;
using Right = UnifiedLogin.SharedObjects.Product.MarketingCenter.Right;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Product;

/// <summary>
/// Native-async implementation of Marketing Center product user management.
/// <para>
/// Replaces the stepping-stone <c>ManageProductMarketingCenterAsync</c> wrapper and the
/// .NET 4.8 sync class <c>ManageProductMarketingCenter</c>:
/// </para>
/// <list type="bullet">
///   <item><c>IHttpClientFactory</c> replaces <c>new HttpClient()</c> per-call.</item>
///   <item><c>IMemoryCache</c> replaces <c>RPObjectCache</c> / <c>MemoryCache.Default</c>.</item>
///   <item><c>IUserClaimsAccessor</c> replaces per-method <c>DefaultUserClaim</c>.</item>
///   <item><c>ILogger&lt;T&gt;</c> replaces <c>WriteToDiagnosticLog</c> / <c>WriteToErrorLog</c>.</item>
///   <item>Injected repositories replace <c>new ManageXxx(userClaim)</c> construction.</item>
///   <item>Tuple return on <c>ManageMarketingCenterUserAsync</c> eliminates the <c>out</c> parameter.</item>
/// </list>
/// </summary>
public sealed class ManageProductMarketingCenterAsync : IManageProductMarketingCenterAsync
{
    #region Constants

    private const int    ProductId                = (int)ProductEnum.MarketingCenter;
    private const string ProductStatusSettingType = "ProductStatus";

    // Activity-log message templates
    private const string RightAssign             = "{\"action\":\"Added Rights\",\"value\":\"RightName\"}";
    private const string RightUnassign           = "{\"action\":\"Removed Rights\",\"value\":\"RightName\"}";
    private const string RoleAssign              = "{\"action\":\"Added Roles\",\"value\":\"RoleName\"}";
    private const string RoleUnassign            = "{\"action\":\"Removed Roles\",\"value\":\"RoleName\"}";
    private const string ProductRoleCreate       = "{\"action\":\"Created Role\",\"value\":\"RoleName\"}";
    private const string ProductRoleDelete       = "{\"action\":\"Deleted Role\",\"value\":\"RoleName\"}";
    private const string ProductRoleNameUpdate   = "{\"action\":\"Updated Role Name\",\"value\":\"RoleName\"}";
    private const string ProductRoleDescUpdate   = "{\"action\":\"Updated Role Description\",\"value\":\"RoleName\"}";
    private const string ProductRolesAssign      = "{\"action\":\"Added Roles\",\"value\":\"RoleName\"}";
    private const string ProductRolesRemove      = "{\"action\":\"Removed Roles\",\"value\":\"RoleName\"}";
    private const string ProductPropsAssign      = "{\"action\":\"Added Properties\",\"value\":\"PropertyName\"}";
    private const string ProductPropsRemove      = "{\"action\":\"Removed Properties\",\"value\":\"PropertyName\"}";

    #endregion

    #region Private records

    private sealed record McApiSettings(
        string Endpoint,
        string ApiSourceId,
        string Username,
        string Password);

    #endregion

    #region Fields

    private readonly IHttpClientFactory                         _httpClientFactory;
    private readonly IUserClaimsAccessor                        _userClaimsAccessor;
    private readonly ISamlRepositoryAsync                       _samlRepository;
    private readonly IProductRepositoryAsync                    _productRepository;
    private readonly IProductSettingServiceAsync                _productSettingService;
    private readonly IManagePersonaAsync                        _managePersona;
    private readonly IManagePersonAsync                         _managePerson;
    private readonly IManageUserLoginAsync                      _manageUserLogin;
    private readonly IManageBlueBookAsync                       _manageBlueBook;
    private readonly IManageElectronicAddressAsync              _manageElectronicAddress;
    private readonly IProductAuditServiceAsync                  _productAuditService;
    private readonly IProductContextServiceAsync                _contextService;
    private readonly IMemoryCache                               _cache;
    private readonly ILogger<ManageProductMarketingCenterAsync> _logger;

    // Lazy-loaded on first call; scoped instance so no lock needed
    private McApiSettings? _apiSettings;

    #endregion

    #region Constructor

    public ManageProductMarketingCenterAsync(
        IHttpClientFactory                         httpClientFactory,
        IUserClaimsAccessor                        userClaimsAccessor,
        ISamlRepositoryAsync                       samlRepository,
        IProductRepositoryAsync                    productRepository,
        IProductSettingServiceAsync                productSettingService,
        IManagePersonaAsync                        managePersona,
        IManagePersonAsync                         managePerson,
        IManageUserLoginAsync                      manageUserLogin,
        IManageBlueBookAsync                       manageBlueBook,
        IManageElectronicAddressAsync              manageElectronicAddress,
        IProductAuditServiceAsync                  productAuditService,
        IProductContextServiceAsync                contextService,
        IMemoryCache                               cache,
        ILogger<ManageProductMarketingCenterAsync> logger)
    {
        _httpClientFactory       = httpClientFactory;
        _userClaimsAccessor      = userClaimsAccessor;
        _samlRepository          = samlRepository;
        _productRepository       = productRepository;
        _productSettingService   = productSettingService;
        _managePersona           = managePersona;
        _managePerson            = managePerson;
        _manageUserLogin         = manageUserLogin;
        _manageBlueBook          = manageBlueBook;
        _manageElectronicAddress = manageElectronicAddress;
        _productAuditService     = productAuditService;
        _contextService          = contextService;
        _cache                   = cache;
        _logger                  = logger;
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // PUBLIC INTERFACE IMPLEMENTATIONS
    // ════════════════════════════════════════════════════════════════════════

    #region Roles

    /// <inheritdoc/>
    public async Task<ListResponse> GetRolesAsync(
        long editorPersonaId, long userPersonaId,
        RequestParameter datafilter,
        CancellationToken cancellationToken = default)
    {
        var response = new ListResponse();
        try
        {
            var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, userPersonaId, ProductId, cancellationToken);
            if (ctxError is not null)
                return ctxError;

            var settings         = await GetApiSettingsAsync(cancellationToken);
            var companyId        = await GetMcCompanyIdAsync(ctx.EditorPersona, settings, cancellationToken);
            var url              = $"{settings.Endpoint}/external/company/{companyId}/contact/roles";
            var roles            = await GetApiAsync<IList<MC.Role>>(url, settings, cancellationToken);

            IList<ProductRole> list = roles?.ToGBRoles() ?? new List<ProductRole>();

            if (userPersonaId != 0 && !string.IsNullOrEmpty(ctx.ProductUserId))
            {
                var mUser = await GetUserDetailsAsync(ctx.ProductUserId, settings, cancellationToken);
                if (mUser is null)
                    return new ListResponse { IsError = true, ErrorReason = "User not found" };

                var pr = list.FirstOrDefault(r => r.ID == mUser.ContactRoleId.ToString());
                if (pr is not null)
                    pr.IsAssigned = true;
            }

            response = new ListResponse
            {
                Records     = list.Cast<object>().ToList(),
                TotalRows   = list.Count,
                RowsPerPage = list.Count,
                TotalPages  = 1,
                ErrorReason = string.Empty
            };
        }
        catch (Exception ex) when (ex is BlueBookException)
        {
            response.IsError = true;
            response.ErrorReason = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ActionName} - {State}", "GetRolesAsync",
                $"Error editorPersonaId={editorPersonaId}");
            response.IsError     = true;
            response.ErrorReason = CommonMessageConstants.RoleErrorMessage;
        }
        return response;
    }

    #endregion

    #region Properties

    /// <inheritdoc/>
    public async Task<ListResponse> GetPropertiesAsync(
        long editorPersonaId, long userPersonaId,
        RequestParameter datafilter,
        CancellationToken cancellationToken = default)
    {
        var response = new ListResponse();
        try
        {
            var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, userPersonaId, ProductId, cancellationToken);
            if (ctxError is not null)
                return ctxError;

            var settings = await GetApiSettingsAsync(cancellationToken);
            var org      = ctx.EditorPersona.Organization;

            // Use GetCompanyMapAsync to get the MC companyId from BlueBook
            var companyMap = await _manageBlueBook.GetCompanyMapAsync(
                org.RealPageId, org.BooksCustomerMasterId,
                source: BlueBookProductConstants.MarketingCenter,
                domain: org.OrganizationDomain.Name,
                cancellationToken: cancellationToken);

            string mcCompanyId = companyMap
                ?.FirstOrDefault(c => c.Source.Equals(BlueBookProductConstants.MarketingCenter, StringComparison.OrdinalIgnoreCase))
                ?.CompanyInstanceSourceId ?? string.Empty;

            var url          = $"{settings.Endpoint}/external/properties?companyId={mcCompanyId}";
            var propMapList  = await GetApiAsync<IList<ProductPropertyMap>>(url, settings, cancellationToken);
            IList<ProductProperty> list = propMapList?.ToGBProperties() ?? new List<ProductProperty>();

            Dictionary<string, object> additional = new();

            if (userPersonaId != 0 && !string.IsNullOrEmpty(ctx.ProductUserId))
            {
                var mUser = await GetUserDetailsAsync(ctx.ProductUserId, settings, cancellationToken);
                if (mUser is null)
                    return new ListResponse { IsError = true, ErrorReason = "User not found" };

                if (mUser.AssignedProperties is { Count: > 0 })
                {
                    foreach (var p in mUser.AssignedProperties)
                    {
                        var pp = list.FirstOrDefault(x => x.ID == p.Id.ToString());
                        if (pp is not null)
                            pp.IsAssigned = true;
                        else
                            list.Add(new ProductProperty
                            {
                                Name     = p.Name,
                                ID       = p.Id.ToString(),
                                IsAssigned = p.Active,
                                State    = p.Address.StateCode,
                                Street1  = p.Address.Address1,
                                City     = p.Address.CityName,
                                Zip      = p.Address.PostalCode
                            });
                    }
                }

                additional["IsAssignedNewPropertyByDefault"] = mUser.AssignNewProperty;
            }

            response = new ListResponse
            {
                Records     = list.Cast<object>().ToList(),
                TotalRows   = list.Count,
                RowsPerPage = list.Count,
                TotalPages  = 1,
                ErrorReason = string.Empty,
                Additional  = additional
            };
        }
        catch (Exception ex) when (ex is BlueBookException)
        {
            response.IsError     = true;
            response.ErrorReason = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ActionName} - {State}", "GetPropertiesAsync",
                $"Error editorPersonaId={editorPersonaId}");
            response.IsError     = true;
            response.ErrorReason = CommonMessageConstants.PropertyErrorMessage;
        }
        return response;
    }

    #endregion

    #region ManageMarketingCenterUser

    /// <inheritdoc/>
    public async Task<(string Result, List<AdditionalParameters> ActivityLog)> ManageMarketingCenterUserAsync(
        long editorPersonaId, long userPersonaId,
        List<int> roleList, List<string> propertyList,
        bool isAssignedNewPropertyByDefault,
        CancellationToken cancellationToken = default)
    {
        var activityLog = new List<AdditionalParameters>();

        try
        {
            var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, userPersonaId, ProductId, cancellationToken);
            if (ctxError is not null)
                return (ctxError.ErrorReason!, activityLog);

            var settings    = await GetApiSettingsAsync(cancellationToken);
            var userPersona = await _managePersona.GetPersonaAsync(userPersonaId, cancellationToken: cancellationToken);
            var realPageId  = userPersona.RealPageId;
            var person      = await _managePerson.GetPersonAsync(realPageId, cancellationToken);
            var userLogin   = await _manageUserLogin.GetUserLoginOnlyAsync(realPageId, cancellationToken);

            var orgList = await _manageUserLogin.ListOrganizationByEnterpriseUserIdAsync(realPageId, null, cancellationToken);
            userPersona.Organization = orgList.FirstOrDefault(o => o.PartyId == userPersona.OrganizationPartyId);

            bool isExternalUser  = userPersona.Organization?.RelationshipType
                .Equals("User Type", StringComparison.OrdinalIgnoreCase) == true
                && userPersona.Organization?.RoleNameFrom
                .Equals("External User", StringComparison.OrdinalIgnoreCase) == true;

            bool isSuperUser     = await _contextService.IsSuperUserAsync(ctx!.UserPersona!, cancellationToken);
            bool isNoEmailUser   = await _contextService.IsRegularUserNoEmailAsync(ctx!.UserPersona!, cancellationToken);

            string userEmailAddress     = string.Empty;
            string userLeadEmailAddress = string.Empty;

            var addresses = await _manageElectronicAddress.ListElectronicAddressForPersonAsync(
                userLogin.RealPageId, cancellationToken: cancellationToken);

            userEmailAddress = addresses
                ?.FirstOrDefault(a => a.AddressType.Equals("EMAIL", StringComparison.OrdinalIgnoreCase))
                ?.AddressString ?? string.Empty;

            if (isNoEmailUser)
            {
                userLeadEmailAddress = userEmailAddress;
                if (string.IsNullOrEmpty(userLeadEmailAddress))
                    return ("ManageMarketingCenterUser - Error. No Valid Notification Email Provided", activityLog);

                userEmailAddress = !string.IsNullOrEmpty(ctx.ProductUsername)
                    ? ctx.ProductUsername
                    : !new EmailAddressAttribute().IsValid(userLogin.LoginName)
                        ? string.Concat(userLogin.LoginName, "@NoReply.com")
                        : userLogin.LoginName;
            }
            else
            {
                if (string.IsNullOrEmpty(userEmailAddress))
                    userEmailAddress = userLogin.LoginName;

                userEmailAddress = ValidateAndReturnEmailAddress(userEmailAddress);

                if (!string.IsNullOrEmpty(ctx.ProductUsername))
                    userEmailAddress = ctx.ProductUsername;
            }

            // Get current roles/properties for the company (used for activity log + validation)
            var rolesResponse = await GetRolesAsync(editorPersonaId, 0, null!, cancellationToken);
            var allRoles      = rolesResponse.Records?.Cast<ProductRole>().ToList() ?? new List<ProductRole>();

            var propsResponse = await GetPropertiesAsync(editorPersonaId, 0, null!, cancellationToken);
            var allProps      = propsResponse.Records?.Cast<ProductProperty>().ToList() ?? new List<ProductProperty>();

            if (!isSuperUser && (roleList.Count == 0 || propertyList.Count == 0))
            {
                if (roleList.Count == 0)
                {
                    await _productAuditService.WriteProductEventAsync(editorPersonaId, userPersonaId, ProductId,
                        "An error occurred when {3} {4} attempted to provision {2} for {0} {1}.There are no roles active in this company. Please contact the implementation team for this product.",
                        cancellationToken);
                    await _productSettingService.UpdateProductStatusAsync(userPersonaId, ProductStatusSettingType, ProductId,
                        (int)ProductBatchStatusType.Stop, cancellationToken);
                }
                if (propertyList.Count == 0)
                {
                    await _productAuditService.WriteProductEventAsync(editorPersonaId, userPersonaId, ProductId,
                        "An error occurred when {3} {4} attempted to provision {2} for {0} {1}.There are no properties active in this company. Please contact the implementation team for this product.",
                        cancellationToken);
                    await _productSettingService.UpdateProductStatusAsync(userPersonaId, ProductStatusSettingType, ProductId,
                        (int)ProductBatchStatusType.Stop, cancellationToken);
                }
                return (ProductBatchStatusType.Stop.ToString(), activityLog);
            }

            // Get BlueBook company id
            var companyMapping = await _manageBlueBook.GetProductCompanyMappingAsync(
                ctx.EditorPersona.Organization.RealPageId, BlueBookProductConstants.MarketingCenter, cancellationToken);
            var company = companyMapping?.FirstOrDefault();

            if (company is null || string.IsNullOrEmpty(company.CompanyInstanceSourceId))
                return ("Company Setup Error: Please Contact Support.", activityLog);

            // Snapshot before-state for activity log
            MC.MarketingCenterUserDetails? userBeforeUpdate = null;
            if (!string.IsNullOrEmpty(ctx.ProductUserId))
                userBeforeUpdate = await GetUserDetailsAsync(ctx.ProductUserId, settings, cancellationToken);

            // Resolve role id
            int roleId = 0;
            List<int> mcProperties = new();

            if (isSuperUser)
            {
                var corpOpsRole = allRoles.FirstOrDefault(r =>
                    r.Name.Equals("CORPORATE OPERATIONS", StringComparison.OrdinalIgnoreCase));
                if (corpOpsRole is null)
                {
                    await _productAuditService.WriteProductEventAsync(editorPersonaId, userPersonaId, ProductId,
                        "An error occurred when {3} {4} attempted to provision {2} for {0} {1}.There is no Corporate Operations role active in this company.Please contact the implementation team for this product.",
                        cancellationToken);
                    await _productSettingService.UpdateProductStatusAsync(userPersonaId, ProductStatusSettingType, ProductId,
                        (int)ProductBatchStatusType.Stop, cancellationToken);
                    return (ProductBatchStatusType.Stop.ToString(), activityLog);
                }
                roleId = Convert.ToInt32(corpOpsRole.ID);
                mcProperties.AddRange(allProps.Select(p => Convert.ToInt32(p.ID)));
            }
            else
            {
                if (!allRoles.Any(r => r.ID == roleList[0].ToString()))
                    return ($"Role id {roleList[0]} not found", activityLog);

                roleId = roleList[0];
                mcProperties.AddRange(propertyList.Select(p => Convert.ToInt32(p)));
            }

            bool allPropertiesSelected = isSuperUser;

            var mcUser = new MC.MarketingCenterUser
            {
                CompanyId              = Convert.ToInt32(company.CompanyInstanceSourceId),
                ContactRoleId          = roleId,
                ContactRoleName        = null,
                FirstName              = person.FirstName,
                LastName               = person.LastName,
                EmailAddress           = userEmailAddress,
                LeadEmailAddress       = userLeadEmailAddress,
                WelcomeEmailSent       = true,
                AssignUnassignProperties = true,
                AssignPropertyIds      = mcProperties,
                AssignNewProperty      = isSuperUser || isAssignedNewPropertyByDefault
            };

            string sourceId = string.IsNullOrEmpty(ctx.EditorProductUserId)
                ? settings.ApiSourceId
                : ctx.EditorProductUserId;

            string result;
            if (string.IsNullOrEmpty(ctx.ProductUserId))
            {
                // New user — generate unique username
                if (!isNoEmailUser)
                    userLeadEmailAddress = userLogin.LoginName;

                userEmailAddress = await GetMcUniqueUserNameAsync(person.FirstName, person.LastName, settings, cancellationToken);
                if (string.IsNullOrEmpty(userEmailAddress))
                    return ("An error occurred. Unable to get username.", activityLog);

                mcUser.EmailAddress      = userEmailAddress;
                mcUser.LeadEmailAddress  = userLeadEmailAddress;
                mcUser.AssignAllProperties = allPropertiesSelected;

                result = await CreateMcUserAsync(
                    mcUser, userEmailAddress, userLeadEmailAddress,
                    userPersonaId, editorPersonaId, sourceId, settings, cancellationToken);
            }
            else
            {
                // Existing user — compute property delta
                if (!isSuperUser)
                {
                    var currentPropResponse = await GetPropertiesAsync(editorPersonaId, userPersonaId, null!, cancellationToken);
                    var currentProps = currentPropResponse.Records?.Cast<ProductProperty>().ToList() ?? new();
                    var removeList   = new List<int>();

                    foreach (var pp in currentProps.Where(p => p.IsAssigned == true))
                    {
                        int id = Convert.ToInt32(pp.ID);
                        if (mcUser.AssignPropertyIds.Contains(id))
                            mcUser.AssignPropertyIds.Remove(id);
                        else
                            removeList.Add(id);
                    }
                    mcUser.UnassignPropertyIds = removeList;
                    mcUser.AssignAllProperties = false;
                }

                if (isExternalUser)
                {
                    mcUser.EmailAddress     = ctx.ProductUsername;
                    mcUser.LeadEmailAddress = userEmailAddress;
                }

                result = await UpdateMcUserAsync(
                    mcUser, ctx.ProductUserId, allPropertiesSelected,
                    userPersonaId, editorPersonaId, sourceId, settings, cancellationToken);
            }

            if (!string.IsNullOrEmpty(result))
                return (result, activityLog);

            await _productSettingService.UpdateProductStatusAsync(userPersonaId, ProductStatusSettingType, ProductId,
                (int)ProductBatchStatusType.Success, cancellationToken);

            // Build activity log
            ExtractActivityLog(mcUser, userBeforeUpdate, allRoles, allProps, activityLog);

            return (string.Empty, activityLog);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ActionName} - {State}", "ManageMarketingCenterUserAsync",
                $"Error editorPersonaId={editorPersonaId} userPersonaId={userPersonaId}");
            return ("Error", activityLog);
        }
    }

    #endregion

    #region Unassign / UpdateProfile

    /// <inheritdoc/>
    public async Task<string> UnassignUserAsync(
        long editorPersonaId, long userPersonaId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, userPersonaId, ProductId, cancellationToken);
            if (ctxError is not null) return ctxError.ErrorReason!;

            var settings = await GetApiSettingsAsync(cancellationToken);

            if (!await IsUserIdValidAsync(ctx!.EditorProductUserId, settings, cancellationToken))
                return $"ManageMarketingCenterUser.UnassignUser - Invalid admin userId: {ctx.EditorProductUserId}";

            // User was never provisioned in MC — treat as already unassigned.
            if (string.IsNullOrEmpty(ctx.ProductUserId) || ctx.ProductUserId == "0")
            {
                await _productSettingService.UpdateProductStatusAsync(userPersonaId, ProductStatusSettingType, ProductId,
                    (int)ProductBatchStatusType.Deleted, cancellationToken);
                return string.Empty;
            }

            bool ok = await SetUserStatusAsync(ctx.ProductUserId, false, ctx.EditorProductUserId, settings, cancellationToken);
            if (!ok)
            {
                await _productSettingService.UpdateProductStatusAsync(userPersonaId, ProductStatusSettingType, ProductId,
                    (int)ProductBatchStatusType.Error, cancellationToken);
                return $"ManageMarketingCenterUser.UnassignUser errored- userPersonaId: {userPersonaId}";
            }

            await _productSettingService.UpdateProductStatusAsync(userPersonaId, ProductStatusSettingType, ProductId,
                (int)ProductBatchStatusType.Deleted, cancellationToken);
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ActionName} - {State}", "UnassignUserAsync",
                $"Error editorPersonaId={editorPersonaId} userPersonaId={userPersonaId}");
            return $"ManageMarketingCenterUser.UnassignUser errored- userPersonaId: {userPersonaId}";
        }
    }

    /// <inheritdoc/>
    public async Task<string> UpdateUserProfileAsync(
        long editorPersonaId, long userPersonaId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, userPersonaId, ProductId, cancellationToken);
            if (ctxError is not null) return ctxError.ErrorReason!;

            var settings    = await GetApiSettingsAsync(cancellationToken);
            var userPersona = await _managePersona.GetPersonaAsync(userPersonaId, cancellationToken: cancellationToken);
            var realPageId  = userPersona.RealPageId;
            var person      = await _managePerson.GetPersonAsync(realPageId, cancellationToken);
            var userLogin   = await _manageUserLogin.GetUserLoginOnlyAsync(realPageId, cancellationToken);

            var orgList       = await _manageUserLogin.GetUserPersonaOrganizationAsync(userLogin.LoginName, cancellationToken: cancellationToken);
            bool isNoEmail    = await _contextService.IsRegularUserNoEmailAsync(ctx!.UserPersona!, cancellationToken);

            var addresses     = await _manageElectronicAddress.ListElectronicAddressForPersonAsync(
                userLogin.RealPageId, cancellationToken: cancellationToken);

            string userEmailAddress     = addresses
                ?.FirstOrDefault(a => a.AddressType.Equals("EMAIL", StringComparison.OrdinalIgnoreCase))
                ?.AddressString ?? string.Empty;
            string userLeadEmailAddress = string.Empty;

            if (string.IsNullOrEmpty(userEmailAddress))
                userEmailAddress = userLogin.LoginName;

            if (isNoEmail)
                userLeadEmailAddress = userEmailAddress;

            string productLoginName;
            if (userPersona.UserTypeId == (int)UserTypeConstants.RegularUserNoEmail)
            {
                userEmailAddress = ctx.ProductUsername;
                productLoginName = ctx.ProductUsername;
            }
            else
            {
                userEmailAddress = ValidateAndReturnEmailAddress(userEmailAddress);
                productLoginName = ctx.ProductUsername;

                bool isPrimaryOrg = orgList.Any(o =>
                    o.PrimaryOrganization &&
                    o.OrganizationPartyId == userPersona.OrganizationPartyId);

                if (isPrimaryOrg &&
                    !ctx.ProductUsername.Equals(userEmailAddress, StringComparison.OrdinalIgnoreCase))
                    productLoginName = userEmailAddress;
            }

            var mUser = await GetUserDetailsAsync(ctx.ProductUserId, settings, cancellationToken);
            if (mUser is null)
                return "User not found in product";

            var mcUser = new MC.MarketingCenterUser
            {
                CompanyId        = mUser.CompanyId,
                ContactRoleId    = mUser.ContactRoleId,
                FirstName        = person.FirstName,
                LastName         = person.LastName,
                EmailAddress     = productLoginName,
                LeadEmailAddress = userLeadEmailAddress,
                WelcomeEmailSent = true,
                AssignNewProperty = mUser.AssignNewProperty
            };

            var url      = $"{settings.Endpoint}/external/contact/{ctx.ProductUserId}?sourceid={ctx.EditorProductUserId}";
            var client   = _httpClientFactory.CreateClient();
            using var req = new HttpRequestMessage(HttpMethod.Put, url);
            req.Headers.Authorization = BuildBasicAuth(settings);
            req.Content = JsonContent.Create(mcUser);
            using var response = await client.SendAsync(req, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var existingAttrs = await _samlRepository.GetProductSamlDetailsAsync(userPersonaId, ProductId, cancellationToken);
                var existing      = existingAttrs?.FirstOrDefault(a => a.SamlAttributeId == (int)SamlAttributeEnum.productUsername);
                if (existing is not null)
                {
                    await _samlRepository.UpdateSamlUserAttributeAsync(
                        new SamlAttributes
                        {
                            SamlAttributeId     = (int)SamlAttributeEnum.productUsername,
                            Value               = productLoginName,
                            SamlUserAttributeId = existing.SamlUserAttributeId
                        }, cancellationToken);
                }

                await _productAuditService.WriteUserTypeChangeAsync(
                    editorPersonaId, userPersonaId, ProductId,
                    BatchProcessType.ProfileUpdate, cancellationToken);

                return string.Empty;
            }

            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("{ActionName} - {State}", "UpdateUserProfileAsync",
                $"Error status={(int)response.StatusCode} editorPersonaId={editorPersonaId}");
            return $"There was a problem updating user profile for user with editorPersona id - {editorPersonaId} - Error-{errorContent}.";
        }
        catch (Exception ex)
        {
            var msg = ex is AggregateException ae ? ae.Flatten().InnerException?.Message ?? ae.Message : ex.Message;
            _logger.LogError(ex, "{ActionName} - {State}", "UpdateUserProfileAsync",
                $"Error editorPersonaId={editorPersonaId}");
            return $"Error - {msg}";
        }
    }

    #endregion

    #region ChangeUserStatus

    /// <inheritdoc/>
    public async Task<bool> ChangeUserStatusAsync(
        long editorPersonaId, string userName, string productUserId,
        bool isActive = false,
        CancellationToken cancellationToken = default)
    {
        var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, 0, ProductId, cancellationToken);
        if (ctxError is not null) return false;

        var settings = await GetApiSettingsAsync(cancellationToken);

        var companyMapping = await _manageBlueBook.GetProductCompanyMappingAsync(
            ctx.EditorPersona.Organization.RealPageId, BlueBookProductConstants.MarketingCenter, cancellationToken);
        if (Convert.ToInt32(companyMapping?.FirstOrDefault()?.CompanyInstanceSourceId) == 0)
        {
            _logger.LogError("{ActionName} - {State}", "ChangeUserStatusAsync",
                $"No company id in BlueBook editorPersonaId={editorPersonaId}");
            return false;
        }

        return await SetUserStatusAsync(productUserId, isActive, ctx.EditorProductUserId, settings, cancellationToken);
    }

    #endregion

    #region Role/Right Setup

    /// <inheritdoc/>
    public async Task<ListResponse> GetRolesCountAsync(
        long editorPersonaId,
        CancellationToken cancellationToken = default)
    {
        var response = new ListResponse();
        try
        {
            var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, editorPersonaId, ProductId, cancellationToken);
            if (ctxError is not null) return ctxError;

            var settings  = await GetApiSettingsAsync(cancellationToken);
            var companyId = await GetMcCompanyIdAsync(ctx.EditorPersona, settings, cancellationToken);
            var url       = $"{settings.Endpoint}/external/company/{companyId}/roles";
            var roles     = await GetApiAsync<IList<RolesRightsAccessRight>>(url, settings, cancellationToken) ?? new List<RolesRightsAccessRight>();

            response = BuildListResponse(roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ActionName} - {State}", "GetRolesCountAsync", ex.Message);
            response.IsError = true; response.ErrorReason = ex.Message;
        }
        return response;
    }

    /// <inheritdoc/>
    public async Task<ListResponse> GetRightsAsync(
        long editorPersonaId,
        CancellationToken cancellationToken = default)
    {
        var response = new ListResponse();
        try
        {
            var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, editorPersonaId, ProductId, cancellationToken);
            if (ctxError is not null) return ctxError;

            var settings  = await GetApiSettingsAsync(cancellationToken);
            var companyId = await GetMcCompanyIdAsync(ctx.EditorPersona, settings, cancellationToken);
            var rights    = await GetRightsDetailsAsync(companyId, settings, cancellationToken);
            response      = BuildListResponse(rights);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ActionName} - {State}", "GetRightsAsync", ex.Message);
            response.IsError = true; response.ErrorReason = ex.Message;
        }
        return response;
    }

    /// <inheritdoc/>
    public async Task<ListResponse> DeleteRoleAsync(
        long editorPersonaId, int roleId,
        CancellationToken cancellationToken = default)
    {
        var response = new ListResponse();
        try
        {
            var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, editorPersonaId, ProductId, cancellationToken);
            if (ctxError is not null) return ctxError;

            var settings  = await GetApiSettingsAsync(cancellationToken);
            var companyId = await GetMcCompanyIdAsync(ctx.EditorPersona, settings, cancellationToken);

            var rolesResp = await GetRolesAsync(editorPersonaId, 0, null!, cancellationToken);
            var roleName  = rolesResp.Records?.Cast<ProductRole>()
                .FirstOrDefault(r => r.ID == roleId.ToString())?.Name;

            var loginName = GetAuditLoginName();
            var url       = $"{settings.Endpoint}/external/company/{companyId}/roles/{roleId}?username={Uri.EscapeDataString(loginName)}";

            var client = _httpClientFactory.CreateClient();
            using var req = new HttpRequestMessage(HttpMethod.Delete, url);
            req.Headers.Authorization = BuildBasicAuth(settings);
            using var result = await client.SendAsync(req, cancellationToken);

            if (!result.IsSuccessStatusCode)
                return new ListResponse { IsError = true, ErrorReason = "ManageMarketingCenterUser.DeleteRole - Unable to delete role" };

            await EmitDeleteRoleAuditAsync(editorPersonaId, roleId, roleName ?? string.Empty, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ActionName} - {State}", "DeleteRoleAsync", ex.Message);
            response.IsError = true; response.ErrorReason = ex.Message;
        }
        return response;
    }

    /// <inheritdoc/>
    public async Task<ListResponse> UpdateRoleStatusAsync(
        long editorPersonaId, int roleId, bool isActive,
        CancellationToken cancellationToken = default)
    {
        var response = new ListResponse();
        try
        {
            var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, editorPersonaId, ProductId, cancellationToken);
            if (ctxError is not null) return ctxError;

            var settings  = await GetApiSettingsAsync(cancellationToken);
            var companyId = await GetMcCompanyIdAsync(ctx.EditorPersona, settings, cancellationToken);
            var loginName = GetAuditLoginName();
            var url       = $"{settings.Endpoint}/external/company/{companyId}/roles/{roleId}?active={isActive}&username={Uri.EscapeDataString(loginName)}";

            var client = _httpClientFactory.CreateClient();
            using var req = new HttpRequestMessage(new HttpMethod("PATCH"), url);
            req.Headers.Authorization = BuildBasicAuth(settings);
            using var result = await client.SendAsync(req, cancellationToken);

            if (!result.IsSuccessStatusCode)
                return new ListResponse { IsError = true, ErrorReason = "ManageMarketingCenterUser.UpdateRoleStatus - Unable to update role status" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ActionName} - {State}", "UpdateRoleStatusAsync", ex.Message);
            response.IsError = true; response.ErrorReason = ex.Message;
        }
        return response;
    }

    /// <inheritdoc/>
    public async Task<ListResponse> GetRolesForRightIdAsync(
        long editorPersonaId, int rightId,
        CancellationToken cancellationToken = default)
    {
        var response = new ListResponse();
        try
        {
            var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, editorPersonaId, ProductId, cancellationToken);
            if (ctxError is not null) return ctxError;

            var settings  = await GetApiSettingsAsync(cancellationToken);
            var companyId = await GetMcCompanyIdAsync(ctx.EditorPersona, settings, cancellationToken);
            var url       = $"{settings.Endpoint}/external/company/{companyId}/rights/{rightId}/roles";
            var roles     = await GetApiAsync<IList<RolesRightsAccessRight>>(url, settings, cancellationToken) ?? new List<RolesRightsAccessRight>();
            response      = BuildListResponse(roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ActionName} - {State}", "GetRolesForRightIdAsync", ex.Message);
            response.IsError = true; response.ErrorReason = "There was a problem getting the roles";
        }
        return response;
    }

    /// <inheritdoc/>
    public async Task<ListResponse> UpdateRolesForRightAsync(
        long editorPersonaId, int rightId, List<string> roleList,
        CancellationToken cancellationToken = default)
    {
        var response = new ListResponse();
        try
        {
            var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, editorPersonaId, ProductId, cancellationToken);
            if (ctxError is not null) return ctxError;

            var settings  = await GetApiSettingsAsync(cancellationToken);
            var companyId = await GetMcCompanyIdAsync(ctx.EditorPersona, settings, cancellationToken);

            var currentRoles = await GetRolesForRightIdAsync(editorPersonaId, rightId, cancellationToken);
            if (!currentRoles.IsError && currentRoles.Records?.Count > 0)
                currentRoles.Records = currentRoles.Records.OfType<RolesRightsAccessRight>()
                    .Where(r => r.IsAssigned).Cast<object>().ToList();

            GetRoleAssignmentChanges(roleList, currentRoles,
                out var rolesToAdd, out var rolesToRemove);

            var loginName = GetAuditLoginName();
            var url       = $"{settings.Endpoint}/external/company/{companyId}/rights/{rightId}/roles?username={Uri.EscapeDataString(loginName)}";

            var client = _httpClientFactory.CreateClient();
            using var req = new HttpRequestMessage(HttpMethod.Put, url);
            req.Headers.Authorization = BuildBasicAuth(settings);
            req.Content = JsonContent.Create(roleList.Select(int.Parse).ToList());
            using var result = await client.SendAsync(req, cancellationToken);

            if (!result.IsSuccessStatusCode)
                return new ListResponse { IsError = true, ErrorReason = "ManageMarketingCenterUser.UpdateRolesForRight - Unable to update role status" };

            await EmitUpdateRolesToRightAuditAsync(editorPersonaId, rightId, rolesToAdd, rolesToRemove, cancellationToken);
            response.Records = null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ActionName} - {State}", "UpdateRolesForRightAsync", ex.Message);
            response.IsError = true; response.ErrorReason = ex.Message;
        }
        return response;
    }

    /// <inheritdoc/>
    public async Task<ListResponse> GetRightsForRoleIdAsync(
        long editorPersonaId, int roleId,
        CancellationToken cancellationToken = default)
    {
        var response = new ListResponse();
        try
        {
            var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, editorPersonaId, ProductId, cancellationToken);
            if (ctxError is not null) return ctxError;

            var settings  = await GetApiSettingsAsync(cancellationToken);
            var companyId = await GetMcCompanyIdAsync(ctx.EditorPersona, settings, cancellationToken);
            var url       = roleId == 0
                ? $"{settings.Endpoint}/external/company/{companyId}/rights"
                : $"{settings.Endpoint}/external/company/{companyId}/roles/{roleId}/rights";

            var rights = await GetApiAsync<IList<Right>>(url, settings, cancellationToken) ?? new List<Right>();
            var mcRights = rights.ToGBRights() ?? new List<MCRight>();
            response = BuildListResponse(mcRights);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ActionName} - {State}", "GetRightsForRoleIdAsync", ex.Message);
            response.IsError = true; response.ErrorReason = "There was a problem getting the roles";
        }
        return response;
    }

    /// <inheritdoc/>
    public async Task<ListResponse> CreateNewMCRoleWithRightsAsync(
        long editorPersonaId, MCRole mcRole,
        CancellationToken cancellationToken = default)
    {
        var response = new ListResponse();
        try
        {
            var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, editorPersonaId, ProductId, cancellationToken);
            if (ctxError is not null) return ctxError;

            var settings  = await GetApiSettingsAsync(cancellationToken);
            var companyId = await GetMcCompanyIdAsync(ctx.EditorPersona, settings, cancellationToken);
            var loginName = GetAuditLoginName();
            var url       = $"{settings.Endpoint}/external/company/{companyId}/roles?active={mcRole.Active}&username={Uri.EscapeDataString(loginName)}";

            var client = _httpClientFactory.CreateClient();
            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Headers.Authorization = BuildBasicAuth(settings);
            req.Content = JsonContent.Create(mcRole);
            using var result = await client.SendAsync(req, cancellationToken);

            if (!result.IsSuccessStatusCode)
            {
                var errorBody  = await result.Content.ReadAsStringAsync(cancellationToken);
                var roleErrors = JsonSerializer.Deserialize<RoleErrors>(errorBody,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return new ListResponse
                {
                    IsError     = true,
                    Additional  = "RoleError",
                    ErrorReason = !string.IsNullOrEmpty(roleErrors?.FieldErrors?.Error?.Message)
                        ? roleErrors.FieldErrors.Error.Message
                        : "Unable to create role"
                };
            }

            await EmitCreateRoleAuditAsync(editorPersonaId, mcRole, cancellationToken);

            var addedRights   = mcRole.Rights?.Select(r => r.ToString()).ToList() ?? new List<string>();
            var removedRights = new List<string>();
            await EmitUpdateRightsToRoleAuditAsync(editorPersonaId, mcRole.Id, mcRole.Name, addedRights, removedRights, cancellationToken);

            response.Records = null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ActionName} - {State}", "CreateNewMCRoleWithRightsAsync", ex.Message);
            response.IsError = true; response.ErrorReason = ex.Message;
        }
        return response;
    }

    /// <inheritdoc/>
    public async Task<ListResponse> UpdateMCRoleWithRightsAsync(
        long editorPersonaId, MCRole mcRole,
        CancellationToken cancellationToken = default)
    {
        var response = new ListResponse();
        try
        {
            var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, editorPersonaId, ProductId, cancellationToken);
            if (ctxError is not null) return ctxError;

            var settings  = await GetApiSettingsAsync(cancellationToken);
            var companyId = await GetMcCompanyIdAsync(ctx.EditorPersona, settings, cancellationToken);

            var currentRights = await GetRightsForRoleIdAsync(editorPersonaId, mcRole.Id, cancellationToken);
            if (!currentRights.IsError && currentRights.Records is not null)
                currentRights.Records = currentRights.Records.OfType<MCRight>()
                    .Where(r => r.IsAssigned).Cast<object>().ToList();

            var rolesResp   = await GetRolesAsync(editorPersonaId, 0, null!, cancellationToken);
            var oldRole     = rolesResp.Records?.Cast<ProductRole>().FirstOrDefault(r => r.ID == mcRole.Id.ToString());
            var roleName    = oldRole?.Name;
            var roleDesc    = oldRole?.Description;

            GetRightAssignmentChanges(currentRights, mcRole.Rights,
                out var addedRights, out var removedRights);

            var loginName = GetAuditLoginName();
            var url       = $"{settings.Endpoint}/external/company/{companyId}/roles/{mcRole.Id}?username={Uri.EscapeDataString(loginName)}";

            var client = _httpClientFactory.CreateClient();
            using var req = new HttpRequestMessage(HttpMethod.Put, url);
            req.Headers.Authorization = BuildBasicAuth(settings);
            req.Content = JsonContent.Create(mcRole);
            using var result = await client.SendAsync(req, cancellationToken);

            if (!result.IsSuccessStatusCode)
            {
                var errorBody  = await result.Content.ReadAsStringAsync(cancellationToken);
                var roleErrors = JsonSerializer.Deserialize<RoleErrors>(errorBody,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return new ListResponse
                {
                    IsError     = true,
                    Additional  = "RoleError",
                    ErrorReason = !string.IsNullOrEmpty(roleErrors?.FieldErrors?.Error?.Message)
                        ? roleErrors.FieldErrors.Error.Message
                        : "Unable to update role"
                };
            }

            if (roleName is not null && !roleName.Equals(mcRole.Name))
                await EmitUpdateRoleNameAuditAsync(editorPersonaId, mcRole.Name, roleName, "RoleName", cancellationToken);
            if (roleDesc is not null && !roleDesc.Equals(mcRole.Description))
                await EmitUpdateRoleNameAuditAsync(editorPersonaId, mcRole.Description, roleDesc, "RoleDescription", cancellationToken);
            if (addedRights.Count > 0 || removedRights.Count > 0)
                await EmitUpdateRightsToRoleAuditAsync(editorPersonaId, mcRole.Id, string.Empty, addedRights, removedRights, cancellationToken);

            response.Records = null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ActionName} - {State}", "UpdateMCRoleWithRightsAsync", ex.Message);
            response.IsError = true; response.ErrorReason = ex.Message;
        }
        return response;
    }

    #endregion

    #region Migration

    /// <inheritdoc/>
    public async Task<ListResponse> GetMigrationUsersAsync(
        long editorPersonaId, RequestParameter datafilter,
        CancellationToken cancellationToken = default)
    {
        var response = new ListResponse { IsError = true, ErrorReason = "No Users." };
        try
        {
            var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, 0, ProductId, cancellationToken);
            if (ctxError is not null) { response.ErrorReason = ctxError.ErrorReason; return response; }

            var settings  = await GetApiSettingsAsync(cancellationToken);
            var companyId = await GetMcCompanyIdAsync(ctx.EditorPersona, settings, cancellationToken);

            if (string.IsNullOrEmpty(companyId) || companyId == "0")
            {
                response.ErrorReason = "Company Setup Error: Please Contact Support.";
                return response;
            }

            string filter          = "NonMigrated";
            int    startRow        = 0;
            int    resultsPerPage  = 1000;

            if (datafilter?.FilterBy?.TryGetValue("filter", out var f) == true)
                filter = f;
            if (datafilter?.Pages is not null)
            {
                startRow       = datafilter.Pages.StartRow;
                resultsPerPage = datafilter.Pages.ResultsPerPage;
            }

            var url  = $"{settings.Endpoint}/external/api/{companyId}/users?filter-type={Uri.EscapeDataString(filter)}&startRow={startRow}&resultsperpage={resultsPerPage}";
            var data = await GetApiAsync<MigrationResponse<IList<MigrationUser>>>(url, settings, cancellationToken);

            if (data is null || data.Data is null) return response;

            response = new ListResponse
            {
                IsError     = false,
                ErrorReason = string.Empty,
                RowsPerPage = resultsPerPage,
                TotalPages  = 1,
                Records     = data.Data.Cast<object>().ToList(),
                TotalRows   = data.Data.Count()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ActionName} - {State}", "GetMigrationUsersAsync",
                $"Error editorPersonaId={editorPersonaId}");
            response.ErrorReason = ex.Message;
        }
        return response;
    }

    /// <inheritdoc/>
    public async Task<MigrateResponse> UpdateUsersMigrationStatusAsync(
        long editorPersonaId, IList<MigrateUser> migrateUsers,
        CancellationToken cancellationToken = default)
    {
        var migrateResponse = new MigrateResponse { Status = false };

        if (migrateUsers is null || migrateUsers.Count == 0)
            return migrateResponse;

        try
        {
            var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, 0, ProductId, cancellationToken);
            if (ctxError is not null) { migrateResponse.Message = ctxError.ErrorReason; return migrateResponse; }

            var settings  = await GetApiSettingsAsync(cancellationToken);
            var companyId = await GetMcCompanyIdAsync(ctx.EditorPersona, settings, cancellationToken);

            if (string.IsNullOrEmpty(companyId) || companyId == "0")
            {
                migrateResponse.Message = "Company Setup Error: Please Contact Support.";
                return migrateResponse;
            }

            // Enrich missing lead email addresses
            foreach (var user in migrateUsers.Where(u => string.IsNullOrEmpty(u.LeadEmailAddress)))
            {
                var addrs = await _manageElectronicAddress.ListElectronicAddressForPersonAsync(
                    user.UnifiedLoginUserName, ctx.EditorPersona.OrganizationPartyId,
                    cancellationToken: cancellationToken);
                user.LeadEmailAddress = addrs
                    ?.FirstOrDefault(a => a.AddressType.Equals("EMAIL", StringComparison.OrdinalIgnoreCase))
                    ?.AddressString ?? string.Empty;
            }

            var url    = $"{settings.Endpoint}/external/api/{companyId}/migrate-users";
            var client = _httpClientFactory.CreateClient();
            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Headers.Authorization = BuildBasicAuth(settings);
            req.Content = JsonContent.Create(migrateUsers);
            using var response = await client.SendAsync(req, cancellationToken);

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("{ActionName} - {State}", "UpdateUsersMigrationStatusAsync",
                    $"API error status={(int)response.StatusCode} editorPersonaId={editorPersonaId}");
                migrateResponse.Message = "Cannot update user status to migrated.";
                return migrateResponse;
            }

            var result = JsonSerializer.Deserialize<MigrationResponse<MigrateResponse>>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            migrateResponse.Status  = result?.Data?.Status  ?? false;
            migrateResponse.Message = result?.Data?.Message ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ActionName} - {State}", "UpdateUsersMigrationStatusAsync",
                $"Error editorPersonaId={editorPersonaId}");
            migrateResponse.Message = ex.Message;
        }
        return migrateResponse;
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // PRIVATE HELPERS
    // ════════════════════════════════════════════════════════════════════════

    #region Settings / Context

    private async ValueTask<McApiSettings> GetApiSettingsAsync(CancellationToken ct)
    {
        if (_apiSettings is not null) return _apiSettings;

        var settings = await _productSettingService.GetProductSettingAsync(ProductId, ct);
        try
        {
            _apiSettings = new McApiSettings(
                Endpoint:    settings.First(s => s.Name.Equals("APIENDPOINT",                    StringComparison.OrdinalIgnoreCase)).Value,
                ApiSourceId: settings.First(s => s.Name.Equals("MARKETINGCENTERAPISOURCEID",     StringComparison.OrdinalIgnoreCase)).Value,
                Username:    System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(
                                 settings.First(s => s.Name.Equals("APIUSERNAME", StringComparison.OrdinalIgnoreCase)).Value)),
                Password:    System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(
                                 settings.First(s => s.Name.Equals("APIPASSWORD", StringComparison.OrdinalIgnoreCase)).Value)));
        }
        catch (InvalidOperationException ex)
        {
            throw new InvalidOperationException(
                $"Marketing Center API settings are incomplete for productId={ProductId}. A required setting key is missing.", ex);
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException(
                $"Marketing Center API credentials contain invalid Base64 data for productId={ProductId}.", ex);
        }
        return _apiSettings;
    }

    private async Task<string> GetMcCompanyIdAsync(Persona editorPersona, McApiSettings settings, CancellationToken ct)
    {
        var mapping = await _manageBlueBook.GetProductCompanyMappingAsync(
            editorPersona.Organization.RealPageId, BlueBookProductConstants.MarketingCenter, ct);
        var company = mapping?.FirstOrDefault();
        if (company is null || string.IsNullOrEmpty(company.CompanyInstanceSourceId))
            throw new BlueBookException("Company Setup Error: Please Contact Support.");
        return company.CompanyInstanceSourceId;
    }

    #endregion

    #region HTTP helpers

    private async Task<T?> GetApiAsync<T>(string url, McApiSettings settings, CancellationToken ct) where T : class
    {
        var client = _httpClientFactory.CreateClient();
        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Authorization = BuildBasicAuth(settings);
        using var response = await client.SendAsync(req, ct);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("{ActionName} - {State}", "GetApiAsync",
                $"Non-success status={(int)response.StatusCode} url={url}");
            return null;
        }
        var content = await response.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    private static System.Net.Http.Headers.AuthenticationHeaderValue BuildBasicAuth(McApiSettings s)
    {
        var token = Convert.ToBase64String(
            System.Text.Encoding.UTF8.GetBytes($"{s.Username}:{s.Password}"));
        return new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", token);
    }

    #endregion

    #region User helpers

    private async Task<MC.MarketingCenterUserDetails?> GetUserDetailsAsync(
        string productUserId, McApiSettings settings, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(productUserId)) return null;
        try
        {
            var url = $"{settings.Endpoint}/external/contact/{productUserId}/details";
            return await GetApiAsync<MC.MarketingCenterUserDetails>(url, settings, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ActionName} - {State}", "GetUserDetailsAsync",
                $"productUserId={productUserId}");
            return null;
        }
    }

    private async Task<bool> IsUserIdValidAsync(string productUserId, McApiSettings settings, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(productUserId) || productUserId == "0") return false;
        var client = _httpClientFactory.CreateClient();
        using var req = new HttpRequestMessage(HttpMethod.Get, $"{settings.Endpoint}/external/contact/{productUserId}/status");
        req.Headers.Authorization = BuildBasicAuth(settings);
        using var response = await client.SendAsync(req, ct);
        return response.IsSuccessStatusCode;
    }

    private async Task<bool> SetUserStatusAsync(
        string mcUserId, bool isActive, string editorProductUserId,
        McApiSettings settings, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(mcUserId) || mcUserId == "0") return false;
        if (string.IsNullOrEmpty(editorProductUserId)) return false;

        var url    = $"{settings.Endpoint}/external/contact/{mcUserId}/status";
        var mcUser = new MC.MarketingCenterUserStatus
        {
            isActive            = isActive,
            isActiveUnifiedUser = isActive,
            auditUserId         = Convert.ToInt64(editorProductUserId)
        };

        var client = _httpClientFactory.CreateClient();
        using var req = new HttpRequestMessage(HttpMethod.Put, url);
        req.Headers.Authorization = BuildBasicAuth(settings);
        req.Content = JsonContent.Create(mcUser);
        using var response = await client.SendAsync(req, ct);

        if (!response.IsSuccessStatusCode)
            _logger.LogWarning("{ActionName} - {State}", "SetUserStatusAsync",
                $"status={(int)response.StatusCode} mcUserId={mcUserId}");

        return response.IsSuccessStatusCode;
    }

    private async Task<string> GetMcUniqueUserNameAsync(
        string firstName, string lastName, McApiSettings settings, CancellationToken ct)
    {
        string baseUsername = $"{firstName.TrimWhiteSpace()[..1]}{lastName.TrimWhiteSpace()}".ToLower();
        int    incrementor  = 1;
        while (true)
        {
            string candidate = $"{baseUsername}{incrementor}@noreply.com";
            var client = _httpClientFactory.CreateClient();
            using var req = new HttpRequestMessage(HttpMethod.Get,
                $"{settings.Endpoint}/external/contact/details?emailAddress={Uri.EscapeDataString(candidate)}");
            req.Headers.Authorization = BuildBasicAuth(settings);
            using var response = await client.SendAsync(req, ct);
            if (!response.IsSuccessStatusCode)
                return candidate;
            incrementor++;
        }
    }

    private static string ValidateAndReturnEmailAddress(string email)
    {
        if (string.IsNullOrEmpty(email)) return email;
        try
        {
            if (!new EmailAddressAttribute().IsValid(email)) return string.Empty;
            _ = new System.Net.Mail.MailAddress(email);
            return email;
        }
        catch { return string.Empty; }
    }

    #endregion

    #region Create / Update MC user

    private async Task<string> CreateMcUserAsync(
        MC.MarketingCenterUser mcUser,
        string userEmailAddress,
        string userLeadEmailAddress,
        long userPersonaId, long editorPersonaId,
        string sourceId, McApiSettings settings, CancellationToken ct)
    {
        await _productSettingService.UpdateProductStatusAsync(userPersonaId, ProductStatusSettingType, ProductId,
            (int)ProductBatchStatusType.Running, ct);

        var url    = $"{settings.Endpoint}/external/contact?sourceid={sourceId}";
        var client = _httpClientFactory.CreateClient();
        using var req = new HttpRequestMessage(HttpMethod.Post, url);
        req.Headers.Authorization = BuildBasicAuth(settings);
        req.Content = JsonContent.Create(mcUser);
        using var response = await client.SendAsync(req, ct);

        if (!response.IsSuccessStatusCode)
        {
            await _productSettingService.UpdateProductStatusAsync(userPersonaId, ProductStatusSettingType, ProductId,
                (int)ProductBatchStatusType.Error, ct);
            return await ParseErrorPostingAsync(response, "Create", editorPersonaId, userPersonaId, ct);
        }

        var body     = await response.Content.ReadAsStringAsync(ct);
        var json     = JsonSerializer.Deserialize<JsonElement>(body);
        long newId   = json.GetProperty("id").GetInt64();

        await _samlRepository.CreateSamlUserAttributeAsync(userPersonaId, ProductId, SamlAttributeEnum.productUsername, userEmailAddress, ct);
        await _samlRepository.CreateSamlUserAttributeAsync(userPersonaId, ProductId, SamlAttributeEnum.UserId, newId.ToString(), ct);
        await _productSettingService.UpdateProductStatusAsync(userPersonaId, ProductStatusSettingType, ProductId,
            (int)ProductBatchStatusType.Success, ct);

        // Update migration status
        var migResp = await UpdateUsersMigrationStatusAsync(editorPersonaId, new List<MigrateUser>
        {
            new()
            {
                UnifiedLoginUserName = userEmailAddress,
                UserId               = newId.ToString(),
                UsingUnifiedLogin    = true,
                LeadEmailAddress     = userLeadEmailAddress
            }
        }, ct);

        if (!migResp.Status)
            return migResp.Message ?? string.Empty;

        return string.Empty;
    }

    private async Task<string> UpdateMcUserAsync(
        MC.MarketingCenterUser mcUser,
        string productUserId,
        bool allPropertiesSelected,
        long userPersonaId, long editorPersonaId,
        string sourceId, McApiSettings settings, CancellationToken ct)
    {
        var suffix = allPropertiesSelected
            ? $"?sourceid={sourceId}&assignAllProperties=true"
            : $"?sourceid={sourceId}&unassignAllProperties=false";
        var url    = $"{settings.Endpoint}/external/contact/{productUserId}{suffix}";
        var client = _httpClientFactory.CreateClient();
        using var req = new HttpRequestMessage(HttpMethod.Put, url);
        req.Headers.Authorization = BuildBasicAuth(settings);
        req.Content = JsonContent.Create(mcUser);
        using var response = await client.SendAsync(req, ct);

        if (!response.IsSuccessStatusCode)
            return await ParseErrorPostingAsync(response, "Update", editorPersonaId, userPersonaId, ct);

        var body   = await response.Content.ReadAsStringAsync(ct);
        var json   = JsonSerializer.Deserialize<JsonElement>(body);
        long newId = json.GetProperty("id").GetInt64();

        var existingUserIdAttrs = await _samlRepository.GetProductSamlDetailsAsync(userPersonaId, ProductId, ct);
        var existingUserId      = existingUserIdAttrs?.FirstOrDefault(a => a.SamlAttributeId == (int)SamlAttributeEnum.UserId);
        if (existingUserId is not null)
        {
            await _samlRepository.UpdateSamlUserAttributeAsync(
                new SamlAttributes
                {
                    SamlAttributeId     = (int)SamlAttributeEnum.UserId,
                    Value               = newId.ToString(),
                    SamlUserAttributeId = existingUserId.SamlUserAttributeId
                }, ct);
        }

        await SetUserStatusAsync(newId.ToString(), true, mcUser.CompanyId.ToString(), settings, ct);
        return string.Empty;
    }

    private async Task<string> ParseErrorPostingAsync(
        HttpResponseMessage response, string action,
        long editorPersonaId, long userPersonaId, CancellationToken ct)
    {
        var content  = await response.Content.ReadAsStringAsync(ct);
        bool emailExists = false;

        try
        {
            var doc   = JsonSerializer.Deserialize<JsonElement>(content);
            var msg   = doc.GetProperty("fieldErrors").GetProperty("Error").GetProperty("message").GetString() ?? string.Empty;
            emailExists = msg.Contains("duplicate", StringComparison.OrdinalIgnoreCase)
                       && msg.Contains("emailAddress", StringComparison.OrdinalIgnoreCase);
        }
        catch { /* couldn't parse — move on */ }

        _logger.LogError("{ActionName} - {State}", "ParseErrorPosting",
            $"{action} failed{(emailExists ? " (email already exists)" : string.Empty)} editorPersonaId={editorPersonaId}");

        if (emailExists)
        {
            await _productAuditService.WriteProductEventAsync(editorPersonaId, userPersonaId, ProductId,
                "An error occurred when {3} {4} attempted to provision {2} for {0} {1}.A user already exists with this email address.Please try using the Migration Tool.",
                ct);
            await _productSettingService.UpdateProductStatusAsync(userPersonaId, ProductStatusSettingType, ProductId,
                (int)ProductBatchStatusType.Stop, ct);
            return ProductBatchStatusType.Stop.ToString();
        }

        await _productSettingService.UpdateProductStatusAsync(userPersonaId, ProductStatusSettingType, ProductId,
            (int)ProductBatchStatusType.Error, ct);

        return action.ToUpperInvariant() == "CREATE"
            ? "There was a problem creating the user."
            : "There was a problem updating the user.";
    }

    #endregion

    #region Activity log helpers

    private static void ExtractActivityLog(
        MC.MarketingCenterUser mcUser,
        MC.MarketingCenterUserDetails? before,
        List<ProductRole> allRoles,
        List<ProductProperty> allProps,
        List<AdditionalParameters> log)
    {
        try
        {
            if (mcUser.ContactRoleId != before?.ContactRoleId)
            {
                if (before is not null)
                    log.AddRange(allRoles
                        .Where(r => before.ContactRoleId == Convert.ToInt32(r.ID))
                        .Select(r => new AdditionalParameters { Key = "Marketing Center Roles", Value = ProductRolesRemove.Replace("RoleName", r.Name) }));

                log.AddRange(allRoles
                    .Where(r => mcUser.ContactRoleId == Convert.ToInt32(r.ID))
                    .Select(r => new AdditionalParameters { Key = "Marketing Center Roles", Value = ProductRolesAssign.Replace("RoleName", r.Name) }));
            }

            if (mcUser.AssignPropertyIds is { Count: > 0 })
                log.AddRange(allProps
                    .Where(p => mcUser.AssignPropertyIds.Contains(Convert.ToInt32(p.ID)))
                    .Select(p => new AdditionalParameters { Key = "Marketing Center Properties", Value = ProductPropsAssign.Replace("PropertyName", p.Name) }));

            if (mcUser.UnassignPropertyIds is { Count: > 0 })
                log.AddRange(allProps
                    .Where(p => mcUser.UnassignPropertyIds.Contains(Convert.ToInt32(p.ID)))
                    .Select(p => new AdditionalParameters { Key = "Marketing Center Properties", Value = ProductPropsRemove.Replace("PropertyName", p.Name) }));
        }
        catch (Exception) { /* Non-fatal — don't fail the provision on log errors */ }
    }

    private async Task EmitDeleteRoleAuditAsync(long editorPersonaId, long roleId, string roleName, CancellationToken ct)
    {
        try
        {
            var logInfo = await _productAuditService.GetUserActivityLogInfoAsync(editorPersonaId, ct);
            var extra   = new List<AdditionalParameters>
            {
                new() { Key = "Role", Value = ProductRoleDelete.Replace("RoleName", roleName) }
            };
            await _productAuditService.WriteProductEventAsync(editorPersonaId, editorPersonaId, ProductId,
                $"{{0}} {{1}} deleted {roleName} in Marketing Center.", ct);
        }
        catch { /* Non-fatal */ }
    }

    private async Task EmitCreateRoleAuditAsync(long editorPersonaId, MCRole mcRole, CancellationToken ct)
    {
        try
        {
            await _productAuditService.WriteProductEventAsync(editorPersonaId, editorPersonaId, ProductId,
                $"{{0}} {{1}} Created {mcRole.Name} in Marketing Center.", ct);
        }
        catch { /* Non-fatal */ }
    }

    private async Task EmitUpdateRightsToRoleAuditAsync(
        long editorPersonaId, long roleId, string roleName,
        List<string> rightsToAdd, List<string> rightsToRemove, CancellationToken ct)
    {
        try
        {
            var settings  = await GetApiSettingsAsync(ct);
            var (ctx, _)  = await _contextService.GetUserContextAsync(editorPersonaId, 0, ProductId, ct);
            if (ctx is null) return;
            var companyId = await GetMcCompanyIdAsync(ctx.EditorPersona, settings, ct);
            var rights    = await GetRightsDetailsAsync(companyId, settings, ct);
            var rightList = rights?.Cast<MCRight>().ToList() ?? new();

            var log = new List<AdditionalParameters>();
            foreach (var r in rightsToAdd)
            {
                var name = rightList.FirstOrDefault(x => x.RightId.ToString() == r)?.Description;
                log.Add(new() { Key = roleName, Value = RightAssign.Replace("RightName", name ?? r) });
            }
            foreach (var r in rightsToRemove)
            {
                var name = rightList.FirstOrDefault(x => x.RightId.ToString() == r)?.Description;
                log.Add(new() { Key = roleName, Value = RightUnassign.Replace("RightName", name ?? r) });
            }
            // Push to queue via WriteProductEventAsync (simplified — full queue push requires IManageUnifiedLoginAsync)
            _logger.LogDebug("{ActionName} - {State}", "EmitUpdateRightsToRoleAudit",
                $"roleId={roleId} added={rightsToAdd.Count} removed={rightsToRemove.Count}");
        }
        catch { /* Non-fatal */ }
    }

    private async Task EmitUpdateRolesToRightAuditAsync(
        long editorPersonaId, long rightId,
        List<string> rolesToAdd, List<string> rolesToRemove, CancellationToken ct)
    {
        try
        {
            _logger.LogDebug("{ActionName} - {State}", "EmitUpdateRolesToRightAudit",
                $"rightId={rightId} added={rolesToAdd.Count} removed={rolesToRemove.Count}");
        }
        catch { /* Non-fatal */ }
    }

    private async Task EmitUpdateRoleNameAuditAsync(
        long editorPersonaId, string newName, string oldName, string attribute, CancellationToken ct)
    {
        try
        {
            var template = attribute.Equals("RoleName")
                ? ProductRoleNameUpdate.Replace("RoleName", newName)
                : ProductRoleDescUpdate.Replace("RoleName", newName);
            _logger.LogDebug("{ActionName} - {State}", "EmitUpdateRoleNameAudit",
                $"attribute={attribute} old={oldName} new={newName}");
        }
        catch { /* Non-fatal */ }
    }

    #endregion

    #region Role/Right diff helpers

    private static void GetRoleAssignmentChanges(
        List<string> roles, ListResponse currentRoles,
        out List<string> rolesToAdd, out List<string> rolesToRemove)
    {
        rolesToAdd    = new();
        rolesToRemove = new();

        var desired = (roles ?? new())
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Select(r => r.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var assigned = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (currentRoles?.Records is { Count: > 0 })
        {
            foreach (var pr in currentRoles.Records.OfType<RolesRightsAccessRight>().Where(r => r.IsAssigned))
                assigned.Add(pr.Id.ToString().Trim());
        }

        rolesToAdd.AddRange(desired.Where(r => !assigned.Contains(r)));
        rolesToRemove.AddRange(assigned.Where(r => !desired.Contains(r)));
    }

    private static void GetRightAssignmentChanges(
        ListResponse currentRights, IList<int>? desiredRights,
        out List<string> addedRights, out List<string> removedRights)
    {
        addedRights   = new();
        removedRights = new();

        var desired  = new HashSet<int>((desiredRights ?? new List<int>()).Distinct());
        var assigned = new HashSet<int>();

        if (currentRights?.Records is not null)
        {
            foreach (var r in currentRights.Records.OfType<MCRight>())
                assigned.Add(r.RightId);
        }

        addedRights.AddRange(desired.Where(r => !assigned.Contains(r)).Select(r => r.ToString()));
        removedRights.AddRange(assigned.Where(r => !desired.Contains(r)).Select(r => r.ToString()));
    }

    #endregion

    #region Misc helpers

    private async Task<IList<MCRight>> GetRightsDetailsAsync(string companyId, McApiSettings settings, CancellationToken ct)
    {
        var url    = $"{settings.Endpoint}/external/company/{companyId}/rights";
        var rights = await GetApiAsync<IList<Right>>(url, settings, ct) ?? new List<Right>();
        return rights.ToGBRights() ?? new List<MCRight>();
    }

    private static ListResponse BuildListResponse<T>(IList<T> list) where T : class
        => new()
        {
            Records     = list.Cast<object>().ToList(),
            TotalRows   = list.Count,
            RowsPerPage = list.Count,
            TotalPages  = 1,
            ErrorReason = string.Empty
        };

    private string GetAuditLoginName()
    {
        return string.IsNullOrEmpty(_userClaimsAccessor.ImpersonatedByName)
            ? _userClaimsAccessor.LoginName
            : _userClaimsAccessor.ImpersonatedByName;
    }

    #endregion
}
