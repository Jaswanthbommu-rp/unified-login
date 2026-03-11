using Dapper;
using RealPage.DataAccess.Dapper;
using System.Data;
using UnifiedLogin.BusinessLogic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Cache;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository;

public class OrganizationRepositoryAsync(ICacheService cacheService, IDbConnection db) : IOrganizationRepositoryAsync
{

    /// <summary>
    /// Used to get the Organization based on the realPageId
    /// </summary>
    /// <param name="realPageId">Organization unique identifier</param>
    /// <param name="organizationPartyId">Optional organization PartyId</param>
    /// <param name="token"></param>
    /// <returns>Organization object</returns>
    public async Task<Organization?> GetOrganizationAsync(Guid? realPageId = null, long? organizationPartyId = null, CancellationToken token = default)
    {

        return await cacheService.GetOrSetAsync($"{nameof(OrganizationRepository)}_getOrganizationAsync_{realPageId}_{organizationPartyId}", async _ =>
        {
            object param = new
            {
                RealPageId = (realPageId == Guid.Empty) ? null : realPageId,
                PartyId = organizationPartyId
            };

            var organization = await db.GetOneAsync<Organization>(StoredProcNameConstants.SP_GetOrganization, param, token: token);

            if (organization == null) return null;

            var orgType = (await ListOrganizationTypeAsync(token) ?? throw new InvalidOperationException()).FirstOrDefault(o => o.OrganizationTypeId == organization.OrganizationTypeId);
            organization.organizationType = orgType != null ? new OrganizationType { Name = orgType.Name, OrganizationTypeId = orgType.OrganizationTypeId, CreateDate = orgType.CreateDate } : new OrganizationType();
            var orgDomain = (await ListOrganizationDomainAsync(token) ?? throw new InvalidOperationException()).FirstOrDefault(d => d.OrganizationDomainId == organization.OrganizationDomainId);
            organization.OrganizationDomain = orgDomain != null ? new OrganizationDomain { OrganizationDomainId = orgDomain.OrganizationDomainId, Name = orgDomain.Name, CreateDate = orgDomain.CreateDate } : new OrganizationDomain();

            return organization;
        }, new CacheEntryOptions() { ExpirationTimeInMinutes = 2 }, cancellationToken: token);
    }

    /// <summary>
    /// Used to get the list of all Organization Types
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>OrganizationType list object</returns>
    public async Task<List<OrganizationType>?> ListOrganizationTypeAsync(CancellationToken cancellationToken)
    {
        return (await cacheService.GetOrSetAsync($"{nameof(OrganizationRepository)}_listOrganizationTypeAsync", async _ =>
            (await db.GetManyAsync<OrganizationType>(StoredProcNameConstants.SP_ListOrganizationType, token: cancellationToken)).ToList(), new CacheEntryOptions() { ExpirationTimeInMinutes = 2 }, cancellationToken: cancellationToken))!;
    }

    /// <summary>
    /// Used to get the list of all Organization Domains
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>OrganizationDomain list</returns>
    public async Task<List<OrganizationDomain>?> ListOrganizationDomainAsync(CancellationToken cancellationToken = default)
    {
        return await cacheService.GetOrSetAsync($"{nameof(OrganizationRepository)}_listOrganizationDomainAsync", async _ =>
            (await db.GetManyAsync<OrganizationDomain>(StoredProcNameConstants.SP_ListOrganizationDomain, token: cancellationToken)).ToList(), new CacheEntryOptions() { ExpirationTimeInMinutes = 2 }, cancellationToken);
    }

}

