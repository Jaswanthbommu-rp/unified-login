using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.LogicAsync.Models;
using UnifiedLogin.BusinessLogic.LogicAsync.Product.Integrations;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.Product.RPDocumentManagement;
using UnifiedLogin.SharedObjects.Saml;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Product;

/// <summary>
/// Native-async implementation of <see cref="IManageProductRPDocumentManagementAsync"/>.
/// Replaces the stepping-stone <c>ManageProductRPDocumentManagement</c> wrapper.
/// <para>
/// Thread-safe: no mutable instance fields.  Every call resolves context, config, and
/// domain locally — safe for concurrent DI-scoped use.
/// </para>
/// </summary>
public sealed class ManageProductRPDocumentManagementAsync : IManageProductRPDocumentManagementAsync
{
    // ── Constants ─────────────────────────────────────────────────────────────
    private const int    ProductId             = (int)ProductEnum.RPDocumentManagement;
    private const string UdmSourceCode         = "DOC";   // BlueBookProductConstants.RPDocumentManagement
    private const string ConfigCacheKey        = "RPDM_ProductConfig";
    private const string DomainCacheKeyPrefix  = "RPDM_Domain_";
    private const string ClassifierCachePrefix = "RPDM_Classifier_";

    private static readonly TimeSpan ConfigCacheTtl     = TimeSpan.FromHours(1);
    private static readonly TimeSpan DomainCacheTtl     = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan ClassifierCacheTtl = TimeSpan.FromMinutes(5);

    // ── Dependencies ──────────────────────────────────────────────────────────
    private readonly IProductContextServiceAsync  _context;
    private readonly IProductRepositoryAsync      _productRepository;
    private readonly ISamlAttributeServiceAsync   _samlService;
    private readonly IManageBlueBookAsync         _blueBook;
    private readonly IManagePersonaAsync          _managePersona;
    private readonly IManagePersonAsync           _managePerson;
    private readonly IManageUserLoginAsync        _manageUserLogin;
    private readonly IManageContactMechanismAsync _manageContactMechanism;
    private readonly IHttpClientFactory           _httpClientFactory;
    private readonly IMemoryCache                 _cache;
    private readonly ILogger<ManageProductRPDocumentManagementAsync> _logger;

    public ManageProductRPDocumentManagementAsync(
        IProductContextServiceAsync  context,
        IProductRepositoryAsync      productRepository,
        ISamlAttributeServiceAsync   samlService,
        IManageBlueBookAsync         blueBook,
        IManagePersonaAsync          managePersona,
        IManagePersonAsync           managePerson,
        IManageUserLoginAsync        manageUserLogin,
        IManageContactMechanismAsync manageContactMechanism,
        IHttpClientFactory           httpClientFactory,
        IMemoryCache                 cache,
        ILogger<ManageProductRPDocumentManagementAsync> logger)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(productRepository);
        ArgumentNullException.ThrowIfNull(samlService);
        ArgumentNullException.ThrowIfNull(blueBook);
        ArgumentNullException.ThrowIfNull(managePersona);
        ArgumentNullException.ThrowIfNull(managePerson);
        ArgumentNullException.ThrowIfNull(manageUserLogin);
        ArgumentNullException.ThrowIfNull(manageContactMechanism);
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(logger);

