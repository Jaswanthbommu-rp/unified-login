using System.Threading.Tasks;
using RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityBatchProcessor.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityBatchProcessor.Model;

namespace RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityBatchProcessor.Repository
{
    public class ProductApiCaller : IProductApiCaller
    {
        public async Task<string> ProcessBatchRecord(BatchProcessorInput batchProcessorInput)
        {
            var result = await ApiCaller.PostApi<string, BatchProcessorInput>(batchProcessorInput, "https://www-local2.realpage.com/api/batchprocessor");
            return result;
        }
    }
}
