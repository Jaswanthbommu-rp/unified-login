using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
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
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Lead2Lease;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.Saml;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Product;

/// <summary>
/// Native-async implementation of Lead2Lease product user management.
/// <para>
/// Replaces the stepping-stone <c>ManageProductLead2LeaseAsync</c> wrapper and the
/// .NET 4.8 sync class <c>ManageProductLead2Lease</c>:
/// </para>
/// <list type="bullet">
///   <item><c>IHttpClientFactory</c> replaces <c>new HttpClient()</c> per-call.</item>
///   <item><c>IMemoryCache</c> replaces <c>MemoryCache.Default</c> / <c>RPObjectCache</c>.</item>
///   <item><c>IProductContextServiceAsync</c> replaces per-method <c>DefaultUserClaim</c> and the private <c>GetCallContextAsync</c> helper.</item>
///   <item><c>ILogger&lt;T&gt;</c> replaces <c>WriteToDiagnosticLog</c> / <c>WriteToErrorLog</c>.</item>
///   <item>Injected repositories replace <c>new ProductRepository()</c> etc.</item>
///   <item>Tuple return on <c>ManageLead2LeaseUserAsync</c> eliminates the <c>out</c> parameter.</item>
/// </list>
/// </summary>
public sealed class ManageProductLead2LeaseAsync : IManageProductLead2LeaseAsync
{
    #region Constants

    private const int    ProductId                   = (int)ProductEnum.Lead2Lease;
    private const string ProductStatusSettingType    = "ProductStatus";
    private const int    DeactivatedBatchCacheSeconds = 600;

    // Activity-log templates (match ManageProductBase constants)
    private const string RolesAssignedMsg   = "{\"action\":\"Assigned\",\"value\":\"RoleName\"}";
    private const string RolesRemovedMsg    = "{\"action\":\"Removed\",\"value\":\"RoleName\"}";
    private const string PropsAssignedMsg   = "{\"action\":\"Assigned\",\"value\":\"PropertyName\"}";
    private const string PropsRemovedMsg    = "{\"action\":\"Removed\",\"value\":\"PropertyName\"}";

    // Super-user admin rights in L2L (used to assign all admin permissions)
    private static readonly IReadOnlySet<string> AdminRights = new HashSet<string>(
        StringComparer.OrdinalIgnoreCase)
    {
        "ALLOW USER TO CHANGE PASSWORDS MANUALLY",
        "ATTACH FILE FROM ATTACHMENT MANAGER",
        "ATTACH FILES ON DEMAND",
        "CAN SCHEDULE REPORTS",
        "ENABLE PUSH NOTIFICATIONS",
        "EXPORT LEADS - MULTIPLE PROPERTIES",
        "FULL ACCESS",
        "MANAGE FILES IN ATTACHMENT MANAGER",
        "RUN MULTI PROPERTY REPORTS",
        "SCORE CALLS",
        "SEND EMAILS FROM LEAD2LEASE",
        "SET AUTORESPONSE POLICIES",
        "SET EMAIL PREFERENCES",
        "SET PROPERTY SETTINGS",
        "SUPER USER"
    };

    #endregion

    #region Private records

    private sealed record L2LApiSettings(string ApiEndpoint, string MtApiEndpoint);

    #endregion

    #region Fields

    private readonly IHttpClientFactory                      _httpClientFactory;
    private readonly IProductContextServiceAsync             _contextService;
    private readonly ISamlRepositoryAsync                    _samlRepository;
    private readonly IProductRepositoryAsync                 _productRepository;
    private readonly IProductSettingServiceAsync             _productSettingService;
    private readonly IManagePersonaAsync                     _managePersona;
    private readonly IManagePersonAsync                      _managePerson;
    private readonly IManageUserLoginAsync                   _manageUserLogin;
    private readonly IManageBlueBookAsync                    _manageBlueBook;
    private readonly IManageElectronicAddressAsync           _manageElectronicAddress;
    private readonly IManageProductOneSiteAsync              _manageOneSite;
    private readonly IProductAuditServiceAsync               _productAuditService;
    private readonly IMemoryCache                            _cache;
    private readonly ILogger<ManageProductLead2LeaseAsync>   _logger;

    // Lazy-loaded on first call; scoped instance so no lock needed
    private L2LApiSettings? _apiSettings;

    #endregion

    #region Constructor

