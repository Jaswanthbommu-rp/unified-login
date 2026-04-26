using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

public sealed class ManageGeographicBoundaryAsync(IGeographicBoundaryRepositoryAsync repository) : IManageGeographicBoundaryAsync
{
    private readonly IGeographicBoundaryRepositoryAsync _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public Task<RepositoryResponse> CreateGeographicBoundaryAsync(
        IGeographicBoundary geographicBoundary, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(geographicBoundary);
        return _repository.CreateGeographicBoundaryAsync(geographicBoundary, cancellationToken);
    }
}
