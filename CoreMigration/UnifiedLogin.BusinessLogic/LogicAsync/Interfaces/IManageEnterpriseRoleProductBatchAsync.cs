using UnifiedLogin.SharedObjects.Batch;
using UnifiedLogin.SharedObjects.EnterpriseRole;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

public interface IManageEnterpriseRoleProductBatchAsync
{
    Task<string> GenerateEnterpriseRoleUserProductBatchAsync(
        EnterpriseRoleBatch batch, CancellationToken cancellationToken = default);
}