    public ManageProductLead2LeaseAsync(
        IHttpClientFactory                      httpClientFactory,
        IProductContextServiceAsync             contextService,
        ISamlRepositoryAsync                    samlRepository,
        IProductRepositoryAsync                 productRepository,
        IProductSettingServiceAsync             productSettingService,
        IManagePersonaAsync                     managePersona,
        IManagePersonAsync                      managePerson,
        IManageUserLoginAsync                   manageUserLogin,
        IManageBlueBookAsync                    manageBlueBook,
        IManageElectronicAddressAsync           manageElectronicAddress,
        IManageProductOneSiteAsync              manageOneSite,
        IProductAuditServiceAsync               productAuditService,
        IMemoryCache                            cache,
        ILogger<ManageProductLead2LeaseAsync>   logger)
    {
        _httpClientFactory      = httpClientFactory;
        _contextService         = contextService;
        _samlRepository         = samlRepository;
        _productRepository      = productRepository;
        _productSettingService  = productSettingService;
        _managePersona          = managePersona;
        _managePerson           = managePerson;
        _manageUserLogin        = manageUserLogin;
        _manageBlueBook         = manageBlueBook;
        _manageElectronicAddress = manageElectronicAddress;
        _manageOneSite          = manageOneSite;
        _productAuditService    = productAuditService;
        _cache                  = cache;
        _logger                 = logger;
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

            var settings  = await GetApiSettingsAsync(cancellationToken);
            var roleInfo  = await GetRolesMainAsync(settings, cancellationToken);
            var list      = roleInfo.Roles?.ToGBRoles() ?? new List<ProductRole>();

            if (!string.IsNullOrEmpty(ctx.ProductUserId))
            {
                var user = await GetUserAsync(ctx.ProductUserId, settings, cancellationToken);
                if (user is null)
                    return new ListResponse { IsError = true, ErrorReason = "User info is missing" };

                foreach (var p in user.Permissions ?? [])
                {
                    var pr = list.FirstOrDefault(r => r.ID == p.UserRoleId.ToString());
                    if (pr is not null)
                        pr.IsAssigned = true;
                }
            }

            list = list.OrderBy(r => r.Name).ToList();
            response = new ListResponse
            {
                Records     = list.Cast<object>().ToList(),
                TotalRows   = list.Count,
                RowsPerPage = 9999,
                TotalPages  = 1,
                ErrorReason = string.Empty
            };

            if (roleInfo.Presets?.Count > 0)
                response.Additional = new Dictionary<string, object> { ["Presets"] = roleInfo.Presets };
        }
        catch (Exception ex) when (ex is BlueBookException)
        {
            response.IsError     = true;
            response.ErrorReason = ex.Message;
        }
        catch (Exception ex)
        {
            response.IsError     = true;
            response.ErrorReason = CommonMessageConstants.RightErrorMessage;
            _logger.LogError(ex, "{ActionName} - {State}", "GetRolesAsync",
                $"Error editorPersonaId={editorPersonaId}");
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

            var settings     = await GetApiSettingsAsync(cancellationToken);
            var company      = await GetL2LCompanyAsync(ctx.EditorPersona.Organization.RealPageId, cancellationToken);
            var allProps     = await GetPropertyMainAsync(company.CompanyInstanceSourceId, settings, cancellationToken);

            if (allProps is null)
                return new ListResponse { IsError = true, ErrorReason = "Company Setup Error: Please Contact Support." };

            var list = allProps.ToGBProperty() ?? new List<ProductProperty>();

            if (!string.IsNullOrEmpty(ctx.ProductUserId))
            {
                var user = await GetUserAsync(ctx.ProductUserId, settings, cancellationToken);
                if (user is null)
                    return new ListResponse { IsError = true, ErrorReason = "User info is missing" };

                var assignedProps = user.Properties ?? [];

                // If user has no active properties, fall back to deactivated batch data
                if (assignedProps.Count == 0)
                {
                    var batch = await GetDeactivatedBatchDataAsync(userPersonaId, cancellationToken);
                    if (batch?.PropertyList is { Count: > 0 })
                    {
                        assignedProps = batch.PropertyList
                            .Select(id => new Property { PropertyId = Convert.ToInt32(id) })
                            .ToList();
                    }
                }

                foreach (var p in assignedProps)
                {
                    var pp = list.FirstOrDefault(x => x.ID == p.PropertyId.ToString());
                    if (pp is not null)
                        pp.IsAssigned = true;
                }
            }

            list = list.OrderBy(p => p.Name).ToList();
            response = new ListResponse
            {
                Records     = list.Cast<object>().ToList(),
                TotalRows   = list.Count,
                RowsPerPage = 9999,
                TotalPages  = 1,
                ErrorReason = string.Empty
            };
        }
        catch (Exception ex) when (ex is BlueBookException)
        {
            response.IsError     = true;
            response.ErrorReason = ex.Message;
        }
        catch (Exception ex)
        {
            response.IsError     = true;
            response.ErrorReason = CommonMessageConstants.PropertyErrorMessage;
            _logger.LogError(ex, "{ActionName} - {State}", "GetPropertiesAsync",
                $"Error editorPersonaId={editorPersonaId}");
        }
        return response;
    }

    #endregion

    #region ManageLead2LeaseUser

    /// <inheritdoc/>
    public async Task<(string Error, List<AdditionalParameters> ActivityLog)> ManageLead2LeaseUserAsync(
        long editorPersonaId, long userPersonaId,
        List<string> roleList, List<string> propertyList,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("{ActionName} - {State}", "ManageLead2LeaseUserAsync",
            $"Begin editorPersonaId={editorPersonaId} userPersonaId={userPersonaId}");

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
            var isSuperUser = await _contextService.IsSuperUserAsync(ctx!.UserPersona!, cancellationToken);

            _logger.LogDebug("{ActionName} - {State}", "ManageLead2LeaseUserAsync",
                $"isSuperUser={isSuperUser} userPersonaId={userPersonaId}");

            // Resolve email address
            var userEmailAddress = await GetEmailAddressAsync(userLogin.RealPageId, userLogin.LoginName, cancellationToken);
            userEmailAddress     = ValidateAndReturnEmailAddress(userEmailAddress);

            _logger.LogDebug("{ActionName} - {State}", "ManageLead2LeaseUserAsync",
                $"userEmailAddress resolved userPersonaId={userPersonaId}");

            // Get API data needed for both create and update paths
            var company      = await GetL2LCompanyAsync(ctx.EditorPersona.Organization.RealPageId, cancellationToken);
            var allProps     = await GetPropertyMainAsync(company.CompanyInstanceSourceId, settings, cancellationToken);
            if (allProps is null)
            {
                _logger.LogError("{ActionName} - {State}", "ManageLead2LeaseUserAsync", "GetPropertyMain returned null");
                return ("Company Setup Error: Please Contact Support.", activityLog);
            }

            var roleInfo = await GetRolesMainAsync(settings, cancellationToken);

            // Build the L2L user object
            var l2LUser        = new Lead2LeaseUser();
            var userBeforeUpdate = new Lead2LeaseUser();

            if (string.IsNullOrEmpty(ctx.ProductUserId))
            {
                // New user
                l2LUser.Password = Guid.NewGuid().ToString("N");
                l2LUser.UserType = isSuperUser ? "power user" : "user";
            }
            else
            {
                // Existing user — load current state
                l2LUser = await GetUserAsync(ctx.ProductUserId, settings, cancellationToken) ?? new Lead2LeaseUser();
                if (l2LUser.UserId == 0)
                {
                    _logger.LogError("{ActionName} - {State}", "ManageLead2LeaseUserAsync", "User info missing");
                    return ("User info missing", activityLog);
                }
                userBeforeUpdate = (Lead2LeaseUser)l2LUser.Clone();
            }

            l2LUser.FirstName = person.FirstName;
            l2LUser.LastName  = person.LastName;
            l2LUser.Email     = userEmailAddress;

            // Build property list to save
            if (isSuperUser)
            {
                l2LUser.UserType = "power user";
                propertyList = allProps.Select(p => p.PropertyId.ToString()).ToList();
            }

            var propertyListToSave = await BuildPropertyListAsync(
                propertyList, allProps, isSuperUser,
                editorPersonaId, userPersonaId, settings, cancellationToken);

            // Clean up PMSystemID that has no PMUserId
            foreach (var p in propertyListToSave)
            {
                if (!string.IsNullOrEmpty(p.PMSystemID) && p.PMUserId == null)
                    p.PMSystemID = null;
            }

            // Build permissions list
            var permissionListToSave = await BuildPermissionsListAsync(
                propertyListToSave, roleList, isSuperUser, roleInfo, cancellationToken);

            l2LUser.Properties  = propertyListToSave;
            l2LUser.Permissions = permissionListToSave;

            // Create or update the user via the L2L API
            if (string.IsNullOrEmpty(ctx.ProductUserId))
            {
                var createError = await CreateL2LUserAsync(
                    l2LUser, userBeforeUpdate, userPersona, userLogin,
                    roleInfo, allProps, editorPersonaId, userPersonaId,
                    settings, activityLog, cancellationToken);
                if (!string.IsNullOrEmpty(createError))
                    return (createError, activityLog);
            }
            else
            {
                var updateError = await UpdateL2LUserAsync(
                    l2LUser, userBeforeUpdate,
                    roleInfo, allProps, editorPersonaId, userPersonaId,
                    settings, activityLog, cancellationToken);
                if (!string.IsNullOrEmpty(updateError))
                    return (updateError, activityLog);
            }

            await _productSettingService.UpdateProductStatusAsync(
                userPersonaId, ProductStatusSettingType, ProductId,
                (int)ProductBatchStatusType.Success, cancellationToken);

            return (string.Empty, activityLog);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ActionName} - {State}", "ManageLead2LeaseUserAsync",
                $"Error editorPersonaId={editorPersonaId} userPersonaId={userPersonaId}");
            return ("Error", activityLog);
        }
    }

    #endregion

    #region Unassign / Profile

    /// <inheritdoc/>
    public async Task<string> UnassignUserAsync(
        long editorPersonaId, long userPersonaId,
        CancellationToken cancellationToken = default)
    {
        var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, userPersonaId, ProductId, cancellationToken);
        if (ctxError is not null)
            return ctxError.ErrorReason!;

        var settings = await GetApiSettingsAsync(cancellationToken);
        var disableError = await EnableDisableUserAsync(ctx.ProductUserId, isActive: false, settings, cancellationToken);

        if (!string.IsNullOrEmpty(disableError))
        {
            await _productSettingService.UpdateProductStatusAsync(
                userPersonaId, ProductStatusSettingType, ProductId,
                (int)ProductBatchStatusType.Error, cancellationToken);
            _logger.LogWarning("{ActionName} - {State}", "UnassignUserAsync",
                $"Disable failed userPersonaId={userPersonaId}");
            return "Error";
        }

        _logger.LogDebug("{ActionName} - {State}", "UnassignUserAsync",
            $"Successfully deactivated userPersonaId={userPersonaId}");
        await _productSettingService.UpdateProductStatusAsync(
            userPersonaId, ProductStatusSettingType, ProductId,
            (int)ProductBatchStatusType.Deleted, cancellationToken);

        return string.Empty;
    }

    /// <inheritdoc/>
    public async Task<string> UpdateLead2LeaseUserProfileAsync(
        long editorPersonaId, long userPersonaId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, userPersonaId, ProductId, cancellationToken);
            if (ctxError is not null)
                return ctxError.ErrorReason!;

            var settings    = await GetApiSettingsAsync(cancellationToken);
            var userPersona = await _managePersona.GetPersonaAsync(userPersonaId, cancellationToken: cancellationToken);
            var realPageId  = userPersona.RealPageId;
            var person      = await _managePerson.GetPersonAsync(realPageId, cancellationToken);
            var userLogin   = await _manageUserLogin.GetUserLoginOnlyAsync(realPageId, cancellationToken);

            var orgList          = await _manageUserLogin.GetUserPersonaOrganizationAsync(userLogin.LoginName, cancellationToken: cancellationToken);
            var isRegularNoEmail = await _contextService.IsRegularUserNoEmailAsync(ctx!.UserPersona!, cancellationToken);

            var userEmailAddress = await GetEmailAddressAsync(userLogin.RealPageId, userLogin.LoginName, cancellationToken);

            // Choose final email / product login name based on user type
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

                // If primary org and login name changed, update product login too
                bool isPrimaryOrg = orgList.Any(o =>
                    o.PrimaryOrganization &&
                    o.OrganizationPartyId == userPersona.OrganizationPartyId);

                if (isPrimaryOrg &&
                    !ctx.ProductUsername.Equals(userEmailAddress, StringComparison.OrdinalIgnoreCase))
                {
                    productLoginName = userEmailAddress;
                }
            }

            var user = await GetUserAsync(ctx.ProductUserId, settings, cancellationToken);
            if (user is null)
            {
                _logger.LogWarning("{ActionName} - {State}", "UpdateLead2LeaseUserProfileAsync",
                    $"User not found in product userPersonaId={userPersonaId}");
                return "User not found in product";
            }

            var l2LUser = new Lead2LeaseUser
            {
                UserId    = user.UserId,
                FirstName = person.FirstName,
                LastName  = person.LastName,
                Email     = userEmailAddress,
                UserName  = productLoginName
            };

            var url      = $"{settings.ApiEndpoint}/Users/profile";
            var response = await PutApiAsync(url, l2LUser, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug("{ActionName} - {State}", "UpdateLead2LeaseUserProfileAsync",
                    $"Updating SAML productUsername={productLoginName}");
                await UpdateSamlUsernameAsync(userPersonaId, productLoginName, cancellationToken);
                await _productAuditService.WriteUserTypeChangeAsync(
                    editorPersonaId, userPersonaId, ProductId,
                    BatchProcessType.ProfileUpdate, cancellationToken);
                return string.Empty;
            }

            string errorContent = string.Empty;
            try { errorContent = await response.Content.ReadAsStringAsync(cancellationToken); } catch { /* ignored */ }

            _logger.LogError("{ActionName} - {State}", "UpdateLead2LeaseUserProfileAsync",
                $"PUT failed editorPersonaId={editorPersonaId} status={(int)response.StatusCode}");
            return $"There was a problem updating user profile for editorPersona id - {editorPersonaId} - Error-{errorContent}.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ActionName} - {State}", "UpdateLead2LeaseUserProfileAsync",
                $"Error editorPersonaId={editorPersonaId}");
            return $"Error - {ex.Message}";
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
        try
        {
            // Use the caller-supplied productUserId (migration path), not the one from SAML
            var url      = isActive
                ? $"{settings.ApiEndpoint}/Users/Enable/{productUserId}"
                : $"{settings.ApiEndpoint}/Users/Disable/{productUserId}";

            var l2LUser = await GetUserAsync(productUserId, settings, cancellationToken);
            int[] propIds = l2LUser?.Properties?.Select(p => p.PropertyId).ToArray() ?? [];

            var response = await PutApiAsync(url, propIds, cancellationToken);
            if (response.IsSuccessStatusCode) return true;

            var errMsg = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("{ActionName} - {State}", "ChangeUserStatusAsync",
                $"status={(int)response.StatusCode} error={errMsg} editorPersonaId={editorPersonaId}");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ActionName} - {State}", "ChangeUserStatusAsync",
                $"Failed userName={userName} editorPersonaId={editorPersonaId}");
            return false;
        }
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
            var company   = await GetL2LCompanyAsync(ctx.EditorPersona.Organization.RealPageId, cancellationToken);
            var companyId = Convert.ToInt32(company.CompanyInstanceSourceId);
            if (companyId == 0)
            {
                _logger.LogError("{ActionName} - {State}", "GetMigrationUsersAsync",
                    $"No company id in BlueBook for editorPersonaId={editorPersonaId}");
                response.ErrorReason = "Company Setup Error: Please Contact Support.";
                return response;
            }

            var filter      = "NonMigrated";
            var startRow    = 0;
            var resultsPerPage = 1000;
            if (datafilter != null)
            {
                if (datafilter.FilterBy?.TryGetValue("filter", out var f) == true)
                    filter = f;
                if (datafilter.Pages != null)
                {
                    startRow       = datafilter.Pages.StartRow;
                    resultsPerPage = datafilter.Pages.ResultsPerPage;
                }
            }

            var url      = $"{settings.MtApiEndpoint}/{companyId}/users"
                         + $"?filter={Uri.EscapeDataString(filter)}"
                         + $"&startRow={startRow}&resultsperpage={resultsPerPage}";
            var allUsers = await GetApiAsync<IList<MigrationUser>>(url, cancellationToken);

            if (allUsers is null)
            {
                _logger.LogError("{ActionName} - {State}", "GetMigrationUsersAsync",
                    $"No users from API editorPersonaId={editorPersonaId}");
                return response;
            }

            response = new ListResponse
            {
                IsError     = false,
                ErrorReason = string.Empty,
                RowsPerPage = resultsPerPage,
                TotalPages  = 1,
                Records     = allUsers.Cast<object>().ToList(),
                TotalRows   = allUsers.Count
            };
        }
        catch (Exception ex)
        {
            response = new ListResponse { IsError = true, ErrorReason = ex.Message };
            _logger.LogError(ex, "{ActionName} - {State}", "GetMigrationUsersAsync",
                $"Error editorPersonaId={editorPersonaId}");
        }
        return response;
    }

    /// <inheritdoc/>
    public async Task<MigrateResponse> UpdateUsersMigrationStatusAsync(
        long editorPersonaId, IList<MigrateUser> migrateUsers,
        CancellationToken cancellationToken = default)
    {
        var migrateResponse = new MigrateResponse { Status = false };
        try
        {
            var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, 0, ProductId, cancellationToken);
            if (ctxError is not null) { migrateResponse.Message = ctxError.ErrorReason; return migrateResponse; }

            var settings  = await GetApiSettingsAsync(cancellationToken);
            var company   = await GetL2LCompanyAsync(ctx.EditorPersona.Organization.RealPageId, cancellationToken);
            var companyId = Convert.ToInt32(company.CompanyInstanceSourceId);
            if (companyId == 0)
            {
                _logger.LogError("{ActionName} - {State}", "UpdateUsersMigrationStatusAsync",
                    $"No company id in BlueBook for editorPersonaId={editorPersonaId}");
                migrateResponse.Message = "Company Setup Error: Please Contact Support.";
                return migrateResponse;
            }

            var url      = $"{settings.MtApiEndpoint}/{companyId}/migrate-users";
            var response = await PutApiAsync(url, migrateUsers, cancellationToken);
            var content  = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var result = System.Text.Json.JsonSerializer.Deserialize<MigrateResponse>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                migrateResponse.Status  = result?.Status  ?? false;
                migrateResponse.Message = result?.Message ?? string.Empty;
                return migrateResponse;
            }

            _logger.LogError("{ActionName} - {State}", "UpdateUsersMigrationStatusAsync",
                $"API error status={(int)response.StatusCode} editorPersonaId={editorPersonaId}");
            migrateResponse.Message = "Cannot update user status to migrated.";
            return migrateResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ActionName} - {State}", "UpdateUsersMigrationStatusAsync",
                $"Error editorPersonaId={editorPersonaId}");
            return new MigrateResponse { Status = false, Message = ex.Message };
        }
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // PRIVATE HELPERS
    // ════════════════════════════════════════════════════════════════════════

    #region Settings / Context

    /// <summary>
    /// Lazily loads API settings from <see cref="IProductSettingServiceAsync"/> on first call.
    /// Cached in the scoped field <c>_apiSettings</c> — no lock needed for scoped services.
    /// </summary>
    private async ValueTask<L2LApiSettings> GetApiSettingsAsync(CancellationToken ct)
    {
        if (_apiSettings is not null) return _apiSettings;

        var settings = await _productSettingService.GetProductSettingAsync(ProductId, ct);
        _apiSettings = new L2LApiSettings(
            ApiEndpoint:   settings.First(s => s.Name.ToUpper() == "APIENDPOINT").Value,
            MtApiEndpoint: settings.First(s => s.Name.ToUpper() == "MTAPIENDPOINT").Value);
        return _apiSettings;
    }

    #endregion

    #region BlueBook / Company

    /// <summary>
    /// Returns the L2L <c>CustomerCompanyMap</c> for the given organisation.
    /// Replaces <c>ManageProductBase.GetProductCompanyInstanceId(_udmSourceCode)</c>.
    /// </summary>
    private async Task<CustomerCompanyMap> GetL2LCompanyAsync(Guid orgRealPageId, CancellationToken ct)
    {
        var list    = await _manageBlueBook.GetProductCompanyMappingAsync(orgRealPageId, "L2L", ct);
        var company = list?.FirstOrDefault();
        if (company is null || string.IsNullOrEmpty(company.CompanyInstanceSourceId))
            throw new BlueBookException("Company Setup Error: Please Contact Support.");
        return company;
    }

    #endregion

    #region HTTP helpers

    private async Task<T?> GetApiAsync<T>(string url, CancellationToken ct) where T : class
    {
        using var client   = _httpClientFactory.CreateClient();
        var response = await client.GetAsync(url, ct);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("{ActionName} - {State}", "GetApiAsync",
                $"HTTP {(int)response.StatusCode} {response.ReasonPhrase} url={url}");
            return null;
        }
        var json = await response.Content.ReadAsStringAsync(ct);
        return System.Text.Json.JsonSerializer.Deserialize<T>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    private async Task<System.Net.Http.HttpResponseMessage> PutApiAsync<T>(
        string url, T body, CancellationToken ct)
    {
        using var client = _httpClientFactory.CreateClient();
        return await client.PutAsJsonAsync(url, body, ct);
    }

    private async Task<System.Net.Http.HttpResponseMessage> PostApiAsync<T>(
        string url, T body, CancellationToken ct)
    {
        using var client = _httpClientFactory.CreateClient();
        return await client.PostAsJsonAsync(url, body, ct);
    }

    #endregion

    #region L2L API wrappers

    /// <summary>Fetches active roles from the L2L API. Throws <see cref="BlueBookException"/> if unavailable.</summary>
    private async Task<RoleInfo> GetRolesMainAsync(L2LApiSettings settings, CancellationToken ct)
    {
        var url    = $"{settings.ApiEndpoint}/Users/ActiveRoles";
        var result = await GetApiAsync<RoleInfo>(url, ct);
        if (result is null || result.Presets?.Any() != true)
            throw new BlueBookException(CommonMessageConstants.CompanyErrorMessage);
        return result;
    }

    /// <summary>Fetches active properties for the company from the L2L API.</summary>
    private async Task<IList<Property>?> GetPropertyMainAsync(
        string companyInstanceSourceId, L2LApiSettings settings, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(companyInstanceSourceId) || companyInstanceSourceId == "0")
            return null;

        var url = $"{settings.ApiEndpoint}/Users/ActiveProperties/{companyInstanceSourceId}";
        try
        {
            return await GetApiAsync<IList<Property>>(url, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ActionName} - {State}", "GetPropertyMainAsync",
                $"url={url}");
            return null;
        }
    }

    /// <summary>Fetches a single user by their L2L product user id.</summary>
    private async Task<Lead2LeaseUser?> GetUserAsync(
        string productUserId, L2LApiSettings settings, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(productUserId)) return null;
        var url = $"{settings.ApiEndpoint}/Users/{productUserId}";
        try
        {
            return await GetApiAsync<Lead2LeaseUser>(url, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ActionName} - {State}", "GetUserAsync",
                $"productUserId={productUserId}");
            return null;
        }
    }

    /// <summary>Enables or disables a L2L user account.</summary>
    private async Task<string> EnableDisableUserAsync(
        string productUserId, bool isActive, L2LApiSettings settings, CancellationToken ct)
    {
        var user    = await GetUserAsync(productUserId, settings, ct);
        var propIds = user?.Properties?.Select(p => p.PropertyId).ToArray() ?? [];
        var url     = isActive
            ? $"{settings.ApiEndpoint}/Users/Enable/{productUserId}"
            : $"{settings.ApiEndpoint}/Users/Disable/{productUserId}";

        var response = await PutApiAsync(url, propIds, ct);
        if (response.IsSuccessStatusCode) return string.Empty;

        var msg = await response.Content.ReadAsStringAsync(ct);
        _logger.LogError("{ActionName} - {State}", "EnableDisableUserAsync",
            $"status={(int)response.StatusCode} error={msg}");
        return $"There was a problem disabling the user - Error-{msg}.";
    }

    #endregion

    #region ManageLead2LeaseUser helpers

    /// <summary>
    /// Builds the list of <see cref="Property"/> objects to save for the user,
    /// enriching each with OneSite PMUser data where applicable.
    /// </summary>
    private async Task<List<Property>> BuildPropertyListAsync(
        List<string> propertyList,
        IList<Property> allProps,
        bool isSuperUser,
        long editorPersonaId, long userPersonaId,
        L2LApiSettings settings,
        CancellationToken ct)
    {
        var propertyListToSave = new List<Property>();

        foreach (var prptyId in propertyList)
        {
            if (isSuperUser)
            {
                propertyListToSave.Add(new Property { PropertyId = Convert.ToInt32(prptyId) });
                continue;
            }

            var p = allProps.FirstOrDefault(a => a.PropertyId.ToString() == prptyId);
            if (p is null) continue;

            propertyListToSave.Add(new Property
            {
                PropertyId  = p.PropertyId,
                PMSystemID  = p.PMSystemID
            });
        }

        // Enrich OneSite cross-product data for properties that have a PMSystemID
        var checkOneSite = !isSuperUser && propertyListToSave.Any(p => !string.IsNullOrEmpty(p.PMSystemID));
        if (checkOneSite)
        {
            var oneSiteAttrs = await _samlRepository.GetProductSamlDetailsAsync(
                userPersonaId, (int)ProductEnum.OneSite, ct);

            var oneSiteId = oneSiteAttrs
                .FirstOrDefault(a => a.Name.ToUpper() == "USERID")?.Value;

            if (!string.IsNullOrEmpty(oneSiteId))
            {
                var osUser = await _manageOneSite.GetOneSiteUserInfoAsync(oneSiteId, ct);
                var osPropsResponse = await _manageOneSite.GetPropertiesAsync(editorPersonaId, userPersonaId, null!, ct);
                var osPropertyList  = osPropsResponse.Records.Cast<ProductProperty>().ToList();

                bool? isLeasingAgent      = null;
                bool  leasingAgentChecked = false;

                foreach (var p in propertyListToSave)
                {
                    if (string.IsNullOrEmpty(p.PMSystemID)) continue;
                    if (!osPropertyList.Any(a => a.ID == p.PMSystemID)) continue;

                    if (!leasingAgentChecked)
                    {
                        isLeasingAgent      = await _manageOneSite.UserInLeasingAgentListAsync(oneSiteId, Convert.ToInt32(p.PMSystemID), ct);
                        leasingAgentChecked = true;
                    }

                    if (isLeasingAgent == true && osUser is not null)
                    {
                        p.PMUserId   = osUser.UserId.ToString();
                        p.PMUserName = osUser.SystemIdentifier.Split('|').ElementAtOrDefault(1);
                    }
                    else
                    {
                        p.PMSystemID = null;
                    }
                }
            }
        }

        return propertyListToSave;
    }

    /// <summary>
    /// Builds the list of <see cref="Permission"/> objects (property × role cross-product)
    /// for the user. Super-users get every admin right across all properties.
    /// </summary>
    private Task<List<Permission>> BuildPermissionsListAsync(
        List<Property> propertyListToSave,
        List<string> roleList,
        bool isSuperUser,
        RoleInfo roleInfo,
        CancellationToken ct)
    {
        var permissionListToSave = new List<Permission>();

        if (!isSuperUser)
        {
            foreach (var p in propertyListToSave)
            foreach (var roleId in roleList)
                permissionListToSave.Add(new Permission
                {
                    PropertyId = p.PropertyId,
                    UserRoleId = Convert.ToInt32(roleId)
                });
        }
        else if (roleInfo.Roles is not null)
        {
            foreach (var p in propertyListToSave)
            foreach (var role in roleInfo.Roles)
                if (AdminRights.Contains(role.UserRoleName))
                    permissionListToSave.Add(new Permission
                    {
                        PropertyId = p.PropertyId,
                        UserRoleId = role.UserRoleId
                    });
        }

        return Task.FromResult(permissionListToSave);
    }

    /// <summary>Creates a new user in the L2L API and sets SAML attributes.</summary>
    private async Task<string> CreateL2LUserAsync(
        Lead2LeaseUser l2LUser,
        Lead2LeaseUser userBeforeUpdate,
        Persona userPersona,
        UserLoginOnly userLogin,
        RoleInfo roleInfo,
        IList<Property> allProps,
        long editorPersonaId, long userPersonaId,
        L2LApiSettings settings,
        List<AdditionalParameters> activityLog,
        CancellationToken ct)
    {
        await _productSettingService.UpdateProductStatusAsync(
            userPersonaId, ProductStatusSettingType, ProductId,
            (int)ProductBatchStatusType.Running, ct);

        // Remove non-alphanumeric chars from org name for the URL
        var orgName  = new string(userPersona.Organization.Name.Where(char.IsLetterOrDigit).ToArray());
        var url      = $"{settings.ApiEndpoint}/Users/{orgName}";
        var response = await PostApiAsync(url, l2LUser, ct);

        _logger.LogDebug("{ActionName} - {State}", "CreateL2LUserAsync",
            $"POST {(int)response.StatusCode} url={url} userPersonaId={userPersonaId}");

        if (!response.IsSuccessStatusCode)
        {
            await _productSettingService.UpdateProductStatusAsync(
                userPersonaId, ProductStatusSettingType, ProductId,
                (int)ProductBatchStatusType.Error, ct);
            _logger.LogError("{ActionName} - {State}", "CreateL2LUserAsync",
                $"Create user failed status={(int)response.StatusCode} userPersonaId={userPersonaId}");
            return "Error";
        }

        var json       = await response.Content.ReadAsStringAsync(ct);
        var userResult = System.Text.Json.JsonSerializer.Deserialize<Lead2LeaseUser>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (userResult is not null)
        {
            await _samlRepository.CreateSamlUserAttributeAsync(
                userPersonaId, ProductId, SamlAttributeEnum.productUsername, userResult.UserName, ct);
            await _samlRepository.CreateSamlUserAttributeAsync(
                userPersonaId, ProductId, SamlAttributeEnum.UserId, userResult.UserId.ToString(), ct);

            _logger.LogDebug("{ActionName} - {State}", "CreateL2LUserAsync",
                $"Created UserName={userResult.UserName} UserId={userResult.UserId}");

            activityLog.AddRange(ExtractActivityDetailLogs(userBeforeUpdate, l2LUser, roleInfo, allProps));

            // Update migration status
            var migrateSettings = await GetApiSettingsAsync(ct);
            var updateResponse  = await UpdateUsersMigrationStatusAsync(editorPersonaId,
                [new MigrateUser
                {
                    UnifiedLoginUserName = userLogin.LoginName,
                    UserId               = userResult.UserId.ToString(),
                    UsingUnifiedLogin    = true
                }], ct);

            if (!updateResponse.Status)
                return updateResponse.Message;
        }

        return string.Empty;
    }

    /// <summary>Updates an existing L2L user via the API.</summary>
    private async Task<string> UpdateL2LUserAsync(
        Lead2LeaseUser l2LUser,
        Lead2LeaseUser userBeforeUpdate,
        RoleInfo roleInfo,
        IList<Property> allProps,
        long editorPersonaId, long userPersonaId,
        L2LApiSettings settings,
        List<AdditionalParameters> activityLog,
        CancellationToken ct)
    {
        var url      = $"{settings.ApiEndpoint}/Users/edit";
        var response = await PutApiAsync(url, l2LUser, ct);

        _logger.LogDebug("{ActionName} - {State}", "UpdateL2LUserAsync",
            $"PUT {(int)response.StatusCode} url={url} userPersonaId={userPersonaId}");

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("{ActionName} - {State}", "UpdateL2LUserAsync",
                $"Update failed status={(int)response.StatusCode} userPersonaId={userPersonaId}");
            return "Error";
        }

        activityLog.AddRange(ExtractActivityDetailLogs(userBeforeUpdate, l2LUser, roleInfo, allProps));
        return string.Empty;
    }

    #endregion

    #region Deactivated batch data

    private async Task<RolePropertyList?> GetDeactivatedBatchDataAsync(long userPersonaId, CancellationToken ct)
    {
        var cacheKey = $"l2l_deactivated_batch_{userPersonaId}";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(DeactivatedBatchCacheSeconds);

            var settings = await _productRepository.GetProductSettingsByPersonaAsync(userPersonaId, ct);
            bool isDeactivated = settings.Any(s =>
                s.ProductId == ProductId &&
                s.Value     == Convert.ToString((int)UserUiStatusType.Deactivated));

            return isDeactivated
                ? await _productRepository.GetUserProductDataFromProductBatchAsync(userPersonaId, ProductId, ct)
                : null;
        });
    }

    #endregion

    #region Email helpers

    private async Task<string> GetEmailAddressAsync(
        Guid realPageId, string loginName, CancellationToken ct)
    {
        var addresses = await _manageElectronicAddress.ListElectronicAddressForPersonAsync(realPageId, string.Empty, ct);
        if (addresses?.Any(a =>
            a.AddressType?.ToUpper() == "EMAIL" &&
            a.contactMechanismUsageType?.Name?.ToUpper() == "PRIMARY") == true)
        {
            return addresses
                .Where(a => a.AddressType!.ToUpper() == "EMAIL" &&
                            a.contactMechanismUsageType!.Name.ToUpper() == "PRIMARY")
                .Select(a => a.AddressString)
                .FirstOrDefault() ?? loginName;
        }
        return loginName;
    }

    /// <summary>
    /// Ensures the email address looks valid — appends domain parts if missing.
    /// Pure static port of <c>ManageProductBase.ValidateAndReturnEmailAddress</c>.
    /// </summary>
    private static string ValidateAndReturnEmailAddress(string emailAddress)
    {
        if (new EmailAddressAttribute().IsValid(emailAddress))
            return emailAddress;

        try
        {
            var ma = new MailAddress(emailAddress);
            if (!ma.Host.Contains('.'))
                return ValidateAndReturnEmailAddress(emailAddress + ".com");
        }
        catch
        {
            if (!emailAddress.Contains('@'))
                return ValidateAndReturnEmailAddress(emailAddress + "@bogusemail.com");
        }

        return emailAddress;
    }

    #endregion

    #region SAML update helper

    private async Task UpdateSamlUsernameAsync(long userPersonaId, string newUsername, CancellationToken ct)
    {
        var attrs = await _samlRepository.GetProductSamlDetailsAsync(userPersonaId, ProductId, ct);
        var existing = attrs.FirstOrDefault(a => a.Name.ToUpper() == "PRODUCTUSERNAME");
        if (existing is null) return;

        await _samlRepository.UpdateSamlUserAttributeAsync(
            new SamlAttributes
            {
                SamlAttributeId     = (int)SamlAttributeEnum.productUsername,
                Value               = newUsername,
                SamlUserAttributeId = existing.SamlUserAttributeId
            }, ct);
    }

    #endregion

    #region Activity log

    /// <summary>
    /// Builds the diff activity-log entries comparing the user state before and after update.
    /// Static pure function — no I/O.
    /// </summary>
    private static List<AdditionalParameters> ExtractActivityDetailLogs(
        Lead2LeaseUser userBeforeUpdate,
        Lead2LeaseUser l2LUser,
        RoleInfo roleInfo,
        IList<Property> allProps)
    {
        var result = new List<AdditionalParameters>();
        try
        {
            userBeforeUpdate.Permissions ??= [];
            userBeforeUpdate.Properties  ??= [];

            var oldRoleIds = userBeforeUpdate.Permissions.Select(p => p.UserRoleId).ToHashSet();
            var newRoleIds = l2LUser.Permissions?.Select(p => p.UserRoleId).ToHashSet() ?? [];

            if (roleInfo.Roles is not null)
            {
                foreach (var id in newRoleIds.Except(oldRoleIds))
                {
                    var name = roleInfo.Roles.FirstOrDefault(r => r.UserRoleId == id)?.UserRoleName ?? id.ToString();
                    result.Add(new AdditionalParameters
                    {
                        Key   = "Lead2Lease Rights",
                        Value = RolesAssignedMsg.Replace("RoleName", name)
                    });
                }
                foreach (var id in oldRoleIds.Except(newRoleIds))
                {
                    var name = roleInfo.Roles.FirstOrDefault(r => r.UserRoleId == id)?.UserRoleName ?? id.ToString();
                    result.Add(new AdditionalParameters
                    {
                        Key   = "Lead2Lease Rights",
                        Value = RolesRemovedMsg.Replace("RoleName", name)
                    });
                }
            }

            var oldPropIds = userBeforeUpdate.Properties.Select(p => p.PropertyId).ToHashSet();
            var newPropIds = l2LUser.Properties?.Select(p => p.PropertyId).ToHashSet() ?? [];

            if (allProps is not null)
            {
                foreach (var id in newPropIds.Except(oldPropIds))
                {
                    var name = allProps.FirstOrDefault(p => p.PropertyId == id)?.ComplexName ?? id.ToString();
                    result.Add(new AdditionalParameters
                    {
                        Key   = "Lead2Lease Properties",
                        Value = PropsAssignedMsg.Replace("PropertyName", name)
                    });
                }
                foreach (var id in oldPropIds.Except(newPropIds))
                {
                    var name = allProps.FirstOrDefault(p => p.PropertyId == id)?.ComplexName ?? id.ToString();
                    result.Add(new AdditionalParameters
                    {
                        Key   = "Lead2Lease Properties",
                        Value = PropsRemovedMsg.Replace("PropertyName", name)
                    });
                }
            }
        }
        catch (Exception)
        {
            // Activity log failure is non-fatal
        }
        return result;
    }

    #endregion

    // Inner type to deserialize the /Users/ActiveRoles API response
    private sealed class RoleInfo
    {
        public IList<Preset>? Presets { get; set; }
        public IList<Role>?   Roles   { get; set; }
    }
}
