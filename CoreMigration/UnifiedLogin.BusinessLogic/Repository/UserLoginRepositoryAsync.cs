using Dapper;
using Microsoft.Extensions.Logging;
using System.Data;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.DataAccess.Model;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Helper;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Landing.Enum;

namespace UnifiedLogin.BusinessLogic.Repository;

/// <summary>
/// Async-first User Login Repository.
/// Uses injected <see cref="IDbConnection"/> (Dapper) directly —
/// no <see cref="BaseRepository"/> inheritance, no <c>new</c> keyword.
/// </summary>
public sealed class UserLoginRepositoryAsync : IUserLoginRepositoryAsync
{
    #region Fields

    private readonly IDbConnection _db;
    private readonly IOrganizationRepositoryAsync _organizationRepository;
    private readonly ILogger<UserLoginRepositoryAsync> _logger;

    #endregion

    #region Constructor

    /// <summary>
    /// Primary DI constructor — all dependencies injected, no <c>new</c>.
    /// </summary>
    public UserLoginRepositoryAsync(
        IDbConnection db,
        IOrganizationRepositoryAsync organizationRepository,
        ILogger<UserLoginRepositoryAsync> logger)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    #region IUserLoginRepositoryAsync Implementation

    /// <inheritdoc/>
    public async Task<RepositoryResponse> CreateUserLoginAsync(Guid realPageId, IUserLogin userLogin)
    {
        var param = new
        {
            realPageId,
            userLogin.LoginName,
            userLogin.FromDate,
            userLogin.ThruDate
        };

        return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_CreateUserLogin,
                param,
                commandType: CommandType.StoredProcedure));
    }

    /// <inheritdoc/>
    public async Task<UserLogin> GetUserLoginAsync(Guid realPageId, long orgPartyId)
    {
        var userLogin = await _db.QuerySingleOrDefaultAsync<UserLogin>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetUserLogin,
                new { realPageId },
                commandType: CommandType.StoredProcedure));

        if (userLogin is null) return null!;

        // Inline: replaces new CredentialRepository().GetIdentityProviderTypeByLoginName()
        // No ICredentialRepositoryAsync exists yet — direct Dapper call avoids the sync dependency.
        var authType = await _db.QuerySingleOrDefaultAsync<string>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetIdentityProviderTypeByLoginName,
                new { loginName = userLogin.LoginName },
                commandType: CommandType.StoredProcedure));

        userLogin.Is3rdPartyIDP = authType is not null && !new IdentityProviderType { AuthenticationType = authType }.IsLocal;

        var orgStatus = await GetUserOrganizationWithStatusAsync(
            userLogin.UserId, userLogin.LastLogin, orgPartyId, false);

        if (orgStatus is null) return null!;

        userLogin = MapCompanyStatusToUserStatus(userLogin, orgStatus);
        userLogin = UserLoginStatus.SetUserLoginStatus(userLogin);

        return userLogin;
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> UpdateUserLoginAsync(
        Guid realPageId,
        IUserLogin userLogin,
        long organizationPartyId)
    {
        var param = new
        {
            realPageId,
            userLogin.LoginName,
            userLogin.PasswordHash,
            userLogin.PasswordSalt,
            userLogin.FromDate,
            userLogin.ThruDate,
            PartyId = organizationPartyId
        };

        return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_UpdateUserLogin,
                param,
                commandType: CommandType.StoredProcedure));
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> UpdateLastLoginAsync(string username)
    {
        return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_UpdateLastLogin,
                new { LoginName = username },
                commandType: CommandType.StoredProcedure));
    }

    /// <inheritdoc/>
    public async Task<int> UpdateBulkUserStatusAsync(
        IList<Guid> userRealPageIdList,
        int statusTypeId,
        DateTime fromDate,
        DateTime? thruDate,
        long organizationPartyId)
    {
        // Replaces: repository.ExecuteStoredProcWithTvp<Guid>(tvp, userRealPageIdList, param)
        var table = new DataTable();
        table.Columns.Add("RealPageId", typeof(Guid));
        foreach (var id in userRealPageIdList)
            table.Rows.Add(id);

        var param = new DynamicParameters();
        param.Add("StatusTypeId", statusTypeId);
        param.Add("FromDate", fromDate);
        param.Add("StatusThruDate", thruDate);
        param.Add("PartyId", organizationPartyId);
        param.Add("RealPageId", table.AsTableValuedParameter("dbo.PARTYGUID"));

        return await _db.ExecuteAsync(
            new CommandDefinition(
                StoredProcNameConstants.SP_UpdateBulkUserStatus,
                param,
                commandType: CommandType.StoredProcedure));
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> LinkIdentityProviderToUserLoginAsync(
        long personaId,
        long userId,
        int contactMechanismId)
    {
        var param = new
        {
            PersonaId = personaId,
            UserId = userId,
            ContactMechanismId = contactMechanismId
        };

        return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_LinkIdentityProviderToUserLogin,
                param,
                commandType: CommandType.StoredProcedure));
    }

    /// <inheritdoc/>
    public async Task<IList<UserOrganization>> ListOrganizationByLoginNameAsync(
        string loginName,
        Guid? organizationRealPageId = null)
    {
        var result = await _db.QueryAsync<UserOrganization>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListOrganizationByLoginName,
                new { LoginName = loginName, OrganizationRealPageId = organizationRealPageId },
                commandType: CommandType.StoredProcedure));

        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<IList<UserOrganization>> ListAllOrganizationByLoginNameAsync(string loginName)
    {
        var result = await _db.QueryAsync<UserOrganization>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListAllOrganizationByLoginName,
                new { LoginName = loginName },
                commandType: CommandType.StoredProcedure));

        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<ActivityAttemptDetails> GetActivityAttemptExceedsAsync(
        long organizationPartyId,
        string enterpriseUserName,
        int activityId)
    {
        return await _db.QuerySingleOrDefaultAsync<ActivityAttemptDetails>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetActivityAttemptExceeds,
                new { enterpriseUserName, activityTypeId = activityId, partyId = organizationPartyId },
                commandType: CommandType.StoredProcedure));
    }

    /// <inheritdoc/>
    public async Task<UserLoginOnly> GetUserLoginOnlyAsync(string enterpriseUserName)
        => await GetUserLoginOnlyCoreAsync(enterpriseUserName, 0, Guid.Empty);

    /// <inheritdoc/>
    public async Task<UserLoginOnly> GetUserLoginOnlyAsync(long userId)
        => await GetUserLoginOnlyCoreAsync(null, userId, Guid.Empty);

    /// <inheritdoc/>
    public async Task<UserLoginOnly> GetUserLoginOnlyAsync(Guid realPageId)
        => await GetUserLoginOnlyCoreAsync(null, 0, realPageId);

    /// <inheritdoc/>
    public async Task<IList<OrganizationStatus>> ListOrganizationWithoutStatusByUserIdAsync(long userId)
    {
        var result = await _db.QueryAsync<OrganizationStatus>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListOrganizationStatusByUserId,
                new { UserId = userId },
                commandType: CommandType.StoredProcedure));

        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<IList<Organization>> ListOrganizationByEnterpriseUserIdAsync(
        Guid realPageId,
        string relationshipType)
    {
        var param = new
        {
            RealPageId = realPageId,
            RelationshipTypeName = relationshipType
        };

        // Sequential: _db is the same IDbConnection instance shared with OrganizationRepositoryAsync.
        // Running _db.QueryAsync concurrently with the org-type / org-domain lookups (which also
        // use the same connection on a cache miss) raises "connection does not support
        // MultipleActiveResultSets". Await each query in turn; cache-backed lookups are fast
        // after the first miss so the performance impact is minimal.
        var orgList = (await _db.QueryAsync<Organization>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListOrganizationByRealPageId,
                param,
                commandType: CommandType.StoredProcedure))).ToList();

        var orgTypes = await _organizationRepository.ListOrganizationTypeAsync();
        var orgDomains = await _organizationRepository.ListOrganizationDomainAsync();

        foreach (var org in orgList)
        {
            var orgType = orgTypes.FirstOrDefault(t => t.OrganizationTypeId == org.OrganizationTypeId);
            org.organizationType = orgType is not null
                ? new OrganizationType { Name = orgType.Name, OrganizationTypeId = orgType.OrganizationTypeId, CreateDate = orgType.CreateDate }
                : new OrganizationType();

            var orgDomain = orgDomains.FirstOrDefault(d => d.OrganizationDomainId == org.OrganizationDomainId);
            org.OrganizationDomain = orgDomain is not null
                ? new OrganizationDomain { OrganizationDomainId = orgDomain.OrganizationDomainId, Name = orgDomain.Name, CreateDate = orgDomain.CreateDate }
                : new OrganizationDomain();
        }

        return orgList;
    }

    /// <inheritdoc/>
    public async Task<long> GetPrimaryOrgIdByUserIdAsync(long userId)
    {
        var org = await GetPrimaryOrgWithoutStatusByUserIdAsync(userId);
        return org?.PartyId ?? 0L;
    }

    /// <inheritdoc/>
    public async Task<OrganizationStatus> GetPrimaryOrgWithoutStatusByUserIdAsync(long userId)
    {
        var list = await ListOrganizationWithoutStatusByUserIdAsync(userId);
        return list.FirstOrDefault(p => p.PrimaryOrganization);
    }

    /// <inheritdoc/>
    public async Task<OrganizationStatus> GetUserOrganizationWithStatusAsync(
        long userId,
        DateTime? lastLogin,
        long organizationPartyId,
        bool getPrimaryOrg)
    {
        var organizationList = await ListOrganizationWithoutStatusByUserIdAsync(userId);

        var orgStatus = getPrimaryOrg
            ? organizationList.FirstOrDefault(p => p.PrimaryOrganization)
            : organizationList.FirstOrDefault(p => p.PartyId == organizationPartyId);

        orgStatus?.SetOrganizationStatus(lastLogin is not null);

        return orgStatus;
    }

    /// <inheritdoc/>
    public async Task<IList<string>> GetBlacklistedDomainsAsync()
    {
        var result = await _db.QueryAsync<string>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetBlacklistedDomains,
                commandType: CommandType.StoredProcedure));

        return result.ToList();
    }

    #endregion

    #region Private Helpers

    private async Task<UserLoginOnly> GetUserLoginOnlyCoreAsync(
        string? enterpriseUserName,
        long userId,
        Guid realPageId)
    {
        object param = enterpriseUserName is not null
            ? new { EnterpriseUserName = enterpriseUserName }
            : userId != 0
                ? new { UserId = userId }
                : (object)new { RealPageId = realPageId };

        return await _db.QuerySingleOrDefaultAsync<UserLoginOnly>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetUserLoginOnly,
                param,
                commandType: CommandType.StoredProcedure));
    }

    private static UserLogin MapCompanyStatusToUserStatus(UserLogin userLogin, OrganizationStatus orgStatus)
    {
        userLogin.StatusId = orgStatus.StatusTypeId;
        userLogin.Status = orgStatus.Status;
        userLogin.IsPending = orgStatus.IsPending;
        userLogin.IsExpired = orgStatus.IsExpired;
        userLogin.IsActive = orgStatus.IsActive;
        userLogin.IsLocked = orgStatus.IsLocked;
        userLogin.IsForceReSetPassword = orgStatus.IsForceReSetPassword;
        userLogin.IsTainted = orgStatus.IsTainted;
        userLogin.FromDate = orgStatus.FromDate;
        userLogin.ThruDate = orgStatus.ThruDate;
        userLogin.StatusThruDate = orgStatus.StatusThruDate;
        return userLogin;
    }

    #endregion
}