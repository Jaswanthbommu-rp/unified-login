using Dapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Security.Cryptography;
using UnifiedLogin.BusinessLogic.CacheHelper;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.DataAccess.Helper;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Cache;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.UnifiedLogin;

namespace UnifiedLogin.BusinessLogic.Repository;

/// <summary>
/// Async-first User Role/Right Repository.
/// Uses injected <see cref="IDbConnection"/> (Dapper) directly —
/// no <see cref="BaseRepository"/> inheritance, no <c>new</c> keyword.
/// </summary>
public sealed class UserRoleRightRepositoryAsync : IUserRoleRightRepositoryAsync
{
    #region Fields

    private readonly IDbConnection _db;
    private readonly ICacheService _cache;
    private readonly ILogger<UserRoleRightRepositoryAsync> _logger;

    // Replaces: RPObjectCache inline TTL of 120 seconds = 2 minutes
    private static readonly CacheEntryOptions RoleByPersonaCacheOptions = new() { ExpirationTimeInMinutes = 2 };
    private static readonly CacheEntryOptions PersistRightCacheOptions = new() { ExpirationTimeInMinutes = 3 };
    #endregion

    #region Constructor

    /// <summary>
    /// Primary DI constructor — all dependencies injected, no <c>new</c>.
    /// </summary>
    public UserRoleRightRepositoryAsync(
        IDbConnection db,
        ICacheService cache,
        ILogger<UserRoleRightRepositoryAsync> logger)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    #region IUserRoleRightRepositoryAsync Implementation

    /// <inheritdoc/>
    public async Task<List<Role>> ListRoleByPersonaAsync(
        int productId,
        long? userPersonaId = null,
        long? organizationPartyId = null)
    {
        var cacheKey = $"sp_ListRolesForProductsByPersonaId_{productId}_{userPersonaId}_{organizationPartyId}";

        // Replaces: new RPObjectCache().GetFromCache(cacheKey, 120, factory)
        return await _cache.GetOrSetAsync(
            cacheKey,
            async _ =>
            {
                var rows = (await _db.QueryAsync<dynamic>(
                    new CommandDefinition(
                        StoredProcNameConstants.SP_ListRolesForProductsByPersonaId,
                        new { ProductID = productId, PersonaID = userPersonaId, PartyId = organizationPartyId },
                        commandType: CommandType.StoredProcedure))).ToList();

                return rows
                    .Select(item => new Role
                    {
                        RoleID = item.RoleId,
                        Name = item.Role,
                        PersonaId = item.PersonaId?.ToString(),
                        RoleNickName = item.RoleNickName
                    })
                    .ToList();
            },
            RoleByPersonaCacheOptions) ?? [];
    }

