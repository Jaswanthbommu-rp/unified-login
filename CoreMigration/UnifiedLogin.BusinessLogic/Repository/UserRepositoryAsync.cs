using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.CacheHelper;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.DataAccess.Helper;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Cache;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enterprise;
using UnifiedLogin.SharedObjects.EnterpriseRole;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Extensions;
using UnifiedLogin.SharedObjects.Helper;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using SO = UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository;

/// <summary>
/// Async-first User Repository — pure data-access only.
/// Uses injected <see cref="IDbConnection"/> (Dapper) directly —
/// no <see cref="BaseRepository"/> inheritance, no <c>new</c> keyword.
/// <para>
/// Orchestration methods (CreateUser, UpdateUser, DisableUserProduct, etc.)
/// are NOT here — they belong in <c>UserService</c>.
/// </para>
/// </summary>
public sealed class UserRepositoryAsync : IUserRepositoryAsync
{
    #region Fields

    private readonly IDbConnection _db;
    private readonly ICacheService _cache;
    private readonly ILogger<UserRepositoryAsync> _logger;

    // Replaces: RPObjectCache TTL 180 s for navigation menu
    private static readonly CacheEntryOptions NavMenuCacheOptions = new() { ExpirationTimeInMinutes = 3 };

    #endregion

    #region Constructor

