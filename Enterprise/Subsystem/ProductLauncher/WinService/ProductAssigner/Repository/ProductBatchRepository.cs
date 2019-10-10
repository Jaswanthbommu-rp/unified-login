using System.Collections.Generic;
using System.Linq;
using RP.Enterprise.Subsystem.ProductLauncher.WinService.ProductAssigner.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.WinService.ProductAssigner.Model;

namespace RP.Enterprise.Subsystem.ProductLauncher.WinService.ProductAssigner.Repository
{
    public class ProductBatchRepository : BaseRepository, IProductBatchRepository
    {
        #region Constructor

        /// <summary>
        /// base Constructor
        /// </summary>
        public ProductBatchRepository() : base(DbConnectionEnum.IdpConfigurationDb)
        {
        }

        #endregion

        #region Public Methods

        public IList<ProductBatch> GetProductBatchToProcess(int batchSize, bool shouldIncludeErrorRecords, int retrycount = 3)
        {
            using (var repository = GetRepository())
            {
                var result = repository.GetMany<ProductBatch>(StoredProcNameConstants.SP_ListProductBatch,
                    new { IncludeErrorRecord = shouldIncludeErrorRecords, batchSize = batchSize, retrycount = retrycount }).ToList();
                return result;
            }
        }

        #endregion

        public int UpdateBatchRecord(int productBatchId, BatchStatusType batchStatusType, string inputJson = null, string errorDetails = null)
        {
            using (var repository = GetRepository())
            {
                int statusTypeId = (int)batchStatusType;
                var result = repository.Execute<int>(StoredProcNameConstants.SP_UpdateProductBatch,
                    new { productBatchId, statusTypeId, inputJson, errorDetails });
                return result;
            }
        }

        public void UpdateBatch(IList<ProductBatch> batch, BatchStatusType batchStatusType, string inputJson = null, string errorDetails = null)
        {
            foreach (var record in batch)
            {
                UpdateBatchRecord(record.ProductBatchId, batchStatusType, inputJson, errorDetails);
            }
        }
    }
}