    /// <inheritdoc/>
    public async Task<long> GetRoleIdByPersonaAsync(long userPersonaId, int productId)
    {
        var result = await _db.QuerySingleOrDefaultAsync<dynamic>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListRolesForProductsByPersonaId,
                new { PersonaID = userPersonaId, ProductID = productId },
                commandType: CommandType.StoredProcedure));

        return result is not null ? Convert.ToInt64(result.RoleId) : 0L;
    }

    /// <inheritdoc/>
    /// <inheritdoc/>
    public async Task<List<long>> GetRoleIdsByPersonaAsync(long userPersonaId, int productId)
    {
        var rows = await _db.QueryAsync<dynamic>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListRolesForProductsByPersonaId,
                new { PersonaID = userPersonaId, ProductID = productId },
                commandType: CommandType.StoredProcedure));

        return rows.Select<dynamic, long>(r => Convert.ToInt64(r.RoleId)).ToList();
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> InsertAssignedRoleToUserAsync(
        long userPersonaId,
        long roleId,
        int userId,
        bool deleteRole = false)
    {
        // Replaces: branching on _repository == null — _db is always available via DI
        return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_LinkPersonaToRole,
                new
                {
                    PersonaID = userPersonaId,
                    RoleID = roleId,
                    IsDeleted = deleteRole,
                    CreatedBy = userId,
                    PersonaPrivilgeID = 0
                },
                commandType: CommandType.StoredProcedure))
            ?? new RepositoryResponse();
    }

    /// <inheritdoc/>
    public async Task<IList<UserRoleRights>> GetAllRoleRightsAsync(
        long partyId,
        IList<int> productIdList,
        int productId)
    {
        if (productIdList.Count == 0)
            throw new ArgumentException("Missing company product id list", nameof(productIdList));

        var userRoles = new Dictionary<int, UserRoleRights>();

        // Replaces: repository.GetManyWithSpliOn<UserRoleRights, Right, UserRoleRights>(...)
        // Standard Dapper multi-mapping: splitOn marks where Right columns begin in the result set
        var param = BuildRoleRightsParam(partyId, productId, productIdList);

        await _db.QueryAsync<UserRoleRights, Right, UserRoleRights>(
            StoredProcNameConstants.SP_ListRightsAssociatedWithRoles,
            (role, right) =>
            {
                if (!userRoles.ContainsKey(role.RoleId))
                    userRoles.Add(role.RoleId, role);
                userRoles[role.RoleId].UserRights.Add(right);
                return userRoles[role.RoleId];
            },
            param,
            splitOn: "RightId",
            commandType: CommandType.StoredProcedure);

        return userRoles.Values.ToList();
    }

    /// <inheritdoc/>
    public async Task<IList<UnifiedLoginRoleRights>> GetPlatformRoleRightsAsync(
        long partyId,
        IList<int> productIdList,
        int productId)
    {
        if (productIdList.Count == 0)
            throw new ArgumentException("Missing company product id list", nameof(productIdList));

        var userRoles = new Dictionary<int, UnifiedLoginRoleRights>();

        // Replaces: repository.GetManyWithSpliOn<UnifiedLoginRoleRights, UnifiedLoginRight, UnifiedLoginRoleRights>(...)
        var param = BuildRoleRightsParam(partyId, productId, productIdList);

        await _db.QueryAsync<UnifiedLoginRoleRights, UnifiedLoginRight, UnifiedLoginRoleRights>(
            StoredProcNameConstants.SP_ListRightsAssociatedWithRoles,
            (role, right) =>
            {
                if (!userRoles.ContainsKey(role.RoleId))
                    userRoles.Add(role.RoleId, role);
                userRoles[role.RoleId].UserRights.Add(right);
                return userRoles[role.RoleId];
            },
            param,
            splitOn: "RightId",
            commandType: CommandType.StoredProcedure);

        return userRoles.Values.ToList();
    }

    public async Task<IList<Right>> GetPersistRightsAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = "getPersistRights";

        return await _cache.GetOrSetAsync(
            cacheKey,
            async _ =>
            {
                return (await _db.QueryAsync<Right>(
                    new CommandDefinition(
                        StoredProcNameConstants.SP_GetPersistRights,
                        null,
                        commandType: CommandType.StoredProcedure))).ToList();
               
            },
            PersistRightCacheOptions) ?? [];
    }

    public async Task<IList<Right>> GetADGroupRightsByPersonaIdAsync(
            long personaId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"getADGroupRights_{personaId}";

        return await _cache.GetOrSetAsync(
            cacheKey,
            async _ =>
            {
                return (await _db.QueryAsync<Right>(
                     new CommandDefinition(
                         StoredProcNameConstants.SP_GetADGroupRightsByPersona,
                         new { PersonaId = personaId },
                         commandType: CommandType.StoredProcedure))).ToList();

            },
            PersistRightCacheOptions) ?? [];
        
    }
    #endregion

    #region Private Helpers

    /// <summary>
    /// Builds the <see cref="DynamicParameters"/> shared by both role-rights queries,
    /// including the TVP for the product id list.
    /// Replaces: inline <c>TableValueParamHelper</c> usage duplicated in both methods.
    /// </summary>
    private static DynamicParameters BuildRoleRightsParam(
        long partyId,
        int productId,
        IList<int> productIdList)
    {
        var param = new DynamicParameters();
        param.Add("PartyId", partyId);
        param.Add("ProductId", productId);
        // TableValueParamHelper is a static utility — not a new dependency instantiation
        param.Add("TargetProductId",
            TableValueParamHelper.ConvertToTableValuedParameter(productIdList, "enterprise.productidtype"));
        return param;
    }

    #endregion
}

//Summary of every change:
//Old pattern Replacement
//BaseRepository inheritance + 4 constructors Single DI constructor with IDbConnection, ICacheService, ILogger
//new RPObjectCache().GetFromCache(...)   ICacheService.GetOrSetAsync(key, factory, CacheEntryOptions)
//new ProductInternalSettingRepository()  Removed — getRoleRightsSchemaName() private method not required by interface
//_repository == null branch in InsertAssignedRoleToUser(long, long, int, bool)   Removed — _db is always present via DI
//repository.GetManyWithSpliOn<T1, T2, TReturn>(...)	_db.QueryAsync<T1, T2, TReturn>(..., splitOn: "RightId") — standard Dapper multi - mapping
//Duplicated TableValueParamHelper +param construction in two methods    Extracted into private BuildRoleRightsParam static helper
//Exception("Missing...") ArgumentException with nameof — more precise exception type
