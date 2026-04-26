using UnifiedLogin.SharedObjects.Batch;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

public interface IManagePrimaryPropertiesBatchAsync
{
    /// <summary>
    /// Runs enterprise-role and primary-property batch processing for the given
    /// subject user, then records Success or Error against the batch queue row.
    /// Returns an empty string on success or an error message on failure.
    /// </summary>
    Task<string> GeneratePrimaryPropertiesUserProductBatchAsync(
        PrimaryPropertyBatch batch,
        CancellationToken cancellationToken = default);
}
