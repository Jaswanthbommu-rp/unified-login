using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.BatchProcessor.Factory;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using ZiggyCreatures.Caching.Fusion;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.BatchProcessor
{
    /// <summary>
    /// Batch Processor Logic
    /// </summary>
    public class BatchProcessorLogic
    {
        public BatchProcessorLogic(FusionCache cache)
        {
            
        }
        /// <summary>
        /// Process Batch based on process type
        /// </summary>
        public string ProcessBatch(ProductUserProperitiesRoles batchRecord)
        {
            var processToExecute = ProcessExecutionFactory.GetProductLogic(batchRecord.BatchProcessType);
            return processToExecute.ExecuteProcess(batchRecord);
        }
    }
}
 