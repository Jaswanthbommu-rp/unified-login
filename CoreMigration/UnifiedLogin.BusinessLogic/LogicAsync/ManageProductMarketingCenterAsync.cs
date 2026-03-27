using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.LogicAsync.Models;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Extensions;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.MarketingCenter;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.ResponseObject;
using UnifiedLogin.SharedObjects.Saml;
using MC = UnifiedLogin.SharedObjects.Product.MarketingCenter;
using Right = UnifiedLogin.SharedObjects.Product.MarketingCenter.Right;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// True async implementation of Marketing Center user-management operations.
/// Replaces the stepping-stone <c>Task.FromResult(new ManageProductMarketingCenter(userClaim).Method(...))</c>
/// pattern with fully async/await calls backed by injected services.
/// </summary>
public sealed class ManageProductMarketingCenterAsync : IManageProductMarketingCenterAsync
{
    private const int    ProductId               = (int)ProductEnum.MarketingCenter;
    private const string ProductStatusSettingType = "ProductStatus";

    // Activity-log JSON templates — mirrors ManageProductMarketingCenter constants
    private const string RolesAssignMsg    = "{\"action\":\"Assigned\",\"value\":\"RoleName\"}";
    private const string RolesRemovedMsg   = "{\"action\":\"Removed\",\"value\":\"RoleName\"}";
    private const string PropsAssignMsg    = "{\"action\":\"Assigned\",\"value\":\"PropertyName\"}";
    private const string PropsRemovedMsg   = "{\"action\":\"Removed\",\"value\":\"PropertyName\"}";
    private const string RoleCreateMsg     = "{\"action\":\"Created Role\",\"value\":\"RoleName\"}";
    private const string RoleDeleteMsg     = "{\"action\":\"Deleted Role\",\"value\":\"RoleName\"}";
    private const string RoleNameUpdateMsg = "{\"action\":\"Updated Role Name\",\"value\":\"RoleName\"}";
    private const string RoleDescUpdateMsg = "{\"action\":\"Updated Role Description\",\"value\":\"RoleName\"}";
    private const string RightAssignMsg    = "{\"action\":\"Added Rights\",\"value\":\"RightName\"}";
    private const string RightUnassignMsg  = "{\"action\":\"Removed Rights\",\"value\":\"RightName\"}";
    private const string RoleAssignMsg     = "{\"action\":\"Added Roles\",\"value\":\"RoleName\"}";
    private const string RoleUnassignMsg   = "{\"action\":\"Removed Roles\",\"value\":\"RoleName\"}";

    private readonly IProductContextServiceAsync              _contextService;
    private readonly IProductSettingServiceAsync              _settingService;
    private readonly IManageBlueBookAsync                     _blueBook;
    private readonly IManagePersonaAsync                      _managePersona;
    private readonly IManagePersonAsync                       _managePerson;
    private readonly IManageUserLoginAsync                    _manageUserLogin;
    private readonly IManagePartyRelationshipAsync            _managePartyRelationship;
    private readonly ISamlAttributeServiceAsync               _samlAttributeService;
    private readonly IManageElectronicAddressAsync            _manageElectronicAddress;
    private readonly IHttpClientFactory                       _httpClientFactory;
    private readonly IUserRepositoryAsync                     _userRepository;
    private readonly ILogger<ManageProductMarketingCenterAsync> _logger;

    public ManageProductMarketingCenterAsync(
        IProductContextServiceAsync               contextService,
        IProductSettingServiceAsync               settingService,
        IManageBlueBookAsync                      blueBook,
        IManagePersonaAsync                       managePersona,
        IManagePersonAsync                        managePerson,
        IManageUserLoginAsync                     manageUserLogin,
        IManagePartyRelationshipAsync             managePartyRelationship,
        ISamlAttributeServiceAsync                samlAttributeService,
        IManageElectronicAddressAsync             manageElectronicAddress,
        IHttpClientFactory                        httpClientFactory,
        IUserRepositoryAsync                      userRepository,
        ILogger<ManageProductMarketingCenterAsync> logger)
    {
        ArgumentNullException.ThrowIfNull(contextService);          _contextService          = contextService;
        ArgumentNullException.ThrowIfNull(settingService);          _settingService          = settingService;
        ArgumentNullException.ThrowIfNull(blueBook);                _blueBook                = blueBook;
        ArgumentNullException.ThrowIfNull(managePersona);           _managePersona           = managePersona;
        ArgumentNullException.ThrowIfNull(managePerson);            _managePerson            = managePerson;
        ArgumentNullException.ThrowIfNull(manageUserLogin);         _manageUserLogin         = manageUserLogin;
        ArgumentNullException.ThrowIfNull(managePartyRelationship); _managePartyRelationship = managePartyRelationship;
        ArgumentNullException.ThrowIfNull(samlAttributeService);    _samlAttributeService    = samlAttributeService;
        ArgumentNullException.ThrowIfNull(manageElectronicAddress); _manageElectronicAddress = manageElectronicAddress;
        ArgumentNullException.ThrowIfNull(httpClientFactory);       _httpClientFactory       = httpClientFactory;
        ArgumentNullException.ThrowIfNull(userRepository);          _userRepository          = userRepository;
        ArgumentNullException.ThrowIfNull(logger);                  _logger                  = logger;
    }

    // ── Private support types ────────────────────────────────────────────────

    private sealed record MCSettings(string BaseUrl, string ApiSourceId, string Username, string Password);

    // ── Private helpers ──────────────────────────────────────────────────────

    private async Task<MCSettings> GetMCSettingsAsync(CancellationToken ct)
    {
        var settings = await _settingService.GetProductSettingAsync(ProductId, ct);
        string baseUrl  = settings.First(s => s.Name.Equals("APIENDPOINT",                StringComparison.OrdinalIgnoreCase)).Value;
        string sourceId = settings.First(s => s.Name.Equals("MarketingCenterApiSourceID", StringComparison.OrdinalIgnoreCase)).Value;
        string username = Encoding.UTF8.GetString(Convert.FromBase64String(
            settings.First(s => s.Name.Equals("APIUSERNAME", StringComparison.OrdinalIgnoreCase)).Value));
        string password = Encoding.UTF8.GetString(Convert.FromBase64String(
            settings.First(s => s.Name.Equals("APIPASSWORD", StringComparison.OrdinalIgnoreCase)).Value));
        return new MCSettings(baseUrl, sourceId, username, password);
    }

    /// <summary>
    /// Creates an <see cref="HttpClient"/> configured with Basic authentication.
    /// Callers are responsible for disposing the returned client (use <c>using</c>).
    /// </summary>
    private HttpClient BuildHttpClient(MCSettings s)
    {
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(s.BaseUrl);
        var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{s.Username}:{s.Password}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        return client;
    }

    /// <summary>
    /// Resolves the Marketing Center company instance ID via BlueBook
    /// using the product's UDM source code. Mirrors <c>GetProductCompanyInstanceId(_udmSourceCode)</c>.
    /// </summary>
    private async Task<string> GetMCCompanyIdAsync(ProductCallContext ctx, MCSettings s, CancellationToken ct)
    {
        var productDetail = await _settingService.GetProductDetailAsync(ProductId, ct);
        string udmSourceCode = productDetail?.UDMSourceCode ?? BlueBookProductConstants.MarketingCenter;
        var maps = await _blueBook.GetProductCompanyMappingAsync(
            ctx.EditorPersona.Organization.RealPageId, udmSourceCode, ct);
        return maps?.FirstOrDefault()?.CompanyInstanceSourceId ?? string.Empty;
    }