    public UserRepositoryAsync(
        IDbConnection db,
        ICacheService cache,
        ILogger<UserRepositoryAsync> logger)
    {
        _db     = db     ?? throw new ArgumentNullException(nameof(db));
        _cache  = cache  ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    #region Identity / login lookups

    /// <inheritdoc/>
    public async Task<SO.User> GetEnterpriseUserAsync(
        string enterpriseUserName, CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<SO.User>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetUserByLoginId,
                new { loginid = enterpriseUserName },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }

    /// <inheritdoc/>
    /// <remarks>Replaces: <c>GetEnterpriseUser(Guid)</c> which threw <see cref="NotImplementedException"/>.</remarks>
    public async Task<UserLogin> GetEnterpriseUserByRealPageIdAsync(
        Guid realPageId, CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<UserLogin>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetUserLogin,
                new { RealPageId = realPageId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }

    /// <inheritdoc/>
    public async Task<bool> CheckOrganizationAdminUserAsync(
        Guid userRealPageId, long orgPartyId, CancellationToken cancellationToken = default)
    {
        var result = await _db.QuerySingleOrDefaultAsync<int>(
            new CommandDefinition(
                StoredProcNameConstants.SP_EnterpriseCheckOrgAdmin,
                new { UserRealPageId = userRealPageId, OrgPartyId = orgPartyId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return result > 0;
    }

    /// <inheritdoc/>
    public async Task<UserLogin> UpdateUserLoginAsync(
        Guid realPageId, long organizationPartyId,
        string loginId = null, bool? isActive = null,
        string passwordHash = null, string passwordSalt = null,
        bool? isLocked = null, bool? isTainted = null,
        DateTime? fromDate = null, DateTime? thruDate = null,
        CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<UserLogin>(
            new CommandDefinition(
                StoredProcNameConstants.SP_UpdateUserLogin,
                new
                {
                    RealPageId    = realPageId,   LoginId       = loginId,
                    IsActive      = isActive,      PasswordHash  = passwordHash,
                    PasswordSalt  = passwordSalt,  IsLocked      = isLocked,
                    IsTainted     = isTainted,     FromDate      = fromDate,
                    ThruDate      = thruDate,      PartyId       = organizationPartyId
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }

    #endregion

    #region Profile

    /// <inheritdoc/>
    public async Task<StarterProfileOptionsResponse> GetStarterProfileOptionsAsync(
        string enterpriseUserName, CancellationToken cancellationToken = default)
    {
        var user = await GetEnterpriseUserAsync(enterpriseUserName, cancellationToken);

        return new StarterProfileOptionsResponse
        {
            EnterpriseUserName = user.LoginId,
            Firstname          = user.Firstname,
            Lastname           = user.Lastname,
            StandardJobTitles  = GetJobTitles(),
            PhoneTypes         = GetPhoneTypes()
        };
    }

    /// <inheritdoc/>
    /// <remarks>Pure constructor — no DB call; kept synchronous.</remarks>
    public SetStarterProfile SetStarterProfileOptions(StarterProfile starterProfileOptions)
        => new() { EnterpriseUserName = starterProfileOptions.EnterpriseUserName, IsSuccess = true };

    /// <inheritdoc/>
    public async Task<UserDetails> GetUserDetailsAsync(
        long? personaId = null, string userRealPageId = null,
        CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<UserDetails>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetUserDetails,
                new { personaId, userRealPageId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }

    /// <inheritdoc/>
    public async Task<long> GetSuperUserCountByOrganizationAsync(
        long? organizationPartyId, CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<long>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetSuperUsersCountByOrganization,
                new { OrganizationPartyId = organizationPartyId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }

    #endregion

    #region Employee / supervisor

    /// <inheritdoc/>
    public async Task<IUserEmployeeId> GetUserEmployeeIdAsync(
        long userLoginPersonaId, long organizationPartyId,
        CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<UserEmployee>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetEmployeeId,
                new { UserLoginPersonaId = userLoginPersonaId, OrganizationPartyId = organizationPartyId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> UpdateUserEmployeeIdAsync(
        IUserEmployeeId employeeIdDetail, CancellationToken cancellationToken = default)
    {
        if (employeeIdDetail.UserEmployeeId <= 0)
            return new RepositoryResponse();

        return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_UpdateEmployeeId,
                new { employeeIdDetail.UserEmployeeId, employeeIdDetail.EmployeeId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            ?? new RepositoryResponse();
    }

    /// <inheritdoc/>
    public async Task<UserInfoLite> GetSuperVisorInformationAsync(
        long userId, long organizationPartyId, CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<UserInfoLite>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetSuperVisorId,
                new { UserId = userId, OrgPartyId = organizationPartyId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }

    #endregion

    #region Delegate admin

    /// <inheritdoc/>
    public async Task<List<int>> GetDelegateAdminRoleTemplateAsync(
        long userLoginPersonaId, CancellationToken cancellationToken = default)
    {
        // Replaces: GetMany<dynamic> + foreach loop
        var rows = await _db.QueryAsync<dynamic>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetEnterpriseDelegateRole,
                new { UserLoginPersonaId = userLoginPersonaId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return rows.Select(item => (int)item.RoleTemplateId).ToList();
    }

    #endregion

    #region Navigation menu (cached)

    /// <inheritdoc/>
    public async Task<IList<NavigationMenuEntry>> GetNavigationMenuAsync(
        CancellationToken cancellationToken = default)
    {
        // Replaces: RPObjectCache.GetFromCache("navigationMenuEntries", 180, ...)
        return await _cache.GetOrSetAsync(
            "navigationMenuEntries",
            async ct =>
            {
                var rows = await _db.QueryAsync<NavigationMenuEntry>(
                    new CommandDefinition(
                        StoredProcNameConstants.SP_GetNavigationMenu,
                        commandType: CommandType.StoredProcedure,
                        cancellationToken: ct));
                return (IList<NavigationMenuEntry>)rows.ToList();
            },
            NavMenuCacheOptions,
            cancellationToken) ?? [];
    }

    /// <inheritdoc/>
    public async Task<IList<NavigationMenuRightEntry>> GetNavigationMenuRightsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrSetAsync(
            "navigationMenuRightEntries",
            async ct =>
            {
                var rows = await _db.QueryAsync<NavigationMenuRightEntry>(
                    new CommandDefinition(
                        StoredProcNameConstants.SP_GetNavigationMenuRights,
                        commandType: CommandType.StoredProcedure,
                        cancellationToken: ct));
                return (IList<NavigationMenuRightEntry>)rows.ToList();
            },
            NavMenuCacheOptions,
            cancellationToken) ?? [];
    }

    /// <inheritdoc/>
    public async Task<IList<NavigationMenuSetting>> GetNavigationMenuSettingsUnaccessableAsync(
        long partyId, CancellationToken cancellationToken = default)
    {
        var rows = await _db.QueryAsync<NavigationMenuSetting>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetNavigationMenuSettingUnaccessable,
                new { partyId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return rows.ToList();
    }

    #endregion

    #region AD / Azure

    /// <inheritdoc/>
    public async Task<AdUserDetail> GetAzureUserDetailsAsync(
        long userId, CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<AdUserDetail>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetADDetailsForUser,
                new { userId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }

    /// <inheritdoc/>
    public async Task<IList<EmployeeProductMapping>> GetEmployeeProductADGroupMappingAsync(
        long personaId, int productId, CancellationToken cancellationToken = default)
    {
        var rows = await _db.QueryAsync<EmployeeProductMapping>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetEmployeeProductADGroupMapping,
                new { ProductId = productId, PersonaId = personaId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return rows.ToList();
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> AddUpdateEmployeeProductADGroupMappingAsync(
        long personaId, int productId, int adGroupId,
        CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_AddUpdateEmployeeProductADGroupMapping,
                new { ProductId = productId, PersonaId = personaId, ADGroupId = adGroupId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            ?? new RepositoryResponse();
    }

    #endregion

    #region External user relationship

    /// <inheritdoc/>
    public async Task<ExternalUserRelationship> GetExternalUserRelationshipAsync(
        long? userLoginPersonaId, CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<ExternalUserRelationship>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetExternalUserRelationship,
                new { userLoginPersonaId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }

    #endregion

    #region Status

    /// <inheritdoc/>
    /// <remarks>
    /// Single SP call only.
    /// The "if disabled → iterate personas → remove products" loop that existed in
    /// the original <c>UpdateUserStatusByCompany</c> is orchestration — it belongs
    /// in <c>UserService.UpdateUserStatusByCompanyAsync</c>.
    /// </remarks>
    public async Task<RepositoryResponse> UpdateUserStatusByCompanyAsync(
        Guid realPageId, long organizationPartyId,
        int statusTypeId, DateTime fromDate, DateTime? thruDate,
        CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_UpdateUserStatusByCompany,
                new
                {
                    RealPageId          = realPageId,
                    OrganizationPartyId = organizationPartyId,
                    StatusTypeId        = statusTypeId,
                    FromDate            = fromDate,
                    StatusThruDate      = thruDate
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            ?? new RepositoryResponse();
    }

    #endregion

    #region Activity attempts

    /// <inheritdoc/>
    public async Task<ActivityAttempt> UpdateUserActivityAttemptsAsync(
        string enterpriseUserName, ActivityType activityType,
        UserDeviceDetails userDeviceDetails, long partyId,
        string authenticationServiceId = "",
        CancellationToken cancellationToken = default)
    {
        // Replaces: null guard + dynamic param in original
        userDeviceDetails ??= new UserDeviceDetails();

        return await _db.QuerySingleOrDefaultAsync<ActivityAttempt>(
            new CommandDefinition(
                StoredProcNameConstants.SP_UpdateActivityAttempt,
                new
                {
                    enterpriseUserName,
                    activityTypeId       = (int)activityType,
                    userDeviceDetails.BrowserName,
                    userDeviceDetails.BrowserType,
                    userDeviceDetails.IpAddress,
                    userDeviceDetails.IsMobile,
                    userDeviceDetails.Platform,
                    userDeviceDetails.Version,
                    userDeviceDetails.DeviceType,
                    userDeviceDetails.Timezone,
                    authenticationServiceId,
                    partyId
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }

    #endregion

    #region Bulk IDP update (data layer only)

    /// <inheritdoc/>
    /// <remarks>
    /// Returns the updated user IDs.
    /// Replaces: <c>ThirdPartyIdpBulkUpdate</c> which also contained audit
    /// logging (<c>ActivityLogForBulkIDPUpdate</c>) — that logic belongs in
    /// <c>UserService.ThirdPartyIdpBulkUpdateAsync</c>.
    /// </remarks>
    public async Task<IList<long>> ThirdPartyIdpBulkUpdateAsync(
        IList<long> userIds, bool isEnabled, long organizationPartyId,
        CancellationToken cancellationToken = default)
    {
        if (userIds.Count == 0) return [];

        var param = new DynamicParameters();
        param.Add("@OrganizationPartyId", organizationPartyId);
        param.Add("@UserIds", TableValueParamHelper.ConvertToTableValuedParameter(userIds.ToList(), "Enterprise.BigIntListType"));
        param.Add("@IsEnabled", isEnabled);

        var result = await _db.QueryAsync<long>(
            new CommandDefinition(
                StoredProcNameConstants.SP_UpdateUsersIDP,
                param,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<IList<UserActivityLogInfo>> GetUserProfilesByUserIdsAsync(
        long organizationPartyId, IList<long> userIds,
        CancellationToken cancellationToken = default)
    {
        var param = new DynamicParameters();
        param.Add("@OrganizationPartyId", organizationPartyId);
        param.Add("@UserIds", TableValueParamHelper.ConvertToTableValuedParameter(userIds.ToList(), "Enterprise.BigIntListType"));

        var result = await _db.QueryAsync<UserActivityLogInfo>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetUserProfileByUserIds,
                param,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return result.ToList();
    }

    #endregion

    #region Private — pure helpers (no DB)

    private static IList<Phone> GetPhoneTypes()
        => Enum.GetValues<PhoneType>()
               .Select(e => new Phone { PhoneTypeId = (int)e, PhoneType = e.ToEnumDescription() })
               .ToList<Phone>();

    private static IList<JobTitle> GetJobTitles()
        => Enum.GetValues<JobTitleType>()
               .Select(e => new JobTitle { JobTitleId = (int)e, Name = e.ToEnumDescription() })
               .ToList<JobTitle>();

    #endregion
}