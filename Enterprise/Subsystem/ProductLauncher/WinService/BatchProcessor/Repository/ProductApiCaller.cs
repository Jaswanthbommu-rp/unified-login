using System.Threading.Tasks;
using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityBatchProcessor.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityBatchProcessor.Model;

namespace RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityBatchProcessor.Repository
{
    public class ProductApiCaller : IProductApiCaller
    {
        public async Task<string> ProcessBatchRecord(BatchProcessorInput batchProcessorInput)
        {
            var res = JsonConvert.SerializeObject(batchProcessorInput);
            var result = await ApiCaller.PostApi<string, BatchProcessorInput>(batchProcessorInput, "http://localhost/api/batchprocessor");
            return result;
        }
        public async Task<string> ProcessEnterpriseRoleBatchRecord(EnterpriseRoleBatch batchProcessorInput, string processApiEndPoint)
        {
            var result = await ApiCaller.PostApi<string, EnterpriseRoleBatch>(batchProcessorInput, processApiEndPoint);
            return result;
        }

        public async Task<string> ProcessPrimaryPropertyBatchRecord(PrimaryPropertyBatch batchProcessorInput, string processApiEndPoint)
        {
            var result = await ApiCaller.PostApi<string, PrimaryPropertyBatch>(batchProcessorInput, processApiEndPoint);
            return result;
        }
    }
}
