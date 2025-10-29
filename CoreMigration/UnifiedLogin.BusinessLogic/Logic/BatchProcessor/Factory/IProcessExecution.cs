using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Logic.BatchProcessor.Factory
{
    /// <summary>
    /// Interface for process execution
    /// </summary>
    public interface IProcessExecution
    {
        /// <summary>
        /// Execute batch process
        /// </summary>
        string ExecuteProcess(ProductUserProperitiesRoles batchRecord);
    }
}