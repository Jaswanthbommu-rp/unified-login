using System;
using System.Collections.Generic;
using System.Text;
using UnifiedLogin.SharedObjects.Enterprise;
using UnifiedLogin.SharedObjects.Landing;
using SO = UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Services.User
{
    public interface IUserQueryService
    {
        /// <summary>
        /// Get user details by persona ID or RealPageId (Async)
        /// </summary>
        Task<UserDetails> GetUserDetailsAsync(
            long? personaId = null,
            string userRealPageId = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get enterprise user by username (Async)
        /// </summary>
        Task<SO.User> GetEnterpriseUserAsync(
            string enterpriseUserName,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if user is organization admin (Async)
        /// </summary>
        Task<bool> CheckOrganizationAdminUserAsync(
            Guid userRealPageId,
            long orgPartyId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get Azure AD user details (Async)
        /// </summary>
        Task<AdUserDetail> GetAzureUserDetailsAsync(
            long userId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get navigation menu entries with caching (Async)
        /// </summary>
        Task<IList<NavigationMenuEntry>> GetNavigationMenuAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get navigation menu rights with caching (Async)
        /// </summary>
        Task<IList<NavigationMenuRightEntry>> GetNavigationMenuRightsAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get supervisor information (Async)
        /// </summary>
        Task<UserInfoLite> GetSuperVisorInformationAsync(
            long userId,
            long organizationPartyId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get external user relationship (Async)
        /// </summary>
        Task<ExternalUserRelationship> GetExternalUserRelationshipAsync(
            long? userLoginPersonaId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get super user count by organization (Async)
        /// </summary>
        Task<long> GetSuperUserCountByOrganizationAsync(
            long? organizationPartyId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Stream users for large result sets using IAsyncEnumerable (.NET 10 feature)
        /// </summary>
        //IAsyncEnumerable<UserDetails> StreamUsersByOrganizationAsync(
        //    long organizationPartyId,
        //    CancellationToken cancellationToken = default);

        /// <summary>
        /// Batch get multiple user details in parallel (.NET 10 optimization)
        /// </summary>
        Task<IReadOnlyDictionary<long, UserDetails>> GetUserDetailsBatchAsync(
            IEnumerable<long> personaIds,
            CancellationToken cancellationToken = default);
    }
}
