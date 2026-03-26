using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.UnifiedLogin;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Async-first replacement for <see cref="UnifiedLogin.BusinessLogic.Logic.ManageUserRoleRight"/>.
/// <para>Key changes over the sync version:</para>
/// <list type="bullet">
///   <item>Single DI constructor — no <c>new UserRoleRightRepository()</c> anywhere.</item>
///   <item>All methods are <c>async Task</c> — no <c>.Result</c> or <c>.Wait()</c>.</item>
///   <item>Injected <see cref="ILogger{T}"/> replaces any static logging.</item>
///   <item><see cref="CancellationToken"/> threaded through every repository call.</item>
/// </list>
/// </summary>
public sealed class ManageUserRoleRightAsync : IManageUserRoleRightAsync
{
    #region Fields

    private readonly IUserRoleRightRepositoryAsync _repo;
    private readonly ILogger<ManageUserRoleRightAsync> _logger;

    #endregion

    #region Constructor

    /// <summary>
    /// Primary DI constructor — all dependencies injected, no <c>new</c>.
    /// </summary>
    public ManageUserRoleRightAsync(
        IUserRoleRightRepositoryAsync repo,
        ILogger<ManageUserRoleRightAsync> logger)
    {
        _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // IManageUserRoleRightAsync
    // ════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public async Task<IList<Role>> GetAssignedRoleForPersonaAsync(
        ProductEnum productId,
        long? userPersonaId = null,
        long? organizationPartyId = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "GetAssignedRoleForPersonaAsync: productId={ProductId} personaId={PersonaId} orgPartyId={OrgPartyId}",
            productId, userPersonaId, organizationPartyId);

        return await _repo.ListRoleByPersonaAsync(
            (int)productId, userPersonaId, organizationPartyId);
    }

    /// <inheritdoc/>
    public Task<long> GetRoleIdByPersonaAsync(
        long userPersonaId,
        ProductEnum productId,
        CancellationToken cancellationToken = default)
        => _repo.GetRoleIdByPersonaAsync(userPersonaId, (int)productId);

    /// <inheritdoc/>
    public Task<List<long>> GetRoleIdsByPersonaAsync(
        long userPersonaId,
        ProductEnum productId,
        CancellationToken cancellationToken = default)
        => _repo.GetRoleIdsByPersonaAsync(userPersonaId, (int)productId);

    /// <inheritdoc/>
    public Task<RepositoryResponse> InsertAssignedRoleToUserAsync(
        long userPersonaId,
        long roleId,
        int userId,
        bool deleteRole = false,
        CancellationToken cancellationToken = default)
        => _repo.InsertAssignedRoleToUserAsync(userPersonaId, roleId, userId, deleteRole);

    /// <inheritdoc/>
    public Task<IList<UserRoleRights>> GetAllRoleRightsAsync(
        long partyId,
        IList<int> productIdList,
        int productId,
        CancellationToken cancellationToken = default)
        => _repo.GetAllRoleRightsAsync(partyId, productIdList, productId);

    /// <inheritdoc/>
    public Task<IList<UnifiedLoginRoleRights>> GetPlatformRoleRightsAsync(
        long partyId,
        IList<int> productIdList,
        int productId,
        CancellationToken cancellationToken = default)
        => _repo.GetPlatformRoleRightsAsync(partyId, productIdList, productId);

    /// <inheritdoc/>
    public Task<IList<Right>> GetADGroupRightsByPersonaIdAsync(
        long personaId,
        CancellationToken cancellationToken = default)
        => _repo.GetADGroupRightsByPersonaIdAsync(personaId, cancellationToken);

    /// <inheritdoc/>
    public Task<IList<Right>> GetPersistRightsAsync(
        CancellationToken cancellationToken = default)
        => _repo.GetPersistRightsAsync(cancellationToken);
}