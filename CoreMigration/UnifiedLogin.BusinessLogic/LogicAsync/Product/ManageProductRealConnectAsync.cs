using System.Collections.Frozen;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.RealConnect;
using UnifiedLogin.SharedObjects.Saml;
using ProductRoleModel = UnifiedLogin.SharedObjects.Product;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Product;

/// <summary>
/// Native-async implementation of RealConnect product user management.
/// <para>
/// Replaces <c>ManageProductRealConnect</c>:
/// <c>DefaultUserClaim</c>-bound constructor → 11-dep DI constructor;
/// all blocking <c>.Result</c> calls → <c>await</c>;
/// <c>new HttpClient</c> → <c>IHttpClientFactory</c> (named <c>"RealConnect"</c>);
/// <c>RPObjectCache</c> → <c>IMemoryCache</c>;
/// <c>new ManageUnifiedSettings</c> → <c>IManageBlueBookAsync</c>;
/// recursive cursor paging → iterative async loop.
/// </para>
/// <para>
/// All RC HTTP payloads are serialised with Newtonsoft (the shared-objects DTOs
/// carry <c>[JsonProperty]</c> attributes).  Responses are also deserialised via
/// Newtonsoft for the same reason.
/// </para>
/// </summary>
public sealed class ManageProductRealConnectAsync : IManageProductRealConnectAsync
{
    // ── Constants ─────────────────────────────────────────────────────────────

    private const int    ProductId                = (int)ProductEnum.RealConnect;
    private const string ProductStatusSettingType = "ProductStatus";
    private const string ApiEndpointKey           = "APIENDPOINT";
    private const string ApiKeyKey                = "APIKEY";
    private const string GreenbookCaresKey        = "IsGreenbookCaresCheckRequired";
    private const string HttpClientName           = "RealConnect";

    /// <summary>
    /// Ref1 values that represent valid, usable license types.
    /// <c>FrozenSet</c> gives O(1) lookup with zero allocation on Contains.
    /// </summary>
    private static readonly FrozenSet<string> ValidRef1Types =
        FrozenSet.ToFrozenSet(["custom", "location", "position", "property"],
            StringComparer.OrdinalIgnoreCase);

    // ── Fields ────────────────────────────────────────────────────────────────

    private readonly IProductContextServiceAsync              _contextService;
    private readonly IProductRepositoryAsync                  _productRepository;
    private readonly ISamlAttributeServiceAsync               _samlService;
    private readonly IManageBlueBookAsync                     _manageBlueBook;
    private readonly IManagePersonaAsync                      _managePersona;
    private readonly IManagePersonAsync                       _managePerson;
    private readonly IManageUserLoginAsync                    _manageUserLogin;
    private readonly IManageElectronicAddressAsync            _manageElectronicAddress;
    private readonly IMemoryCache                             _memoryCache;
    private readonly IHttpClientFactory                       _httpClientFactory;
    private readonly ILogger<ManageProductRealConnectAsync>   _logger;

    // ── Constructor ───────────────────────────────────────────────────────────

    public ManageProductRealConnectAsync(
        IProductContextServiceAsync            contextService,
        IProductRepositoryAsync                productRepository,
        ISamlAttributeServiceAsync             samlService,
        IManageBlueBookAsync                   manageBlueBook,
        IManagePersonaAsync                    managePersona,
        IManagePersonAsync                     managePerson,
        IManageUserLoginAsync                  manageUserLogin,
        IManageElectronicAddressAsync          manageElectronicAddress,
        IMemoryCache                           memoryCache,
        IHttpClientFactory                     httpClientFactory,
        ILogger<ManageProductRealConnectAsync> logger)
    {
        ArgumentNullException.ThrowIfNull(contextService);
        ArgumentNullException.ThrowIfNull(productRepository);
        ArgumentNullException.ThrowIfNull(samlService);
        ArgumentNullException.ThrowIfNull(manageBlueBook);
        ArgumentNullException.ThrowIfNull(managePersona);
        ArgumentNullException.ThrowIfNull(managePerson);
        ArgumentNullException.ThrowIfNull(manageUserLogin);
        ArgumentNullException.ThrowIfNull(manageElectronicAddress);
        ArgumentNullException.ThrowIfNull(memoryCache);
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentNullException.ThrowIfNull(logger);

        _contextService          = contextService;
        _productRepository       = productRepository;
        _samlService             = samlService;
        _manageBlueBook          = manageBlueBook;
        _managePersona           = managePersona;
        _managePerson            = managePerson;
        _manageUserLogin         = manageUserLogin;
        _manageElectronicAddress = manageElectronicAddress;
        _memoryCache             = memoryCache;
        _httpClientFactory       = httpClientFactory;
        _logger                  = logger;
    }

    // ── Public: GetRolesAsync ─────────────────────────────────────────────────

    public async Task<ListResponse> GetRolesAsync(
        long editorPersonaId, long userPersonaId,
        RequestParameter datafilter,
        CancellationToken ct = default)
    {
        _logger.LogDebug("GetRoles - Getting roles for product {ProductId} editorPersonaId {EditorPersonaId}",
            ProductId, editorPersonaId);

        var (ctx, error) = await _contextService.GetUserContextAsync(editorPersonaId, userPersonaId, ProductId, ct);
        if (error is not null) return error;

        try
        {
            var roles = await _productRepository.ListRolesForProductByPartyAsync(
                ctx!.EditorPersona.Organization.PartyId, [ProductId], ProductId, ct);

            _logger.LogDebug("GetRoles - Merging roles for user {UserPersonaId}", userPersonaId);
            return await MergeRolesWithUserAsync(roles, ctx.ProductLearnerId, ctx.ProductManagerId, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "GetRoles - Error getting roles for product {ProductId}", ProductId);
            return new ListResponse { IsError = true, ErrorReason = CommonMessageConstants.RoleErrorMessage };
        }
    }

    // ── Public: GetPropertiesAsync ────────────────────────────────────────────

