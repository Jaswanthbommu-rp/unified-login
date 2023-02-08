using Dapper;
using Newtonsoft.Json;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Batch;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Saml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository
{
	public class BatchProductBulkUpdateRepository : BaseRepository
	{
		#region Constructor
		/// <summary>
		/// Base constructor
		/// </summary>
		public BatchProductBulkUpdateRepository() : base(DbConnectionEnum.IdpConfigurationDb)
		{
		}

		/// <summary>
		/// Unit test constructor
		/// </summary>
		/// <param name="repository"></param>
		public BatchProductBulkUpdateRepository(IRepository repository) : base(repository)
		{
		}
		#endregion
		public bool SaveProductBatch(long editorUserPersonaId, long subjectUserPersonaId, Guid editorUserRealPageId,
			IList<ProductBatch> userProductList, string onesiteWithOherProductsJson, bool isOnesiteMix, int batchProcessType)
		{
			var batchGroup = CreateBatchProcessGroup();

			using (var repository = GetRepository())
			{
				foreach (var prod in userProductList)
				{
					int statusType = prod.InputJson.IsAssigned ? batchProcessType : (int)BatchProcessType.CreateUpdateProductUser;

					string inputJson = JsonConvert.SerializeObject(prod.InputJson);
					
					if (prod.ProductId == (int)ProductEnum.OneSite && isOnesiteMix)
					{
						inputJson = onesiteWithOherProductsJson;
					}
					dynamic productBatch = new
					{
						PersonRealPageId = editorUserRealPageId,
						CreateUserPersonaId = editorUserPersonaId,
						AssignUserPersonaId = subjectUserPersonaId,
						ProductId = prod.ProductId,
						StatusTypeId = 5,
						RetryCount = 0,
						InputJson = inputJson,
						CorrelationId = Guid.NewGuid().ToString(),
						BatchProcessTypeId = statusType,
						BatchProcessorGroupId = batchGroup.BatchProcessorGroupId
					};

					var repositoryResponse = repository.Execute<dynamic>(StoredProcNameConstants.SP_CreateProductBatch, productBatch);

					//In-case of an error creating a product batch record, append the ProductCode to the ErrorReason
					if (repositoryResponse.Id == 0)
					{
						throw new Exception($"Exception while inserting product with code {prod.ProductId} in the product batch.");
					}					
				}
				return true;
			}
			return false;
		}

		private BatchProcessorGroup CreateBatchProcessGroup(IRepository repo)
		{
			{
				DynamicParameters param = new DynamicParameters();
				int groupID = 0;
				param.Add("@BatchProcessorGroupID", groupID, dbType: DbType.Int32, direction: ParameterDirection.Output);

				try
				{
					var a = repo.Execute(StoredProcNameConstants.SP_CreateBatchProcessorGroup, param);
					groupID = param.Get<int>("@BatchProcessorGroupID");
				}
				catch (Exception ex)
				{
				}

				return new BatchProcessorGroup()
				{
					BatchProcessorGroupId = groupID,
					BatchProcessorGroupActivityLogged = false
				};
			}
		}

		/// <summary>
		/// Create the Batch for Admin Portal
		/// </summary>
		/// <param name="editorUserPersonaId">editorUserPersonaId</param>
		/// <param name="subjectUserPersonaId">subjectUserPersonaId</param>
		/// <param name="editorUserRealPageId">editorUserRealPageId</param>
		/// <param name="productId">productId</param>
		/// <param name="retryCheckCount">retryCheckCount</param>
		/// <param name="statusCheckSleep">statusCheckSleep</param>
		/// <returns>list SamlAttributes object</returns>
		public bool CreateBatch(long editorUserPersonaId, long subjectUserPersonaId, Guid editorUserRealPageId, int productId, int retryCheckCount, int statusCheckSleep, string defaultUserRole)
		{
			int batchProcessorGroupId;
			using (var repository = GetRepository())
			{
				//repository.UnitOfWork.BeginTransaction();
				List<string> roleList = new List<string>();
				roleList.Add(defaultUserRole);
				var batchGroup = CreateBatchProcessGroup(repository);
				batchProcessorGroupId = batchGroup.BatchProcessorGroupId;
				dynamic productBatch = new
				{
					PersonRealPageId = editorUserRealPageId,
					CreateUserPersonaId = editorUserPersonaId,
					AssignUserPersonaId = subjectUserPersonaId,
					ProductId = productId,
					StatusTypeId = 5,
					RetryCount = 0,
					BatchProcessorGroupId = batchProcessorGroupId,
					InputJson = JsonConvert.SerializeObject(new RolePropertyList()
					{
						PropertyList = new List<string> { "-1" },
						RoleList = roleList,
						IsAssigned = true
					}),
				};

				var repositoryResponse = repository.Execute<dynamic>(StoredProcNameConstants.SP_CreateProductBatch, productBatch);

				//In-case of an error creating a product batch record, append the ProductCode to the ErrorReason
				if (repositoryResponse.Id == 0)
				{
					throw new Exception($"Exception while inserting product {productId} in the product batch.");
				}
			}
			 
			while (retryCheckCount >= 0)
			{
				System.Threading.Thread.Sleep(statusCheckSleep);
				List<UserBatchProductDetail> lst = (List<UserBatchProductDetail>)GetUserBatchDetails(batchProcessorGroupId, editorUserPersonaId, subjectUserPersonaId);
				if (lst.Select(a => a.StatusTypeId == 8).Any())
				{
					return true;
				}
				else if (lst.Select(a => a.StatusTypeId == 7).Any())
				{
					return false; 
				}
				else
				{
					retryCheckCount--;
				}
			}
			return false;
		}

		public IList<UserBatchProductDetail> GetUserBatchDetails(int batchGroupId, long editorUserPersonId, long subjectUserPersonId)
		{
			using (var repo = GetRepository())
			{
				var data = repo.GetMany<UserBatchProductDetail>(StoredProcNameConstants.SP_GetUserBatchRecords, new
				{
					batchProcessorGroupId = batchGroupId,
					editorUserPersonId = editorUserPersonId,
					subjectUserPersonId = subjectUserPersonId

				}).ToList();
				return data;
			}
		}
		/// <summary>
		/// Update a Enterprise Role Product Batch
		/// </summary>
		/// <returns>Repository response object</returns>
		public bool UpdateEnterpriseRoleProductBatch(long productBatchId, int statusTypeId)
		{
			using (var repository = GetRepository())
			{
				var result = repository.Execute<bool>(StoredProcNameConstants.SP_UpdateEnterpriseRoleProductBatch,
				   new { productBatchId, statusTypeId });

				return result;
			}
		}

		/// <summary>
		/// Update a Primary Property Product Batch
		/// </summary>
		/// <returns>Repository response object</returns>
		public bool UpdatePrimaryPropertyProductBatch(long productBatchId, int statusTypeId)
		{
			using (var repository = GetRepository())
			{
				var result = repository.Execute<bool>(StoredProcNameConstants.SP_UpdatePrimaryPropertyProductBatch,
				   new { productBatchId, statusTypeId });

				return result;
			}
		}

		public void UpdateUnifiedPlatFormRole(int roleId, long editorUserId, long userPersonaId)
		{
			using (var repository = GetRepository())
			{
				dynamic param = new
				{
					personaID = userPersonaId,
					roleID = roleId,
					CreatedBy = editorUserId,
					personaPrivilgeID = 0
				};
				
				var repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkPersonaToRole, param);
			}
		}

		private BatchProcessorGroup CreateBatchProcessGroup()
		{
			{
				DynamicParameters param = new DynamicParameters();
				int groupID = 0;
				param.Add("@BatchProcessorGroupID", groupID, dbType: DbType.Int32, direction: ParameterDirection.Output);

				try
				{
					using (var repo = GetRepository())
					{
						var a = repo.Execute(StoredProcNameConstants.SP_CreateBatchProcessorGroup, param);
						groupID = param.Get<int>("@BatchProcessorGroupID");
					}

				}
				catch (Exception ex)
				{
				}

				return new BatchProcessorGroup()
				{
					BatchProcessorGroupId = groupID,
					BatchProcessorGroupActivityLogged = false
				};
			}
		}
	}
}
