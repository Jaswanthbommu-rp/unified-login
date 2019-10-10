using System.Threading.Tasks;
using RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityBatchProcessor.Model;

namespace RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityBatchProcessor.Repository
{
    public interface IProductApiCaller
    {
        Task<string> ProcessBatchRecord(BatchProcessorInput batchProcessorInput);
    }
}