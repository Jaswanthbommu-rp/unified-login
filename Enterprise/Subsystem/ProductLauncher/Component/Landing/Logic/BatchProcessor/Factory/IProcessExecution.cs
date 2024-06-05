using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using ZiggyCreatures.Caching.Fusion;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.BatchProcessor.Factory
{
    /// <summary>
    /// Interface for process execution
    /// </summary>
    public interface IProcessExecution
    {
        /// <summary>
        /// Execute batch process
        /// </summary>
        string ExecuteProcess(ProductUserProperitiesRoles batchRecord, FusionCache cache);
    }
}