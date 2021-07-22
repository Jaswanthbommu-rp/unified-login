using Dapper;
using Newtonsoft.Json;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Batch;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository
{
	public class EnterpriseRoleProductRepository : BaseRepository
	{
		#region Constructor
		/// <summary>
		/// Base constructor
		/// </summary>
		public EnterpriseRoleProductRepository() : base(DbConnectionEnum.IdpConfigurationDb)
		{
		}

		/// <summary>
		/// Unit test constructor
		/// </summary>
		/// <param name="repository"></param>
		public EnterpriseRoleProductRepository(IRepository repository) : base(repository)
		{
		}
		#endregion
		public bool SaveProductBatch(long editorUserPersonaId, long subjectUserPersonaId, Guid editorUserRealPageId,
			IList<ProductBatch> userProductList, string onesiteWithOherProductsJson, bool isOnesiteMix)
		{
			var batchGroup = CreateBatchProcessGroup();

			using (var repository = GetRepository())
			{
				foreach (var prod in userProductList)
				{
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
						BatchProcessTypeId = BatchProcessType.CreateUpdateProductUser,
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
