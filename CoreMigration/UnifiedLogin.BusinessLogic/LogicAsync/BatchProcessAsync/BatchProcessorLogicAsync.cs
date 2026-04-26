using UnifiedLogin.BusinessLogic.LogicAsync.BatchProcessAsync.Factory;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.BatchProcessAsync;

/// <summary>
/// Async-first entry point for the batch processing pipeline.
/// <para>
/// Replaces <c>BatchProcessorLogic</c> whose <c>ProcessBatch</c> method called
/// <c>ProcessExecutionFactory.GetProductLogic(batchRecord.BatchProcessType)</c>
/// and executed synchronously via <c>Activator.CreateInstance</c>.
/// </para>
/// <para>
/// This class delegates directly to <see cref="ProcessExecutionFactoryAsync"/> which resolves
/// the appropriate <see cref="IProcessExecutionAsync"/> handler — all instances are
/// DI-injected singletons: no heap allocation per call, no reflection at runtime.
/// </para>
/// <para>
/// <b>DI registration:</b>
/// <code>
/// services.AddScoped&lt;BatchProcessorLogicAsync&gt;();
/// </code>
/// </para>
/// </summary>
public sealed class BatchProcessorLogicAsync
{
    private readonly ProcessExecutionFactoryAsync _factory;

    public BatchProcessorLogicAsync(ProcessExecutionFactoryAsync factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    /// <summary>
    /// Resolves the handler for <c>batchRecord.BatchProcessType</c> and executes it.
    /// Returns <see cref="string.Empty"/> on success; otherwise an error message.
    /// </summary>
    public async Task<string> ProcessBatchAsync(
        ProductUserProperitiesRoles batchRecord,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(batchRecord);

        var handler = _factory.GetProductLogic(batchRecord.BatchProcessType);
        return await handler.ExecuteProcessAsync(batchRecord, cancellationToken)
            .ConfigureAwait(false);
    }
}
