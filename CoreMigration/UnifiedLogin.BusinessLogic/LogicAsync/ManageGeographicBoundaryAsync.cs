using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Stepping-stone async wrapper for geographic boundary operations.
/// Delegates to the existing sync <see cref="IManageGeographicBoundary"/> via <see cref="Task.FromResult{TResult}"/>.
/// </summary>
public sealed class ManageGeographicBoundaryAsync : IManageGeographicBoundaryAsync
{
    private readonly IManageGeographicBoundaryAsync _manageGeographicBoundary;

    public ManageGeographicBoundaryAsync(IManageGeographicBoundaryAsync manageGeographicBoundary)
    {
        _manageGeographicBoundary = manageGeographicBoundary ?? throw new ArgumentNullException(nameof(manageGeographicBoundary));
    }

    public async Task<RepositoryResponse> CreateGeographicBoundaryAsync(IGeographicBoundary geographicBoundary, CancellationToken cancellationToken = default)
        => await _manageGeographicBoundary.CreateGeographicBoundaryAsync(geographicBoundary, cancellationToken);
}
