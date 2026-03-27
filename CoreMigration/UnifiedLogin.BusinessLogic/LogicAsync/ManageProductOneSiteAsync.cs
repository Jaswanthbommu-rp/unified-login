using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using UnifiedLogin.BusinessLogic.CacheHelper;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.LogicAsync.Models;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Cache;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Exceptions;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.Product.OneSite;
using UnifiedLogin.SharedObjects.Saml;
using RoleType = UnifiedLogin.SharedObjects.Product.OneSite.RoleType;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// True async implementation of OneSite per-call user-management operations.
/// <para>
/// Replaces the stepping-stone <c>ManageProductOneSiteAsync</c> wrapper with fully
/// async/await calls backed by injected services. No .Result, no .Wait(), no
/// self-referencing wrapper pattern.
/// </para>
/// <para>
/// <see cref="DefaultUserClaim"/> has been removed from all public method signatures;
/// call context is resolved per-call via <see cref="IProductContextServiceAsync"/>.
/// </para>
/// </summary>
public sealed class ManageProductOneSiteAsync : IManageProductOneSiteAsync
{
    // ── Constants ────────────────────────────────────────────────────────────

    private const int    ProductId              = (int)ProductEnum.OneSite;
    private const int    PmcCacheMinutes        = 10;
    private const int    TokenCacheMinutes      = 10;
    private const int    GetUserMaxRetry        = 5;
    private const string ProductStatusSetting   = "ProductStatus";

    // Product internal setting names
    private const string SettingApiEndpoint     = "APIENDPOINT";
    private const string SettingApiUsername     = "APIUSERNAME";
    private const string SettingApiPassword     = "APIPASSWORD";
    private const string SettingMtApiEndpoint   = "MTAPIENDPOINT";
    private const string SettingMtTokenEndpoint = "MTTOKENENDPOINT";
    private const string SettingMtClientId      = "MTCLIENTID";
    private const string SettingMtClientSecret  = "MTCLIENTSECRET";

    // Audit message templates
    private const string PropAssignMsg  = "User was assigned to PropertyName in OneSite.";
    private const string PropRemovedMsg = "User was removed from PropertyName in OneSite.";
    private const string RoleAssignMsg  = "User was assigned to RoleName in OneSite.";
    private const string RoleRemovedMsg = "User was removed from RoleName in OneSite.";

    // ── Injected services ────────────────────────────────────────────────────

    private readonly IProductContextServiceAsync        _contextService;
    private readonly IProductSettingServiceAsync        _settingService;
    private readonly IManagePersonaAsync                _managePersona;
    private readonly IManagePersonAsync                 _managePerson;
    private readonly IManageUserLoginAsync              _manageUserLogin;
    private readonly IManagePartyRelationshipAsync      _managePartyRelationship;
    private readonly IManageBlueBookAsync               _blueBook;
    private readonly ISamlRepositoryAsync               _samlRepository;
    private readonly IUserLoginPersonaRepositoryAsync   _userLoginPersonaRepository;
    private readonly IUserRepositoryAsync               _userRepository;
    private readonly IOneSiteProductService             _service;
    private readonly IHttpClientFactory                 _httpClientFactory;
    private readonly ICacheService                      _cache;
    private readonly ILogger<ManageProductOneSiteAsync> _logger;

    // ── Constructor ──────────────────────────────────────────────────────────

