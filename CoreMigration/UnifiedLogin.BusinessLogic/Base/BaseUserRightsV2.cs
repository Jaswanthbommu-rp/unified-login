using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Security.Claims;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.BusinessLogic.Services.User;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.UnifiedLogin;

namespace UnifiedLogin.BusinessLogic.Base;

/// <summary>
/// Async-first replacement for <c>BaseUserRights</c> (static) and the sync
/// <see cref="BaseUserRightsV2"/> that still used <c>new RPObjectCache()</c>.
/// <para>Key changes:</para>
/// <list type="bullet">
///   <item><c>new RPObjectCache()</c> replaced by injected <see cref="IMemoryCache"/>.</item>
///   <item>Sync <c>IUserRoleRightRepository</c> replaced by <see cref="IUserRoleRightRepositoryAsync"/>.</item>
///   <item>Sync <c>IProductInternalSettingRepository</c> replaced by <see cref="IProductInternalSettingRepositoryAsync"/>.</item>
///   <item>Broken sync <c>IUserQueryService</c> methods replaced by direct <see cref="IPersonaRepositoryAsync"/> +
///         <see cref="IProductRepositoryAsync"/> calls; <c>CheckOrganizationAdminUserAsync</c> via
///         the async <see cref="IUserQueryService"/>.</item>
///   <item>All methods return <c>Task</c> — the sync boundary lives in <c>UserClaimsAccessor</c>
///         via <c>.GetAwaiter().GetResult()</c> (safe in ASP.NET Core: no SynchronizationContext).</item>
/// </list>
/// </summary>
public sealed class BaseUserRightsV2 : IBaseUserRightsAsync
{
    #region Fields

    private readonly IPersonaRepositoryAsync _personaRepo;
    private readonly IProductRepositoryAsync _productRepo;
    private readonly IUserRoleRightRepositoryAsync _roleRightRepo;
    private readonly IProductInternalSettingRepositoryAsync _internalSettingRepo;
    private readonly IUserQueryService _userQueryService;   // async — CheckOrganizationAdminUserAsync only
    private readonly IMemoryCache _cache;
    private readonly ILogger<BaseUserRightsV2> _logger;

    // Cache TTLs — mirror original RPObjectCache durations
    private static readonly TimeSpan RoleRightsTtl = TimeSpan.FromSeconds(120);
    private static readonly TimeSpan UserRolesTtl = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan SettingsTtl = TimeSpan.FromSeconds(120);

    #endregion

    #region Constructor

