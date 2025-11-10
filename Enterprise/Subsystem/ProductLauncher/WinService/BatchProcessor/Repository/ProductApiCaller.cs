using System.Threading.Tasks;
using RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityBatchProcessor.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityBatchProcessor.Model;

namespace RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityBatchProcessor.Repository
{
    public class ProductApiCaller : IProductApiCaller
    {
        public async Task<string> ProcessBatchRecord(BatchProcessorInput batchProcessorInput)
        {
            var result = await ApiCaller.PostApi<string, BatchProcessorInput>(batchProcessorInput, batchProcessorInput.ProcessApiEndPoint);
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

        public async Task<string> ProcessBulkUserBatchRecord(BulkUserBatch batchProcessorInput, string processApiEndPoint)
        {
            var result = await ApiCaller.PostApi<string, BulkUserBatch>(batchProcessorInput, processApiEndPoint);
            return result;
        }
        public async Task<string> ProcessCompanyBatchRecord(CompanyPropertyBatch batchProcessorInput, string processApiEndPoint)
        {
            var result = await ApiCaller.PostApi<string, CompanyPropertyBatch>(batchProcessorInput, processApiEndPoint);
            return result;
        }
    }
}