    public ManageProductOneSiteAsync(
        IProductContextServiceAsync        contextService,
        IProductSettingServiceAsync        settingService,
        IManagePersonaAsync                managePersona,
        IManagePersonAsync                 managePerson,
        IManageUserLoginAsync              manageUserLogin,
        IManagePartyRelationshipAsync      managePartyRelationship,
        IManageBlueBookAsync               blueBook,
        ISamlRepositoryAsync               samlRepository,
        IUserLoginPersonaRepositoryAsync   userLoginPersonaRepository,
        IUserRepositoryAsync               userRepository,
        IOneSiteProductService             service,
        IHttpClientFactory                 httpClientFactory,
        ICacheService                      cache,
        ILogger<ManageProductOneSiteAsync> logger)
    {
        ArgumentNullException.ThrowIfNull(contextService);             _contextService             = contextService;
        ArgumentNullException.ThrowIfNull(settingService);             _settingService             = settingService;
        ArgumentNullException.ThrowIfNull(managePersona);              _managePersona              = managePersona;
        ArgumentNullException.ThrowIfNull(managePerson);               _managePerson               = managePerson;
        ArgumentNullException.ThrowIfNull(manageUserLogin);            _manageUserLogin            = manageUserLogin;
        ArgumentNullException.ThrowIfNull(managePartyRelationship);    _managePartyRelationship    = managePartyRelationship;
        ArgumentNullException.ThrowIfNull(blueBook);                   _blueBook                   = blueBook;
        ArgumentNullException.ThrowIfNull(samlRepository);             _samlRepository             = samlRepository;
        ArgumentNullException.ThrowIfNull(userLoginPersonaRepository); _userLoginPersonaRepository = userLoginPersonaRepository;
        ArgumentNullException.ThrowIfNull(userRepository);             _userRepository             = userRepository;
        ArgumentNullException.ThrowIfNull(service);                    _service                    = service;
        ArgumentNullException.ThrowIfNull(httpClientFactory);          _httpClientFactory          = httpClientFactory;
        ArgumentNullException.ThrowIfNull(cache);                      _cache                      = cache;
        ArgumentNullException.ThrowIfNull(logger);                     _logger                     = logger;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Public API  (IManageProductOneSiteAsync)
    // ═══════════════════════════════════════════════════════════════════════════

    // ── GetRolesAsync ──────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetRolesAsync(
        long editorPersonaId, long userPersonaId,
        RequestParameter datafilter, CancellationToken ct = default)
    {
        var (ctx, ctxError) = await _contextService.GetUserContextAsync(
            editorPersonaId, userPersonaId, ProductId, ct);
        if (ctxError is not null) return ctxError;

        var settings = await _settingService.GetProductSettingAsync(ProductId, ct);
        ConfigureService(settings);

        FilterSortParameters wsParams = OneSiteHelpers.GenerateSearchAndPaging(
            datafilter, "RoleName", 0, 9999, false);

        RoleList? roleList = null;
        try
        {
            string systemIdentifier = ctx!.ProductUserId;

            if (!string.IsNullOrEmpty(systemIdentifier))
            {
                // User already has an OneSite account — fetch their specific role list
                var onesiteUser = await GetOneSiteUserInfoAsync(systemIdentifier, ct);
                if (string.IsNullOrEmpty(onesiteUser?.SystemIdentifier))
                    throw new Exception("Unable to locate user info");

                _logger.LogDebug("GetRolesAsync - GetUserRolesAsync systemIdentifier={SystemIdentifier}", systemIdentifier);
                roleList = await _service.GetUserRolesAsync(systemIdentifier, false, wsParams);
            }
            else
            {
                // User has no OneSite login yet — return full company role list with IsAssigned=false
                return await GetRoleListAllInternalAsync(editorPersonaId, ctx!, datafilter, ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetRolesAsync - error editorPersonaId={EditorPersonaId}", editorPersonaId);
            string reason = ex is BlueBookException ? ex.Message : CommonMessageConstants.RoleErrorMessage;
            return new ListResponse { IsError = true, ErrorReason = reason };
        }

        roleList ??= new RoleList();
        IList<ProductRole> list = roleList.ToGBRoles() ?? new List<ProductRole>();
        return new ListResponse
        {
            Records     = list.Cast<object>().ToList(),
            TotalRows   = list.Count,
            RowsPerPage = 9999,
            ErrorReason = string.Empty,
            TotalPages  = 1
        };
    }

    // ── GetPropertiesAsync ─────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetPropertiesAsync(
        long editorPersonaId, long userPersonaId,
        RequestParameter datafilter, CancellationToken ct = default)
    {
        var (ctx, ctxError) = await _contextService.GetUserContextAsync(
            editorPersonaId, userPersonaId, ProductId, ct);
        if (ctxError is not null) return ctxError;

        var settings = await _settingService.GetProductSettingAsync(ProductId, ct);
        ConfigureService(settings);

        try
        {
            // Which persona drives the PMCID lookup
            Persona targetPersona = (userPersonaId != 0 ? ctx!.UserPersona : null)
                                    ?? ctx!.EditorPersona;

            string pmcId = await GetOneSitePmcIdFromPersonaAsync(targetPersona, ct);

            FilterSortParameters wsParams = OneSiteHelpers.GenerateSearchAndPaging(
                datafilter, "SiteName", 0, 9999);

            string        systemIdentifier = ctx!.ProductUserId;
            PropertyList? propertyList;
            OneSiteUser?  onesiteUser = null;

            if (!string.IsNullOrEmpty(systemIdentifier))
            {
                onesiteUser = await GetOneSiteUserInfoAsync(systemIdentifier, ct);
                if (string.IsNullOrEmpty(onesiteUser?.SystemIdentifier))
                    throw new Exception("Unable to locate user info");

                _logger.LogDebug("GetPropertiesAsync - GetUserPropertiesAsync systemIdentifier={SystemIdentifier}", systemIdentifier);
                propertyList = await _service.GetUserPropertiesAsync(systemIdentifier, false, wsParams);
            }
            else
            {
                var uiArgs = ToNameValuePairs(new Dictionary<string, string> { { "PMCID", pmcId } });
                _logger.LogDebug("GetPropertiesAsync - GetAllPropertiesAsync pmcId={PmcId}", pmcId);
                propertyList = await _service.GetAllPropertiesAsync(uiArgs, systemIdentifier, wsParams);
            }

            propertyList ??= new PropertyList();
            IList<ProductProperty> list = propertyList.ToGBProperties() ?? new List<ProductProperty>();

            return new ListResponse
            {
                Records     = list.Cast<object>().ToList(),
                TotalRows   = propertyList.TotalProperties,
                RowsPerPage = datafilter is null ? list.Count : datafilter.Pages.ResultsPerPage,
                TotalPages  = 1,
                ErrorReason = string.Empty,
                Additional  = new Dictionary<string, bool> { { "allProperties", onesiteUser?.AllProperties ?? false } }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetPropertiesAsync - error editorPersonaId={EditorPersonaId}", editorPersonaId);
            string reason = ex is BlueBookException ? ex.Message : CommonMessageConstants.PropertyErrorMessage;
            return new ListResponse { IsError = true, ErrorReason = reason };
        }
    }

    // ── GetRegionsAsync ────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetRegionsAsync(
        long editorPersonaId, long userPersonaId,
        RequestParameter datafilter, CancellationToken ct = default)
    {
        // "Regions" maps to the full company role list scoped to the editor's PMC
        var (ctx, ctxError) = await _contextService.GetUserContextAsync(
            editorPersonaId, userPersonaId, ProductId, ct);
        if (ctxError is not null) return ctxError;

        var settings = await _settingService.GetProductSettingAsync(ProductId, ct);
        ConfigureService(settings);

        return await GetRoleListAllInternalAsync(editorPersonaId, ctx!, datafilter, ct);
    }

    // ── GetMigrationUsersAsync ─────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetMigrationUsersAsync(
        long editorPersonaId, RequestParameter datafilter, CancellationToken ct = default)
    {
        var response = new ListResponse { IsError = true, ErrorReason = "No Users." };

        var (ctx, ctxError) = await _contextService.GetUserContextAsync(
            editorPersonaId, 0, ProductId, ct);
        if (ctxError is not null) { response.ErrorReason = ctxError.ErrorReason; return response; }

        var settings = await _settingService.GetProductSettingAsync(ProductId, ct);
        ConfigureService(settings);

        string pmcId = await GetOneSitePmcIdFromPersonaAsync(ctx!.EditorPersona, ct);
        if (!int.TryParse(pmcId, out int companyInstanceSourceId) || companyInstanceSourceId == 0)
        {
            _logger.LogError("GetMigrationUsersAsync - could not resolve PMC ID for editorPersonaId={EditorPersonaId}", editorPersonaId);
            response.ErrorReason = "Company Setup Error: Please Contact Support.";
            return response;
        }

        string filter        = "NonMigrated";
        int    startRow      = 0;
        int    resultPerPage = 1000;
        if (datafilter != null)
        {
            if (datafilter.FilterBy?.ContainsKey("filter") == true)
                filter = datafilter.FilterBy["filter"];
            if (datafilter.Pages != null)
            {
                startRow      = datafilter.Pages.StartRow;
                resultPerPage = datafilter.Pages.ResultsPerPage;
            }
        }

        var pmcInfo = await GetPmcInfoAsync(companyInstanceSourceId, ct);
        if (pmcInfo == null || pmcInfo.ID != companyInstanceSourceId)
        {
            response.ErrorReason = $"Could not get PMC Info for company Instance Source id - {companyInstanceSourceId}.";
            _logger.LogError("GetMigrationUsersAsync - {ErrorReason}", response.ErrorReason);
            return response;
        }

        string mtApiEndpoint = GetSettingValue(settings, SettingMtApiEndpoint);
        string url = $"https://{pmcInfo.PMCURL}/{mtApiEndpoint}/{companyInstanceSourceId}/users"
                   + $"?filter={filter}&startRow={startRow}&resultsPerPage={resultPerPage}";

        string accessToken = await GetMtTokenAsync(pmcInfo, settings, ct);
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            response.ErrorReason = $"Could not get access token from PMC for company Instance Source id - {companyInstanceSourceId}.";
            _logger.LogError("GetMigrationUsersAsync - {ErrorReason}", response.ErrorReason);
            return response;
        }

