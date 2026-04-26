using UnifiedLogin.BusinessLogic.LogicAsync.BatchProcessAsync.Factory;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.BatchProcessAsync.Process;

/// <summary>
/// Async handler for user-type change batch processes:
/// <c>UserTypeAdminToRegular</c>, <c>UserTypeRegularToAdmin</c>,
/// <c>UserTypeAdminToExternal</c>, <c>UserTypeExternalToAdmin</c>.
/// <para>
/// Replaces the sync <c>ChangeProductUserType</c> which directly instantiated
/// <c>new ManageProductUser(new DefaultUserClaim { CorrelationId = batchRecord.CorrelationId })</c>.
/// </para>
/// </summary>
public sealed class ChangeProductUserTypeAsync : IProcessExecutionAsync
{
    private readonly IManageProductUserAsync _manageProductUser;

    public ChangeProductUserTypeAsync(IManageProductUserAsync manageProductUser)
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

        return await _manageProductUser.ChangeUserTypeAsync(batchRecord, cancellationToken)
            .ConfigureAwait(false);
    }
}
