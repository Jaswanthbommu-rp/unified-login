using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Batch;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

public sealed class ManagePrimaryPropertiesBatchAsync : IManagePrimaryPropertiesBatchAsync
{
    private readonly IManageEnterpriseRolesPrimaryPropertiesAsync _enterpriseRoles;
    private readonly IBatchProductBulkUpdateRepositoryAsync       _batchRepo;
    private readonly IProductRepositoryAsync                      _productRepo;
    private readonly IProductInternalSettingRepositoryAsync       _productInternalSettingRepo;
    private readonly IUnifiedSettingsRepositoryAsync              _unifiedSettingsRepo;
    private readonly IUserClaimsAccessor                          _userClaims;
    private readonly ILogger<ManagePrimaryPropertiesBatchAsync>   _logger;

    public ManagePrimaryPropertiesBatchAsync(
        IManageEnterpriseRolesPrimaryPropertiesAsync enterpriseRoles,
        IBatchProductBulkUpdateRepositoryAsync       batchRepo,
        IProductRepositoryAsync                      productRepo,
        IProductInternalSettingRepositoryAsync       productInternalSettingRepo,
        IUnifiedSettingsRepositoryAsync              unifiedSettingsRepo,
        IUserClaimsAccessor                          userClaims,
        ILogger<ManagePrimaryPropertiesBatchAsync>   logger)
    {
        _enterpriseRoles            = enterpriseRoles            ?? throw new ArgumentNullException(nameof(enterpriseRoles));
        _batchRepo                  = batchRepo                  ?? throw new ArgumentNullException(nameof(batchRepo));
        _productRepo                = productRepo                ?? throw new ArgumentNullException(nameof(productRepo));
        _productInternalSettingRepo = productInternalSettingRepo ?? throw new ArgumentNullException(nameof(productInternalSettingRepo));
        _unifiedSettingsRepo        = unifiedSettingsRepo        ?? throw new ArgumentNullException(nameof(unifiedSettingsRepo));
        _userClaims                 = userClaims                 ?? throw new ArgumentNullException(nameof(userClaims));
        _logger                     = logger                     ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> GeneratePrimaryPropertiesUserProductBatchAsync(
        PrimaryPropertyBatch batch,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(batch);

        try
        {
            var statusMessage = await _enterpriseRoles
                .ProcessEnterpriseRolesAndPrimaryPropertiesDataAsync(
                    batch.EditorUserPersonaId,
                    batch.SubjectUserPersonaId,
                    batchProcessTypeId: batch.BatchProcessTypeId,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            var statusTypeId = string.IsNullOrEmpty(statusMessage)
                ? (int)ProductBatchStatusType.Success
                : (int)ProductBatchStatusType.Error;

            await _batchRepo
                .UpdatePrimaryPropertyProductBatchAsync(
                    batch.PrimaryPropertyBatchProcessId, statusTypeId, cancellationToken)
                .ConfigureAwait(false);

            return string.IsNullOrEmpty(statusMessage) ? string.Empty : statusMessage;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "{ActionName} failed for batch {BatchId}",
                nameof(GeneratePrimaryPropertiesUserProductBatchAsync),
                batch.PrimaryPropertyBatchProcessId);

            await _batchRepo
                .UpdatePrimaryPropertyProductBatchAsync(
                    batch.PrimaryPropertyBatchProcessId, (int)ProductBatchStatusType.Error, cancellationToken)
                .ConfigureAwait(false);

            return "Error";
        }
    }

    // ── Private helpers ────────────────────────────────────────────────────

    private async Task<bool> GetPrimaryPropertySettingsForCompanyAndProductAsync(
        int productId,
        Guid orgRealPageId,
        long orgPartyId,
        CancellationToken cancellationToken)
    {
        // Three independent repository reads — run concurrently.
        var globalSettingsTask  = _productInternalSettingRepo.GetProductSettingByTypeAsync("UsePrimaryProperties", cancellationToken);
        var companySettingsTask = _productRepo.GetProductSettingsAsync(orgRealPageId, cancellationToken);
        var orgSettingsTask     = _unifiedSettingsRepo.GetUnifiedSettingsAsync(orgPartyId, "Company", cancellationToken);

        await Task.WhenAll(globalSettingsTask, companySettingsTask, orgSettingsTask).ConfigureAwait(false);

        var globalSettings  = globalSettingsTask.Result;
        var companySettings = companySettingsTask.Result;
        var orgSettings     = orgSettingsTask.Result;

        var orgSetting = orgSettings.FirstOrDefault(s =>
            s.Name.Equals("PrimaryProperty", StringComparison.OrdinalIgnoreCase));

        if (orgSetting is null
            || !int.TryParse(orgSetting.Value, out int orgUsePrimaryProperties)
            || orgUsePrimaryProperties < 0)
            return false;

        var globalEntry = globalSettings.FirstOrDefault(p =>
            p.Name.Equals("UsePrimaryProperties", StringComparison.OrdinalIgnoreCase)
            && p.ProductId == productId);

        if (globalEntry is null
            || !int.TryParse(globalEntry.Value?.Trim(), out int globalValue)
            || globalValue < 0)
            return false;

        int.TryParse(
            companySettings?.FirstOrDefault(p =>
                p.Name.Equals("UsePrimaryProperties", StringComparison.OrdinalIgnoreCase)
                && p.ProductId == productId)?.Value?.Trim(),
            out int companyValue);

        return globalValue == 1 && orgUsePrimaryProperties == 1 && companyValue == 1;
    }
}