    public BaseUserRightsV2(
        IPersonaRepositoryAsync personaRepo,
        IProductRepositoryAsync productRepo,
        IUserRoleRightRepositoryAsync roleRightRepo,
        IProductInternalSettingRepositoryAsync internalSettingRepo,
        IUserQueryService userQueryService,
        IMemoryCache cache,
        ILogger<BaseUserRightsV2> logger)
    {
        _personaRepo = personaRepo ?? throw new ArgumentNullException(nameof(personaRepo));
        _productRepo = productRepo ?? throw new ArgumentNullException(nameof(productRepo));
        _roleRightRepo = roleRightRepo ?? throw new ArgumentNullException(nameof(roleRightRepo));
        _internalSettingRepo = internalSettingRepo ?? throw new ArgumentNullException(nameof(internalSettingRepo));
        _userQueryService = userQueryService ?? throw new ArgumentNullException(nameof(userQueryService));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // IBaseUserRightsAsync
    // ════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public async Task<List<string>> GetUserRightsAsync(
        ClaimsPrincipal userPrincipal,
        DefaultUserClaim userClaim,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(userPrincipal);

        if (userClaim.IsRPEmployee
            && userClaim.OrganizationRealPageGuid != DefaultUserClaim.EmployeeCompanyRealPageId)
        {
            userClaim.ImpersonatedBy = userClaim.UserRealPageGuid;
        }

        var identity = (ClaimsIdentity)userPrincipal.Identity!;

        return userClaim.ImpersonatedBy == Guid.Empty
            ? await HandleDirectLoginAsync(identity, userClaim, cancellationToken)
            : await HandleImpersonationAsync(identity, userClaim, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<List<string>> GetImpersonatedUserRightsAsync(
        Guid impersonatedBy,
        DefaultUserClaim userClaims,
        CancellationToken cancellationToken = default)
    {
        // FIX: was _userQueryService.GetActivePersonaWithoutRights(Guid) — sync method that
        // no longer exists on the async IUserQueryService. Now uses IPersonaRepositoryAsync directly.
        var personaId = await _personaRepo.GetActivePersonaIdAsync(impersonatedBy, cancellationToken);
        if (personaId == 0) return [];

        var persona = await _personaRepo.GetPersonaAsync(personaId, withRights: false, cancellationToken);
        return await BuildImpersonatedRightsAsync(persona, userClaims, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<List<string>> GetImpersonatedUserRightsByPersonaAsync(
        Persona impersonateUserPersona,
        DefaultUserClaim userClaims,
        CancellationToken cancellationToken = default)
        => BuildImpersonatedRightsAsync(impersonateUserPersona, userClaims, cancellationToken);

    // ════════════════════════════════════════════════════════════════════════
    // Private — direct login
    // ════════════════════════════════════════════════════════════════════════

    private async Task<List<string>> HandleDirectLoginAsync(
        ClaimsIdentity identity,
        DefaultUserClaim userClaim,
        CancellationToken ct)
    {
        List<string> userRights = [];

        var companyRoles = await GetCompanyRolesAsync(
            userClaim, userClaim.OrganizationPartyId, userClaim.OrganizationRealPageGuid, ct);

        var roleIds = ExtractRoleIds(identity);
        var matched = companyRoles.Where(x => roleIds.Contains(x.RoleId));

        foreach (var r in matched)
            userRights.AddRange(r.UserRights.Select(x => x.RightNickName));

        // RP Employee — add AD Group rights
        if (userClaim.IsRPEmployee)
        {
            // FIX: was _userQueryService.ListPersona(Guid) — sync method no longer on async IUserQueryService.
            // Now uses IPersonaRepositoryAsync directly.
            var personas = await _personaRepo.ListPersonaAsync(userClaim.UserRealPageGuid, ct);
            var rpPersona = personas.FirstOrDefault(c =>
                c.Organization.RealPageId == DefaultUserClaim.EmployeeCompanyRealPageId);

            if (rpPersona is not null)
            {
                var adRights = await _roleRightRepo.GetADGroupRightsByPersonaIdAsync(
                    rpPersona.PersonaId, ct);
                userRights.AddRange(adRights.Select(x => x.RightNickName));
            }
        }

        return FinalizeRights(identity, userRights);
    }

    // ════════════════════════════════════════════════════════════════════════
    // Private — impersonation
    // ════════════════════════════════════════════════════════════════════════

    private async Task<List<string>> HandleImpersonationAsync(
        ClaimsIdentity identity,
        DefaultUserClaim userClaim,
        CancellationToken ct)
    {
        List<string> userRights = [];

        // FIX: was _userQueryService.ListPersona(Guid) sync.
        var personas = await _personaRepo.ListPersonaAsync(userClaim.ImpersonatedBy, ct);
        var rpEmployeePersona = personas.FirstOrDefault(c =>
            c.Organization.RealPageId == DefaultUserClaim.EmployeeCompanyRealPageId);

        if (rpEmployeePersona is null)
        {
            _logger.LogWarning(
                "No RP employee persona found for impersonatedBy={ImpersonatedBy}",
                userClaim.ImpersonatedBy);
            return [];
        }

        var adRights = await _roleRightRepo.GetADGroupRightsByPersonaIdAsync(
            rpEmployeePersona.PersonaId, ct);
        userRights.AddRange(
            adRights.Where(m => m.IsExcludeRightFromImpersonation != true)
                    .Select(x => x.RightNickName));

        bool isAdGroupMgmt = await IsUserManagementByADGroupEnabledAsync(ct);
        if (!isAdGroupMgmt)
        {
            var companyRoles = await GetCompanyRolesAsync(
                userClaim, userClaim.OrganizationPartyId, userClaim.OrganizationRealPageGuid, ct);

            var roleIds = ExtractRoleIds(identity);
            foreach (var r in companyRoles.Where(x => roleIds.Contains(x.RoleId)))
                userRights.AddRange(
                    r.UserRights
                     .Where(m => m.IsExcludeRightFromImpersonation != true)
                     .Select(x => x.RightNickName));
        }

        var distinctRights = userRights.Distinct().OrderBy(x => x).ToList();

        // FIX: was _userQueryService.CheckOrganizationAdminUser(Guid, long) sync.
        // CheckOrganizationAdminUserAsync EXISTS on the async IUserQueryService.
        bool isOrgAdmin = await _userQueryService.CheckOrganizationAdminUserAsync(
            userClaim.UserRealPageGuid, userClaim.OrganizationPartyId, ct);

        if (isOrgAdmin)
        {
            var impersonatorRights = await BuildImpersonatedRightsAsync(
                rpEmployeePersona, userClaim, ct);

            var persistRights = await _roleRightRepo.GetPersistRightsAsync(ct);

            foreach (var right in persistRights)
            {
                if (!distinctRights.Contains(right.RightName)
                    && impersonatorRights.Contains(right.RightName))
                    distinctRights.Add(right.RightName);
            }
        }

        return FinalizeRights(identity, distinctRights);
    }

    // ════════════════════════════════════════════════════════════════════════
    // Private — shared impersonated-rights builder
    // ════════════════════════════════════════════════════════════════════════

    private async Task<List<string>> BuildImpersonatedRightsAsync(
        Persona impersonateUserPersona,
        DefaultUserClaim userClaims,
        CancellationToken ct)
    {
        List<string> rights = [];

        var companyRoles = await GetCompanyRolesAsync(
            userClaims,
            impersonateUserPersona.OrganizationPartyId,
            impersonateUserPersona.Organization.RealPageId,
            ct);

        var userRoles = await GetUserRolesAsync(
            impersonateUserPersona.PersonaId,
            impersonateUserPersona.OrganizationPartyId,
            ct);

        var roleIds = userRoles.Select(c => c.RoleID).ToList();

        foreach (var r in companyRoles.Where(x => roleIds.Contains(x.RoleId)))
            rights.AddRange(r.UserRights.Select(x => x.RightNickName));

        return rights;
    }

    // ════════════════════════════════════════════════════════════════════════
    // Private — cached data access helpers
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// FIX: <c>new RPObjectCache().GetFromCache(...)</c> replaced by <see cref="IMemoryCache"/>.
    /// </summary>
    private async Task<bool> IsUserManagementByADGroupEnabledAsync(CancellationToken ct)
    {
        string cacheKey = $"productInternalSetting_{(int)ProductEnum.UnifiedPlatform}";

        var settings = await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = SettingsTtl;
            return await _internalSettingRepo.GetProductInternalSettingsAsync(
                (int)ProductEnum.UnifiedPlatform, ct);
        }) ?? [];

        return settings
            .FirstOrDefault(s => s.Name.Equals(
                "IsUserManagementByADGroup", StringComparison.OrdinalIgnoreCase))
            ?.Value == "1";
    }

    /// <summary>
    /// FIX: <c>_userQueryService.GetProductIdsByCompany(Guid)</c> sync method replaced by
    /// <see cref="IProductRepositoryAsync.GetProductIdsByCompanyAsync(Guid, CancellationToken)"/>.
    /// <c>new RPObjectCache()</c> replaced by <see cref="IMemoryCache"/>.
    /// </summary>
    private async Task<IList<UserRoleRights>> GetCompanyRolesAsync(
        DefaultUserClaim userClaim, long orgPartyId, Guid orgGuid,
        CancellationToken ct)
    {
        if (orgGuid == Guid.Empty) return [];

        // FIX: was _userQueryService.GetProductIdsByCompany(Guid) — sync, no longer on async interface.
        var productList = await _productRepo.GetProductIdsByCompanyAsync(orgGuid, ct);
        int productHash = productList?.GetHashCode() ?? 0;

        string cacheKey = $"getAllRoleRights_{orgPartyId}_{productHash}";

        return await _cache.GetOrCreateAsync<IList<UserRoleRights>>(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = RoleRightsTtl;
            // FIX: was IUserRoleRightRepository.GetAllRoleRights — now async
            return await _roleRightRepo.GetAllRoleRightsAsync(
                orgPartyId, productList, (int)ProductEnum.UnifiedPlatform);
        }) ?? [];
    }

    /// <summary>
    /// FIX: <c>new RPObjectCache()</c> replaced by <see cref="IMemoryCache"/>.
    /// Sync <c>IUserRoleRightRepository.ListRoleByPersona</c> replaced by async.
    /// </summary>
    private async Task<List<Role>> GetUserRolesAsync(
        long personaId, long orgPartyId, CancellationToken ct)
    {
        string cacheKey = $"getRoleByPersona_{orgPartyId}_{personaId}";

        return await _cache.GetOrCreateAsync<List<Role>>(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = UserRolesTtl;
            // FIX: was IUserRoleRightRepository.ListRoleByPersona — now async
            return await _roleRightRepo.ListRoleByPersonaAsync(
                (int)ProductEnum.UnifiedPlatform, personaId, orgPartyId);
        }) ?? [];
    }

    // ════════════════════════════════════════════════════════════════════════
    // Private — pure-logic statics (unchanged)
    // ════════════════════════════════════════════════════════════════════════

    private static List<long> ExtractRoleIds(ClaimsIdentity identity) =>
        identity.Claims
            .Where(p => p.Type.Equals("roleid", StringComparison.OrdinalIgnoreCase)
                     || p.Type.Equals(
                         "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
                         StringComparison.OrdinalIgnoreCase))
            .Select(item => int.TryParse(item.Value, out int id) ? (long?)id : null)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToList();

    private static List<string> FinalizeRights(
        ClaimsIdentity identity, IEnumerable<string> rights)
    {
        var distinct = rights.Distinct().OrderBy(x => x).ToList();
        identity.AddClaims(distinct.Select(a => new Claim("right", a)));
        return distinct;
    }
}