    public async Task<ListResponse> GetPropertiesAsync(
        long editorPersonaId, long userPersonaId,
        RequestParameter datafilter,
        CancellationToken ct = default)
    {
        _logger.LogDebug("GetProperties - Getting licenses for product {ProductId}", ProductId);

        var (ctx, error) = await _contextService.GetUserContextAsync(editorPersonaId, userPersonaId, ProductId, ct);
        if (error is not null) return error;

        var (apiEndpoint, apiKey) = await GetProductConfigAsync(ct);
        var orgRealPageId         = ctx!.EditorPersona.Organization.RealPageId;
        var clientId              = await GetClientIdAsync(orgRealPageId, ct);

        if (string.IsNullOrEmpty(clientId))
        {
            _logger.LogWarning("GetProperties - ClientId not found for company {OrgRealPageId}", orgRealPageId);
            return new ListResponse { IsError = true, ErrorReason = "ClientId not found or company doesnt have product assigned" };
        }

        try
        {
            using var client      = CreateHttpClient(apiKey);
            var clientLicenses    = await GetClientLicenseDetailsAsync(apiEndpoint, clientId, client, ctx.EditorPersona.Organization.PartyId, ct);
            var licenseJson       = JsonConvert.SerializeObject(clientLicenses);
            var companyLicenses   = new CompanyLicenses
            {
                ManagerLicenses = JsonConvert.DeserializeObject<ClientLicenseDetails>(licenseJson)!,
                LearnerLicenses = JsonConvert.DeserializeObject<ClientLicenseDetails>(licenseJson)!
            };

            // Mark assigned learner licenses
            if (!string.IsNullOrEmpty(ctx.ProductLearnerId))
            {
                var learnerUser = await GetRcUserAsync(ctx.ProductLearnerId, apiEndpoint, client, ct);
                if (learnerUser is not null)
                {
                    foreach (var lic in learnerUser.AllocatedLicenses)
                    {
                        var match = companyLicenses.LearnerLicenses.Licenses.Find(l => l.Id == lic.LicenseId);
                        if (match is not null) match.IsAssigned = true;
                    }
                }
            }

            // Mark assigned manager licenses
            if (!string.IsNullOrEmpty(ctx.ProductManagerId))
            {
                var managerUser = await GetRcUserAsync(ctx.ProductManagerId, apiEndpoint, client, ct);
                if (managerUser is not null)
                {
                    foreach (var lic in managerUser.AllocatedLicenses)
                    {
                        var match = companyLicenses.ManagerLicenses.Licenses.Find(l => l.Id == lic.LicenseId);
                        if (match is not null) match.IsAssigned = true;
                    }
                }
            }

            // Sort manager licenses
            companyLicenses.ManagerLicenses.Licenses = companyLicenses.ManagerLicenses.Licenses
                .Select(l => { l.SortId = ManagerLicenseSortId(l.Ref1); return l; })
                .OrderBy(l => l.SortId).ThenBy(l => l.Name)
                .ToList();

            companyLicenses.LearnerLicenses.Licenses = companyLicenses.LearnerLicenses.Licenses
                .OrderBy(l => l.Name).ToList();

            var result = new List<CompanyLicenses> { companyLicenses };

            return new ListResponse
            {
                Records      = result.Cast<object>().ToList(),
                TotalRows    = clientLicenses.Licenses.Count,
                RowsPerPage  = 9999,
                ErrorReason  = string.Empty,
                TotalPages   = 1
            };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "GetProperties - Error getting properties for product {ProductId}", ProductId);
            return new ListResponse { IsError = true, ErrorReason = CommonMessageConstants.PropertyErrorMessage };
        }
    }

    // ── Public: CreateUpdateUserAsync ─────────────────────────────────────────

    public async Task<string> CreateUpdateUserAsync(
        Guid createUserRealPageId,
        long createUserPersonaId, long assignUserPersonaId,
        object rolePropList,
        CancellationToken ct = default)
    {
        var (ctx, error) = await _contextService.GetUserContextAsync(createUserPersonaId, assignUserPersonaId, ProductId, ct);
        if (error is not null) return error.ErrorReason;

        var (apiEndpoint, apiKey) = await GetProductConfigAsync(ct);
        var orgRealPageId         = ctx!.EditorPersona.Organization.RealPageId;
        var clientId              = await GetClientIdAsync(orgRealPageId, ct);

        if (string.IsNullOrEmpty(clientId))
        {
            _logger.LogWarning("CreateUpdateUser - ClientId not found for company {OrgRealPageId}", orgRealPageId);
            return "ClientId not found or company doesnt have product assigned";
        }

        var userProp = rolePropList as ProductUserRolePropertiesGroups
            ?? throw new ArgumentException("rolePropList must be ProductUserRolePropertiesGroups", nameof(rolePropList));

        userProp.RCLicenseDetails ??= new RCProductBatch
        {
            LearnerLicenseId = [],
            ManagerLicenseId = []
        };

        try
        {
            using var client   = CreateHttpClient(apiKey);
            var roles          = await _productRepository.ListRolesForProductByPartyAsync(
                ctx.EditorPersona.Organization.PartyId, [ProductId], ProductId, ct);
            var selectedRoles  = roles
                .Where(x => userProp.RoleList.Contains(x.ID))
                .Select(c => c.Alias)
                .ToList();

            if (selectedRoles.Count > 2)
                _logger.LogWarning("CreateUpdateUser - More than 2 roles selected for user in product {ProductId}", ProductId);

            var clientLicenses    = await GetClientLicenseDetailsAsync(apiEndpoint, clientId, client, ctx.EditorPersona.Organization.PartyId, ct);
            var selectedLicenses  = clientLicenses.Licenses
                .Where(x => userProp.RCLicenseDetails.LearnerLicenseId.Contains(x.Id))
                .ToList();

            if (!(selectedLicenses.Any(a => a.Ref1 == "position")
               && selectedLicenses.Any(a => a.Ref1 == "property")
               && selectedLicenses.Any(a => a.Ref1 == "location")))
            {
                _logger.LogError("CreateUpdateUser - License validation failed (missing position/property/location)");
                return "No license and manager information.";
            }

            var userPersona  = await _managePersona.GetPersonaAsync(assignUserPersonaId, cancellationToken: ct);
            var realPageId   = userPersona.RealPageId;
            var person       = await _managePerson.GetPersonAsync(realPageId, ct);
            var userLogin    = await _manageUserLogin.GetUserLoginOnlyAsync(realPageId, ct);
            var emailAddress = await FormattedEmailAsync(userLogin.LoginName, assignUserPersonaId, realPageId,
                userPersona, ct);

            var user = new CreateRCUser
            {
                FirstName          = person.FirstName,
                LastName           = person.LastName,
                Email              = emailAddress,
                ClientId           = clientId,
                CourseIds          = selectedLicenses.SelectMany(y => y.CourseIds).Distinct().ToList(),
                StudentLicenseIds  = selectedLicenses.Select(l => l.Id).Distinct().ToList(),
                ExternalCustomerId = userLogin.UserId.ToString(),
                Role               = "student"
            };

            _logger.LogDebug("CreateUpdateUser - Prepared RC user payload for persona {AssignUserPersonaId}", assignUserPersonaId);

            if (string.IsNullOrEmpty(ctx.ProductLearnerId) || ctx.ProductLearnerId == Guid.Empty.ToString())
            {
                return await CreateNewRcUserAsync(user, selectedRoles, assignUserPersonaId,
                    clientId, apiEndpoint, client, clientLicenses, selectedLicenses,
                    person, userLogin, emailAddress, userProp, ct);
            }
            else
            {
                return await UpdateExistingRcUserAsync(user, selectedRoles, assignUserPersonaId,
                    ctx.ProductLearnerId, ctx.ProductManagerId,
                    apiEndpoint, client, clientLicenses, selectedLicenses,
                    person, userLogin, emailAddress, userProp, ct);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "CreateUpdateUser - Unhandled exception for persona {AssignUserPersonaId}", assignUserPersonaId);
            return $"Error creating user {ex.Message}";
        }
    }

    // ── Public: UnassignUserAsync ─────────────────────────────────────────────

    public async Task<string> UnassignUserAsync(
        long editorPersonaId, long userPersonaId,
        string userStatus = "disabled",
        CancellationToken ct = default)
    {
        var (ctx, error) = await _contextService.GetUserContextAsync(editorPersonaId, userPersonaId, ProductId, ct);
        if (error is not null) return error.ErrorReason;

        if (string.IsNullOrEmpty(ctx!.ProductLearnerId))
        {
            _logger.LogError("UnassignUser - ProductLearnerId is empty for product {ProductId}", ProductId);
            return $"Error unassigning userPersona {userPersonaId}";
        }

        var (apiEndpoint, apiKey) = await GetProductConfigAsync(ct);

        try
        {
            using var client = CreateHttpClient(apiKey);
            string url       = $"{apiEndpoint}/users/{ctx.ProductLearnerId}/updateStatus";

            _logger.LogDebug("UnassignUser - Setting status to '{UserStatus}' via {Url}", userStatus, url);

            using var content  = SerializeToContent(new RCUserStatus { Status = userStatus });
            var response       = await client.PutAsync(url, content, ct);

            var jsonContent    = await response.Content.ReadAsStringAsync(ct);

            if (response.IsSuccessStatusCode)
            {
                if (jsonContent.Contains("errors"))
                {
                    _logger.LogError("UnassignUser - API returned errors: {Json}", jsonContent);
                    await _productRepository.UpdateProductSettingProductStatusAsync(
                        userPersonaId, ProductId, ProductStatusSettingType,
                        (int)ProductBatchStatusType.Error, ct);
                    return $"Error unassigning user {jsonContent}";
                }

                int statusValue = userStatus == "disabled"
                    ? (int)ProductBatchStatusType.Deactivated
                    : (int)ProductBatchStatusType.Success;
                await _productRepository.UpdateProductSettingProductStatusAsync(
                    userPersonaId, ProductId, ProductStatusSettingType, statusValue, ct);
                return string.Empty;
            }

            _logger.LogError("UnassignUser - HTTP {StatusCode}: {Body}", response.StatusCode, jsonContent);
            return $"Error unassigning user {jsonContent}";
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "UnassignUser - Exception for persona {UserPersonaId}", userPersonaId);
            return $"Error unassigning user {ex.Message}";
        }
    }

    // ── Public: UpdateProductUserProfileAsync ─────────────────────────────────

    public async Task<string> UpdateProductUserProfileAsync(
        long editorPersonaId, long userPersonaId,
        CancellationToken ct = default)
    {
        _logger.LogDebug("UpdateProductUserProfile - Begin profile update for persona {UserPersonaId}", userPersonaId);

        var (ctx, error) = await _contextService.GetUserContextAsync(editorPersonaId, userPersonaId, ProductId, ct);
        if (error is not null) return error.ErrorReason;

        var (apiEndpoint, apiKey) = await GetProductConfigAsync(ct);
        var orgRealPageId         = ctx!.EditorPersona.Organization.RealPageId;
        var clientId              = await GetClientIdAsync(orgRealPageId, ct);

        if (string.IsNullOrEmpty(clientId))
        {
            _logger.LogWarning("UpdateProductUserProfile - ClientId not found for company {OrgRealPageId}", orgRealPageId);
            return "ClientId not found or company doesnt have product assigned";
        }

        try
        {
            using var client  = CreateHttpClient(apiKey);
            var userPersona   = await _managePersona.GetPersonaAsync(userPersonaId, cancellationToken: ct);
            var realPageId    = userPersona.RealPageId;
            var person        = await _managePerson.GetPersonAsync(realPageId, ct);
            var userLogin     = await _manageUserLogin.GetUserLoginOnlyAsync(realPageId, ct);
            var emailAddress  = await FormattedEmailAsync(userLogin.LoginName, userPersonaId, realPageId,
                userPersona, ct);

            var targetUserId   = !string.IsNullOrEmpty(ctx.ProductManagerId) ? ctx.ProductManagerId : ctx.ProductLearnerId;
            string url         = $"{apiEndpoint}/users/{targetUserId}";

            var profile = new UpdateUserProfile
            {
                FirstName = person.FirstName,
                LastName  = person.LastName,
                Email     = emailAddress,
                ClientId  = clientId,
                Upsert    = !string.IsNullOrEmpty(ctx.ProductManagerId)
            };

            _logger.LogDebug("UpdateProductUserProfile - Calling {Url}", url);

            using var content  = SerializeToContent(profile);
            var response       = await client.PutAsync(url, content, ct);
            var jsonContent    = await response.Content.ReadAsStringAsync(ct);

            if (response.IsSuccessStatusCode)
            {
                if (jsonContent.Contains("errors"))
                {
                    _logger.LogError("UpdateProductUserProfile - API returned errors: {Json}", jsonContent);
                    await _productRepository.UpdateProductSettingProductStatusAsync(
                        userPersonaId, ProductId, ProductStatusSettingType,
                        (int)ProductBatchStatusType.Error, ct);
                    return $"Error updating user profile {jsonContent}";
                }

                await _samlService.UpsertAttributeAsync(userPersonaId, ProductId,
                    SamlAttributeEnum.productUsername, emailAddress, ct);
                _logger.LogDebug("UpdateProductUserProfile - Profile update successful for persona {UserPersonaId}", userPersonaId);
                return string.Empty;
            }

            _logger.LogError("UpdateProductUserProfile - HTTP {StatusCode}: {Body}", response.StatusCode, jsonContent);
            return $"Error updating user profile {jsonContent}";
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "UpdateProductUserProfile - Exception for persona {UserPersonaId}", userPersonaId);
            return $"Error updating user profile {ex.Message}";
        }
    }

    // ── Private: create/update helpers ────────────────────────────────────────

    private async Task<string> CreateNewRcUserAsync(
        CreateRCUser user,
        List<string> selectedRoles,
        long assignUserPersonaId,
        string clientId,
        string apiEndpoint,
        HttpClient client,
        ClientLicenseDetails clientLicenses,
        List<License> selectedLicenses,
        Person person,
        UserLoginOnly userLogin,
        string emailAddress,
        ProductUserRolePropertiesGroups userProp,
        CancellationToken ct)
    {
        string url = $"{apiEndpoint}/users";
        _logger.LogDebug("CreateNewRcUser - Creating new user at {Url}", url);

        using var content  = SerializeToContent(user);
        var response       = await client.PostAsync(url, content, ct);
        var jsonContent    = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("CreateNewRcUser - HTTP {StatusCode}: {Body}", response.StatusCode, jsonContent);
            await _productRepository.UpdateProductSettingProductStatusAsync(
                assignUserPersonaId, ProductId, ProductStatusSettingType,
                (int)ProductBatchStatusType.Error, ct);
            return $"Error creating user {jsonContent}";
        }

        if (jsonContent.Contains("errors"))
        {
            _logger.LogError("CreateNewRcUser - API errors: {Json}", jsonContent);
            await _productRepository.UpdateProductSettingProductStatusAsync(
                assignUserPersonaId, ProductId, ProductStatusSettingType,
                (int)ProductBatchStatusType.Error, ct);
            return $"Error creating user {jsonContent}";
        }

        var userResponse = JsonConvert.DeserializeObject<RealConnectUser>(jsonContent)!;
        _logger.LogDebug("CreateNewRcUser - User created, id {UserId}", userResponse.Id);

        await _productRepository.UpdateProductSettingProductStatusAsync(
            assignUserPersonaId, ProductId, ProductStatusSettingType,
            (int)ProductBatchStatusType.Success, ct);

        await _samlService.UpsertAttributesAsync(assignUserPersonaId, ProductId,
            new Dictionary<SamlAttributeEnum, string>
            {
                [SamlAttributeEnum.LearnerId]       = userResponse.Id.ToString(),
                [SamlAttributeEnum.productUsername]  = user.Email!
            }, ct);

        string result = string.Empty;
        if (selectedRoles.Count > 1)
        {
            result = await AddDualRoleToUserAsync(userResponse.Id.ToString(), string.Empty,
                selectedRoles, assignUserPersonaId,
                apiEndpoint, client, clientLicenses,
                person, userLogin, emailAddress, userProp, ct);
        }

        result += await BulkContentAssignmentAsync(userResponse.Id.ToString(), apiEndpoint, client,
            clientLicenses, selectedLicenses, ct);
        return result;
    }

    private async Task<string> UpdateExistingRcUserAsync(
        CreateRCUser user,
        List<string> selectedRoles,
        long assignUserPersonaId,
        string productLearnerId,
        string productManagerId,
        string apiEndpoint,
        HttpClient client,
        ClientLicenseDetails clientLicenses,
        List<License> selectedLicenses,
        Person person,
        UserLoginOnly userLogin,
        string emailAddress,
        ProductUserRolePropertiesGroups userProp,
        CancellationToken ct)
    {
        // Reactivate if currently disabled
        var existingUser = await GetRcUserAsync(productLearnerId, apiEndpoint, client, ct);
        if (existingUser?.Disabled == true)
            await UnassignUserAsync(0, assignUserPersonaId, "active", ct);

        user.ReplaceLicenseAccess = true;
        string url = $"{apiEndpoint}/users/{productLearnerId}";
        _logger.LogDebug("UpdateExistingRcUser - Updating user at {Url}", url);

        using var content  = SerializeToContent(user);
        var response       = await client.PutAsync(url, content, ct);
        var jsonContent    = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("UpdateExistingRcUser - HTTP {StatusCode}: {Body}", response.StatusCode, jsonContent);
            await _productRepository.UpdateProductSettingProductStatusAsync(
                assignUserPersonaId, ProductId, ProductStatusSettingType,
                (int)ProductBatchStatusType.Error, ct);
            return $"Error updating user {jsonContent}";
        }

        if (jsonContent.Contains("errors"))
        {
            _logger.LogError("UpdateExistingRcUser - API errors: {Json}", jsonContent);
            await _productRepository.UpdateProductSettingProductStatusAsync(
                assignUserPersonaId, ProductId, ProductStatusSettingType,
                (int)ProductBatchStatusType.Error, ct);
            return $"Error updating user {jsonContent}";
        }

        var userResponse = JsonConvert.DeserializeObject<RealConnectUser>(jsonContent)!;
        _logger.LogDebug("UpdateExistingRcUser - User updated, id {UserId}", userResponse.Id);

        await _productRepository.UpdateProductSettingProductStatusAsync(
            assignUserPersonaId, ProductId, ProductStatusSettingType,
            (int)ProductBatchStatusType.Success, ct);
        await _samlService.UpsertAttributeAsync(assignUserPersonaId, ProductId,
            SamlAttributeEnum.productUsername, user.Email!, ct);

        string result = string.Empty;
        if (selectedRoles.Count > 1)
        {
            result = await AddDualRoleToUserAsync(userResponse.Id.ToString(), productManagerId,
                selectedRoles, assignUserPersonaId,
                apiEndpoint, client, clientLicenses,
                person, userLogin, emailAddress, userProp, ct);
        }
        else if (!string.IsNullOrEmpty(productManagerId))
        {
            result += await RemoveDualRoleFromUserAsync(assignUserPersonaId, productManagerId,
                apiEndpoint, client, ct);
        }

        result += await BulkContentAssignmentAsync(productLearnerId, apiEndpoint, client,
            clientLicenses, selectedLicenses, ct);
        return result;
    }

    // ── Private: dual-role helpers ────────────────────────────────────────────

    private async Task<string> AddDualRoleToUserAsync(
        string learnerUserId,
        string currentManagerId,
        List<string> roles,
        long assignUserPersonaId,
        string apiEndpoint,
        HttpClient client,
        ClientLicenseDetails clientLicenses,
        Person person,
        UserLoginOnly userLogin,
        string emailAddress,
        ProductUserRolePropertiesGroups userProp,
        CancellationToken ct)
    {
        if (string.IsNullOrEmpty(learnerUserId))
        {
            _logger.LogError("AddDualRoleToUser - Cannot update dual role, learnerId is empty");
            return "Cannot update dual role, userid is empty";
        }

        string managerId = currentManagerId;
        if (string.IsNullOrEmpty(managerId))
        {
            // Tag learner for dual role to get a manager ID
            (string tagError, string newManagerId) = await TagDualRoleToUserAsync(
                learnerUserId, roles, assignUserPersonaId, apiEndpoint, client, ct);
            if (!string.IsNullOrEmpty(tagError)) return tagError;
            managerId = newManagerId;
        }

        string url            = $"{apiEndpoint}/users/{managerId}";
        string dualRoleName   = roles.Find(x => x != "student") ?? string.Empty;
        var selectedLicenses  = clientLicenses.Licenses
            .Where(x => userProp.RCLicenseDetails!.ManagerLicenseId.Contains(x.Id));

        var managerUser = new CreateRCUser
        {
            FirstName         = person.FirstName,
            LastName          = person.LastName,
            Email             = emailAddress,
            ClientId          = clientLicenses.Id,
            ManagerLicenseIds = selectedLicenses.Select(l => l.Id).Distinct().ToList(),
            ExternalCustomerId = userLogin.UserId.ToString(),
            Role              = dualRoleName,
            DualRole          = true,
            Upsert            = !string.IsNullOrEmpty(currentManagerId)
        };

        _logger.LogDebug("AddDualRoleToUser - Updating manager user at {Url}", url);

        using var content  = SerializeToContent(managerUser);
        var response       = await client.PutAsync(url, content, ct);
        var jsonContent    = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("AddDualRoleToUser - HTTP {StatusCode}: {Body}", response.StatusCode, jsonContent);
            return $"Error adding dual role {jsonContent}";
        }

        if (jsonContent.Contains("errors"))
        {
            _logger.LogError("AddDualRoleToUser - API errors: {Json}", jsonContent);
            await _productRepository.UpdateProductSettingProductStatusAsync(
                assignUserPersonaId, ProductId, ProductStatusSettingType,
                (int)ProductBatchStatusType.Error, ct);
            return $"Error adding dual role {jsonContent}";
        }

        _logger.LogDebug("AddDualRoleToUser - Dual role added successfully");
        return string.Empty;
    }

    private async Task<(string error, string managerId)> TagDualRoleToUserAsync(
        string learnerUserId,
        List<string> roles,
        long assignUserPersonaId,
        string apiEndpoint,
        HttpClient client,
        CancellationToken ct)
    {
        string url          = $"{apiEndpoint}/users/{learnerUserId}/makeDualRole";
        string dualRoleName = roles.Find(x => x != "student") ?? string.Empty;

        _logger.LogDebug("TagDualRoleToUser - Tagging user {LearnerId} with role {Role}", learnerUserId, dualRoleName);

        using var content  = SerializeToContent(new RCRole { Role = dualRoleName });
        var response       = await client.PutAsync(url, content, ct);
        var jsonContent    = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("TagDualRoleToUser - HTTP {StatusCode}: {Body}", response.StatusCode, jsonContent);
            return ($"Error tagging dual role {jsonContent}", string.Empty);
        }

        if (jsonContent.Contains("errors"))
        {
            _logger.LogError("TagDualRoleToUser - API errors: {Json}", jsonContent);
            await _productRepository.UpdateProductSettingProductStatusAsync(
                assignUserPersonaId, ProductId, ProductStatusSettingType,
                (int)ProductBatchStatusType.Error, ct);
            return ($"Error tagging dual role {jsonContent}", string.Empty);
        }

        var roleResponse = JsonConvert.DeserializeObject<RCRoleResponse>(jsonContent)!;
        string managerId = roleResponse.ManagerId;

        await _samlService.UpsertAttributesAsync(assignUserPersonaId, ProductId,
            new Dictionary<SamlAttributeEnum, string>
            {
                [SamlAttributeEnum.ManagerId] = managerId,
                [SamlAttributeEnum.DualRole]  = "true"
            }, ct);

        _logger.LogDebug("TagDualRoleToUser - Tagged successfully, managerId {ManagerId}", managerId);
        return (string.Empty, managerId);
    }

    private async Task<string> RemoveDualRoleFromUserAsync(
        long assignUserPersonaId,
        string managerId,
        string apiEndpoint,
        HttpClient client,
        CancellationToken ct)
    {
        string url = $"{apiEndpoint}/users/bulkRemoveDualRoleManager";
        _logger.LogDebug("RemoveDualRoleFromUser - Removing manager {ManagerId}", managerId);

        var payload = new BulkRemoveDualRoleManager { UserIds = [managerId] };

        using var content  = SerializeToContent(payload);
        var response       = await client.PutAsync(url, content, ct);
        var jsonContent    = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("RemoveDualRoleFromUser - HTTP {StatusCode}: {Body}", response.StatusCode, jsonContent);
            return $"Error removing dual role {jsonContent}";
        }

        if (jsonContent.Contains("errors"))
        {
            _logger.LogError("RemoveDualRoleFromUser - API errors: {Json}", jsonContent);
            await _productRepository.UpdateProductSettingProductStatusAsync(
                assignUserPersonaId, ProductId, ProductStatusSettingType,
                (int)ProductBatchStatusType.Error, ct);
            return $"Error removing dual role {jsonContent}";
        }

        var bulkResponse = JsonConvert.DeserializeObject<BulkRemoveDualRoleManagerResponse>(jsonContent)!;
        if (bulkResponse.InvalidUserIds?.Count > 0)
        {
            _logger.LogError("RemoveDualRoleFromUser - Invalid userIds: {Ids}", bulkResponse.InvalidUserIds);
            return $"Error removing dual role — invalid user ids: {string.Join(',', bulkResponse.InvalidUserIds)}";
        }

        await _samlService.RemoveAttributeAsync(assignUserPersonaId, ProductId, SamlAttributeEnum.ManagerId, ct);
        await _samlService.RemoveAttributeAsync(assignUserPersonaId, ProductId, SamlAttributeEnum.DualRole, ct);

        _logger.LogDebug("RemoveDualRoleFromUser - Dual role removed successfully");
        return string.Empty;
    }

    // ── Private: bulk content assignment ─────────────────────────────────────

    private async Task<string> BulkContentAssignmentAsync(
        string learnerId,
        string apiEndpoint,
        HttpClient client,
        ClientLicenseDetails clientDetails,
        List<License> selectedLicenses,
        CancellationToken ct)
    {
        if (clientDetails is null || !clientDetails.LearningPathIds.Any())
        {
            _logger.LogError("BulkContentAssignment - No learning paths for client");
            return $"No Learning path for the client";
        }
        if (selectedLicenses is null)
        {
            _logger.LogError("BulkContentAssignment - No licenses to assign for user {LearnerId}", learnerId);
            return $"No Licenses to assign for the user {learnerId}";
        }

        var selectedLearningPaths = clientDetails.Licenses
            .Where(c => selectedLicenses.Select(s => s.Id).Contains(c.Id))
            .SelectMany(d => d.LearningPathIds)
            .Distinct()
            .ToList();

        string url = $"{apiEndpoint}/users/bulkContentAssignment";

        var bulkContent = new BulkContentAssignment { Id = learnerId, LearningPathIds = selectedLearningPaths };
        var bulkAssign  = new BulkAssignContent();
        bulkAssign.Users.Add(bulkContent);

        _logger.LogDebug("BulkContentAssignment - Assigning {Count} learning paths to {LearnerId}",
            selectedLearningPaths.Count, learnerId);

        using var content  = SerializeToContent(bulkAssign);
        var response       = await client.PostAsync(url, content, ct);
        var jsonContent    = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("BulkContentAssignment - HTTP {StatusCode}: {Body}", response.StatusCode, jsonContent);
            return "Unable to assign bulk content - status is error";
        }

        var bulkResponse = JsonConvert.DeserializeObject<BulkContentAssignmentResponse>(jsonContent)!;
        if (bulkResponse.Errors.Count > 0)
        {
            _logger.LogError("BulkContentAssignment - Errors returned: {Errors}", bulkResponse.Errors);
            return "Unable to assign bulk content";
        }

        _logger.LogDebug("BulkContentAssignment - Assigned successfully for {LearnerId}", learnerId);
        return string.Empty;
    }

    // ── Private: license retrieval ────────────────────────────────────────────

    private async Task<ClientLicenseDetails> GetClientLicenseDetailsAsync(
        string apiEndpoint, string clientId, HttpClient client,
        long orgPartyId, CancellationToken ct)
    {
        string cacheKey = $"RC_Licenses_{orgPartyId}";

        return await _memoryCache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(1800);

            var result  = await FetchLicensePageAsync(string.Empty, apiEndpoint, clientId, client, ct);
            if (result is null) return new ClientLicenseDetails { Licenses = [] };

            // Iterative cursor paging — avoids stack growth of the original recursive pattern
            while (result.PageInfo?.HasMore == true && !string.IsNullOrEmpty(result.PageInfo.Cursor))
            {
                var nextPage = await FetchLicensePageAsync(result.PageInfo.Cursor, apiEndpoint, clientId, client, ct);
                if (nextPage is null) break;
                result.Licenses.AddRange(nextPage.Licenses);
                result.PageInfo = nextPage.PageInfo;
            }

            return result;
        }) ?? new ClientLicenseDetails { Licenses = [] };
    }

    private async Task<ClientLicenseDetails?> FetchLicensePageAsync(
        string cursor, string apiEndpoint, string clientId, HttpClient client, CancellationToken ct)
    {
        string url = string.IsNullOrEmpty(cursor)
            ? $"{apiEndpoint}/clients/{clientId}/licenses"
            : $"{apiEndpoint}/clients/{clientId}/licenses?cursor={cursor}";

        try
        {
            var response    = await client.GetAsync(url, ct);
            var jsonContent = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("FetchLicensePage - HTTP {StatusCode}: {Body}", response.StatusCode, jsonContent);
                return null;
            }

            if (jsonContent.Contains("errors"))
            {
                _logger.LogError("FetchLicensePage - API errors: {Json}", jsonContent);
                return null;
            }

            var details = JsonConvert.DeserializeObject<ClientLicenseDetails>(jsonContent)!;
            // Filter out licenses with unsupported Ref1 values
            details.Licenses = details.Licenses.Where(x => ValidRef1Types.Contains(x.Ref1 ?? string.Empty)).ToList();
            return details;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "FetchLicensePage - Exception fetching {Url}", url);
            return null;
        }
    }

    // ── Private: user/role helpers ────────────────────────────────────────────

    private async Task<RealConnectUser?> GetRcUserAsync(
        string userIdentity, string apiEndpoint, HttpClient client, CancellationToken ct)
    {
        string url = $"{apiEndpoint}/users/{userIdentity}";
        try
        {
            var response    = await client.GetAsync(url, ct);
            var jsonContent = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("GetRcUser - HTTP {StatusCode} for {UserIdentity}", response.StatusCode, userIdentity);
                return null;
            }

            return JsonConvert.DeserializeObject<RealConnectUser>(jsonContent);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "GetRcUser - Exception for {UserIdentity}", userIdentity);
            return null;
        }
    }

    private async Task<ListResponse> MergeRolesWithUserAsync(
        IList<ProductRoleModel.ProductRole> allRoles,
        string productLearnerId,
        string productManagerId,
        CancellationToken ct)
    {
        if (allRoles is null)
        {
            _logger.LogWarning("MergeRolesWithUser - No roles found for product {ProductId}", ProductId);
            return new ListResponse();
        }

        var (apiEndpoint, apiKey) = await GetProductConfigAsync(ct);
        using var client          = CreateHttpClient(apiKey);

        List<string> userRoles = [];

        if (!string.IsNullOrEmpty(productLearnerId))
        {
            var rcUser = await GetRcUserAsync(productLearnerId, apiEndpoint, client, ct);
            if (rcUser is not null)
            {
                userRoles.Add(rcUser.RoleKey);
                if (rcUser.ManagerUserId is not null)
                {
                    var rcManagerUser = await GetRcUserAsync(rcUser.ManagerUserId.ToString()!, apiEndpoint, client, ct);
                    if (rcManagerUser is not null) userRoles.Add(rcManagerUser.RoleKey);
                }
            }
        }

        foreach (var role in userRoles)
        {
            var match = allRoles.FirstOrDefault(a => a.Alias == role);
            if (match is not null) match.IsAssigned = true;
        }

        foreach (var role in allRoles)
            role.SortId = RoleSortId(role.Alias);

        var sorted = allRoles.OrderBy(o => o.SortId).ToList();

        return new ListResponse
        {
            Records     = sorted.Cast<object>().ToList(),
            TotalRows   = sorted.Count,
            RowsPerPage = 9999,
            ErrorReason = string.Empty,
            TotalPages  = 1
        };
    }

    // ── Private: company/config resolution ───────────────────────────────────

    /// <summary>
    /// Resolves the RealConnect client ID for <paramref name="orgRealPageId"/>
    /// via BlueBook UDM lookup.  Result is cached for 30 minutes per company.
    /// </summary>
    private async Task<string> GetClientIdAsync(Guid orgRealPageId, CancellationToken ct)
    {
        string cacheKey = $"RC_ClientId_{orgRealPageId}";
        return await _memoryCache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
            return await ResolveClientIdFromUdmAsync(orgRealPageId, ct) ?? string.Empty;
        }) ?? string.Empty;
    }

    private async Task<string?> ResolveClientIdFromUdmAsync(Guid orgRealPageId, CancellationToken ct)
    {
        var settings               = await _productRepository.GetProductInternalSettingsAsync(ProductId, ct);
        var greenbookCaresValue    = settings.FirstOrDefault(s =>
            s.Name.Equals(GreenbookCaresKey, StringComparison.OrdinalIgnoreCase))?.Value;

        bool isGreenbookCaresRequired = greenbookCaresValue is not null && greenbookCaresValue != "0";
        if (!isGreenbookCaresRequired) return null;

        var allProducts   = await _productRepository.ListProductsAsync(ProductId, null, null, null, ct);
        var productDetail = allProducts.FirstOrDefault(x => x.ProductId == ProductId);
        if (productDetail is null) return null;

        var booksCompanyInstance = await _manageBlueBook.GetCompanyInstanceByUPFMCompanyIdAsync(
            orgRealPageId.ToString().ToLower(), ct);

        int customerCompanyId = booksCompanyInstance?.Attributes?.CustomerCompanyMap
            .FirstOrDefault()?.CustomerCompanyId ?? 0;
        string? domain = booksCompanyInstance?.Attributes?.Domain;

        if (string.IsNullOrEmpty(domain) || customerCompanyId == 0)
        {
            _logger.LogWarning("ResolveClientIdFromUdm - CustomerCompanyId or domain missing for {OrgRealPageId}", orgRealPageId);
            return null;
        }

        var companyMap        = await _manageBlueBook.GetCustomerCompanyMapByCustomerCompanyIdAsync(customerCompanyId, domain, ct);
        var rcCompanyInstance = companyMap?.Find(p => p.Source == productDetail.UDMSourceCode);

        if (rcCompanyInstance is null)
        {
            _logger.LogWarning("ResolveClientIdFromUdm - RC company instance not found for {OrgRealPageId}", orgRealPageId);
            return null;
        }

        _logger.LogDebug("ResolveClientIdFromUdm - Found clientId {ClientId} for {OrgRealPageId}",
            rcCompanyInstance.CompanyInstanceSourceId, orgRealPageId);
        return rcCompanyInstance.CompanyInstanceSourceId;
    }

    /// <summary>
    /// Returns the cached product config (API endpoint + API key).
    /// Resolved once from <see cref="IProductRepositoryAsync.GetProductInternalSettingsAsync"/>
    /// and held for 1 hour.
    /// </summary>
    private async Task<(string apiEndpoint, string apiKey)> GetProductConfigAsync(CancellationToken ct)
    {
        return await _memoryCache.GetOrCreateAsync("RC_ProductConfig", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            var settings = await _productRepository.GetProductInternalSettingsAsync(ProductId, ct);
            var apiEndpoint = settings.FirstOrDefault(a =>
                a.Name.Equals(ApiEndpointKey, StringComparison.OrdinalIgnoreCase))?.Value
                ?? throw new InvalidOperationException("RealConnect APIENDPOINT not found in product internal settings.");
            var apiKey = settings.FirstOrDefault(a =>
                a.Name.Equals(ApiKeyKey, StringComparison.OrdinalIgnoreCase))?.Value
                ?? throw new InvalidOperationException("RealConnect APIKEY not found in product internal settings.");
            return (apiEndpoint, apiKey);
        });
    }

    // ── Private: email helpers ────────────────────────────────────────────────

    /// <summary>
    /// Returns the formatted (mangled) email that RealConnect uses as the user identifier.
    /// <para>
    /// For no-email users, the notification email from the electronic address list is used
    /// as the base address; for all others the login name is used.
    /// Format: <c>local+{personaId}@domain</c>.
    /// </para>
    /// </summary>
    private async Task<string> FormattedEmailAsync(
        string email,
        long personaId,
        Guid realPageId,
        Persona userPersona,
        CancellationToken ct)
    {
        bool isValidEmail    = new EmailAddressAttribute().IsValid(email);
        bool isUserNoEmail   = await _contextService.IsRegularUserNoEmailAsync(userPersona, ct);
        string emailResult;

        if (isUserNoEmail)
        {
            var addresses       = await _manageElectronicAddress.ListElectronicAddressForPersonAsync(realPageId, "EMAIL", ct);
            var notificationEmail = addresses
                .FirstOrDefault(a => !string.IsNullOrEmpty(a.AddressString)
                    && new EmailAddressAttribute().IsValid(a.AddressString))
                ?.AddressString;

            if (!string.IsNullOrEmpty(notificationEmail))
            {
                var parts   = notificationEmail.Split('@');
                emailResult = $"{parts[0]}+{personaId}@{parts[1]}";
            }
            else if (isValidEmail)
            {
                var parts   = email.Split('@');
                emailResult = $"{parts[0]}+{personaId}@{parts[1]}";
            }
            else
            {
                emailResult = $"{email}+{personaId}@bogusemail.com";
            }
        }
        else
        {
            var parts   = email.Split('@');
            emailResult = $"{parts[0]}+{personaId}@{parts[1]}";
        }

        return emailResult.ToLowerInvariant();
    }

    // ── Private: HTTP utilities ───────────────────────────────────────────────

    /// <summary>
    /// Creates a configured <c>HttpClient</c> from the factory with the Bearer token
    /// from product settings. The named handler "RealConnect" carries the Polly retry
    /// policy registered at DI time.
    /// </summary>
    private HttpClient CreateHttpClient(string apiKey)
    {
        var client = _httpClientFactory.CreateClient(HttpClientName);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);
        return client;
    }

    /// <summary>
    /// Serializes <paramref name="payload"/> with Newtonsoft (respects
    /// <c>[JsonProperty]</c> attributes on the RC shared-object DTOs) and wraps
    /// in a UTF-8 <c>application/json</c> <see cref="StringContent"/>.
    /// </summary>
    private static StringContent SerializeToContent<T>(T payload) =>
        new(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

    // ── Private: sort helpers ─────────────────────────────────────────────────

    private static int ManagerLicenseSortId(string? ref1) => ref1 switch
    {
        "property" => 1,
        "position" => 2,
        "location" => 3,
        _          => 4
    };

    private static int RoleSortId(string? alias) => alias switch
    {
        "student"                  => 1,
        "sublicense-manager"       => 2,
        "customer-reporting-only"  => 3,
        "customer-admin"           => 4,
        _                          => 5
    };

    // ── Private: Polly rate-limit policy ─────────────────────────────────────

    /// <summary>
    /// Builds a Polly retry policy that honours the RealConnect
    /// <c>X-RateLimit-Reset</c> header on HTTP 429 responses.
    /// Register this as a <c>DelegatingHandler</c> on the "RealConnect" named client
    /// in the DI bootstrapper.
    /// </summary>
    internal static IAsyncPolicy<HttpResponseMessage> GetRateLimitPolicy() =>
        Policy
            .HandleResult<HttpResponseMessage>(msg => msg.StatusCode == (System.Net.HttpStatusCode)429)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: (attempt, outcome, _) =>
                {
                    if (outcome.Result.Headers.TryGetValues("X-RateLimit-Reset", out var values)
                        && int.TryParse(values.First(), out int seconds))
                        return TimeSpan.FromSeconds(seconds);
                    return TimeSpan.FromSeconds(Math.Pow(2, attempt));
                },
                onRetryAsync: (_, timespan, attempt, _) =>
                {
                    // \e is the C# 13 Unicode escape for ESC (U+001B)
                    Console.WriteLine($"\e[33mRealConnect rate-limited — retrying in {timespan.TotalSeconds:F1}s (attempt {attempt})\e[0m");
                    return Task.CompletedTask;
                });
}

// ── File-private: RateLimitPolicyHandler ─────────────────────────────────────

/// <summary>
/// Delegating handler that executes HTTP sends through the provided Polly policy.
/// Register via:
/// <code>
/// services.AddHttpClient("RealConnect")
///         .AddHttpMessageHandler(() => new RcRateLimitPolicyHandler(
///             ManageProductRealConnectAsync.GetRateLimitPolicy()));
/// </code>
/// </summary>
file sealed class RcRateLimitPolicyHandler(IAsyncPolicy<HttpResponseMessage> policy) : DelegatingHandler
{
    private readonly IAsyncPolicy<HttpResponseMessage> _policy =
        policy ?? throw new ArgumentNullException(nameof(policy));

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken) =>
        _policy.ExecuteAsync(ct => base.SendAsync(request, ct), cancellationToken);
}