    private async Task<bool> IsSuperUserAsync(Persona userPersona, CancellationToken ct)
    {
        var rel = await _managePartyRelationship.GetPartyRelationshipAsync(
            userPersona.RealPageId, userPersona.Organization.RealPageId,
            roleTypeNameFrom: null, roleTypeNameTo: null, relationshipTypeName: "User Type", ct);
        return rel is not null
            && rel.RoleTypeFrom.Name.Equals("SuperUser", StringComparison.OrdinalIgnoreCase);
    }

    private async Task<bool> IsRegularUserNoEmailAsync(Persona userPersona, CancellationToken ct)
    {
        var rel = await _managePartyRelationship.GetPartyRelationshipAsync(
            userPersona.RealPageId, userPersona.Organization.RealPageId,
            roleTypeNameFrom: null, roleTypeNameTo: null, relationshipTypeName: "User Type", ct);
        return rel?.RoleTypeFrom.Name.Equals("USER (NO EMAIL)", StringComparison.OrdinalIgnoreCase) == true;
    }

    /// <summary>
    /// Returns the login name for the editor, used as the MC audit <c>username</c> parameter.
    /// Mirrors the legacy <c>GetLoginName()</c> helper.
    /// </summary>
    private async Task<string> GetLoginNameAsync(ProductCallContext ctx, CancellationToken ct)
    {
        var userLogin = await _manageUserLogin.GetUserLoginOnlyAsync(ctx.EditorPersona.RealPageId, ct);
        return userLogin?.LoginName ?? string.Empty;
    }

