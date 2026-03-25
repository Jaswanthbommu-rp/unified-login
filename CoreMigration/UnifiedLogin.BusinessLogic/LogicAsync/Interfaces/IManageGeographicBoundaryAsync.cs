using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async interface for geographic boundary management operations.
/// </summary>
public interface IManageGeographicBoundaryAsync
{
    Task<RepositoryResponse> CreateGeographicBoundaryAsync(IGeographicBoundary geographicBoundary, CancellationToken cancellationToken = default);
}
