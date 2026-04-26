using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Batch;
using UnifiedLogin.SharedObjects.EnterpriseRole;
using UnifiedLogin.SharedObjects.Enum;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

public sealed class ManageEnterpriseRoleProductBatchAsync : IManageEnterpriseRoleProductBatchAsync
{
    private readonly IManageEnterpriseRolesPrimaryPropertiesAsync _primaryProperties;
    private readonly IBatchProductBulkUpdateRepositoryAsync _batchRepo;
    private readonly ILogger<ManageEnterpriseRoleProductBatchAsync> _logger;

    public ManageEnterpriseRoleProductBatchAsync(
        IManageEnterpriseRolesPrimaryPropertiesAsync primaryProperties,
        IBatchProductBulkUpdateRepositoryAsync batchRepo,
        ILogger<ManageEnterpriseRoleProductBatchAsync> logger)
    {
        _primaryProperties = primaryProperties ?? throw new ArgumentNullException(nameof(primaryProperties));
        _batchRepo         = batchRepo         ?? throw new ArgumentNullException(nameof(batchRepo));
        _logger            = logger            ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> GenerateEnterpriseRoleUserProductBatchAsync(
        EnterpriseRoleBatch batch, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(batch);

        string statusMessage;
        try
        {
            if (batch.BatchProcessTypeId == (int)BatchProcessType.BulkAddUpdateEnterpriseRole)
            {
                // First pass: unassign all existing products
                statusMessage = await _primaryProperties.ProcessEnterpriseRolesAndPrimaryPropertiesDataAsync(
                    batch.EditorUserPersonaId, batch.SubjectUserPersonaId,
                    batch.EnterpriseRoleTemplateId, batch.CreatedDateTime,
                    batch.BatchProcessTypeId, isUnassignAllProducts: true,
                    cancellationToken).ConfigureAwait(false);

                // Second pass: assign new products from template
                if (string.IsNullOrEmpty(statusMessage))
                {
                    statusMessage = await _primaryProperties.ProcessEnterpriseRolesAndPrimaryPropertiesDataAsync(
                        batch.EditorUserPersonaId, batch.SubjectUserPersonaId,
                        batch.EnterpriseRoleTemplateId, batch.CreatedDateTime,
                        batch.BatchProcessTypeId, isUnassignAllProducts: false,
                        cancellationToken).ConfigureAwait(false);
                }
            }
            else
            {
                statusMessage = await _primaryProperties.ProcessEnterpriseRolesAndPrimaryPropertiesDataAsync(
                    batch.EditorUserPersonaId, batch.SubjectUserPersonaId,
                    batch.EnterpriseRoleTemplateId, batch.CreatedDateTime,
                    cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            if (string.IsNullOrEmpty(statusMessage))
            {
                await _batchRepo.UpdateEnterpriseRoleProductBatchAsync(
                    batch.EnterpriseRoleBatchProcessId, (int)ProductBatchStatusType.Success,
                    cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "GenerateEnterpriseRoleUserProductBatchAsync failed BatchId={BatchId}",
                batch.EnterpriseRoleBatchProcessId);

            await _batchRepo.UpdateEnterpriseRoleProductBatchAsync(
                batch.EnterpriseRoleBatchProcessId, (int)ProductBatchStatusType.Error,
                cancellationToken).ConfigureAwait(false);

            return "Error";
        }

        return statusMessage ?? "";
    }
}
