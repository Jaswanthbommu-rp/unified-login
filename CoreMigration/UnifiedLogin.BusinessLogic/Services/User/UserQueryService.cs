using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.CacheHelper;
using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enterprise;
using UnifiedLogin.SharedObjects.Landing;
using SO = UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Services.User
{
    /// <summary>
    /// Async implementation of read-only user query operations
    /// Extracted from UserRepository for better separation of concerns
    /// </summary>
    public class UserQueryService : IUserQueryService
    {
        private readonly IRepositoryAsync _repositoryAsync;
        private readonly ILogger<UserQueryService> _logger;
        private readonly RPObjectCache _cache;
        private readonly IRedisCacheService _redisCache;

        public UserQueryService(
            IRepositoryAsync repositoryAsync, 
            IRedisCacheService redisCache,
            ILogger<UserQueryService> logger)
        {
            _repositoryAsync = repositoryAsync ?? throw new ArgumentNullException(nameof(repositoryAsync));
            _redisCache = redisCache ?? throw new ArgumentNullException(nameof(redisCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = new RPObjectCache();
        }

        #region Async Methods (Recommended for .NET 10)

        /// <summary>
        /// Get user details by persona ID or RealPageId (Async)
        /// </summary>
        public async Task<UserDetails> GetUserDetailsAsync(
            long? personaId = null,
            string userRealPageId = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Getting user details for PersonaId={PersonaId}, RealPageId={RealPageId}",
                    personaId, userRealPageId);

                await using var repo = _repositoryAsync;

                var result = await repo.GetOneAsync<UserDetails>(
                    StoredProcNameConstants.SP_GetUserDetails,
                    new { personaId, userRealPageId },
                    cancellationToken);

                if (result == null)
                {
                    _logger.LogWarning("No user details found for PersonaId={PersonaId}, RealPageId={RealPageId}",
                        personaId, userRealPageId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error getting user details for PersonaId={PersonaId}, RealPageId={RealPageId}",
                    personaId, userRealPageId);
                throw;
            }
        }

        /// <summary>
        /// Get enterprise user by username (Async)
        /// </summary>
        public async Task<SO.User> GetEnterpriseUserAsync(
            string enterpriseUserName,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(enterpriseUserName))
                throw new ArgumentNullException(nameof(enterpriseUserName), "Enterprise username cannot be null or empty");

            try
            {
                _logger.LogDebug("Getting enterprise user by username: {UserName}", enterpriseUserName);

                await using var repo = _repositoryAsync;

                var result = await repo.GetOneAsync<SO.User>(
                    StoredProcNameConstants.SP_GetUserByLoginId,
                    new { loginid = enterpriseUserName },
                    cancellationToken);

                if (result == null)
                {
                    _logger.LogWarning("Enterprise user not found: {UserName}", enterpriseUserName);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting enterprise user: {UserName}", enterpriseUserName);
                throw;
            }
        }

        /// <summary>
        /// Check if user is organization admin (Async)
        /// </summary>
        public async Task<bool> CheckOrganizationAdminUserAsync(
            Guid userRealPageId,
            long orgPartyId,
            CancellationToken cancellationToken = default)
        {
            if (userRealPageId == Guid.Empty)
                throw new ArgumentException("User RealPageId cannot be empty", nameof(userRealPageId));

            if (orgPartyId <= 0)
                throw new ArgumentException("Organization PartyId must be greater than 0", nameof(orgPartyId));

            try
            {
                _logger.LogDebug("Checking if user {UserRealPageId} is admin for org {OrgPartyId}",
                    userRealPageId, orgPartyId);

                await using var repo = _repositoryAsync;

                var response = await repo.GetOneAsync<int>(
                    StoredProcNameConstants.SP_EnterpriseCheckOrgAdmin,
                    new { UserRealPageId = userRealPageId, OrgPartyId = orgPartyId },
                    cancellationToken);

                return response > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error checking organization admin for UserRealPageId={UserRealPageId}, OrgPartyId={OrgPartyId}",
                    userRealPageId, orgPartyId);
                throw;
            }
        }

        /// <summary>
        /// Get Azure AD user details (Async)
        /// </summary>
        public async Task<AdUserDetail> GetAzureUserDetailsAsync(
            long userId,
            CancellationToken cancellationToken = default)
        {
            if (userId <= 0)
                throw new ArgumentException("UserId must be greater than 0", nameof(userId));

            try
            {
                _logger.LogDebug("Getting Azure AD details for UserId={UserId}", userId);

                await using var repo = _repositoryAsync;

                return await repo.GetOneAsync<AdUserDetail>(
                    StoredProcNameConstants.SP_GetADDetailsForUser,
                    new { userId },
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Azure AD details for UserId={UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Get navigation menu entries with caching (Async)
        /// </summary>
        public async Task<IList<NavigationMenuEntry>> GetNavigationMenuAsync(
            CancellationToken cancellationToken = default)
        {
            var cacheKey = "navigationMenuEntries";

            try
            {
                // Try to get from cache first
                var cachedResult = _redisCache.GetCacheValue<List<NavigationMenuEntry>>(cacheKey);
                if (cachedResult != null)
                {
                    _logger.LogDebug("Navigation menu retrieved from cache");
                    return cachedResult;
                }

                // Cache miss - fetch from database
                _logger.LogDebug("Navigation menu cache miss - fetching from database");

                await using var repo = _repositoryAsync;

                var result = await repo.GetManyAsync<NavigationMenuEntry>(
                    StoredProcNameConstants.SP_GetNavigationMenu,
                    null,
                    cancellationToken);

                var menuList = result?.ToList() ?? new List<NavigationMenuEntry>();

                // Cache for 3 hours (180 minutes)
                _redisCache.SetCacheValue(cacheKey, menuList, TimeSpan.FromMinutes(180));

                return menuList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting navigation menu");
                throw;
            }
        }

        /// <summary>
        /// Get navigation menu rights with caching (Async)
        /// </summary>
        public async Task<IList<NavigationMenuRightEntry>> GetNavigationMenuRightsAsync(
            CancellationToken cancellationToken = default)
        {
            var cacheKey = "navigationMenuRightEntries";

            try
            {
                // Try to get from cache first
                var cachedResult = _redisCache.GetCacheValue<List<NavigationMenuRightEntry>>(cacheKey);
                if (cachedResult != null)
                {
                    _logger.LogDebug("Navigation menu rights retrieved from cache");
                    return cachedResult;
                }

                // Cache miss - fetch from database
                _logger.LogDebug("Navigation menu rights cache miss - fetching from database");

                await using var repo = _repositoryAsync;

                var result = await repo.GetManyAsync<NavigationMenuRightEntry>(
                    StoredProcNameConstants.SP_GetNavigationMenuRights,
                    null,
                    cancellationToken);

                var rightsList = result?.ToList() ?? new List<NavigationMenuRightEntry>();

                // Cache for 3 hours (180 minutes)
                _redisCache.SetCacheValue(cacheKey, rightsList, TimeSpan.FromMinutes(180));

                return rightsList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting navigation menu rights");
                throw;
            }
        }

        /// <summary>
        /// Get supervisor information (Async)
        /// </summary>
        public async Task<UserInfoLite> GetSuperVisorInformationAsync(
            long userId,
            long organizationPartyId,
            CancellationToken cancellationToken = default)
        {
            if (userId <= 0)
                throw new ArgumentException("UserId must be greater than 0", nameof(userId));

            if (organizationPartyId <= 0)
                throw new ArgumentException("OrganizationPartyId must be greater than 0", nameof(organizationPartyId));

            try
            {
                _logger.LogDebug("Getting supervisor info for UserId={UserId}, OrgPartyId={OrgPartyId}",
                    userId, organizationPartyId);

                await using var repo = _repositoryAsync;

                return await repo.GetOneAsync<UserInfoLite>(
                    StoredProcNameConstants.SP_GetSuperVisorId,
                    new { UserId = userId, OrgPartyId = organizationPartyId },
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error getting supervisor info for UserId={UserId}, OrgPartyId={OrgPartyId}",
                    userId, organizationPartyId);
                throw;
            }
        }

        /// <summary>
        /// Get external user relationship (Async)
        /// </summary>
        public async Task<ExternalUserRelationship> GetExternalUserRelationshipAsync(
            long? userLoginPersonaId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Getting external user relationship for PersonaId={PersonaId}", userLoginPersonaId);

                await using var repo = _repositoryAsync;

                return await repo.GetOneAsync<ExternalUserRelationship>(
                    StoredProcNameConstants.SP_GetExternalUserRelationship,
                    new { userLoginPersonaId },
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error getting external user relationship for PersonaId={PersonaId}",
                    userLoginPersonaId);
                throw;
            }
        }

        /// <summary>
        /// Get super user count by organization (Async)
        /// </summary>
        public async Task<long> GetSuperUserCountByOrganizationAsync(
            long? organizationPartyId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Getting super user count for OrgPartyId={OrgPartyId}", organizationPartyId);

                await using var repo = _repositoryAsync;

                var result = await repo.GetOneAsync<long>(
                    StoredProcNameConstants.SP_GetSuperUsersCountByOrganization,
                    new { OrganizationPartyId = organizationPartyId },
                    cancellationToken);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error getting super user count for OrgPartyId={OrgPartyId}",
                    organizationPartyId);
                throw;
            }
        }

        /// <summary>
        /// Get employee product AD group mapping (Async)
        /// </summary>
        public async Task<IList<EmployeeProductMapping>> GetEmployeeProductADGroupMappingAsync(
            long personaId,
            int productId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Getting AD group mapping for PersonaId={PersonaId}, ProductId={ProductId}",
                    personaId, productId);

                await using var repo = _repositoryAsync;

                var result = await repo.GetManyAsync<EmployeeProductMapping>(
                    StoredProcNameConstants.SP_GetEmployeeProductADGroupMapping,
                    new { ProductId = productId, PersonaId = personaId },
                    cancellationToken);

                return result?.ToList() ?? new List<EmployeeProductMapping>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error getting AD group mapping for PersonaId={PersonaId}, ProductId={ProductId}",
                    personaId, productId);
                throw;
            }
        }

        /// <summary>
        /// Get navigation menu settings unaccessible (Async)
        /// </summary>
        public async Task<IList<NavigationMenuSetting>> GetNavigationMenuSettingsUnaccessableAsync(
            long partyId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Getting unaccessible navigation menu settings for PartyId={PartyId}", partyId);

                await using var repo = _repositoryAsync;

                var result = await repo.GetManyAsync<NavigationMenuSetting>(
                    StoredProcNameConstants.SP_GetNavigationMenuSettingUnaccessable,
                    new { partyId },
                    cancellationToken);

                return result?.ToList() ?? new List<NavigationMenuSetting>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error getting unaccessible navigation menu settings for PartyId={PartyId}",
                    partyId);
                throw;
            }
        }

        /// <summary>
        /// Get delegate admin role template (Async)
        /// </summary>
        public async Task<List<int>> GetDelegateAdminRoleTemplateAsync(
            long userLoginPersonaId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Getting delegate admin roles for PersonaId={PersonaId}", userLoginPersonaId);

                await using var repo = _repositoryAsync;

                var result = await repo.GetManyAsync<dynamic>(
                    StoredProcNameConstants.SP_GetEnterpriseDelegateRole,
                    new { UserLoginPersonaId = userLoginPersonaId },
                    cancellationToken);

                var rolesList = new List<int>();
                if (result != null)
                {
                    foreach (var item in result)
                    {
                        rolesList.Add((int)item.RoleTemplateId);
                    }
                }

                return rolesList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error getting delegate admin roles for PersonaId={PersonaId}",
                    userLoginPersonaId);
                throw;
            }
        }

        /// <summary>
        /// Stream users for large result sets using IAsyncEnumerable (.NET 10 feature)
        /// </summary>
        /// <param name="organizationPartyId">Organization Party ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Async enumerable of user details</returns>
        //public async IAsyncEnumerable<UserDetails> StreamUsersByOrganizationAsync(
        //    long organizationPartyId,
        //    [EnumeratorCancellation] CancellationToken cancellationToken = default)
        //{
        //    _logger.LogDebug("Streaming users for OrgPartyId={OrgPartyId}", organizationPartyId);

        //    await using var repo = _repositoryAsync;

        //    var users = await repo.GetManyAsync<UserDetails>(
        //        StoredProcNameConstants.SP_ListUsersByOrganization,
        //        new { OrganizationPartyId = organizationPartyId },
        //        cancellationToken);

        //    foreach (var user in users)
        //    {
        //        cancellationToken.ThrowIfCancellationRequested();
        //        yield return user;
        //    }
        //}

        /// <summary>
        /// Batch get multiple user details in parallel (.NET 10 optimization)
        /// </summary>
        public async Task<IReadOnlyDictionary<long, UserDetails>> GetUserDetailsBatchAsync(
            IEnumerable<long> personaIds,
            CancellationToken cancellationToken = default)
        {
            if (personaIds == null || !personaIds.Any())
                return new Dictionary<long, UserDetails>();

            try
            {
                _logger.LogDebug("Batch getting user details for {Count} personas", personaIds.Count());

                var tasks = personaIds.Select(async personaId =>
                {
                    var userDetails = await GetUserDetailsAsync(personaId, null, cancellationToken);
                    return new KeyValuePair<long, UserDetails>(personaId, userDetails);
                });

                var results = await Task.WhenAll(tasks);

                return results
                    .Where(kvp => kvp.Value != null)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in batch getting user details");
                throw;
            }
        }

        #endregion
    }
}
