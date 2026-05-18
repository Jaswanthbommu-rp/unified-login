using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityBatchProcessor.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityBatchProcessor.Model;

namespace RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityBatchProcessor.Repository
{
	public class BatchRepository : BaseRepository, IBatchRepository
	{
		#region Constructor

		/// <summary>
		/// base Constructor
		/// </summary>
		public BatchRepository() : base(DbConnectionEnum.IdpConfigurationDb)
		{
		}

		#endregion

		#region Public Methods

		public IList<Batch> GetBatchToProcess(int batchSize, bool shouldIncludeErrorRecords, int retrycount = 3)
		{
			using (var repository = GetRepository())
			{
				var result = repository.GetMany<Batch>(StoredProcNameConstants.SP_ListBatch,
					new { IncludeErrorRecord = shouldIncludeErrorRecords, batchSize = batchSize, retrycount = retrycount }).ToList();

				return result;
			}
		}
        public IList<CompanyPropertyBatch> GetCompanyBatchByStatus(int batchSize, BatchStatusType statusType)
		{
			using (var repository = GetRepository())
			{
				int statusTypeId = (int)statusType;
				var result = repository.GetMany<CompanyPropertyBatch>(StoredProcNameConstants.SP_ListCompanyBatchData,
					new { batchSize = batchSize, statusTypeId = statusTypeId }).ToList();
				return result;
			}
        }

        public IList<EnterpriseRoleBatch> GetEnterpriseRoleProductUpdateBatchToProcess(int batchSize)
		{
			using (var repository = GetRepository())
			{
				var result = repository.GetMany<EnterpriseRoleBatch>(StoredProcNameConstants.SP_EnterpriseRoleListBatch,
					new { batchSize = batchSize }).ToList();

				return result;
			}
		}

		public IList<PrimaryPropertyBatch> GetPrimaryPropertyProductUpdateBatchToProcess(int batchSize)
		{
			using (var repository = GetRepository())
			{
				var result = repository.GetMany<PrimaryPropertyBatch>(StoredProcNameConstants.SP_PrimaryPropertiesBatch,
					new { batchSize = batchSize }).ToList();

				return result;
			}
		}

        public IList<BulkUserBatch> GetBulkUsersUpdateBatchToProcess(int batchSize)
        {
            using (var repository = GetRepository())
            {
                var result = repository.GetMany<BulkUserBatch>(StoredProcNameConstants.SP_BulkUserBatch,
                    new { batchSize = batchSize }).ToList();

                return result;
            }
        }

        public int UpdateBatchRecord(int productBatchId, BatchStatusType batchStatusType, string inputJson = null, string errorDetails = null)
		{
			using (var repository = GetRepository())
			{
				int statusTypeId = (int)batchStatusType;
				var result = repository.Execute<int>(StoredProcNameConstants.SP_UpdateBatch,
					new { productBatchId, statusTypeId, inputJson, errorDetails });

				return result;
			}
		}

		/// <summary>
		/// Update a Enterprise Role Product Batch
		/// </summary>
		/// <returns>Repository response object</returns>
		public void UpdateEnterpriseRoleProductBatch(long productBatchId, int statusTypeId)
		{
			using (var repository = GetRepository())
			{
				var result = repository.Execute<bool>(StoredProcNameConstants.SP_UpdateEnterpriseRoleProductBatch,
				   new { productBatchId, statusTypeId });
			}
		}

		public void UpdateBatch(IList<Batch> batch, BatchStatusType batchStatusType, string inputJson = null, string errorDetails = null)
		{
			foreach (var record in batch)
			{
				UpdateBatchRecord(record.BatchProcessorId, batchStatusType, inputJson, errorDetails);
			}
		}

		public void UpdatePrimaryPropertyProductBatch(long productBatchId, int statusTypeId)
		{
			using (var repository = GetRepository())
			{
				var result = repository.Execute<bool>(StoredProcNameConstants.SP_UpdatePrimaryPropertyProductBatch,
				   new { productBatchId, statusTypeId });
			}
		}

        public void UpdateBulkUserBatch(long productBatchId, int statusTypeId)
        {
            using (var repository = GetRepository())
            {
                var result = repository.Execute<bool>(StoredProcNameConstants.SP_UpdateBulkUserBatch,
                   new { productBatchId, statusTypeId });
            }
        }

        public void UpdateCompanyPropertyBatches(IList<CompanyPropertyBatch> batch, BatchStatusType batchStatusType)
        {
            foreach (var record in batch)
            {
                UpdateCompanyPropertyBatch(record.CompanyBatchJobId, (int)batchStatusType);
            }
        }
        public void UpdateCompanyPropertyBatch(long companyBatchJobId, int statusTypeId)
        {
            using (var repository = GetRepository())
            {
                var result = repository.Execute<bool>(StoredProcNameConstants.SP_UpdateCompanyPropertyBatch,
                   new { companyBatchJobId, statusTypeId });
            }
        }

        public IList<BulkResetPasswordBatch> GetPendingBulkResetPassword(int batchSize)
        {
            using (var repository = GetRepository())
            {
                var result = repository.GetMany<BulkResetPasswordBatch>(
                    StoredProcNameConstants.SP_ListPendingBulkResetPassword,
                    new { batchSize }).ToList();
                return result;
            }
        }

        public void UpdateBulkResetPasswordStatus(long id, int status)
        {
            using (var repository = GetRepository())
            {
                var result = repository.Execute<bool>(
                    StoredProcNameConstants.SP_UpdateBulkResetPasswordStatus,
                    new { id, status });
            }
        }

        public IList<BatchConfiguration> GetBatchConfigurations()
		{
			// cache the configurations
			ObjectCache tokenCache = MemoryCache.Default;

			// Get   values from cache 
			var batchConfigs = tokenCache["batch_configs"] as List<BatchConfiguration>;

			if (batchConfigs == null)
			{
				using (var repository = GetRepository())
				{
					batchConfigs = repository
						.GetMany<BatchConfiguration>(StoredProcNameConstants.SP_ListBatchConfiguration, null)
						.ToList();
				}

				var cachePolicy = new CacheItemPolicy
				{
					// Expire cache every after 60 minutes  
					AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(60)
				};

				tokenCache.Set("batch_configs", batchConfigs, cachePolicy);
			}

			return batchConfigs;
		}

		public IList<ProductInternalSetting> GetProductInternalSettings(int productId)
		{
			// cache the configurations
			ObjectCache tokenCache = MemoryCache.Default;

			// Get   values from cache 
			var productsettings = tokenCache[$"productsettings_{productId}"] as List<ProductInternalSetting>;

			if (productsettings == null)
			{
				using (var repository = GetRepository())
				{
					dynamic param = new { ProductId = productId };
					productsettings = repository.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, param);
				}

				var cachePolicy = new CacheItemPolicy
				{
					// Expire cache every 15 minutes  
					AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(15)
				};

				tokenCache.Set($"productsettings_{productId}", productsettings, cachePolicy);
			}

			return productsettings;
		}

		#endregion
	}
}

