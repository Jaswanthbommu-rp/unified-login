using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces;

public interface IOrganizationRepositoryAsync
{
    Task<Organization?> GetOrganizationAsync(Guid? realPageId = null, long? organizationPartyId = null, CancellationToken token = default);

    Task<List<OrganizationType>?> ListOrganizationTypeAsync(CancellationToken cancellationToken);

    Task<List<OrganizationDomain>?> ListOrganizationDomainAsync(CancellationToken cancellationToken);
}

