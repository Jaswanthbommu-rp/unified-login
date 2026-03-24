using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces;

public interface IGeographicBoundaryRepositoryAsync
{
    Task<RepositoryResponse> CreateGeographicBoundaryAsync(IGeographicBoundary geographicBoundary, CancellationToken cancellationToken = default);
}