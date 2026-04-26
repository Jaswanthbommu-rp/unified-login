using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.BatchProcessAsync.Factory;

/// <summary>
/// Async contract for a single batch process step.
/// Replaces the sync <c>IProcessExecution</c> interface whose <c>ExecuteProcess</c> method
/// blocked the thread via <c>new ManageProductUser(new DefaultUserClaim { ... })</c>.
/// </summary>
public interface IProcessExecutionAsync
{
    /// <summary>
    /// Executes the batch process step for the given record.
    /// Returns <see cref="string.Empty"/> on success; otherwise an error message.
    /// </summary>
    Task<string> ExecuteProcessAsync(
        ProductUserProperitiesRoles batchRecord,
        CancellationToken cancellationToken = default);
}