        _context                = context;
        _productRepository      = productRepository;
        _samlService            = samlService;
        _blueBook               = blueBook;
        _managePersona          = managePersona;
        _managePerson           = managePerson;
        _manageUserLogin        = manageUserLogin;
        _manageContactMechanism = manageContactMechanism;
        _httpClientFactory      = httpClientFactory;
        _cache                  = cache;
        _logger                 = logger;
    }

    // ── Public interface ──────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetDomainAsync(long editorPersonaId, CancellationToken ct = default)
    {
        var (ctx, error) = await _context.GetUserContextAsync(editorPersonaId, 0, ProductId, ct);
        if (error is not null) return error;

        string domain = await ResolveDomainAsync(ctx!, ct);
        if (string.IsNullOrEmpty(domain) || IsDomainError(domain))
            return ProductManagerHelpers.ErrorResponse(domain.Length > 0 ? domain : "Could not resolve Document Director domain.");

        return new ListResponse { Additional = domain };
    }

    /// <inheritdoc/>
    public async Task<ListResponse> GetRolesAsync(
        long editorPersonaId, long userPersonaId,
        RequestParameter datafilter, CancellationToken ct = default)
    {
        var (ctx, error) = await _context.GetUserContextAsync(editorPersonaId, userPersonaId, ProductId, ct);
        if (error is not null) return error;

        return await GetRolesCoreAsync(ctx!, ct);
    }

    /// <inheritdoc/>
    public async Task<ListResponse> GetPropertyRolesAsync(
        long editorPersonaId, long userPersonaId,
        RequestParameter datafilter, CancellationToken ct = default)
    {
        var (ctx, error) = await _context.GetUserContextAsync(editorPersonaId, userPersonaId, ProductId, ct);
        if (error is not null) return error;

        var config = await GetProductConfigAsync(ct);
        string domain = await ResolveDomainAsync(ctx!, ct);
        if (IsDomainError(domain))
            return ProductManagerHelpers.ErrorResponse("There was a problem getting the role details");

        return await GetPropertyRolesCoreAsync(ctx!, config, domain, ct);
    }

    /// <inheritdoc/>
    public async Task<ListResponse> GetRoleClassifierDatasetAsync(
        long editorPersonaId, long userPersonaId,
        string roleId, RequestParameter datafilter, CancellationToken ct = default)
    {
        var (ctx, error) = await _context.GetUserContextAsync(editorPersonaId, userPersonaId, ProductId, ct);
        if (error is not null) return error;

        var config = await GetProductConfigAsync(ct);
        string domain = await ResolveDomainAsync(ctx!, ct);
        if (IsDomainError(domain))
            return ProductManagerHelpers.ErrorResponse("There was a problem getting the classifier");

        // Public entrypoint — no organizationPartyId → no classifier cache
        return await GetRoleClassifierDatasetCoreAsync(ctx!.ProductUserId, config, domain, roleId, organizationPartyId: null, ct);
    }

    /// <inheritdoc/>
    public async Task<(string result, List<AdditionalParameters> auditParams)> ManageRPDMUserAsync(
        long editorPersonaId, long userPersonaId,
        RolePropertyList rolePropertyEntityList, CancellationToken ct = default)
    {
        List<AdditionalParameters> auditParams = [];

        var (ctx, error) = await _context.GetUserContextAsync(editorPersonaId, userPersonaId, ProductId, ct);
        if (error is not null) return (error.ErrorReason, auditParams);

        var config = await GetProductConfigAsync(ct);
        string domain = await ResolveDomainAsync(ctx!, ct);
        if (IsDomainError(domain)) return (domain, auditParams);

        // ── Resolve user identity ──────────────────────────────────────────────
        var persona    = await _managePersona.GetPersonaAsync(userPersonaId, cancellationToken: ct);
        var realPageId = persona.RealPageId;

        var personTask  = _managePerson.GetPersonAsync(realPageId, ct);
        var loginTask   = _manageUserLogin.GetUserLoginOnlyAsync(realPageId, ct);
        var contactTask = _manageContactMechanism.ListContactMechanismForPersonAsync(realPageId, null, ct);
        await Task.WhenAll(personTask, loginTask, contactTask);

        var person   = personTask.Result;
        var login    = loginTask.Result;
        var contacts = contactTask.Result;

        // Sanitise names — remove non-alphanumeric characters
        person.FirstName = Regex.Replace(person.FirstName ?? "", @"[^A-Za-z0-9]+", "");
        person.LastName  = Regex.Replace(person.LastName  ?? "", @"[^A-Za-z0-9]+", "");

        string userEmailAddress = GetEmailAddress(contacts, login);
        bool   isSuperUser      = await _context.IsSuperUserAsync(ctx!.UserPersona!, ct);

        // ── Snapshot user state before update (for audit diff) ─────────────────
        RPDMUser? userBeforeUpdate = !string.IsNullOrEmpty(ctx.ProductUserId)
            ? await GetUserDetailsAsync(config, domain, ctx.ProductUserId, ct)
            : null;
        userBeforeUpdate ??= new RPDMUser
        {
            Roles  = [new RPDMUserRoles { Role = new RPDMScope(), Entity = new RPDMScope() }],
            Groups = []
        };

        // ── Determine username for new users ───────────────────────────────────
        string productUsername = ctx.ProductUsername;
        if (string.IsNullOrEmpty(ctx.ProductUserId))
        {
            string baseUsername = (
                person.FirstName.Substring(0, 1) +
                person.LastName.Substring(0, Math.Min(person.LastName.Length, 19))
            ).ToLower().Replace(" ", "");

            productUsername = baseUsername;
            int inc = 0;
            while (await CheckIfUserLoginIsUsedAsync(config, domain, productUsername, ct))
                productUsername = baseUsername + (++inc);
        }

        // ── Normalise role-property list ───────────────────────────────────────
        if (rolePropertyEntityList.RolePropertiesList is null &&
            (rolePropertyEntityList.RoleList?.Count > 0 ||
             rolePropertyEntityList.PropertyList?.Count > 0 ||
             rolePropertyEntityList.DepartmentList?.Count > 0))
        {
            if (rolePropertyEntityList.DepartmentList?.Count > 0 && rolePropertyEntityList.PropertyList?.Count > 0)
                rolePropertyEntityList.PropertyList!.AddRange(rolePropertyEntityList.DepartmentList);
            else if (rolePropertyEntityList.DepartmentList?.Count > 0)
                rolePropertyEntityList.PropertyList = rolePropertyEntityList.DepartmentList;

            rolePropertyEntityList.RolePropertiesList = rolePropertyEntityList.RoleList!
                .Select(rId => new PAMRolePropertyList { RoleId = rId, PropertyIds = rolePropertyEntityList.PropertyList })
                .ToList();
        }

        if (!isSuperUser &&
            (rolePropertyEntityList?.RolePropertiesList is null ||
             rolePropertyEntityList.RolePropertiesList.Count == 0))
        {
            _logger.LogDebug("{Action} - Missing role list. Count={Count}",
                "ManageRPDMUser", rolePropertyEntityList?.RolePropertiesList?.Count.ToString() ?? "0");
            return ("There was a problem creating the user. Missing required information.", auditParams);
        }

        // ── Fetch available roles from API ─────────────────────────────────────
        var rpdmResult = await GetFromApiAsync<RPDMResult<RPDMRole>>(config, domain, "/roles?isApi=true&sort=name", ct);
        if (rpdmResult is null)
        {
            _logger.LogError("{Action} - Could not retrieve roles from API", "ManageRPDMUser");
            return ("There was a problem creating the user. Missing role details", auditParams);
        }

        // ── Build RPDMUser payload ─────────────────────────────────────────────
        var manageUser = new RPDMUser
        {
            Domain    = domain,
            FirstName = person.FirstName,
            LastName  = person.LastName,
            Name      = productUsername,
            Email     = userEmailAddress,
            TimeZone  = "US/Central",
            Locale    = "en",
            Enabled   = true,
            Roles     = []
        };

        // ── Populate roles with parallel detail fetches (eliminates N+1) ──────
        try
        {
            if (rolePropertyEntityList?.RolePropertiesList?.Count > 0)
            {
                var matchedRoles = rolePropertyEntityList.RolePropertiesList
                    .Where(r => rpdmResult.Page.Exists(p => p.ID == r.RoleId))
                    .ToList();

                // Parallel fetch of role detail for each requested role
                var detailTasks = matchedRoles.Select(async role =>
                {
                    RPDMRole      roleDetail    = rpdmResult.Page.First(p => p.ID == role.RoleId);
                    RPDMRoleDetail? rpdmDetail  = await GetFromApiAsync<RPDMRoleDetail>(config, domain, $"/roles/{role.RoleId}", ct);
                    return (role, roleDetail, rpdmDetail);
                });

                foreach (var (role, roleDetail, rpdmDetail) in await Task.WhenAll(detailTasks))
                {
                    if (rpdmDetail?.Scope is not null && !string.IsNullOrEmpty(rpdmDetail.Scope.HRef))
                    {
                        // Role has a classifier scope — resolve properties
                        var classifier = await GetFromApiAsync<RPDMClassifier>(config, domain, rpdmDetail.Scope.HRef, ct);
                        if (classifier?.DataSet?.HRef is not null)
                        {
                            var datasetPath   = classifier.DataSet.HRef + "/values";
                            var dataSetResult = await GetFromApiAsync<RPDMResult<RPDMDataset>>(config, domain, datasetPath, ct);
                            if (dataSetResult?.Page.Count > 0)
                                InsertRoleDetails(role.PropertyIds, dataSetResult, roleDetail, rpdmDetail, manageUser);
                        }
                        else
                        {
                            // Scope present but no dataset — add role without entity
                            manageUser.Roles.Add(new RPDMUserRoles
                            {
                                Role = new RPDMScope { HRef = roleDetail.HRef, Id = roleDetail.ID, Name = roleDetail.Name }
                            });
                        }
                    }
                    else
                    {
                        // No scope — plain role assignment
                        manageUser.Roles.Add(new RPDMUserRoles
                        {
                            Role = new RPDMScope { HRef = roleDetail.HRef, Id = roleDetail.ID, Name = roleDetail.Name }
                        });
                    }
                }
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "{Action} - Role population error. {Message}", "ManageRPDMUser", ex.Message);
            await SetProductStatusAsync(userPersonaId, ProductBatchStatusType.Error, ct);
            return ($"There was a problem creating the user. {ex.Message}", auditParams);
        }

        // ── Ensure super-users have Domain Admin role ──────────────────────────
        if (isSuperUser && !manageUser.Roles.Any(r => r.Role.Name?.ToUpper() == "DOMAIN ADMIN"))
        {
            var adminRole = rpdmResult.Page.Find(p => p.Name?.ToUpper() == "DOMAIN ADMIN");
            if (adminRole is not null)
            {
                manageUser.Roles.Add(new RPDMUserRoles
                {
                    Role = new RPDMScope { HRef = adminRole.HRef, Id = adminRole.ID, Name = adminRole.Name }
                });
            }
        }

        // ── Create or update ───────────────────────────────────────────────────
        string insUpdResult;
        if (string.IsNullOrEmpty(ctx.ProductUserId))
        {
            try
            {
                await SetProductStatusAsync(userPersonaId, ProductBatchStatusType.Running, ct);

                string postUrl  = BuildUrl(config, domain, "/users/newuser");
                var    content  = Serialize(manageUser);
                _logger.LogDebug("{Action} - Creating user at {Url}", "ManageRPDMUser", postUrl);

                var postResponse = await PostToApiAsync(config, postUrl, content, ct);

                if (postResponse.IsSuccessStatusCode)
                {
                    var body = await postResponse.Content.ReadAsStringAsync(ct);
                    dynamic? result = JsonConvert.DeserializeObject<dynamic>(body);
                    string href  = (string)result!.target.href;
                    string newId = href.Split('/')[^1];

                    var newUser = await GetUserDetailsAsync(config, domain, newId, ct);
                    if (newUser is not null)
                    {
                        await _samlService.UpsertAttributesAsync(userPersonaId, ProductId,
                            new Dictionary<SamlAttributeEnum, string>
                            {
                                [SamlAttributeEnum.UserId]          = newId,
                                [SamlAttributeEnum.productUsername] = newUser.Name ?? productUsername
                            }, ct);

                        _logger.LogDebug("{Action} - User created. NewId={NewId} Login={Login}",
                            "ManageRPDMUser", newId, newUser.Name);
                        await SetProductStatusAsync(userPersonaId, ProductBatchStatusType.Success, ct);
                    }
                    insUpdResult = string.Empty;
                }
                else
                {
                    var errBody = await postResponse.Content.ReadAsStringAsync(ct);
                    _logger.LogError("{Action} - Create failed. Status={Status} Body={Body}",
                        "ManageRPDMUser", postResponse.StatusCode, errBody);
                    await SetProductStatusAsync(userPersonaId, ProductBatchStatusType.Error, ct);
                    insUpdResult = $"There was a problem creating the user. {errBody}";
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "{Action} - Create user errored. {Message}", "ManageRPDMUser", ex.Message);
                await SetProductStatusAsync(userPersonaId, ProductBatchStatusType.Error, ct);
                insUpdResult = $"There was a problem creating the user. {ex.Message}";
            }
        }
        else
        {
            insUpdResult = await UpdateRPDMUserCoreAsync(config, domain, ctx.ProductUserId, manageUser, userPersonaId, isUserProfile: false, ct);
        }

        // ── Build audit diff ───────────────────────────────────────────────────
        if (string.IsNullOrEmpty(insUpdResult))
        {
            var oldRoleIds   = userBeforeUpdate.Roles.Where(r => r.Role?.Id is not null).Select(r => r.Role!.Id!).ToHashSet();
            var newRoleIds   = manageUser.Roles.Where(r => r.Role?.Id is not null).Select(r => r.Role!.Id!).ToHashSet();
            var mergedRoles  = userBeforeUpdate.Roles.Concat(manageUser.Roles).ToList();

            foreach (string id in oldRoleIds.Except(newRoleIds))
            {
                var name = mergedRoles.FirstOrDefault(r => r.Role?.Id == id)?.Role?.Name ?? id;
                auditParams.Add(new AdditionalParameters
                {
                    Key   = "Document Director Roles",
                    Value = $"{name} was removed"
                });
            }
            foreach (string id in newRoleIds.Except(oldRoleIds))
            {
                var name = mergedRoles.FirstOrDefault(r => r.Role?.Id == id)?.Role?.Name ?? id;
                auditParams.Add(new AdditionalParameters
                {
                    Key   = "Document Director Roles",
                    Value = $"{name} was assigned"
                });
            }

            var oldEntityHrefs = userBeforeUpdate.Roles.Where(r => r.Entity?.Id is not null).Select(r => r.Entity!.HRef!).ToHashSet();
            var newEntityHrefs = manageUser.Roles.Where(r => r.Entity?.Id is not null).Select(r => r.Entity!.HRef!).ToHashSet();
            var mergedEntities = userBeforeUpdate.Roles.Select(r => r.Entity)
                .Concat(manageUser.Roles.Select(r => r.Entity))
                .Where(e => e is not null)
                .ToList();

            foreach (string href in oldEntityHrefs.Except(newEntityHrefs))
            {
                var name = mergedEntities.FirstOrDefault(e => e!.HRef == href)?.Name ?? href;
                auditParams.Add(new AdditionalParameters
                {
                    Key   = "Document Director Properties",
                    Value = $"{name} was removed"
                });
            }
            foreach (string href in newEntityHrefs.Except(oldEntityHrefs))
            {
                var name = mergedEntities.FirstOrDefault(e => e!.HRef == href)?.Name ?? href;
                auditParams.Add(new AdditionalParameters
                {
                    Key   = "Document Director Properties",
                    Value = $"{name} was assigned"
                });
            }
        }

        return (insUpdResult, auditParams);
    }

    /// <inheritdoc/>
    public async Task<string> UpdateRPDMUserProfileAsync(
        long editorPersonaId, long userPersonaId, CancellationToken ct = default)
    {
        var (ctx, error) = await _context.GetUserContextAsync(editorPersonaId, userPersonaId, ProductId, ct);
        if (error is not null) return error.ErrorReason;

        var    config = await GetProductConfigAsync(ct);
        string domain = await ResolveDomainAsync(ctx!, ct);
        if (IsDomainError(domain)) return domain;

        try
        {
            var persona    = await _managePersona.GetPersonaAsync(userPersonaId, cancellationToken: ct);
            var realPageId = persona.RealPageId;

            var personTask  = _managePerson.GetPersonAsync(realPageId, ct);
            var loginTask   = _manageUserLogin.GetUserLoginOnlyAsync(realPageId, ct);
            var contactTask = _manageContactMechanism.ListContactMechanismForPersonAsync(realPageId, null, ct);
            await Task.WhenAll(personTask, loginTask, contactTask);

            string email  = GetEmailAddress(contactTask.Result, loginTask.Result);
            var    person = personTask.Result;

            var manageUser = new RPDMUser
            {
                Domain    = domain,
                FirstName = person.FirstName,
                LastName  = person.LastName,
                Name      = ctx!.ProductUsername,
                Email     = email,
                TimeZone  = "US/Central",
                Locale    = "en",
                Enabled   = true
            };

            return await UpdateRPDMUserCoreAsync(config, domain, ctx.ProductUserId, manageUser, userPersonaId, isUserProfile: true, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "{Action} - Update profile errored. {Message}", "UpdateRPDMUserProfile", ex.Message);
            await SetProductStatusAsync(userPersonaId, ProductBatchStatusType.Error, ct);
            return $"There was a problem updating the user profile. {ex.Message}";
        }
    }

    /// <inheritdoc/>
    public async Task<string> UnassignUserAsync(
        long editorPersonaId, long userPersonaId,
        int productUserId = 0, CancellationToken ct = default)
    {
        var (ctx, error) = await _context.GetUserContextAsync(editorPersonaId, userPersonaId, ProductId, ct);
        if (error is not null) return error.ErrorReason;

        // Explicit productUserId override (migration portal path)
        string resolvedUserId = productUserId != 0 ? productUserId.ToString() : ctx!.ProductUserId;

        var    config = await GetProductConfigAsync(ct);
        string domain = await ResolveDomainAsync(ctx!, ct);
        if (IsDomainError(domain)) return domain;

        string disableUrl = BuildUrl(config, domain, $"/users/{resolvedUserId}/disable");
        _logger.LogDebug("{Action} - Disabling user at {Url}", "UnassignUser", disableUrl);

        var response = await PostToApiAsync(config, disableUrl, new StringContent(string.Empty), ct);

        if (!response.IsSuccessStatusCode)
        {
            if (productUserId != 0 && userPersonaId == 0)
            {
                var errMsg = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError("{Action} - Disable failed. Status={Status}", "UnassignUser", response.StatusCode);
                return $"There was a problem deleting Document Director user with editorPersona id - {editorPersonaId} - Error-{errMsg}.";
            }

            await SetProductStatusAsync(userPersonaId, ProductBatchStatusType.Error, ct);
            return "Error";
        }

        _logger.LogDebug("{Action} - User disabled. UserPersonaId={UserPersonaId}", "UnassignUser", userPersonaId);

        if (userPersonaId != 0 && productUserId == 0)
            await SetProductStatusAsync(userPersonaId, ProductBatchStatusType.Deleted, ct);

        return string.Empty;
    }

    /// <inheritdoc/>
    public async Task<ListResponse> GetMigrationUsersAsync(
        long editorPersonaId, RequestParameter datafilter, CancellationToken ct = default)
    {
        var listResponse = new ListResponse { IsError = true, ErrorReason = "No Users." };

        var (ctx, error) = await _context.GetUserContextAsync(editorPersonaId, 0, ProductId, ct);
        if (error is not null) { listResponse.ErrorReason = error.ErrorReason; return listResponse; }

        try
        {
            var    config = await GetProductConfigAsync(ct);
            string domain = await ResolveDomainAsync(ctx!, ct);
            if (IsDomainError(domain)) { listResponse.ErrorReason = domain; return listResponse; }

            // Migration uses CompanyInstanceSourceId — no attributes needed
            var maps = await _blueBook.GetProductCompanyMappingAsync(
                ctx!.EditorPersona.Organization.RealPageId, UdmSourceCode, ct);
            string? companyInstanceSourceId = maps?.FirstOrDefault()?.CompanyInstanceSourceId;

            if (companyInstanceSourceId is null)
            {
                _logger.LogError("{Action} - CompanyInstanceSourceId not found. EditorPersonaId={Id}", "GetMigrationUsers", editorPersonaId);
                listResponse.ErrorReason = "Company Setup Error: Please Contact Support.";
                return listResponse;
            }

            string filter         = "NonMigrated";
            int    resultsPerPage = 1000;
            int    pageNumber     = 1;

            if (datafilter is not null)
            {
                if (datafilter.FilterBy?.ContainsKey("filter") == true)
                    filter = datafilter.FilterBy["filter"];
                if (datafilter.Pages is not null)
                {
                    pageNumber    = datafilter.Pages.StartRow;
                    resultsPerPage = datafilter.Pages.ResultsPerPage;
                }
            }

            // Migration URL pattern differs: no /api/{domain} segment
            string migrationUrl = $"/api/unity/{companyInstanceSourceId}/users?filter={filter}&pageNumber={pageNumber}&resultsperpage={resultsPerPage}";
            _logger.LogDebug("{Action} - Fetching migration users. Url={Url}", "GetMigrationUsers", migrationUrl);

            var migrationList = await GetFromApiAsync<IList<RPDMigrationUser>>(config, domain, migrationUrl, ct, migrationMode: true);

            if (migrationList is null)
            {
                _logger.LogError("{Action} - No users returned by product. EditorPersonaId={Id}", "GetMigrationUsers", editorPersonaId);
                return listResponse;
            }

            var allUsers = migrationList.Select(x => new MigrationUser
            {
                CompanyInstanceSourceId = x.companyId,
                Email                   = x.Email,
                Extra                   = x.Extra,
                FirstName               = x.FirstName,
                LastActivity            = x.LastActivity,
                LastName                = x.LastName,
                MiddleName              = x.MiddleName,
                Phone                   = x.Phone,
                Status                  = x.isActive ? "true" : "false",
                Title                   = x.Title,
                UserId                  = x.UserId,
                Username                = x.Username,
                Properties              = x.Properties,
                isRealPageEmployee      = x.isRealPageEmployee,
                isAdminUser             = x.isAdminUser,
                isReadOnly              = x.isReadOnly,
                isMigratedUser          = x.isMigratedUser
            }).ToList();

            _logger.LogDebug("{Action} - {Count} migration users for EditorPersonaId={Id}", "GetMigrationUsers", allUsers.Count, editorPersonaId);

            listResponse.RowsPerPage = resultsPerPage;
            listResponse.ErrorReason = string.Empty;
            listResponse.IsError     = false;
            listResponse.TotalPages  = 1;
            listResponse.Records     = allUsers.Cast<object>().ToList();
            listResponse.TotalRows   = allUsers.Count;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "{Action} - Error. EditorPersonaId={Id}", "GetMigrationUsers", editorPersonaId);
            listResponse = new ListResponse { IsError = true, ErrorReason = ex.Message };
        }

        return listResponse;
    }

    /// <inheritdoc/>
    public async Task<MigrateResponse> UpdateUsersMigrationStatusAsync(
        long editorPersonaId, IList<MigrateUser> migrateUsers, CancellationToken ct = default)
    {
        var migrateResponse = new MigrateResponse { Status = false };

        var (ctx, error) = await _context.GetUserContextAsync(editorPersonaId, 0, ProductId, ct);
        if (error is not null) { migrateResponse.Message = error.ErrorReason; return migrateResponse; }

        var    config = await GetProductConfigAsync(ct);
        string domain = await ResolveDomainAsync(ctx!, ct);
        if (IsDomainError(domain))
        {
            _logger.LogError("{Action} - Domain error. EditorPersonaId={Id}", "UpdateUsersMigrationStatus", editorPersonaId);
            return new MigrateResponse { Status = false, Message = "Domain resolution error." };
        }

        try
        {
            var maps = await _blueBook.GetProductCompanyMappingAsync(
                ctx!.EditorPersona.Organization.RealPageId, UdmSourceCode, ct);
            string? companyInstanceSourceId = maps?.FirstOrDefault()?.CompanyInstanceSourceId;

            if (companyInstanceSourceId is null)
            {
                _logger.LogError("{Action} - CompanyInstanceSourceId not found. EditorPersonaId={Id}", "UpdateUsersMigrationStatus", editorPersonaId);
                migrateResponse.Message = "Company Setup Error: Please Contact Support.";
                return migrateResponse;
            }

            string patchUrl = config.ApiEndpoint.Replace("{{domain}}", domain)
                + $"/api/users/{companyInstanceSourceId}/migrate";

            _logger.LogDebug("{Action} - PATCH migration status at {Url}", "UpdateUsersMigrationStatus", patchUrl);

            using var client  = CreateClient(config);
            var       content = Serialize(migrateUsers);
            var patchResponse = await client.PatchAsync(patchUrl, content, ct);

            if (patchResponse.IsSuccessStatusCode)
            {
                var body   = await patchResponse.Content.ReadAsStringAsync(ct);
                var result = JsonConvert.DeserializeObject<MigrateResponse>(body);
                migrateResponse.Message = result?.Message;
                migrateResponse.Status  = result?.Status ?? false;
                _logger.LogDebug("{Action} - PATCH success", "UpdateUsersMigrationStatus");
            }
            else
            {
                _logger.LogError("{Action} - PATCH failed. Status={Status}", "UpdateUsersMigrationStatus", patchResponse.StatusCode);
                migrateResponse.Message = "Cannot update user status to migrated.";
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "{Action} - Error. EditorPersonaId={Id}", "UpdateUsersMigrationStatus", editorPersonaId);
            return new MigrateResponse { Status = false, Message = ex.Message };
        }

        return migrateResponse;
    }

    // ── Private — config & domain ─────────────────────────────────────────────

    /// <summary>
    /// Returns <see cref="RpdmConfig"/> from <see cref="IMemoryCache"/>, refreshing on miss.
    /// Zero Task allocation on cache hit — returns cached value synchronously via ValueTask.
    /// </summary>
    private async ValueTask<RpdmConfig> GetProductConfigAsync(CancellationToken ct)
    {
        if (_cache.TryGetValue(ConfigCacheKey, out RpdmConfig? hit))
            return hit!;

        var settings = await _productRepository.GetProductInternalSettingsAsync(ProductId, ct);

        string endpoint = settings.First(s => s.Name.ToUpper() == "APIENDPOINT").Value;
        string rawUser  = Encoding.UTF8.GetString(
            Convert.FromBase64String(settings.First(s => s.Name.ToUpper() == "APIUSERNAME").Value));
        string rawPass  = Encoding.UTF8.GetString(
            Convert.FromBase64String(settings.First(s => s.Name.ToUpper() == "APIPASSWORD").Value));

        var cfg = new RpdmConfig(endpoint, rawUser, rawPass);
        _cache.Set(ConfigCacheKey, cfg, ConfigCacheTtl);
        return cfg;
    }

    /// <summary>
    /// Resolves the Document Director domain for the editor's organisation.
    /// Reads <c>BooksUseUPFMId</c> from the UnifiedPlatform product settings to determine
    /// whether to use the DOMAIN ID BlueBook attribute or the CompanyInstanceSourceId directly.
    /// Result is cached per organisation for <see cref="DomainCacheTtl"/>.
    /// </summary>
    private async Task<string> ResolveDomainAsync(ProductCallContext ctx, CancellationToken ct)
    {
        string cacheKey = $"{DomainCacheKeyPrefix}{ctx.EditorPersona.Organization.RealPageId}";
        if (_cache.TryGetValue(cacheKey, out string? cached))
            return cached!;

        try
        {
            // Check UnifiedPlatform flag that switches domain resolution strategy
            var unifiedSettings = await _productRepository.GetProductInternalSettingsAsync(
                (int)ProductEnum.UnifiedPlatform, ct);
            bool useUPFMInstance = unifiedSettings?
                .FirstOrDefault(s => s.Name.Equals("BooksUseUPFMId", StringComparison.OrdinalIgnoreCase))
                ?.Value == "1";

            // Fetch company map with attributes for DOMAIN ID lookup
            var maps = await _blueBook.GetCompanyMapAsync(
                ctx.EditorPersona.Organization.RealPageId,
                ctx.EditorPersona.Organization.PartyId,
                UdmSourceCode,
                "",
                "companyInstance.attributes",
                cancellationToken: ct);

            var companyMap = maps?.FirstOrDefault();
            if (companyMap is null)
                return CommonMessageConstants.CompanyErrorMessage;

            string domain;
            if (!useUPFMInstance)
            {
                var attr = companyMap.CompanyInstance?[0]?.Attributes?
                    .Find(a => a.AttributeName.ToUpper() == "DOMAIN ID");
                domain = attr?.AttributeValue ?? string.Empty;
            }
            else
            {
                domain = companyMap.CompanyInstanceSourceId ?? string.Empty;
            }

            if (!string.IsNullOrEmpty(domain))
                _cache.Set(cacheKey, domain, DomainCacheTtl);

            return domain;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "{Action} - Domain resolution failed", "ResolveDomain");
            return CommonMessageConstants.CompanyErrorMessage;
        }
    }

    // ── Private — core operations ─────────────────────────────────────────────

    private async Task<ListResponse> GetRolesCoreAsync(ProductCallContext ctx, CancellationToken ct)
    {
        try
        {
            var    config = await GetProductConfigAsync(ct);
            string domain = await ResolveDomainAsync(ctx, ct);
            if (IsDomainError(domain))
                return ProductManagerHelpers.ErrorResponse("There was a problem getting the role details");

            var rpdmResult = await GetFromApiAsync<RPDMResult<RPDMRole>>(config, domain, "/roles?isApi=true&sort=name", ct);
            if (rpdmResult is null)
            {
                _logger.LogError("{Action} - rpdmResult is null", "GetRoles");
                return ProductManagerHelpers.ErrorResponse("There was a problem getting the role details");
            }

            // Parallel fetch of per-role detail — eliminates N+1
            List<ProductRole>? list = await GetAdditionalRoleDetailsAsync(config, domain, rpdmResult, ct);
            if (list is null)
            {
                _logger.LogError("{Action} - Role detail list is null (partial API failure)", "GetRoles");
                return ProductManagerHelpers.ErrorResponse("There was a problem getting the role details");
            }

            // Flag roles currently assigned to the user
            if (!string.IsNullOrEmpty(ctx.ProductUserId))
            {
                var user = await GetUserDetailsAsync(config, domain, ctx.ProductUserId, ct);
                if (user?.Roles?.Count > 0)
                {
                    var assignedIds = user.Roles
                        .Where(r => r.Role?.Id is not null)
                        .Select(r => r.Role!.Id!)
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    foreach (var pr in list.Where(pr => assignedIds.Contains(pr.ID ?? "")))
                        pr.IsAssigned = true;
                }
            }

            list = [.. list.OrderBy(p => p.Name)];
            return new ListResponse
            {
                Records     = list.Cast<object>().ToList(),
                TotalRows   = list.Count,
                RowsPerPage = list.Count,
                TotalPages  = 1,
                ErrorReason = ""
            };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "{Action} - Error. {Message}", "GetRoles", ex.Message);
            return ProductManagerHelpers.ErrorResponse("There was a problem getting the role details");
        }
    }

    private async Task<ListResponse> GetPropertyRolesCoreAsync(
        ProductCallContext ctx, RpdmConfig config, string domain, CancellationToken ct)
    {
        try
        {
            long orgPartyId = ctx.EditorPersona.Organization.PartyId;

            var rolesResponse = await GetRolesCoreAsync(ctx, ct);
            if (rolesResponse.TotalRows == 0) return rolesResponse;

            var list         = rolesResponse.Records.Cast<ProductRole>().ToArray();
            var enrichedList = new List<ProductRole>(list.Length);

            foreach (var item in list)
            {
                var pRole = item;
                if (!string.IsNullOrEmpty(item.Roletype))
                {
                    // Strip role-type suffix from display name
                    if (item.Name.Contains($"({item.Roletype})"))
                        pRole.Name = item.Name.Replace($"({item.Roletype})", "").Trim();

                    if (item.Roletype.Contains("site", StringComparison.OrdinalIgnoreCase))
                        pRole.Roletype = "Property";

                    // Load classifier properties with per-org/role caching
                    var propResponse = await GetRoleClassifierDatasetCoreAsync(
                        ctx.ProductUserId, config, domain, item.ID, orgPartyId, ct);
                    if (propResponse.Records.Count > 0)
                        pRole.propertiesList = propResponse.Records;
                }
                enrichedList.Add(pRole);
            }

            enrichedList.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
            return new ListResponse
            {
                Records     = enrichedList.Cast<object>().ToList(),
                TotalRows   = enrichedList.Count,
                RowsPerPage = enrichedList.Count,
                TotalPages  = 1,
                ErrorReason = ""
            };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "{Action} - Error. {Message}", "GetPropertyRoles", ex.Message);
            return ProductManagerHelpers.ErrorResponse("There was a problem getting the role details");
        }
    }

    private async Task<ListResponse> GetRoleClassifierDatasetCoreAsync(
        string productUserId, RpdmConfig config, string domain,
        string roleId, long? organizationPartyId, CancellationToken ct)
    {
        try
        {
            IList<ProductProperty>? list;

            if (organizationPartyId.HasValue)
            {
                // Cache classifier list per company+role for repeated GetPropertyRoles calls
                string cacheKey = $"{ClassifierCachePrefix}{organizationPartyId}_{roleId}";
                list = await _cache.GetOrCreateAsync(cacheKey, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = ClassifierCacheTtl;
                    return await FetchClassifierPropertiesAsync(config, domain, roleId, ct);
                });
            }
            else
            {
                list = await FetchClassifierPropertiesAsync(config, domain, roleId, ct);
            }

            if (list is null) return new ListResponse();

            // Merge user's current assignments into the IsAssigned flags
            if (!string.IsNullOrEmpty(productUserId))
            {
                var user = await GetUserDetailsAsync(config, domain, productUserId, ct);
                if (user?.Roles?.Count > 0)
                {
                    var userRoles = user.Roles;
                    foreach (var pp in list)
                    {
                        pp.IsAssigned = userRoles.Any(ur =>
                            string.Equals(pp.ID, ur.Entity?.Id, StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(roleId, ur.Role?.Id, StringComparison.OrdinalIgnoreCase));
                    }
                }
            }
            else
            {
                foreach (var pp in list) pp.IsAssigned = false;
            }

            list = [.. list.OrderBy(p => p.Name)];
            return new ListResponse
            {
                Records     = list.Cast<object>().ToList(),
                TotalRows   = list.Count,
                RowsPerPage = list.Count,
                TotalPages  = 1,
                ErrorReason = ""
            };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "{Action} - Error. {Message}", "GetRoleClassifierDataset", ex.Message);
            return ProductManagerHelpers.ErrorResponse("There was a problem getting the classifier");
        }
    }

    /// <summary>
    /// Fetches the list of properties for a role's classifier scope.
    /// Returns <c>null</c> when the role has no scope or the classifier has no dataset.
    /// </summary>
    private async Task<IList<ProductProperty>?> FetchClassifierPropertiesAsync(
        RpdmConfig config, string domain, string roleId, CancellationToken ct)
    {
        var roleDetail = await GetFromApiAsync<RPDMRoleDetail>(config, domain, $"/roles/{roleId}", ct);
        if (roleDetail?.Scope is null || string.IsNullOrEmpty(roleDetail.Scope.HRef))
            return null;

        var classifier = await GetFromApiAsync<RPDMClassifier>(config, domain, roleDetail.Scope.HRef, ct);
        if (classifier?.DataSet?.HRef is null) return null;

        var datasetResult = await GetFromApiAsync<RPDMResult<RPDMDataset>>(
            config, domain, classifier.DataSet.HRef + "/values", ct);

        return datasetResult?.Page?.Count > 0
            ? datasetResult.Page.ToGBProperties()
            : null;
    }

    /// <summary>
    /// Fetches per-role detail for every role in <paramref name="rpdmRoleResult"/> in parallel.
    /// Returns <c>null</c> if any single detail call fails, mirroring the sync behaviour.
    /// Eliminates the N+1 serial API calls in the original implementation.
    /// </summary>
    private async Task<List<ProductRole>?> GetAdditionalRoleDetailsAsync(
        RpdmConfig config, string domain,
        RPDMResult<RPDMRole> rpdmRoleResult, CancellationToken ct)
    {
        if (rpdmRoleResult.Page.Count == 0) return [];

        var tasks = rpdmRoleResult.Page.Select(async role =>
        {
            var detail = await GetFromApiAsync<RPDMRoleDetail>(config, domain, $"/roles/{role.ID}", ct);
            if (detail is null) return (ProductRole?)null;

            var pr = new ProductRole { ID = role.ID, Name = role.Name, Alias = role.HRef };
            if (detail.Scope is not null && !string.IsNullOrEmpty(detail.Scope.Name))
                pr.Roletype = detail.Scope.Name;
            return (ProductRole?)pr;
        });

        var results = await Task.WhenAll(tasks);
        // If any role detail call returned null, surface an error to the caller
        return results.Any(r => r is null) ? null : [.. results.Select(r => r!)];
    }

    private async Task<string> UpdateRPDMUserCoreAsync(
        RpdmConfig config, string domain, string productUserId,
        RPDMUser manageUser, long userPersonaId, bool isUserProfile, CancellationToken ct)
    {
        try
        {
            var currentUser = await GetUserDetailsAsync(config, domain, productUserId, ct);
            if (currentUser is null)
                return "There was a problem updating the user: user not found.";

            // Copy immutable fields from the current server record
            manageUser.Id       = productUserId;
            manageUser.TimeZone = currentUser.TimeZone;
            manageUser.Locale   = currentUser.Locale;
            manageUser.Photo    = currentUser.Photo;
            manageUser.Groups   = currentUser.Groups;

            if (isUserProfile)
                manageUser.Roles = currentUser.Roles;

            // Re-enable a disabled user before applying full update
            if (!currentUser.Enabled && !isUserProfile)
            {
                string enableUrl = BuildUrl(config, domain, $"/users/{productUserId}/enable");
                _logger.LogDebug("{Action} - Re-enabling disabled user {UserId}", "UpdateRPDMUser", productUserId);

                var enableContent  = Serialize(manageUser);
                var enableResponse = await PostToApiAsync(config, enableUrl, enableContent, ct);

                if (!enableResponse.IsSuccessStatusCode &&
                    enableResponse.StatusCode != System.Net.HttpStatusCode.NotModified)
                {
                    var errBody = await enableResponse.Content.ReadAsStringAsync(ct);
                    _logger.LogError("{Action} - Enable failed. {Body}", "UpdateRPDMUser", errBody);
                    return $"There was a problem updating the user. {errBody}";
                }
            }

            string updateUrl      = BuildUrl(config, domain, $"/users/{productUserId}");
            var    updateContent  = Serialize(manageUser);
            var    updateResponse = await PostToApiAsync(config, updateUrl, updateContent, ct);

            if (updateResponse.IsSuccessStatusCode ||
                updateResponse.StatusCode == System.Net.HttpStatusCode.NotModified)
            {
                await SetProductStatusAsync(userPersonaId, ProductBatchStatusType.Success, ct);
                return string.Empty;
            }

            var errorBody = await updateResponse.Content.ReadAsStringAsync(ct);
            _logger.LogError("{Action} - Update failed. {Body}", "UpdateRPDMUser", errorBody);
            return $"There was a problem updating the user. {errorBody}";
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "{Action} - Update errored. {Message}", "UpdateRPDMUser", ex.Message);
            return "There was a problem updating the user";
        }
    }

    // ── Private — HTTP helpers ────────────────────────────────────────────────

    /// <summary>
    /// Central GET helper replacing blocking <c>GetResultFromApi&lt;T&gt;</c>.
    /// Always appends <c>pageSize=9999</c> to the query string.
    /// </summary>
    /// <param name="migrationMode">
    /// When <c>true</c>, omits the <c>/api/{domain}</c> segment — used by migration-only endpoints.
    /// </param>
    private async Task<T?> GetFromApiAsync<T>(
        RpdmConfig config, string domain, string relativeUrl,
        CancellationToken ct, bool migrationMode = false)
        where T : class
    {
        // Append pageSize=9999 consistent with sync implementation
        relativeUrl = relativeUrl.Contains('?')
            ? relativeUrl + "&pageSize=9999"
            : relativeUrl + "?pageSize=9999";

        string url = migrationMode
            ? config.ApiEndpoint.Replace("{{domain}}", domain) + relativeUrl
            : BuildUrl(config, domain, relativeUrl);

        _logger.LogDebug("{Action} - GET {Url}", "GetFromApi", url);

        try
        {
            using var client   = CreateClient(config);
            using var response = await client.GetAsync(url, ct);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(ct);
                return JsonConvert.DeserializeObject<T>(json);
            }

            _logger.LogError("{Action} - Non-200. Url={Url} Status={Status}", "GetFromApi", url, response.StatusCode);
            return null;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "{Action} - Request failed. Url={Url}", "GetFromApi", url);
            return null;
        }
    }

    private async Task<HttpResponseMessage> PostToApiAsync(
        RpdmConfig config, string absoluteUrl, HttpContent content, CancellationToken ct)
    {
        using var client = CreateClient(config);
        return await client.PostAsync(absoluteUrl, content, ct);
    }

    /// <summary>Creates a per-request <see cref="HttpClient"/> with Basic Auth header set.</summary>
    private HttpClient CreateClient(RpdmConfig config)
    {
        var client = _httpClientFactory.CreateClient("RPDocumentManagement");
        var creds  = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{config.ApiUsername}:{config.ApiPassword}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", creds);
        return client;
    }

    // ── Private — utilities ───────────────────────────────────────────────────

    private async Task<RPDMUser?> GetUserDetailsAsync(RpdmConfig config, string domain, string userId, CancellationToken ct)
        => await GetFromApiAsync<RPDMUser>(config, domain, $"/users/{userId}", ct);

    private async Task<bool> CheckIfUserLoginIsUsedAsync(
        RpdmConfig config, string domain, string loginName, CancellationToken ct)
    {
        var result = await GetFromApiAsync<RPDMResult<RPDMDataset>>(
            config, domain, $"/users?s=username({loginName})", ct);
        return result?.Page?.Count > 0;
    }

    private Task SetProductStatusAsync(long userPersonaId, ProductBatchStatusType status, CancellationToken ct)
        => _productRepository.UpdateProductSettingProductStatusAsync(
               userPersonaId, ProductId, "ProductStatus", (int)status, ct);

    /// <summary>Constructs a standard RPDM API URL: <c>{endpoint}/api/{domain}{path}</c>.</summary>
    private static string BuildUrl(RpdmConfig config, string domain, string path)
        => config.ApiEndpoint.Replace("{{domain}}", domain) + $"/api/{domain}" + path;

    /// <summary>Serialises <paramref name="value"/> as UTF-8 JSON <see cref="StringContent"/>.</summary>
    private static StringContent Serialize<T>(T value)
        => new(JsonConvert.SerializeObject(value), Encoding.UTF8, "application/json");

    /// <summary>Returns <c>true</c> when the domain string is an error sentinel.</summary>
    private static bool IsDomainError(string domain)
        => string.IsNullOrEmpty(domain) || domain.StartsWith("There was a problem", StringComparison.Ordinal);

    private static string GetEmailAddress(IList<CommonAddress> contacts, UserLoginOnly userLogin)
    {
        string email = contacts
            .FirstOrDefault(a =>
                a.AddressType?.ToUpper() == "EMAIL" &&
                a.contactMechanismUsageType?.Name.ToUpper() == "PRIMARY")
            ?.AddressString ?? userLogin.LoginName;

        return ProductManagerHelpers.ValidateAndReturnEmailAddress(email);
    }

    /// <summary>
    /// Populates <paramref name="manageUser"/>.Roles with entries whose property IDs
    /// match entries in <paramref name="dataSetResults"/>.
    /// Pure static — no I/O, side-effect free apart from mutating the user's role list.
    /// </summary>
    private static void InsertRoleDetails(
        List<string>?          propertyIds,
        RPDMResult<RPDMDataset> dataSetResults,
        RPDMRole                roleDetail,
        RPDMRoleDetail          rpdmRoleDetail,
        RPDMUser                manageUser)
    {
        if (propertyIds is null) return;

        foreach (string id in propertyIds)
        {
            var dataset = dataSetResults.Page.Find(p => p.Id == id);
            if (dataset is null) continue;

            manageUser.Roles.Add(new RPDMUserRoles
            {
                Role   = new RPDMScope { HRef = roleDetail.HRef, Id = roleDetail.ID, Name = roleDetail.Name },
                Entity = new RPDMScope { Id = dataset.Id, HRef = dataset.HRef, Name = dataset.Name, Rel = rpdmRoleDetail.Type }
            });
        }
    }
}

/// <summary>Immutable Document Director config snapshot — safe to cache in <see cref="IMemoryCache"/>.</summary>
internal sealed record RpdmConfig(string ApiEndpoint, string ApiUsername, string ApiPassword);