    private async Task<MC.MarketingCenterUserDetails?> GetUserDetailsInternalAsync(
        string baseUrl, string productUserId, HttpClient http, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(productUserId)) return null;
        try
        {
            var url = $"{baseUrl}/external/contact/{productUserId}/details";
            using var response = await http.GetAsync(url, ct);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(ct);
                return JsonConvert.DeserializeObject<MC.MarketingCenterUserDetails>(json);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "GetUserDetailsInternal failed for productUserId {ProductUserId}", productUserId);
        }
        return null;
    }

    private async Task<bool> CheckIfUserExistInProductAsync(string baseUrl, string email, HttpClient http, CancellationToken ct)
    {
        var url = $"{baseUrl}/external/contact/details?emailAddress={email}";
        using var response = await http.GetAsync(url, ct);
        return response.IsSuccessStatusCode;
    }

    private async Task<string> GetMCUniqueUserNameAsync(
        string baseUrl, string firstName, string lastName, HttpClient http, CancellationToken ct)
    {
        string stem  = $"{firstName.TrimWhiteSpace()[..1]}{lastName.TrimWhiteSpace().ToLower()}";
        int increment = 1;
        string email  = $"{stem}{increment}@noreply.com";
        while (await CheckIfUserExistInProductAsync(baseUrl, email, http, ct))
        {
            increment++;
            email = $"{stem}{increment}@noreply.com";
        }
        return email;
    }

    private async Task<bool> SetMarketingCenterUserStatusAsync(
        string baseUrl, string editorProductUserId, string mcUserId, bool isActive,
        HttpClient http, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(editorProductUserId))
        {
            _logger.LogWarning("SetMarketingCenterUserStatus: editorProductUserId is null/empty");
            return false;
        }
        if (string.IsNullOrEmpty(mcUserId) || mcUserId == "0") return false;
        try
        {
            var url = $"{baseUrl}/external/contact/{mcUserId}/status";
            var statusObj = new MC.MarketingCenterUserStatus
            {
                isActive           = isActive,
                isActiveUnifiedUser = isActive,
                auditUserId        = Convert.ToInt64(editorProductUserId)
            };
            using var response = await http.PutAsJsonAsync(url, statusObj, ct);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SetMarketingCenterUserStatus failed for mcUserId {McUserId}", mcUserId);
            return false;
        }
    }

    private static string ValidateAndReturnEmailAddress(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return email;
        try { _ = new System.Net.Mail.MailAddress(email); return email; }
        catch { return email; }
    }

    private static async Task<(string message, bool stopBatch)> ParseErrorPostingAsync(
        HttpResponseMessage response, string action, CancellationToken ct)
    {
        bool emailAlreadyExists = false;
        try
        {
            var content = await response.Content.ReadAsStringAsync(ct);
            var parsed  = JsonConvert.DeserializeObject<dynamic>(content);
            string errorText = parsed?.fieldErrors?.Error?.message;
            if (!string.IsNullOrEmpty(errorText)
                && errorText.Contains("duplicate") && errorText.Contains("emailAddress"))
            {
                emailAlreadyExists = true;
            }
        }
        catch { /* ignore parse failure, treat as generic error */ }

        string verb = action.Equals("Create", StringComparison.OrdinalIgnoreCase) ? "creating" : "updating";
        if (emailAlreadyExists)
            return ($"There was a problem {verb} the user. Email already exists", stopBatch: true);

        return ($"There was a problem {verb} the user.", stopBatch: false);
    }

    private static (List<string> toAdd, List<string> toRemove) GetRoleAssignmentChanges(
        List<string> desired, ListResponse currentRoles)
    {
        var desiredSet = (desired ?? new List<string>())
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Select(r => r.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var assignedNow = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (currentRoles?.Records != null)
            foreach (var pr in currentRoles.Records.OfType<RolesRightsAccessRight>().Where(r => r.IsAssigned))
                assignedNow.Add(pr.Id.ToString().Trim());

        return (desiredSet.Except(assignedNow, StringComparer.OrdinalIgnoreCase).ToList(),
                assignedNow.Except(desiredSet, StringComparer.OrdinalIgnoreCase).ToList());
    }

    private static (List<string> added, List<string> removed) GetRightAssignmentChanges(
        ListResponse currentRights, IList<int> desiredRights)
    {
        var desired = new HashSet<int>((desiredRights ?? new List<int>()).Distinct());
        var currentlyAssigned = new HashSet<int>();
        if (currentRights?.Records != null)
            foreach (var r in currentRights.Records.OfType<MCRight>().Where(r => r.IsAssigned))
                currentlyAssigned.Add(r.RightId);

        return (desired.Except(currentlyAssigned).Select(id => id.ToString()).ToList(),
                currentlyAssigned.Except(desired).Select(id => id.ToString()).ToList());
    }

    // ── GetRolesAsync ────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetRolesAsync(
        long editorPersonaId, long userPersonaId, RequestParameter datafilter,
        CancellationToken ct = default)
    {
        var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, userPersonaId, ProductId, ct);
        if (ctxError is not null) return ctxError;

        var s = await GetMCSettingsAsync(ct);
        using var http = BuildHttpClient(s);
        var result = new ListResponse();
        try
        {
            string marketingCompanyId = await GetMCCompanyIdAsync(ctx!, s, ct);
            if (string.IsNullOrEmpty(marketingCompanyId))
                return new ListResponse { IsError = true, ErrorReason = "Company Setup Error: Please Contact Support." };

            var url = $"{s.BaseUrl}/external/company/{marketingCompanyId}/contact/roles";
            _logger.LogDebug("GetRolesAsync GET {Url}", url);
            using var response = await http.GetAsync(url, ct);
            if (response.IsSuccessStatusCode)
            {
                var json      = await response.Content.ReadAsStringAsync(ct);
                var rolesList = JsonConvert.DeserializeObject<IList<MC.Role>>(json) ?? new List<MC.Role>();
                IList<ProductRole> list = rolesList.ToGBRoles() ?? new List<ProductRole>();

                if (userPersonaId != 0 && !string.IsNullOrEmpty(ctx!.ProductUserId))
                {
                    var mUser = await GetUserDetailsInternalAsync(s.BaseUrl, ctx.ProductUserId, http, ct);
                    if (mUser is null)
                        return new ListResponse { IsError = true, ErrorReason = "User not found" };
                    var pr = list.FirstOrDefault(a => a.ID == mUser.ContactRoleId.ToString());
                    if (pr is not null) pr.IsAssigned = true;
                }

                result = new ListResponse
                {
                    Records     = list.Cast<object>().ToList(),
                    TotalRows   = list.Count,
                    RowsPerPage = list.Count,
                    TotalPages  = 1,
                    ErrorReason = string.Empty
                };
            }
            else
            {
                result.IsError    = true;
                result.ErrorReason = CommonMessageConstants.RoleErrorMessage;
                _logger.LogError("GetRolesAsync HTTP {Status} for editorPersonaId {EditorPersonaId}", response.StatusCode, editorPersonaId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetRolesAsync error for editorPersonaId {EditorPersonaId}", editorPersonaId);
            result = new ListResponse { IsError = true, ErrorReason = CommonMessageConstants.RoleErrorMessage };
        }
        return result;
    }

    // ── GetPropertiesAsync ───────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetPropertiesAsync(
        long editorPersonaId, long userPersonaId, RequestParameter datafilter,
        CancellationToken ct = default)
    {
        var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, userPersonaId, ProductId, ct);
        if (ctxError is not null) return ctxError;

        var s = await GetMCSettingsAsync(ct);
        using var http = BuildHttpClient(s);
        var result = new ListResponse();
        try
        {
            // Use the BlueBook GetCompanyMap call with the MarketingCenter source ("LS"),
            // mirroring the sync _blueBook.GetCompanyMap(realPageId, booksId, source: "LS", domain: ...).
            var companyMaps = await _blueBook.GetCompanyMapAsync(
                ctx!.EditorPersona.Organization.RealPageId,
                ctx.EditorPersona.Organization.BooksCustomerMasterId,
                BlueBookProductConstants.MarketingCenter,
                ctx.EditorPersona.Organization.OrganizationDomain?.Name ?? string.Empty,
                cancellationToken: ct);

            string marketingCenterCompanyId = companyMaps?
                .FirstOrDefault(a => a.Source.Equals(BlueBookProductConstants.MarketingCenter, StringComparison.OrdinalIgnoreCase))?
                .CompanyInstanceSourceId ?? string.Empty;

            var url = $"{s.BaseUrl}/external/properties?companyId={marketingCenterCompanyId}";
            _logger.LogDebug("GetPropertiesAsync GET {Url}", url);
            using var response = await http.GetAsync(url, ct);
            if (response.IsSuccessStatusCode)
            {
                var json         = await response.Content.ReadAsStringAsync(ct);
                var propertyList = JsonConvert.DeserializeObject<IList<ProductPropertyMap>>(json) ?? new List<ProductPropertyMap>();
                IList<ProductProperty> list = propertyList.ToGBProperties() ?? new List<ProductProperty>();
                var allProperties = new Dictionary<string, bool>();

                if (userPersonaId != 0 && !string.IsNullOrEmpty(ctx.ProductUserId))
                {
                    var mUser = await GetUserDetailsInternalAsync(s.BaseUrl, ctx.ProductUserId, http, ct);
                    if (mUser is null)
                        return new ListResponse { IsError = true, ErrorReason = "User not found" };

                    if (mUser.AssignedProperties != null)
                    {
                        foreach (var p in mUser.AssignedProperties)
                        {
                            var pp = list.FirstOrDefault(a => a.ID == p.Id.ToString());
                            if (pp is not null)
                                pp.IsAssigned = true;
                            else
                                list.Add(new ProductProperty
                                {
                                    Name    = p.Name,
                                    ID      = p.Id.ToString(),
                                    IsAssigned = p.Active,
                                    State   = p.Address.StateCode,
                                    Street1 = p.Address.Address1,
                                    City    = p.Address.CityName,
                                    Zip     = p.Address.PostalCode
                                });
                        }
                    }
                    allProperties["IsAssignedNewPropertyByDefault"] = mUser.AssignNewProperty;
                }

                result = new ListResponse
                {
                    Records     = list.Cast<object>().ToList(),
                    TotalRows   = list.Count,
                    RowsPerPage = list.Count,
                    TotalPages  = 1,
                    ErrorReason = string.Empty,
                    Additional  = allProperties
                };
            }
            else
            {
                result.IsError    = true;
                result.ErrorReason = CommonMessageConstants.PropertyErrorMessage;
                _logger.LogError("GetPropertiesAsync HTTP {Status} for editorPersonaId {EditorPersonaId}", response.StatusCode, editorPersonaId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetPropertiesAsync error for editorPersonaId {EditorPersonaId}", editorPersonaId);
            result = new ListResponse { IsError = true, ErrorReason = CommonMessageConstants.PropertyErrorMessage };
        }
        return result;
    }

    // ── ManageMarketingCenterUserAsync ───────────────────────────────────────

    /// <inheritdoc/>
    public async Task<(string result, List<AdditionalParameters> additionalParameters)> ManageMarketingCenterUserAsync(
        long editorPersonaId, long userPersonaId,
        List<int> roleList, List<string> propertyList,
        bool isAssignedNewPropertyByDefault, CancellationToken ct = default)
    {
        var additionalParameters = new List<AdditionalParameters>();

        var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, userPersonaId, ProductId, ct);
        if (ctxError is not null) return (ctxError.ErrorReason, additionalParameters);

        var s = await GetMCSettingsAsync(ct);
        using var http = BuildHttpClient(s);

        try
        {
            var userPersona      = await _managePersona.GetPersonaAsync(userPersonaId, true, ct);
            var realPageId       = userPersona.RealPageId;
            var person           = await _managePerson.GetPersonAsync(realPageId, ct);
            var userLogin        = await _manageUserLogin.GetUserLoginOnlyAsync(realPageId, ct);
            var organizationList = await _manageUserLogin.ListOrganizationByEnterpriseUserIdAsync(realPageId, cancellationToken: ct);
            userPersona.Organization ??= organizationList.FirstOrDefault(i => i.PartyId == userPersona.OrganizationPartyId);

            var personaOrg = userPersona.Organization;
            bool isExternalUser = personaOrg?.RelationshipType?.Equals("User Type", StringComparison.OrdinalIgnoreCase) == true
                               && personaOrg?.RoleNameFrom?.Equals("External User", StringComparison.OrdinalIgnoreCase) == true;

            bool isRegularUserNoEmail = await IsRegularUserNoEmailAsync(userPersona, ct);

            // Resolve email address for the product user
            string userEmailAddress     = string.Empty;
            string userLeadEmailAddress = string.Empty;
            var addresses = await _manageElectronicAddress.ListElectronicAddressForPersonAsync(userLogin.RealPageId, null, ct);
            if (addresses != null)
            {
                userEmailAddress = addresses
                    .FirstOrDefault(a => a.AddressType.Equals("EMAIL", StringComparison.OrdinalIgnoreCase))?
                    .AddressString ?? string.Empty;
            }

            if (isRegularUserNoEmail)
            {
                userLeadEmailAddress = userEmailAddress;
                if (string.IsNullOrEmpty(userLeadEmailAddress))
                    return ("ManageMarketingCenterUser - Error. No Valid Notification Email Provided", additionalParameters);

                string existingProductUsername = ctx!.ProductUsername;
                if (!string.IsNullOrEmpty(existingProductUsername))
                    userEmailAddress = existingProductUsername;
                else if (!new EmailAddressAttribute().IsValid(userLogin.LoginName))
                    userEmailAddress = $"{userLogin.LoginName}@NoReply.com";
                else
                    userEmailAddress = userLogin.LoginName;
            }
            else
            {
                if (string.IsNullOrEmpty(userEmailAddress))
                    userEmailAddress = userLogin.LoginName;
            }

            userEmailAddress = ValidateAndReturnEmailAddress(userEmailAddress);

            // If user already has a product login, use it (preserves existing username)
            if (!string.IsNullOrEmpty(ctx!.ProductUsername))
                userEmailAddress = ctx.ProductUsername;

            bool isSuperUser = await IsSuperUserAsync(userPersona, ct);

            if (!isSuperUser && (roleList.Count == 0 || propertyList.Count == 0))
            {
                if (roleList.Count == 0)
                    await _settingService.UpdateProductStatusAsync(userPersonaId, ProductStatusSettingType, ProductId, (int)ProductBatchStatusType.Stop, ct);
                if (propertyList.Count == 0)
                    await _settingService.UpdateProductStatusAsync(userPersonaId, ProductStatusSettingType, ProductId, (int)ProductBatchStatusType.Stop, ct);
                return (ProductBatchStatusType.Stop.ToString(), additionalParameters);
            }

            // Get company-wide roles and properties (needed for super-user assignment and activity log)
            var rolesListResponse      = await GetRolesAsync(editorPersonaId, 0, null, ct);
            var companyRoleList        = rolesListResponse.Records?.Cast<ProductRole>().ToList() ?? new List<ProductRole>();
            var propertiesListResponse = await GetPropertiesAsync(editorPersonaId, 0, null, ct);
            var companyPropertyList    = propertiesListResponse.Records?.Cast<ProductProperty>().ToList() ?? new List<ProductProperty>();
            bool allPropertiesSelected = false;

            // Snapshot product user before update for activity log delta
            var productUserBeforeUpdate = !string.IsNullOrEmpty(ctx.ProductUserId)
                ? await GetUserDetailsInternalAsync(s.BaseUrl, ctx.ProductUserId, http, ct)
                : null;

            var mcProperties = new List<int>();
            int roleId       = 0;

            if (isSuperUser)
            {
                var corpOpsRole = companyRoleList.FirstOrDefault(a =>
                    a.Name.Equals("CORPORATE OPERATIONS", StringComparison.OrdinalIgnoreCase));
                if (corpOpsRole is null)
                {
                    await _settingService.UpdateProductStatusAsync(userPersonaId, ProductStatusSettingType, ProductId, (int)ProductBatchStatusType.Stop, ct);
                    return (ProductBatchStatusType.Stop.ToString(), additionalParameters);
                }
                roleId = Convert.ToInt32(corpOpsRole.ID);
                mcProperties.AddRange(companyPropertyList.Select(a => Convert.ToInt32(a.ID)));
                allPropertiesSelected = true;
            }
            else
            {
                if (!companyRoleList.Any(a => a.ID == roleList[0].ToString()))
                    return ($"Role id {roleList[0]} not found", additionalParameters);
                roleId = roleList[0];
                mcProperties.AddRange(propertyList.Select(p => Convert.ToInt32(p)));
            }

            string marketingCompanyId = await GetMCCompanyIdAsync(ctx, s, ct);
            if (string.IsNullOrEmpty(marketingCompanyId))
                return ("Company Setup Error: Please Contact Support.", additionalParameters);

            // For new users, generate a unique MC login name
            if (string.IsNullOrEmpty(ctx.ProductUsername))
            {
                if (!isRegularUserNoEmail)
                    userLeadEmailAddress = userLogin.LoginName;
                userEmailAddress = await GetMCUniqueUserNameAsync(s.BaseUrl, person.FirstName, person.LastName, http, ct);
                if (string.IsNullOrEmpty(userEmailAddress))
                    return ("An error occurred. Unable to get username.", additionalParameters);
            }

            string sourceid = string.IsNullOrEmpty(ctx.EditorProductUserId) ? s.ApiSourceId : ctx.EditorProductUserId;

            var mcUser = new MC.MarketingCenterUser
            {
                CompanyId              = Convert.ToInt32(marketingCompanyId),
                ContactRoleId          = roleId,
                ContactRoleName        = null,
                FirstName              = person.FirstName,
                LastName               = person.LastName,
                EmailAddress           = userEmailAddress,
                LeadEmailAddress       = userLeadEmailAddress,
                WelcomeEmailSent       = true,
                AssignUnassignProperties = true,
                AssignPropertyIds      = mcProperties,
                AssignNewProperty      = isAssignedNewPropertyByDefault
            };
            if (isSuperUser) mcUser.AssignNewProperty = true;

            if (string.IsNullOrEmpty(ctx.ProductUsername))
            {
                // ── Create new user ──────────────────────────────────────────
                mcUser.AssignAllProperties = allPropertiesSelected;
                await _settingService.UpdateProductStatusAsync(userPersonaId, ProductStatusSettingType, ProductId, (int)ProductBatchStatusType.Running, ct);

                var createUrl = $"{s.BaseUrl}/external/contact?sourceid={sourceid}";
                _logger.LogDebug("ManageMarketingCenterUserAsync POST {Url}", createUrl);
                using var createResponse = await http.PostAsJsonAsync(createUrl, mcUser, ct);
                if (createResponse.IsSuccessStatusCode)
                {
                    var createJson = await createResponse.Content.ReadAsStringAsync(ct);
                    var userResult = JsonConvert.DeserializeObject<dynamic>(createJson)!;
                    long newid     = userResult.id;

                    await _samlAttributeService.UpsertAttributeAsync(userPersonaId, ProductId, SamlAttributeEnum.productUsername, userEmailAddress, ct);
                    await _samlAttributeService.UpsertAttributeAsync(userPersonaId, ProductId, SamlAttributeEnum.UserId, newid.ToString(), ct);
                    await _settingService.UpdateProductStatusAsync(userPersonaId, ProductStatusSettingType, ProductId, (int)ProductBatchStatusType.Success, ct);

                    var migrateResult = await UpdateUsersMigrationStatusAsync(editorPersonaId,
                        new List<MigrateUser>
                        {
                            new MigrateUser
                            {
                                UnifiedLoginUserName = userEmailAddress,
                                UserId               = newid.ToString(),
                                UsingUnifiedLogin    = true,
                                LeadEmailAddress     = userLeadEmailAddress
                            }
                        }, ct);
                    if (!migrateResult.Status)
                        return (migrateResult.Message, additionalParameters);
                }
                else
                {
                    await _settingService.UpdateProductStatusAsync(userPersonaId, ProductStatusSettingType, ProductId, (int)ProductBatchStatusType.Error, ct);
                    var (msg, stopBatch) = await ParseErrorPostingAsync(createResponse, "Create", ct);
                    if (stopBatch)
                        await _settingService.UpdateProductStatusAsync(userPersonaId, ProductStatusSettingType, ProductId, (int)ProductBatchStatusType.Stop, ct);
                    return (stopBatch ? ProductBatchStatusType.Stop.ToString() : msg, additionalParameters);
                }
            }
            else
            {
                // ── Update existing user ─────────────────────────────────────
                if (!isSuperUser)
                {
                    var currentPropsResp = await GetPropertiesAsync(editorPersonaId, userPersonaId, null, ct);
                    var currentProps     = currentPropsResp.Records?.Cast<ProductProperty>().ToList() ?? new List<ProductProperty>();
                    var removePropertyList = new List<int>();

                    foreach (var pp in currentProps.Where(p => p.IsAssigned == true))
                    {
                        int ppId = Convert.ToInt32(pp.ID);
                        if (mcUser.AssignPropertyIds.Contains(ppId))
                            mcUser.AssignPropertyIds.Remove(ppId);
                        else
                            removePropertyList.Add(ppId);
                    }
                    mcUser.UnassignPropertyIds = removePropertyList;
                    mcUser.AssignAllProperties = allPropertiesSelected;
                }

                if (isExternalUser)
                {
                    mcUser.EmailAddress      = ctx.ProductUsername;
                    mcUser.LeadEmailAddress  = userEmailAddress;
                }

                string updateParam = allPropertiesSelected ? "assignAllProperties=true" : "unassignAllProperties=false";
                var updateUrl = $"{s.BaseUrl}/external/contact/{ctx.ProductUserId}?sourceid={sourceid}&{updateParam}";
                _logger.LogDebug("ManageMarketingCenterUserAsync PUT {Url}", updateUrl);
                using var updateResponse = await http.PutAsJsonAsync(updateUrl, mcUser, ct);
                if (updateResponse.IsSuccessStatusCode)
                {
                    await _settingService.UpdateProductStatusAsync(userPersonaId, ProductStatusSettingType, ProductId, (int)ProductBatchStatusType.Success, ct);
                    var updateJson = await updateResponse.Content.ReadAsStringAsync(ct);
                    var userResult = JsonConvert.DeserializeObject<dynamic>(updateJson)!;
                    long newid     = userResult.id;

                    await _samlAttributeService.UpsertAttributeAsync(userPersonaId, ProductId, SamlAttributeEnum.UserId, newid.ToString(), ct);

                    if (!string.IsNullOrEmpty(ctx.EditorProductUserId))
                        await SetMarketingCenterUserStatusAsync(s.BaseUrl, ctx.EditorProductUserId, newid.ToString(), true, http, ct);
                }
                else
                {
                    await _settingService.UpdateProductStatusAsync(userPersonaId, ProductStatusSettingType, ProductId, (int)ProductBatchStatusType.Error, ct);
                    var (msg, stopBatch) = await ParseErrorPostingAsync(updateResponse, "Update", ct);
                    if (stopBatch)
                        await _settingService.UpdateProductStatusAsync(userPersonaId, ProductStatusSettingType, ProductId, (int)ProductBatchStatusType.Stop, ct);
                    return (stopBatch ? ProductBatchStatusType.Stop.ToString() : msg, additionalParameters);
                }
            }

            // ── Build activity-log additional parameters ─────────────────────
            try
            {
                if (mcUser.ContactRoleId != productUserBeforeUpdate?.ContactRoleId)
                {
                    if (productUserBeforeUpdate != null)
                        additionalParameters.AddRange(companyRoleList
                            .Where(f => productUserBeforeUpdate.ContactRoleId == Convert.ToInt32(f.ID))
                            .Select(f => new AdditionalParameters
                            {
                                Key   = "Marketing Center Roles",
                                Value = RolesRemovedMsg.Replace("RoleName", f.Name)
                            }));

                    additionalParameters.AddRange(companyRoleList
                        .Where(f => mcUser.ContactRoleId == Convert.ToInt32(f.ID))
                        .Select(f => new AdditionalParameters
                        {
                            Key   = "Marketing Center Roles",
                            Value = RolesAssignMsg.Replace("RoleName", f.Name)
                        }));
                }

                if (mcUser.AssignPropertyIds != null)
                    additionalParameters.AddRange(companyPropertyList
                        .Where(f => mcUser.AssignPropertyIds.Contains(Convert.ToInt32(f.ID)))
                        .Select(f => new AdditionalParameters
                        {
                            Key   = "Marketing Center Properties",
                            Value = PropsAssignMsg.Replace("PropertyName", f.Name)
                        }));

                if (mcUser.UnassignPropertyIds != null)
                    additionalParameters.AddRange(companyPropertyList
                        .Where(f => mcUser.UnassignPropertyIds.Contains(Convert.ToInt32(f.ID)))
                        .Select(f => new AdditionalParameters
                        {
                            Key   = "Marketing Center Properties",
                            Value = PropsRemovedMsg.Replace("PropertyName", f.Name)
                        }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ManageMarketingCenterUserAsync: error building activity-log params for editorPersonaId {EditorPersonaId}", editorPersonaId);
            }

            return (string.Empty, additionalParameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ManageMarketingCenterUserAsync error for editorPersonaId {EditorPersonaId}", editorPersonaId);
            return ("There was a problem managing the user.", additionalParameters);
        }
    }

    // ── ChangeUserStatusAsync ────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<bool> ChangeUserStatusAsync(
        long editorPersonaId, string userName, string productUserId,
        CancellationToken ct = default)
    {
        var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, 0, ProductId, ct);
        if (ctxError is not null) return false;

        var s = await GetMCSettingsAsync(ct);
        using var http = BuildHttpClient(s);
        try
        {
            string marketingCompanyId = await GetMCCompanyIdAsync(ctx!, s, ct);
            if (string.IsNullOrEmpty(marketingCompanyId) || marketingCompanyId == "0")
            {
                _logger.LogError("ChangeUserStatusAsync: no company found for editorPersonaId {EditorPersonaId}", editorPersonaId);
                return false;
            }
            return await SetMarketingCenterUserStatusAsync(
                s.BaseUrl, ctx!.EditorProductUserId, productUserId, isActive: false, http, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ChangeUserStatusAsync error for editorPersonaId {EditorPersonaId} user {UserName}", editorPersonaId, userName);
            return false;
        }
    }

    // ── GetRolesCountAsync ───────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetRolesCountAsync(long editorPersonaId, CancellationToken ct = default)
    {
        var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, editorPersonaId, ProductId, ct);
        if (ctxError is not null) return ctxError;

        var s = await GetMCSettingsAsync(ct);
        using var http = BuildHttpClient(s);
        try
        {
            string marketingCompanyId = await GetMCCompanyIdAsync(ctx!, s, ct);
            if (string.IsNullOrEmpty(marketingCompanyId))
                return new ListResponse { IsError = true, ErrorReason = "Company Setup Error: Please Contact Support." };

            var url = $"{s.BaseUrl}/external/company/{marketingCompanyId}/roles";
            _logger.LogDebug("GetRolesCountAsync GET {Url}", url);
            using var response = await http.GetAsync(url, ct);
            if (response.IsSuccessStatusCode)
            {
                var json      = await response.Content.ReadAsStringAsync(ct);
                var rolesList = JsonConvert.DeserializeObject<IList<RolesRightsAccessRight>>(json) ?? new List<RolesRightsAccessRight>();
                return new ListResponse
                {
                    Records     = rolesList.Cast<object>().ToList(),
                    TotalRows   = rolesList.Count,
                    RowsPerPage = rolesList.Count,
                    TotalPages  = 1,
                    ErrorReason = string.Empty
                };
            }
            return new ListResponse { IsError = true, ErrorReason = "There was a problem getting the roles" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetRolesCountAsync error for editorPersonaId {EditorPersonaId}", editorPersonaId);
            return new ListResponse { IsError = true, ErrorReason = "There was a problem getting the roles" };
        }
    }

    // ── GetRightsAsync ───────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetRightsAsync(long editorPersonaId, CancellationToken ct = default)
    {
        var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, editorPersonaId, ProductId, ct);
        if (ctxError is not null) return ctxError;

        var s = await GetMCSettingsAsync(ct);
        using var http = BuildHttpClient(s);
        try
        {
            string marketingCompanyId = await GetMCCompanyIdAsync(ctx!, s, ct);
            if (string.IsNullOrEmpty(marketingCompanyId))
                return new ListResponse { IsError = true, ErrorReason = "Company Setup Error: Please Contact Support." };

            var url = $"{s.BaseUrl}/external/company/{marketingCompanyId}/rights";
            _logger.LogDebug("GetRightsAsync GET {Url}", url);
            using var response = await http.GetAsync(url, ct);
            if (response.IsSuccessStatusCode)
            {
                var json    = await response.Content.ReadAsStringAsync(ct);
                var rights  = JsonConvert.DeserializeObject<IList<Right>>(json) ?? new List<Right>();
                var mcRights = rights.ToGBRights() ?? new List<MCRight>();
                return new ListResponse
                {
                    Records     = mcRights.Cast<object>().ToList(),
                    TotalRows   = mcRights.Count,
                    RowsPerPage = mcRights.Count,
                    TotalPages  = 1,
                    ErrorReason = string.Empty
                };
            }
            return new ListResponse { IsError = true, ErrorReason = "There was a problem getting the rights" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetRightsAsync error for editorPersonaId {EditorPersonaId}", editorPersonaId);
            return new ListResponse { IsError = true, ErrorReason = "There was a problem getting the rights" };
        }
    }

    // ── DeleteRoleAsync ──────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> DeleteRoleAsync(long editorPersonaId, int roleId, CancellationToken ct = default)
    {
        var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, editorPersonaId, ProductId, ct);
        if (ctxError is not null) return ctxError;

        var s = await GetMCSettingsAsync(ct);
        using var http = BuildHttpClient(s);
        try
        {
            string marketingCompanyId = await GetMCCompanyIdAsync(ctx!, s, ct);
            if (string.IsNullOrEmpty(marketingCompanyId))
                return new ListResponse { IsError = true, ErrorReason = "Company Setup Error: Please Contact Support." };

            // Resolve role name for the activity log
            var rolesResponse = await GetRolesAsync(editorPersonaId, 0, null, ct);
            var roleName = rolesResponse.Records?.Cast<ProductRole>()
                .FirstOrDefault(r => r.ID == roleId.ToString())?.Name;

            string loginName = await GetLoginNameAsync(ctx!, ct);
            var url = $"{s.BaseUrl}/external/company/{marketingCompanyId}/roles/{roleId}?username={loginName}";
            _logger.LogDebug("DeleteRoleAsync DELETE {Url}", url);
            using var response = await http.DeleteAsync(url, ct);
            if (response.IsSuccessStatusCode)
            {
                // TODO: requires IManageUnifiedLoginAsync.PushToQueueAsync (not yet available on interface)
                return new ListResponse { ErrorReason = string.Empty };
            }
            _logger.LogError("DeleteRoleAsync HTTP {Status} for roleId {RoleId}", response.StatusCode, roleId);
            return new ListResponse { IsError = true, ErrorReason = "ManageMarketingCenterUser.DeleteRole - Unable to delete role" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteRoleAsync error for roleId {RoleId}", roleId);
            return new ListResponse { IsError = true, ErrorReason = ex.Message };
        }
    }

    // ── UpdateRoleStatusAsync ────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> UpdateRoleStatusAsync(
        long editorPersonaId, int roleId, bool isActive, CancellationToken ct = default)
    {
        var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, editorPersonaId, ProductId, ct);
        if (ctxError is not null) return ctxError;

        var s = await GetMCSettingsAsync(ct);
        using var http = BuildHttpClient(s);
        try
        {
            string marketingCompanyId = await GetMCCompanyIdAsync(ctx!, s, ct);
            if (string.IsNullOrEmpty(marketingCompanyId))
                return new ListResponse { IsError = true, ErrorReason = "Company Setup Error: Please Contact Support." };

            string loginName = await GetLoginNameAsync(ctx!, ct);
            var url = $"{s.BaseUrl}/external/company/{marketingCompanyId}/roles/{roleId}?active={isActive}&username={loginName}";
            _logger.LogDebug("UpdateRoleStatusAsync PATCH {Url}", url);
            using var request  = new HttpRequestMessage(new HttpMethod("PATCH"), url);
            using var response = await http.SendAsync(request, ct);
            if (response.IsSuccessStatusCode)
                return new ListResponse { ErrorReason = string.Empty };

            _logger.LogError("UpdateRoleStatusAsync HTTP {Status} for roleId {RoleId}", response.StatusCode, roleId);
            return new ListResponse { IsError = true, ErrorReason = "ManageMarketingCenterUser.UpdateRoleStatus - Unable to update role status" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateRoleStatusAsync error for roleId {RoleId}", roleId);
            return new ListResponse { IsError = true, ErrorReason = ex.Message };
        }
    }

    // ── GetRolesForRightIdAsync ──────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetRolesForRightIdAsync(
        long editorPersonaId, int rightId, CancellationToken ct = default)
    {
        var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, editorPersonaId, ProductId, ct);
        if (ctxError is not null) return ctxError;

        var s = await GetMCSettingsAsync(ct);
        using var http = BuildHttpClient(s);
        try
        {
            string marketingCompanyId = await GetMCCompanyIdAsync(ctx!, s, ct);
            if (string.IsNullOrEmpty(marketingCompanyId))
                return new ListResponse { IsError = true, ErrorReason = "Company Setup Error: Please Contact Support." };

            var url = $"{s.BaseUrl}/external/company/{marketingCompanyId}/rights/{rightId}/roles";
            _logger.LogDebug("GetRolesForRightIdAsync GET {Url}", url);
            using var response = await http.GetAsync(url, ct);
            if (response.IsSuccessStatusCode)
            {
                var json     = await response.Content.ReadAsStringAsync(ct);
                var roleList = JsonConvert.DeserializeObject<IList<RolesRightsAccessRight>>(json) ?? new List<RolesRightsAccessRight>();
                return new ListResponse
                {
                    Records     = roleList.Cast<object>().ToList(),
                    TotalRows   = roleList.Count,
                    RowsPerPage = roleList.Count,
                    TotalPages  = 1,
                    ErrorReason = string.Empty
                };
            }
            return new ListResponse { IsError = true, ErrorReason = "There was a problem getting the roles" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetRolesForRightIdAsync error for rightId {RightId}", rightId);
            return new ListResponse { IsError = true, ErrorReason = "There was a problem getting the roles" };
        }
    }

    // ── UpdateRolesForRightAsync ─────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> UpdateRolesForRightAsync(
        long editorPersonaId, int rightId, List<string> roleList, CancellationToken ct = default)
    {
        var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, editorPersonaId, ProductId, ct);
        if (ctxError is not null) return ctxError;

        var s = await GetMCSettingsAsync(ct);
        using var http = BuildHttpClient(s);
        try
        {
            string marketingCompanyId = await GetMCCompanyIdAsync(ctx!, s, ct);
            if (string.IsNullOrEmpty(marketingCompanyId))
                return new ListResponse { IsError = true, ErrorReason = "Company Setup Error: Please Contact Support." };

            // Compute role assignment changes (used for activity log)
            var currentRolesResp = await GetRolesForRightIdAsync(editorPersonaId, rightId, ct);
            if (!currentRolesResp.IsError && currentRolesResp.Records != null)
                currentRolesResp.Records = currentRolesResp.Records
                    .OfType<RolesRightsAccessRight>().Where(r => r.IsAssigned).Cast<object>().ToList();

            var (rolesToAdd, rolesToRemove) = GetRoleAssignmentChanges(roleList, currentRolesResp);

            string loginName = await GetLoginNameAsync(ctx!, ct);
            var url = $"{s.BaseUrl}/external/company/{marketingCompanyId}/rights/{rightId}/roles?username={loginName}";
            _logger.LogDebug("UpdateRolesForRightAsync PUT {Url}", url);
            using var response = await http.PutAsJsonAsync(url, roleList.Select(int.Parse).ToList(), ct);
            if (response.IsSuccessStatusCode)
            {
                // TODO: requires IManageUnifiedLoginAsync.PushToQueueAsync (not yet available on interface)
                return new ListResponse { ErrorReason = string.Empty };
            }
            _logger.LogError("UpdateRolesForRightAsync HTTP {Status} for rightId {RightId}", response.StatusCode, rightId);
            return new ListResponse { IsError = true, ErrorReason = "ManageMarketingCenterUser.UpdateRolesForRight - Unable to update role status" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateRolesForRightAsync error for rightId {RightId}", rightId);
            return new ListResponse { IsError = true, ErrorReason = ex.Message };
        }
    }

    // ── GetRightsForRoleIdAsync ──────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetRightsForRoleIdAsync(
        long editorPersonaId, int roleId, CancellationToken ct = default)
    {
        var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, editorPersonaId, ProductId, ct);
        if (ctxError is not null) return ctxError;

        var s = await GetMCSettingsAsync(ct);
        using var http = BuildHttpClient(s);
        try
        {
            string marketingCompanyId = await GetMCCompanyIdAsync(ctx!, s, ct);
            if (string.IsNullOrEmpty(marketingCompanyId))
                return new ListResponse { IsError = true, ErrorReason = "Company Setup Error: Please Contact Support." };

            // roleId == 0 → return all company rights; otherwise → rights for the specific role
            var url = roleId == 0
                ? $"{s.BaseUrl}/external/company/{marketingCompanyId}/rights"
                : $"{s.BaseUrl}/external/company/{marketingCompanyId}/roles/{roleId}/rights";
            _logger.LogDebug("GetRightsForRoleIdAsync GET {Url}", url);
            using var response = await http.GetAsync(url, ct);
            if (response.IsSuccessStatusCode)
            {
                var json    = await response.Content.ReadAsStringAsync(ct);
                var rights  = JsonConvert.DeserializeObject<IList<Right>>(json) ?? new List<Right>();
                var mcRights = rights.ToGBRights() ?? new List<MCRight>();
                return new ListResponse
                {
                    Records     = mcRights.Cast<object>().ToList(),
                    TotalRows   = mcRights.Count,
                    RowsPerPage = mcRights.Count,
                    TotalPages  = 1,
                    ErrorReason = string.Empty
                };
            }
            return new ListResponse { IsError = true, ErrorReason = "There was a problem getting the roles" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetRightsForRoleIdAsync error for roleId {RoleId}", roleId);
            return new ListResponse { IsError = true, ErrorReason = "There was a problem getting the roles" };
        }
    }

    // ── CreateNewMCRoleWithRightsAsync ───────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> CreateNewMCRoleWithRightsAsync(
        long editorPersonaId, MCRole mcRole, CancellationToken ct = default)
    {
        var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, editorPersonaId, ProductId, ct);
        if (ctxError is not null) return ctxError;

        var s = await GetMCSettingsAsync(ct);
        using var http = BuildHttpClient(s);
        try
        {
            string marketingCompanyId = await GetMCCompanyIdAsync(ctx!, s, ct);
            if (string.IsNullOrEmpty(marketingCompanyId))
                return new ListResponse { IsError = true, ErrorReason = "Company Setup Error: Please Contact Support." };

            string loginName = await GetLoginNameAsync(ctx!, ct);
            var url = $"{s.BaseUrl}/external/company/{marketingCompanyId}/roles?active={mcRole.Active}&username={loginName}";
            _logger.LogDebug("CreateNewMCRoleWithRightsAsync POST {Url}", url);
            using var response = await http.PostAsJsonAsync(url, mcRole, ct);
            if (response.IsSuccessStatusCode)
            {
                // TODO: requires IManageUnifiedLoginAsync.PushToQueueAsync (not yet available on interface)
                return new ListResponse { ErrorReason = string.Empty };
            }
            var json       = await response.Content.ReadAsStringAsync(ct);
            var roleErrors = JsonConvert.DeserializeObject<RoleErrors>(json);
            _logger.LogError("CreateNewMCRoleWithRightsAsync HTTP {Status}", response.StatusCode);
            return new ListResponse
            {
                IsError    = true,
                Additional = "RoleError",
                ErrorReason = !string.IsNullOrEmpty(roleErrors?.FieldErrors?.Error?.Message)
                    ? roleErrors.FieldErrors.Error.Message
                    : "Unable to create role"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateNewMCRoleWithRightsAsync error for editorPersonaId {EditorPersonaId}", editorPersonaId);
            return new ListResponse { IsError = true, ErrorReason = ex.Message };
        }
    }

    // ── UpdateMCRoleWithRightsAsync ──────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> UpdateMCRoleWithRightsAsync(
        long editorPersonaId, MCRole mcRole, CancellationToken ct = default)
    {
        var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, editorPersonaId, ProductId, ct);
        if (ctxError is not null) return ctxError;

        var s = await GetMCSettingsAsync(ct);
        using var http = BuildHttpClient(s);
        try
        {
            string marketingCompanyId = await GetMCCompanyIdAsync(ctx!, s, ct);
            if (string.IsNullOrEmpty(marketingCompanyId))
                return new ListResponse { IsError = true, ErrorReason = "Company Setup Error: Please Contact Support." };

            // Snapshot current rights and role metadata for activity-log delta computation
            var currentRightsResp = await GetRightsForRoleIdAsync(editorPersonaId, mcRole.Id, ct);
            if (!currentRightsResp.IsError && currentRightsResp.Records != null)
                currentRightsResp.Records = currentRightsResp.Records
                    .OfType<MCRight>().Where(r => r.IsAssigned).Cast<object>().ToList();

            var rolesResp    = await GetRolesAsync(editorPersonaId, 0, null, ct);
            var existingRole = rolesResp.Records?.Cast<ProductRole>().FirstOrDefault(r => r.ID == mcRole.Id.ToString());
            string oldRoleName = existingRole?.Name        ?? string.Empty;
            string oldRoleDesc = existingRole?.Description ?? string.Empty;

            var (addedRights, removedRights) = GetRightAssignmentChanges(currentRightsResp, mcRole.Rights);

            string loginName = await GetLoginNameAsync(ctx!, ct);
            var url = $"{s.BaseUrl}/external/company/{marketingCompanyId}/roles/{mcRole.Id}?username={loginName}";
            _logger.LogDebug("UpdateMCRoleWithRightsAsync PUT {Url}", url);
            using var response = await http.PutAsJsonAsync(url, mcRole, ct);
            if (response.IsSuccessStatusCode)
            {
                // TODO: requires IManageUnifiedLoginAsync.PushToQueueAsync (not yet available on interface)
                return new ListResponse { ErrorReason = string.Empty };
            }
            var json       = await response.Content.ReadAsStringAsync(ct);
            var roleErrors = JsonConvert.DeserializeObject<RoleErrors>(json);
            _logger.LogError("UpdateMCRoleWithRightsAsync HTTP {Status}", response.StatusCode);
            return new ListResponse
            {
                IsError    = true,
                Additional = "RoleError",
                ErrorReason = !string.IsNullOrEmpty(roleErrors?.FieldErrors?.Error?.Message)
                    ? roleErrors.FieldErrors.Error.Message
                    : "Unable to update role"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateMCRoleWithRightsAsync error for editorPersonaId {EditorPersonaId}", editorPersonaId);
            return new ListResponse { IsError = true, ErrorReason = ex.Message };
        }
    }

    // ── GetMigrationUsersAsync ───────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetMigrationUsersAsync(
        long editorPersonaId, RequestParameter datafilter, CancellationToken ct = default)
    {
        var response = new ListResponse { IsError = true, ErrorReason = "No Users." };

        var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, 0, ProductId, ct);
        if (ctxError is not null) { response.ErrorReason = ctxError.ErrorReason; return response; }

        var s = await GetMCSettingsAsync(ct);
        using var http = BuildHttpClient(s);
        try
        {
            string marketingCompanyId = await GetMCCompanyIdAsync(ctx!, s, ct);
            if (string.IsNullOrEmpty(marketingCompanyId) || marketingCompanyId == "0")
            {
                response.ErrorReason = "Company Setup Error: Please Contact Support.";
                return response;
            }
            int companyInstanceSourceId = Convert.ToInt32(marketingCompanyId);

            string filter    = "NonMigrated";
            int startRow     = 0;
            int resultPerRow = 1000;
            if (datafilter?.FilterBy?.ContainsKey("filter") == true)
                filter = datafilter.FilterBy["filter"];
            if (datafilter?.Pages != null)
            {
                startRow     = datafilter.Pages.StartRow;
                resultPerRow = datafilter.Pages.ResultsPerPage;
            }

            var url = $"{s.BaseUrl}/external/api/{companyInstanceSourceId}/users?filter-type={filter}&startRow={startRow}&resultsperpage={resultPerRow}";
            _logger.LogDebug("GetMigrationUsersAsync GET {Url}", url);
            using var apiResponse = await http.GetAsync(url, ct);
            if (!apiResponse.IsSuccessStatusCode)
            {
                _logger.LogError("GetMigrationUsersAsync HTTP {Status} for editorPersonaId {EditorPersonaId}", apiResponse.StatusCode, editorPersonaId);
                return response;
            }

            var json              = await apiResponse.Content.ReadAsStringAsync(ct);
            var migrationResponse = JsonConvert.DeserializeObject<MigrationResponse<IList<MigrationUser>>>(json);
            if (migrationResponse is null) return response;

            response.RowsPerPage = resultPerRow;
            response.ErrorReason = string.Empty;
            response.IsError     = false;
            response.TotalPages  = 1;
            response.Records     = migrationResponse.Data.Cast<object>().ToList();
            response.TotalRows   = migrationResponse.Data.Count();
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetMigrationUsersAsync error for editorPersonaId {EditorPersonaId}", editorPersonaId);
            return response;
        }
    }

    // ── UpdateUsersMigrationStatusAsync ─────────────────────────────────────

    /// <inheritdoc/>
    public async Task<MigrateResponse> UpdateUsersMigrationStatusAsync(
        long editorPersonaId, IList<MigrateUser> migrateUsers, CancellationToken ct = default)
    {
        var migrateResponse = new MigrateResponse { Status = false };

        var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, 0, ProductId, ct);
        if (ctxError is not null) { migrateResponse.Message = ctxError.ErrorReason; return migrateResponse; }

        var s = await GetMCSettingsAsync(ct);
        using var http = BuildHttpClient(s);
        try
        {
            string marketingCompanyId = await GetMCCompanyIdAsync(ctx!, s, ct);
            if (string.IsNullOrEmpty(marketingCompanyId) || marketingCompanyId == "0")
            {
                migrateResponse.Message = "Company Setup Error: Please Contact Support.";
                return migrateResponse;
            }
            int companyInstanceSourceId = Convert.ToInt32(marketingCompanyId);

            // Resolve lead email addresses for users that don't already have one
            foreach (var user in migrateUsers)
            {
                if (string.IsNullOrEmpty(user.LeadEmailAddress))
                {
                    var addresses = await _manageElectronicAddress.ListElectronicAddressForPersonAsync(
                        user.UnifiedLoginUserName, ctx!.EditorPersona.OrganizationPartyId, null, ct);
                    if (addresses != null)
                        user.LeadEmailAddress = addresses
                            .FirstOrDefault(a => a.AddressType.Equals("EMAIL", StringComparison.OrdinalIgnoreCase))?
                            .AddressString;
                }
            }

            var url = $"{s.BaseUrl}/external/api/{companyInstanceSourceId}/migrate-users";
            _logger.LogDebug("UpdateUsersMigrationStatusAsync POST {Url}", url);
            using var response = await http.PostAsJsonAsync(url, migrateUsers, ct);
            var responseContent = await response.Content.ReadAsStringAsync(ct);

            if (response.IsSuccessStatusCode)
            {
                var parsed = JsonConvert.DeserializeObject<MigrationResponse<MigrateResponse>>(responseContent);
                migrateResponse.Message = parsed?.Data?.Message;
                migrateResponse.Status  = parsed?.Data?.Status ?? false;
            }
            else
            {
                _logger.LogError("UpdateUsersMigrationStatusAsync HTTP {Status} for editorPersonaId {EditorPersonaId}", response.StatusCode, editorPersonaId);
                migrateResponse.Message = "Cannot update user status to migrated.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateUsersMigrationStatusAsync error for editorPersonaId {EditorPersonaId}", editorPersonaId);
            migrateResponse.Message = ex.Message;
        }
        return migrateResponse;
    }
}
