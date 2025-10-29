using UnifiedLogin.BusinessLogic.Logic.BatchProcessor.Factory;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Logic.BatchProcessor
{
    /// <summary>
    /// Batch Processor Logic
    /// </summary>
    public class BatchProcessorLogic
    {
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
 