using UnifiedLogin.BusinessLogic.LogicAsync.BatchProcessAsync.Factory;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.BatchProcessAsync.Process;

/// <summary>
/// Async handler for <c>BatchProcessType.DeactivateProductUser</c>.
/// <para>
/// Mirrors the sync <c>DeactivateProductUser</c> which threw
/// <see cref="NotImplementedException"/>. Preserved as a placeholder pending
/// implementation of the deactivation workflow.
/// </para>
/// <para>
/// <b>TODO:</b> Implement deactivation logic via <c>IManageProductUserAsync</c>
/// once the corresponding async method is ported from <c>ManageProductUser</c>.
/// </para>
/// </summary>
public sealed class DeactivateProductUserAsync : IProcessExecutionAsync
{
    /// <inheritdoc/>
    /// <exception cref="NotImplementedException">Always — deactivation not yet ported to async.</exception>
    public Task<string> ExecuteProcessAsync(
        ProductUserProperitiesRoles batchRecord,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException(
            "DeactivateProductUser has not been ported to the async pipeline yet.");
}
