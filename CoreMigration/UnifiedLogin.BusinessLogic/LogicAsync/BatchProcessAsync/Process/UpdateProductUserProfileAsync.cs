using UnifiedLogin.BusinessLogic.LogicAsync.BatchProcessAsync.Factory;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.BatchProcessAsync.Process;

/// <summary>
/// Async handler for <c>BatchProcessType.ProfileUpdate</c>.
/// <para>
/// Replaces the sync <c>UpdateProductUserProfile</c> which directly instantiated
/// <c>new ManageProductUser(new DefaultUserClaim { CorrelationId = Guid.NewGuid() })</c>.
/// All repository and integration-type dependencies are resolved through DI via
/// <see cref="IManageProductUserAsync"/>.
/// </para>
/// </summary>
public sealed class UpdateProductUserProfileAsync : IProcessExecutionAsync
{
    private readonly IManageProductUserAsync _manageProductUser;

    public UpdateProductUserProfileAsync(IManageProductUserAsync manageProductUser)
    {
        _manageProductUser = manageProductUser
            ?? throw new ArgumentNullException(nameof(manageProductUser));
    }

    /// <inheritdoc/>
    public async Task<string> ExecuteProcessAsync(
        ProductUserProperitiesRoles batchRecord,
        CancellationToken cancellationToken = default)
    {
        if (batchRecord.CorrelationId == Guid.Empty)
            batchRecord.CorrelationId = Guid.NewGuid();

        return await _manageProductUser
            .UpdateProductUserProfileAsync(batchRecord, cancellationToken)
            .ConfigureAwait(false);
    }
}