        _logger.LogDebug("GetMigrationUsersAsync - GET {Url}", url);
        var allUsers = await GetResultFromApiAsync<IList<OneSiteMigrateUser>>(accessToken, url, ct);

        if (allUsers == null)
        {
            _logger.LogError("GetMigrationUsersAsync - no users received for editorPersonaId={EditorPersonaId}", editorPersonaId);
            return response;
        }

        foreach (var user in allUsers)
        {
            user.CompanyInstanceSourceId = companyInstanceSourceId.ToString();
            user.EmployeeId              = user.ReferenceNumber;
        }

        response.RowsPerPage = resultPerPage;
        response.ErrorReason = string.Empty;
        response.IsError     = false;
        response.TotalPages  = 1;
        response.Records     = allUsers.Cast<object>().ToList();
        response.TotalRows   = allUsers.Count;
        return response;
    }

    // ── UpdateUsersMigrationStatusAsync ───────────────────────────────────────

    /// <inheritdoc/>
    public async Task<MigrateResponse> UpdateUsersMigrationStatusAsync(
        long editorPersonaId, IList<MigrateUser> migrateUsers, CancellationToken ct = default)
    {
        var migrateResponse = new MigrateResponse { Status = false };

        var (ctx, ctxError) = await _contextService.GetUserContextAsync(
            editorPersonaId, 0, ProductId, ct);
        if (ctxError is not null) { migrateResponse.Message = ctxError.ErrorReason; return migrateResponse; }

        var settings = await _settingService.GetProductSettingAsync(ProductId, ct);
        ConfigureService(settings);

        string pmcId = await GetOneSitePmcIdFromPersonaAsync(ctx!.EditorPersona, ct);
        if (!int.TryParse(pmcId, out int companyInstanceSourceId) || companyInstanceSourceId == 0)
        {
            _logger.LogError("UpdateUsersMigrationStatusAsync - could not resolve PMC ID for editorPersonaId={EditorPersonaId}", editorPersonaId);
            migrateResponse.Message = "Company Setup Error: Please Contact Support.";
            return migrateResponse;
        }

        var pmcInfo = await GetPmcInfoAsync(companyInstanceSourceId, ct);
        if (pmcInfo == null || pmcInfo.ID != companyInstanceSourceId)
        {
            migrateResponse.Message = $"Could not get PMC Info for company Instance Source id - {companyInstanceSourceId}.";
            _logger.LogError("UpdateUsersMigrationStatusAsync - {Message}", migrateResponse.Message);
            return migrateResponse;
        }

        string mtApiEndpoint = GetSettingValue(settings, SettingMtApiEndpoint);
        string url           = $"https://{pmcInfo.PMCURL}/{mtApiEndpoint}/{companyInstanceSourceId}/migrate-users";

        string accessToken = await GetMtTokenAsync(pmcInfo, settings, ct);
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            migrateResponse.Message = $"Could not get access token from PMC for company Instance Source id - {companyInstanceSourceId}.";
            _logger.LogError("UpdateUsersMigrationStatusAsync - {Message}", migrateResponse.Message);
            return migrateResponse;
        }

        using var client = _httpClientFactory.CreateClient("OneSiteMT");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        using var httpResponse = await client.PutAsJsonAsync(url, migrateUsers, ct);
        string responseContent = await httpResponse.Content.ReadAsStringAsync(ct);

        if (httpResponse.IsSuccessStatusCode)
        {
            _logger.LogDebug("UpdateUsersMigrationStatusAsync - success PUT {Url}", url);
            migrateResponse.Message = "Success";
            migrateResponse.Status  = true;
            return migrateResponse;
        }

        _logger.LogError("UpdateUsersMigrationStatusAsync - error PUT {Url}: {Response}", url, responseContent);
        migrateResponse.Message = "Cannot update user status to migrated.";
        return migrateResponse;
    }

    // ── ChangeUserStatusAsync ──────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<bool> ChangeUserStatusAsync(
        long editorPersonaId, string userId, CancellationToken ct = default)
    {
        var (ctx, ctxError) = await _contextService.GetUserContextAsync(
            editorPersonaId, 0, ProductId, ct);
        if (ctxError is not null) return false;

        var settings = await _settingService.GetProductSettingAsync(ProductId, ct);
        ConfigureService(settings);

        string pmcId = await GetOneSitePmcIdFromPersonaAsync(ctx!.EditorPersona, ct);
        if (!int.TryParse(pmcId, out int companyInstanceSourceId) || companyInstanceSourceId == 0)
        {
            _logger.LogError("ChangeUserStatusAsync - could not resolve PMC ID for editorPersonaId={EditorPersonaId}", editorPersonaId);
            return false;
        }

        // userId is the OneSite login name; combine with PMCID to form the system identifier
        string systemIdentifier = $"{companyInstanceSourceId}|{userId}";
        try
        {
            _logger.LogDebug("ChangeUserStatusAsync - DisableUserAsync systemIdentifier={SystemIdentifier}", systemIdentifier);
            await _service.DisableUserAsync(systemIdentifier);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "ChangeUserStatusAsync - failed for systemIdentifier={SystemIdentifier} editorPersonaId={EditorPersonaId}",
                systemIdentifier, editorPersonaId);
            return false;
        }
    }

    // ── ManageOneSiteUserAsync ─────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<(string error, List<AdditionalParameters> auditParams)> ManageOneSiteUserAsync(
        long editorPersonaId, long userPersonaId,
        List<string> roleList, List<string> propertyList,
        bool isUserProfileChanged = false,
        CancellationToken ct = default)
    {
        var additionalParameters = new List<AdditionalParameters>();

        _logger.LogDebug("ManageOneSiteUserAsync - begin editorPersonaId={EditorPersonaId} userPersonaId={UserPersonaId}",
            editorPersonaId, userPersonaId);

        var (ctx, ctxError) = await _contextService.GetUserContextAsync(
            editorPersonaId, userPersonaId, ProductId, ct);
        if (ctxError is not null) return (ctxError.ErrorReason, additionalParameters);

        var settings = await _settingService.GetProductSettingAsync(ProductId, ct);
        ConfigureService(settings);

        string pmcId            = await GetOneSitePmcIdFromPersonaAsync(ctx!.EditorPersona, ct);
        string systemIdentifier = ctx.ProductUserId;  // empty = new user
        string onesiteLoginName = string.Empty;
        string onesitePin       = "XXXX";             // XXXX = use existing pin in OneSite
        bool   existingUser     = false;

        if (string.IsNullOrEmpty(systemIdentifier))
        {
            onesitePin = new Random().Next(1, 10000).ToString("D4");
        }
        else
        {
            onesiteLoginName = systemIdentifier.Split('|')[1];
            existingUser     = true;
        }

        // Resolve user persona, person record, and login
        Persona       userPersona = await _managePersona.GetPersonaAsync(userPersonaId, cancellationToken: ct);
        Person        person      = await _managePerson.GetPersonAsync(userPersona.RealPageId, cancellationToken: ct);
        UserLoginOnly userLogin   = await _manageUserLogin.GetUserLoginOnlyAsync(userPersona.RealPageId, cancellationToken: ct);

        // Employee ID
        var ulPersonas = await _userLoginPersonaRepository.ListUserLoginPersonaAsync(
            null, userPersona.UserId, userPersona.Organization.PartyId, ct);
        var employeeIdRecord = await _userRepository.GetUserEmployeeIdAsync(
            ulPersonas[0].UserLoginPersonaId, userPersona.OrganizationPartyId, ct);
        person.EmployeeId = (employeeIdRecord != null && !string.IsNullOrEmpty(employeeIdRecord.EmployeeId))
            ? employeeIdRecord.EmployeeId
            : null;

        // SuperUser check
        bool isSuperUser = await IsSuperUserAsync(userPersona, ct);

        // Third-party reference reconciliation
        string userThirdPartyReference = person.EmployeeId ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(systemIdentifier))
        {
            var onesiteUserInfo = await GetOneSiteUserInfoAsync(systemIdentifier, ct);
            userThirdPartyReference = (person.EmployeeId != onesiteUserInfo?.UserThirdPartyReference)
                ? person.EmployeeId ?? string.Empty
                : onesiteUserInfo?.UserThirdPartyReference ?? string.Empty;
        }

        // Email address — skip loginName for no-email users
        string userEmailAddress = IsRegularUserNoEmail(userPersona) ? string.Empty : userLogin.LoginName;
        if (!string.IsNullOrWhiteSpace(userEmailAddress))
            userEmailAddress = ValidateAndReturnEmailAddress(userEmailAddress);

        if (userLogin.PartyId <= 0)
        {
            _logger.LogError("ManageOneSiteUserAsync - missing party id for userPersonaId={UserPersonaId}", userPersonaId);
            await _settingService.UpdateProductStatusAsync(
                userPersonaId, ProductStatusSetting, ProductId,
                (int)ProductBatchStatusType.Error, ct);
            return ($"Error : Missing party id for userPersonaId {userPersonaId}", additionalParameters);
        }

        // Build the NameValuePair array
        string safeFirst = new string(person.FirstName.Where(char.IsLetter).ToArray());
        string safeLast  = new string(person.LastName.Where(char.IsLetter).ToArray());

        var userArray = new List<NameValuePair>
        {
            new() { Name = "FirstName",       Value = safeFirst },
            new() { Name = "LastName",        Value = safeLast },
            new() { Name = "Pin",             Value = onesitePin },
            new() { Name = "ReferenceNumber", Value = userThirdPartyReference },
            new() { Name = "PMCID",           Value = pmcId },
            new() { Name = "IsSuperuser",     Value = isSuperUser ? "1" : "0" },
            new() { Name = "LogonName",       Value = onesiteLoginName },
            new() { Name = "IsULLinked",      Value = "1" },
            new() { Name = "EmailAddress",    Value = userEmailAddress.Contains("@bogusemail.com")
                                                          ? string.Empty
                                                          : userEmailAddress }
        };

        if (isSuperUser)
            userArray.Add(new NameValuePair { Name = "Title", Value = "SuperUser" });

        NameValuePair[] serviceResponse;
        string errorMessage = string.Empty;

        try
        {
            if (string.IsNullOrEmpty(systemIdentifier))
            {
                // ── Create new user ─────────────────────────────────────────────────
                await _settingService.UpdateProductStatusAsync(
                    userPersonaId, ProductStatusSetting, ProductId,
                    (int)ProductBatchStatusType.Running, ct);

                _logger.LogDebug("ManageOneSiteUserAsync - creating {UserType} user",
                    isSuperUser ? "superuser" : "regular user");

                // The NameValuePair[] overloads have no async counterpart on the SOAP proxy.
                // Offloaded to thread-pool to avoid blocking the async pipeline.
                if (isSuperUser)
                    serviceResponse = await Task.Run(() => _service.CreateSuperuser(userArray.ToArray()), ct);
                else
                    serviceResponse = await Task.Run(() => _service.CreateUser(userArray.ToArray()), ct);

                // Persist SAML attributes
                await _samlRepository.CreateSamlUserAttributeAsync(
                    userPersonaId, ProductId, SamlAttributeEnum.PMCID, pmcId, ct);

                foreach (var nvp in serviceResponse)
                {
                    if (nvp.Name.Equals("SYSTEMIDENTIFIER", StringComparison.OrdinalIgnoreCase))
                    {
                        string pmcUserLogin = nvp.Value;
                        await _samlRepository.CreateSamlUserAttributeAsync(
                            userPersonaId, ProductId, SamlAttributeEnum.UserId, pmcUserLogin, ct);
                        await _samlRepository.CreateSamlUserAttributeAsync(
                            userPersonaId, ProductId, SamlAttributeEnum.productUsername,
                            pmcUserLogin.Split('|')[1], ct);
                        systemIdentifier = pmcUserLogin;
                        break;
                    }
                }
            }
            else
            {
                // ── Update existing user ─────────────────────────────────────────────
                _logger.LogDebug("ManageOneSiteUserAsync - updating {UserType} user systemIdentifier={SystemIdentifier}",
                    isSuperUser ? "superuser" : "regular user", systemIdentifier);

                // Same: NameValuePair[] overloads have no async counterpart on the SOAP proxy.
                if (isSuperUser)
                    serviceResponse = await Task.Run(
                        () => _service.UpdateSuperuser(systemIdentifier, userArray.ToArray()), ct);
                else
                    serviceResponse = await Task.Run(
                        () => _service.UpdateUser(systemIdentifier, userArray.ToArray()), ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ManageOneSiteUserAsync - SOAP error for userPersonaId={UserPersonaId}", userPersonaId);
            if (string.IsNullOrEmpty(systemIdentifier))
            {
                await _settingService.UpdateProductStatusAsync(
                    userPersonaId, ProductStatusSetting, ProductId,
                    (int)ProductBatchStatusType.Error, ct);
            }
            return ("Error : " + ex.Message, additionalParameters);
        }

        // Parse the response envelope
        bool hasError = false;
        foreach (var nvp in serviceResponse)
        {
            switch (nvp.Name.ToUpperInvariant())
            {
                case "ISSUCCESSFUL":
                    if (nvp.Value == "0") hasError = true;
                    break;
                case "SYSTEMIDENTIFIER":
                    systemIdentifier = nvp.Value;
                    break;
                case "ERRORMESSAGE":
                    errorMessage = nvp.Value;
                    break;
            }
        }

        if (hasError)
        {
            _logger.LogError("ManageOneSiteUserAsync - OneSite returned error: {ErrorMessage}", errorMessage);
            await _settingService.UpdateProductStatusAsync(
                userPersonaId, ProductStatusSetting, ProductId,
                (int)ProductBatchStatusType.Error, ct);
            return (errorMessage, additionalParameters);
        }

        // Ensure existing user is active
        if (existingUser)
        {
            try
            {
                await _service.EnableUserAsync(systemIdentifier);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "ManageOneSiteUserAsync - EnableUserAsync warning for systemIdentifier={SystemIdentifier}",
                    systemIdentifier);
            }
        }

        roleList     ??= new List<string>();
        propertyList ??= new List<string>();

        await _settingService.UpdateProductStatusAsync(
            userPersonaId, ProductStatusSetting, ProductId,
            (int)ProductBatchStatusType.Success, ct);

        _logger.LogDebug("ManageOneSiteUserAsync - updating roles and properties");

        if ((roleList.Count > 0 || isSuperUser) && !isUserProfileChanged)
        {
            var (_, roleAudit) = await UpdateRolesForUserInternalAsync(
                systemIdentifier, pmcId, isSuperUser, roleList, ct);
            additionalParameters.AddRange(roleAudit);
        }

        if (propertyList.Count > 0 && !isUserProfileChanged)
        {
            var (_, propAudit) = await UpdatePropertiesForUserInternalAsync(
                systemIdentifier, pmcId, propertyList, ct);
            additionalParameters.AddRange(propAudit);
        }

        _logger.LogDebug("ManageOneSiteUserAsync - complete userPersonaId={UserPersonaId}", userPersonaId);
        return (string.Empty, additionalParameters);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Private helpers
    // ═══════════════════════════════════════════════════════════════════════════

    // ── Service configuration ──────────────────────────────────────────────────

    /// <summary>
    /// Applies URL and credentials from resolved settings to the SOAP service proxy.
    /// Called once per public method after settings have been fetched.
    /// </summary>
    private void ConfigureService(List<ProductInternalSetting> settings)
    {
        string url             = GetSettingValue(settings, SettingApiEndpoint);
        string encodedUsername = GetSettingValue(settings, SettingApiUsername);
        string encodedPassword = GetSettingValue(settings, SettingApiPassword);

        if (!string.IsNullOrWhiteSpace(url))
            _service.Url = url;

        if (!string.IsNullOrWhiteSpace(encodedUsername) && !string.IsNullOrWhiteSpace(encodedPassword))
        {
            string username = Encoding.UTF8.GetString(Convert.FromBase64String(encodedUsername));
            string password = Encoding.UTF8.GetString(Convert.FromBase64String(encodedPassword));
            _service.PreAuthenticate = true;
            _service.Credentials    = new System.Net.NetworkCredential(username, password);
        }
    }

    private static string GetSettingValue(List<ProductInternalSetting> settings, string name)
        => settings.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase))?.Value
           ?? string.Empty;

    // ── Role list helpers ──────────────────────────────────────────────────────

    /// <summary>
    /// Returns the full company role list (all roles, IsAssigned=false) scoped to
    /// the editor's PMCID. Used by GetRolesAsync (new user path) and GetRegionsAsync.
    /// </summary>
    private async Task<ListResponse> GetRoleListAllInternalAsync(
        long editorPersonaId,
        ProductCallContext ctx,
        RequestParameter? datafilter,
        CancellationToken ct)
    {
        try
        {
            string pmcId = await GetOneSitePmcIdFromPersonaAsync(ctx.EditorPersona, ct);
            if (string.IsNullOrWhiteSpace(pmcId))
                throw new BlueBookException(CommonMessageConstants.CompanyErrorMessage);

            var args     = new Dictionary<string, string> { { "PMCID", pmcId } };
            var roleList = await GetOneSiteRoleListMainAsync(args, datafilter, ctx.EditorProductUserId, ct);

            IList<ProductRole> list = roleList.ToGBRoles() ?? new List<ProductRole>();
            foreach (var pr in list)
                pr.IsAssigned = false;

            return new ListResponse
            {
                Records     = list.Cast<object>().ToList(),
                TotalRows   = list.Count,
                RowsPerPage = 9999,
                ErrorReason = string.Empty,
                TotalPages  = 1
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetRoleListAllInternalAsync - error editorPersonaId={EditorPersonaId}", editorPersonaId);
            string reason = ex is BlueBookException ? ex.Message : CommonMessageConstants.RoleErrorMessage;
            return new ListResponse { IsError = true, ErrorReason = reason };
        }
    }

    private async Task<RoleList> GetOneSiteRoleListMainAsync(
        Dictionary<string, string> args,
        RequestParameter? datafilter,
        string systemIdentifier,
        CancellationToken ct)
    {
        try
        {
            FilterSortParameters wsParams = OneSiteHelpers.GenerateSearchAndPaging(
                datafilter, "RoleName", 0, 9999);
            return await _service.GetAllRolesAsync(ToNameValuePairs(args), systemIdentifier, wsParams);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetOneSiteRoleListMainAsync - error");
            return new RoleList();
        }
    }

    // ── Property list helpers ──────────────────────────────────────────────────

    private async Task<PropertyList> GetOneSitePropertyListMainAsync(
        Dictionary<string, string> args,
        RequestParameter? datafilter,
        string systemIdentifier,
        CancellationToken ct)
    {
        try
        {
            FilterSortParameters wsParams = OneSiteHelpers.GenerateSearchAndPaging(
                datafilter, "SiteName", 0, 9999);
            return await _service.GetAllPropertiesAsync(ToNameValuePairs(args), systemIdentifier, wsParams);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetOneSitePropertyListMainAsync - error");
            return new PropertyList();
        }
    }

    // ── Roles update (internal) ────────────────────────────────────────────────

    private async Task<(string resultCount, List<AdditionalParameters> auditParams)>
        UpdateRolesForUserInternalAsync(
            string systemIdentifier,
            string pmcId,
            bool isSuperUser,
            List<string> rolesToAssign,
            CancellationToken ct)
    {
        var additionalParameters = new List<AdditionalParameters>();
        var rolesToRemove        = new List<string>();
        string roleIdAddList     = string.Empty;
        string roleIdRemoveList  = string.Empty;

        var args    = new Dictionary<string, string> { { "PMCID", pmcId } };
        var df      = new RequestParameter { Pages = new PageRequest { ResultsPerPage = 9999 } };
        var current = await GetOneSiteRoleListMainAsync(args, df, systemIdentifier, ct);

        foreach (RoleType role in current.Role ?? Array.Empty<RoleType>())
        {
            if (role.IsInternal && !OneSiteHelpers.IsValidRoleForCustomer(role, false))
            {
                if (rolesToAssign.Contains(role.RoleID) && role.IsAssigned)
                    rolesToAssign.Remove(role.RoleID);
                continue;
            }

            if (!rolesToAssign.Contains(role.RoleID))
            {
                if (isSuperUser && role.RoleName.Equals("E-DOC SIGNER", StringComparison.OrdinalIgnoreCase))
                {
                    if (!role.IsAssigned) rolesToAssign.Add(role.RoleID);
                }
                else if (role.IsAssigned)
                {
                    rolesToRemove.Add(role.RoleID);
                }
            }

            if (rolesToAssign.Contains(role.RoleID) && role.IsAssigned)
                rolesToAssign.Remove(role.RoleID);
        }

        if (rolesToAssign.Count > 0)
        {
            additionalParameters.AddRange(
                current.Role
                    .Where(r => rolesToAssign.Contains(r.RoleID))
                    .Select(r => new AdditionalParameters
                    {
                        Key   = "OneSite Roles",
                        Value = RoleAssignMsg.Replace("RoleName", r.RoleName)
                    }));
            roleIdAddList = string.Join("|", rolesToAssign);
        }

        if (rolesToRemove.Count > 0)
        {
            additionalParameters.AddRange(
                current.Role
                    .Where(r => rolesToRemove.Contains(r.RoleID))
                    .Select(r => new AdditionalParameters
                    {
                        Key   = "OneSite Roles",
                        Value = RoleRemovedMsg.Replace("RoleName", r.RoleName)
                    }));
            roleIdRemoveList = string.Join("|", rolesToRemove);
        }

        if (!string.IsNullOrWhiteSpace(roleIdRemoveList))
        {
            try   { await _service.RemoveRolesFromUserAsync(systemIdentifier, roleIdRemoveList); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateRolesForUserInternalAsync - error removing roles");
            }
        }

        if (!string.IsNullOrWhiteSpace(roleIdAddList))
        {
            try   { await _service.AssignRolesToUserAsync(systemIdentifier, roleIdAddList); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateRolesForUserInternalAsync - error assigning roles");
            }
        }

        string resultCount = (rolesToAssign.Count + rolesToRemove.Count).ToString();
        return (resultCount, additionalParameters);
    }

    // ── Properties update (internal) ──────────────────────────────────────────

    private async Task<(string resultCount, List<AdditionalParameters> auditParams)>
        UpdatePropertiesForUserInternalAsync(
            string systemIdentifier,
            string pmcId,
            List<string> propertiesToAssign,
            CancellationToken ct)
    {
        var additionalParameters    = new List<AdditionalParameters>();
        var propertiesToRemove      = new List<string>();
        string propertyIdAddList    = string.Empty;
        string propertyIdRemoveList = string.Empty;
        string resultCount          = string.Empty;

        if (propertiesToAssign.Count > 0
            && !propertiesToAssign[0].Equals("ALL", StringComparison.OrdinalIgnoreCase))
        {
            var args        = new Dictionary<string, string> { { "PMCID", pmcId } };
            var df          = new RequestParameter { Pages = new PageRequest { ResultsPerPage = 9999 } };
            var current     = await GetOneSitePropertyListMainAsync(args, df, systemIdentifier, ct);
            var onesiteUser = await GetOneSiteUserInfoAsync(systemIdentifier, ct);

            if (onesiteUser?.AllProperties == true
                && current.Property?.Count() == propertiesToAssign.Count)
            {
                // All-properties shortcut (fix for GB-7138)
                additionalParameters.AddRange(
                    current.Property
                        .Where(p => propertiesToAssign.Contains(p.PropertyID))
                        .Select(p => new AdditionalParameters
                        {
                            Key   = "OneSite Properties",
                            Value = PropAssignMsg.Replace("PropertyName", p.PropertyName)
                        }));
                propertyIdAddList = string.Join("|", propertiesToAssign);
            }
            else
            {
                foreach (var prop in current.Property ?? Array.Empty<PropertyType>())
                {
                    if (!propertiesToAssign.Contains(prop.PropertyID) && prop.IsAssignedToUser)
                        propertiesToRemove.Add(prop.PropertyID);

                    if (propertiesToAssign.Contains(prop.PropertyID) && prop.IsAssignedToUser)
                        propertiesToAssign.Remove(prop.PropertyID);
                }

                if (propertiesToAssign.Count > 0)
                {
                    additionalParameters.AddRange(
                        current.Property
                            .Where(p => propertiesToAssign.Contains(p.PropertyID))
                            .Select(p => new AdditionalParameters
                            {
                                Key   = "OneSite Properties",
                                Value = PropAssignMsg.Replace("PropertyName", p.PropertyName)
                            }));
                    propertyIdAddList = string.Join("|", propertiesToAssign);
                }

                if (propertiesToRemove.Count > 0)
                {
                    additionalParameters.AddRange(
                        current.Property
                            .Where(p => propertiesToRemove.Contains(p.PropertyID))
                            .Select(p => new AdditionalParameters
                            {
                                Key   = "OneSite Properties",
                                Value = PropRemovedMsg.Replace("PropertyName", p.PropertyName)
                            }));
                    propertyIdRemoveList = string.Join("|", propertiesToRemove);
                }
            }

            resultCount = (propertiesToAssign.Count + propertiesToRemove.Count).ToString();
        }
        else
        {
            // Assign ALL properties
            propertyIdAddList = "ALL";
            resultCount       = "All";
            additionalParameters.Add(new AdditionalParameters
            {
                Key   = "OneSite Properties",
                Value = PropAssignMsg.Replace("PropertyName", "ALL")
            });
        }

        if (!string.IsNullOrWhiteSpace(propertyIdRemoveList))
        {
            try   { await _service.RemovePropertiesFromUserAsync(systemIdentifier, propertyIdRemoveList); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdatePropertiesForUserInternalAsync - error removing properties");
            }
        }

        if (!string.IsNullOrWhiteSpace(propertyIdAddList))
        {
            try   { await _service.AssignPropertiesToUserAsync(systemIdentifier, propertyIdAddList); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdatePropertiesForUserInternalAsync - error assigning properties");
            }
        }

        return (resultCount, additionalParameters);
    }

    // ── GetOneSiteUserInfo ─────────────────────────────────────────────────────

    private async Task<OneSiteUser?> GetOneSiteUserInfoAsync(
        string systemIdentifier, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(systemIdentifier))
            return null;

        string pmcId     = systemIdentifier.Split('|')[0];
        string logonName = systemIdentifier.Split('|')[1];

        var userArray = new List<NameValuePair>
        {
            new() { Name = "PMCID",     Value = pmcId },
            new() { Name = "LogonName", Value = logonName }
        };

        var osu      = new OneSiteUser();
        int tryCount = 0;

        try
        {
            while (tryCount < GetUserMaxRetry)
            {
                ct.ThrowIfCancellationRequested();

                _logger.LogDebug("GetOneSiteUserInfoAsync - attempt {Attempt} systemIdentifier={SystemIdentifier}",
                    tryCount + 1, systemIdentifier);

                // GetUser(NameValuePair[]) has no async NameValuePair overload on the SOAP proxy.
                // Offloaded to thread-pool to avoid blocking the async pipeline.
                NameValuePair[] response = await Task.Run(
                    () => _service.GetUser(userArray.ToArray()), ct);

                if (response.Length > 0
                    && response.Any(a => a.Name.Equals("USERID", StringComparison.OrdinalIgnoreCase)))
                {
                    string userId = response
                        .First(a => a.Name.Equals("USERID", StringComparison.OrdinalIgnoreCase))
                        .Value;

                    if (!userId.Equals("UNKNOWN", StringComparison.OrdinalIgnoreCase))
                    {
                        osu = ParseOneSiteGetUser(response);
                        break;
                    }
                }

                tryCount++;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetOneSiteUserInfoAsync - error systemIdentifier={SystemIdentifier}", systemIdentifier);
        }

        return osu;
    }

    private static OneSiteUser ParseOneSiteGetUser(NameValuePair[] response)
    {
        string? Get(string name) => response
            .FirstOrDefault(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase))?.Value;

        var osu = new OneSiteUser
        {
            SystemIdentifier        = Get("SYSTEMIDENTIFIER") ?? string.Empty,
            FirstName               = Get("FIRSTNAME")        ?? string.Empty,
            LastName                = Get("LASTNAME")         ?? string.Empty,
            UserThirdPartyReference = Get("USERTHIRDPARTYREFERENCE") ?? string.Empty,
            AllProperties           = Get("USERALLPROPERTY") == "1"
        };

        string? userId = Get("USERID");
        if (userId is not null && int.TryParse(userId, out int uid))
            osu.UserId = uid;

        string? pin = Get("USERPIN");
        if (pin is not null && int.TryParse(pin, out int p))
            osu.UserPin = p;

        return osu;
    }

    // ── PMCID resolution ──────────────────────────────────────────────────────

    private async Task<string> GetOneSitePmcIdFromPersonaAsync(
        Persona persona, CancellationToken ct)
    {
        _logger.LogDebug("GetOneSitePmcIdFromPersonaAsync - begin personaId={PersonaId}", persona.PersonaId);

        // 1. SAML UserId attribute contains "PMCID|loginname"
        var productAttributes = await _samlRepository.GetProductSamlDetailsAsync(
            persona.PersonaId, ProductId, ct);

        string? uniqueIdentifier = productAttributes
            .FirstOrDefault(a => a.Name.Equals("USERID", StringComparison.OrdinalIgnoreCase))?.Value;

        if (!string.IsNullOrEmpty(uniqueIdentifier))
        {
            string pmcId = uniqueIdentifier.Split('|')[0];
            _logger.LogDebug("GetOneSitePmcIdFromPersonaAsync - PMCID={PmcId} from SAML", pmcId);
            return pmcId;
        }

        // 2. Fall back to BlueBook company map
        try
        {
            var companyMap = await _blueBook.GetCompanyMapAsync(
                persona.Organization.RealPageId,
                persona.Organization.BooksCustomerMasterId,
                source: BlueBookProductConstants.OneSite,
                domain: persona.Organization.OrganizationDomain.Name,
                cancellationToken: ct);

            var oneSiteEntry = companyMap?.FirstOrDefault(
                m => m.Source.Equals(BlueBookProductConstants.OneSite, StringComparison.OrdinalIgnoreCase));

            if (oneSiteEntry != null)
            {
                _logger.LogDebug("GetOneSitePmcIdFromPersonaAsync - PMCID={PmcId} from BlueBook", oneSiteEntry.CompanyInstanceSourceId);
                return oneSiteEntry.CompanyInstanceSourceId;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetOneSitePmcIdFromPersonaAsync - BlueBook error personaId={PersonaId}", persona.PersonaId);
        }

        _logger.LogWarning("GetOneSitePmcIdFromPersonaAsync - could not resolve PMCID for personaId={PersonaId}", persona.PersonaId);
        return string.Empty;
    }

    // ── PMC info (cached SOAP call) ────────────────────────────────────────────

    private async Task<PMCInfo?> GetPmcInfoAsync(int pmcId, CancellationToken ct)
    {
        string cacheKey = $"onesitePMCInfo_{pmcId}";
        return await _cache.GetOrSetAsync<PMCInfo?>(
            cacheKey,
            async innerCt =>
            {
                try
                {
                    return await _service.GetPMCUrlAsync(pmcId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "GetPmcInfoAsync - error for pmcId={PmcId}", pmcId);
                    return null;
                }
            },
            new CacheEntryOptions { ExpirationTimeInMinutes = PmcCacheMinutes },
            ct);
    }

    // ── MT OAuth2 bearer token (cached) ───────────────────────────────────────

    private async Task<string> GetMtTokenAsync(
        PMCInfo pmcInfo, List<ProductInternalSetting> settings, CancellationToken ct)
    {
        string cacheKey      = $"mt_access_token_{pmcInfo.ID}";
        string tokenEndpoint = GetSettingValue(settings, SettingMtTokenEndpoint);
        string clientId      = GetSettingValue(settings, SettingMtClientId);
        string clientSecret  = GetSettingValue(settings, SettingMtClientSecret);

        string? token = await _cache.GetOrSetAsync<string?>(
            cacheKey,
            async innerCt =>
            {
                _logger.LogDebug("GetMtTokenAsync - requesting token from https://{PmcUrl}/{Endpoint}",
                    pmcInfo.PMCURL, tokenEndpoint);

                using var client = _httpClientFactory.CreateClient("OneSiteMT");
                client.DefaultRequestHeaders.Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/json"));

                using var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type",    "client_credentials"),
                    new KeyValuePair<string, string>("client_id",     clientId),
                    new KeyValuePair<string, string>("client_secret", clientSecret)
                });

                using var response = await client.PostAsync(
                    $"https://{pmcInfo.PMCURL}/{tokenEndpoint}", content, innerCt);

                if (!response.IsSuccessStatusCode)
                {
                    string err = await response.Content.ReadAsStringAsync(innerCt);
                    throw new Exception($"Exception while getting token. {err}");
                }

                string json    = await response.Content.ReadAsStringAsync(innerCt);
                dynamic parsed = JsonConvert.DeserializeObject<dynamic>(json)!;
                return (string)parsed.access_token.ToString();
            },
            new CacheEntryOptions { ExpirationTimeInMinutes = TokenCacheMinutes },
            ct);

        return token ?? string.Empty;
    }

    // ── MT API generic GET ─────────────────────────────────────────────────────

    private async Task<T?> GetResultFromApiAsync<T>(
        string token, string url, CancellationToken ct) where T : class
    {
        using var client = _httpClientFactory.CreateClient("OneSiteMT");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using var response = await client.GetAsync(url, ct);
        if (!response.IsSuccessStatusCode)
        {
            string error = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("GetResultFromApiAsync - GET {Url} failed: {Error}", url, error);
            return null;
        }

        string json = await response.Content.ReadAsStringAsync(ct);
        return JsonConvert.DeserializeObject(json, typeof(T)) as T;
    }

    // ── SuperUser check ────────────────────────────────────────────────────────

    private async Task<bool> IsSuperUserAsync(Persona userPersona, CancellationToken ct)
    {
        try
        {
            var rel = await _managePartyRelationship.GetPartyRelationshipAsync(
                realPageIdFrom:       userPersona.Organization.RealPageId,
                realPageIdTo:         userPersona.RealPageId,
                roleTypeNameFrom:     null,
                roleTypeNameTo:       "SuperUser",
                relationshipTypeName: null,
                ct);
            return rel is not null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "IsSuperUserAsync - error for personaId={PersonaId}", userPersona.PersonaId);
            return false;
        }
    }

    // ── Email / no-email helpers ───────────────────────────────────────────────

    /// <summary>
    /// Returns <c>true</c> when the persona represents a "no-email" user.
    /// Async equivalent of <c>ManageProductBase.IsRegularUserNoEmail</c>:
    /// uses <see cref="UserRoleType.UserNoEmail"/> on the persona directly,
    /// matching the pattern established in <see cref="ManageProductOpsAsync"/>.
    /// </summary>
    private static bool IsRegularUserNoEmail(Persona userPersona)
        => userPersona.UserTypeId == (int)UserRoleType.UserNoEmail;

    // ── Email validation ───────────────────────────────────────────────────────

    private static string ValidateAndReturnEmailAddress(string email)
        => email.Contains('@') ? email : string.Empty;

    // ── Utility ────────────────────────────────────────────────────────────────

    private static NameValuePair[] ToNameValuePairs(Dictionary<string, string> args)
        => args.Select(kv => new NameValuePair { Name = kv.Key, Value = kv.Value }).ToArray();
}
