using RealPage.DataAccess.Dapper;
using System.Data;
using UnifiedLogin.BusinessLogic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository;

public class UserLoginRepositoryAsync(IDbConnection db, ICacheService cacheService, IOrganizationRepositoryAsync organizationRepositoryAsync) : IUserLoginRepositoryAsync
{
    public async Task<List<Organization>> ListOrganizationByRealPageIdAsync(Guid realPageId, CancellationToken token)
    {
        var relationshipTypeName = "User Type";

        var organizationListTask = db.GetManyAsync<Organization>(StoredProcNameConstants.SP_ListOrganizationByRealPageId, new { realPageId, relationshipTypeName }, token: token);
        var orgTypesTask = organizationRepositoryAsync.ListOrganizationTypeAsync(token);
        var orgDomainsTask = organizationRepositoryAsync.ListOrganizationDomainAsync(token);

        await Task.WhenAll(organizationListTask, orgTypesTask, orgDomainsTask);

        var organizations = (await organizationListTask).ToList();
        var orgTypeLookup = (await orgTypesTask)!.ToDictionary(o => o.OrganizationTypeId);
        var orgDomainLookup = (await orgDomainsTask)!.ToDictionary(d => d.OrganizationDomainId);

        foreach (var organization in organizations)
        {
            organization.organizationType = orgTypeLookup.TryGetValue(organization.OrganizationTypeId, out var orgType)
                ? new OrganizationType { Name = orgType.Name, OrganizationTypeId = orgType.OrganizationTypeId, CreateDate = orgType.CreateDate }
                : new OrganizationType();
            organization.OrganizationDomain = orgDomainLookup.TryGetValue(organization.OrganizationDomainId, out var orgDomain)
                ? new OrganizationDomain { OrganizationDomainId = orgDomain.OrganizationDomainId, Name = orgDomain.Name, CreateDate = orgDomain.CreateDate }
                : new OrganizationDomain();
        }

        return organizations;
    }
}

